package com.starry.magic.h5box;

import android.app.Activity;
import android.content.Context;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.JavascriptInterface;

import com.starry.magic.menu.Preferences;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.lang.ref.WeakReference;
import java.lang.reflect.Method;

/**
 * H5 盒子游戏（腾讯 X5 WebView）注入桥 — 《城主别慌张》。
 *
 * <p>本作没有原生游戏逻辑（Cocos Creator 3.8.8 web-mobile 构建跑在 X5 WebView 里，
 * 业务代码 100% 来自 CDN），所以「星空版」的功能主体是<b>页面内 JS 补丁</b>；
 * 本类只干四件事：
 *
 * <ol>
 *   <li><b>找 WebView</b>：{@link #attachTo(Activity)} 遍历 Activity 视图树，
 *       认出游戏 WebView（优先 {@code com.chaunmi.fastwebview.view.x5.FastX5WebView}，
 *       多个候选时取面积最大的那一个 —— 宿主还带有一个 overlayWebView）；</li>
 *   <li>把 {@code StarryH5} 这个 {@link JavascriptInterface} 挂上去，
 *       供补丁同步读取菜单状态（{@link #getState()}）；</li>
 *   <li>起<b>注入守护</b>：周期性把 {@code assets/starry/czbhz_patch.js} 灌进页面。
 *       补丁自身幂等 + 自延迟，因此页面重载 / 渲染进程重建后都能自动重装，
 *       完全不需要依赖 onPageFinished 之类的回调时序；</li>
 *   <li>把补丁关键日志回传 logcat（{@link #log(String)}）。</li>
 * </ol>
 *
 * <p><b>为什么不 patch 游戏 classes.dex</b>：宿主 {@code MainActivity} 的 smali 注入
 * 需要 baksmali/smali 整包重建 classes.dex（7.7MB、多 dex 顶部，风险高）。
 * 改由 Manifest 声明两个 Provider（框架的 {@code StarryInitProvider} 负责
 * 加载 so + SignSpoof + 起菜单；本包的 {@code CzbhzH5Provider} 负责挂本类），
 * 于是 classes.dex / classes2.dex / classes3.dex 全部字节不变。
 *
 * <p><b>授权门控</b>：{@link #getState()} 的 {@code live} 为 0 时补丁侧一律不生效。
 * 判定顺序：原生短票（反射 {@code ModBridge.nativeHasLiveSession()}）→ 兜底
 * {@link Preferences#isFeaturesAllowed()}；两者都拿不到时按不可用处理（fail-closed），
 * 并在 logcat 打印原因，避免"未授权却生效"。
 */
public final class StarryH5 {

    private static final String TAG = "StarryH5";
    private static final String JS_NAME = "StarryH5";
    /**
     * 补丁资产名与日志 tag 一律按运行时 game_id 推导（本类是全部 H5 游戏共用的，禁止写死某一款）：
     * {@code assets/starry/<game_id>_patch.js}；game_id 由 native 同步进 {@link
     * com.starry.magic.menu.Preferences#getAuthGameId()}。
     *
     * <p>未就绪时<b>不注入</b>（fail-closed）：宁可晚一秒再灌，也绝不把别的游戏的补丁
     * 灌进本游戏——写死资产名的旧写法在换游戏时会静默注入上一款补丁。
     */
    private static final String PATCH_ASSET_DIR = "starry/";
    private static volatile String sPatchAsset;
    private static volatile String sJsTag;

    private static boolean resolvePatchAsset() {
        if (sPatchAsset != null) {
            return true;
        }
        String gid = null;
        try {
            gid = com.starry.magic.menu.Preferences.getAuthGameId();
        } catch (Throwable t) {
            Log.w(TAG, "取 game_id 失败: " + t);
        }
        if (gid == null || gid.length() == 0) {
            return false;
        }
        sJsTag = "StarryJs_" + gid;
        sPatchAsset = PATCH_ASSET_DIR + gid + "_patch.js";
        Log.i(TAG, "补丁资产=" + sPatchAsset + "（game_id 已就绪）");
        return true;
    }
    /** 宿主游戏 WebView 的具体类名（优先命中；认不出时退回能力探测） */
    private static final String HOST_WEBVIEW_CLASS =
            "com.chaunmi.fastwebview.view.x5.FastX5WebView";

