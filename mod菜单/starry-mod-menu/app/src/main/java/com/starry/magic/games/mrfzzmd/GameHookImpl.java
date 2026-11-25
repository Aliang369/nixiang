package com.starry.magic.games.mrfzzmd;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * 明日方舟：终末地（{@code com.hypergryph.endfield}，game_id = {@code mrfzzmd}）——
 * TapTap 改签包强制内嵌网页登录。
 *
 * <p><b>为什么需要这个类</b>：本包 TapSDK 4.7.0 的类在官方 dex 里明文可达，但
 * <b>官方 dex 一个字节都不能改</b>（ACE 按 dex 文件字节/哈希点名，V1/V2 实验已定案：
 * 官方 classes2/3/5 只要字节变动 ⇒ 冷启 ~2s SIGBUS/SIGILL 必死）。所以「改包法」不可用，
 * 只能走运行时 SPI：登录入口由 {@link TapLoginForceWeb} 在 Activity 生命周期早期改写，
 * 让 TapSDK 的客户端 SSO 路径失败后退回内嵌 WebView。
 *
 * <p><b>写在哪里</b>（2026-11-11 框架收口）：{@code com.starry.magic.games.<game_id>.GameHookImpl}，
 * 由 {@code GameHooks.resolve()} 按约定名反射装载。这是唯一允许出现单游戏逻辑的地方 ——
 * 不碰 {@code StarryApp} / {@code StarryInitProvider}，也不存在任何包名白名单
 * （{@code TapLoginForceWeb} 内的白名单已于 2026-11-11 整体删除）。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都必须有 {@code -keepnames class com.starry.magic.games.**}，否则 R8 改名 ⇒ 反射装载
 * ClassNotFound、补丁静默失效（784 轮实测踩过：dex 里 {@code games/} 命中 0、
 * {@code magic/o/} 命中 154）。{@code check_framework_isolation.sh} 校验两个规则文件一致。
 *
 * <p><b>验证点</b>：logcat 出现 {@code game hook 已装载: com.starry.magic.games.mrfzzmd.GameHookImpl}；
 * 点登录时出现 {@code forced -> client throws, web fallback} 一类回退行（TapSDK 侧日志）。
 */
public final class GameHookImpl implements GameHook {

    /** 终末地 TapTap 渠道包名。 */
    private static final String PKG = "com.hypergryph.endfield";

    /**
     * {@code Activity.onCreate} 之前 —— TapSDK 的 handler 遍历尚未发生，窗口正好。
     * 在这里把 intent extra 的 request 换成会抛异常的形态，逼 LoginClient 回退到内嵌 WebView。
     */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapLoginForceWeb.onPreCreated(activity);
    }

    /** {@code Activity.onCreate} 之后 —— 新版 fallbackToWeb 兜底与 API&lt;29 清洗。 */
    @Override
    public void onActivityCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapLoginForceWeb.onCreated(activity);
    }

    /** 独立助手形态下告诉框架目标是哪个包（注入形态用不到，但保持 SPI 契约一致）。 */
    @Override
    public String targetPackage() {
        return PKG;
    }

    /**
     * 自己的包自己判断 —— SPI 契约要求实现类能承受「不该被调到」的调用
     * （别的游戏也加载本类时不得产生副作用）。
     * <p>这是<b>唯一</b>的生效判据：{@link TapLoginForceWeb} 本身已无白名单。
     */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
