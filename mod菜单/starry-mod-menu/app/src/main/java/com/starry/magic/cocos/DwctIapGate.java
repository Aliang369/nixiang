package com.starry.magic.cocos;

import android.content.Context;
import android.util.Log;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.zip.GZIPInputStream;
import java.util.zip.GZIPOutputStream;

/**
 * 动物餐厅免内购：把 {@code patch_dwct_iap_gate.js} 打进热更
 * {@code files/restaurant-hot-update/.../Script/index.jsc}。
 * 菜单只负责 {@code window.__starryNoIap}；不要靠 require wrap。
 */
public final class DwctIapGate {
    private static final String TAG = "DwctIapGate";
    private static final String PKG = "inland.twgame.restaurant";
    private static final String KEY = "431FEWJOTe-gecTW";
    private static final String REL_JSC = "assets/Script/index.jsc";
    private static final AtomicBoolean BUSY = new AtomicBoolean(false);
    private static volatile long lastMtime = -1;
    private static volatile long lastLen = -1;
    private static volatile boolean lastGated = false;

    private static final String GATE2 =
            "if(\"undefined\"!=typeof window&&window.__starryNoIap)return Promise.resolve({status:0,orderItem:{orderId:\"fakepay\",payType:arguments[0],pay_situation:arguments[1]}});";
    private static final String GATE1 =
            "if(\"undefined\"!=typeof window&&window.__starryNoIap)return Promise.resolve({status:0,orderItem:{orderId:\"fakepay\",payType:arguments[0]}});";
    private static final String GATE_COS =
            "if(\"undefined\"!=typeof window&&window.__starryNoIap)return Promise.resolve(!0);";
    private static final String[] TWO_ARG = {
            "t.prototype.androidWxPay = function(t, i) {",
            "t.prototype.wxMiNiGamePay = function(e, t) {",
            "t.prototype.wxIOSPay = function(e, t) {",
            "t.prototype.huaweiPMSPay = function(e, t) {",
            "t.prototype.googlePay = function(e, t) {",
            "t.prototype.globalIosPay = function(e, t) {",
            "t.prototype.inlandIosPay = function(e, t) {",
    };
    private static final String[] ONE_ARG = {
            "t.prototype.sdkboxPay = function(e) {",
            "t.prototype.qqMiNiPay = function(e) {",
    };

    private DwctIapGate() {}

