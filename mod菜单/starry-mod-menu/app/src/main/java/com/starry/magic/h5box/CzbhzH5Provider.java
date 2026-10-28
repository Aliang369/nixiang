package com.starry.magic.h5box;

import android.app.Activity;
import android.app.Application;
import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.net.Uri;
import android.util.Log;

/**
 * 《城主别慌张》H5 注入的启动器 —— 只在 AndroidManifest 里声明，
 * {@code classes.dex / classes2.dex / classes3.dex} 一律不改。
 *
 * <p>为什么用 Provider：本作宿主 {MainActivity} 的 smali 注入要
 * baksmali/smali 整包重建 classes.dex（7.7MB、多 dex 顶部，风险高）。
 * ContentProvider 的 {@code onCreate} 在 Application 启动早期自动执行，
 * 于是"改 Manifest + 加一个自己的 classes4.dex"就能拿到启动时机与
 * Activity 生命周期，代价最小。
 *
 * <p>职责边界：
 * <ul>
 *   <li>框架的 {@code StarryInitProvider}（同样在 Manifest 声明）负责
 *       加载 libstarry_magic.so、起 MenuService（运行时签名伪装 SignSpoof 已于 2026-10-28 删除）；</li>
 *   <li>本类只负责在宿主 Activity 出现后，把 {@link StarryH5} 挂到它的
 *       游戏 WebView 上（找 WebView 的逻辑在 StarryH5 内）。</li>
 * </ul>
 *
 * <p>多进程防护：只在本进程名 == 包名时工作（复用框架的
 * {@code StarryApp.isMainProcess}）。
 */
public final class CzbhzH5Provider extends ContentProvider {

    private static final String TAG = "CzbhzH5Provider";

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
            Log.i(TAG, "register ActivityLifecycleCallbacks");
            application.registerActivityLifecycleCallbacks(
                    new Application.ActivityLifecycleCallbacks() {
                        @Override
                        public void onActivityResumed(Activity activity) {
                            try {
                                StarryH5.attachTo(activity);
                            } catch (Throwable t) {
                                Log.w(TAG, "attachTo 异常: " + t);
                            }
                        }

                        @Override
                        public void onActivityCreated(Activity activity, android.os.Bundle b) {
                            try {
                                // onCreated 时视图树通常已建好，先试一次，缩短注入等待
                                StarryH5.attachTo(activity);
                            } catch (Throwable t) {
                                Log.w(TAG, "attachTo(created) 异常: " + t);
                            }
                        }

                        @Override public void onActivityStarted(Activity activity) {}
                        @Override public void onActivityPaused(Activity activity) {}
                        @Override public void onActivityStopped(Activity activity) {}
                        @Override public void onActivitySaveInstanceState(Activity a, android.os.Bundle b) {}
                        @Override public void onActivityDestroyed(Activity activity) {}
                    });
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
