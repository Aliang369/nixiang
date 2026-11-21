package com.starry.magic.ysxz;

import android.app.Application;
import android.util.Log;

import com.starry.magic.sign.StarryAppComponentFactory;

/**
 * 勇士之心 1.0.4 专用 appComponentFactory：在框架实例化真实 Application **之前**启动
 * SecShell 运行态净化看门狗（native 实现见 Games/YsxzSecShell.cpp）。
 *
 * <p>为什么必须挂在这里：壳 libsecret.so 在启动 413~483ms 跑完整性自检，本作重签名 +
 * 外科改动导致 3 项失败（failedMask 0x212），壳把 dirty 置 1，于是 ~649ms 的 unshell
 * 只给真实 Application 调 attachBaseContext、<b>跳过 onCreate</b> ⇒ BingoApplication.onCreate
 * 不跑 ⇒ PluginHelper.initWithApplication 不跑 ⇒ {@code PluginHelper.app == null} ⇒
 * AppActivity 启动时 {@code AppInfo.parameters} NPE 闪退。
 *
 * <p>实测本工厂在启动 ~365ms 就被框架调用（远早于壳自检 413ms），是本进程内「早于壳自检」
 * 的唯一现成 Java 入口——StarryInitProvider.onCreate 要到 ~651ms，libstarry_magic.so 的
 * JNI_OnLoad 也随之太晚。
 *
 * <p>2026-10-28：运行时签名伪装已整体删除，父类更名为 {@link StarryAppComponentFactory}，
 * 只负责"保留原厂 Application 并委托 CoreComponentFactory"；本类只追加 native 预备调用。
 */
public class YsxzAppComponentFactory extends StarryAppComponentFactory {
    private static final String TAG = "YsxzAppFactory";

    /** Games/YsxzSecShell.cpp：Java_com_starry_magic_ysxz_YsxzAppComponentFactory_nativePrepareSecureShell */
    private static native void nativePrepareSecureShell();

    static {
        loadNativeEarly();
    }

    @Override
    public Application instantiateApplication(ClassLoader cl, String className)
            throws InstantiationException, IllegalAccessException, ClassNotFoundException {
        startSecShellWatchdog(className);
        return super.instantiateApplication(cl, className);
    }

    private static void startSecShellWatchdog(String className) {
        try {
            nativePrepareSecureShell();
            Log.i(TAG, "SecShell watchdog started before application=" + className);
        } catch (Throwable t) {
            Log.e(TAG, "SecShell watchdog start failed", t);
        }
    }

    /**
     * 尽早把 libstarry_magic.so 载入（只加载包内那一个）：
     * 加载动作本身会跑 JNI_OnLoad → 注册桥接 + 游戏 onLoad。
     * 2026-11-21：OTA 载荷体系下线 —— 不再扫描任何已下载补丁目录，只走包内路径。
     */
    private static void loadNativeEarly() {
        try {
            System.loadLibrary("starry_magic");
            Log.i(TAG, "native so via loadLibrary(starry_magic)");
        } catch (Throwable t) {
            Log.e(TAG, "loadLibrary starry_magic failed", t);
        }
    }

}
