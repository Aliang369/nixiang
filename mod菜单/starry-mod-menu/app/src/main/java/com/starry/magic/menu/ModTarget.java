package com.starry.magic.menu;

import android.content.Context;

/**
 * 组装目标常量 —— <b>运行时解析</b>，不再写死任何具体游戏。
 *
 * <p>历史问题：本类曾把 {@code GAME_PKG} 写死成 {@code com.nekki.shadowfight3}（暗影格斗3）。
 * 于是每一次换游戏都必须手工改这个共用文件 —— 正是「一款游戏的改动要动框架」的典型。
 * 更早还写死过 {@code cn.alltune.endless}（邂逅在迷宫），导致助手往别的游戏目录写状态文件。
 *
 * <p>现在的口径：
 * <ul>
 *   <li><b>注入形态</b>（菜单跑在游戏进程内）：{@link #gamePackage(Context)} 直接返回
 *       {@code ctx.getPackageName()}。零配置、零改文件，换游戏不用碰本类。</li>
 *   <li><b>独立助手形态</b>（免 root 直装，助手=自己的包名）：目标游戏包名由
 *       {@link com.starry.magic.core.GameHooks} 的实现类提供（{@code targetPackage()}），
 *       没有实现类时返回空串 ⇒ 走「无外部目录」分支，不会误写别人的目录。</li>
 * </ul>
 */
public final class ModTarget {

    /** 独立助手（免 root 直装形态）的包名，与应用自身包名一致。 */
    public static final String HELPER_PKG = "com.starry.magic";

    private static final String EXTERNAL_ROOT = "/storage/emulated/0";

    private ModTarget() {}

    /**
     * 当前上下文里「游戏」的包名。
     * <p>注入形态 = 自身包名；助手形态 = 显式配置的目标包名（可能为空）。
     */
    public static String gamePackage(Context ctx) {
        if (ctx == null) return "";
        try {
            String self = ctx.getPackageName();
            if (!HELPER_PKG.equals(self)) return self;   // 注入形态：自身就是游戏
        } catch (Throwable ignored) {
            return "";
        }
        // 助手形态：向 game hook 要目标包名（没实现可为空）
        try {
            String t = com.starry.magic.core.GameHooks.get().targetPackage();
            return t == null ? "" : t.trim();
        } catch (Throwable ignored) {
            return "";
        }
    }

    /**
     * 助手写、游戏读的状态文件候选目录（按优先级）。
     * <p>顺序理由见 {@link com.starry.magic.menu.StateMirror}：Android 11+ 的 FUSE 只对
     * {@code Android/data} 与 {@code Android/obb} 做跨包隔离，{@code Android/media/<pkg>}
     * 不受限，所以 media 优先、data 次之。目标包名为空时返回空数组。
     */
    public static String[] externalDirs(Context ctx) {
        String pkg = gamePackage(ctx);
        if (pkg.isEmpty()) return new String[0];
        return new String[] {
                EXTERNAL_ROOT + "/Android/media/" + pkg,
                EXTERNAL_ROOT + "/Android/data/" + pkg + "/files",
        };
    }
}
