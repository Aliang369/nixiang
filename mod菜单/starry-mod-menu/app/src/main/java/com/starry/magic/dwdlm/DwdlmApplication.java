package com.starry.magic.dwdlm;

import android.content.Context;
import android.util.Log;

import com.starry.magic.sign.SignSpoof;

/** Installs original-certificate spoofing before APKSecure starts signature checks. */
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
        try {
            SignSpoof.install();
            Log.i(TAG, "signature spoof installed before ProxyApplication");
        } catch (Throwable t) {
            Log.e(TAG, "signature spoof failed", t);
        }
        super.attachBaseContext(base);
    }
}
