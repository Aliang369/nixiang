package com.starry.magic.core;

import android.app.Activity;

/**
 * 【编译期 Stub —— 绝不进 dex / 不进 APK】
 *
 * 为什么需要：本仓 HEAD(946f44f4) 的 app/src/main/java/com/starry/magic/games/wxsj/GameHookImpl.java
 * 引用了 com.starry.magic.core.TapTapWebLogin（文件首行 import + onPreCreated/onCreated 调用），
 * 但该类在整仓任何位置都不存在（git log 无历史、工作区无未跟踪文件）⇒ javac/gradle 均编译失败。
 * 这是**框架级缺陷**，会影响所有后续游戏的菜单 dex 构建。
 *
 * 处置（Lead 2026-11-19 裁决 = 方案 A）：本类只在编译期作为 classpath 提供，编译产物**不打包进 jar/dex**；
 * 冒险战争(mxzz) 运行期永远不会走到 wxsj 的 GameHook 路径 ⇒ 缺失不影响本作。
 * 建议 mac 侧补真实实现入库后删除本目录。
 */
public final class TapTapWebLogin {
    private TapTapWebLogin() {}

    public static void onPreCreated(Activity activity) { }

    public static void onCreated(Activity activity) { }
}
