package com.starry.magic.core;

import android.app.Activity;
import android.app.Application;
import android.content.Context;

/**
 * 单游戏接入点（SPI）——框架层<b>唯一</b>允许出现游戏差异的地方。
 *
 * <p>背景（2026-11-11 框架收口）：此前共用类里混进了单游戏逻辑，典型形态有三种：
 * <ul>
 *   <li>{@code StarryApp.attachBaseContext} 无条件调动物餐厅的 {@code DwctIapGate}；</li>
 *   <li>{@code StarryApp} / {@code StarryInitProvider} 无条件跑 {@code TapLoginForceWeb}
 *       （内部再用 37 项包名白名单自检）；</li>
 *   <li>{@code StarryInitProvider} 里写死 {@code com.fightingstars.games.animal} 的
 *       APKSecure 补丁线程。</li>
 * </ul>
 * 后果：每个游戏的包里都跑别的游戏的代码，且「某游戏为过检测改的东西」会永久留在
 * 公共框架里。现在统一改为：<b>框架只按 game_id 反射找一个实现类，找不到就当没这回事</b>。
 *
 * <h3>命名约定（不要改，注入脚本按此裁剪）</h3>
 * <pre>
 *   com.starry.magic.games.{game_id}.GameHookImpl
 * </pre>
 * {@code game_id} 来自 native 烘焙值（{@code GameConfig.h} ← {@code Android_<tag>.mk}），
 * 或独立助手形态下 host-app 探测结果，见 {@link GameHooks#currentGameId()}。
 *
 * <h3>隔离契约</h3>
 * <ul>
 *   <li>框架层<b>不得</b> import 任何 {@code com.starry.magic.games.*} 的具体类；
 *       一律经 {@link GameHooks} 反射取得。此约束由
 *       {@code 04_scripts/_common/check_framework_isolation.sh} 机器校验。</li>
 *   <li>实现类缺方法不视为错误：全部方法都有空默认实现，老游戏不需要写。</li>
 *   <li>实现类必须能承受「不该被调到」的调用（例如别的游戏也加载了它）——
 *       自己的判断自己写在实现里，框架不再替它做包名白名单。</li>
 * </ul>
 */
public interface GameHook {

    /**
     * {@code Application.attachBaseContext} 之后，native 加载之前。
     * 用于必须在最早时机做的补丁（IAP 校验、壳的 PluginHelper 回填等）。
     */
    default void onAttachBaseContext(Application app) {}

    /**
     * {@code Application.onCreate} 之后、菜单调度之前。
     * 此时 native 已加载，适合需要 JNI 的初始化。
     */
    default void onApplicationCreate(Application app) {}

    /** {@code Activity.onCreate} 之前（handler 遍历尚未发生，窗口正好）。 */
    default void onActivityPreCreated(Activity activity) {}

    /** {@code Activity.onCreate} 之后。 */
    default void onActivityCreated(Activity activity) {}

    /**
     * 独立助手形态（无 root 直装）专用：菜单不在游戏进程内时，
     * 由助手侧主动挂到目标游戏。返回 true 表示本实现已接管。
     */
    default boolean onHelperAttach(Context helperContext) { return false; }

    /**
     * 独立助手形态下的<b>目标游戏包名</b>（注入形态不需要，框架直接用自身包名）。
     * <p>返回空串 = 本实现不提供，{@code ModTarget.externalDirs()} 会返回空数组，
     * 框架不会误写任何外部目录。
     */
    default String targetPackage() { return ""; }

    /**
     * 本游戏的包是否把 starry so 藏在别的 native 入口里
     * （SecShell 之类的壳不接受新增 so 条目；见 {@code ota/StarryNativeLoad}）。
     * <p>返回 true 时框架会尝试 {@code System.loadLibrary("SecShell-x86")} 别名加载。
     */
    default boolean usesPackagedSoAlias() { return false; }
}
