package com.starry.magic.lvmeng;

import android.app.Application;
import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * 《绿梦：时空之声》直装包的落地引导 Provider —— 只在 AndroidManifest 里声明，
 * {@code classes.dex / classes2.dex / classes3.dex} 一律不改。
 *
 * <p>为什么用 Provider：ContentProvider 的 {@code onCreate} 在 Application.onCreate
 * 之前、远早于 Unity 起来读 YooAsset，用它抢「游戏读 Dll bundle 之前」这个时机，
 * 不必 patch 任何游戏 dex/smali。
 *
 * <p>职责（只做落盘，不碰菜单/授权；那些由框架的 {@code StarryInitProvider} 负责）：
 * <ol>
 *   <li>assets/lvmeng/{__data,__info} → {@code <ext>/yoo/Dll/BundleFiles/e4/<guid>/}；</li>
 *   <li>assets/lvmeng/starry_lvmeng.dll → {@code <filesDir>/starry_lvmeng.dll}（mod 侧读它）；</li>
 *   <li>{@code <ext>/yoo/Dll/ManifestFiles/CacheRecords.snapshot} 不存在时补一条记录，
 *       否则新装首启 YooAsset 找不到该 bundle 的缓存记录，会重新下载官方 bundle 覆盖补丁。</li>
 * </ol>
 *
 * <p>snapshot 格式（逐字段复刻 YooAsset {@code DefaultCacheFileSystem.SaveRecordsSnapshot}，
 * 见 {@code 06_dumped/绿梦_20261016/decompiled/YooAsset/YooAsset/DefaultCacheFileSystem.cs:557}）：
 * <pre>
 *   i32LE magic = 1380270916            // "DCER"
 *   i32LE count = 1
 *   每条：string key, string infoFilePath, string dataFilePath, string dataFileCRC, i64LE dataFileSize
 *   string = C# BinaryWriter 的 7-bit 长度前缀 + UTF-8 字节
 * </pre>
 *
 * <p>多进程防护：只在本进程名 == 包名时工作（复用框架的
 * {@code StarryApp.isMainProcess}）。
 */
public final class LvmengPayloadProvider extends ContentProvider {

    private static final String TAG = "LvmengPayloadProvider";

    /** 现役 Dll bundle GUID（清单 Dll_prod_1.0.0_prod.a_39）。 */
    /* 2026-10-28: v1.0.5 实测的 Dll bundle 是 f8124a60…（雷电 14 上 yoo/Dll/BundleFiles/f8/f8124a60… 的 __data
       被游戏直接读取）。旧值 e4d84398… 是上一版，会让补丁 bundle 落到游戏不看的目录。 */
    private static final String BUNDLE_GUID = "f8124a60540376c3583f7e7d15eaf9e0";
    private static final String GUID_DIR = "f8/" + BUNDLE_GUID;
    private static final String MOD_DLL = "starry_lvmeng.dll";
    /** YooAsset CacheRecords.snapshot 的 RecordSnapshotMagic。 */
    private static final int SNAPSHOT_MAGIC = 1380270916;

    @Override
    public boolean onCreate() {
        try {
            Context holder = getContext();
            Context app = holder == null ? null : holder.getApplicationContext();
            if (!(app instanceof Application)) {
                Log.i(TAG, "no application context, skip");
                return true;
            }
            final Application application = (Application) app;
            if (!com.starry.magic.StarryApp.isMainProcess(application)) {
                Log.i(TAG, "skip（proc=" + com.starry.magic.StarryApp.currentProcessName() + "）");
                return true;
            }
            copyPayload(application);
        } catch (Throwable t) {
            // 任何落地失败都不许把 Provider 拖崩（崩了 = 游戏起不来）
            Log.e(TAG, "onCreate failed", t);
        }
        return true;
    }

