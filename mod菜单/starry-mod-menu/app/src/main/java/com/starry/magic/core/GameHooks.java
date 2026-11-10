package com.starry.magic.core;

import android.util.Log;

import com.starry.magic.menu.Preferences;

/**
 * 单游戏实现的反射装载器 —— 框架层与 {@code com.starry.magic.games.*} 之间唯一的通道。
 *
 * <p>设计要点（为什么不是直接写 {@code new DwctIapGate()}）：
 * <ul>
 *   <li><b>隔离</b>：框架类不 import 任何具体游戏类，编译期依赖为零；注入脚本可以按
 *       game_id 只裁 {@code games/<id>/} 这一棵子树，其余游戏的实现类根本不进包。</li>
 *   <li><b>容错</b>：类不存在 / 方法签名不匹配一律静默降级（日志 WARN 一行），
 *       绝不因为「某游戏没写 hook」而让别的游戏崩。</li>
 *   <li><b>零配置</b>：实现类只按约定名 {@code games.{game_id}.GameHookImpl} 摆放，
 *       不需要注册表、不需要改框架、不需要改 Manifest。</li>
 * </ul>
 *
 * <p>解析结果缓存一次：{@code game_id} 在进程生命周期内不会变（native 烘焙值），
 * 而解析失败也要缓存，否则每个 Activity 回调都要走一次 Class.forName 抛异常。
 */
public final class GameHooks {

    private static final String TAG = "GameHooks";
    private static final String PKG_PREFIX = "com.starry.magic.games.";
    private static final String IMPL_SUFFIX = ".GameHookImpl";

    /** 哨兵：尚未解析。null 表示「已解析且没有实现」。 */
    private static volatile GameHook sHook;
    private static volatile String sResolvedFor;
    private static volatile boolean sResolved;

    private GameHooks() {}

    /**
     * 当前 game_id。
     *
     * <p><b>顺序很重要</b>（2026-11-11）：先问 native 烘焙值，再退回 prefs。
     * 原因是 hook 可能在 {@code Application.attachBaseContext} 阶段就要被装载，
     * 那时 prefs 里的 {@code auth_game_id} 在**首启**还是空的（它由 native 同步进来），
     * 若只读 prefs，首启那次单游戏补丁会静默不生效 —— 这类「第二次启动才好」的
     * 半死状态最难查。native 未加载时 getter 会抛 UnsatisfiedLinkError，被吞掉后退 prefs。
     */
    public static String currentGameId() {
        try {
            String id = com.starry.magic.bridge.ModBridge.nativeGetAuthGameIdPublic();
            if (id != null && !id.trim().isEmpty()) return id.trim();
        } catch (Throwable ignored) {
            // native 还没加载 / 老 so 没这个导出：退回 prefs
        }
        try {
            String id = Preferences.getAuthGameId();
            return id == null ? "" : id.trim();
        } catch (Throwable ignored) {
            // Preferences 尚未初始化（attachBaseContext 早期）：返回空，调用方按「无 hook」处理
            return "";
        }
    }

    /**
     * 取当前游戏的实现；没有就返回一个空实现（永不返回 null）。
     *
     * <p>线程安全：解析一次并缓存；并发调用最多重复解析一次，不会出错。
     */
    public static GameHook get() {
        GameHook h = sHook;
        if (h != null) return h;
        String gid = currentGameId();
        if (sResolved && gid.equals(sResolvedFor)) {
            return sHook != null ? sHook : NONE;
        }
        synchronized (GameHooks.class) {
            if (sResolved && gid.equals(sResolvedFor)) {
                return sHook != null ? sHook : NONE;
            }
            sHook = resolve(gid);
            sResolvedFor = gid;
            sResolved = true;
            return sHook != null ? sHook : NONE;
        }
    }

    /** 显式强制解析（测试 / 助手形态在 game_id 后到时可调用）。 */
    public static void invalidate() {
        synchronized (GameHooks.class) {
            sHook = null;
            sResolved = false;
            sResolvedFor = null;
        }
    }

    private static GameHook resolve(String gid) {
        // 0) 存量兼容垫片：旧机制游戏（只减不增，见 LegacyTapTapGames 注释）
        if (com.starry.magic.core.legacy.LegacyTapTapGames.uses(gid)) {
            Log.i(TAG, "game hook 走存量兼容垫片: " + gid);
            return com.starry.magic.core.legacy.LegacyTapTapGames.hook();
        }
        if (gid == null || gid.isEmpty()) return null;
        // game_id 只允许 [a-z0-9_]：防止 native 侧被改后反射到任意类
        for (int i = 0; i < gid.length(); i++) {
            char c = gid.charAt(i);
            boolean ok = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_';
            if (!ok) {
                Log.w(TAG, "非法 game_id，跳过 hook 解析: " + gid);
                return null;
            }
        }
        String fqcn = PKG_PREFIX + gid + IMPL_SUFFIX;
        try {
            Class<?> c = Class.forName(fqcn, true, GameHooks.class.getClassLoader());
            Object o = c.getDeclaredConstructor().newInstance();
            if (!(o instanceof GameHook)) {
                Log.w(TAG, fqcn + " 未实现 GameHook，忽略");
                return null;
            }
            Log.i(TAG, "game hook 已装载: " + fqcn);
            return (GameHook) o;
        } catch (ClassNotFoundException e) {
            // 常态：本游戏没写 hook
            Log.i(TAG, "game hook 未提供: " + fqcn);
        } catch (Throwable t) {
            Log.w(TAG, "game hook 装载失败: " + fqcn + " → " + t);
        }
        return null;
    }

    /** 空实现：所有方法都是默认空体。 */
    private static final GameHook NONE = new GameHook() {};
}
