package com.starry.magic.sign;

import android.app.Activity;
import android.content.Intent;
import android.os.Build;
import android.os.Parcelable;
import android.util.Log;

import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;

/**
 * TapTap 强制内嵌网页登录（纯运行时反射，不改 dex —— ACE 校验官方 dex，字节级修改即闪退）。
 * <p>
 * 旧版 TapSDK（终末地 4.7.0 内嵌版，loginHandlers 数组路由）：
 * {@code TapTapLoginActivity.<init>} new {@code LoginActivityImpl}，其
 * {@code loginHandlers} 固定为 [CloudLoginHandler, ClientLoginHandler]；onCreate 遍历
 * canHandle，全不命中 → fallbackToWeb（WebLoginFragment）。窗口期把数组里
 * ClientLoginHandler 换 CloudLoginHandler 实例（非云游戏 canHandle=false）→ 强制网页。
 * <p>
 * 新版 TapSDK（无尽探险队 tap-login_release 版）：
 * {@code TapTapLoginActivity.onCreate} 读 intent extra request，首建（savedInstanceState==null）
 * 即 {@code fragmentProvider.provide(request)} → {@code DefaultTapLoginFragmentProvider}
 * 按 {@code LoginRequest.loginType} 分流（"mobile_client"→ClientLoginFragment /
 * "embedded_web"→WebLoginFragment / 云端→CloudGameFragment）。SDK 自带
 * {@code fallbackToWeb(request)}（copy request 置 loginType=WEB 后 replace）。
 * 强制网页 = 两路：
 *   ① onActivityPreCreated（onCreate 前）：反射改 intent extra 的 LoginRequest.loginType
 *      = "embedded_web"，让首建 provide 直接走 WebLoginFragment（不拉起 TapTap 客户端）。
 *   ② onActivityCreated 兜底：当前 fragment 非 WebLoginFragment 时反射调 fallbackToWeb。
 */
public final class TapLoginForceWeb {
    private static final String TAG = "StarryTapLogin";

    private static final String LOGIN_ACTIVITY = "com.taptap.sdk.login.ui.TapTapLoginActivity";
    private static final String LOGIN_ACTIVITY_IMPL =
            "com.taptap.sdk.login.impl.ui.activity.TapTapLoginActivity";
    private static final String OLD_LOGIN_ACTIVITY = "com.taptap.sdk.ui.TapTapActivity";
    private static final String OLD_REQUEST_EXTRA = "request";
    private static final String OLD_REQUEST_CLASS =
            "com.starry.magic.sign.YnttLoginRequest";
    private static final String CLIENT_HANDLER =
            "com.taptap.sdk.login.internal.handlers.client.ClientLoginHandler";
    private static final String CLOUD_HANDLER =
            "com.taptap.sdk.login.internal.handlers.cloud.CloudLoginHandler";
    private static final String REQUEST_EXTRA = "com_taptap_sdk_login_request";
    private static final String FRAGMENT_TAG = "com_taptap_sdk_login_fragment";
    private static final String WEB_LOGIN_FRAGMENT =
            "com.taptap.sdk.login.impl.ui.fragment.WebLoginFragment";
    private static final String OLD_WEB_LOGIN_FRAGMENT =
            "com.taptap.sdk.login.internal.handlers.web.WebLoginFragment";
    private static final String OLD_FRAGMENT_TAG = "login_fragment";

