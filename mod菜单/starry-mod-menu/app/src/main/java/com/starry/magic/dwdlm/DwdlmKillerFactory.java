package com.starry.magic.dwdlm;

import android.app.AppComponentFactory;
import android.app.Application;
import android.util.Log;

/**
 * 动物大联盟 1.0.25 · 无壳包「运行时签名替换」挂载工厂（唯一变量实验）。
 *
 * <p>只做一件事：在框架实例化 Application 之前，先把 MT 签名替换 dex 的静态初始化跑起来
 * （{@code bin.mt.signature.KillerApplication193} 的 &lt;clinit&gt; 负责替换
 * {@code PackageInfo.CREATOR} 并清理 PackageManager / Parcel 缓存），
 * 让后续任何 {@code getPackageInfo(...).signatures} 读到官方证书。
 *
 * <p>挂载点选 {@code instantiateApplication}：它由 {@code LoadedApk} 在最早期调用，
 * 早于 Application.onCreate 与任何 SDK 初始化。
 *
 * <p>纪律：只重写 {@code instantiateApplication}，其余 4 个 instantiate* 一律走父类默认实现
 * （不包 Activity/Provider/Receiver/Service，避免与壳/Provider 注入链互相干扰）。
 * killer dex 缺失时不抛异常，仅记日志，保证无壳包原有启动链不受影响。
 */
public class DwdlmKillerFactory extends AppComponentFactory {

    private static final String TAG = "DwdKillerFactory";
    private static final String KILLER_CLASS = "bin.mt.signature.KillerApplication193";

    private static boolean sKillerMounted = false;

    @Override
    public Application instantiateApplication(ClassLoader cl, String className)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        mountKiller(cl);
        return super.instantiateApplication(cl, className);
    }

    private static synchronized void mountKiller(ClassLoader cl) {
        if (sKillerMounted) {
            return;
        }
        try {
            Class.forName(KILLER_CLASS, true, cl);
            sKillerMounted = true;
            Log.i(TAG, "killer mounted");
        } catch (Throwable t) {
            Log.i(TAG, "killer absent: " + t);
        }
    }
}
