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
import java.util.ArrayList;
import java.util.List;

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
 *   <li>定位「当前 Dll 包」的缓存目录：读游戏自己的 {@code CacheRecords.snapshot}，
 *       取其中**路径落在 {@code /yoo/Dll/BundleFiles/} 下**的记录，用记录的
 *       {@code dataFilePath} 父目录（= {@code BundleFiles/<xx>/<guid>/}）作为落地目标；
 *   <li>assets/lvmeng/{__data,__info} → 上述每个目标目录；
 *   <li>assets/lvmeng/starry_lvmeng.dll → {@code <filesDir>/starry_lvmeng.dll}（mod 侧读它）；
 *   <li>把这些记录的 {@code crc/size} 改成**我们自己 bundle 的** crc/size 后整份重写 snapshot
 *       （其余记录逐字段原样保留）——否则游戏用官方 crc/size 判「缓存无效」会删档重下，把补丁冲掉。
 * </ol>
 *
 * <p><b>为什么改成动态定位（2026-10-31，r8）</b>：此前 GUID 是写死的常量，游戏每次热更换 Dll bundle
 * 哈希（{@code a_55→a_56→a_57}）都会换目录并清掉旧目录 ⇒ 补丁不被加载、五项功能全失效，已连续
 * 踩两轮。改为按 snapshot 记录定位后，热更换代不再需要改包，装一次管到底。
 * 写死的 {@link #BUNDLE_GUID} 只保留为**兜底**：首装、或热更首启（游戏还没写出 snapshot）时用。
 *
 * <p>snapshot 格式（逐字段复刻 YooAsset {@code DefaultCacheFileSystem.SaveRecordsSnapshot}，
 * 见 {@code 06_dumped/绿梦_20261016/decompiled/YooAsset/YooAsset/DefaultCacheFileSystem.cs:557}）：
 * <pre>
 *   i32LE magic = 1380270916            // "DCER"
 *   i32LE count = N
 *   每条：string key(guid), string infoFilePath, string dataFilePath, string dataFileCRC, i64LE dataFileSize
 *   string = C# BinaryWriter 的 7-bit 长度前缀 + UTF-8 字节
 * </pre>
 *
 * <p>多进程防护：只在本进程名 == 包名时工作（复用框架的
 * {@code StarryApp.isMainProcess}）。
 */
public final class LvmengPayloadProvider extends ContentProvider {

    private static final String TAG = "LvmengPayloadProvider";

    /** **兜底** GUID：仅当 snapshot 不存在/解析失败/无 Dll 记录时使用（首装首启、热更首启）。 */
    /* 2026-10-31（r8）：热更换代已由 snapshot 动态定位吸收，本常量不再需要每轮跟着改；
       保留最近一次已知值（清单 Dll_prod_1.0.0_prod.a_57，游戏构建时间 2026/10/30 19:35）作兜底。
       见 10_docs/绿梦/热更20261030_修复.md §8.6 / §9。 */
    private static final String BUNDLE_GUID = "c9e46c6e213e2d705366b41cddf59809";
    private static final String GUID_DIR = "c9/" + BUNDLE_GUID;
    private static final String MOD_DLL = "starry_lvmeng.dll";
    /** YooAsset CacheRecords.snapshot 的 RecordSnapshotMagic。 */
    private static final int SNAPSHOT_MAGIC = 1380270916;
    /** Dll 包缓存记录的路径特征（用于把「Dll 包」记录和 res/其它包记录区分开）。 */
    private static final String DLL_PATH_MARKER = "/yoo/Dll/BundleFiles/";
    /** 记录数上限（防御：snapshot 被读坏时不要按天文数字分配/循环）。 */
    private static final int MAX_RECORDS = 256;

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

        // ---- ① 定位目标目录：按 snapshot 记录（准），失败才回退硬编码 GUID ----
        File mfDir = new File(ext, "yoo/Dll/ManifestFiles");
        File snapshot = new File(mfDir, "CacheRecords.snapshot");
        List<Record> records = null;
        String why;
        if (!snapshot.isFile()) {
            why = "snapshot 不存在";
        } else {
            try {
                records = parseSnapshot(snapshot);
                why = "ok";
            } catch (Throwable t) {
                why = "snapshot 解析失败(" + t.getMessage() + ")";
                records = null;
            }
        }

        List<Record> dllRecords = new ArrayList<Record>();
        if (records != null) {
            for (Record r : records) {
                if (r.isDll()) {
                    dllRecords.add(r);
                }
            }
        }

        List<File> dirs = new ArrayList<File>();
        for (Record r : dllRecords) {
            File d = r.dataDir();
            if (d != null && dirs.indexOf(d) < 0) {
                dirs.add(d);
            }
        }

        boolean dynamic = !dirs.isEmpty();
        if (!dynamic) {
            // 兜底：首装首启（游戏还没建 snapshot）、或 snapshot 里没有 Dll 记录
            Log.w(TAG, "动态定位失败[" + why + "] records="
                    + (records == null ? -1 : records.size())
                    + " ⇒ 回退硬编码 GUID " + BUNDLE_GUID);
            dirs.add(new File(ext, "yoo/Dll/BundleFiles/" + GUID_DIR));
        } else {
            Log.i(TAG, "动态定位 records=" + records.size() + " dllRecords=" + dllRecords.size()
                    + " 目标目录=" + dirs.size() + " → " + dirs.get(0).getAbsolutePath());
        }

        // ---- ② 逐目录落载荷 ----
        boolean allOk = true;
        for (File bdir : dirs) {
            try {
                if (!bdir.exists() && !bdir.mkdirs()) {
                    Log.e(TAG, "mkdir failed: " + bdir);
                    allOk = false;
                    continue;
                }
                copyAsset(ctx, "lvmeng/__data", new File(bdir, "__data"));
                copyAsset(ctx, "lvmeng/__info", new File(bdir, "__info"));
            } catch (Throwable t) {
                Log.e(TAG, "bundle copy failed @" + bdir + ": " + t);
                allOk = false;
            }
        }

        try {
            copyAsset(ctx, "lvmeng/" + MOD_DLL, new File(ctx.getFilesDir(), MOD_DLL));
        } catch (Throwable t) {
            Log.e(TAG, "mod dll copy failed: " + t);
        }

        // bundle 没落成就不写 snapshot：否则游戏会认为「文件已在缓存」，实际却没有
        if (!allOk) {
            Log.e(TAG, "bundle not ready, skip snapshot");
            return;
        }

        // ---- ③ 重写 snapshot：我方记录改成我们的 crc/size，其余记录原样保留 ----
        try {
            String crc = readInfoCrc(ctx);
            if (crc == null || crc.isEmpty()) {
                Log.e(TAG, "no crc from __info, skip snapshot");
                return;
            }
            long size = new File(dirs.get(0), "__data").length();
            if (size <= 0) {
                Log.e(TAG, "empty __data, skip snapshot");
                return;
            }

            List<Record> out;
            if (dynamic) {
                out = records;
                for (Record r : dllRecords) {
                    r.crc = crc;
                    r.size = size;
                }
            } else {
                File bdir = dirs.get(0);
                Record r = new Record();
                r.key = BUNDLE_GUID;
                r.infoPath = new File(bdir, "__info").getAbsolutePath();
                r.dataPath = new File(bdir, "__data").getAbsolutePath();
                r.crc = crc;
                r.size = size;
                out = new ArrayList<Record>();
                out.add(r);
            }
            writeSnapshot(snapshot, out);
            Log.i(TAG, "snapshot -> " + snapshot + " (records=" + out.size()
                    + ", crc=" + crc + ", size=" + size + ", dynamic=" + dynamic + ")");
        } catch (Throwable t) {
            Log.e(TAG, "snapshot write failed", t);
        }
    }

    // ------------------------------------------------------------------ snapshot 记录
    /** 一条缓存记录（字段顺序与 YooAsset 写盘顺序一致）。包级可见 = 供离线自测。 */
    static final class Record {
        String key;
        String infoPath;
        String dataPath;
        String crc;
        long size;

        boolean isDll() {
            return dataPath != null && dataPath.indexOf(DLL_PATH_MARKER) >= 0;
        }

        /** dataFilePath 的父目录（= BundleFiles/<xx>/<guid>/）；不像 Dll 目录就返回 null。 */
        File dataDir() {
            // 判据一律用记录里的**原始路径串**：getAbsolutePath() 依赖平台
            // （Windows JVM 会把 /storage/... 改写成 C:\storage\...，判据就会失效）。
            if (dataPath == null || dataPath.indexOf(DLL_PATH_MARKER) < 0) {
                return null;
            }
            File d = new File(dataPath).getParentFile();
            return d;
        }
    }

    /** 解析 snapshot；格式不符直接抛（调用方回退硬编码 GUID，绝不带病改写）。包级可见 = 供离线自测。 */
    static List<Record> parseSnapshot(File snapshot) throws IOException {
        byte[] all = readAll(snapshot);
        Cursor0 c = new Cursor0(all);
        int magic = c.readInt();
        if (magic != SNAPSHOT_MAGIC) {
            throw new IOException("magic mismatch: " + magic);
        }
        int count = c.readInt();
        if (count < 0 || count > MAX_RECORDS) {
            throw new IOException("count out of range: " + count);
        }
        List<Record> list = new ArrayList<Record>(count);
        for (int i = 0; i < count; i++) {
            Record r = new Record();
            r.key = c.readCsString();
            r.infoPath = c.readCsString();
            r.dataPath = c.readCsString();
            r.crc = c.readCsString();
            r.size = c.readLong();
            list.add(r);
        }
        if (!c.atEnd()) {
            throw new IOException("trailing " + (all.length - c.pos) + " bytes");
        }
        return list;
    }

    /** 整份重写 snapshot（先 .tmp 再 rename）。未修改的字段按同样编码回写 ⇒ 字节等价。包级可见 = 供离线自测。 */
    static void writeSnapshot(File snapshot, List<Record> records) throws IOException {
        File mfDir = snapshot.getParentFile();
        if (mfDir != null && !mfDir.exists() && !mfDir.mkdirs()) {
            throw new IOException("mkdir failed: " + mfDir);
        }
        ByteArrayOutputStream body = new ByteArrayOutputStream(256);
        writeIntLE(body, SNAPSHOT_MAGIC);
        writeIntLE(body, records.size());
        for (Record r : records) {
            writeCsString(body, r.key == null ? "" : r.key);
            writeCsString(body, r.infoPath == null ? "" : r.infoPath);
            writeCsString(body, r.dataPath == null ? "" : r.dataPath);
            writeCsString(body, r.crc == null ? "" : r.crc);
            writeLongLE(body, r.size);
        }

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
    }

    /** 小端 + 7-bit 长度串的读游标（只在解析 snapshot 时用）。 */
    private static final class Cursor0 {
        final byte[] buf;
        int pos;

        Cursor0(byte[] buf) {
            this.buf = buf;
        }

        boolean atEnd() {
            return pos == buf.length;
        }

        int readInt() throws IOException {
            if (pos + 4 > buf.length) {
                throw new IOException("eof int @" + pos);
            }
            int v = (buf[pos] & 0xFF) | ((buf[pos + 1] & 0xFF) << 8)
                    | ((buf[pos + 2] & 0xFF) << 16) | ((buf[pos + 3] & 0xFF) << 24);
            pos += 4;
            return v;
        }

        long readLong() throws IOException {
            if (pos + 8 > buf.length) {
                throw new IOException("eof long @" + pos);
            }
            long v = 0;
            for (int i = 0; i < 8; i++) {
                v |= ((long) (buf[pos + i] & 0xFF)) << (8 * i);
            }
            pos += 8;
            return v;
        }

        /** C# BinaryReader.ReadString：7-bit 长度前缀 + UTF-8。 */
        String readCsString() throws IOException {
            int len = 0;
            int shift = 0;
            while (true) {
                if (pos >= buf.length) {
                    throw new IOException("eof string len @" + pos);
                }
                int b = buf[pos++] & 0xFF;
                len |= (b & 0x7F) << shift;
                if ((b & 0x80) == 0) {
                    break;
                }
                shift += 7;
                if (shift > 28) {
                    throw new IOException("bad 7-bit len");
                }
            }
            if (len < 0 || pos + len > buf.length) {
                throw new IOException("eof string body @" + pos + " len=" + len);
            }
            String s = new String(buf, pos, len, "UTF-8");
            pos += len;
            return s;
        }
    }

    private static byte[] readAll(File f) throws IOException {
        long len = f.length();
        if (len <= 0 || len > (4 << 20)) {
            throw new IOException("snapshot size out of range: " + len);
        }
        byte[] out = new byte[(int) len];
        FileInputStream in = new FileInputStream(f);
        try {
            int off = 0;
            while (off < out.length) {
                int n = in.read(out, off, out.length - off);
                if (n <= 0) {
                    throw new IOException("short read @" + off);
                }
                off += n;
            }
        } finally {
            in.close();
        }
        return out;
    }

    /**
     * 先写 .tmp 再 rename，避免游戏读到半写文件。
     * （语义照抄旧助手 Boot.copyAsset，见 2026-10-19 归档）
     */
    private static void copyAsset(Context ctx, String asset, File dst) throws IOException {
        InputStream in = null;
        try {
            in = ctx.getAssets().open(asset);
            // 2026-10-28: always refresh the payload. The device may re-download the
            // official bundle over ours; re-copying on every launch self-heals that.
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

    /** 供离线自测用：把 __info 的字节解成 CRC 串（不依赖 Context）。 */
    static String crcFromInfoBytes(byte[] info) {
        if (info == null || info.length < 3) {
            return null;
        }
        int len = (info[0] & 0xFF) | ((info[1] & 0xFF) << 8);
        if (len <= 0 || len > 64 || info.length < 2 + len) {
            return null;
        }
        try {
            return new String(info, 2, len, "US-ASCII");
        } catch (IOException e) {
            return null;
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
