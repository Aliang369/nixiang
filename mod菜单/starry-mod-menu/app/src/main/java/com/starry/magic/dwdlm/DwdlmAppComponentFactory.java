package com.starry.magic.dwdlm;

import android.app.Activity;
import android.app.AppComponentFactory;
import android.app.Application;
import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.ContentProvider;
import android.content.Intent;
import android.util.Log;

/**
 * 动物大联盟 1.0.25（TapTap 原版 / APKSecure 壳）专用**最早 Java 入口**工厂。
 *
 * <p>职责两件，都必须在框架实例化真实 {@code Application}（壳的 {@code dev.mars.secure.ProxyApplication}）
 * <b>之前</b>完成：
 * <ol>
 *   <li><b>挂运行时签名替换</b>（{@link #mountSigKiller(ClassLoader)}）：触发包内
 *       {@code bin.mt.signature.KillerApplication193} 的静态初始化，把
 *       {@code PackageInfo.CREATOR} 换成返回官方证书的实现，并清
 *       {@code PackageManager.sPackageInfoCache} / {@code Parcel.mCreators} /
 *       {@code Parcel.sPairedCreators}。</li>
 *   <li><b>载 native + 起看门狗</b>：把**包内** {@code libstarry_magic.so} 载入并调用
 *       {@code nativeEnvStart()}，让 {@code Games/DwdlmSecShell.cpp} 的看门狗在壳跑完整性
 *       自检之前就位。</li>
 * </ol>
 * 不改 Application 类、不改壳的任何行为；官方 dex 一个字节都不动。
 *
 * <p><b>为什么自己实现而不复用 {@code sign.StarryAppComponentFactory}：</b>公共工厂是跨游戏共享件，
 * 本作要在同一入口再加「早期 native 载入」这一步，且**只挂这一条签名链**（MT
 * ApkSignatureKillerEx 形态的运行时签名替换）。共享类一字未改，本类自持一份最小实现，
 * 不为单个游戏改共享件。
 *
 * <p>仍<b>不复活</b> {@code SignSpoof} / {@code SignSpoofAppComponentFactory}（2026-10-28 已整体
 * 删除，属"静态改签名"的旧路）；本轮走的是双轨制里的「运行时签名替换」轨，两者不是同一条链路，
 * 成品包里禁止出现 {@code SignSpoof} 字符串（由构建脚本断言）。
 *
 * <p><b>为什么必须挂签名杀手（E1 证据，2026-11-05 实测）：</b>1.0.25 是 APKSecure 壳。**纯重签、
 * 零注入**的对照包首次冷启同样崩 —— {@code ConfigurationController.updateLocaleListFromAppContext:342}
 * NPE + 14 条 {@code StubProvider class missing}（真实 Provider 类缺失），phase app-ready 4029ms。
 * ⇒ 崩溃与我们注入的菜单/native **无关**，根因是「重签后壳拿不到官方签名」。
 * {@code lib/arm64-v8a/libsecret.so} 内含
 * {@code key-unwrap: get runtime signature failed} /
 * {@code sig-check path: GET_SIGNATURES(fallback) API=%d} /
 * {@code signature cert cross mismatch}：壳用**运行时签名**（PackageManager 查询结果）
 * 解 {@code assets/apksecurefile} 的密钥；重签后 unwrap 失败 ⇒ 真实 dex 不加载 ⇒ 壳半成品 ⇒
 * 系统侧 NPE。把运行时签名换回官方值，是唯一"官方 dex 一字节不动"的解法。
 *
 * <p><b>为什么挂载点是这里（最早）：</b>{@code AppComponentFactory.instantiateApplication} 由
 * {@code ActivityThread.handleBindApplication} 在 {@code Application.attachBaseContext} / 一切
 * ContentProvider **之前**调用，而壳的签名查询就在 {@code attachBaseContext} 之后的
 * {@code libsecret} unwrap 里。挂在本类的这一行，壳第一次问 PackageManager 时
 * {@code PackageInfo.CREATOR} 已被替换。
 *
 * <p><b>与菜单的关系：</b>菜单注入点仍是 ContentProvider（{@code StarryInitProvider}），
 * 本类不碰 Provider、不接管菜单加载，也不改变官方 {@code ProxyApplication}。
 *
 * <p><b>so 来源（2026-11-05 修复 D4）：只加载包内库</b>（{@code System.loadLibrary}）。
 * 不再按 {@code files/starry_ota/<name>/libstarry_magic.so} 搜索：那条"OTA 优先"路径会
 * 命中上一个候选残留的**旧 so**，本轮直装的新库被绕过（包内新 dex + 设备上旧 native 混跑，
 * 行为与日志都对不上）。本类不删除设备目录、不清理历史包，只保证"装了哪个包就加载哪个包"。
 *
 * <p><b>日志口径：</b>载入成功与 {@code nativeEnvStart} 是否真的把看门狗拉起来分别打印；
 * 失败只打印异常类名并明确写 {@code env NOT started}，**不冒充已启动**。
 *
 * <p><b>API 兼容：</b>{@code AppComponentFactory} 自 API 28 起存在，manifest 的
 * {@code android:appComponentFactory} 属性在 API &lt; 28 上被框架忽略 ⇒ 本类不会被加载；
 * 本作 minSdk 21 / targetSdk 33 因此不受影响。
 */
public class DwdlmAppComponentFactory extends AppComponentFactory {
    private static final String TAG = "DwdlmAppFactory";

    /** 原厂实现：非 Application 组件仍交给它，保持与"未挂 factory"时一致的行为。 */
    private static final String ORIGINAL_FACTORY = "androidx.core.app.CoreComponentFactory";

