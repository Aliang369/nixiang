package com.starry.bootcache;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.database.Cursor;
import android.net.Uri;

/**
 * 启动缓存预置: 进程启动最早阶段(ContentProvider onCreate 早于 Application/Unity)
 * 将 APK 内置的补丁容器复制到 YooAsset 沙箱缓存目录, 使其按缓存命中加载。
 * 目录: <externalFilesDir>/yoo/DefaultPackage/CacheRawFiles/<md5前2>/<md5>/__data.rawfile + __info
 * 全程静默, 任何异常不影响启动。
 */
public class StarryBootCache extends ContentProvider {
    static final String GUID = "a573b3a8f4724464c4fa01d9bc9d0827";
    static final String CRC_HEX = "1bbe1b8d";
    static final int SIZE = 7186452;

    public boolean onCreate() {
        try {
            android.content.Context c = getContext();
            if (c == null) return true;
            java.io.File ext = c.getExternalFilesDir(null);
            if (ext == null) return true;
            java.io.File dir = new java.io.File(ext,
                    "yoo/DefaultPackage/CacheRawFiles/" + GUID.substring(0, 2) + "/" + GUID);
            java.io.File data = new java.io.File(dir, "__data.rawfile");
            if (data.length() != SIZE) {
                dir.mkdirs();
                java.io.InputStream in = c.getAssets().open("yoo/DefaultPackage/" + GUID + ".rawfile");
                java.io.FileOutputStream out = new java.io.FileOutputStream(data);
                byte[] buf = new byte[65536];
                int n;
                while ((n = in.read(buf)) > 0) out.write(buf, 0, n);
                out.flush();
                out.close();
                in.close();
            }
            java.io.File info = new java.io.File(dir, "__info");
            if (info.length() != 18) {
                byte[] b = new byte[18];
                b[0] = 8;
                b[1] = 0;
                byte[] crc = CRC_HEX.getBytes("UTF-8");
                System.arraycopy(crc, 0, b, 2, 8);
                long s = SIZE;
                for (int i = 0; i < 8; i++) b[10 + i] = (byte) ((s >> (8 * i)) & 0xFF);
                java.io.FileOutputStream o2 = new java.io.FileOutputStream(info);
                o2.write(b);
                o2.flush();
                o2.close();
            }
        } catch (Throwable t) {
            // 静默: 任何失败回退到游戏原生流程
        }
        return true;
    }

    public Cursor query(Uri uri, String[] p, String s, String[] a, String o) { return null; }
    public String getType(Uri uri) { return null; }
    public Uri insert(Uri uri, ContentValues v) { return null; }
    public int delete(Uri uri, String s, String[] a) { return 0; }
    public int update(Uri uri, ContentValues v, String s, String[] a) { return 0; }
}
