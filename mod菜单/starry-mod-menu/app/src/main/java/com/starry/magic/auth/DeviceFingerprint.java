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
        try {
            Process p = Runtime.getRuntime().exec(new String[]{
                    "su", "-c",
                    "grep -h auth_device_id "
                            + "/data/data/*/shared_prefs/starry_magic_prefs.xml "
                            + "/data/user/0/*/shared_prefs/starry_magic_prefs.xml "
                            + "2>/dev/null"
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
