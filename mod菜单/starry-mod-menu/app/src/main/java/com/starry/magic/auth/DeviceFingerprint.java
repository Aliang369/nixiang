package com.starry.magic.auth;

import android.content.Context;
import android.os.Build;
import android.os.Environment;
import android.system.Os;
import android.util.Log;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStreamReader;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.UUID;

/**
 * 跨宿主包稳定的设备指纹（全游戏卡密绑定用）。
 *
 * <p>权威策略（防「修五次仍复发」）：
 * <ol>
 *   <li>先<strong>收集</strong>所有来源的合法 id（tmp / sdcard / su 读盘 / 其它包 prefs）。</li>
 *   <li>冲突时优先 sdcard，再 tmp，再 prefs；<strong>禁止</strong>用新算出的 hardware 覆盖已有种子。</li>
 *   <li>选中后写回各副本（只补齐空洞，不 clobber 不同值）。</li>
 *   <li>全无种子才用稳定硬件哈希；写盘失败不致命。</li>
 * </ol>
 *
 * <p>历史坑：v1 ANDROID_ID / v2 Widevine 按包隔离；v3 含 OTA 可变 FINGERPRINT；
 * v4 早期只信 sdcard → 无存储权限宿主落到另一套 hardware，甚至可能把错误 id 写进 tmp 毒化全机。
 */
public final class DeviceFingerprint {

    private static final String TAG = "DeviceFingerprint";
    private static final String FINGERPRINT_VER = "v4";
    private static final String FILE_NAME = "device_id";

    private static final String[] SHARED_REL_DIRS = {
            "Android/starry_magic",
            ".starry_magic",
            "Download/.starry_magic",
    };

    private static final String[] ABS_SHARED_DIRS = {
            "/data/local/tmp/starry_magic",
    };

    private DeviceFingerprint() {}

    public static String get(Context context) {
        if (context == null) return "unknown";

        Map<String, String> found = collectAllIds();
        String chosen = pickCanonical(found);
        if (isValidDevId(chosen)) {
            propagateFillGaps(chosen);
            Log.i(TAG, "fingerprint " + FINGERPRINT_VER + " id=" + chosen
                    + " source=shared sources=" + found);
            return chosen;
        }

        String computed = computeHardwareId();
        if (!isValidDevId(computed)) {
            computed = "dev_" + sha256Hex("fallback|" + UUID.randomUUID()).substring(0, 32);
            Log.w(TAG, "hardware hash failed, using random fallback (will break cross-app)");
        }
        // 再次确认：写之前若 su/盘上已出现种子，绝不覆盖
        found = collectAllIds();
        chosen = pickCanonical(found);
        if (isValidDevId(chosen)) {
            propagateFillGaps(chosen);
            Log.i(TAG, "fingerprint " + FINGERPRINT_VER + " id=" + chosen
                    + " source=shared-late sources=" + found);
            return chosen;
        }
        propagateFillGaps(computed);
        Log.i(TAG, "fingerprint " + FINGERPRINT_VER + " id=" + computed
                + " source=hardware shared=none");
        return computed;
    }

