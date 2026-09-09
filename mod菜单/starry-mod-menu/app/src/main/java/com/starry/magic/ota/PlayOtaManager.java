package com.starry.magic.ota;

import android.content.Context;
import android.util.Log;

import com.starry.magic.auth.AuthClient;
import com.starry.magic.bridge.ModBridge;
import com.starry.magic.menu.Preferences;

import org.json.JSONObject;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.concurrent.atomic.AtomicBoolean;

/**
 * 设置页手动「更新游戏补丁」：查 latest → 确认后合并下载 → 须完全重启游戏。
 * JSON 与 so 可共存；只更新一侧时保留另一侧。二者都要求杀进程再开（不做热替换 so）。
 */
public final class PlayOtaManager {
    private static final String TAG = "PlayOta";
    private static final AtomicBoolean IN_FLIGHT = new AtomicBoolean(false);

    public static final class CheckResult {
        public final boolean ok;
        public final String message;
        public final int remoteVer;
        public final int localVer;
        public final boolean hasUpdate;
        public final String note;
        public final JSONObject manifest;

        CheckResult(boolean ok, String message, int remoteVer, int localVer,
                    boolean hasUpdate, String note, JSONObject manifest) {
            this.ok = ok;
            this.message = message;
            this.remoteVer = remoteVer;
            this.localVer = localVer;
            this.hasUpdate = hasUpdate;
            this.note = note;
            this.manifest = manifest;
        }
    }

    public static final class ApplyResult {
        public final boolean ok;
        public final String message;
        public final int ver;

        ApplyResult(boolean ok, String message, int ver) {
            this.ok = ok;
            this.message = message;
            this.ver = ver;
        }
    }

    private PlayOtaManager() {}

    public static CheckResult check(Context ctx) {
        Preferences.with(ctx);
        AuthClient.init(ctx);
        String gameId = Preferences.getAuthGameId();
        if (gameId == null || gameId.isEmpty()) {
            ModBridge.syncAuthGameIdFromNative();
            gameId = Preferences.getAuthGameId();
        }
        if (gameId == null || gameId.isEmpty()) {
            return new CheckResult(false, "未识别游戏 id", 0, 0, false, "", null);
        }
        int local = Preferences.getOtaLocalVer(gameId);
        String raw = AuthClient.request(
                Preferences.getAuthBaseUrl(), "GET",
                "/v1/ota/" + gameId + "/manifest", null, null);
        try {
            JSONObject root = new JSONObject(raw);
            if (!root.optBoolean("ok", false)) {
                String msg = root.optString("message", "暂无游戏补丁");
                return new CheckResult(false, msg, 0, local, false, "", null);
            }
            JSONObject man = root.optJSONObject("data");
            if (man == null) {
                return new CheckResult(false, "清单为空", 0, local, false, "", null);
            }
            int ver = man.optInt("ver", 0);
            String note = man.optString("note", "");
            boolean has = ver > local;
            String msg;
            if (has) {
                msg = "有新补丁 v" + ver + (note.isEmpty() ? "" : ("：" + note));
            } else if (ver > 0) {
                msg = "已是最新 v" + ver + (note.isEmpty() ? "" : ("\n" + note));
            } else {
                msg = "暂无补丁";
            }
            return new CheckResult(true, msg, ver, local, has, note, man);
        } catch (Exception e) {
            Log.e(TAG, "check failed", e);
            return new CheckResult(false, "检查失败", 0, local, false, "", null);
        }
    }

    /**
     * 覆盖下载到 {@code starry_ota/{game_id}/}（tmp → 校验 → 替换）。
     * 成功后写本地 ver；调用方须提示完全划掉游戏再开。
     * sha 未变则跳过该文件下载；并发第二次调用直接拒绝。
     */
    public static ApplyResult downloadAndApply(Context ctx, JSONObject man) {
        return downloadAndApply(ctx, man, null);
    }

    /**
     * @param progress 下载进度回调（后台线程触发）；null 则不回调
     */
    public static ApplyResult downloadAndApply(Context ctx, JSONObject man,
                                               AuthClient.DownloadProgress progress) {
        if (!IN_FLIGHT.compareAndSet(false, true)) {
            return new ApplyResult(false, "正在下载中，请稍候", 0);
        }
        try {
            return downloadAndApplyLocked(ctx, man, progress);
        } finally {
            IN_FLIGHT.set(false);
        }
    }