    // ------------------------------------------------------------------ 载荷
    private static void copyPayload(Context ctx) {
        File ext = null;
        try {
            ext = ctx.getExternalFilesDir(null);
        } catch (Throwable t) {
            Log.e(TAG, "getExternalFilesDir failed: " + t);
        }
        if (ext == null) {
            Log.e(TAG, "no external files dir, skip");
            return;
        }

        File bdir = new File(ext, "yoo/Dll/BundleFiles/" + GUID_DIR);
        boolean bundleOk = false;
        try {
            if (!bdir.exists() && !bdir.mkdirs()) {
                Log.e(TAG, "mkdir failed: " + bdir);
            }
            copyAsset(ctx, "lvmeng/__data", new File(bdir, "__data"));
            copyAsset(ctx, "lvmeng/__info", new File(bdir, "__info"));
            bundleOk = true;
        } catch (Throwable t) {
            Log.e(TAG, "bundle copy failed: " + t);
        }

        try {
            copyAsset(ctx, "lvmeng/" + MOD_DLL, new File(ctx.getFilesDir(), MOD_DLL));
        } catch (Throwable t) {
            Log.e(TAG, "mod dll copy failed: " + t);
        }

        // bundle 没落成就不写 snapshot：否则游戏会认为「文件已在缓存」，实际却没有
        if (!bundleOk) {
            Log.e(TAG, "bundle not ready, skip snapshot");
            return;
        }
        try {
            writeSnapshotIfAbsent(ctx, ext, bdir);
        } catch (Throwable t) {
            Log.e(TAG, "snapshot write failed: " + t);
        }
    }

