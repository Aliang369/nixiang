package com.starry.magic.menu;

/**
 * 组装目标常量：这套「星光」外壳是所有游戏共用的，换游戏时只应该改这一个文件。
 *
 * <p>历史教训：早期把目标游戏包名（{@code cn.alltune.endless}）直接硬编码进了
 * {@link StateMirror} 的外部目录数组。结果是拿这套壳去装别的游戏时，助手仍然会往
 * 「邂逅在迷宫」的目录里写状态文件 —— 既写不到正确位置，又可能误改另一个游戏。
 * 现在统一从这里取值，杜绝散落各处的字符串字面量。
 *
 * <p>两种运行形态（互斥，靠包名区分）：
 * <ul>
 *   <li><b>注入形态</b>：菜单跑在<b>游戏进程</b>里，助手包的 applicationId 就是游戏包名
 *       （{@link #GAME_PKG}）。此时 {@link MenuService} 能拿到宿主 Activity，
 *       走 ACTIVITY 挂载路径。</li>
 *   <li><b>独立助手形态（免 root 直装）</b>：助手是单独安装的普通应用，包名固定为
 *       {@link #HELPER_PKG}。它拿不到游戏进程的 Activity，只能走 OVERLAY 悬浮窗 +
 *       外部状态文件通道。</li>
 * </ul>
 */
public final class ModTarget {

    /** 目标游戏包名。组装配新游戏时只改这一行。 */
    public static final String GAME_PKG = "cn.alltune.endless";

    /** 独立助手（免 root 直装形态）的包名，与 {@link #GAME_PKG} 必须不同。 */
    public static final String HELPER_PKG = "com.starry.magic";

    /** 外部存储根，两个候选目录都挂在这下面。 */
    private static final String EXTERNAL_ROOT = "/storage/emulated/0";

    /**
     * 助手写、游戏读的状态文件候选目录（按优先级）。
     *
     * <p>顺序理由见 {@link StateMirror} 类注释：Android 11+ 的 FUSE 只对
     * {@code Android/data} 与 {@code Android/obb} 做跨包隔离，{@code Android/media/<pkg>}
     * 不受限，所以 media 优先、data 次之。
     */
    public static final String[] EXTERNAL_DIRS = {
            EXTERNAL_ROOT + "/Android/media/" + GAME_PKG,
            EXTERNAL_ROOT + "/Android/data/" + GAME_PKG + "/files",
    };

    private ModTarget() {
    }
}