    /** C++ Tick / TryInit：热更目录后出现时再打门。 */
    public static void applyFromNative() {
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof Context) apply((Context) app);
        } catch (Throwable t) {
            Log.w(TAG, "applyFromNative: " + t.getMessage());
        }
    }

    public static void apply(Context ctx) {
        if (ctx == null) return;
        try {
            if (!PKG.equals(ctx.getPackageName())) return;
        } catch (Throwable ignored) {
            return;
        }
        File hotRoot = new File(ctx.getFilesDir(), "restaurant-hot-update");
        if (hotRoot.isDirectory()) patchDir(hotRoot);
    }

    static void patchDir(File hotRoot) {
        if (hotRoot == null || !hotRoot.isDirectory()) return;
        if (!BUSY.compareAndSet(false, true)) return;
        try {
            File jsc = new File(hotRoot, REL_JSC);
            if (!jsc.isFile()) {
                jsc = new File(hotRoot, "Script/index.jsc");
            }
            if (!jsc.isFile()) return;
            long m = jsc.lastModified();
            long len = jsc.length();
            if (lastGated && m == lastMtime && len == lastLen) return;
            byte[] raw = readAll(jsc);
            String js;
            try {
                js = decryptJs(raw);
            } catch (Throwable t) {
                Log.w(TAG, "decrypt fail: " + t.getMessage());
                return;
            }
            int before = countGates(js);
            if (before >= 10) {
                lastMtime = m;
                lastLen = len;
                lastGated = true;
                Log.i(TAG, "already gated gates=" + before);
                return;
            }
            String patched = insertGates(js);
            int after = countGates(patched);
            if (after < 10) {
                Log.e(TAG, "gate insert incomplete before=" + before + " after=" + after);
                return;
            }
            byte[] out = encryptJs(patched);
            File tmp = new File(jsc.getAbsolutePath() + ".starry_tmp");
            writeAll(tmp, out);
            if (!tmp.renameTo(jsc)) {
                writeAll(jsc, out);
                //noinspection ResultOfMethodCallIgnored
                tmp.delete();
            }
            updateManifest(new File(hotRoot, "project.manifest"), out.length, md5Hex(out));
            updateManifest(new File(hotRoot, "version.manifest"), out.length, md5Hex(out));
            lastMtime = jsc.lastModified();
            lastLen = jsc.length();
            lastGated = true;
            Log.i(TAG, "patched " + jsc.getAbsolutePath() + " gates=" + after + " size=" + out.length);
        } catch (Throwable t) {
            Log.e(TAG, "patch failed", t);
        } finally {
            BUSY.set(false);
        }
    }

    private static String insertGates(String js) {
        for (String m : TWO_ARG) js = insertAfterOpenBrace(js, m, GATE2);
        for (String m : ONE_ARG) js = insertAfterOpenBrace(js, m, GATE1);
        js = insertAfterOpenBrace(js, "t.prototype.changeOrderStatus = function(e) {", GATE_COS);
        return js;
    }

    private static String insertAfterOpenBrace(String src, String marker, String prelude) {
        int idx = src.indexOf(marker);
        if (idx < 0) throw new IllegalStateException("marker not found: " + marker);
        int open = src.indexOf('{', idx);
        if (open < 0) throw new IllegalStateException("no { after " + marker);
        String peek = src.substring(open, Math.min(src.length(), open + 120));
        if (peek.contains("__starryNoIap")) return src;
        return src.substring(0, open + 1) + "\n" + prelude + "\n" + src.substring(open + 1);
    }

    private static int countGates(String js) {
        int n = 0, from = 0;
        while (true) {
            int i = js.indexOf("__starryNoIap", from);
            if (i < 0) return n;
            n++;
            from = i + 13;
        }
    }

    private static void updateManifest(File mf, int size, String md5) {
        if (mf == null || !mf.isFile()) return;
        try {
            String s = new String(readAll(mf), StandardCharsets.UTF_8);
            int key = s.indexOf("\"assets/Script/index.jsc\"");
            if (key < 0) key = s.indexOf("assets/Script/index.jsc");
            if (key < 0) return;
            int brace = s.indexOf('{', key);
            if (brace < 0) return;
            int end = s.indexOf('}', brace);
            if (end < 0 || end - brace > 400) return;
            String block = s.substring(brace, end + 1);
            String nblock = block
                    .replaceAll("\"md5\"\\s*:\\s*\"[^\"]+\"", "\"md5\":\"" + md5 + "\"")
                    .replaceAll("\"size\"\\s*:\\s*\\d+", "\"size\":" + size);
            if (block.equals(nblock)) return;
            String out = s.substring(0, brace) + nblock + s.substring(end + 1);
            writeAll(mf, out.getBytes(StandardCharsets.UTF_8));
            Log.i(TAG, "manifest " + mf.getName() + " md5=" + md5 + " size=" + size);
        } catch (Throwable t) {
            Log.w(TAG, "manifest skip " + mf.getName() + ": " + t.getMessage());
        }
    }

    private static String decryptJs(byte[] enc) throws Exception {
        byte[] gz = Xxtea.decrypt(enc, KEY.getBytes(StandardCharsets.UTF_8));
        if (gz == null || gz.length == 0) throw new IllegalStateException("xxtea empty");
        return new String(gunzip(gz), StandardCharsets.UTF_8);
    }

    private static byte[] encryptJs(String js) throws Exception {
        byte[] gz = gzip(js.getBytes(StandardCharsets.UTF_8));
        return Xxtea.encrypt(gz, KEY.getBytes(StandardCharsets.UTF_8));
    }

    private static byte[] gzip(byte[] plain) throws Exception {
        ByteArrayOutputStream bos = new ByteArrayOutputStream();
        GZIPOutputStream gos = new GZIPOutputStream(bos);
        gos.write(plain);
        gos.close();
        return bos.toByteArray();
    }

    private static byte[] gunzip(byte[] gz) throws Exception {
        GZIPInputStream gis = new GZIPInputStream(new ByteArrayInputStream(gz));
        ByteArrayOutputStream bos = new ByteArrayOutputStream();
        byte[] buf = new byte[65536];
        int n;
        while ((n = gis.read(buf)) >= 0) bos.write(buf, 0, n);
        gis.close();
        return bos.toByteArray();
    }

    private static byte[] readAll(File f) throws Exception {
        FileInputStream in = new FileInputStream(f);
        ByteArrayOutputStream bos = new ByteArrayOutputStream((int) Math.max(16, f.length()));
        byte[] buf = new byte[65536];
        int n;
        while ((n = in.read(buf)) >= 0) bos.write(buf, 0, n);
        in.close();
        return bos.toByteArray();
    }

    private static void writeAll(File f, byte[] data) throws Exception {
        FileOutputStream out = new FileOutputStream(f);
        out.write(data);
        out.flush();
        out.close();
    }

    private static String md5Hex(byte[] data) throws Exception {
        byte[] d = MessageDigest.getInstance("MD5").digest(data);
        StringBuilder sb = new StringBuilder(32);
        for (byte b : d) sb.append(String.format("%02x", b & 0xff));
        return sb.toString();
    }

    /** xxtea-node：encrypt 带长度，decrypt 去长度。 */
    static final class Xxtea {
        private static final int DELTA = 0x9E3779B9;

        static byte[] encrypt(byte[] data, byte[] key) {
            if (data == null || data.length == 0) return data;
            int[] v = toUint32(data, true);
            int[] k = toUint32(fixk(key), false);
            encryptArr(v, k);
            return toBytes(v, false);
        }

        static byte[] decrypt(byte[] data, byte[] key) {
            if (data == null || data.length == 0) return data;
            int[] v = toUint32(data, false);
            int[] k = toUint32(fixk(key), false);
            decryptArr(v, k);
            return toBytes(v, true);
        }

        private static byte[] fixk(byte[] k) {
            if (k.length >= 16) return k;
            byte[] n = new byte[16];
            System.arraycopy(k, 0, n, 0, k.length);
            return n;
        }

        private static int mx(int sum, int y, int z, int p, int e, int[] k) {
            return ((z >>> 5 ^ y << 2) + (y >>> 3 ^ z << 4))
                    ^ ((sum ^ y) + (k[(p & 3) ^ e] ^ z));
        }

        private static void encryptArr(int[] v, int[] k) {
            int n = v.length - 1;
            int z = v[n];
            long sum = 0;
            int q = 6 + 52 / v.length;
            while (q-- > 0) {
                sum += DELTA & 0xffffffffL;
                int e = ((int) sum >>> 2) & 3;
                int p;
                for (p = 0; p < n; p++) {
                    int y = v[p + 1];
                    z = v[p] += mx((int) sum, y, z, p, e, k);
                }
                int y = v[0];
                z = v[n] += mx((int) sum, y, z, p, e, k);
            }
        }

        private static void decryptArr(int[] v, int[] k) {
            int n = v.length - 1;
            int y = v[0];
            int q = 6 + 52 / v.length;
            long sum = (q & 0xffffffffL) * (DELTA & 0xffffffffL);
            while (sum != 0) {
                int e = ((int) sum >>> 2) & 3;
                for (int p = n; p > 0; p--) {
                    int z = v[p - 1];
                    y = v[p] -= mx((int) sum, y, z, p, e, k);
                }
                int z = v[n];
                y = v[0] -= mx((int) sum, y, z, 0, e, k);
                sum -= DELTA & 0xffffffffL;
            }
        }

        private static int[] toUint32(byte[] bytes, boolean includeLength) {
            int n = bytes.length >> 2;
            if ((bytes.length & 3) != 0) n++;
            int[] v = includeLength ? new int[n + 1] : new int[n];
            for (int i = 0; i < bytes.length; i++) {
                v[i >> 2] |= (bytes[i] & 0xff) << ((i & 3) << 3);
            }
            if (includeLength) v[n] = bytes.length;
            return v;
        }

        private static byte[] toBytes(int[] v, boolean includeLength) {
            int n = v.length << 2;
            if (includeLength) {
                int m = v[v.length - 1];
                n -= 4;
                if (m < n - 3 || m > n) return null;
                n = m;
            }
            byte[] bytes = new byte[n];
            for (int i = 0; i < n; i++) {
                bytes[i] = (byte) (v[i >> 2] >>> ((i & 3) << 3));
            }
            return bytes;
        }
    }
}