    /** 包名白名单：只对这些游戏的 Tap 登录强制网页，其他星空包游戏不受影响。 */
    private static final java.util.Set<String> PKG_WHITELIST =
            new java.util.HashSet<>(java.util.Arrays.asList(
                    "com.hypergryph.endfield",
                    "com.rsg.wdyz",
                    // 超维进化 TapSDK 4.9.3：路由为 LoginTypes.from(ClientLoginUtils.isClientLogin)
                    // → DefaultTapLoginFragmentProvider 三分支，无 loginHandlers 数组（终末地 4.7.0 那套不存在）。
                    // 4.9.3 的强制网页走 CwjhTapGate（smali 层 isClientLogin 恒 false + gatekeeper 放行），
                    // 这里保留白名单以便将来同构版本复用反射方案。
                    "com.grinning.beta.yszl",
                    // 无尽探险队：TapSDK 新版（tap-login_release），见类头说明。
                    "com.wjtxdtt.zy",
                    // 凡尘西游修真记：TapSDK 4.10.7（TapLogin）。
                    // 2026-09-13 核验包内：TapTapLoginActivity.EXTRAS_REQUEST=com_taptap_sdk_login_request、
                    // model/LoginRequest.loginType、LoginTypes$WEB=embedded_web、fallbackToWeb(LoginRequest)、
                    // DefaultTapLoginFragmentProvider → WebLoginFragment 全部存在，走新版反射路径。
                    // 不加白名单时实测走 mobile_client（拉起 TapTap 客户端 SSO），改签包必失败。
                    "com.qxjc.fcxyxzj",
                    // 我的修仙世界：TapSDK 4.7.0，LoginActivityImpl.loginHandlers
                    // = [CloudLoginHandler, ClientLoginHandler]，走旧版 handler 清洗路径。
                    "com.lingsi.xiuxiandanji",
                    // 大话蜀山：TapSDK 4.9.0，TapTapLoginActivity 按 LoginRequest.loginType
                    // 分流（DefaultTapLoginFragmentProvider）；preCreated 改 embedded_web 即直接进网页。
                    "com.cylj.rogue.tp479",
                    // 正中靶心2：TapSDK 新版，TapTapLoginActivity 按 LoginRequest.loginType
                    // 分流；改签后直接走 embedded_web，避免客户端 SSO 校验真实签名。
                    "com.openew.monster",
                    // 一念通天：TapSDK + TapTap 登录；改签包固定走 embedded_web。
                    "com.qinglv.Immortal",
                    // 偶像梦幻祭2：TapTap 渠道原版；存在 TapTap 登录入口时固定走 embedded_web。
                    "com.happyelements.es2",
                    // 燃烧纪元：TapSDK modern 路径（TapTapLoginActivity、LoginRequest.loginType、
                    // fallbackToWeb），改签包固定 embedded_web，避免客户端 SSO 校验真签。
                    "com.rsjy.yf",
                    // 宝石战争：TapTap 渠道原版；改签包固定走 embedded_web，避免客户端 SSO 校验真签。
                    "com.acing.gow.adv"));

    private TapLoginForceWeb() {}

    /** preCreated（onCreate 之前）：新版改 request.loginType，旧版清洗 loginHandlers。 */
    public static void onPreCreated(Activity activity) {
        if (Build.VERSION.SDK_INT >= 29) {
            forceOldImplWebEarly(activity);
            prepareNewImplRequest(activity);
            sanitize(activity);
        }
    }

    /** onCreated（onCreate 之后）：新版 fallbackToWeb 兜底，旧版 API&lt;29 清洗。 */
    public static void onCreated(Activity activity) {
        forceOldImplWeb(activity);
        forceNewImplWeb(activity);
        if (Build.VERSION.SDK_INT < 29) sanitize(activity);
    }

    /**
     * 旧版 com.taptap.sdk.ui.TapTapActivity：SDK 3.30.2 没有 loginType 分流。
     * onCreate 内会同步执行 LoginClient.sendLoginRequest，客户端已安装时必然先走
     * ActivityHandler。这里在 onCreate 前替换 request 为 YnttLoginRequest；其
     * getRequestCode() 抛 Exception，会被 LoginClient 捕获并回落官方 WebViewHandler。
     */
    private static void forceOldImplWebEarly(Activity activity) {
        if (activity == null || !OLD_LOGIN_ACTIVITY.equals(activity.getClass().getName())) return;
        if (!PKG_WHITELIST.contains(activity.getPackageName())) return;
        try {
            Intent it = activity.getIntent();
            if (it == null) return;
            Object original = it.getParcelableExtra(OLD_REQUEST_EXTRA);
            if (original == null) {
                Log.i(TAG, "old-impl no request extra");
                return;
            }
            if (OLD_REQUEST_CLASS.equals(original.getClass().getName())) {
                Log.i(TAG, "old-impl request already forced");
                return;
            }
            Class<?> forcedClass = Class.forName(OLD_REQUEST_CLASS);
            Constructor<?> ctor = forcedClass.getConstructor(String.class, String[].class);
            Object state = invokeGetter(original, "getState");
            Object permissions = invokeGetter(original, "getPermissions");
            Object forced = ctor.newInstance(state, permissions);
            copyStringProperty(original, forced, "getCodeChallenge", "setCodeChallenge");
            copyStringProperty(original, forced, "getCodeChallengeMethod", "setCodeChallengeMethod");
            copyStringProperty(original, forced, "getInfo", "setInfo");
            copyStringProperty(original, forced, "getLoginVersion", "setLoginVersion");
            copyStringProperty(original, forced, "getPhoneVerifyToken", "setPhoneVerifyToken");
            copyStringProperty(original, forced, "getPreferredLoginType", "setPreferredLoginType");
            copyStringProperty(original, forced, "getRedirectUri", "setRedirectUri");
            copyStringProperty(original, forced, "getResponseType", "setResponseType");
            copyStringProperty(original, forced, "getVersionCode", "setVersionCode");
            copyBooleanProperty(original, forced, "isPreApproved", "setPreApproved");
            it.putExtra(OLD_REQUEST_EXTRA, (Parcelable) forced);
            Log.i(TAG, "old-impl request forced -> client throws, web fallback");
        } catch (Throwable t) {
            Log.e(TAG, "forceOldImplWebEarly failed", t);
        }
    }