    private static ApplyResult downloadAndApplyLocked(Context ctx, JSONObject man,
                                                      AuthClient.DownloadProgress progress) {
        Preferences.with(ctx);
        AuthClient.init(ctx);
        if (man == null) {
            return new ApplyResult(false, "无清单", 0);
        }
        String gameId = man.optString("game_id", Preferences.getAuthGameId());
        if (gameId == null || gameId.isEmpty()) {
            return new ApplyResult(false, "game_id 缺失", 0);
        }
        int ver = man.optInt("ver", 0);
        boolean hasJson = man.optBoolean("has_json", false);
        boolean hasSo = man.optBoolean("has_so", false);
        String jsonSha = man.optString("json_sha256", "");
        String soSha = man.optString("so_sha256", "");
        String note = man.optString("note", "");
        String sig = man.optString("sig", "");

        String payload = gameId + "|" + ver + "|"
                + (jsonSha.isEmpty() ? "-" : jsonSha) + "|"
                + (soSha.isEmpty() ? "-" : soSha) + "|"
                + (note == null ? "" : note);
        try {
            if (!ModBridge.nativeVerifyOtaSig(payload, sig)) {
                return new ApplyResult(false, "补丁签名无效", 0);
            }
        } catch (UnsatisfiedLinkError e) {
            return new ApplyResult(false, "native 未就绪，无法验签", 0);
        }

        File destDir = PlayOtaPaths.gameDir(ctx, gameId);
        File tmpDir = new File(destDir, "_tmp");
        try {
            if (tmpDir.exists()) deleteRecursive(tmpDir);
            if (!tmpDir.mkdirs() && !tmpDir.isDirectory()) {
                return new ApplyResult(false, "无法创建临时目录", 0);
            }

            String base = Preferences.getAuthBaseUrl();
            String token = Preferences.getAuthToken();
            if (token != null) token = token.trim();
            boolean cardOn = Preferences.isCardAuthRequired();
            boolean hasToken = token != null && !token.isEmpty();
            // 关卡密时授权态无会话 token，仍应能下补丁；开卡密则必须先登录
            if (!hasToken && cardOn) {
                return new ApplyResult(false, "下载补丁请先登录卡密", 0);
            }
            String bearer = hasToken ? token : null;
            boolean gotJson = false;
            boolean gotSo = false;
            boolean skippedJson = false;
            boolean skippedSo = false;

            if (hasJson) {
                File local = new File(destDir, "patch.json");
                if (local.isFile() && !jsonSha.isEmpty()
                        && jsonSha.equalsIgnoreCase(sha256File(local))) {
                    copyFile(local, new File(tmpDir, "patch.json"));
                    gotJson = true;
                    skippedJson = true;
                    Log.i(TAG, "skip json download (sha match)");
                } else {
                    File f = new File(tmpDir, "patch.json");
                    String err = AuthClient.downloadToFile(
                            base, "/v1/ota/" + gameId + "/file/patch.json", f, bearer, progress);
                    if (err != null) return new ApplyResult(false, "下载 JSON 失败：" + err, 0);
                    if (!jsonSha.isEmpty() && !jsonSha.equalsIgnoreCase(sha256File(f))) {
                        return new ApplyResult(false, "JSON 校验失败", 0);
                    }
                    gotJson = true;
                }
            }
            if (hasSo) {
                File local = new File(destDir, "libstarry_magic.so");
                if (local.isFile() && !soSha.isEmpty()
                        && soSha.equalsIgnoreCase(sha256File(local))) {
                    copyFile(local, new File(tmpDir, "libstarry_magic.so"));
                    gotSo = true;
                    skippedSo = true;
                    Log.i(TAG, "skip so download (sha match)");
                } else {
                    File f = new File(tmpDir, "libstarry_magic.so");
                    String err = AuthClient.downloadToFile(
                            base, "/v1/ota/" + gameId + "/file/libstarry_magic.so", f, bearer, progress);
                    if (err != null) return new ApplyResult(false, "下载 so 失败：" + err, 0);
                    if (!soSha.isEmpty() && !soSha.equalsIgnoreCase(sha256File(f))) {
                        return new ApplyResult(false, "so 校验失败", 0);
                    }
                    gotSo = true;
                }
            }

            // 合并：未下发的一侧保留本机旧文件（与服务器 latest 策略一致）
            if (!gotJson) {
                File old = new File(destDir, "patch.json");
                if (old.isFile()) copyFile(old, new File(tmpDir, "patch.json"));
            }
            if (!gotSo) {
                File old = new File(destDir, "libstarry_magic.so");
                if (old.isFile()) copyFile(old, new File(tmpDir, "libstarry_magic.so"));
            }

            // 写 manifest
            File manOut = new File(tmpDir, "manifest.json");
            try (OutputStream os = new FileOutputStream(manOut)) {
                os.write(man.toString().getBytes(StandardCharsets.UTF_8));
            }

            if (!destDir.exists() && !destDir.mkdirs()) {
                return new ApplyResult(false, "无法创建 OTA 目录", 0);
            }
            for (String name : new String[]{"manifest.json", "patch.json", "libstarry_magic.so"}) {
                File old = new File(destDir, name);
                if (old.exists() && !old.delete()) {
                    Log.w(TAG, "unlink old failed: " + old);
                }
            }
            File[] parts = tmpDir.listFiles();
            if (parts != null) {
                for (File p : parts) {
                    File target = new File(destDir, p.getName());
                    if (!p.renameTo(target)) {
                        copyFile(p, target);
                        //noinspection ResultOfMethodCallIgnored
                        p.delete();
                    }
                }
            }
            deleteRecursive(tmpDir);

            Preferences.setOtaLocalVer(gameId, ver);
            Log.i(TAG, "OTA applied game=" + gameId + " ver=" + ver
                    + " json=" + hasJson + " so=" + hasSo
                    + " skipJson=" + skippedJson + " skipSo=" + skippedSo);
            String tip = (skippedJson && skippedSo)
                    ? ("已同步 v" + ver + "（本地文件未变），请完全划掉游戏再开")
                    : ("已下载 v" + ver + "，请完全划掉游戏再开");
            return new ApplyResult(true, tip, ver);
        } catch (Exception e) {
            Log.e(TAG, "downloadAndApply failed", e);
            deleteRecursive(tmpDir);
            return new ApplyResult(false, "下载失败：" + e.getMessage(), 0);
        }
    }

