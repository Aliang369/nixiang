package com.starry.magic.dwdlm;

import android.content.Context;
import android.util.Log;

/**
 * 动物大联盟（APKSecure 壳）专用 Application。
 *
 * <p>2026-10-28：运行时签名伪装（{@code SignSpoof}）已整体删除，本类只保留壳前置 native 准备；
 * 壳若仍需"签名一致"能力，须按改包法在游戏 dex 侧处理。
 */
public final class DwdlmApplication extends dev.mars.secure.ProxyApplication {
    static {
        System.loadLibrary("starry_magic");
    }

    private static native void prepareSecureShell();
    private static final String TAG = "DwdlmApp";

    @Override
    protected void attachBaseContext(Context base) {
        try {
            prepareSecureShell();
            Log.i(TAG, "secure shell integrity hook installed");
        } catch (Throwable t) {
            Log.e(TAG, "secure shell prepare failed", t);
        }
        super.attachBaseContext(base);
    }
}