    private static Object invokeGetter(Object target, String name) throws Exception {
        Method method = target.getClass().getMethod(name);
        return method.invoke(target);
    }

    private static void copyStringProperty(Object from, Object to, String getter, String setter)
            throws Exception {
        Object value = invokeGetter(from, getter);
        if (value == null) return;
        to.getClass().getMethod(setter, String.class).invoke(to, (String) value);
    }

    private static void copyBooleanProperty(Object from, Object to, String getter, String setter)
            throws Exception {
        Object value = invokeGetter(from, getter);
        if (value == null) return;
        to.getClass().getMethod(setter, Boolean.TYPE).invoke(to, (Boolean) value);
    }

    /** 新版路径①：onCreate 前改 intent extra 的 LoginRequest.loginType → embedded_web。 */
    private static void prepareNewImplRequest(Activity activity) {
        if (activity == null || !LOGIN_ACTIVITY_IMPL.equals(activity.getClass().getName())) return;
        if (!PKG_WHITELIST.contains(activity.getPackageName())) return;
        try {
            Intent it = activity.getIntent();
            if (it == null) return;
            Object req = it.getParcelableExtra(REQUEST_EXTRA);
            if (req == null) {
                Log.i(TAG, "new-impl no request extra (retry via created)");
                return;
            }
            Field lt = req.getClass().getDeclaredField("loginType");
            lt.setAccessible(true);
            String cur = (String) lt.get(req);
            if (!"embedded_web".equals(cur)) {
                lt.set(req, "embedded_web");
                Log.i(TAG, "new-impl request loginType " + cur + " -> embedded_web (preCreated)");
            }
        } catch (Throwable t) {
            Log.e(TAG, "prepareNewImplRequest failed (created fallback will run)", t);
        }
    }

    /** 新版路径②：onCreate 后若当前 fragment 非 Web，反射调 fallbackToWeb(request)。 */
    private static void forceNewImplWeb(Activity activity) {
        if (activity == null || !LOGIN_ACTIVITY_IMPL.equals(activity.getClass().getName())) return;
        if (!PKG_WHITELIST.contains(activity.getPackageName())) return;
        try {
            Field reqF = activity.getClass().getDeclaredField("request");
            reqF.setAccessible(true);
            final Object req = reqF.get(activity);
            if (req == null) {
                Log.w(TAG, "new-impl request field null");
                return;
            }
            Object fm = activity.getFragmentManager();
            if (fm != null) {
                Method find = fm.getClass().getMethod("findFragmentByTag", String.class);
                Object f = find.invoke(fm, FRAGMENT_TAG);
                String cn = f != null ? f.getClass().getName() : null;
                if (WEB_LOGIN_FRAGMENT.equals(cn)) {
                    Log.i(TAG, "new-impl already WebLoginFragment, skip");
                    return;
                }
                Log.i(TAG, "new-impl fragment=" + cn + " → fallbackToWeb");
            }
            Method fb = activity.getClass().getMethod("fallbackToWeb", req.getClass());
            fb.invoke(activity, req);
        } catch (Throwable t) {
            Log.e(TAG, "forceNewImplWeb failed", t);
        }
    }

    /** 旧版兜底：onCreate 后若当前 fragment 非 WebLoginFragment，直接调 impl.fallbackToWeb。 */
    private static void forceOldImplWeb(Activity activity) {
        if (activity == null || !LOGIN_ACTIVITY.equals(activity.getClass().getName())) return;
        if (!PKG_WHITELIST.contains(activity.getPackageName())) return;
        try {
            android.app.FragmentManager fm = activity.getFragmentManager();
            if (fm != null) {
                android.app.Fragment f = fm.findFragmentByTag(OLD_FRAGMENT_TAG);
                if (f != null && OLD_WEB_LOGIN_FRAGMENT.equals(f.getClass().getName())) {
                    Log.i(TAG, "old-impl already WebLoginFragment, skip");
                    return;
                }
            }
            Field implF = activity.getClass().getDeclaredField("impl");
            implF.setAccessible(true);
            Object impl = implF.get(activity);
            Field reqF = activity.getClass().getDeclaredField("request");
            reqF.setAccessible(true);
            Object req = reqF.get(activity);
            if (impl == null || req == null) return;
            Method fb = impl.getClass().getMethod("fallbackToWeb", Activity.class, req.getClass());
            fb.invoke(impl, activity, req);
            Log.i(TAG, "old-impl fallbackToWeb invoked");
        } catch (Throwable t) {
            Log.e(TAG, "forceOldImplWeb failed", t);
        }
    }

    /** 旧版：loginHandlers 数组把 ClientLoginHandler 换成 CloudLoginHandler → 强制 fallbackToWeb。 */
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