    /** 启动后读本地 patch.json 喂给 native（偏移等）。 */
    public static void applyLocalPatchIfAny(Context ctx) {
        try {
            Preferences.with(ctx);
            String gameId = Preferences.getAuthGameId();
            if (gameId == null || gameId.isEmpty()) {
                ModBridge.syncAuthGameIdFromNative();
                gameId = Preferences.getAuthGameId();
            }
            if (gameId == null || gameId.isEmpty()) {
                Log.i(TAG, "applyLocalPatch skip: no game_id");
                return;
            }
            File patch = PlayOtaPaths.patchJson(ctx, gameId);
            if (!patch.isFile()) {
                Log.i(TAG, "applyLocalPatch skip: no patch.json for " + gameId);
                return;
            }
            String json;
            try (InputStream in = new FileInputStream(patch)) {
                long len = patch.length();
                if (len <= 0) return;
                if (len > 1024 * 1024) {
                    Log.w(TAG, "patch.json too large: " + len + ", skip");
                    return;
                }
                byte[] buf = new byte[(int) len];
                int off = 0;
                int n;
                while (off < buf.length && (n = in.read(buf, off, buf.length - off)) > 0) {
                    off += n;
                }
                if (off == 0) return;
                json = new String(buf, 0, off, StandardCharsets.UTF_8);
            }
            ModBridge.nativeApplyPlayPatch(json);
            Log.i(TAG, "local patch.json applied for " + gameId + " bytes=" + json.length());
        } catch (Throwable t) {
            Log.w(TAG, "applyLocalPatchIfAny: " + t.getMessage(), t);
        }
    }

    /** 设置页展示：本地已下载补丁的 ver + note（说明给玩家看，不只后台）。 */
    public static String localStatusText(Context ctx) {
        try {
            Preferences.with(ctx);
            String gameId = Preferences.getAuthGameId();
            if (gameId == null || gameId.isEmpty()) {
                ModBridge.syncAuthGameIdFromNative();
                gameId = Preferences.getAuthGameId();
            }
            if (gameId == null || gameId.isEmpty()) return "尚未识别游戏";
            File manFile = PlayOtaPaths.manifest(ctx, gameId);
            if (!manFile.isFile()) {
                int v = Preferences.getOtaLocalVer(gameId);
                return v > 0 ? ("本地 v" + v) : "尚未下载补丁";
            }
            StringBuilder sb = new StringBuilder();
            try (InputStream in = new FileInputStream(manFile)) {
                byte[] buf = new byte[4096];
                int n;
                while ((n = in.read(buf)) > 0) sb.append(new String(buf, 0, n, StandardCharsets.UTF_8));
            }
            JSONObject man = new JSONObject(sb.toString());
            int ver = man.optInt("ver", Preferences.getOtaLocalVer(gameId));
            String note = man.optString("note", "").trim();
            if (ver <= 0) return "尚未下载补丁";
            if (note.isEmpty()) return "当前 v" + ver;
            return "当前 v" + ver + "：" + note;
        } catch (Throwable t) {
            return "补丁状态未知";
        }
    }

