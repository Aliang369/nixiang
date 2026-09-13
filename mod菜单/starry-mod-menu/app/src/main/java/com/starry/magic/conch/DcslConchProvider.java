package com.starry.magic.conch;

import android.app.Application;
import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.util.Log;

import com.starry.magic.StarryApp;

/**
 * Startup provider for the Conch runtime bridge.
 *
 * <p>Declared in AndroidManifest with initOrder 101, after StarryInitProvider loads
 * the native menu framework. The game Application class is left untouched and the
 * game DEX files are not modified.</p>
 */
public final class DcslConchProvider extends ContentProvider {
    private static final String TAG = "DcslConchProvider";

    @Override
    public boolean onCreate() {
        try {
            Context holder = getContext();
            Context app = holder == null ? null : holder.getApplicationContext();
            if (!(app instanceof Application)) {
                Log.i(TAG, "no application context, skip");
                return true;
            }
            if (!StarryApp.isMainProcess(app)) {
                Log.i(TAG, "skip in secondary process: " + StarryApp.currentProcessName());
                return true;
            }
            StarryConch.start(app);
        } catch (Throwable t) {
            Log.e(TAG, "onCreate failed", t);
        }
        return true;
    }

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
