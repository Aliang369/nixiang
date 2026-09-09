package com.starry.magic;

import android.app.AppComponentFactory;
import android.app.Application;
import android.util.Log;

/**
 * 等长替换 {@code androidx.core.app.CoreComponentFactory}（38 字符）用的入口。
 * 编译期继承框架 {@link AppComponentFactory}（菜单工程无 androidx.core）；
 * 运行时游戏 dex 里仍有 CoreComponentFactory，本类在 PathClassLoader 中可正常工作。
 * <p>
 * 类名长度必须保持 38：{@code com.starry.magic.StarryAppCompFactory0}
 */
public class StarryAppCompFactory0 extends AppComponentFactory {
    private static final String TAG = "StarryAppFact";

    @Override
    public Application instantiateApplication(ClassLoader cl, String className)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        Log.i(TAG, "instantiateApplication orig=" + className + " → StarryApp");
        return super.instantiateApplication(cl, StarryApp.class.getName());
    }
}
