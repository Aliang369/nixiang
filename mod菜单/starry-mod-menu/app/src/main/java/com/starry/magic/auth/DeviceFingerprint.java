package com.starry.magic.auth;

import android.content.Context;
import android.os.Build;
import android.provider.Settings;
import android.util.Log;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Locale;
import java.util.Set;

/**
 * 跨宿主包稳定的设备指纹（全游戏卡密绑定用）。
 *
 * <p>v5 主 ID 只使用 {@link Settings.Secure#ANDROID_ID}（SSAID）。Android 8+ 下，
 * 同一签名证书、同一系统用户的多个应用会得到相同 SSAID，因此无需再依赖外部存储
 * 种子或容易碰撞的 Build 硬件哈希。
 *
 * <p>v4 是 SSAID 缺失/异常时的降级信号；两个版本每次都采集，命中任一稳定 ID 即可用。
 * 调用方还应把候选交给服务端做绑定迁移，避免在 v5/v4 之间反复漂移。
 */
public final class DeviceFingerprint {

    private static final String TAG = "DeviceFingerprint";
    public static final String VERSION = "v5";
    public static final String VERSION_LEGACY_V4 = "v4";
    /** v5/v4 均不可用时的本地兜底号版本标记（Preferences 写入，见 getOrCreateDeviceId） */
    public static final String VERSION_LOCAL = "local";
    /** 共享种子文件名（与 ensure_starry_device_id_seed.sh / 旧客户端一致） */
    private static final String SEED_FILE = "device_id";

    /**
     * su 兜底单次硬超时（毫秒）。KernelSU/Magisk 在「未授权」或授权弹窗未决时，
     * su 可能长时间不返回；所有 su 调用都必须有上限，且不得在持有其它锁时执行。
     */
    private static final long SU_TIMEOUT_MS = 1500L;

    /** 本次进程内 su 已被证明不可用（超时/执行失败）⇒ 后续不再尝试，见 {@link #runSu} */
    private static volatile boolean suBroken = false;

    /**
     * 当前是否有一次 su 调用尚未返回（毫秒时间戳，0=无）。
     * fork 卡在 kernel D 态时线程永不返回、连 destroyForcibly 都没用 ⇒ 用它把「卡住的 su」
     * 限制成最多一个，并把「超过 4s 还没回来」判定为 su 不可用（熔断）。
     */
    private static volatile long suInFlightSince = 0L;

    /** su 单次调用超过这个时长仍未返回 ⇒ 判不可用并熔断 */
    private static final long SU_HANG_SUSPECT_MS = 4000L;


    private static final String[] SHARED_REL_DIRS = {
            "Android/starry_magic",
            ".starry_magic",
            "Download/.starry_magic",
    };

    private static final String[] ABS_SHARED_DIRS = {
            "/data/local/tmp/starry_magic",
    };

    private static final String[] BROKEN_SSAIDS = {
            "9774d56d682e549c",
            "0000000000000000",
    };

    private DeviceFingerprint() {}


    /** @return v5 SSAID 指纹；系统未提供合法 SSAID 时返回 null，由调用方降级。 */
    public static String get(Context context) {
        if (context == null) return null;
        try {
            String ssaid = Settings.Secure.getString(
                    context.getContentResolver(), Settings.Secure.ANDROID_ID);
            if (!isUsableSsaid(ssaid)) {
                Log.e(TAG, "SSAID unavailable or invalid; refuse silent fallback");
                return null;
            }
            String raw = "ver=" + VERSION + "|ssaid=" + ssaid.trim().toLowerCase(Locale.ROOT);
            String hash = sha256Hex(raw);
            if (hash == null || hash.length() < 32) {
                Log.e(TAG, "SSAID hash failed");
                return null;
            }
            String id = "dev_" + hash.substring(0, 32);
            Log.i(TAG, "fingerprint " + VERSION + " id=" + id + " source=ssaid");
            return id;
        } catch (Throwable t) {
            Log.e(TAG, "read SSAID failed", t);
            return null;
        }
    }

