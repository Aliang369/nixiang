package com.starry.magic.games.wxsjdz1;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.core.TapTapWebLogin;

/**
 * 无限升级（{@code com.xingzuodaoshi.infinityUpgrade}，game_id = {@code wxsjdz1}）——
 * 星光版 21 槽 TapTap 改签包<b>兜底</b>强制内嵌网页登录。
 *
 * <p><b>为什么新建这个类（而不是复用 games/wxsj/GameHookImpl）</b>：
 * {@code GameHooks.resolve()} 的唯一判据是 <b>game_id</b>（反射
 * {@code com.starry.magic.games.<game_id>.GameHookImpl}），没有包名白名单、也没有兼容垫片。
 * 本入口的 {@code authGameId = "wxsjdz1"}（后台定制1 的 21 槽模板），
 * 因此 {@code games/wxsj/GameHookImpl}（对应 game_id = wxsj / 11–12 线）<b>装载不到</b>，
 * 需要一个 {@code games/wxsjdz1/} 下的同名实现。包名与本类一致（同一 APK），
 * 两者互不干扰：同一个 game_id 只会解析出一个实现。
 *
 * <p><b>与 dex 手术的关系</b>：13.101 现行出包链已经在 dex 层做「去 TapTap 验签 + 登录改网页」
 * （{@code 04_scripts/无限升级/patch_wxsj_tap_unshelled_dex.sh}）。本类是该链路的**第二条保险**：
 * 一旦 dex 手术漏掉某个入口 / 后续换底包，SPI 这条仍能把登录钉在官方 {@code WebLoginFragment}。
 * 判据：logcat {@code game hook 已装载: com.starry.magic.games.wxsjdz1.GameHookImpl}。
 * ⚠ 本类必须被编进 menu dex 才会生效（Java 侧改动 ⇒ 需重打菜单 dex）；未编入时行为等同于「无 SPI」，
 * 不影响 native 侧任何逻辑。
 *
 * <p><b>不做的事</b>：不复活 {@code SignSpoof}、不加包名白名单、不碰 {@code StarryApp} /
 * {@code StarryInitProvider}；签名侧由本作既有 {@code SigReplace} 链路负责。
 *
 * <p><b>前置条件</b>：{@code app/proguard-rules.pro} 与 {@code app/inject-dex-rules.pro}
 * 都含 {@code -keepnames class com.starry.magic.games.**}（框架收口既定，两文件均已具备）。
 */
public final class GameHookImpl implements GameHook {

    /** 无限升级 TapTap 官方包名（13.101 / vc1650 实测 manifest package）。 */
    private static final String PKG = "com.xingzuodaoshi.infinityUpgrade";

    /** {@code Activity.onCreate} 之前：把登录请求改成内嵌 Web（TapSDK 3.x handler / 4.x fragmentProvider）。 */
    @Override
    public void onActivityPreCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onPreCreated(activity);
    }

    /** {@code Activity.onCreate} 之后：兜底走 SDK 自带 {@code fallbackToWeb(request)}。 */
    @Override
    public void onActivityCreated(Activity activity) {
        if (!isTarget(activity)) return;
        TapTapWebLogin.onCreated(activity);
    }

    /** SPI 契约：独立助手形态据此定位目标包。 */
    @Override
    public String targetPackage() {
        return PKG;
    }

    /** 自己的包自己判断：别的游戏加载到本类时不得产生副作用。 */
    private static boolean isTarget(Activity activity) {
        return activity != null && PKG.equals(activity.getPackageName());
    }
}
