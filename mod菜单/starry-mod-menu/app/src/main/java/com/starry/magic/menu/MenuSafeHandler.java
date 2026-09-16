package com.starry.magic.menu;

import android.os.Handler;
import android.os.Looper;
import android.os.Message;
import android.util.Log;

import com.starry.magic.crash.CrashHandler;

/** Main-thread handler that isolates a menu callback failure instead of killing the game. */
public final class MenuSafeHandler extends Handler {
    private final String tag;

    public MenuSafeHandler(String tag) {
        super(Looper.getMainLooper());
        this.tag = tag;
    }

    @Override
    public void dispatchMessage(Message msg) {
        try {
            super.dispatchMessage(msg);
        } catch (Throwable t) {
            Log.e(tag, "menu callback exception isolated", t);
            try {
                CrashHandler.logException(t);
            } catch (Throwable ignored) {
            }
        }
    }
}
