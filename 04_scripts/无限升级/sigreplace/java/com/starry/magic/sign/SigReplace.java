package com.starry.magic.sign;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.Signature;
import android.os.Parcel;
import android.os.Parcelable;
import android.util.Log;

import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.Map;

/**
 * 运行时签名替换（2026-11-04，无限升级 11.266018 专用）
 *
 * 双轨制「签名轨」：包体签名仍是自签，运行时把 PackageInfo 的签名换成**官方证书**，
 * 让 TapSDK 的包名/签名校验通过。形态 = 包装 PackageInfo.CREATOR（Unsafe 改静态字段）
 * + 清 Parcel / ApplicationPackageManager 缓存；**不使用隐藏 API 豁免**。
 * 入口：StarryInitProvider.onCreate 最早期 invoke-static（注入面不变，不碰 Application）。
 */
public final class SigReplace {
    private static final String TAG = "StarrySigReplace";
    private static final byte[] OFFICIAL = {
		(byte)0x30, (byte)0x82, (byte)0x03, (byte)0x4e, (byte)0x30, (byte)0x82, (byte)0x02, (byte)0x36, (byte)0xa0, (byte)0x03, (byte)0x02, (byte)0x01,
		(byte)0x02, (byte)0x02, (byte)0x04, (byte)0x4a, (byte)0xa0, (byte)0x33, (byte)0xdc, (byte)0x30, (byte)0x0d, (byte)0x06, (byte)0x09, (byte)0x2a,
		(byte)0x86, (byte)0x48, (byte)0x86, (byte)0xf7, (byte)0x0d, (byte)0x01, (byte)0x01, (byte)0x05, (byte)0x05, (byte)0x00, (byte)0x30, (byte)0x68,
		(byte)0x31, (byte)0x0c, (byte)0x30, (byte)0x0a, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x03, (byte)0x0c, (byte)0x03, (byte)0x43,
		(byte)0x75, (byte)0x69, (byte)0x31, (byte)0x10, (byte)0x30, (byte)0x0e, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x0b, (byte)0x0c,
		(byte)0x07, (byte)0x4a, (byte)0x69, (byte)0x61, (byte)0x6e, (byte)0x69, (byte)0x6e, (byte)0x67, (byte)0x31, (byte)0x15, (byte)0x30, (byte)0x13,
		(byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x0a, (byte)0x0c, (byte)0x0c, (byte)0xe6, (byte)0x98, (byte)0x9f, (byte)0xe5, (byte)0xba,
		(byte)0xa7, (byte)0xe5, (byte)0xaf, (byte)0xbc, (byte)0xe5, (byte)0xb8, (byte)0x88, (byte)0x31, (byte)0x10, (byte)0x30, (byte)0x0e, (byte)0x06,
		(byte)0x03, (byte)0x55, (byte)0x04, (byte)0x07, (byte)0x0c, (byte)0x07, (byte)0x4e, (byte)0x61, (byte)0x6e, (byte)0x6e, (byte)0x69, (byte)0x6e,
		(byte)0x67, (byte)0x31, (byte)0x10, (byte)0x30, (byte)0x0e, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x08, (byte)0x0c, (byte)0x07,
		(byte)0x47, (byte)0x75, (byte)0x61, (byte)0x6e, (byte)0x67, (byte)0x58, (byte)0x69, (byte)0x31, (byte)0x0b, (byte)0x30, (byte)0x09, (byte)0x06,
		(byte)0x03, (byte)0x55, (byte)0x04, (byte)0x06, (byte)0x13, (byte)0x02, (byte)0x43, (byte)0x4e, (byte)0x30, (byte)0x20, (byte)0x17, (byte)0x0d,
		(byte)0x32, (byte)0x32, (byte)0x31, (byte)0x30, (byte)0x30, (byte)0x39, (byte)0x31, (byte)0x31, (byte)0x31, (byte)0x31, (byte)0x35, (byte)0x30,
		(byte)0x5a, (byte)0x18, (byte)0x0f, (byte)0x32, (byte)0x30, (byte)0x37, (byte)0x32, (byte)0x30, (byte)0x39, (byte)0x32, (byte)0x36, (byte)0x31,
		(byte)0x31, (byte)0x31, (byte)0x31, (byte)0x35, (byte)0x30, (byte)0x5a, (byte)0x30, (byte)0x68, (byte)0x31, (byte)0x0c, (byte)0x30, (byte)0x0a,
		(byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x03, (byte)0x0c, (byte)0x03, (byte)0x43, (byte)0x75, (byte)0x69, (byte)0x31, (byte)0x10,
		(byte)0x30, (byte)0x0e, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x0b, (byte)0x0c, (byte)0x07, (byte)0x4a, (byte)0x69, (byte)0x61,
		(byte)0x6e, (byte)0x69, (byte)0x6e, (byte)0x67, (byte)0x31, (byte)0x15, (byte)0x30, (byte)0x13, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04,
		(byte)0x0a, (byte)0x0c, (byte)0x0c, (byte)0xe6, (byte)0x98, (byte)0x9f, (byte)0xe5, (byte)0xba, (byte)0xa7, (byte)0xe5, (byte)0xaf, (byte)0xbc,
		(byte)0xe5, (byte)0xb8, (byte)0x88, (byte)0x31, (byte)0x10, (byte)0x30, (byte)0x0e, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x07,
		(byte)0x0c, (byte)0x07, (byte)0x4e, (byte)0x61, (byte)0x6e, (byte)0x6e, (byte)0x69, (byte)0x6e, (byte)0x67, (byte)0x31, (byte)0x10, (byte)0x30,
		(byte)0x0e, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x08, (byte)0x0c, (byte)0x07, (byte)0x47, (byte)0x75, (byte)0x61, (byte)0x6e,
		(byte)0x67, (byte)0x58, (byte)0x69, (byte)0x31, (byte)0x0b, (byte)0x30, (byte)0x09, (byte)0x06, (byte)0x03, (byte)0x55, (byte)0x04, (byte)0x06,
		(byte)0x13, (byte)0x02, (byte)0x43, (byte)0x4e, (byte)0x30, (byte)0x82, (byte)0x01, (byte)0x22, (byte)0x30, (byte)0x0d, (byte)0x06, (byte)0x09,
		(byte)0x2a, (byte)0x86, (byte)0x48, (byte)0x86, (byte)0xf7, (byte)0x0d, (byte)0x01, (byte)0x01, (byte)0x01, (byte)0x05, (byte)0x00, (byte)0x03,
		(byte)0x82, (byte)0x01, (byte)0x0f, (byte)0x00, (byte)0x30, (byte)0x82, (byte)0x01, (byte)0x0a, (byte)0x02, (byte)0x82, (byte)0x01, (byte)0x01,
		(byte)0x00, (byte)0x91, (byte)0x27, (byte)0xc7, (byte)0xd8, (byte)0xec, (byte)0xbd, (byte)0x17, (byte)0x2f, (byte)0x3d, (byte)0x42, (byte)0x14,
		(byte)0x32, (byte)0x76, (byte)0x0b, (byte)0x47, (byte)0x3d, (byte)0x22, (byte)0x39, (byte)0x03, (byte)0x67, (byte)0x42, (byte)0x0d, (byte)0xef,
		(byte)0xac, (byte)0x7c, (byte)0xde, (byte)0x5b, (byte)0xb9, (byte)0xe9, (byte)0x1c, (byte)0xd1, (byte)0xd8, (byte)0xb1, (byte)0x9a, (byte)0xd4,
		(byte)0xc7, (byte)0x82, (byte)0x3b, (byte)0x52, (byte)0x01, (byte)0x8d, (byte)0x3a, (byte)0x9a, (byte)0x42, (byte)0x20, (byte)0xcc, (byte)0xb7,
		(byte)0x72, (byte)0xee, (byte)0xaf, (byte)0xfc, (byte)0xa3, (byte)0x8a, (byte)0xd5, (byte)0xf7, (byte)0x71, (byte)0xae, (byte)0xd4, (byte)0xf3,
		(byte)0x35, (byte)0x1a, (byte)0x38, (byte)0x75, (byte)0xe1, (byte)0xe5, (byte)0x70, (byte)0xc3, (byte)0xd2, (byte)0xd3, (byte)0x75, (byte)0x15,
		(byte)0xd3, (byte)0xcc, (byte)0xa9, (byte)0x19, (byte)0x98, (byte)0xa0, (byte)0x46, (byte)0x1f, (byte)0x0e, (byte)0x13, (byte)0x07, (byte)0x32,
		(byte)0x90, (byte)0x32, (byte)0x31, (byte)0xdb, (byte)0x92, (byte)0x81, (byte)0x5d, (byte)0x39, (byte)0x7c, (byte)0x8e, (byte)0x13, (byte)0xbc,
		(byte)0xa1, (byte)0xaf, (byte)0x74, (byte)0x1b, (byte)0x79, (byte)0xff, (byte)0xef, (byte)0x60, (byte)0x63, (byte)0x63, (byte)0xcc, (byte)0xce,
		(byte)0x8e, (byte)0xcf, (byte)0x82, (byte)0xb6, (byte)0x72, (byte)0xae, (byte)0xe7, (byte)0x8a, (byte)0x37, (byte)0x02, (byte)0xcb, (byte)0x1c,
		(byte)0x06, (byte)0xab, (byte)0x3f, (byte)0xe1, (byte)0x56, (byte)0x95, (byte)0xf6, (byte)0xfb, (byte)0x3d, (byte)0x08, (byte)0x7a, (byte)0x9a,
		(byte)0x61, (byte)0x15, (byte)0x93, (byte)0xd5, (byte)0xdd, (byte)0xc3, (byte)0x29, (byte)0x6c, (byte)0xbd, (byte)0x08, (byte)0xd7, (byte)0x71,
		(byte)0x9a, (byte)0x98, (byte)0x1b, (byte)0x46, (byte)0x50, (byte)0x11, (byte)0xee, (byte)0xc2, (byte)0x95, (byte)0x08, (byte)0x85, (byte)0xed,
		(byte)0xbe, (byte)0x80, (byte)0x3e, (byte)0xb2, (byte)0x22, (byte)0xa4, (byte)0x5f, (byte)0xc6, (byte)0xfa, (byte)0xbb, (byte)0x5d, (byte)0x7e,
		(byte)0xcc, (byte)0x17, (byte)0x3f, (byte)0x44, (byte)0x21, (byte)0xf2, (byte)0xad, (byte)0xd1, (byte)0xc2, (byte)0x29, (byte)0xac, (byte)0x61,
		(byte)0xda, (byte)0x6f, (byte)0xc5, (byte)0x9d, (byte)0xbd, (byte)0x3d, (byte)0x12, (byte)0x75, (byte)0xfb, (byte)0x44, (byte)0x20, (byte)0xc6,
		(byte)0x41, (byte)0x00, (byte)0x83, (byte)0x99, (byte)0x12, (byte)0x6b, (byte)0xd9, (byte)0x3a, (byte)0x62, (byte)0xbc, (byte)0x95, (byte)0xb1,
		(byte)0x62, (byte)0xb1, (byte)0x77, (byte)0x98, (byte)0x84, (byte)0x9d, (byte)0x38, (byte)0xf8, (byte)0xfe, (byte)0x9e, (byte)0x4e, (byte)0xa0,
		(byte)0xf4, (byte)0xe2, (byte)0xe5, (byte)0xee, (byte)0x3e, (byte)0xc1, (byte)0xd0, (byte)0xfa, (byte)0xcd, (byte)0x18, (byte)0x8f, (byte)0x50,
		(byte)0x5d, (byte)0x93, (byte)0x35, (byte)0x8f, (byte)0xbd, (byte)0xd7, (byte)0xc1, (byte)0xae, (byte)0x30, (byte)0xaa, (byte)0x22, (byte)0x3e,
		(byte)0xe4, (byte)0x6f, (byte)0xe2, (byte)0xb0, (byte)0xc2, (byte)0x49, (byte)0x84, (byte)0xb2, (byte)0xb1, (byte)0xa9, (byte)0x4d, (byte)0x5a,
		(byte)0x47, (byte)0x6c, (byte)0x23, (byte)0xc0, (byte)0x49, (byte)0x02, (byte)0x03, (byte)0x01, (byte)0x00, (byte)0x01, (byte)0x30, (byte)0x0d,
		(byte)0x06, (byte)0x09, (byte)0x2a, (byte)0x86, (byte)0x48, (byte)0x86, (byte)0xf7, (byte)0x0d, (byte)0x01, (byte)0x01, (byte)0x05, (byte)0x05,
		(byte)0x00, (byte)0x03, (byte)0x82, (byte)0x01, (byte)0x01, (byte)0x00, (byte)0x87, (byte)0x1a, (byte)0x42, (byte)0xa7, (byte)0xf0, (byte)0x23,
		(byte)0x7f, (byte)0x4e, (byte)0xf0, (byte)0xa0, (byte)0xa6, (byte)0x0b, (byte)0x4e, (byte)0xa9, (byte)0x6a, (byte)0x6c, (byte)0x36, (byte)0xb2,
		(byte)0x4c, (byte)0xbe, (byte)0xc5, (byte)0x0f, (byte)0x25, (byte)0x78, (byte)0x88, (byte)0x02, (byte)0xcf, (byte)0xd1, (byte)0xf3, (byte)0xfe,
		(byte)0x55, (byte)0xb6, (byte)0x50, (byte)0x6e, (byte)0x2f, (byte)0x74, (byte)0x80, (byte)0x0f, (byte)0xf3, (byte)0x2e, (byte)0x32, (byte)0xbb,
		(byte)0x5f, (byte)0xa3, (byte)0x15, (byte)0x52, (byte)0x29, (byte)0x86, (byte)0xb8, (byte)0xae, (byte)0x27, (byte)0x0c, (byte)0x39, (byte)0xa4,
		(byte)0x49, (byte)0xe1, (byte)0xe1, (byte)0x25, (byte)0xce, (byte)0x0e, (byte)0x23, (byte)0xf9, (byte)0x1d, (byte)0xbe, (byte)0xd3, (byte)0x47,
		(byte)0xe7, (byte)0x3e, (byte)0x66, (byte)0x2e, (byte)0x23, (byte)0xf4, (byte)0x7a, (byte)0x93, (byte)0x82, (byte)0x9c, (byte)0x33, (byte)0x2e,
		(byte)0xd9, (byte)0x56, (byte)0xdd, (byte)0xa7, (byte)0x43, (byte)0x04, (byte)0x9b, (byte)0x53, (byte)0xcc, (byte)0x14, (byte)0xc7, (byte)0xef,
		(byte)0xa1, (byte)0x88, (byte)0xaa, (byte)0xbb, (byte)0xbd, (byte)0x40, (byte)0xc8, (byte)0x32, (byte)0x4b, (byte)0x29, (byte)0xe3, (byte)0x23,
		(byte)0xca, (byte)0x10, (byte)0xa8, (byte)0x49, (byte)0x28, (byte)0x88, (byte)0x5b, (byte)0x4f, (byte)0x9a, (byte)0x8a, (byte)0x21, (byte)0x4f,
		(byte)0x6a, (byte)0xbd, (byte)0x3a, (byte)0xf0, (byte)0xd5, (byte)0xa5, (byte)0x8d, (byte)0xde, (byte)0x9f, (byte)0xc4, (byte)0x26, (byte)0xa7,
		(byte)0x82, (byte)0x6d, (byte)0x94, (byte)0x08, (byte)0xb3, (byte)0xdf, (byte)0xfc, (byte)0x49, (byte)0x4f, (byte)0x81, (byte)0x17, (byte)0xfa,
		(byte)0x1b, (byte)0x20, (byte)0xd6, (byte)0xdb, (byte)0x9b, (byte)0x8d, (byte)0x09, (byte)0x9f, (byte)0xe1, (byte)0xdb, (byte)0x0a, (byte)0xc2,
		(byte)0xf4, (byte)0xbe, (byte)0x46, (byte)0x45, (byte)0x8a, (byte)0xa2, (byte)0xee, (byte)0x72, (byte)0x82, (byte)0x9c, (byte)0x8e, (byte)0xee,
		(byte)0xe0, (byte)0x3e, (byte)0x20, (byte)0xaa, (byte)0xb2, (byte)0x00, (byte)0x20, (byte)0xd3, (byte)0xb1, (byte)0x89, (byte)0xf3, (byte)0x85,
		(byte)0xa8, (byte)0x52, (byte)0x36, (byte)0x30, (byte)0x45, (byte)0x34, (byte)0x53, (byte)0x90, (byte)0x3d, (byte)0x12, (byte)0x47, (byte)0x01,
		(byte)0xbb, (byte)0x29, (byte)0xff, (byte)0x56, (byte)0x11, (byte)0x0f, (byte)0xc2, (byte)0xdd, (byte)0x57, (byte)0x53, (byte)0x51, (byte)0x91,
		(byte)0x7d, (byte)0xf2, (byte)0xf7, (byte)0xa7, (byte)0x17, (byte)0xe0, (byte)0x39, (byte)0xd8, (byte)0xfb, (byte)0x70, (byte)0xb3, (byte)0x6d,
		(byte)0x52, (byte)0xf0, (byte)0x8f, (byte)0x1d, (byte)0x5f, (byte)0xd3, (byte)0x04, (byte)0xc8, (byte)0xd0, (byte)0xe5, (byte)0x97, (byte)0xdc,
		(byte)0x92, (byte)0x2c, (byte)0x08, (byte)0x1e, (byte)0x3e, (byte)0x1c, (byte)0x5f, (byte)0xfc, (byte)0x82, (byte)0x4a, (byte)0x52, (byte)0x87,
		(byte)0xa1, (byte)0x82, (byte)0xc5, (byte)0x24, (byte)0x94, (byte)0x2c, (byte)0x92, (byte)0xbe, (byte)0x6d, (byte)0x59, (byte)0x4f, (byte)0xbb,
		(byte)0xf7, (byte)0xc0, (byte)0x18, (byte)0xe5, (byte)0x92, (byte)0xbf, (byte)0x7b, (byte)0xce, (byte)0x86, (byte)0x6b,
    };
    private static volatile boolean sDone;
    private static volatile String sTarget;

    /** 由 so（Games/WxsjVc1637Mod.cpp）导出：JNI 直接写 static 字段，绕开 final（Android 的 Unsafe 没有 staticFieldOffset）。 */
    private static native boolean nativeSetStaticField(Class<?> declaring, java.lang.reflect.Field field, Object value);

    private SigReplace() {}

    public static void install(Context ctx) {
        if (sDone) return;
        sDone = true;
        try { sTarget = ctx.getPackageName(); } catch (Throwable ignored) { }
        if (sTarget == null) sTarget = "com.xingzuodaoshi.infinityUpgrade";
        StringBuilder r = new StringBuilder();
        try { installCreator(r, ctx); r.append("creator=ok "); } catch (Throwable t) { r.append("creator=FAIL(").append(t).append(") "); }
        Log.i(TAG, "runtime signature replace: der=" + OFFICIAL.length + "B " + r);
    }

    /** 取 Unsafe 实例：Android 上 sun.misc.Unsafe 存在但方法集因版本而异，故按名字枚举。 */
    private static Object unsafeInstance() {
        for (String cn : new String[] { "sun.misc.Unsafe", "jdk.internal.misc.Unsafe" }) {
            try {
                Class<?> c = Class.forName(cn);
                Field f = c.getDeclaredField("theUnsafe");
                f.setAccessible(true);
                return f.get(null);
            } catch (Throwable ignored) { }
        }
        return null;
    }

    private static Method findMethod(Class<?> c, String... names) {
        for (Method m : c.getDeclaredMethods()) {
            for (String n : names) {
                if (m.getName().equals(n)) {
                    try { m.setAccessible(true); return m; } catch (Throwable ignored) { }
                }
            }
        }
        return null;
    }

    private static void installCreator(StringBuilder r, final Context ctx) throws Exception {
        Object u = unsafeInstance();
        if (u == null) { r.append("noUnsafe "); return; }
        Class<?> uc = u.getClass();
        Field f = PackageInfo.class.getDeclaredField("CREATOR");
        Method mOff = findMethod(uc, "staticFieldOffset");
        Method mBase = findMethod(uc, "staticFieldBase");
        Method mGet = findMethod(uc, "getObject");
        Method mPut = findMethod(uc, "putObject");
        r.append("m{off=").append(mOff != null).append(",base=").append(mBase != null)
         .append(",get=").append(mGet != null).append(",put=").append(mPut != null).append("} ");
        if (mOff != null && mBase != null && mGet != null && mPut != null) {
            Object base = mBase.invoke(u, f);
            long off = ((Number) mOff.invoke(u, f)).longValue();
            Object orig = mGet.invoke(u, base, Long.valueOf(off));
            mPut.invoke(u, base, Long.valueOf(off), new Wrap((Parcelable.Creator<PackageInfo>) orig));
            r.append("viaUnsafe ");
            return;
        }
        // Android 的 sun.misc.Unsafe 没有 staticFieldOffset/staticFieldBase ⇒ 走 JNI（等 so 加载后执行）
        r.append("viaJni ");
        if (mGet == null || mPut == null) r.append("(unsafePartial) ");
        startJniHook(f, ctx);
    }

    /** 等 so 加载（System.loadLibrary 由 StarryInitProvider 随后触发），再用 JNI 写 static final 字段。 */
    private static void startJniHook(final Field creatorField, final Context ctx) {
        Thread t = new Thread(new Runnable() {
            public void run() {
                for (int i = 0; i < 200; i++) {           // 最多等 ~10s
                    try {
                        Object orig = creatorField.get(null);
                        boolean ok = nativeSetStaticField(PackageInfo.class, creatorField, new Wrap((Parcelable.Creator<PackageInfo>) orig));
                        Log.i(TAG, "jni static-field write: ok=" + ok + " wait=" + (i * 50) + "ms");
                        if (ok) {
                            try { clearCaches(); Log.i(TAG, "cache cleared after hook"); } catch (Throwable ignored) { }
                            primeCache(ctx);
                            selfTest(ctx);
                            return;
                        }
                    } catch (Throwable ignored) { }
                    try { Thread.sleep(50); } catch (Throwable ignored) { }
                }
                Log.e(TAG, "jni static-field write timed out (so not loaded?)");
            }
        }, "starry-sigreplace");
        t.setDaemon(true);
        t.start();
    }

    /** 关键：GET_SIGNATURES 那条读法走的是「已缓存对象」，不经过 Parcel;CREATOR。
     *  这里主动取一次并把对象上的 signatures 就地改写成官方值，让缓存里那份也变官方。 */
    private static void primeCache(Context ctx) {
        try {
            android.content.pm.PackageManager pm = ctx.getPackageManager();
            PackageInfo pi = pm.getPackageInfo(ctx.getPackageName(), 0x00000040);   // GET_SIGNATURES
            if (pi != null) apply(pi);
        } catch (Throwable t) { Log.e(TAG, "primeCache signatures failed", t); }
        try {
            android.content.pm.PackageManager pm = ctx.getPackageManager();
            PackageInfo pi = pm.getPackageInfo(ctx.getPackageName(), 0x08000000);   // GET_SIGNING_CERTIFICATES
            if (pi != null) apply(pi);
        } catch (Throwable t) { Log.e(TAG, "primeCache signing failed", t); }
    }

    /** 自检：hook 之后自己查一次 PackageInfo，把读到的签名 MD5 打出来（对不上就是这条读法没被覆盖）。 */
    private static void selfTest(Context ctx) {
        try {
            android.content.pm.PackageManager pm = ctx.getPackageManager();
            PackageInfo pi = pm.getPackageInfo(ctx.getPackageName(), 0x00000040);
            logSig("GET_SIGNATURES", pi);
        } catch (Throwable t) { Log.e(TAG, "selfTest signatures failed", t); }
        try {
            android.content.pm.PackageManager pm = ctx.getPackageManager();
            PackageInfo pi = pm.getPackageInfo(ctx.getPackageName(), 0x08000000);
            logSig("GET_SIGNING_CERTIFICATES", pi);
        } catch (Throwable t) { Log.e(TAG, "selfTest signing failed", t); }
    }

    private static void logSig(String how, PackageInfo pi) {
        String md5 = "null";
        try {
            Signature[] s = pi.signatures;
            if (s != null && s.length > 0) md5 = md5(s[0].toByteArray());
        } catch (Throwable ignored) { }
        String sigInfo = "n/a";
        try {
            Field fi = PackageInfo.class.getField("signingInfo");
            fi.setAccessible(true);
            Object info = fi.get(pi);
            sigInfo = (info == null) ? "null" : "obj";
            if (info != null) {
                java.lang.reflect.Method m = info.getClass().getMethod("getApkContentsSigners");
                Signature[] arr = (Signature[]) m.invoke(info);
                if (arr != null && arr.length > 0) sigInfo = sigInfo + "/" + md5(arr[0].toByteArray());
            }
        } catch (Throwable ignored) { }
        Log.i(TAG, "selfTest[" + how + "] signatures=" + md5 + " signingInfo=" + sigInfo);
    }

    private static String md5(byte[] b) {
        try {
            java.security.MessageDigest d = java.security.MessageDigest.getInstance("MD5");
            byte[] h = d.digest(b);
            StringBuilder sb = new StringBuilder();
            for (byte x : h) sb.append(String.format("%02x", x));
            return sb.toString();
        } catch (Throwable t) { return "md5err"; }
    }

    static final class Wrap implements Parcelable.Creator<PackageInfo> {
        private final Parcelable.Creator<PackageInfo> d;
        Wrap(Parcelable.Creator<PackageInfo> d) { this.d = d; }
        public PackageInfo createFromParcel(Parcel p) {
            PackageInfo pi = d.createFromParcel(p);
            if (pi != null) apply(pi);
            return pi;
        }
        public PackageInfo[] newArray(int n) { return d.newArray(n); }
    }

    private static volatile int sHits;   // 仅用于控制日志量
    static void apply(PackageInfo pi) {
        // 只改本作：别的包（微信/商店等）签名必须保持真实，否则会连带坏掉第三方校验
        if (pi == null || pi.packageName == null) return;
        String target = sTarget;
        if (target != null && !target.equals(pi.packageName)) return;
        try {
            Field fs = PackageInfo.class.getField("signatures");
            fs.setAccessible(true);
            fs.set(pi, new Signature[] { new Signature(OFFICIAL) });
        } catch (Throwable ignored) { }
        try {
            Object details = buildSigningDetails();
            if (details != null) {
                Constructor<?> c = Class.forName("android.content.pm.SigningInfo").getDeclaredConstructor(details.getClass());
                c.setAccessible(true);
                Field fi = PackageInfo.class.getField("signingInfo");
                fi.setAccessible(true);
                fi.set(pi, c.newInstance(details));
            }
        } catch (Throwable ignored) { }
    }

    private static Object buildSigningDetails() {
        try {
            Signature[] sig = new Signature[] { new Signature(OFFICIAL) };
            Class<?> sd = Class.forName("android.content.pm.SigningDetails");
            for (Constructor<?> c : sd.getDeclaredConstructors()) {
                Class<?>[] pt = c.getParameterTypes();
                try {
                    c.setAccessible(true);
                    if (pt.length == 3 && pt[0] == Signature[].class && pt[1] == Signature[].class && pt[2] == int.class) {
                        return c.newInstance(sig, sig, Integer.valueOf(0));
                    }
                    if (pt.length == 4 && pt[0] == Signature[].class && pt[1] == int.class && pt[3] == int.class) {
                        if (pt[2] == Signature[].class) return c.newInstance(sig, Integer.valueOf(3), sig, Integer.valueOf(0));
                        if (pt[2].getName().endsWith("SigningDetails")) return c.newInstance(sig, Integer.valueOf(3), null, Integer.valueOf(0));
                    }
                } catch (Throwable ignored) { }
            }
        } catch (Throwable ignored) { }
        return null;
    }

    private static void clearCaches() {
        try {
            Class<?> parcel = Class.forName("android.os.Parcel");
            for (String n : new String[] { "mCreators", "sPairedCreators" }) {
                try {
                    Field f = parcel.getDeclaredField(n);
                    f.setAccessible(true);
                    Object v = f.get(null);
                    if (v instanceof Map) ((Map<?, ?>) v).clear();
                } catch (Throwable ignored) { }
            }
        } catch (Throwable ignored) { }
        try {
            Class<?> apm = Class.forName("android.app.ApplicationPackageManager");
            for (Field f : apm.getDeclaredFields()) {
                if (!Map.class.isAssignableFrom(f.getType())) continue;
                try {
                    f.setAccessible(true);
                    Object v = f.get(null);
                    if (v instanceof Map) ((Map<?, ?>) v).clear();
                } catch (Throwable ignored) { }
            }
        } catch (Throwable ignored) { }
    }
}