    /** 当前设备的 v4 稳定 Build 指纹；与旧客户端算法逐字兼容。 */
    public static String getV4() {
        return computeLegacyV4HardwareId();
    }

    /**
     * 采集本机历史 ID：共享种子、其它星光包 prefs，以及 v4 稳定 Build 哈希。
     */
    public static List<String> getLegacyIds(Context context) {
        Set<String> out = new LinkedHashSet<>();
        for (String path : legacySeedPaths()) {
            String id = readFileTrimmed(new File(path));
            if (!isValidDevId(id)) {
                id = readFileViaSu(path);
            }
            if (isValidDevId(id)) {
                out.add(id);
            }
        }
        for (String id : readIdsFromOtherPrefsViaSu()) {
            if (isValidDevId(id)) {
                out.add(id);
            }
        }
        String v4Hardware = computeLegacyV4HardwareId();
        if (isValidDevId(v4Hardware)) {
            out.add(v4Hardware);
        }
        return new ArrayList<>(out);
    }

    public static boolean isValidDevId(String id) {
        return id != null && id.startsWith("dev_") && id.length() >= 8 && id.length() <= 96;
    }

    /**
     * 把已确定的设备号写一份到共享种子文件（外部存储 + {@code /data/local/tmp}），
     * 供本包 prefs 被清/被重建后重新找回同一身份。全程 best-effort：
     * 云机常常既无 su 也无可写外部存储，失败只记日志，绝不影响主流程。
     */
    public static void writeSeed(Context context, String id) {
        if (!isValidDevId(id)) return;
        int written = 0;
        for (String path : legacySeedPaths()) {
            long t0 = System.currentTimeMillis();
            boolean ok = writeSeedFile(path, id);
            // 诊断留痕（2026-10-30）：这条链是「登录点了没反应」的嫌疑卡点，逐条记录耗时与结果
            Log.i(TAG, "seed 写入 " + path + " → " + (ok ? "ok" : "fail")
                    + " (" + (System.currentTimeMillis() - t0) + "ms)");
            if (ok) written++;
        }
        Log.i(TAG, "seed write id=" + id + " files=" + written);
    }

    private static boolean writeSeedFile(String path, String id) {
        File f = new File(path);
        try {
            File dir = f.getParentFile();
            if (dir != null && !dir.isDirectory() && !dir.mkdirs()) return false;
            try (java.io.FileOutputStream out = new java.io.FileOutputStream(f, false)) {
                out.write(id.getBytes(StandardCharsets.UTF_8));
            }
            return true;
        } catch (Throwable t) {
            String pkg = path == null ? "" : path;
            // 直接写入失败时退到 su（仅作可选加速，云机无 su 就自然放弃）
            if (!pkg.contains("starry_magic") || pkg.contains("'") || pkg.contains("\n")) {
                return false;
            }
            return runSu("mkdir -p '" + new File(pkg).getParent() + "' && echo -n '"
                    + id + "' > '" + pkg + "'", SU_TIMEOUT_MS, null) == 0;
        }
    }

    private static boolean isUsableSsaid(String raw) {
        String value = raw == null ? "" : raw.trim().toLowerCase(Locale.ROOT);
        if (value.length() != 16) return false;
        for (int i = 0; i < value.length(); i++) {
            char c = value.charAt(i);
            boolean hex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f');
            if (!hex) return false;
        }
        for (String broken : BROKEN_SSAIDS) {
            if (broken.equals(value)) return false;
        }
        return true;
    }

    /** 仅稳定 Build 字段，保持与 v4 hardware 算法逐字兼容，供旧绑定识别。 */
    private static String computeLegacyV4HardwareId() {
        String raw = "ver=v4"
                + "|board=" + nullToEmpty(Build.BOARD)
                + "|hw=" + nullToEmpty(Build.HARDWARE)
                + "|manu=" + nullToEmpty(Build.MANUFACTURER)
                + "|brand=" + nullToEmpty(Build.BRAND)
                + "|model=" + nullToEmpty(Build.MODEL)
                + "|device=" + nullToEmpty(Build.DEVICE)
                + "|product=" + nullToEmpty(Build.PRODUCT);
        String hash = sha256Hex(raw);
        if (hash == null || hash.isEmpty()) return null;
        return "dev_" + hash.substring(0, Math.min(32, hash.length()));
    }