    /**
     * 先写 .tmp 再 rename，避免游戏读到半写文件；已存在且大小相同则跳过。
     * （语义照抄旧助手 Boot.copyAsset，见 2026-10-19 归档）
     */
    private static void copyAsset(Context ctx, String asset, File dst) throws IOException {
        InputStream in = null;
        try {
            in = ctx.getAssets().open(asset);
            long avail = in.available();
            // 2026-10-28: always refresh the payload. The device may re-download the
            // official bundle over ours; re-copying on every launch self-heals that.
            if (false && dst.isFile() && avail > 0 && dst.length() == avail) {
                Log.i(TAG, "skip（已存在） " + dst + " (" + avail + " B)");
                return;
            }
            File parent = dst.getParentFile();
            if (parent != null && !parent.exists() && !parent.mkdirs()) {
                throw new IOException("mkdir failed: " + parent);
            }
            File tmp = new File(dst.getAbsolutePath() + ".tmp");
            FileOutputStream out = null;
            long total = 0;
            byte[] buf = new byte[65536];
            try {
                out = new FileOutputStream(tmp);
                int n;
                while ((n = in.read(buf)) > 0) {
                    out.write(buf, 0, n);
                    total += n;
                }
                out.flush();
            } finally {
                if (out != null) {
                    try {
                        out.close();
                    } catch (IOException ignored) {
                    }
                }
            }
            if (!tmp.renameTo(dst)) {
                // 某些 FUSE 卷上 rename 会失败，退回逐字节拷
                FileInputStream fis = null;
                FileOutputStream fos = null;
                try {
                    fis = new FileInputStream(tmp);
                    fos = new FileOutputStream(dst);
                    int n;
                    while ((n = fis.read(buf)) > 0) {
                        fos.write(buf, 0, n);
                    }
                    fos.flush();
                } finally {
                    if (fis != null) {
                        try {
                            fis.close();
                        } catch (IOException ignored) {
                        }
                    }
                    if (fos != null) {
                        try {
                            fos.close();
                        } catch (IOException ignored) {
                        }
                    }
                }
                //noinspection ResultOfMethodCallIgnored
                tmp.delete();
            }
            Log.i(TAG, "asset " + asset + " -> " + dst + " (" + total + " B)");
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException ignored) {
                }
            }
        }
    }

    // ------------------------------------------------------------------ snapshot
    /** snapshot 已存在就完全不动（游戏自己会维护 / 增量更新）。 */
    private static void writeSnapshotIfAbsent(Context ctx, File ext, File bdir) throws IOException {
        File mfDir = new File(ext, "yoo/Dll/ManifestFiles");
        File snapshot = new File(mfDir, "CacheRecords.snapshot");
        if (snapshot.isFile()) {
            Log.i(TAG, "snapshot 已存在，跳过: " + snapshot);
            return;
        }
        File data = new File(bdir, "__data");
        File info = new File(bdir, "__info");
        if (!data.isFile() || !info.isFile()) {
            Log.e(TAG, "bundle files missing, skip snapshot");
            return;
        }
        String crc = readInfoCrc(ctx);
        if (crc == null || crc.isEmpty()) {
            Log.e(TAG, "no crc from __info, skip snapshot");
            return;
        }
        if (!mfDir.exists() && !mfDir.mkdirs()) {
            throw new IOException("mkdir failed: " + mfDir);
        }

        ByteArrayOutputStream body = new ByteArrayOutputStream(256);
        writeIntLE(body, SNAPSHOT_MAGIC);
        writeIntLE(body, 1);
        writeCsString(body, BUNDLE_GUID);
        writeCsString(body, info.getAbsolutePath());
        writeCsString(body, data.getAbsolutePath());
        writeCsString(body, crc);
        writeLongLE(body, data.length());

        File tmp = new File(snapshot.getAbsolutePath() + ".tmp");
        FileOutputStream out = new FileOutputStream(tmp);
        try {
            body.writeTo(out);
            out.flush();
        } finally {
            out.close();
        }
        if (!tmp.renameTo(snapshot)) {
            FileInputStream fis = new FileInputStream(tmp);
            FileOutputStream fos = new FileOutputStream(snapshot);
            try {
                byte[] buf = new byte[8192];
                int n;
                while ((n = fis.read(buf)) > 0) {
                    fos.write(buf, 0, n);
                }
                fos.flush();
            } finally {
                fis.close();
                fos.close();
            }
            //noinspection ResultOfMethodCallIgnored
            tmp.delete();
        }
        Log.i(TAG, "snapshot -> " + snapshot + " (" + body.size() + " B, crc=" + crc
                + ", size=" + data.length() + ")");
    }

    /** 读 assets/lvmeng/__info（18 B = u16LE 长度 + ASCII CRC + i64LE 大小），只取 CRC 串。 */
    private static String readInfoCrc(Context ctx) throws IOException {
        InputStream in = ctx.getAssets().open("lvmeng/__info");
        try {
            byte[] buf = new byte[2];
            if (readFully(in, buf, 2) != 2) {
                return null;
            }
            int len = (buf[0] & 0xFF) | ((buf[1] & 0xFF) << 8);
            if (len <= 0 || len > 64) {
                return null;
            }
            byte[] crc = new byte[len];
            if (readFully(in, crc, len) != len) {
                return null;
            }
            return new String(crc, "US-ASCII");
        } finally {
            try {
                in.close();
            } catch (IOException ignored) {
            }
        }
    }

    private static int readFully(InputStream in, byte[] buf, int len) throws IOException {
        int off = 0;
        while (off < len) {
            int n = in.read(buf, off, len - off);
            if (n <= 0) {
                break;
            }
            off += n;
        }
        return off;
    }

    // ------------------------------------------------------- 小端 + C# BinaryWriter 编码
    private static void writeIntLE(OutputStream out, int v) throws IOException {
        out.write(v & 0xFF);
        out.write((v >>> 8) & 0xFF);
        out.write((v >>> 16) & 0xFF);
        out.write((v >>> 24) & 0xFF);
    }

    private static void writeLongLE(OutputStream out, long v) throws IOException {
        for (int i = 0; i < 8; i++) {
            out.write((int) ((v >>> (8 * i)) & 0xFF));
        }
    }

    /**
     * C# {@code BinaryWriter.Write(string)}：7-bit 编码的 UTF-8 字节长度前缀 + UTF-8 字节。
     * 注意是 do-while —— 空串也要写一个 0x00。
     */
    private static void writeCsString(OutputStream out, String s) throws IOException {
        byte[] b = s.getBytes("UTF-8");
        int n = b.length;
        do {
            int x = n & 0x7F;
            n >>>= 7;
            if (n > 0) {
                x |= 0x80;
            }
            out.write(x);
        } while (n > 0);
        out.write(b);
    }

    // ------------------------------------------------------------------ ContentProvider 空实现
    @Override
    public Cursor query(Uri uri, String[] projection, String selection,
                        String[] selectionArgs, String sortOrder) {
        return null;
    }

    @Override
    public String getType(Uri uri) {
        return null;
    }

    @Override
    public Uri insert(Uri uri, ContentValues values) {
        return null;
    }

    @Override
    public int delete(Uri uri, String selection, String[] selectionArgs) {
        return 0;
    }

    @Override
    public int update(Uri uri, ContentValues values, String selection, String[] selectionArgs) {
        return 0;
    }
}
