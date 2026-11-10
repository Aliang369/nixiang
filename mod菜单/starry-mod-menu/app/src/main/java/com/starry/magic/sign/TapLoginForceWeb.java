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
 * <b>2026-10-28 状态</b>：本类属**旧机制**，只服务"还没迁移到改包法"的老包；新包一律按
 * {@code 10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md} 改游戏 dex
 * （门闩恒放行 + 登录入口恒 Web）。同日起**运行时签名伪装 {@code SignSpoof} 已整体删除**，
 * 下面各游戏注释里提到的「SignSpoof 回灌证书 / killPm / SignSpoofAppComponentFactory 早安装」
 * 均为**历史原因记录**，不再有对应实现；需要"签名一致"能力的包必须走改包法。
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
    private static final String PHONE_HANDLER =
            "com.taptap.sdk.login.internal.handlers.phone.PhoneLoginHandler";
    private static final String LOGIN_HANDLER_IFACE =
            "com.taptap.sdk.login.internal.handlers.LoginHandler";
    private static final String REQUEST_EXTRA = "com_taptap_sdk_login_request";
    private static final String FRAGMENT_TAG = "com_taptap_sdk_login_fragment";
    private static final String WEB_LOGIN_FRAGMENT =
            "com.taptap.sdk.login.impl.ui.fragment.WebLoginFragment";
    private static final String OLD_WEB_LOGIN_FRAGMENT =
            "com.taptap.sdk.login.internal.handlers.web.WebLoginFragment";
    private static final String OLD_FRAGMENT_TAG = "login_fragment";

    /**
     * <b>本类不再有任何包名白名单</b>（2026-11-11）。
     *
     * <p>为什么删：白名单是「该不该生效」的第二套判据，与 {@code games/<id>/GameHookImpl}
     * 这套 SPI 并存时必然互相打架 —— 旧版文档写「本类已停用」，白名单里却还列着本包名，
     * 模型只能靠人提醒才知道走哪条路。现在判据唯一：<b>谁的 GameHookImpl 调了本类，谁就生效</b>。
     *
     * <p>调用方负责判断目标（见 {@code games/yntt/GameHookImpl.isTarget()}）。
     * 每次改包都是全新流程，不存在需要兼容的旧包。
     */

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
            Field reqF = null;
            for (String name : new String[]{"loginRequest", "request"}) {
                try {
                    reqF = activity.getClass().getDeclaredField(name);
                    break;
                } catch (NoSuchFieldException ignored) {
                }
            }
            if (reqF == null) {
                Log.w(TAG, "old-impl request field missing");
                return;
            }
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
            // 新版 TapSDK（4.7+）：loginHandlers = {CloudLoginHandler, ClientLoginHandler, PhoneLoginHandler}。
            // Client 在设备装了 TapTap 客户端时 canHandle 恒 true（SSO 校验真签必失败）；Phone 快登同理。
            // 两者都换成 canHandle 恒 false 的 LoginHandler 代理（不动 Cloud：非云游戏它自己 falsy），
            // onCreate 的遍历一个都不命中 → 落 SDK 自带 fallbackToWeb（WebLoginFragment）。
            // 接口类不可用（更老的 SDK）时退回原来的 CloudLoginHandler 替换路。
            Object noop = falseProxy(activity.getClassLoader());
            boolean touched = false;
            for (int i = 0; i < handlers.length; i++) {
                Object h = handlers[i];
                if (h == null) continue;
                String cn = h.getClass().getName();
                if (PHONE_HANDLER.equals(cn) && noop != null) {
                    handlers[i] = noop;
                    touched = true;
                    continue;
                }
                if (!CLIENT_HANDLER.equals(cn)) continue;
                handlers[i] = noop != null
                        ? noop
                        : Class.forName(CLOUD_HANDLER).getDeclaredConstructor().newInstance();
                touched = true;
            }
            Log.i(TAG, "TapTapLoginActivity sanitized=" + touched + " noopProxy=" + (noop != null));
        } catch (Throwable t) {
            Log.e(TAG, "sanitize failed", t);
        }
    }

    /**
     * canHandle 恒 false 的 LoginHandler 代理：不依赖具体 handler 实现类，只认接口。
     * 接口不可用 / 代理创建失败时返回 null，调用方退回 CloudLoginHandler 替换路。
     */
    private static Object falseProxy(ClassLoader cl) {
        if (cl == null) return null;
        try {
            Class<?> itf = Class.forName(LOGIN_HANDLER_IFACE, false, cl);
            if (!itf.isInterface()) {
                Log.w(TAG, LOGIN_HANDLER_IFACE + " is not an interface");
                return null;
            }
            return java.lang.reflect.Proxy.newProxyInstance(cl, new Class<?>[]{itf},
                    (proxy, method, args) -> {
                        String name = method.getName();
                        if ("toString".equals(name)) return "StarryNoopLoginHandler";
                        if ("hashCode".equals(name)) return System.identityHashCode(proxy);
                        if ("equals".equals(name)) {
                            return args != null && args.length > 0 && proxy == args[0];
                        }
                        Class<?> rt = method.getReturnType();
                        if (rt == boolean.class || rt == Boolean.class) return Boolean.FALSE;
                        if (rt == int.class) return 0;
                        if (rt == long.class) return 0L;
                        if (rt == short.class) return (short) 0;
                        if (rt == byte.class) return (byte) 0;
                        if (rt == char.class) return (char) 0;
                        if (rt == float.class) return 0f;
                        if (rt == double.class) return 0d;
                        return null;
                    });
        } catch (Throwable t) {
            Log.w(TAG, "LoginHandler proxy unavailable", t);
            return null;
        }
    }
}
