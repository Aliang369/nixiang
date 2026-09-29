package com.starry.magic.bridge;

import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.webkit.JavascriptInterface;

import com.starry.magic.auth.AuthClient;
import com.starry.magic.menu.FeatureListParser;
import com.starry.magic.menu.MenuService;
import com.starry.magic.menu.Preferences;
import com.starry.magic.menu.StateMirror;
import com.starry.magic.ota.PlayOtaPaths;

import java.io.File;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

/**
 * JS ↔ Java 桥接层
 *
 * HTML 通过 window.ModBridge.xxx() 调用。
 * featNum &gt;= 50 预留为纯 UI 项：只写 SharedPreferences，不进入 C 层 g_features/g_values。
 * 当前设置页为静态 HTML（声明/定制MOD/关闭菜单），不再使用 50+ 控件。
 */
public class ModBridge {

    private static final String TAG = "ModBridge";
    /** 预留：UI 项 featNum 起始，不写入 native 全局数组 */
    public static final int UI_FEAT_MIN = 50;

    static {
        ClassLoader pathCl = resolveAppClassLoader();
        boolean linked = false;
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof Context) {
                if (com.starry.magic.ota.StarryNativeLoad.ensureLoaded((Context) app)) {
                    linked = true;
                    Log.i(TAG, "native via StarryNativeLoad: " + com.starry.magic.ota.StarryNativeLoad.loadedPath());
                }
            }
        } catch (Throwable ignored) {
        }
        String[] cxx = new String[] { "/data/local/tmp/ltzj_st/libc++_shared.so" };
        for (String path : cxx) {
            if (!new java.io.File(path).isFile()) continue;
            if (loadIntoClassLoader(pathCl, path)) {
                Log.i(TAG, "libc++_shared System.load ok");
                break;
            }
        }
        if (!linked) {
            // 仍无 Application 时：有 OTA 文件则只 load OTA，绝不 loadLibrary
            try {
                Class<?> at = Class.forName("android.app.ActivityThread");
                Object app = at.getMethod("currentApplication").invoke(null);
                if (app instanceof Context) {
                    File ota = com.starry.magic.ota.StarryNativeLoad.findOtaSo((Context) app);
                    if (ota != null && loadIntoClassLoader(pathCl, ota.getAbsolutePath())) {
                        Log.i(TAG, "libstarry_magic OTA System.load ok: " + ota.getAbsolutePath());
                        linked = true;
                    }
                }
            } catch (Throwable ignored) {
            }
        }
        if (!linked) {
            String[] abs = new String[] { "/data/local/tmp/ltzj_st/libstarry_magic.so" };
            for (String path : abs) {
                if (!new java.io.File(path).isFile()) continue;
                if (loadIntoClassLoader(pathCl, path)) {
                    Log.i(TAG, "libstarry_magic System.load ok: " + path);
                    linked = true;
                    break;
                }
            }
        }
        if (!linked) {
            try {
                System.loadLibrary("starry_magic");
                Log.i(TAG, "libstarry_magic loaded");
            } catch (UnsatisfiedLinkError e) {
                Log.e(TAG, "loadLibrary starry_magic failed", e);
            }
        }
    }

    private static ClassLoader resolveAppClassLoader() {
        try {
            Class<?> at = Class.forName("android.app.ActivityThread");
            Object app = at.getMethod("currentApplication").invoke(null);
            if (app instanceof Context) {
                return ((Context) app).getClassLoader();
            }
        } catch (Throwable ignored) {
        }
        return ModBridge.class.getClassLoader();
    }

    private static boolean loadIntoClassLoader(ClassLoader cl, String path) {
        try {
            Class<?> linker = Class.forName("com.starry.magic.ltzj.NativeLinker", true, cl);
            linker.getMethod("load", String.class).invoke(null, path);
            return true;
        } catch (Throwable t) {
            Throwable c = t.getCause() != null ? t.getCause() : t;
            String msg = c.getMessage() != null ? c.getMessage() : String.valueOf(c);
            if (msg.contains("already opened") && msg.contains("PathClassLoader")) {
                return true;
            }
            Log.w(TAG, "System.load failed: " + msg);
            return false;
        }
    }




    private final Context context;
    private final Handler mainHandler = new Handler(Looper.getMainLooper());
    private final ExecutorService authExecutor = Executors.newSingleThreadExecutor();

    public static native void nativeInit(Context context);
    private static native void nativeSetFeature(int featNum, String featName, boolean enabled);
    private static native void nativeSetValue(int featNum, String featName, int value);
    private static native void nativeSetLongValue(int featNum, String featName, long value);
    private static native void nativeSetText(int featNum, String featName, String text);
    private static native void nativeExecButton(int featNum, String featName);
    private static native String[] nativeGetFeatureList();
    private static native String[] nativeGetSettingsList();
    private static native boolean nativeIsGameLibLoaded();
    private static native void nativeSetAuthorized(boolean authorized);
    private static native boolean nativeApplyGrant(String grant, String deviceId, String gameId);
    private static native boolean nativeHasLiveSession();
    private static native void nativeClearGrant();
    /** 编译期注入：GameConfig.h 由 STARRY_GAME_* 映射的 game_id */
    private static native String nativeGetAuthGameId();
    /** 雷霆过检门剩余秒；其它游戏恒 0 */
    private static native int nativeGetAceGateRemainSec();
    /** OTA manifest HMAC：payload = game|ver|jsonSha|soSha|note */
    public static native boolean nativeVerifyOtaSig(String payload, String sigHex);
    /** RegisterNatives 迁移后的注册结果自检（JNI_OnLoad 时置位；自测壳启动自检用） */
    public static native boolean nativeBridgeRegistered();
    /** 应用本地 patch.json（偏移等）；空串忽略 */
    public static native void nativeApplyPlayPatch(String json);
    /** so 内烘焙的证书固定公钥（PEM 字节）；AuthClient 专用，assets 路径已废弃 */
    public static native byte[] nativeGetPinnedCert();
    /** so 内烘焙的构建戳字面量 "STARRYSTAMP1:EPOCH=<秒>;"（OTA 守卫判新旧） */
    private static native String nativeGetBuildStamp();

    /** so 内烘焙的构建戳；老包或异常返回 null */
    public static String getBuildStamp() {
        try {
            return nativeGetBuildStamp();
        } catch (Throwable t) {
            return null;
        }
    }

    /** 用 so 内烘焙的 game_id 覆盖 prefs（唯一运行时真相） */
    public static void syncAuthGameIdFromNative() {
        try {
            String id = nativeGetAuthGameId();
            if (id != null && !id.isEmpty()) {
                Preferences.setAuthGameId(id);
                Log.i(TAG, "auth game_id synced from native: " + id);
            }
        } catch (UnsatisfiedLinkError e) {
            Log.w(TAG, "nativeGetAuthGameId unavailable", e);
        }
    }

    /** 过检测中倒计时剩余秒（0=可点玩法） */
    public static int getAceGateRemainSec() {
        try {
            return nativeGetAceGateRemainSec();
        } catch (UnsatisfiedLinkError e) {
            return 0;
        }
    }

    public ModBridge(Context context) {
        this.context = context.getApplicationContext();
        StateMirror.attach(this.context);
        Preferences.with(this.context);
        AuthClient.init(this.context);
        syncAuthGameIdFromNative();
        // 功能列表随 so 切换；丢掉旧 featNum 偏好，默认全关
        Preferences.clearFeaturePrefs();
        // 旧 HTTP 地址迁移到内置 HTTPS
        String base = Preferences.getAuthBaseUrl();
        if (base != null && base.startsWith("http://47.100.216.101")) {
            Preferences.setAuthBaseUrl(Preferences.DEFAULT_AUTH_BASE_URL);
        }
    }

    private static boolean isUiFeature(int featNum) {
        return featNum >= UI_FEAT_MIN;
    }

    /**
     * 菜单侧统一异常兼底：native 库缺失/符号缺失（UnsatisfiedLinkError/LinkageError）、
     * 偏好类型不符（ClassCastException）、静态初始化失败（ExceptionInInitializerError）
     * 等都只记日志，绝不冒泡到菜单 UI（防闪退）。
     */
    private static void safeCall(Runnable action) {
        try {
            action.run();
        } catch (Throwable e) {   // Exception + Error 全覆盖：Error 子类不冒泡才是真兼底
            Log.w(TAG, "menu state push failed: " + e);
        }
    }

    /** 功能项需 native session 活着（登录后不过期，直到进程重启） */
    private boolean requireAuthForFeature(int featNum) {
        if (isUiFeature(featNum)) return true;
        if (ModBridge.getAceGateRemainSec() > 0) {
            Log.w(TAG, "blocked feat #" + featNum + " — ACE gate active");
            return false;
        }
        // native session 活着就放行（后台单项开关只影响 UI 显示，不拦功能）
        if (hasLiveNativeSession()) return true;
        // 进程刚起来可能有残留 grant，尝试 sync
        syncNativeAuthorized();
        if (hasLiveNativeSession()) return true;
        Log.w(TAG, "blocked feat #" + featNum + " — no live session");
        return false;
    }

    private void syncNativeAuthorized() {
        try {
            String grant = Preferences.getAuthGrant();
            String device = Preferences.getOrCreateDeviceId();
            String game = Preferences.getAuthGameId();
            nativeApplyGrant(grant, device, game);
            // session_live() 只看 g_rt_ok，登录后永不过期
        } catch (UnsatisfiedLinkError e) {
            Log.w(TAG, "native grant APIs missing", e);
        } catch (Throwable e) {
            Log.w(TAG, "syncNativeAuthorized failed", e);
        }
    }

    /** Native 短票是否仍有效（供 Java 门控二次确认） */
    public boolean hasLiveNativeSession() {
        try {
            return nativeHasLiveSession();
        } catch (UnsatisfiedLinkError e) {
            return false;
        }
    }

    /** @return false = 未授权被拦（原生 UI 需回弹开关） */
    @JavascriptInterface
    public boolean onToggleChanged(int featNum, String featName, boolean enabled) {
        Log.i(TAG, "Toggle: #" + featNum + " " + featName + " = " + enabled);
        String spec = resolveSpec(featNum);
        // 误把滑块/输入等当开关：禁止写 boolean，按规格复位
        if (!isBoolFeatureSpec(spec)) {
            if (!enabled || !requireAuthForFeature(featNum)) {
                forceFeatureOff(featNum, featName, spec);
            }
            return false;
        }
        if (!requireAuthForFeature(featNum)) {
            if (!isUiFeature(featNum)) {
                forceFeatureOff(featNum, featName, spec);
            }
            return false;
        }
        safeCall(() -> Preferences.changeFeatureBool(featName, featNum, enabled));
        if (!isUiFeature(featNum)) {
            StateMirror.mirror(featNum, enabled ? 1 : 0);
            safeCall(() -> nativeSetFeature(featNum, featName, enabled));
        }
        return true;
    }

    /** @return false = 未授权被拦 */
    @JavascriptInterface
    public boolean onSliderChanged(int featNum, String featName, int value) {
        Log.i(TAG, "Slider: #" + featNum + " " + featName + " = " + value);
        if (!requireAuthForFeature(featNum)) {
            forceFeatureOff(featNum, featName, resolveSpec(featNum));
            return false;
        }
        safeCall(() -> Preferences.changeFeatureInt(featName, featNum, value));
        if (!isUiFeature(featNum)) {
            StateMirror.mirror(featNum, value);
            safeCall(() -> nativeSetValue(featNum, featName, value));
        }
        return true;
    }

    /** @return false = 未授权被拦 */
    @JavascriptInterface
    public boolean onButtonClicked(int featNum, String featName) {
        Log.i(TAG, "Button: #" + featNum + " " + featName);
        if (isUiFeature(featNum)) {
            return true;
        }
        if (!requireAuthForFeature(featNum)) return false;
        safeCall(() -> Preferences.changeFeatureInt(featName, featNum, 0));
        safeCall(() -> nativeExecButton(featNum, featName));
        return true;
    }

    /** @return false = 未授权被拦 */
    @JavascriptInterface
    public boolean onSpinnerChanged(int featNum, String featName, int position) {
        Log.i(TAG, "Spinner: #" + featNum + " " + featName + " = " + position);
        if (!requireAuthForFeature(featNum)) {
            forceFeatureOff(featNum, featName, resolveSpec(featNum));
            return false;
        }
        safeCall(() -> Preferences.changeFeatureInt(featName, featNum, position));
        if (!isUiFeature(featNum)) {
            StateMirror.mirror(featNum, position);
            safeCall(() -> nativeSetValue(featNum, featName, position));
        }
        return true;
    }

    /** @return false = 未授权被拦 */
    @JavascriptInterface
    public boolean onInputChanged(int featNum, String featName, String text) {
        Log.i(TAG, "Input: #" + featNum + " " + featName + " = " + text);
        if (!requireAuthForFeature(featNum)) {
            forceFeatureOff(featNum, featName, resolveSpec(featNum));
            return false;
        }
        safeCall(() -> Preferences.changeFeatureString(featName, featNum, text));
        if (!isUiFeature(featNum)) {
            safeCall(() -> nativeSetText(featNum, featName, text));
        }
        return true;
    }

    @JavascriptInterface
    public String getFeatureListJSON() {
        String[] features = nativeGetFeatureList();
        if (features == null) return "[]";
        StringBuilder sb = new StringBuilder("[");
        for (int i = 0; i < features.length; i++) {
            if (i > 0) sb.append(",");
            sb.append("\"").append(features[i].replace("\"", "\\\"")).append("\"");
        }
        sb.append("]");
        return sb.toString();
    }

    /** 原生菜单：直接取功能表 */
    public String[] getFeatureListRaw() {
        try {
            return nativeGetFeatureList();
        } catch (UnsatisfiedLinkError e) {
            return null;
        }
    }

    /** 原生菜单：登录后同步 grant → native */
    public void syncNativeAuthorizedPublic() {
        syncNativeAuthorized();
    }

    /**
     * 短票续签 / ForceClear 后：按 SharedPreferences 把 Toggle 等重新推回 native。
     * UI 开关可能仍为 ON，但底层 g_features 已被清。
     */
    public void reapplySavedFeaturesToNative() {
        if (!hasLiveNativeSession()) return;
        if (!Preferences.isMenuEnabled()) return;
        String[] features = null;
        try {
            features = nativeGetFeatureList();
        } catch (UnsatisfiedLinkError e) {
            return;
        }
        if (features == null) return;
        for (int i = 0; i < features.length; i++) {
            if (isUiFeature(i)) continue;
            String spec = features[i];
            if (spec == null) continue;
            if (spec.startsWith("Category_") || spec.startsWith("RichTextView_")
                    || spec.startsWith("RichWebView_") || spec.startsWith("Collapse_")) {
                continue;
            }
            try {
                if (spec.startsWith("Toggle_") || spec.startsWith("CheckBox_")) {
                    boolean on = Preferences.loadPrefBool(i, false);
                    StateMirror.mirror(i, on ? 1 : 0);
                    nativeSetFeature(i, spec, on);
                } else if (spec.startsWith("Slider_") || spec.startsWith("SeekBar_")
                        || spec.startsWith("Spinner_")) {
                    int v;
                    if (Preferences.contains(i)) {
                        v = Preferences.loadPrefInt(i);
                    } else {
                        v = FeatureListParser.seekResetValue(spec);
                    }
                    nativeSetValue(i, spec, v);
                    StateMirror.mirror(i, v);
                    // 部分游戏用 feature 开关控制滑块是否生效
                    if (Preferences.contains(i)) {
                        // no boolean key for slider usually
                    }
                } else if (spec.startsWith("InputText_") || spec.startsWith("InputValue_")) {
                    nativeSetText(i, spec, Preferences.loadPrefString(i));
                }
            } catch (UnsatisfiedLinkError ignored) {
            } catch (Exception e) {
                Log.w(TAG, "reapply feat #" + i + ": " + e.getMessage());
            }
        }
        Log.i(TAG, "reapplied saved features to native");
    }

    /** 启动时恢复 UI：SharedPreferences 全量 JSON */
    @JavascriptInterface
    public String getAllPrefsJSON() {
        return Preferences.exportJSON();
    }

    @JavascriptInterface
    public boolean isGameLibLoaded() {
        return nativeIsGameLibLoaded();
    }

    /** legacy HTML touch region; unused in native UI */
    @JavascriptInterface
    public void updateTouchTargets(String json) {
        // no-op: native UI does not use WebView touch-target passthrough
    }

    @JavascriptInterface
    public void setOverlayDragActive(boolean active) {
        // 同步写标记，避免 MOVE 已到但 post 未执行
        MenuService service = MenuService.getInstance();
        if (service != null) {
            service.setOverlayDragActive(active);
        }
    }

    /** 在主线程执行并短暂等待，避免 JS 改坐标与 WM 改窗抢帧 */
    private void runOverlaySync(Runnable action) {
        if (Looper.myLooper() == Looper.getMainLooper()) {
            action.run();
            return;
        }
        CountDownLatch latch = new CountDownLatch(1);
        mainHandler.post(() -> {
            try {
                action.run();
            } finally {
                latch.countDown();
            }
        });
        try {
            latch.await(400, TimeUnit.MILLISECONDS);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
    }

    @JavascriptInterface
    public void setOverlayPopupFullscreen(boolean full) {
        runOverlaySync(() -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.setOverlayPopupFullscreen(full);
            }
        });
    }

    /**
     * 关公告/Toast：同一主线程消息里退出全屏并落到球/面板小窗，避免跳位裁切。
     */
    @JavascriptInterface
    public void exitOverlayPopupToFrame(int x, int y, int w, int h) {
        runOverlaySync(() -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.exitPopupFullscreenToFrame(x, y, w, h);
            }
        });
    }

    @JavascriptInterface
    public void overlayLog(String msg) {
        Log.i("StarryOverlay", msg != null ? msg : "");
    }

    /** 复制文本到系统剪贴板（供菜单「复制 QQ」等） */
    @JavascriptInterface
    public boolean copyText(String text) {
        try {
            android.content.ClipboardManager cm =
                    (android.content.ClipboardManager) context.getSystemService(Context.CLIPBOARD_SERVICE);
            if (cm == null) return false;
            cm.setPrimaryClip(android.content.ClipData.newPlainText("starry", text != null ? text : ""));
            return true;
        } catch (Exception e) {
            Log.w(TAG, "copyText failed: " + e.getMessage());
            return false;
        }
    }

    @JavascriptInterface
    public void beginOverlayLocalDrag(int x, int y, int w, int h) {
        MenuService service = MenuService.getInstance();
        if (service != null) {
            service.beginOverlayLocalDrag(x, y, w, h);
        }
    }

    @JavascriptInterface
    public void setOverlayFrame(int x, int y, int w, int h) {
        MenuService service = MenuService.getInstance();
        if (service != null) {
            service.queueOverlayFrame(x, y, w, h);
        }
    }

    @JavascriptInterface
    public void endOverlayLocalDrag() {
        MenuService service = MenuService.getInstance();
        if (service != null) {
            service.endOverlayLocalDrag();
        }
    }

    /** 同步读取最近触摸 raw 屏幕坐标（勿 post，拖动跟手要当前帧） */
    @JavascriptInterface
    public String getOverlayTouchRawJSON() {
        MenuService service = MenuService.getInstance();
        if (service != null) {
            return service.getOverlayTouchRawJSON();
        }
        return "{\"x\":0,\"y\":0,\"sw\":1,\"sh\":1,\"cw\":1,\"ch\":1,\"t\":0}";
    }

    @JavascriptInterface
    public float getTouchRawX() {
        MenuService service = MenuService.getInstance();
        return service != null ? service.getTouchRawX() : 0f;
    }

    @JavascriptInterface
    public float getTouchRawY() {
        MenuService service = MenuService.getInstance();
        return service != null ? service.getTouchRawY() : 0f;
    }

    @JavascriptInterface
    public int getScreenW() {
        MenuService service = MenuService.getInstance();
        return service != null ? service.getScreenW() : 1;
    }

    @JavascriptInterface
    public int getScreenH() {
        MenuService service = MenuService.getInstance();
        return service != null ? service.getScreenH() : 1;
    }

    @JavascriptInterface
    public void setFocusable(boolean focusable) {
        Runnable r = () -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.setFocusable(focusable);
            }
        };
        // 插队：touchstart 后尽快开窗焦点，赶在 input focus / IME 判定前
        if (Looper.myLooper() == Looper.getMainLooper()) {
            r.run();
        } else if (focusable) {
            mainHandler.postAtFrontOfQueue(r);
        } else {
            mainHandler.post(r);
        }
    }

    @JavascriptInterface
    public void closeMenu() {
        Log.i(TAG, "closeMenu called from JS");
        // 先关全部功能再卸菜单：否则只藏 UI，Hook 仍生效
        lockAllFeatures();
        try { nativeSetAuthorized(false); } catch (UnsatisfiedLinkError ignored) {}
        mainHandler.post(() -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.stopSelf();
            }
        });
    }

    @JavascriptInterface
    public void collapseMenu() {
        Log.i(TAG, "collapseMenu called from JS");
        mainHandler.post(() -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.collapseToBall();
            }
        });
    }

    @JavascriptInterface
    public void expandMenu() {
        Log.i(TAG, "expandMenu called from JS");
        mainHandler.post(() -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.expandFromBall();
            }
        });
    }

    /**
     * 面板展开/收起均用安全 alpha 0.79（ColorOS 对更高 alpha 常压成看不见）。
     * 由 HTML collapsePanel/expandPanel 调用，勿与 collapseMenu 递归混用。
     */
    @JavascriptInterface
    public void setMenuPanelExpanded(boolean expanded) {
        mainHandler.post(() -> {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                service.setMenuPanelExpanded(expanded);
            }
        });
    }

    /** 打开外部链接（公告「立即前往」）；需 http/https */
    @JavascriptInterface
    public void openExternalUrl(String url) {
        if (url == null) return;
        String u = url.trim();
        if (u.isEmpty()) return;
        if (!u.startsWith("http://") && !u.startsWith("https://")) {
            Log.w(TAG, "openExternalUrl rejected: " + u);
            return;
        }
        Log.i(TAG, "openExternalUrl: " + u);
        mainHandler.post(() -> {
            try {
                Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(u));
                intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                context.startActivity(intent);
            } catch (Exception e) {
                Log.e(TAG, "openExternalUrl failed", e);
            }
        });
    }

    // ===== 网络验证 =====

    @JavascriptInterface
    public String getAuthSessionJSON() {
        return Preferences.exportAuthJSON();
    }

    @JavascriptInterface
    public void setAuthBaseUrl(String url) {
        Preferences.setAuthBaseUrl(url);
    }

    @JavascriptInterface
    public void setAuthGameId(String gameId) {
        // 忽略 JS 改写：game_id 只认 native 烘焙值
        syncAuthGameIdFromNative();
        if (gameId != null && !gameId.isEmpty()
                && !gameId.equals(Preferences.getAuthGameId())) {
            Log.w(TAG, "ignore setAuthGameId(" + gameId + "), baked="
                    + Preferences.getAuthGameId());
        }
    }

    /** HTML/调试：当前 APK 烘焙的 auth game_id */
    @JavascriptInterface
    public String getBakedAuthGameId() {
        try {
            String id = nativeGetAuthGameId();
            return id == null ? "" : id;
        } catch (UnsatisfiedLinkError e) {
            return Preferences.getAuthGameId();
        }
    }

    @JavascriptInterface
    public void clearAuthSession() {
        Preferences.clearAuthSession();
        try { nativeClearGrant(); } catch (UnsatisfiedLinkError ignored) {}
        syncNativeAuthorized();
        lockAllFeatures();
    }

    /**
     * 异步 HTTP 请求；完成后回调 JS：window.__onAuthResponse(requestId, jsonString)
     */
    @JavascriptInterface
    public void authRequest(String requestId, String method, String path, String bodyJson) {
        authExecutor.execute(() -> {
            String result = AuthClient.request(
                    Preferences.getAuthBaseUrl(),
                    method,
                    path,
                    bodyJson,
                    null
            );
            deliverAuthResponse(requestId, result);
        });
    }

    @JavascriptInterface
    public void saveAuthSession(String token, String cardKey, int cacheSec, String expireAt,
                                String bootstrapJson, String grant) {
        Preferences.saveAuthSession(
                token,
                cardKey,
                System.currentTimeMillis(),
                cacheSec,
                expireAt,
                bootstrapJson,
                grant
        );
        syncNativeAuthorized();
    }

    /** 后台关闭「卡密启用」时：联网 bootstrap + open 短票 */
    @JavascriptInterface
    public void saveNetworkBootstrap(int cacheSec, String bootstrapJson, String grant) {
        Preferences.saveNetworkBootstrap(cacheSec, bootstrapJson, grant);
        syncNativeAuthorized();
    }

    @JavascriptInterface
    public void setCardAuthRequired(boolean required) {
        Preferences.setCardAuthRequired(required);
        syncNativeAuthorized();
    }

    /** 后台游戏启用开关；关闭时强制关全部功能，保留卡密会话与设置 */
    @JavascriptInterface
    public void setMenuEnabled(boolean enabled) {
        Preferences.setMenuEnabled(enabled);
        if (!enabled) {
            lockAllFeatures();
        } else {
            syncNativeAuthorized();
        }
    }

    @JavascriptInterface
    public void setAuthorized(boolean authorized) {
        if (!authorized) {
            Preferences.clearAuthSession();
            try { nativeClearGrant(); } catch (UnsatisfiedLinkError ignored) {}
            lockAllFeatures();
            return;
        }
        // true 不能单独放行：必须走 grant 验签
        syncNativeAuthorized();
    }

    /** 关闭全部非 UI 功能（验证失败 / 游戏停用 / 锁菜单） */
    @JavascriptInterface
    public void lockAllFeatures() {
        String[] features = nativeGetFeatureList();
        if (features == null) {
            try {
                nativeSetAuthorized(false);
            } catch (UnsatisfiedLinkError ignored) {}
            return;
        }
        for (int i = 0; i < features.length; i++) {
            if (isUiFeature(i)) continue;
            String spec = features[i];
            if (spec == null) continue;
            if (spec.startsWith("Category_") || spec.startsWith("RichTextView_")
                    || spec.startsWith("RichWebView_") || spec.startsWith("Collapse_")) {
                continue;
            }
            forceFeatureOff(i, spec, spec);
        }
        try {
            nativeSetAuthorized(false);
        } catch (UnsatisfiedLinkError ignored) {}
    }

    /**
     * 按 g_featureList 规格强制关闭单项。滑块/下拉写 int，勿写 boolean（ClassCast 闪退）。
     * 2026-08-29：不再写偏好——启动竞态（行构建早于 session 就绪）时本方法被误触发，
     * 会把用户保存的功能偏好清掉（公主连结 key1/2/4 被清实证）。
     * 授权失效的封禁语义由 nativeSetAuthorized(false)（session 关闭 → native 全哑）保证。
     */
    public void forceFeatureOff(int featNum, String featName, String spec) {
        if (isUiFeature(featNum)) return;
        StateMirror.mirror(featNum, 0);
        String s = spec != null ? spec : resolveSpec(featNum);
        String name = (featName != null && !featName.isEmpty()) ? featName : s;
        try {
            if (s.startsWith("Slider_") || s.startsWith("SeekBar_") || s.startsWith("Spinner_")) {
                int reset = seekOrSpinnerResetValue(s);
                try { nativeSetValue(featNum, name, reset); } catch (UnsatisfiedLinkError ignored) {}
                try { nativeSetFeature(featNum, name, false); } catch (UnsatisfiedLinkError ignored) {}
            } else if (s.startsWith("InputText_") || s.startsWith("InputValue_")) {
                try { nativeSetText(featNum, name, ""); } catch (UnsatisfiedLinkError ignored) {}
                try { nativeSetFeature(featNum, name, false); } catch (UnsatisfiedLinkError ignored) {}
            } else if (s.startsWith("Button_")) {
                try { nativeSetFeature(featNum, name, false); } catch (UnsatisfiedLinkError ignored) {}
            } else {
                // Toggle_ / CheckBox_ / 未知：boolean OFF
                try { nativeSetFeature(featNum, name, false); } catch (UnsatisfiedLinkError ignored) {}
            }
        } catch (Exception e) {
            Log.w(TAG, "forceFeatureOff #" + featNum + ": " + e.getMessage());
        }
    }

    /** HTML / JS：按 native 功能表规格关单项（勿再对滑块调 onToggleChanged） */
    @JavascriptInterface
    public void forceFeatureOff(int featNum, String featName) {
        forceFeatureOff(featNum, featName, resolveSpec(featNum));
    }

    private String resolveSpec(int featNum) {
        try {
            String[] features = nativeGetFeatureList();
            if (features != null && featNum >= 0 && featNum < features.length && features[featNum] != null) {
                return features[featNum];
            }
        } catch (UnsatisfiedLinkError ignored) {}
        return "";
    }

    private static boolean isBoolFeatureSpec(String spec) {
        if (spec == null || spec.isEmpty()) return true;
        if (spec.startsWith("Toggle_") || spec.startsWith("CheckBox_")) return true;
        if (spec.startsWith("Slider_") || spec.startsWith("SeekBar_")
                || spec.startsWith("Spinner_") || spec.startsWith("Button_")
                || spec.startsWith("InputText_") || spec.startsWith("InputValue_")
                || spec.startsWith("Category_")) {
            return false;
        }
        // 无前缀的旧条目按开关处理
        return true;
    }

    /** Slider_/SeekBar_ → default（有声明）或 min；Spinner_ → 0 */
    private static int seekOrSpinnerResetValue(String spec) {
        if (spec == null) return 0;
        if (spec.startsWith("Slider_") || spec.startsWith("SeekBar_")) {
            return FeatureListParser.seekResetValue(spec);
        }
        return 0;
    }

    private void deliverAuthResponse(String requestId, String json) {
        mainHandler.post(() -> {
            MenuService service = MenuService.getInstance();
            if (service == null) return;
            service.evaluateJs(
                    "if (typeof window.__onAuthResponse === 'function') {"
                            + " window.__onAuthResponse("
                            + jsonStringLiteral(requestId) + ", "
                            + jsonStringLiteral(json)
                            + "); }"
            );
        });
    }

    private static String jsonStringLiteral(String s) {
        if (s == null) return "\"\"";
        String escaped = s
                .replace("\\", "\\\\")
                .replace("\"", "\\\"")
                .replace("\n", "\\n")
                .replace("\r", "\\r")
                .replace("\u2028", "\\u2028")
                .replace("\u2029", "\\u2029");
        return "\"" + escaped + "\"";
    }

    /* ========================================================
     *  桥接自检（2026-09-06，RegisterNatives 迁移配套）
     *  仅自测壳启动时调用：逐一调用 native 方法，任何签名不匹配/
     *  漏注册都以 UnsatisfiedLinkError 在这里暴露，而不是等到
     *  玩家触发功能时才炸。有副作用的 nativeClearGrant /
     *  nativeApplyPlayPatch / nativeExecButton 不在自检内。
     * ======================================================== */
    private static final String SELFTEST_TAG = "StarryBridgeTest";
    private interface SelfTestStep { void run() throws Exception; }
    private static int stPass, stFail;

    private static void stStep(String name, SelfTestStep s) {
        try {
            s.run();
            Log.i(SELFTEST_TAG, name + " = PASS");
            stPass++;
        } catch (Throwable t) {
            Log.e(SELFTEST_TAG, name + " = FAIL: " + t);
            stFail++;
        }
    }

    /** 仅自测壳（com.starry.magic）调用；注入游戏进程不跑 */
    public static void runBridgeSelfTest() {
        stPass = 0; stFail = 0;
        Log.i(SELFTEST_TAG, "=== 桥接自检开始 ===");
        stStep("nativeBridgeRegistered", () -> {
            if (!nativeBridgeRegistered()) throw new IllegalStateException("RegisterNatives 未全部成功");
        });
        stStep("nativeGetAuthGameId", () -> {
            if (nativeGetAuthGameId() == null) throw new IllegalStateException("null");
        });
        stStep("nativeIsGameLibLoaded", () -> nativeIsGameLibLoaded());
        stStep("nativeHasLiveSession", () -> nativeHasLiveSession());
        stStep("nativeGetAceGateRemainSec", () -> nativeGetAceGateRemainSec());
        stStep("nativeGetFeatureList", nativeGetFeatureList() == null ? null : () -> {});
        stStep("nativeGetSettingsList", () -> nativeGetSettingsList());
        stStep("nativeSetAuthorized(false)", () -> nativeSetAuthorized(false));
        stStep("nativeSetFeature(0,off)", () -> nativeSetFeature(0, "selftest", false));
        stStep("nativeSetValue(0,0)", () -> nativeSetValue(0, "selftest", 0));
        stStep("nativeSetLongValue(0,0)", () -> nativeSetLongValue(0, "selftest", 0L));
        stStep("nativeSetText(0,empty)", () -> nativeSetText(0, "selftest", ""));
        stStep("nativeApplyGrant(垃圾票→拒绝)", () -> nativeApplyGrant("bad|grant", "dev", "game"));
        stStep("nativeVerifyOtaSig(垃圾→false)", () -> nativeVerifyOtaSig("x", "y"));
        stStep("nativeGetPinnedCert", () -> {
            byte[] crt = nativeGetPinnedCert();
            if (crt == null || crt.length == 0) throw new IllegalStateException("烘焙证书为空");
            if (crt.length < 100) throw new IllegalStateException("证书长度异常: " + crt.length);
        });
        Log.i(SELFTEST_TAG, "=== 桥接自检结束: PASS=" + stPass + " FAIL=" + stFail + " ==="
                + (stFail == 0 ? " ✓" : " ✗ 有失败项，检查上方日志"));
    }
}