    /** 补丁未 ack 前的注入间隔（页面正在加载/重载，需要快） */
    private static final long INJECT_FAST_MS = 1000L;
    /** 补丁 ack 之后的巡检间隔（兜底页面重载） */
    private static final long INJECT_SLOW_MS = 5000L;
    /** WebView 未找到时的重试间隔 */
    private static final long ATTACH_RETRY_MS = 1000L;

    private static volatile WeakReference<Activity> sActivity = new WeakReference<Activity>(null);
    private static volatile WeakReference<Object> sWebView = new WeakReference<Object>(null);
    private static volatile Context sAppContext;
    private static volatile String sPatchSrc;
    private static volatile boolean sDaemonRunning;
    private static volatile boolean sAcked;
    private static volatile int sInjectCount;
    private static volatile String sInjectApi = "?";
    private static final Handler sMain = new Handler(Looper.getMainLooper());

    private final Context appContext;

    private StarryH5(Context ctx) {
        this.appContext = ctx;
    }

    // ==================================================================
    // 挂载入口（由 CzbhzH5Provider 的生命周期回调调用）
    // ==================================================================

    /**
     * 尝试把桥挂到该 Activity 的游戏 WebView 上。可反复调用：
     * 已挂好则不重复挂；WebView 被重建（onRenderProcessGone → recreateWebView）
     * 后调用本方法会重新挂到新实例上。必须在主线程调用。
     */
    public static void attachTo(Activity activity) {
        if (activity == null || activity.isFinishing()) {
            return;
        }
        sActivity = new WeakReference<Activity>(activity);
        Object current = sWebView.get();
        if (current != null) {
            return;  // 已挂载且 WebView 还活着
        }
        Object found = findGameWebView(activity);
        if (found == null) {
            return;  // 视图树还没长出来，守护会不断重试
        }
        install(activity, found);
    }

    private static Object findGameWebView(Activity activity) {
        try {
            View decor = activity.getWindow() == null ? null : activity.getWindow().getDecorView();
            if (decor == null) {
                return null;
            }
            View best = null;
            long bestArea = -1;
            // 广度优先遍历（WebView 深度有限，直接递归即可，加深度上限防环）
            java.util.ArrayDeque<View> queue = new java.util.ArrayDeque<View>();
            queue.add(decor);
            int guard = 0;
            while (!queue.isEmpty() && guard++ < 5000) {
                View v = queue.poll();
                if (isWebViewLike(v)) {
                    boolean hostClass = HOST_WEBVIEW_CLASS.equals(v.getClass().getName());
                    long area = (long) v.getWidth() * (long) v.getHeight();
                    // 宿主类优先；同类之间取面积最大（排除 overlayWebView）
                    if (best == null
                            || (hostClass && !HOST_WEBVIEW_CLASS.equals(best.getClass().getName()))
                            || (hostClass == HOST_WEBVIEW_CLASS.equals(best.getClass().getName())
                                && area > bestArea)) {
                        best = v;
                        bestArea = area;
                    }
                }
                if (v instanceof ViewGroup) {
                    ViewGroup g = (ViewGroup) v;
                    for (int i = 0; i < g.getChildCount(); i++) {
                        queue.add(g.getChildAt(i));
                    }
                }
            }
            if (best != null) {
                Log.i(TAG, "找到游戏 WebView: " + best.getClass().getName());
            }
            return best;
        } catch (Throwable t) {
            Log.w(TAG, "查找 WebView 异常: " + t);
            return null;
        }
    }

    /** 能力探测：既能 loadUrl 又能 addJavascriptInterface 的 View 才算 WebView（免 X5 编译依赖） */
    private static boolean isWebViewLike(View v) {
        if (v == null) {
            return false;
        }
        Class<?> c = v.getClass();
        if (c.getName().indexOf("WebView") < 0) {
            return false;
        }
        try {
            c.getMethod("loadUrl", String.class);
            c.getMethod("addJavascriptInterface", Object.class, String.class);
            return true;
        } catch (Throwable t) {
            return false;
        }
    }