    /**
     * 覆盖安装新星光包后：{@code PackageInfo.lastUpdateTime} 会变，但 {@code files/starry_ota/}
     * 仍在 → {@link StarryNativeLoad} 会继续优先加载旧 OTA so，包内新 so 永远进不来。
     * 在首次 load so 之前调用：若 APK 更新时间相对上次启动变了且本地有 OTA，则清空。
     *
     * @return true 表示本次清掉了本地 OTA
     */
    public static boolean invalidateOtaIfApkUpdated(Context ctx) {
        if (ctx == null) return false;
        long nowLu;
        try {
            android.content.pm.PackageInfo pi =
                    ctx.getPackageManager().getPackageInfo(ctx.getPackageName(), 0);
            nowLu = pi.lastUpdateTime;
        } catch (Throwable t) {
            Log.w(TAG, "invalidateOtaIfApkUpdated: no PackageInfo", t);
            return false;
        }
        try {
            Preferences.with(ctx);
        } catch (Throwable t) {
            Log.w(TAG, "invalidateOtaIfApkUpdated: prefs", t);
            return false;
        }
        long stored = Preferences.getOtaApkLastUpdateTime();
        boolean hasOta = hasLocalOtaFiles(ctx);
        // 仅当 APK 变新（覆盖安装）才清 OTA。nowLu!=stored 含「变小/乐变抖动」会误删刚下的补丁。
        if (stored != 0L && nowLu > stored && hasOta) {
            Log.i(TAG, "APK lastUpdateTime " + stored + " -> " + nowLu
                    + " (newer), clearing starry_ota so APK so can load");
            clearLocalOta(ctx);
            Preferences.setOtaApkLastUpdateTime(nowLu);
            return true;
        }
        if (stored != nowLu) {
            Preferences.setOtaApkLastUpdateTime(nowLu);
        }
        return false;
    }

    /** 是否存在任意 game 的 OTA so / manifest / patch.json */
    public static boolean hasLocalOtaFiles(Context ctx) {
        File root = PlayOtaPaths.root(ctx);
        if (!root.isDirectory()) return false;
        File[] kids = root.listFiles();
        if (kids == null) return false;
        for (File d : kids) {
            if (!d.isDirectory()) continue;
            if (new File(d, "libstarry_magic.so").isFile()) return true;
            if (new File(d, "manifest.json").isFile()) return true;
            if (new File(d, "patch.json").isFile()) return true;
        }
        return false;
    }

    /**
     * 删除 {@code files/starry_ota/} 并清本地 OTA ver。
     * 若当前进程已 load 过 OTA so，须完全杀进程后再开才会改用包内 so。
     */
    public static ClearResult clearLocalOta(Context ctx) {
        if (ctx == null) return new ClearResult(false, "无 Context");
        try {
            Preferences.with(ctx);
            File root = PlayOtaPaths.root(ctx);
            boolean existed = root.exists();
            deleteRecursive(root);
            Preferences.clearAllOtaLocalVers();
            // 保持 apk lastUpdate 指纹，避免下次启动误判「又更新了」空转
            String msg = existed
                    ? "已清除游戏补丁，请完全划掉游戏再开（将使用包内 so）"
                    : "本地没有游戏补丁";
            Log.i(TAG, "clearLocalOta done existed=" + existed);
            return new ClearResult(true, msg);
        } catch (Throwable t) {
            Log.e(TAG, "clearLocalOta failed", t);
            return new ClearResult(false, "清除失败：" + t.getMessage());
        }
    }

    public static final class ClearResult {
        public final boolean ok;
        public final String message;

        ClearResult(boolean ok, String message) {
            this.ok = ok;
            this.message = message;
        }
    }

    private static String sha256File(File f) throws Exception {
        MessageDigest md = MessageDigest.getInstance("SHA-256");
        try (InputStream in = new FileInputStream(f)) {
            byte[] buf = new byte[8192];
            int n;
            while ((n = in.read(buf)) > 0) md.update(buf, 0, n);
        }
        byte[] dig = md.digest();
        StringBuilder sb = new StringBuilder(dig.length * 2);
        for (byte b : dig) sb.append(String.format("%02x", b));
        return sb.toString();
    }

    private static void copyFile(File from, File to) throws Exception {
        try (InputStream in = new FileInputStream(from);
             OutputStream out = new FileOutputStream(to)) {
            byte[] buf = new byte[8192];
            int n;
            while ((n = in.read(buf)) > 0) out.write(buf, 0, n);
        }
    }

    private static void deleteRecursive(File f) {
        if (f == null || !f.exists()) return;
        if (f.isDirectory()) {
            File[] kids = f.listFiles();
            if (kids != null) {
                for (File k : kids) deleteRecursive(k);
            }
        }
        //noinspection ResultOfMethodCallIgnored
        f.delete();
    }
}