    /** 仅稳定 Build 字段；不含 DISPLAY / FINGERPRINT（会随 OTA 变）。 */
    private static String computeHardwareId() {
        String raw = "ver=" + FINGERPRINT_VER
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

    /** path → id；按发现顺序，后者不覆盖前者同 path。 */
    private static Map<String, String> collectAllIds() {
        Map<String, String> out = new LinkedHashMap<>();
        for (String path : seedPathsPriority()) {
            String id = readFileTrimmed(new File(path));
            if (!isValidDevId(id)) {
                id = readFileViaSu(path);
            }
            if (isValidDevId(id)) {
                out.put(path, id);
            }
        }
        // 其它已装星光包 prefs（Root）：种子文件全丢时仍能对齐已绑卡
        for (String id : readIdsFromOtherPrefsViaSu()) {
            out.put("prefs:" + id, id);
        }
        return out;
    }

    /**
     * 冲突裁决：sdcard 路径优先于 tmp（tmp 更易被「无权限宿主 + 误写 hardware」毒化）；
     * prefs 再次之。同优先级取先发现。
     */
    private static String pickCanonical(Map<String, String> found) {
        if (found == null || found.isEmpty()) return null;
        Set<String> distinct = new LinkedHashSet<>(found.values());
        if (distinct.size() == 1) {
            return distinct.iterator().next();
        }
        Log.w(TAG, "CONFLICT device_id seeds=" + found);
        String best = null;
        int bestScore = -1;
        for (Map.Entry<String, String> e : found.entrySet()) {
            int score = scorePath(e.getKey());
            if (score > bestScore) {
                bestScore = score;
                best = e.getValue();
            }
        }
        Log.w(TAG, "CONFLICT resolved → " + best + " (heal others if possible)");
        return best;
    }

    private static int scorePath(String path) {
        if (path == null) return 0;
        if (path.contains("/Android/starry_magic/")) return 100;
        if (path.contains("/.starry_magic/")) return 90;
        if (path.contains("/Download/.starry_magic/")) return 70;
        if (path.startsWith("/data/local/tmp/starry_magic")) return 40;
        if (path.startsWith("prefs:")) return 30;
        return 10;
    }

    /** 只补空洞；已有不同合法 id 的文件绝不覆盖。 */
    private static void propagateFillGaps(String id) {
        if (!isValidDevId(id)) return;
        for (String abs : ABS_SHARED_DIRS) {
            File f = new File(abs, FILE_NAME);
            if (!writeOneFileNoClobber(f, id)) {
                writeFileViaSuNoClobber(f.getAbsolutePath(), id);
            }
        }
        File root = Environment.getExternalStorageDirectory();
        if (root == null) return;
        for (String rel : SHARED_REL_DIRS) {
            writeOneFileNoClobber(new File(new File(root, rel), FILE_NAME), id);
        }
    }

    private static List<String> seedPathsPriority() {
        List<String> paths = new ArrayList<>();
        File root = Environment.getExternalStorageDirectory();
        // 收集时也按优先级：先 sdcard 再 tmp（裁决用 score；收集要全）
        if (root != null) {
            for (String rel : SHARED_REL_DIRS) {
                paths.add(new File(new File(root, rel), FILE_NAME).getAbsolutePath());
            }
        }
        for (String abs : ABS_SHARED_DIRS) {
            paths.add(new File(abs, FILE_NAME).getAbsolutePath());
        }
        return paths;
    }

    private static boolean writeOneFileNoClobber(File f, String id) {
        try {
            File dir = f.getParentFile();
            if (dir != null) {
                if (!dir.exists() && !dir.mkdirs()) {
                    return false;
                }
                chmodBestEffort(dir, 0755);
            }
            String existing = readFileTrimmed(f);
            if (isValidDevId(existing)) {
                if (existing.equals(id)) {
                    chmodBestEffort(f, 0666);
                    return true;
                }
                Log.w(TAG, "refuse clobber " + f + " keep=" + existing + " skip=" + id);
                return true; // 视为「已有种子」，不必 su 强写
            }
            try (FileOutputStream fos = new FileOutputStream(f)) {
                fos.write(id.getBytes(StandardCharsets.UTF_8));
                fos.flush();
            }
            //noinspection ResultOfMethodCallIgnored
            f.setReadable(true, false);
            //noinspection ResultOfMethodCallIgnored
            f.setWritable(true, false);
            chmodBestEffort(f, 0666);
            if (isValidDevId(readFileTrimmed(f))) {
                Log.i(TAG, "shared id written @" + f.getAbsolutePath());
                return true;
            }
        } catch (Throwable t) {
            Log.w(TAG, "shared id write fail @" + f + ": " + t.getMessage());
        }
        return false;
    }

    private static List<String> readIdsFromOtherPrefsViaSu() {
        List<String> ids = new ArrayList<>();
        try {
            Process p = Runtime.getRuntime().exec(new String[]{
                    "su", "-c",
                    "grep -h auth_device_id /data/data/*/shared_prefs/starry_magic_prefs.xml 2>/dev/null"
            });
            try (BufferedReader br = new BufferedReader(
                    new InputStreamReader(p.getInputStream(), StandardCharsets.UTF_8))) {
                String line;
                while ((line = br.readLine()) != null) {
                    String id = extractDevIdFromPrefsLine(line);
                    if (isValidDevId(id) && !ids.contains(id)) {
                        ids.add(id);
                    }
                }
            }
            p.waitFor();
        } catch (Throwable ignored) {
        }
        return ids;
    }

    private static String extractDevIdFromPrefsLine(String line) {
        if (line == null) return null;
        int start = line.indexOf(">dev_");
        if (start < 0) return null;
        start += 1; // skip '>'
        int end = line.indexOf('<', start);
        if (end < 0) end = line.length();
        return line.substring(start, end).trim();
    }

    private static String readFileViaSu(String path) {
        if (path == null || path.isEmpty()) return null;
        if (!path.contains("starry_magic") || path.contains("'") || path.contains("\n")) {
            return null;
        }
        try {
            Process p = Runtime.getRuntime().exec(new String[]{
                    "su", "-c", "cat '" + path + "' 2>/dev/null"
            });
            String line;
            try (BufferedReader br = new BufferedReader(
                    new InputStreamReader(p.getInputStream(), StandardCharsets.UTF_8))) {
                line = br.readLine();
            }
            int code = p.waitFor();
            if (code != 0 || line == null) return null;
            line = line.trim();
            return isValidDevId(line) ? line : null;
        } catch (Throwable t) {
            return null;
        }
    }

    private static void writeFileViaSuNoClobber(String path, String id) {
        if (!isValidDevId(id) || path == null || !path.contains("starry_magic")) return;
        if (path.contains("'") || path.contains("\n") || id.contains("'")) return;
        try {
            String existing = readFileViaSu(path);
            if (isValidDevId(existing)) {
                if (!existing.equals(id)) {
                    Log.w(TAG, "refuse su clobber " + path + " keep=" + existing);
                }
                return;
            }
            String dir = new File(path).getParent();
            String cmd = "mkdir -p '" + dir + "' && chmod 755 '" + dir + "'"
                    + " && printf '%s' '" + id + "' > '" + path + "'"
                    + " && chmod 666 '" + path + "'";
            Process p = Runtime.getRuntime().exec(new String[]{"su", "-c", cmd});
            int code = p.waitFor();
            if (code == 0 && isValidDevId(readFileTrimmed(new File(path)))) {
                Log.i(TAG, "shared id written via su @" + path);
            } else if (code == 0 && isValidDevId(readFileViaSu(path))) {
                Log.i(TAG, "shared id written via su (readback su) @" + path);
            } else {
                Log.w(TAG, "shared id su write fail code=" + code + " @" + path);
            }
        } catch (Throwable t) {
            Log.w(TAG, "shared id su write fail @" + path + ": " + t.getMessage());
        }
    }

    private static void chmodBestEffort(File f, int mode) {
        if (f == null) return;
        try {
            Os.chmod(f.getAbsolutePath(), mode);
        } catch (Throwable ignored) {
            try {
                //noinspection ResultOfMethodCallIgnored
                Runtime.getRuntime().exec(new String[]{
                        "chmod", Integer.toOctalString(mode), f.getAbsolutePath()
                }).waitFor();
            } catch (Throwable ignored2) {
            }
        }
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

    private static boolean isValidDevId(String id) {
        return id != null && id.startsWith("dev_") && id.length() >= 8 && id.length() <= 96;
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