    private static void install(Context ctx, Object webView) {
        Context app = (ctx != null) ? ctx.getApplicationContext() : contextOf(webView);
        if (app == null) {
            Log.w(TAG, "install: 拿不到 Context，跳过");
            return;
        }
        sAppContext = app;
        try {
            Preferences.with(app);
        } catch (Throwable t) {
            Log.w(TAG, "Preferences.with 失败（状态查询会降级）: " + t);
        }
        sWebView = new WeakReference<Object>(webView);
        sAcked = false;   // 新页面（或重建后的 WebView）需要重新注入
        try {
            Method m = webView.getClass().getMethod(
                    "addJavascriptInterface", Object.class, String.class);
            m.invoke(webView, new StarryH5(app), JS_NAME);
            Log.i(TAG, "addJavascriptInterface(" + JS_NAME + ") 已挂载");
        } catch (Throwable t) {
            Log.e(TAG, "addJavascriptInterface 失败，补丁将拿不到菜单状态", t);
            return;
        }
        startDaemon();
    }

    private static Context contextOf(Object webView) {
        try {
            Method m = webView.getClass().getMethod("getContext");
            Object c = m.invoke(webView);
            if (c instanceof Context) {
                return ((Context) c).getApplicationContext();
            }
        } catch (Throwable ignored) {
        }
        return null;
    }

    // ==================================================================
    // 注入守护
    // ==================================================================

    private static void startDaemon() {
        if (sDaemonRunning) {
            return;
        }
        sDaemonRunning = true;
        sMain.post(new Runnable() {
            @Override
            public void run() {
                daemonTick();
            }
        });
        Log.i(TAG, "注入守护已启动");
    }

    private static void daemonTick() {
        Object wv = sWebView.get();
        if (wv == null) {
            // WebView 还没出现或被回收：拿最近一次 Activity 再找一遍（永不放弃，
            // 代价只是每 1s 一次视图树遍历；找到就挂上，找不到就继续等）
            Activity a = sActivity.get();
            if (a != null) {
                attachTo(a);
            }
            sMain.postDelayed(new Runnable() {
                @Override
                public void run() {
                    daemonTick();
                }
            }, ATTACH_RETRY_MS);
            return;
        }
        inject(wv);
        final long next = sAcked ? INJECT_SLOW_MS : INJECT_FAST_MS;
        sMain.postDelayed(new Runnable() {
            @Override
            public void run() {
                daemonTick();
            }
        }, next);
    }

    private static void inject(Object webView) {
        String src = patchSource();
        if (src == null || src.isEmpty()) {
            return;
        }
        try {
            evalJs(webView, src);
            int n = ++sInjectCount;
            if (n <= 5 || n % 60 == 0) {
                Log.i(TAG, "注入第 " + n + " 次（" + src.length() + " 字符，经 " + sInjectApi + "）");
            }
        } catch (Throwable t) {
            Log.w(TAG, "注入失败: " + t);
        }
    }

    /**
     * 把脚本送进页面。**首选 {@code evaluateJavascript}**，只有它不可用时才退回
     * {@code loadUrl("javascript:...")}。
     *
     * <p>为什么必须首选 evaluateJavascript（2026-09-12 真机实测教训）：
     * {@code loadUrl("javascript:" + src)} 会把整个字符串当**URL** 处理，
     * 于是 (a) 百分号会被解码 —— 脚本里只要出现 `%23` 就会解出 `#`，
     * 而 `#` 在 URL 里是片段起点，**脚本被当场截断**；
     * (b) 超长脚本还会碰到 URL 长度与转义问题。
     * 实测症状：本作补丁注释里有一处 `%233280`，脚本被截断在块注释中间 →
     * 整段语法错误 → WebView **静默丢弃**（logcat 无任何报错），
     * 表现为"注入日志正常、补丁一行日志都没有"。
     * 改用 evaluateJavascript 后不再经过 URL 解析，任意字符/长度均安全。
     *
     * <p>反射兼容两套签名：{@code android.webkit.WebView} 与腾讯 X5 的
     * {@code evaluateJavascript(String, X5ValueCallback)}（回调类型擦除后不同，
     * 因此按名字+参数个数扫描，而不是按参数类型 getMethod）。
     */
    private static boolean evalJs(Object webView, String src) throws Exception {
        for (Method m : webView.getClass().getMethods()) {
            if (!"evaluateJavascript".equals(m.getName())) {
                continue;
            }
            Class<?>[] ps = m.getParameterTypes();
            if (ps.length == 2 && ps[0] == String.class) {
                sInjectApi = "evaluateJavascript/2";
                m.invoke(webView, src, null);
                return true;
            }
            if (ps.length == 1 && ps[0] == String.class) {
                sInjectApi = "evaluateJavascript/1";
                m.invoke(webView, src);
                return true;
            }
        }
        // 退路：先转义再拼 javascript: URL（% 必须先转，否则会把下面补的 %23 也一起转掉）
        Method m = webView.getClass().getMethod("loadUrl", String.class);
        sInjectApi = "loadUrl/javascript:";
        m.invoke(webView, "javascript:" + src.replace("%", "%25").replace("#", "%23"));
        return true;
    }

