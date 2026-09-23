package com.starry.magic.ota;

import android.content.Context;
import android.content.pm.ApplicationInfo;
import android.os.Build;
import android.util.Log;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;

/**
 * 读 so 内烘焙的构建戳：{@code STARRYSTAMP1:EPOCH=<秒>;}。
 *
 * <p>OTA 守卫用「包内 so 戳 vs OTA so 戳」判新旧，取代原先的
 * {@code PackageInfo.lastUpdateTime}：乐变（免安装 overlay）会把 lastUpdateTime 在真实
 * 安装时间与 overlay APK 的 mtime 之间来回抖，旧实现据此误判「覆盖安装」，
 * 每次冷启动都清掉刚下载的补丁（无悔华夏 2026-09-24 实测：PlayOta clearLocalOta）。
 *
 * <p>戳是 .rodata 里的 ASCII 字面量，so 为 hidden visibility + strip（dlsym 拿不到），
 * 因此按字节扫描文件头若干 MB。
 */
final class OtaSoStamp {

    private static final String TAG = "OtaSoStamp";
    private static final String MARK = "STARRYSTAMP1:EPOCH=";
    private static final byte[] MARK_BYTES = MARK.getBytes(StandardCharsets.US_ASCII);
    /** 字面量必在 .rodata，扫前 8MB 足够（当前 so 约 180KB） */
    private static final int MAX_SCAN = 8 * 1024 * 1024;
    private static final int MIN_DIGITS = 9;

    private OtaSoStamp() {}

    /** 读任意 so 文件的构建戳；不存在/读不到/无戳返回 0。 */
    static long of(File so) {
        if (so == null || !so.isFile()) return 0L;
        InputStream in = null;
        try {
            long len = so.length();
            if (len <= 0L) return 0L;
            int cap = (int) Math.min(len, (long) MAX_SCAN);
            byte[] data = new byte[cap];
            in = new FileInputStream(so);
            int off = 0;
            int n;
            while (off < cap && (n = in.read(data, off, cap - off)) > 0) {
                off += n;
            }
            return scan(data, off);
        } catch (Throwable t) {
            Log.w(TAG, "read stamp failed: " + so + " : " + t.getMessage());
            return 0L;
        } finally {
            try {
                if (in != null) in.close();
            } catch (Throwable ignore) {
            }
        }
    }

    /**
     * 包内 so 的构建戳（取所有候选的最大值）。
     * 候选：extractNativeLibs 解出的 nativeLibraryDir / legacyNativeLibraryDir，
     * 乐变 overlay 目录里 loadLibrary 实际会加载的那份，以及 APK zip 条目兜底。
     */
    static long apkStamp(Context ctx) {
        if (ctx == null) return 0L;
        long best = 0L;
        try {
            ApplicationInfo ai = ctx.getApplicationInfo();
            String[] abis = abisOf();
            List<File> cands = new ArrayList<File>();
            // 1) 解出的原生库目录（extractNativeLibs=true）——即 loadLibrary 实际会命中的那份
            addLib(cands, ai.nativeLibraryDir, null);
            // 2) legacy 目录的等价推导（ApplicationInfo.legacyNativeLibraryDir 是 @hide，公开 SDK 编不过）
            File apkDir = ai.sourceDir != null ? new File(ai.sourceDir).getParentFile() : null;
            if (apkDir != null) addLib(cands, new File(apkDir, "lib").getAbsolutePath(), null);
            for (String abi : abis) {
                addLib(cands, ai.nativeLibraryDir, abi);
                if (apkDir != null) addLib(cands, new File(apkDir, "lib").getAbsolutePath(), abi);
            }
            // 3) 乐变（免安装）overlay：/data/data/<pkg>/lebian/<pkg>-N/lib/<abi>/libstarry_magic.so
            File dataDir = ctx.getFilesDir() != null ? ctx.getFilesDir().getParentFile() : null;
            if (dataDir != null) {
                File[] kids = new File(dataDir, "lebian").listFiles();
                if (kids != null) {
                    for (File d : kids) {
                        if (d == null || !d.isDirectory()) continue;
                        for (String abi : abis) {
                            addLib(cands, new File(d, "lib").getAbsolutePath(), abi);
                        }
                    }
                }
            }
            for (File f : cands) {
                long v = of(f);
                if (v > best) best = v;
            }
            if (best == 0L) {
                for (String abi : abis) {
                    best = fromApkZip(ai, abi);
                    if (best != 0L) break;
                }
            }
        } catch (Throwable t) {
            Log.w(TAG, "apkStamp: " + t.getMessage());
        }
        return best;
    }

    /** 候选 ABI 列表（primaryCpuAbi 是 @hide，公开 SDK 只能用 SUPPORTED_ABIS）。 */
    private static String[] abisOf() {
        try {
            String[] abis = Build.SUPPORTED_ABIS;
            if (abis != null && abis.length > 0) {
                return abis.length > 2 ? new String[] {abis[0], abis[1]} : abis;
            }
        } catch (Throwable ignore) {
        }
        return new String[] {"arm64-v8a"};
    }

    private static void addLib(List<File> out, String dir, String abi) {
        if (dir == null || dir.isEmpty()) return;
        File base = abi == null || abi.isEmpty() ? new File(dir) : new File(dir, abi);
        out.add(new File(base, "libstarry_magic.so"));
    }

    /** 兜底：直接从已安装 APK 的 zip 条目里扫（nativeLibraryDir 不可读时用）。 */
    private static long fromApkZip(ApplicationInfo ai, String abi) {
        String apk = ai.publicSourceDir != null ? ai.publicSourceDir : ai.sourceDir;
        if (apk == null) return 0L;
        ZipFile zf = null;
        InputStream in = null;
        try {
            zf = new ZipFile(apk);
            ZipEntry e = zf.getEntry("lib/" + abi + "/libstarry_magic.so");
            if (e == null) return 0L;
            long len = e.getSize();
            int cap = len > 0 ? (int) Math.min(len, (long) MAX_SCAN) : MAX_SCAN;
            byte[] data = new byte[cap];
            in = zf.getInputStream(e);
            int off = 0;
            int n;
            while (off < cap && (n = in.read(data, off, cap - off)) > 0) {
                off += n;
            }
            return scan(data, off);
        } catch (Throwable t) {
            Log.w(TAG, "fromApkZip: " + t.getMessage());
            return 0L;
        } finally {
            try {
                if (in != null) in.close();
            } catch (Throwable ignore) {
            }
            try {
                if (zf != null) zf.close();
            } catch (Throwable ignore) {
            }
        }
    }

    /** 在字节数组里找 "STARRYSTAMP1:EPOCH=<digits>"，返回秒级时间戳。 */
    private static long scan(byte[] data, int len) {
        if (data == null || len < MARK_BYTES.length + MIN_DIGITS) return 0L;
        outer:
        for (int i = 0; i + MARK_BYTES.length + MIN_DIGITS <= len; i++) {
            for (int j = 0; j < MARK_BYTES.length; j++) {
                if (data[i + j] != MARK_BYTES[j]) continue outer;
            }
            int p = i + MARK_BYTES.length;
            long v = 0L;
            int digits = 0;
            while (p < len && digits < 12) {
                int c = data[p] & 0xff;
                if (c < '0' || c > '9') break;
                v = v * 10L + (long) (c - '0');
                p++;
                digits++;
            }
            if (digits >= MIN_DIGITS) return v;
        }
        return 0L;
    }
}
