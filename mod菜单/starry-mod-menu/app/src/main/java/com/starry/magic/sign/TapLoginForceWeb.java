package com.starry.magic.sign;

import android.app.Activity;
import android.os.Build;
import android.util.Log;

import java.lang.reflect.Field;

/**
 * TapTap 强制内嵌网页登录（纯运行时反射，不改 dex —— ACE 校验官方 dex，字节级修改即闪退）。
 * <p>
 * TapSDK 4.7.0（终末地内嵌版）登录路由链路（smali 级实证）：
 * <ul>
 *   <li>{@code TapTapLoginActivity.<init>} 构造时即 new {@code LoginActivityImpl}，
 *       其 {@code loginHandlers} 固定为 [CloudLoginHandler, ClientLoginHandler]；
 *       {@code webHandler} 单独字段；</li>
 *   <li>{@code TapTapLoginActivity.onCreate}：super.onCreate → … → {@code impl.onCreate(this)}，
 *       后者遍历 {@code loginHandlers} 调 {@code canHandle}，命中即进对应 fragment；</li>
 *   <li>全部不命中时走 {@code fallbackToWeb(activity, request)} → webHandler →
 *       WebLoginFragment（内嵌网页登录）。</li>
 * </ul>
 * 时机：API 29+ {@code ActivityLifecycleCallbacks.onActivityPreCreated} 在
 * {@code performCreate} 内先于 {@code onCreate} 触发（onActivityCreated 在整个
 * onCreate 执行完之后才触发，太晚）。窗口内把数组中 ClientLoginHandler 换成
 * CloudLoginHandler 实例（非云游戏 canHandle=false）→ 全 false → 强制 fallbackToWeb。
 * TapSDK 为 app 自身类，同 classloader 反射无 hidden-API 限制。
 */
public final class TapLoginForceWeb {
    private static final String TAG = "StarryTapLogin";

    private static final String LOGIN_ACTIVITY = "com.taptap.sdk.login.ui.TapTapLoginActivity";
    private static final String CLIENT_HANDLER =
            "com.taptap.sdk.login.internal.handlers.client.ClientLoginHandler";
    private static final String CLOUD_HANDLER =
            "com.taptap.sdk.login.internal.handlers.cloud.CloudLoginHandler";

    /** 包名白名单：只对这些游戏的 Tap 登录强制网页，其他星空包游戏不受影响。 */
    private static final java.util.Set<String> PKG_WHITELIST =
            new java.util.HashSet<>(java.util.Arrays.asList(
                    "com.hypergryph.endfield",
                    "com.rsg.wdyz",
                    // 超维进化 TapSDK 4.9.3：路由为 LoginTypes.from(ClientLoginUtils.isClientLogin)
                    // → DefaultTapLoginFragmentProvider 三分支，无 loginHandlers 数组（终末地 4.7.0 那套不存在）。
                    // 4.9.3 的强制网页走 CwjhTapGate（smali 层 isClientLogin 恒 false + gatekeeper 放行），
                    // 这里保留白名单以便将来同构版本复用反射方案。
                    "com.grinning.beta.yszl"));

    private TapLoginForceWeb() {}

    /** preCreated（onCreate 之前）：handler 遍历尚未发生，主清洗窗口。 */
    public static void onPreCreated(Activity activity) {
        if (Build.VERSION.SDK_INT >= 29) sanitize(activity);
    }

    /** 兜底：API&lt;29 无 preCreated 回调（onActivityCreated 时遍历已过，多半无效但无害）。 */
    public static void onCreated(Activity activity) {
        if (Build.VERSION.SDK_INT < 29) sanitize(activity);
    }

    private static void sanitize(Activity activity) {
        if (activity == null || !LOGIN_ACTIVITY.equals(activity.getClass().getName())) return;
        if (!PKG_WHITELIST.contains(activity.getPackageName())) return;
        try {
            Field implF = activity.getClass().getDeclaredField("impl");
            implF.setAccessible(true);
            Object impl = implF.get(activity);
            if (impl == null) {
                Log.i(TAG, "impl not ready");
                return;
            }
            Field handlersF = impl.getClass().getDeclaredField("loginHandlers");
            handlersF.setAccessible(true);
            Object[] handlers = (Object[]) handlersF.get(impl);
            if (handlers == null || handlers.length == 0) {
                Log.i(TAG, "loginHandlers empty");
                return;
            }
            boolean touched = false;
            for (int i = 0; i < handlers.length; i++) {
                Object h = handlers[i];
                if (h == null || !CLIENT_HANDLER.equals(h.getClass().getName())) continue;
                handlers[i] = Class.forName(CLOUD_HANDLER).getDeclaredConstructor().newInstance();
                touched = true;
            }
            Log.i(TAG, "TapTapLoginActivity sanitized=" + touched);
        } catch (Throwable t) {
            Log.e(TAG, "sanitize failed", t);
        }
    }
}
