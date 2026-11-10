package com.starry.magic.core.legacy;

import android.app.Activity;

import com.starry.magic.core.GameHook;
import com.starry.magic.sign.TapLoginForceWeb;

/**
 * <b>存量兼容垫片（只减不增）</b> —— 把历史上的 TapTap 强制内嵌网页登录从框架生命周期里
 * 搬出来的过渡层。
 *
 * <h3>现状与目标</h3>
 * 2026-11-11 之前，{@code StarryApp} / {@code StarryInitProvider} 的 Activity 回调
 * <b>无条件</b>调用 {@link TapLoginForceWeb#onPreCreated} / {@link TapLoginForceWeb#onCreated}，
 * 由它内部 47 项包名白名单自检。问题是：这是<b>框架生命周期</b>里的单游戏逻辑，新游戏
 * 一上来就继承全部历史补丁，正是「一款游戏的改动残留到后来的游戏」。
 *
 * <p>目标形态：新游戏一律通过自己的 {@code games/<id>/GameHookImpl} 显式决定要不要这套补丁。
 * 但存量 20+ 款已交付游戏没有 GameHookImpl，直接摘掉会回归（TapTap 改签包登录必失败），
 * 所以保留本清单作为过渡。
 *
 * <h3>铁律（机器校验）</h3>
 * <ul>
 *   <li><b>禁止新增</b>：{@code 04_scripts/_common/check_framework_isolation.sh} 把本文件的
 *       条目数与基线比对，变多即 FAIL。</li>
 *   <li>新游戏要这套补丁 → 在 {@code games/<id>/GameHookImpl} 里自己调
 *       {@link TapLoginForceWeb}，<b>不要</b>加到本清单。</li>
 *   <li>某游戏迁移到 GameHookImpl 后，把它的 game_id 从这里删掉（只减）。</li>
 *   <li>全部迁移完 ⇒ 删除本文件与框架里的调用点。</li>
 * </ul>
 *
 * <p>注意：清单命中的游戏仍由 {@link TapLoginForceWeb} 内部按<b>包名</b>二次自检，
 * 所以这里写的是 game_id（native 烘焙值），两者不一致时以包名为准 —— 不会误伤。
 */
public final class LegacyTapTapGames {

    /**
     * 仍在用旧机制的游戏 game_id。
     * <p>⚠️ 只减不增。迁移到 GameHookImpl 后请删除对应行。
     */
    private static final java.util.Set<String> GAME_IDS = new java.util.HashSet<>(
            java.util.Arrays.asList(
                    // 以下为 2026-11-11 时点上已交付且依赖 TapTap 强制网页登录的游戏。
                    // 迁移一个删一个。
                    "cwjh",    // 超维进化（smali 层 CwjhTapGate 为主，此路径为反射备用）
                    "zjmc",    // 追迹迷城
                    "mrfzzmd", // 末地（终末地）
                    "wdyz",    // 我的勇者
                    "phigros", // Phigros
                    "oxmhj2",  // 偶像梦幻祭2
                    "xxh2",    // 消消英雄2
                    "wxsj"     // 无限升级
            ));

    private LegacyTapTapGames() {}

    /** 该 game_id 是否仍走旧机制。 */
    public static boolean uses(String gameId) {
        return gameId != null && GAME_IDS.contains(gameId.trim());
    }

    /** 旧机制的 hook 实现；框架在 {@link com.starry.magic.core.GameHooks} 解析不到
     *  本游戏自有实现时才回退到这里。 */
    public static GameHook hook() {
        return new Bridge();
    }

    private static final class Bridge implements GameHook {
        @Override public void onActivityPreCreated(Activity a) {
            TapLoginForceWeb.onPreCreated(a);
        }
        @Override public void onActivityCreated(Activity a) {
            TapLoginForceWeb.onCreated(a);
        }
    }
}
