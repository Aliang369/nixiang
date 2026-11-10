package com.starry.magic.dwdlm;

import android.content.pm.PackageInfo;
import android.content.pm.Signature;
import android.content.pm.SigningInfo;
import android.util.Base64;
import android.util.Log;

import java.lang.reflect.Field;
import java.lang.reflect.InvocationHandler;
import java.lang.reflect.Method;
import java.lang.reflect.Proxy;

/**
 * 动物大联盟 1.0.25 —— 私有签名回灌桥（照 1.0.23 SignSpoof 的等价物）。
 *
 * <p>为什么需要它：包里现成的 MT 签名杀手（bin.mt.signature.KillerApplication193）是
 * GET_SIGNATURES 派，只改 PackageInfo.signatures。1.0.25 的壳 libsecret.so 走的是
 * <b>GET_SIGNING_CERTIFICATES</b>（反汇编字符串 "sig-check: getPackageInfo(GET_SIGNING_CERTIFICATES)
 * exception"），返回 SigningInfo，老杀手管不到 ⇒ 壳仍拿到星光证书 ⇒ apksecurefile 的 KEY 错 ⇒
 * 真实 dex 解不出 ⇒ Application 半成品 attach ⇒ NPE Context.getResources() on null。
 *
 * <p>做法：用动态代理包住 ActivityThread.sPackageManager（IPackageManager），在
 * getPackageInfo* 的返回上把 signatures 与 signingInfo 都换成官方证书。所有重载
 * （(String,int)、(String,long,int)、getPackageInfoVersioned）都会被覆盖，不依赖方法签名匹配。
 *
 * <p>只读、不改系统设置；安装失败仅打日志，不影响启动（与不装时行为一致）。
 */
public final class DwdlmSignBridge {

    private static final String TAG = "DwdlmSignBridge";
    private static final String OFFICIAL_CERT_B64 =
        "MIIDaTCCAlGgAwIBAgIEaqhNXzANBgkqhkiG9w0BAQsFADBkMQswCQYDVQQGEwJDTjESMBAGA1UECBMJZ3Vhbmdkb25nMRIwEAYDVQQHEwlndWFuZ3pob3UxDjAMBgNVBAoTBWJpbmdvMQ4wDAYDVQQLEwViaW5nbzENMAsGA1UEAxMEc2hhbjAgFw0yMzA5MDkwNTA3NDFaGA8yMTIzMDgxNjA1MDc0MVowZDELMAkGA1UEBhMCQ04xEjAQBgNVBAgTCWd1YW5nZG9uZzESMBAGA1UEBxMJZ3Vhbmd6aG91MQ4wDAYDVQQKEwViaW5nbzEOMAwGA1UECxMFYmluZ28xDTALBgNVBAMTBHNoYW4wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCfR0huISUVuONz3O/C0HR93zyxx1y23fbLfAZDmm99Ye3MKsStG0dV+VfopLfnRv3Zg3BtSj2c7lpIdROdxEsT0EWuQ4o6JNKgCTParqNouB57xMWhqN+E8iaF1lP9fYT/DQRaGLWoY1lfcu3JXy7kJiDsXXGgQUYiMJjgDbFHKPP+2qMfKKbdCyqHOWM4R1vjdkKifsd3YTw8+FOrMsQ6XFYLPgDZAdmA4yWFsLJXVZaTbcRRcYl/9b1U1EXGsdARF8X0zWNW+lokddhKGwn05tEnbpke18cclOKSstIiZGEab4aoifo6lWUbEM4sgILSJIozane5GbuGdqfz9huRAgMBAAGjITAfMB0GA1UdDgQWBBTvLE8/I1WCrRjo25ZLgRZtFP1x7jANBgkqhkiG9w0BAQsFAAOCAQEAJmar+MxzZ+Oz4E+R2gT/L9h2sfjkv8U7I60k6wCkMIl6nvEgPoVBHFvJ+CgiXlWvWGJT/StLP1A0TbS4Ic+fMrCB1a7tf3qopaJ8a/pbH/el4r6HQgI0rrqBXD8gtBcCT0q5NrcGaD3CNtz0MgwJB7Q7NF/bdtoX6+B2JhhYsnNDyGe3CuK/XAEOo242gqe/uOAAh5jMdtzX3SibHsEEjOFMuh0W/1djI9lpKUtoQ9jjZ3wLOd6bPDpgQeeul8GVY4Vo5J/X33ooWzAYukJlL/Ur3+10JcsPFDBLVwj5gG+eBR9GtsJoRfUF3qcqVP1rpIumaNfiwEMOfzzZmbNv+w==";

    private static boolean sInstalled;
    private static Signature sSig;

    public static synchronized void install() {
        if (sInstalled) return;
        try {
            Class<?> atClass = Class.forName("android.app.ActivityThread");
            Method current = atClass.getDeclaredMethod("currentActivityThread");
            current.setAccessible(true);
            Object thread = current.invoke(null);
            if (thread == null) {
                Log.w(TAG, "no ActivityThread; skipped");
                return;
            }
            Field field = atClass.getDeclaredField("sPackageManager");
            field.setAccessible(true);
            Object original = field.get(null);
            if (original == null) {
                Log.w(TAG, "sPackageManager null; skipped");
                return;
            }
            Class<?> iface = Class.forName("android.content.pm.IPackageManager");
            Object proxy = Proxy.newProxyInstance(iface.getClassLoader(),
                    new Class<?>[]{iface}, new Handler(original));
            field.set(null, proxy);
            sInstalled = true;
            Log.i(TAG, "installed over " + original.getClass().getName());
        } catch (Throwable t) {
            Log.w(TAG, "install failed: " + t);
        }
    }

    private static final class Handler implements InvocationHandler {
        private final Object mBase;

        Handler(Object base) {
            mBase = base;
        }

        @Override
        public Object invoke(Object proxyObj, Method method, Object[] args) throws Throwable {
            Object result = method.invoke(mBase, args);
            if (result instanceof PackageInfo && method.getName().startsWith("getPackageInfo")) {
                fill((PackageInfo) result);
            }
            return result;
        }
    }

    private static void fill(PackageInfo pi) {
        try {
            Signature sig = official();
            pi.signatures = new Signature[]{sig};
            try {
                Class<?> siClass = Class.forName("android.content.pm.SigningInfo");
                Object info = siClass.getDeclaredConstructor().newInstance();
                Method set = siClass.getDeclaredMethod("setSignatures", Signature[].class);
                set.setAccessible(true);
                set.invoke(info, (Object) new Signature[]{sig});
                pi.signingInfo = (SigningInfo) info;
            } catch (Throwable inner) {
                if (LOG_LIMIT < 3) Log.w(TAG, "signingInfo patch failed: " + inner);
            }
            if (LOG_LIMIT < 3) {
                LOG_LIMIT++;
                Log.i(TAG, "spoofed packageInfo signatures for " + pi.packageName);
            }
        } catch (Throwable t) {
            Log.w(TAG, "fill failed: " + t);
        }
    }

    private static int LOG_LIMIT = 0;

    private static Signature official() {
        if (sSig == null) {
            sSig = new Signature(Base64.decode(OFFICIAL_CERT_B64, Base64.DEFAULT));
        }
        return sSig;
    }
}
