package com.starry.magic.menu;

import android.content.Context;
import android.content.SharedPreferences;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

/**
 * 注入事件流水（写在 App 私有 SharedPreferences，跨进程重启保留）。
 *
 * <p>背景：玩家拿不到 logcat，我们也看不到别人的手机。所以「上一次注入到底走到哪一步」
 * 必须留在本机，由 {@link Diag} 显示在「体检」界面上 —— 玩家拍屏或点「复制」发回来即可定性。
 *
 * <p>全部写入都吞异常：体检是旁观者，绝不能拖垮注入本身。
 */
public final class InjectJournal {

    /** 点了 ③（注入按钮） */
    public static final String K_CLICK = "click";
    /** 被权限闸门拦下 */
    public static final String K_BLOCKED = "blocked";
    /** 已把游戏拉起来 */
    public static final String K_LAUNCH = "launch";
    /** 注入通道开始建立 */
    public static final String K_CH_START = "ch_start";
    /** VPN 隧道真的建起来了 */
    public static final String K_CH_UP = "ch_up";
    /** 通道关闭（自动/手动） */
    public static final String K_CH_CLOSE = "ch_close";
    /** establish() 返回 null */
    public static final String K_VPN_NULL = "vpn_null";
    /** 通道被别的 VPN 抢占 */
    public static final String K_REVOKE = "revoke";
    /** 游戏来取过补丁文件（任何大小） */
    public static final String K_FETCH = "fetch";
    /** 游戏来取过补丁 DLL（那个大 bundle） */
    public static final String K_BUNDLE = "bundle";
    /** 最近一次失败原因 */
    public static final String K_ERR = "err";

    /** 补丁 DLL bundle 的判据：比它小的都是小文件 */
    private static final long BUNDLE_MIN_BYTES = 1000L * 1000L;

    private static final String PREF = "starry_health";

    private InjectJournal() {
    }

    private static SharedPreferences prefs(Context ctx) {
        if (ctx == null) {
            return null;
        }
        Context app = ctx.getApplicationContext();
        return (app != null ? app : ctx).getSharedPreferences(PREF, Context.MODE_PRIVATE);
    }

    /** 记一条事件（时间 + 可选说明）。 */
    public static void note(Context ctx, String key, String value) {
        try {
            SharedPreferences p = prefs(ctx);
            if (p == null) {
                return;
            }
            SharedPreferences.Editor e = p.edit()
                    .putLong(key + "_ts", System.currentTimeMillis());
            if (value != null) {
                e.putString(key + "_val", value);
            }
            e.apply();
        } catch (Throwable ignored) {
        }
    }

    public static void note(Context ctx, String key) {
        note(ctx, key, null);
    }

    /** 新一轮注入开始（玩家点 ③）：清掉上一轮的过程事件，避免旧时间戳误导。 */
    public static void newSession(Context ctx) {
        try {
            SharedPreferences p = prefs(ctx);
            if (p == null) {
                return;
            }
            SharedPreferences.Editor e = p.edit();
            String[] keys = {K_BLOCKED, K_CH_START, K_CH_UP, K_CH_CLOSE, K_VPN_NULL,
                    K_REVOKE, K_FETCH, K_BUNDLE, K_ERR};
            for (String k : keys) {
                e.remove(k + "_ts").remove(k + "_val");
            }
            e.putInt(K_FETCH + "_count", 0);
            e.apply();
        } catch (Throwable ignored) {
        }
    }

    /** 游戏这次来取了哪个文件：记名字+大小，并累计次数。 */
    public static void served(Context ctx, String name, long bytes) {
        String what = name + "（" + bytes + " B）";
        note(ctx, K_FETCH, what);
        bump(ctx, K_FETCH);
        if (bytes >= BUNDLE_MIN_BYTES) {
            note(ctx, K_BUNDLE, what);
        }
    }

    public static void bump(Context ctx, String key) {
        try {
            SharedPreferences p = prefs(ctx);
            if (p == null) {
                return;
            }
            p.edit().putInt(key + "_count", p.getInt(key + "_count", 0) + 1).apply();
        } catch (Throwable ignored) {
        }
    }

    public static long ts(Context ctx, String key) {
        SharedPreferences p = prefs(ctx);
        return p == null ? 0L : p.getLong(key + "_ts", 0L);
    }

    public static String val(Context ctx, String key) {
        SharedPreferences p = prefs(ctx);
        return p == null ? null : p.getString(key + "_val", null);
    }

    public static int count(Context ctx, String key) {
        SharedPreferences p = prefs(ctx);
        return p == null ? 0 : p.getInt(key + "_count", 0);
    }

    /** MM-dd HH:mm:ss，跨天也看得出先后。 */
    public static String fmt(long ts) {
        if (ts <= 0L) {
            return "—";
        }
        return new SimpleDateFormat("MM-dd HH:mm:ss", Locale.US).format(new Date(ts));
    }
}