    /** 读一次 assets 并缓存（补丁是静态资源，进程内不变） */
    private static String patchSource() {
        String cached = sPatchSrc;
        if (cached != null) {
            return cached;
        }
        Context ctx = sAppContext;
        if (ctx == null) {
            return null;
        }
        if (!resolvePatchAsset()) {
            Log.i(TAG, "game_id 未就绪，本轮跳过注入（下一轮重试）");
            return null;
        }
        final String asset = sPatchAsset;
        InputStream in = null;
        try {
            in = ctx.getAssets().open(asset);
            ByteArrayOutputStream bo = new ByteArrayOutputStream(32768);
            byte[] buf = new byte[8192];
            int n;
            while ((n = in.read(buf)) > 0) {
                bo.write(buf, 0, n);
            }
            String s = new String(bo.toByteArray(), "UTF-8");
            sPatchSrc = s;
            Log.i(TAG, "补丁源已载入 " + asset + " (" + s.length() + " 字符)");
            return s;
        } catch (Throwable t) {
            Log.e(TAG, "读取 " + asset + " 失败（外科是否漏塞 assets？）", t);
            return null;
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (Throwable ignored) {
                }
            }
        }
    }

    // ==================================================================
    // JS 接口
    // ==================================================================

    /** 补丁日志 → logcat（tag 与网页控制台分开，便于过滤） */
    @JavascriptInterface
    public void log(String msg) {
        Log.i(sJsTag != null ? sJsTag : TAG, msg == null ? "" : msg);
    }

    /** 补丁完成安装（含钩子装点）后调用；守护据此降频 */
    @JavascriptInterface
    public void ack(String detail) {
        sAcked = true;
        Log.i(TAG, "补丁 ack: " + (detail == null ? "" : detail));
    }

    /** 授权是否有效（补丁可单独问） */
    @JavascriptInterface
    public boolean authorized() {
        return authorizedInternal();
    }

    /**
     * 菜单状态快照。返回 JSON：
     * <pre>{"live":0|1,"specs":[...],"bools":[0|1,...],"vals":[...]}</pre>
     * {@code live=0} 时补丁侧必须一律不生效（未授权/过期/后台停用）。
     * {@code specs} 来自 so 内烘焙的功能表（反射 {@code ModBridge.nativeGetFeatureList()}），
     * {@code bools/vals} 来自菜单偏好（与 {@code ModBridge.reapplySavedFeaturesToNative()} 同源）。
     */
    @JavascriptInterface
    public String getState() {
        boolean live = authorizedInternal();
        String[] specs = featureSpecs();
        StringBuilder sb = new StringBuilder(512);
        sb.append("{\"live\":").append(live ? 1 : 0).append(",\"specs\":[");
        for (int i = 0; i < specs.length; i++) {
            if (i > 0) {
                sb.append(',');
            }
            appendJsonString(sb, specs[i]);
        }
        sb.append("],\"bools\":[");
        for (int i = 0; i < specs.length; i++) {
            if (i > 0) {
                sb.append(',');
            }
            sb.append((live && isBoolSpec(specs[i]) && boolOf(i)) ? 1 : 0);
        }
        sb.append("],\"vals\":[");
        for (int i = 0; i < specs.length; i++) {
            if (i > 0) {
                sb.append(',');
            }
            sb.append((live && isIntSpec(specs[i])) ? valOf(i) : 0);
        }
        sb.append("]}");
        return sb.toString();
    }

    /**
     * 按 spec 前缀判类型（与 ModBridge.reapplySavedFeaturesToNative 同一套口径）。
     *
     * <p>2026-10-07 炁术行者实测事故：getState 原先对**每个** feature 都调 boolOf(i)，
     * 对滑块（值是 int）会走 getBoolean → ClassCastException，旧 readBoolean 再静默
     * remove 掉该键 ⇒ 玩家刚拖的滑块值在同一帧内被删，H5 补丁永远读到 0
     * （现象：拖完滑块游戏没反应）。故必须先按类型分流，绝不误读。
     */
    private static boolean isBoolSpec(String spec) {
        return spec != null && (spec.startsWith("Toggle_") || spec.startsWith("CheckBox_"));
    }

    private static boolean isIntSpec(String spec) {
        return spec != null && (spec.startsWith("Slider_") || spec.startsWith("SeekBar_")
                || spec.startsWith("Spinner_"));
    }

    private static boolean boolOf(int featNum) {
        try {
            return Preferences.loadPrefBool(featNum, false);
        } catch (Throwable t) {
            return false;
        }
    }

    private static int valOf(int featNum) {
        try {
            if (Preferences.contains(featNum)) {
                return Preferences.loadPrefInt(featNum);
            }
        } catch (Throwable ignored) {
        }
        return 0;   // 0 = 联动功能关闭
    }

    private static String[] featureSpecs() {
        try {
            Method m = Class.forName("com.starry.magic.bridge.ModBridge")
                    .getDeclaredMethod("nativeGetFeatureList");
            m.setAccessible(true);
            Object r = m.invoke(null);
            if (r instanceof String[]) {
                return (String[]) r;
            }
        } catch (Throwable t) {
            Log.w(TAG, "读功能表失败（so 未加载？）: " + t);
        }
        return new String[0];
    }

    /**
     * 授权判定：原生短票优先（最终真相），拿不到时退回 Java 侧会话判定。
     * 任何异常都按「不可用」处理（fail-closed），并打印原因便于排障。
     */
    private static boolean authorizedInternal() {
        try {
            if (!Preferences.isMenuEnabled()) {
                return false;
            }
        } catch (Throwable t) {
            Log.w(TAG, "isMenuEnabled 异常, 按未授权处理: " + t);
            return false;
        }
        Boolean live = nativeSessionLive();
        if (live != null) {
            return live.booleanValue();
        }
        try {
            boolean ok = Preferences.isFeaturesAllowed();
            if (!ok) {
                Log.i(TAG, "授权：native 不可查 → Java 侧 isFeaturesAllowed=false");
            }
            return ok;
        } catch (Throwable t) {
            Log.w(TAG, "授权判定异常, 按未授权处理: " + t);
            return false;
        }
    }

    /** @return Boolean 或 null（反射不可用） */
    private static Boolean nativeSessionLive() {
        try {
            Method m = Class.forName("com.starry.magic.bridge.ModBridge")
                    .getDeclaredMethod("nativeHasLiveSession");
            m.setAccessible(true);
            Object r = m.invoke(null);
            if (r instanceof Boolean) {
                return (Boolean) r;
            }
        } catch (Throwable t) {
            Log.w(TAG, "nativeHasLiveSession 不可用（仅作提示，不影响判定）: " + t);
        }
        return null;
    }

    private static void appendJsonString(StringBuilder sb, String s) {
        sb.append('"');
        if (s != null) {
            for (int i = 0; i < s.length(); i++) {
                char c = s.charAt(i);
                switch (c) {
                    case '"':
                        sb.append("\\\"");
                        break;
                    case '\\':
                        sb.append("\\\\");
                        break;
                    case '\n':
                        sb.append("\\n");
                        break;
                    case '\r':
                        sb.append("\\r");
                        break;
                    case '\t':
                        sb.append("\\t");
                        break;
                    default:
                        if (c < 0x20) {
                            sb.append(String.format("\\u%04x", (int) c));
                        } else {
                            sb.append(c);
                        }
                        break;
                }
            }
        }
        sb.append('"');
    }
}