    /** 包内签名杀手类（MT ApkSignatureKillerEx 形态；安装动作全在它的 {@code <clinit>} 里）。 */
    private static final String KILLER_CLASS = "bin.mt.signature.KillerApplication193";

    private static volatile boolean envStarted;
    private static volatile boolean nativeLoaded;
    private static volatile boolean sigKillerMounted;

    /** Games/DwdlmSecShell.cpp：Java_com_starry_magic_dwdlm_DwdlmAppComponentFactory_nativeEnvStart */
    private static native void nativeEnvStart();

    /** Games/DwdlmSecShell.cpp：只读状态查询（0 IDLE / 1 RESOLVING / 2 ACTIVE / 3 ABORT / 4 DONE） */
    private static native int nativeEnvState();

    static {
        // 类初始化时只做"载入包内 so"这一件事：不引用任何游戏类，避免类加载循环。
        loadNativeEarly();
    }

    private volatile AppComponentFactory delegate;

    @Override
    public Application instantiateApplication(ClassLoader cl, String className)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        // 第 0 步，必须是这里的**第一行**：壳在 attachBaseContext 里就要查运行时签名。
        mountSigKiller(cl);
        startNativeEnv();
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

    /**
     * 触发包内签名杀手的静态初始化（{@code initialize=true} ⇒ 立即执行 {@code <clinit>}，
     * 换 {@code PackageInfo.CREATOR} + 清 PM/Parcel 缓存，安装动作全在那一个静态块里，
     * 类加载完成即已生效）。
     *
     * <p><b>失败不抛</b>：包内没有该类（killer dex 未注入 / 版本不匹配）时只是少一层签名替换，
     * 行为与未挂本工厂时等价；日志用 {@code sig killer absent:} 与成功日志明确区分，
     * 不把"没挂上"报成"已挂上"。
     */
    private static void mountSigKiller(ClassLoader cl) {
        if (sigKillerMounted) return;
        synchronized (DwdlmAppComponentFactory.class) {
            if (sigKillerMounted) return;
            try {
                Class.forName(KILLER_CLASS, true, cl);
                sigKillerMounted = true;
                Log.i(TAG, "sig killer mounted: " + KILLER_CLASS
                        + " (PackageInfo.CREATOR replaced, PM/Parcel caches cleared)");
            } catch (Throwable t) {
                Log.i(TAG, "sig killer absent: " + t);
            }
        }
    }

    /**
     * 触发 native 侧看门狗。Java 侧幂等（只进一次 JNI），native 侧亦幂等；
     * 任何异常都被吞掉——**绝不让环境接入失败阻断启动**（最坏情况是退回"无看门狗"，
     * 与未挂本工厂时等价）。失败日志只写异常类名，不写路径/包名/堆栈。
     */
    private static void startNativeEnv() {
        if (envStarted) return;
        synchronized (DwdlmAppComponentFactory.class) {
            if (envStarted) return;
            if (!nativeLoaded) {
                // 包内 so 都没载入：不打 JNI，也不写"已启动"。
                Log.w(TAG, "nativeEnvStart skipped: bundled lib not loaded (env NOT started)");
                return;
            }
            try {
                nativeEnvStart();
            } catch (Throwable t) {
                Log.w(TAG, "nativeEnvStart failed: " + t.getClass().getSimpleName()
                        + " (env NOT started)");
                return;
            }
            // 状态查询单独兜底：万一 so 与 dex 不同源（没有 nativeEnvState），
            // 只把状态记为未知，不把已经起来的看门狗反报成失败。
            int state = -1;
            try {
                state = nativeEnvState();
            } catch (Throwable t) {
                state = -1;
            }
            if (state == 0) {
                // Start 里 pthread_create 失败会退回 IDLE：明确判失败，允许后续重试。
                Log.w(TAG, "nativeEnvStart returned but watchdog NOT started (state=0)");
                return;
            }
            envStarted = true;
            Log.i(TAG, "nativeEnvStart ok (bundled lib, watchdog state=" + state + ")");
        }
    }

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
                Log.w(TAG, "original factory unavailable, use framework default: "
                        + t.getClass().getSimpleName());
                return null;
            }
        }
    }

    /**
     * 只加载**包内** so（这一轮 D4 的口径）：{@code System.loadLibrary("starry_magic")}。
     *
     * <p>刻意**不**再搜索 {@code files/starry_ota/...}：那是历史候选的落地目录，最新
     * 修改时间优先的规则会让本次直装命中**旧 so**，从而绕过本轮新库（例如 D1 的
     * process_vm_writev 写入通道根本没生效）。本方法不删除设备上的目录或历史包，
     * 只是不再从那里取库。
     *
     * <p>加载会触发 JNI_OnLoad（注册桥接 + 本游戏 onLoad；onLoad 里也会幂等调一次
     * {@code DwdlmSecShell_Start}）。载入失败只记一次日志并置 {@code nativeLoaded=false}，
     * 后续 {@code startNativeEnv} 直接跳过——不把"so 没进来"报成"已启动"。
     */
    private static void loadNativeEarly() {
        try {
            System.loadLibrary("starry_magic");
            nativeLoaded = true;
            Log.i(TAG, "bundled libstarry_magic.so loaded via loadLibrary");
        } catch (Throwable t) {
            nativeLoaded = false;
            Log.w(TAG, "bundled loadLibrary failed: " + t.getClass().getSimpleName()
                    + " (env NOT started)");
        }
    }
}
