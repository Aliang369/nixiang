package com.starry.magic.sign;

import android.app.Activity;
import android.app.AppComponentFactory;
import android.app.Application;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.ContentProvider;
import android.content.Intent;
import android.util.Log;

/**
 * Installs the signature spoof before the game's real Application is created.
 *
 * Provider.onCreate runs after Application.attachBaseContext and after other
 * providers have already started, which is too late for cold-start TapTap token
 * refresh. This factory preserves the original Application class and delegates
 * every non-Application component to the game's original CoreComponentFactory.
 */
public final class SignSpoofAppComponentFactory extends AppComponentFactory {
    private static final String TAG = "StarrySignFactory";
    private static final String ORIGINAL_FACTORY = "androidx.core.app.CoreComponentFactory";

    private volatile AppComponentFactory delegate;

    private AppComponentFactory delegate(ClassLoader cl) {
        AppComponentFactory current = delegate;
        if (current != null) return current;
        synchronized (this) {
            current = delegate;
            if (current != null) return current;
            try {
                current = (AppComponentFactory) Class.forName(ORIGINAL_FACTORY, true, cl)
                        .getDeclaredConstructor()
                        .newInstance();
                delegate = current;
                return current;
            } catch (Throwable t) {
                Log.w(TAG, "original factory unavailable, using framework default", t);
                return null;
            }
        }
    }

    @Override
    public Application instantiateApplication(ClassLoader cl, String className)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        try {
            SignSpoof.install();
            Log.i(TAG, "SignSpoof installed before application=" + className);
        } catch (Throwable t) {
            Log.e(TAG, "SignSpoof pre-install failed", t);
        }
        AppComponentFactory original = delegate(cl);
        return original != null
                ? original.instantiateApplication(cl, className)
                : super.instantiateApplication(cl, className);
    }

    @Override
    public Activity instantiateActivity(ClassLoader cl, String className, Intent intent)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        AppComponentFactory original = delegate(cl);
        return original != null
                ? original.instantiateActivity(cl, className, intent)
                : super.instantiateActivity(cl, className, intent);
    }

    @Override
    public ContentProvider instantiateProvider(ClassLoader cl, String className)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        AppComponentFactory original = delegate(cl);
        return original != null
                ? original.instantiateProvider(cl, className)
                : super.instantiateProvider(cl, className);
    }

    @Override
    public BroadcastReceiver instantiateReceiver(ClassLoader cl, String className, Intent intent)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        AppComponentFactory original = delegate(cl);
        return original != null
                ? original.instantiateReceiver(cl, className, intent)
                : super.instantiateReceiver(cl, className, intent);
    }

    @Override
    public Service instantiateService(ClassLoader cl, String className, Intent intent)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        AppComponentFactory original = delegate(cl);
        return original != null
                ? original.instantiateService(cl, className, intent)
                : super.instantiateService(cl, className, intent);
    }
}
