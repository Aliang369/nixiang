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
 * 星光"早启动 AppComponentFactory"入口：保留游戏原本的 Application 类，
 * 把所有非 Application 组件的实例化交给原厂 {@code androidx.core.app.CoreComponentFactory}。
 *
 * <p><b>历史与现状（2026-10-28）</b>：本类原名为 {@code SignSpoofAppComponentFactory}，
 * 唯一职责是在真实 Application 创建前安装运行时签名伪装（{@code SignSpoof.install()}）。
 * 用户 2026-10-28 定案「TapTap 验签/登录一律改包法，不再用签名伪装」后，签名伪装已**整体删除**，
 * 本类只作为"早于 Application 的 Java 入口"保留（勇士之心 1.0.4 的 SecShell 看门狗
 * {@code ysxz/YsxzAppComponentFactory} 需要它；其它需要在 ~365ms 内跑代码的包也用它）。
 *
 * <p><b>新包请优先用 {@code StarryInitProvider}（ContentProvider）做入口</b>；
 * 只有在"必须早于 Application/壳自检"时才改 Manifest 的 {@code appComponentFactory} 到本类。
 */
public class StarryAppComponentFactory extends AppComponentFactory {
    private static final String TAG = "StarryAppFactory";
    private static final String ORIGINAL_FACTORY = "androidx.core.app.CoreComponentFactory";

    /**
     * 2026-10-29「双轨制」签名轨（B）：MT ApkSignatureKillerEx 形态的运行时签名替换
     * （{@code bin.mt.signature.KillerApplication193}，其 {@code <clinit>} 完成
     * 换 {@code PackageInfo.CREATOR} + 清 {@code PackageManager.sPackageInfoCache}
     * / {@code Parcel.mCreators} / {@code Parcel.sPairedCreators}）。
     *
     * <p>挂载点选这里而不是改游戏 Application 父类：{@code instantiateApplication} 早于
     * 真实 Application 的 {@code attachBaseContext/onCreate} 与一切 ContentProvider，
     * 且**官方 dex 一个字节都不用动**。killer 类是随包注入的独立 dex，包内没有该类时
     * 本方法静默跳过（对其余游戏零影响）。
     */
    private static final String KILLER_CLASS = "bin.mt.signature.KillerApplication193";
    private static volatile boolean killerMounted;

    private static void mountSignatureKiller(ClassLoader cl) {
        if (killerMounted) return;
        try {
            // initialize=true ⇒ 立即跑 KillerApplication193.<clinit>（安装动作全在那里）
            Class.forName(KILLER_CLASS, true, cl);
            killerMounted = true;
            Log.i(TAG, "SignatureKiller mounted: PackageInfo.CREATOR replaced, PM/Parcel caches cleared");
        } catch (Throwable t) {
            Log.i(TAG, "SignatureKiller absent, skip: " + t.getClass().getSimpleName());
        }
    }

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
        // 必须早于 Application 实例化（TapSDK/风控在 Application 与 Provider 阶段就开始读签名）
        mountSignatureKiller(cl);
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