    private static List<String> legacySeedPaths() {
        List<String> paths = new ArrayList<>();
        File external = android.os.Environment.getExternalStorageDirectory();
        if (external != null) {
            for (String rel : SHARED_REL_DIRS) {
                paths.add(new File(new File(external, rel), "device_id").getAbsolutePath());
            }
        }
        for (String abs : ABS_SHARED_DIRS) {
            paths.add(new File(abs, "device_id").getAbsolutePath());
        }
        return paths;
    }

    private static List<String> readIdsFromOtherPrefsViaSu() {
        List<String> ids = new ArrayList<>();
        List<String> lines = new ArrayList<>();
        int code = runSu("grep -h auth_device_id "
                + "/data/data/*/shared_prefs/starry_magic_prefs.xml "
                + "/data/user/0/*/shared_prefs/starry_magic_prefs.xml "
                + "2>/dev/null", SU_TIMEOUT_MS, lines);
        if (code != 0) return ids;
        for (String line : lines) {
            String id = extractDevIdFromPrefsLine(line);
            if (isValidDevId(id) && !ids.contains(id)) {
                ids.add(id);
            }
        }
        return ids;
    }

    /**
     * 带硬超时的 su 执行（唯一 su 入口）。
     *
     * <p>★ 2026-10-28 修复（绿梦真机「点悬浮球即卡死闪退」）：旧实现三处各写一遍
     * {@code Runtime.exec + waitFor()}，<b>没有任何超时</b>。在装了 KernelSU 但未给本包
     * 授权的机型上，{@code su} 会长时间不返回，而 {@code Preferences.getOrCreateDeviceId()}
     * 当时是 {@code static synchronized} ⇒ <b>类锁被钉死</b> ⇒ 主线程
     * {@code showMenuNative → Preferences.with()} 永久 BLOCKED ⇒ 输入超时 ANR ⇒ 进程被杀。
     *
     * <p>现在：① 读 stdout 与等待都用独立线程，超时到点 {@code destroyForcibly()}；
     * ② 一旦本次进程内出现过超时/执行失败，<b>熔断</b>后续所有 su 尝试（{@code suBroken}），
     * 避免每条路径都白等一轮；③ 只返回退出码与可选输出行，调用点不再自己 exec。
     *
     * @return 进程退出码；超时/不可用返回 -1（并置熔断）
     */
    private static int runSu(String shellCmd, long timeoutMs, List<String> sink) {
        long now = System.currentTimeMillis();
        long since = suInFlightSince;
        if (since != 0L && now - since > SU_HANG_SUSPECT_MS) {
            suBroken = true;   // 上一笔 su 至今没回来 ⇒ 卡死，熔断
            Log.w(TAG, "上一次 su 调用 " + (now - since) + "ms 未返回 ⇒ 判定 su 不可用，本次进程内不再尝试");
        }
        if (suBroken) return -1;
        suInFlightSince = now;
        Log.i(TAG, "su 尝试(" + timeoutMs + "ms 上限): "
                + (shellCmd.length() > 48 ? shellCmd.substring(0, 48) + "…" : shellCmd));
        Process p;
        try {
            p = Runtime.getRuntime().exec(new String[]{"su", "-c", shellCmd});
        } catch (Throwable t) {
            suBroken = true;
            return -1;
        }
        final Process fp = p;
        final List<String> out = sink != null ? sink : new ArrayList<String>();
        Thread reader = new Thread(new Runnable() {
            @Override
            public void run() {
                try (BufferedReader br = new BufferedReader(
                        new InputStreamReader(fp.getInputStream(), StandardCharsets.UTF_8))) {
                    String line;
                    int n = 0;
                    while ((line = br.readLine()) != null && n < 200) {
                        out.add(line);
                        n++;
                    }
                } catch (Throwable ignored) {
                }
                try { fp.getErrorStream().close(); } catch (Throwable ignored) { }
            }
        }, "starry-su-out");
        reader.setDaemon(true);
        reader.start();
        // ★ 2026-10-30 修复（绿梦真机「菜单点登录只弹『登录中…』、一个请求都不发」）：
        //   原来这里直接 p.waitFor() 无超时，靠旁路线程到点 destroyForcibly() 兜底。
        //   但 destroyForcibly 对卡在不可中断状态（D 态 / 被 seccomp 笼子停住的子进程）根本不生效
        //   ⇒ waitFor() 永久阻塞 ⇒ 卡住的是**调用线程本身**。绿梦实测这条链挂在 fetchBootstrapOnce
        //   （authExec 单线程池的首个任务）：getOrCreateDeviceId → writeSeed → runSu → waitFor()
        //   永不返回 ⇒ 后续所有授权任务（含点「登录」）在队列里排到死，日志上只有「登录中…」toast。
        //   现在：waitFor 挪进旁路线程，主调方 join(timeoutMs) 到点即返回 -1 并熔断，调用方不再被拖死。
        final int[] rc = {-1};
        Thread waiter = new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    rc[0] = fp.waitFor();
                } catch (Throwable ignored) { }
            }
        }, "starry-su-wait");
        waiter.setDaemon(true);
        waiter.start();
        try {
            waiter.join(timeoutMs);
        } catch (InterruptedException ignored) { }
        if (waiter.isAlive()) {
            try { fp.destroyForcibly(); } catch (Throwable ignored) { }
            suBroken = true;
            Log.w(TAG, "su 调用 " + timeoutMs + "ms 未返回 ⇒ 本次进程内熔断 su 兜底（调用方不再等待）");
            return -1;
        }
        int code = rc[0];
        try { reader.join(300L); } catch (InterruptedException ignored) { }
        if (code == 0) suInFlightSince = 0L;
        if (code != 0) {
            // 137=SIGKILL(destroyForcibly)，-1=异常 ⇒ 判定 su 链路不可用并熔断
            if (code == -1 || code == 137) {
                suBroken = true;
                Log.w(TAG, "su 调用超时/不可用（" + timeoutMs + "ms），本次进程内熔断后续 su 兜底");
            }
            return code;
        }
        return 0;
    }

    private static String extractDevIdFromPrefsLine(String line) {
        if (line == null) return null;
        int start = line.indexOf(">dev_");
        if (start < 0) return null;
        start += 1;
        int end = line.indexOf('<', start);
        if (end < 0) end = line.length();
        return line.substring(start, end).trim();
    }

    private static String readFileViaSu(String path) {
        if (path == null || path.isEmpty()) return null;
        if (!path.contains("starry_magic") || path.contains("'") || path.contains("\n")) {
            return null;
        }
        List<String> lines = new ArrayList<>();
        if (runSu("cat '" + path + "' 2>/dev/null", SU_TIMEOUT_MS, lines) != 0) return null;
        for (String raw : lines) {
            String line = raw == null ? "" : raw.trim();
            if (isValidDevId(line)) return line;
        }
        return null;
    }

    private static String readFileTrimmed(File f) {
        if (f == null || !f.isFile() || f.length() == 0 || f.length() > 128) return null;
        try (FileInputStream in = new FileInputStream(f)) {
            byte[] buf = new byte[(int) f.length()];
            int n = in.read(buf);
            if (n <= 0) return null;
            return new String(buf, 0, n, StandardCharsets.UTF_8).trim();
        } catch (Throwable ignored) {
            return null;
        }
    }

    private static String sha256Hex(String input) {
        try {
            MessageDigest md = MessageDigest.getInstance("SHA-256");
            byte[] dig = md.digest(input.getBytes(StandardCharsets.UTF_8));
            return toHex(dig);
        } catch (Exception e) {
            return null;
        }
    }

    private static String toHex(byte[] bytes) {
        StringBuilder sb = new StringBuilder(bytes.length * 2);
        for (byte b : bytes) {
            sb.append(String.format("%02x", b));
        }
        return sb.toString();
    }

    private static String nullToEmpty(String s) {
        return s == null ? "" : s;
    }
}
