package com.starry.magic.menu;

import android.app.AlertDialog;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.graphics.Outline;
import android.graphics.PixelFormat;
import android.graphics.Typeface;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.GradientDrawable;
import android.net.Uri;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.os.SystemClock;
import android.provider.Settings;
import android.text.InputType;
import android.text.Layout;
import android.text.StaticLayout;
import android.text.TextPaint;
import android.text.TextUtils;
import android.util.Log;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewConfiguration;
import android.view.ViewGroup;
import android.view.ViewOutlineProvider;
import android.view.WindowManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.PopupWindow;
import android.widget.ScrollView;
import android.widget.SeekBar;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.Toast;

import com.starry.magic.auth.AuthClient;
import com.starry.magic.bridge.ModBridge;
import com.starry.magic.ota.PlayOtaManager;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * 全原生「星光魔法阵」菜单：Activity 主路径 + Overlay 备用。
 */
public final class NativeMenuController {

    private static final String TAG = "NativeMenu";
    /**
     * Overlay 窗口不透明度。曾经是 0.79f，被怀疑是「菜单看得见球、展开后却几乎看不见」
     * 的元凶，改成 1.0f（完全不透明）。
     */
    private static final float OVERLAY_SAFE_ALPHA = 1.0f;

    /** 字号阶梯（对齐旧 HTML 观感，略抬正文可读性） */
    private static final float FS_TITLE = 16f;
    private static final float FS_SECTION = 14f; // 分类标题（原 12）
    private static final float FS_BODY = 13f;    // 功能行标签（原 12）
    private static final float FS_TAB = 12f;
    private static final float FS_VALUE = 13f;   // 选择值/输入/滑块数（原 11，与 BODY 对齐）
    private static final float FS_BTN = 11f;
    private static final float FS_STATUS = 11f;
    private static final float FS_HINT = 10f;
    private static final float FS_LOGIN_TITLE = 15f;
    private static final float FS_PRIMARY_BTN = 14f;

    /** FAB：MagicRes.fabIcon() ← design/fab/fab_icon_q88.webp（Base64 内嵌） */
    private static final int FAB_WM_DP = 72;
    private static final int FAB_ICON_DP = 72;

    /** 垂直节奏（全面板统一） */
    private static final int SPACE = 8;
    private static final int SPACE_TIGHT = 4;
    private static final int ROW_PAD_V = 7;
    /** 花藤探出面板描边的外圈（dp）；与角饰尺寸配合形成内外缠绕 */
    private static final int VINE_OVERHANG_DP = 22;
    private static final int PANEL_INNER_W_DP = 312;
    private static final int VINE_CORNER_DP = 112;
    // 内容滚动区高度预算：正常屏上限 260dp；小屏（横屏/低分辨率/大显示缩放）按屏幕 dp 视口收缩
    private static final int PANEL_LIST_MAX_DP = 260;
    private static final int PANEL_LIST_MIN_DP = 110;
    // 滚动区之外的固定装饰高度（藤蔓外探44 + 上下padding32 + 标题栏约48 + 分割线约26 + tab栏44），实测估算
    private static final int PANEL_CHROME_H_DP = 190;

    public enum AttachMode { ACTIVITY, OVERLAY, NONE }

    private final Context appContext;
    private final Handler mainHandler = new MenuSafeHandler(TAG);
    private final ExecutorService authExec = Executors.newSingleThreadExecutor();

    /**
     * ★r24 防闪退：线程池任务体纳入 Throwable 兜底。
     * 为什么必须包：`ThreadPoolExecutor` 把任务抛出的 Throwable 重新抛给线程 ⇒ 线程终止 ⇒
     * 触发 Thread 的未捕获异常处理 ⇒ 我们的 CrashHandler 记录后又交回系统默认处理 ⇒
     * **整个游戏进程被杀**。也就是说「后台一个 OTA/授权任务出异常 = 玩家闪退」。
     * 包一层后失败只记日志，任务本身作废、进程继续。
     */
    private void runAuth(Runnable body) {
        authExec.execute(() -> {
            try {
                body.run();
            } catch (Throwable t) {
                Log.e(TAG, "authExec task failed (isolated, 进程继续)", t);
            }
        });
    }
    private final ModBridge bridge;

    private FrameLayout root;
    private FrameLayout fab;
    private ImageView fabIconView;
    private View panel;
    private LinearLayout featureList;
    private LinearLayout currentCategoryBody;
    /** refreshFeatures 时递增；分类展开状态跨刷新保留 */
    private int categoryBuildIndex;
    private final Map<String, Boolean> categoryOpenState = new HashMap<>();
    private LinearLayout authPane;
    private LinearLayout settingsPane;
    private LinearLayout accountLoginBox;
    private LinearLayout accountMainBox;
    private ScrollView featScroll;
    private ScrollView authScroll;
    private ScrollView setScroll;
    private TextView tabFeat;
    private TextView tabAuth;
    private TextView tabSet;
    private TextView statusLine;
    private boolean aceGateTickerStarted;
    private boolean aceGateWasBlocking;
    private final Runnable aceGateTickRunnable = new Runnable() {
        @Override
        public void run() {
            try {
                refreshAuthStatus();
                int left = ModBridge.getAceGateRemainSec();
                if (aceGateWasBlocking && left <= 0) {
                    aceGateWasBlocking = false;
                    refreshFeatures();
                    showToast("过检完成，功能可用");
                } else if (left > 0) {
                    aceGateWasBlocking = true;
                }
            } finally {
                if (aceGateTickerStarted) {
                    mainHandler.postDelayed(this, 1000L);
                }
            }
        }
    };
    private TextView accountExpireTv;
    private TextView accountUsesTv;
    private Button accountUnbindBtn;
    private EditText cardInput;
    private boolean unbindArmed;
    private final Runnable unbindArmClear = () -> unbindArmed = false;
    private boolean bootstrapFetched;
    /** 展开/功能拦截/冷启动续票 */
    private volatile boolean grantRenewInFlight;
    private long lastGrantRenewElapsed;
    private static final long GRANT_RENEW_MIN_INTERVAL_MS = 15_000L;  // 续票最小间隔
    /**
     * 冷启动续票（2026-09-05）：砍掉 60s 周期心跳。进程内只在启动后对一次账——
     * 服务端硬拒（封禁/删除/过期/解绑）即踢回登录；网络失败有界重试后放弃。
     * 会话本身登录后永不过期（native g_rt_ok），封禁等控制下次冷启动生效。
     */
    private static final long GRANT_RENEW_RETRY_DELAY_MS = 30_000L;
    private static final int GRANT_RENEW_MAX_RETRIES = 5;
    private int grantRenewRetries;
    private boolean grantBgRenewStarted;
    /** 本进程冷启动续票已出结果（成功/硬拒/重试耗尽），hide/show 不再重开对账 */
    private boolean grantRenewCycleDone;
    /**
     * 本进程是否已尝试过「静默自愈重登」。设备指纹漂移 / 会话失效属可自愈类：
     * 本地存着卡密时用已存卡密重登即可恢复，<b>不得清存档</b>（2026-10-04 云机修复）。
     */
    private boolean authSelfHealAttempted;
    private final Runnable grantBgRenewRunnable = new Runnable() {
        @Override
        public void run() {
            if (!grantBgRenewStarted) return;
            // 成功/硬拒在 maybeRenewGrant 回调里结束本循环；网络失败靠这里的重试
            maybeRenewGrant("cold-start");
            if (++grantRenewRetries <= GRANT_RENEW_MAX_RETRIES) {
                mainHandler.postDelayed(this, GRANT_RENEW_RETRY_DELAY_MS);
            } else {
                grantBgRenewStarted = false;
                grantRenewCycleDone = true;
            }
        }
    };
    private final List<JSONObject> announceQueue = new ArrayList<>();
    private boolean announceShowing;
    /** 当前展示中的公告（关闭时 once_device 记已读） */
    private JSONObject currentAnnounce;
    /** Activity 切换/暂时分离时公告重试等待重挂（超限才降级 toast） */
    private static final int ANNOUNCE_ATTACH_RETRY_MAX = 8;
    private static final long ANNOUNCE_ATTACH_RETRY_MS = 600L;
    private int announceAttachRetry;
    // 公告错峰：公告播放期间挂起 startup/gate toast 与悬浮球，全部公告关闭后按原节奏补放，
    // 避免公告弹窗、toast、悬浮球同屏叠出看不过来
    private final List<String> stagedGateToasts = new ArrayList<>();
    private final List<String> stagedStartupToasts = new ArrayList<>();
    private boolean menuHoldForAnnounce;
    private PopupWindow openDropdown;
    private View toastOverlay;
    private TextView toastText;
    private ImageView toastIcon;
    private WindowManager.LayoutParams toastLp;
    private final Runnable toastHideRunnable = this::hideMagicToast;
    private View announceOverlay;
    private WindowManager.LayoutParams announceLp;

    private WindowManager windowManager;
    private WindowManager.LayoutParams layoutParams;
    private AttachMode attachMode = AttachMode.NONE;
    private Activity boundActivity;

    private boolean expanded;
    private boolean attached;
    /** 防止 BadToken → ensureAlive → setExpanded → BadToken 递归 */
    private boolean hostRebindInFlight;

    private float downRawX, downRawY;
    private int downX, downY;
    private boolean dragging;
    private static final int TAP_SLOP_PX = 24;

    public NativeMenuController(Context context) {
        this.appContext = context.getApplicationContext();
        Preferences.with(appContext);
        this.bridge = new ModBridge(appContext);
        ModBridge.syncAuthGameIdFromNative();
    }

    public boolean isAttached() {
        return attached;
    }

    public AttachMode getAttachMode() {
        return attachMode;
    }

    public Activity getBoundActivity() {
        return boundActivity;
    }

    public View getRoot() {
        return root;
    }

    /**
     * ACTIVITY 窗是否仍挂在活着的宿主上。
     * Splash→Game / 乐变切章后旧 token 会失效：窗还「看得见」但裁切且无法点击。
     */
    public boolean isBoundHostAlive() {
        if (attachMode == AttachMode.OVERLAY) {
            return attached && root != null && root.getParent() != null;
        }
        if (!attached || attachMode != AttachMode.ACTIVITY || boundActivity == null) {
            return false;
        }
        if (boundActivity.isFinishing()) return false;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR1
                && boundActivity.isDestroyed()) {
            return false;
        }
        try {
            if (boundActivity.getWindow() == null) return false;
            View decor = boundActivity.getWindow().getDecorView();
            if (decor == null || !decor.isAttachedToWindow()) return false;
        } catch (Exception e) {
            return false;
        }
        return root != null && root.getParent() != null;
    }

    /** 宿主失效或切 Activity 时强制重绑；保留展开态。 */
    public void ensureAliveOrRebind(Activity preferredHost) {
        if (hostRebindInFlight) return;
        hostRebindInFlight = true;
        try {
            ensureAliveOrRebindInner(preferredHost);
        } catch (Throwable t) {
            Log.e(TAG, "ensureAliveOrRebind isolated", t);
            try {
                detachWindowOnly();
                expanded = false;
            } catch (Throwable ignored) {
            }
        } finally {
            hostRebindInFlight = false;
        }
    }

    private void ensureAliveOrRebindInner(Activity preferredHost) {
        if (attachMode == AttachMode.OVERLAY) {
            if (menuHoldForAnnounce) return; // 公告播放中：不 reattach，保持悬浮球隐藏
            if (!isBoundHostAlive()) {
                Log.w(TAG, "overlay parent lost → reattach overlay");
                boolean wasExpanded = expanded;
                detachWindowOnly();
                if (canOverlay() && tryAttachOverlay() && wasExpanded) {
                    setExpanded(true);
                }
            }
            return;
        }
        Activity host = preferredHost;
        if (host == null || host.isFinishing()
                || (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR1
                && host.isDestroyed())) {
            host = findResumedHint(preferredHost);
        }
        if (host == null) {
            if (attached && !isBoundHostAlive()) {
                Log.w(TAG, "ensureAliveOrRebind: dead host, no replacement → detach");
                detachWindowOnly();
                expanded = false;
            }
            return;
        }
        boolean need = !attached || !isBoundHostAlive() || boundActivity != host;
        if (!need) {
            if (root != null && root.getParent() == null) {
                if (menuHoldForAnnounce) return; // 公告播放中：不 re-addView，保持隐藏
                Log.i(TAG, "rebind same host: re-addView after hide");
                setVisible(true);
                if (root.getParent() == null) {
                    Log.w(TAG, "re-addView failed → full reattach");
                    need = true;
                } else {
                    return;
                }
            } else {
                return;
            }
        }
        boolean wasExpanded = expanded;
        Log.i(TAG, "ensureAliveOrRebind → " + host.getClass().getSimpleName()
                + " alive=" + isBoundHostAlive()
                + " was=" + (boundActivity != null ? boundActivity.getClass().getSimpleName() : "null")
                + " expanded=" + wasExpanded);
        detachWindowOnly();
        boolean ok = tryAttachActivity(host);
        if (!ok && canOverlay()) {
            ok = tryAttachOverlay();
        }
        if (ok && menuHoldForAnnounce) {
            applyAnnounceHold(); // 公告播放中：重挂后立即收起，保持公告优先
        }
        if (ok && wasExpanded) {
            setExpanded(true);
            requestPanelRelayout();
            clampWindowOnScreen();
        }
    }

    /** 展开后坐标/高度越界时收进屏内，避免「裁掉一半且点不到」。 */
    private void clampWindowOnScreen() {
        if (layoutParams == null || windowManager == null || root == null || !attached) return;
        try {
            android.util.DisplayMetrics dm = appContext.getResources().getDisplayMetrics();
            int screenW = dm.widthPixels;
            int screenH = dm.heightPixels;
            boolean heightClamped = false;
            // 兜底：窗口高度超屏时压回屏内，否则底部 tab 栏出屏看不到也点不到
            // （requestPanelRelayout 已按屏限高，此处防异常路径漏网，如重挂后 WRAP_CONTENT）
            if (layoutParams.height > screenH) {
                layoutParams.height = screenH;
                heightClamped = true;
                Log.i(TAG, "clampWindowOnScreen: height clamped to " + screenH);
            }
            int w = layoutParams.width > 0 ? layoutParams.width : root.getWidth();
            int h = layoutParams.height > 0 ? layoutParams.height : root.getHeight();
            if (w <= 0) w = dp(FAB_WM_DP);
            if (h <= 0) h = dp(FAB_WM_DP);
            int maxX = Math.max(0, screenW - Math.min(w, screenW));
            int maxY = Math.max(0, screenH - Math.min(h, screenH));
            int nx = Math.max(0, Math.min(layoutParams.x, maxX));
            int ny = Math.max(0, Math.min(layoutParams.y, maxY));
            if (heightClamped || nx != layoutParams.x || ny != layoutParams.y) {
                layoutParams.x = nx;
                layoutParams.y = ny;
                windowManager.updateViewLayout(root, layoutParams);
                Log.i(TAG, "clampWindowOnScreen → (" + nx + "," + ny + ")");
            }
        } catch (Exception e) {
            Log.w(TAG, "clampWindowOnScreen: " + e.getMessage());
        }
    }

    /** 优先 Activity 窗；失败且有悬浮窗权限则 Overlay。 */
    public boolean show(Activity preferredHost) {
        if (attached) {
            ensureAliveOrRebind(preferredHost);
            applyAnnounceHold();
            return isAttached();
        }
        buildUiIfNeeded();
        if (tryAttachActivity(preferredHost)) {
            fetchBootstrapOnce();
            startGrantBgRenew();
            applyAnnounceHold();
            return true;
        }
        Activity resumed = findResumedHint(preferredHost);
        if (resumed != null && resumed != preferredHost && tryAttachActivity(resumed)) {
            fetchBootstrapOnce();
            startGrantBgRenew();
            applyAnnounceHold();
            return true;
        }
        if (canOverlay()) {
            boolean ok = tryAttachOverlay();
            if (ok) {
                fetchBootstrapOnce();
                startGrantBgRenew();
                applyAnnounceHold();
            }
            return ok;
        }
        Log.w(TAG, "show failed: no activity attach and no overlay permission");
        return false;
    }

    public void rebind(Activity activity) {
        if (activity == null) return;
        MenuService svc = MenuService.getInstance();
        if (svc != null && svc.isMenuUserHidden()) {
            return;
        }
        if (attachMode == AttachMode.OVERLAY) {
            // Overlay 不依赖 Activity token；父节点丢了仍要补挂
            if (!isBoundHostAlive()) {
                ensureAliveOrRebind(activity);
            }
            return;
        }
        // 宿主已死或非当前 Activity：整窗重绑（Splash→Game 必经）
        if (!isBoundHostAlive() || boundActivity != activity) {
            ensureAliveOrRebind(activity);
            return;
        }
        // 同 Activity：hide 只 removeView，必须补挂，否则回前台悬浮窗永久消失
        if (root != null && root.getParent() == null) {
            ensureAliveOrRebind(activity);
        }
    }

    public void hide() {
        stopGrantBgRenew();
        detachWindowOnly();
        if (root != null) {
            root.removeAllViews();
            root = null;
        }
        fab = null;
        fabIconView = null;
        panel = null;
        featureList = null;
        currentCategoryBody = null;
        authPane = null;
        settingsPane = null;
        accountLoginBox = null;
        accountMainBox = null;
        accountExpireTv = null;
        accountUsesTv = null;
        accountUnbindBtn = null;
        cardInput = null;
        dismissEditClipboardBar();
        unbindArmed = false;
        bootstrapFetched = false;
        announceQueue.clear();
        announceShowing = false;
        announceAttachRetry = 0;
        currentAnnounce = null;
        // 公告错峰状态一并复位（菜单重建后 bootstrap 会重新走一遍）
        stagedGateToasts.clear();
        stagedStartupToasts.clear();
        menuHoldForAnnounce = false;
        expanded = false;
        attached = false;
        attachMode = AttachMode.NONE;
        boundActivity = null;
    }

    public void setVisible(boolean visible) {
        if (!attached || root == null || layoutParams == null || windowManager == null) return;
        try {
            if (visible) {
                root.setVisibility(View.VISIBLE);
                if (root.getParent() == null) {
                    windowManager.addView(root, layoutParams);
                }
                root.setAlpha(1f);
            } else {
                if (root.getParent() != null) {
                    windowManager.removeView(root);
                }
            }
        } catch (Exception e) {
            Log.w(TAG, "setVisible(" + visible + "): " + e.getMessage());
        }
    }

    public void collapse() {
        setExpanded(false);
    }

    /** 隐藏菜单前：关掉下拉、公告等临时浮层。 */
    public void dismissTransientUi() {
        dismissMagicDropdown();
        removeWmToast();
        removeMagicAnnounce();
        if (announceShowing && currentAnnounce != null) {
            // 公告被临时关掉：延迟重试恢复，保持状态一致
            mainHandler.postDelayed(this::reshowCurrentAnnounce, ANNOUNCE_ATTACH_RETRY_MS);
        }
    }

    public void expand() {
        setExpanded(true);
    }

    public void showToast(String text) {
        showToast(text, inferToastKind(text));
    }

    /** @param kind success | error | info */
    public void showToast(String text, String kind) {
        final String msg = text != null ? text : "";
        final String k = kind != null ? kind : "info";
        mainHandler.post(() -> showMagicToast(msg, k));
    }

    private static String inferToastKind(String text) {
        if (text == null) return "info";
        if (text.contains("失败") || text.contains("异常") || text.contains("错误")
                || text.contains("维护") || text.contains("请先") || text.contains("不可")
                || text.contains("未开放") || text.contains("未登录") || text.contains("未授权")
                || text.contains("已用过自助解绑")) {
            return "error";
        }
        if (text.contains("成功") || text.contains("已开放") || text.contains("已恢复")
                || text.contains("已解绑") || text.contains("已执行") || text.contains("已关闭卡密")
                || text.contains("已退出")) {
            return "success";
        }
        return "info";
    }

    private void showMagicToast(String msg, String kind) {
        try {
            if (windowManager != null && attached) {
                ensureWmToast();
                applyToastStyle(kind);
                toastText.setText(msg);
                mainHandler.removeCallbacks(toastHideRunnable);
                toastOverlay.setVisibility(View.VISIBLE);
                toastOverlay.setAlpha(0f);
                toastOverlay.setTranslationY(dp(18));
                toastOverlay.animate().cancel();
                toastOverlay.animate().alpha(1f).translationY(0f).setDuration(280).start();
                // 长文多给时间读完（开屏免责声明等）
                long dur = Math.min(6000L, Math.max(2200L, 1600L + msg.length() * 45L));
                mainHandler.postDelayed(toastHideRunnable, dur);
                return;
            }
        } catch (Exception e) {
            Log.w(TAG, "magic toast wm failed: " + e.getMessage());
        }
        Toast.makeText(appContext, msg, Toast.LENGTH_SHORT).show();
    }

    private void ensureWmToast() {
        if (toastOverlay != null && toastOverlay.getParent() != null) return;
        LinearLayout box = new LinearLayout(appContext);
        box.setOrientation(LinearLayout.HORIZONTAL);
        box.setGravity(Gravity.CENTER_VERTICAL);
        box.setBackground(MagicRes.toastBg(appContext));
        box.setPadding(dp(18), dp(12), dp(18), dp(12));
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            box.setElevation(dp(8));
        }
        box.setClickable(false);
        box.setFocusable(false);

        ImageView icon = new ImageView(appContext);
        icon.setScaleType(ImageView.ScaleType.CENTER_INSIDE);
        LinearLayout.LayoutParams ilp = new LinearLayout.LayoutParams(dp(16), dp(16));
        ilp.rightMargin = dp(8);
        box.addView(icon, ilp);

        TextView tv = new TextView(appContext);
        tv.setTextColor(MagicRes.INK);
        setFs(tv, 13f);
        tv.setMaxLines(8);
        tv.setEllipsize(null);
        int screenW = appContext.getResources().getDisplayMetrics().widthPixels;
        // 左右各留约 24dp，长句可完整换行显示
        tv.setMaxWidth(Math.max(dp(240), screenW - dp(48)));
        box.addView(tv, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        toastOverlay = box;
        toastIcon = icon;
        toastText = tv;

        toastLp = new WindowManager.LayoutParams(
                ViewGroup.LayoutParams.WRAP_CONTENT,
                ViewGroup.LayoutParams.WRAP_CONTENT,
                0, 0,
                layoutParams != null ? layoutParams.type : WindowManager.LayoutParams.TYPE_APPLICATION,
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
                        | WindowManager.LayoutParams.FLAG_NOT_TOUCHABLE
                        | WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN
                        | WindowManager.LayoutParams.FLAG_HARDWARE_ACCELERATED,
                PixelFormat.TRANSLUCENT
        );
        toastLp.gravity = Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL;
        toastLp.y = dp(96);
        if (layoutParams != null) {
            toastLp.token = layoutParams.token;
        }
        windowManager.addView(toastOverlay, toastLp);
        toastOverlay.setVisibility(View.GONE);
    }

    private void applyToastStyle(String kind) {
        if (toastOverlay == null || toastIcon == null) return;
        GradientDrawable bg = MagicRes.toastBg(appContext);
        toastOverlay.setBackground(bg);
        if ("success".equals(kind)) {
            toastIcon.setImageDrawable(MagicRes.toastOk(appContext));
            toastIcon.clearColorFilter();
            bg.setStroke(dp(1), 0x4D7EE0A0);
        } else if ("error".equals(kind)) {
            toastIcon.setImageDrawable(MagicRes.toastErr(appContext));
            toastIcon.clearColorFilter();
            bg.setStroke(dp(1), 0x4DE07E7E);
        } else {
            toastIcon.setImageDrawable(MagicRes.toastSparkles(appContext));
            toastIcon.setColorFilter(MagicRes.BRAND);
            bg.setStroke(dp(1), 0x408B5CF6);
        }
    }

    private void hideMagicToast() {
        if (toastOverlay == null) return;
        toastOverlay.animate().cancel();
        toastOverlay.animate().alpha(0f).translationY(dp(14)).setDuration(220)
                .withEndAction(() -> {
                    if (toastOverlay != null) toastOverlay.setVisibility(View.GONE);
                }).start();
    }

    private void removeWmToast() {
        mainHandler.removeCallbacks(toastHideRunnable);
        if (toastOverlay != null && windowManager != null && toastOverlay.getParent() != null) {
            try {
                toastOverlay.animate().cancel();
                windowManager.removeView(toastOverlay);
            } catch (Exception ignored) {}
        }
        toastOverlay = null;
        toastText = null;
        toastIcon = null;
        toastLp = null;
    }

    public void refreshFeatures() {
        if (featureList == null) return;
        featureList.removeAllViews();
        currentCategoryBody = null;
        categoryBuildIndex = 0;
        String[] specs = bridge.getFeatureListRaw();
        if (specs == null) {
            addHint(featureList, "暂无功能列表（native 未就绪）");
            return;
        }
        for (int i = 0; i < specs.length; i++) {
            FeatureListParser.Item item = FeatureListParser.parse(i, specs[i]);
            addFeatureRow(item);
        }
        syncTrailingDividers(featureList);
        scheduleDeferredRestore();
    }

    private Runnable deferredRestoreRunnable;

    /**
     * 启动竞态防护（2026-08-29 公主连结实证）：行构建早于 native session 就绪时，
     * setChecked 恢复推送会走授权失败路径 forceFeatureOff，把保存的功能偏好清掉，
     * 之后 reapply 每 60s 忠实推 OFF。会话就绪后再统一按偏好 reapply 一次（幂等）。
     */
    private void scheduleDeferredRestore() {
        if (deferredRestoreRunnable == null) {
            deferredRestoreRunnable = new Runnable() {
                private int tries = 0;
                @Override public void run() {
                    if (bridge.hasLiveNativeSession()) {
                        bridge.reapplySavedFeaturesToNative();
                        return;
                    }
                    if (++tries <= 30) mainHandler.postDelayed(this, 1000L);
                }
            };
        }
        mainHandler.removeCallbacks(deferredRestoreRunnable);
        mainHandler.postDelayed(deferredRestoreRunnable, 800L);
    }

    private static final Object ROW_DIVIDER_TAG = new Object();
    /** 分类内容容器标记：末项 divider 在 sync 时自动隐藏 */
    private static final Object CATEGORY_BODY_TAG = new Object();

    /** 各页最末一条可见 row divider 去掉，避免与底栏 Tab 顶线叠成双线；
     *  分类内容区内末项 divider 也去掉，避免与下一分类标题「双线」。 */
    private void syncAllPaneTrailingDividers() {
        syncTrailingDividers(featureList);
        syncTrailingDividers(settingsPane);
        syncTrailingDividers(accountMainBox);
    }

    private void syncTrailingDividers(ViewGroup root) {
        if (root == null) return;
        List<View> dividers = new ArrayList<>();
        collectRowDividers(root, dividers);
        for (View d : dividers) {
            d.setVisibility(View.VISIBLE);
        }
        List<ViewGroup> bodies = new ArrayList<>();
        collectCategoryBodies(root, bodies);
        if (!bodies.isEmpty()) {
            // 有分类：只藏每个分类内容区末项，避免与下一分类「双线」；
            // 勿再对 root 藏一次，否则会把末分类里功能之间的线也剥掉。
            for (ViewGroup body : bodies) {
                hideLastShownDivider(body);
            }
        } else {
            // 无分类（设置/账号等）：只藏整页最后一条，避免顶 Tab 双线
            hideLastShownDivider(root);
        }
    }

    private void hideLastShownDivider(ViewGroup root) {
        List<View> dividers = new ArrayList<>();
        collectRowDividers(root, dividers);
        View lastShown = null;
        for (View d : dividers) {
            if (isViewTreeShown(d, root)) lastShown = d;
        }
        if (lastShown != null) lastShown.setVisibility(View.GONE);
    }

    private void collectCategoryBodies(ViewGroup group, List<ViewGroup> out) {
        for (int i = 0; i < group.getChildCount(); i++) {
            View c = group.getChildAt(i);
            if (c.getTag() == CATEGORY_BODY_TAG && c instanceof ViewGroup) {
                out.add((ViewGroup) c);
            } else if (c instanceof ViewGroup) {
                collectCategoryBodies((ViewGroup) c, out);
            }
        }
    }

    private void collectRowDividers(ViewGroup group, List<View> out) {
        for (int i = 0; i < group.getChildCount(); i++) {
            View c = group.getChildAt(i);
            if (c.getTag() == ROW_DIVIDER_TAG) {
                out.add(c);
            } else if (c instanceof ViewGroup) {
                collectRowDividers((ViewGroup) c, out);
            }
        }
    }

    private boolean isViewTreeShown(View v, ViewGroup root) {
        View cur = v;
        while (cur != null) {
            if (cur.getVisibility() != View.VISIBLE) return false;
            if (cur == root) return true;
            Object p = cur.getParent();
            cur = p instanceof View ? (View) p : null;
        }
        return false;
    }

    /** 当前分类内容容器；无分类时落到根列表 */
    private LinearLayout featureHost() {
        return currentCategoryBody != null ? currentCategoryBody : featureList;
    }

    // ---- attach ----

    boolean canOverlay() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M) return true;
        return Settings.canDrawOverlays(appContext);
    }

    private Activity findResumedHint(Activity preferred) {
        if (preferred != null && !preferred.isFinishing()) return preferred;
        // 从 MenuService 拿最近 resumed 的 Activity
        return MenuService.getLastResumedActivity();
    }

    private boolean tryAttachActivity(Activity activity) {
        if (activity == null || activity.isFinishing()) return false;
        try {
            WindowManager wm = activity.getWindowManager();
            WindowManager.LayoutParams lp = baseLayoutParams();
            lp.type = WindowManager.LayoutParams.TYPE_APPLICATION;
            // Activity WM 自带 token，勿手填 null token
            wm.addView(root, lp);
            windowManager = wm;
            layoutParams = lp;
            boundActivity = activity;
            attachMode = AttachMode.ACTIVITY;
            attached = true;
            applyCollapsedGeometry();
            Log.i(TAG, "attached ACTIVITY host=" + activity.getClass().getSimpleName());
            return true;
        } catch (Exception e) {
            Log.w(TAG, "tryAttachActivity: " + e.getMessage());
            return false;
        }
    }

    private boolean tryAttachOverlay() {
        try {
            WindowManager wm = (WindowManager) appContext.getSystemService(Context.WINDOW_SERVICE);
            WindowManager.LayoutParams lp = baseLayoutParams();
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                lp.type = WindowManager.LayoutParams.TYPE_APPLICATION_OVERLAY;
            } else {
                lp.type = WindowManager.LayoutParams.TYPE_PHONE;
            }
            lp.alpha = OVERLAY_SAFE_ALPHA;
            wm.addView(root, lp);
            windowManager = wm;
            layoutParams = lp;
            boundActivity = null;
            attachMode = AttachMode.OVERLAY;
            attached = true;
            applyCollapsedGeometry();
            Log.i(TAG, "attached OVERLAY");
            return true;
        } catch (Exception e) {
            Log.e(TAG, "tryAttachOverlay: " + e.getMessage());
            return false;
        }
    }

    void detachWindowOnly() {
        dismissMagicDropdown();
        removeWmToast();
        removeMagicAnnounce();
        if (announceShowing && currentAnnounce != null) {
            // 公告随窗口被拆（Activity 切换）：延迟重试等重挂，不丢公告、不死锁队列
            mainHandler.postDelayed(this::reshowCurrentAnnounce, ANNOUNCE_ATTACH_RETRY_MS);
        }
        if (windowManager != null && root != null && root.getParent() != null) {
            try {
                windowManager.removeView(root);
            } catch (Exception ignored) {}
        }
        attached = false;
        attachMode = AttachMode.NONE;
        boundActivity = null;
        windowManager = null;
        layoutParams = null;
    }

    private WindowManager.LayoutParams baseLayoutParams() {
        WindowManager.LayoutParams lp = new WindowManager.LayoutParams(
                dp(FAB_WM_DP), dp(FAB_WM_DP),
                0, 0,
                WindowManager.LayoutParams.TYPE_APPLICATION,
                WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
                        | WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN
                        | WindowManager.LayoutParams.FLAG_HARDWARE_ACCELERATED,
                PixelFormat.TRANSLUCENT
        );
        lp.gravity = Gravity.TOP | Gravity.START;
        lp.x = dp(12);
        lp.y = dp(120);
        return lp;
    }

    // ---- UI ----

    private void buildUiIfNeeded() {
        if (root != null) return;
        try {
            root = new FrameLayout(appContext);
            root.setClipChildren(false);
            root.setClipToPadding(false);

            fab = buildFab();
            fab.setOnTouchListener(this::onWindowDrag);
            root.addView(fab);

            panel = buildPanel();
            panel.setVisibility(View.GONE);
            panel.setAlpha(0f);
            panel.setScaleX(0.92f);
            panel.setScaleY(0.92f);
            root.addView(panel);

            setExpanded(false);
            refreshFeatures();
            refreshAuthStatus();
            updateAccountTabVisibility();
        } catch (RuntimeException e) {
            // 避免半截 UI 导致后续 show 直接 return、球永远不出现
            if (root != null) {
                root.removeAllViews();
                root = null;
            }
            fab = null;
            fabIconView = null;
            panel = null;
            throw e;
        }
    }

    private FrameLayout buildFab() {
        FrameLayout ball = new FrameLayout(appContext);
        ball.setLayoutParams(new FrameLayout.LayoutParams(dp(FAB_WM_DP), dp(FAB_WM_DP)));
        ball.setClipChildren(false);

        fabIconView = new ImageView(appContext);
        fabIconView.setImageDrawable(MagicRes.fabIcon(appContext));
        fabIconView.setScaleType(ImageView.ScaleType.FIT_CENTER);
        fabIconView.setLayoutParams(new FrameLayout.LayoutParams(
                dp(FAB_ICON_DP), dp(FAB_ICON_DP), Gravity.CENTER));
        ball.addView(fabIconView);
        return ball;
    }

    private View buildPanel() {
        final int overhang = dp(VINE_OVERHANG_DP);
        FrameLayout shell = new FrameLayout(appContext);
        // 外壳比面板宽一圈，给花藤探出边框的空间（窗口本身也会按此尺寸算）
        FrameLayout.LayoutParams plp = new FrameLayout.LayoutParams(
                dp(PANEL_INNER_W_DP) + overhang * 2, ViewGroup.LayoutParams.WRAP_CONTENT);
        shell.setLayoutParams(plp);
        shell.setClipToPadding(false);
        shell.setClipChildren(false);

        FrameLayout card = new FrameLayout(appContext);
        card.setBackground(MagicRes.panelBg(appContext));
        // elevation 会盖住同级无 elevation 的花藤内侧；阴影改挂外壳
        card.setElevation(0f);
        card.setClipToPadding(true);
        card.setClipChildren(true);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            card.setClipToOutline(true);
            final float radius = dp(16);
            card.setOutlineProvider(new ViewOutlineProvider() {
                @Override
                public void getOutline(View view, Outline outline) {
                    outline.setRoundRect(0, 0, view.getWidth(), view.getHeight(), radius);
                }
            });
        }
        FrameLayout.LayoutParams cardLp = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        cardLp.setMargins(overhang, overhang, overhang, overhang);
        shell.addView(card, cardLp);
        shell.setElevation(dp(12));

        // 花藤挂在外壳四角，横跨描边（外探 + 内侧压边）
        addVineCorner(shell, Gravity.TOP | Gravity.START);
        addVineCorner(shell, Gravity.TOP | Gravity.END);
        addVineCorner(shell, Gravity.BOTTOM | Gravity.START);
        addVineCorner(shell, Gravity.BOTTOM | Gravity.END);

        LinearLayout box = new LinearLayout(appContext);
        box.setOrientation(LinearLayout.VERTICAL);
        box.setPadding(dp(18), dp(16), dp(18), dp(16));
        card.addView(box, new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        // Header：标题 + 授权状态副标题；整块可拖
        FrameLayout header = new FrameLayout(appContext);
        header.setPadding(0, dp(SPACE_TIGHT), 0, dp(SPACE_TIGHT));
        LinearLayout.LayoutParams hlp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        box.addView(header, hlp);
        header.setOnTouchListener((v, e) -> {
            if (e.getActionMasked() == MotionEvent.ACTION_DOWN
                    && e.getX() > v.getWidth() - dp(44)) {
                return false; // 留给收起按钮
            }
            return onWindowDrag(v, e);
        });

        LinearLayout titleBlock = new LinearLayout(appContext);
        titleBlock.setOrientation(LinearLayout.VERTICAL);
        titleBlock.setGravity(Gravity.CENTER_HORIZONTAL);
        titleBlock.setPadding(dp(36), 0, dp(36), 0);
        FrameLayout.LayoutParams titleBlockLp = new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT,
                Gravity.CENTER_VERTICAL);
        header.addView(titleBlock, titleBlockLp);

        TextView title = new TextView(appContext);
        title.setText("星光魔法菜单");
        title.setTextColor(MagicRes.INK);
        setFs(title, FS_TITLE);
        title.setTypeface(Typeface.DEFAULT_BOLD);
        title.setGravity(Gravity.CENTER);
        title.setLetterSpacing(0.02f);
        title.setIncludeFontPadding(false);
        titleBlock.addView(title, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        statusLine = new TextView(appContext);
        statusLine.setTextColor(MagicRes.INK_2);
        setFs(statusLine, FS_STATUS);
        statusLine.setGravity(Gravity.CENTER);
        statusLine.setIncludeFontPadding(false);
        statusLine.setAlpha(0.92f);
        LinearLayout.LayoutParams statusLp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        statusLp.topMargin = dp(2);
        titleBlock.addView(statusLine, statusLp);

        ImageView close = new ImageView(appContext);
        close.setImageDrawable(MagicRes.icCollapse(appContext));
        close.setBackground(MagicRes.closeChipBg(appContext));
        close.setScaleType(ImageView.ScaleType.CENTER);
        close.setPadding(dp(6), dp(6), dp(6), dp(6));
        close.setContentDescription("收起菜单");
        close.setAlpha(0.85f);
        FrameLayout.LayoutParams closeLp = new FrameLayout.LayoutParams(
                dp(32), dp(32), Gravity.END | Gravity.CENTER_VERTICAL);
        close.setOnClickListener(v -> setExpanded(false));
        header.addView(close, closeLp);

        box.addView(makeHairline(), hairlineLp(/*top*/ SPACE_TIGHT, /*bottom*/ SPACE));

        featureList = new LinearLayout(appContext);
        featureList.setOrientation(LinearLayout.VERTICAL);
        featScroll = wrapScroll(featureList);
        box.addView(featScroll);

        authPane = buildAuthPane();
        authScroll = wrapScroll(authPane);
        authScroll.setVisibility(View.GONE);
        box.addView(authScroll);

        settingsPane = buildSettingsPane();
        setScroll = wrapScroll(settingsPane);
        setScroll.setVisibility(View.GONE);
        box.addView(setScroll);

        // 内容 ↔ 底栏：与状态分隔同一套间距
        box.addView(makeHairline(), hairlineLp(SPACE, SPACE_TIGHT));

        LinearLayout tabs = new LinearLayout(appContext);
        tabs.setOrientation(LinearLayout.HORIZONTAL);
        tabs.setGravity(Gravity.CENTER_VERTICAL);
        tabFeat = makeTab("功能", true);
        tabAuth = makeTab("账号", false);
        tabSet = makeTab("设置", false);
        tabs.addView(tabFeat);
        tabs.addView(tabAuth);
        tabs.addView(tabSet);
        box.addView(tabs, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, dp(40)));

        Runnable relayout = this::requestPanelRelayout;

        tabFeat.setOnClickListener(v -> selectTab(0));
        tabAuth.setOnClickListener(v -> {
            selectTab(1);
            refreshAuthStatus();
            relayout.run();
        });
        tabSet.setOnClickListener(v -> selectTab(2));

        return shell;
    }

    /** @param which 0=功能 1=账号 2=设置 */
    private void selectTab(int which) {
        dismissMagicDropdown();
        if (tabFeat == null || featScroll == null) return;
        tabFeat.setSelected(which == 0);
        if (tabAuth != null) tabAuth.setSelected(which == 1);
        if (tabSet != null) tabSet.setSelected(which == 2);
        featScroll.setVisibility(which == 0 ? View.VISIBLE : View.GONE);
        if (authScroll != null) authScroll.setVisibility(which == 1 ? View.VISIBLE : View.GONE);
        if (setScroll != null) setScroll.setVisibility(which == 2 ? View.VISIBLE : View.GONE);
        // 账号登录页需要可焦点，否则输入框长按/剪贴板条出不来（不等输入框获焦，切页即放开）
        if (which == 1 && accountLoginBox != null
                && accountLoginBox.getVisibility() == View.VISIBLE) {
            setWindowFocusableForIme(true);
            MenuService.notifyOverlayInputFocus(true);
        } else if (cardInput == null || !cardInput.hasFocus()) {
            setWindowFocusableForIme(false);
            MenuService.notifyOverlayInputFocus(false);
        }
        if (root != null) root.post(this::requestPanelRelayout);
        else requestPanelRelayout();
    }

    /** 关卡密时隐藏账号 Tab；若正停在账号页则切回功能 */
    private void updateAccountTabVisibility() {
        if (tabAuth == null) return;
        boolean show = Preferences.isCardAuthRequired();
        tabAuth.setVisibility(show ? View.VISIBLE : View.GONE);
        if (!show && tabAuth.isSelected()) {
            selectTab(0);
        } else if (root != null) {
            root.post(this::requestPanelRelayout);
        }
    }

    private void addVineCorner(FrameLayout shell, int gravity) {
        ImageView iv = new ImageView(appContext);
        iv.setImageDrawable(MagicRes.vineCorner(appContext, gravity));
        iv.setAlpha(0.98f);
        iv.setScaleType(ImageView.ScaleType.FIT_XY);
        // 上下外探维持；左右再往里一点
        int overhang = dp(VINE_OVERHANG_DP);
        int mV = overhang - dp(14);
        int mH = overhang - dp(12); // 左右比「再往里」回调 2dp
        int left = 0, top = 0, right = 0, bottom = 0;
        if ((gravity & Gravity.START) == Gravity.START) left = mH;
        if ((gravity & Gravity.END) == Gravity.END) right = mH;
        if ((gravity & Gravity.TOP) == Gravity.TOP) top = mV;
        if ((gravity & Gravity.BOTTOM) == Gravity.BOTTOM) bottom = mV;
        FrameLayout.LayoutParams lp = new FrameLayout.LayoutParams(
                dp(VINE_CORNER_DP), dp(VINE_CORNER_DP), gravity);
        lp.setMargins(left, top, right, bottom);
        shell.addView(iv, lp);
        // 必须高于面板，否则 elevation/绘制顺序会把内侧藤蔓盖掉
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            iv.setTranslationZ(dp(16));
            iv.setElevation(dp(16));
        }
        iv.bringToFront();
    }

    private View makeHairline() {
        View line = new View(appContext);
        line.setBackground(MagicRes.statusDivider(appContext));
        return line;
    }

    private LinearLayout.LayoutParams hairlineLp(int topDp, int bottomDp) {
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, dp(1));
        lp.topMargin = dp(topDp);
        lp.bottomMargin = dp(bottomDp);
        lp.leftMargin = dp(SPACE);
        lp.rightMargin = dp(SPACE);
        return lp;
    }

    private void setFs(TextView t, float sp) {
        t.setTextSize(TypedValue.COMPLEX_UNIT_SP, sp);
    }

    private TextView makeTab(String label, boolean selected) {
        TextView t = new TextView(appContext);
        t.setText(label);
        t.setTextColor(MagicRes.INK);
        setFs(t, FS_TAB);
        t.setTypeface(Typeface.DEFAULT_BOLD);
        t.setIncludeFontPadding(false);
        t.setPadding(dp(8), 0, dp(8), 0);
        t.setBackground(MagicRes.tabBg(appContext));
        t.setSelected(selected);
        t.setGravity(Gravity.CENTER);
        t.setMinHeight(0);
        t.setMinimumHeight(0);
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(0, dp(36), 1f);
        lp.setMargins(dp(3), dp(2), dp(3), dp(2));
        t.setLayoutParams(lp);
        return t;
    }

    /** 内容高度变化后按实测高度收紧窗口（WM 的 WRAP_CONTENT 经常不回缩） */
    private void requestPanelRelayout() {
        if (root == null || !expanded) return;
        root.post(() -> {
            if (root == null || layoutParams == null || windowManager == null || !attached || !expanded) {
                return;
            }
            int width = layoutParams.width;
            if (width <= 0) {
                width = dp(PANEL_INNER_W_DP + VINE_OVERHANG_DP * 2);
            }
            root.measure(
                    View.MeasureSpec.makeMeasureSpec(width, View.MeasureSpec.EXACTLY),
                    View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));
            int h = root.getMeasuredHeight();
            if (h <= 0) return;
            // 兜底：窗口总高不得超屏幕 9 成。内容区已按屏动态限高，此处防预算
            // 偏差/异常装饰超高把底部 tab 栏挤出屏幕。
            try {
                int maxWinH = Math.round(appContext.getResources()
                        .getDisplayMetrics().heightPixels * 0.9f);
                if (h > maxWinH) h = maxWinH;
            } catch (Throwable ignored) {}
            layoutParams.width = width;
            layoutParams.height = h;
            try {
                windowManager.updateViewLayout(root, layoutParams);
                clampWindowOnScreen();
            } catch (Exception e) {
                Log.w(TAG, "requestPanelRelayout: " + e.getMessage());
                String msg = e.getMessage() != null ? e.getMessage() : "";
                if (msg.contains("token") || msg.contains("not attached")
                        || msg.contains("BadToken") || msg.contains("dead")) {
                    ensureAliveOrRebind(findResumedHint(boundActivity));
                }
            }
        });
    }

    /** 内容滚动区高度上限（px）：小屏（横屏/低分辨率/大显示缩放）按屏幕 dp 视口收缩，
     *  正常屏维持 PANEL_LIST_MAX_DP，保证面板总高不超过屏幕高度的约 82%，
     *  底部 tab 栏（功能/账号/设置）始终可见可点。onMeasure 时动态取值，
     *  旋转/密度变化后重测自适应。 */
    private int contentScrollMaxH() {
        android.util.DisplayMetrics dm = appContext.getResources().getDisplayMetrics();
        int screenHDp = Math.max(1, Math.round(dm.heightPixels / dm.density));
        int budgetDp = Math.round(screenHDp * 0.82f) - PANEL_CHROME_H_DP;
        if (budgetDp > PANEL_LIST_MAX_DP) budgetDp = PANEL_LIST_MAX_DP;
        if (budgetDp < PANEL_LIST_MIN_DP) budgetDp = PANEL_LIST_MIN_DP;
        return dp(budgetDp);
    }

    private ScrollView wrapScroll(View child) {
        ScrollView sv = new ScrollView(appContext) {
            @Override
            protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
                // 上限动态取：小屏收缩防面板总高超屏裁掉底部 tab 栏
                final int maxH = contentScrollMaxH();
                // 先测内容真实高度，矮则跟内容，高过 maxH 才定高滚动
                if (getChildCount() == 0) {
                    setMeasuredDimension(
                            getDefaultSize(getSuggestedMinimumWidth(), widthMeasureSpec), 0);
                    return;
                }
                View c = getChildAt(0);
                int horiz = getPaddingLeft() + getPaddingRight();
                int vert = getPaddingTop() + getPaddingBottom();
                int childW = getChildMeasureSpec(
                        widthMeasureSpec, horiz,
                        c.getLayoutParams() != null
                                ? c.getLayoutParams().width
                                : ViewGroup.LayoutParams.MATCH_PARENT);
                c.measure(childW, MeasureSpec.makeMeasureSpec(0, MeasureSpec.UNSPECIFIED));
                int contentH = c.getMeasuredHeight() + vert;
                int width = resolveSize(
                        Math.max(getSuggestedMinimumWidth(), c.getMeasuredWidth() + horiz),
                        widthMeasureSpec);
                if (contentH <= maxH) {
                    // 精确高度 = 内容，避免 ScrollView 吃掉 AT_MOST 上限留下空白
                    super.onMeasure(
                            MeasureSpec.makeMeasureSpec(width, MeasureSpec.EXACTLY),
                            MeasureSpec.makeMeasureSpec(contentH, MeasureSpec.EXACTLY));
                } else {
                    super.onMeasure(
                            MeasureSpec.makeMeasureSpec(width, MeasureSpec.EXACTLY),
                            MeasureSpec.makeMeasureSpec(maxH, MeasureSpec.EXACTLY));
                }
            }
        };
        sv.setFillViewport(false);
        sv.setOverScrollMode(View.OVER_SCROLL_IF_CONTENT_SCROLLS);
        sv.setVerticalScrollBarEnabled(false);
        sv.setLayoutParams(new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        sv.addView(child, new ScrollView.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        return sv;
    }

    private LinearLayout buildAuthPane() {
        LinearLayout box = new LinearLayout(appContext);
        box.setOrientation(LinearLayout.VERTICAL);

        accountLoginBox = new LinearLayout(appContext);
        accountLoginBox.setOrientation(LinearLayout.VERTICAL);
        accountLoginBox.setGravity(Gravity.CENTER_HORIZONTAL);

        accountLoginBox.setPadding(0, 0, 0, 0);

        TextView loginTitle = new TextView(appContext);
        loginTitle.setText("账号登录");
        loginTitle.setTextColor(MagicRes.INK);
        loginTitle.setTypeface(Typeface.DEFAULT_BOLD);
        setFs(loginTitle, FS_LOGIN_TITLE);
        loginTitle.setGravity(Gravity.CENTER);
        loginTitle.setPadding(0, dp(SPACE_TIGHT), 0, dp(SPACE_TIGHT));
        accountLoginBox.addView(loginTitle, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        cardInput = new EditText(appContext);
        cardInput.setHint("请输入账号");
        cardInput.setHintTextColor(MagicRes.INK_3);
        cardInput.setTextColor(MagicRes.INK);
        cardInput.setBackground(MagicRes.inputBg(appContext));
        cardInput.setGravity(Gravity.CENTER);
        cardInput.setTypeface(Typeface.MONOSPACE);
        setFs(cardInput, 13f);
        cardInput.setIncludeFontPadding(false);
        cardInput.setSingleLine(true);
        cardInput.setMaxLines(1);
        cardInput.setMinHeight(0);
        cardInput.setMinimumHeight(0);
        cardInput.setPadding(dp(14), dp(9), dp(14), dp(9));
        cardInput.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS);
        cardInput.setOnFocusChangeListener((v, hasFocus) -> {
            setWindowFocusableForIme(hasFocus);
            MenuService.notifyOverlayInputFocus(hasFocus);
        });
        // 悬浮窗里系统长按菜单不可用：自绘全选/剪切/复制/粘贴弹条（111 已验证方案）
        enableOverlayNativeEditClipboard(cardInput);
        LinearLayout.LayoutParams inputLp = new LinearLayout.LayoutParams(dp(240), dp(36));
        inputLp.gravity = Gravity.CENTER_HORIZONTAL;
        accountLoginBox.addView(cardInput, inputLp);

        Button login = magicButton("登录");
        login.setOnClickListener(v -> doAuthLogin());
        LinearLayout.LayoutParams loginLp = (LinearLayout.LayoutParams) login.getLayoutParams();
        loginLp.width = dp(240);
        loginLp.height = dp(34);
        loginLp.topMargin = dp(SPACE);
        loginLp.gravity = Gravity.CENTER_HORIZONTAL;
        login.setLayoutParams(loginLp);
        accountLoginBox.addView(login);
        box.addView(accountLoginBox);

        accountMainBox = new LinearLayout(appContext);
        accountMainBox.setOrientation(LinearLayout.VERTICAL);
        accountMainBox.setVisibility(View.GONE);
        accountMainBox.setPadding(0, 0, 0, 0);

        accountExpireTv = addOptionValueRow(accountMainBox, "账号时间", "—");
        accountUsesTv = addOptionValueRow(accountMainBox, "剩余次数", "不限");

        LinearLayout unbindRow = addOptionRowShell(accountMainBox, "设备解绑");
        accountUnbindBtn = execButton("解绑", false);
        accountUnbindBtn.setOnClickListener(v -> doUnbind());
        unbindRow.addView(accountUnbindBtn);

        LinearLayout logoutRow = addOptionRowShell(accountMainBox, "退出登录");
        Button logout = execButton("退出", true);
        logout.setOnClickListener(v -> {
            bridge.clearAuthSession();
            Preferences.saveAuthCardMeta("{}");
            updateAccountTabVisibility();
            refreshAuthStatus();
            refreshFeatures();
            showToast("已退出登录");
        });
        logoutRow.addView(logout);

        box.addView(accountMainBox);
        syncTrailingDividers(accountMainBox);
        return box;
    }

    private static final String DISCLAIMER =
            "本模组由 AI 生成，仅供个人学习与技术研究，不得用于商业用途。"
                    + "安装、启用或使用本模组，即表示您已知晓并自愿承担全部风险（含账号封禁、数据丢失、设备异常及法律纠纷等）。"
                    + "因使用产生的一切后果均由使用者自行承担。";

    private LinearLayout buildSettingsPane() {
        LinearLayout box = new LinearLayout(appContext);
        box.setOrientation(LinearLayout.VERTICAL);

        TextView disclaimer = new TextView(appContext);
        disclaimer.setText(DISCLAIMER);
        disclaimer.setTextColor(MagicRes.INK_2);
        setFs(disclaimer, FS_HINT);
        disclaimer.setLineSpacing(dp(2), 1f);
        disclaimer.setPadding(dp(SPACE), dp(SPACE), dp(SPACE), dp(SPACE));
        disclaimer.setBackground(MagicRes.rowBg(appContext));
        LinearLayout.LayoutParams dlp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        dlp.bottomMargin = dp(SPACE);
        box.addView(disclaimer, dlp);

        LinearLayout otaRow = addOptionRowShell(box, "游戏补丁");
        TextView otaLab = (TextView) otaRow.getChildAt(0);
        if (otaLab != null) {
            String st = PlayOtaManager.localStatusText(appContext);
            otaLab.setText("游戏补丁\n" + st);
            otaLab.setLineSpacing(0f, 1.15f);
        }
        Button otaBtn = execButton("检查更新", false);
        otaBtn.setOnClickListener(v -> startPlayOtaCheck(otaBtn));
        otaRow.addView(otaBtn);

        LinearLayout otaClearRow = addOptionRowShell(box, "清除游戏补丁");
        Button otaClearBtn = execButton("清除", false);
        otaClearBtn.setOnClickListener(v -> confirmClearPlayOta());
        otaClearRow.addView(otaClearBtn);

        LinearLayout hideRow = addOptionRowShell(box, "隐藏菜单");
        TextView hideLab = (TextView) hideRow.getChildAt(0);
        if (hideLab != null) {
            hideLab.setText("隐藏菜单\n摇一摇可恢复");
            hideLab.setLineSpacing(0f, 1.15f);
        }
        Button hideBtn = execButton("隐藏", false);
        hideBtn.setOnClickListener(v -> hideMenuFromSettings());
        hideRow.addView(hideBtn);

        LinearLayout closeRow = addOptionRowShell(box, "关闭菜单");
        Button exit = execButton("退出", true);
        exit.setOnClickListener(v -> {
            showToast("正在关闭菜单…");
            bridge.closeMenu();
        });
        closeRow.addView(exit);
        syncTrailingDividers(box);
        return box;
    }

    /** 设置页：隐藏悬浮 UI（功能仍开），摇一摇恢复。 */
    private void hideMenuFromSettings() {
        MenuService service = MenuService.getInstance();
        if (service != null && service.isMenuUserHidden()) {
            Toast.makeText(appContext, "菜单已隐藏，摇一摇恢复", Toast.LENGTH_SHORT).show();
            return;
        }
        if (service != null) {
            service.hideMenuFromUser();
        }
    }

    private org.json.JSONObject pendingOtaManifest;

    /** 设置页：检查 / 下载玩法 OTA（JSON+so），成功后须完全重启。 */
    private void startPlayOtaCheck(Button btn) {
        if (btn != null && pendingOtaManifest != null
                && "确认下载".contentEquals(btn.getText() == null ? "" : btn.getText())) {
            org.json.JSONObject man = pendingOtaManifest;
            pendingOtaManifest = null;
            btn.setText("检查更新");
            startPlayOtaDownload(btn, man);
            return;
        }
        if (btn != null) btn.setEnabled(false);
        showToast("正在检查游戏补丁…");
        runAuth(() -> {
            PlayOtaManager.CheckResult cr = PlayOtaManager.check(appContext);
            mainHandler.post(() -> {
                if (btn != null) btn.setEnabled(true);
                if (!cr.ok) {
                    showToast(cr.message == null ? "检查失败" : cr.message);
                    return;
                }
                if (!cr.hasUpdate) {
                    showToast(cr.message);
                    return;
                }
                Activity host = focusedHostActivity();
                if (host != null && !host.isFinishing()) {
                    String body = "发现游戏补丁 v" + cr.remoteVer
                            + (cr.note == null || cr.note.isEmpty() ? "" : ("\n" + cr.note))
                            + "\n\n将合并覆盖本机补丁（JSON/so 可共存）。下载后须完全划掉游戏再开。";
                    new AlertDialog.Builder(host)
                            .setTitle("更新游戏补丁")
                            .setMessage(body)
                            .setNegativeButton("取消", null)
                            .setPositiveButton("下载", (d, w) -> startPlayOtaDownload(btn, cr.manifest))
                            .show();
                    return;
                }
                pendingOtaManifest = cr.manifest;
                if (btn != null) btn.setText("确认下载");
                showToast("发现 v" + cr.remoteVer + "，再点「确认下载」。完成后请划掉游戏再开");
            });
        });
    }

    private void startPlayOtaDownload(Button btn, org.json.JSONObject manifest) {
        if (btn != null) btn.setEnabled(false);
        showToast("正在下载游戏补丁…");
        final long[] lastPct = {-2L};
        runAuth(() -> {
            PlayOtaManager.ApplyResult ar = PlayOtaManager.downloadAndApply(appContext, manifest,
                    (done, total) -> {
                        if (btn == null) return;
                        final long pct = (total > 0) ? (done * 100 / total) : -1L;
                        if (pct == lastPct[0]) return;
                        lastPct[0] = pct;
                        mainHandler.post(() -> {
                            btn.setEnabled(false);
                            if (pct >= 0) {
                                btn.setText("下载中 " + pct + "%");
                            } else {
                                btn.setText("下载中…");
                            }
                        });
                    });
            mainHandler.post(() -> {
                if (btn != null) {
                    btn.setEnabled(true);
                    btn.setText("检查更新");
                }
                showToast(ar.message == null ? (ar.ok ? "完成" : "失败") : ar.message);
            });
        });
    }

    private void confirmClearPlayOta() {
        Activity host = focusedHostActivity();
        String body = "将删除本机已下载的游戏补丁（starry_ota），下次启动使用安装包内的 so。\n"
                + "清除后须完全划掉游戏再开。";
        if (host != null && !host.isFinishing()) {
            new AlertDialog.Builder(host)
                    .setTitle("清除游戏补丁")
                    .setMessage(body)
                    .setNegativeButton("取消", null)
                    .setPositiveButton("清除", (d, w) -> doClearPlayOta())
                    .show();
            return;
        }
        doClearPlayOta();
    }

    private void doClearPlayOta() {
        showToast("正在清除…");
        runAuth(() -> {
            PlayOtaManager.ClearResult r = PlayOtaManager.clearLocalOta(appContext);
            mainHandler.post(() -> showToast(r.message == null
                    ? (r.ok ? "已清除" : "清除失败") : r.message));
        });
    }

    private Activity focusedHostActivity() {
        try {
            if (root != null && root.getContext() instanceof Activity) {
                return (Activity) root.getContext();
            }
            if (panel != null && panel.getContext() instanceof Activity) {
                return (Activity) panel.getContext();
            }
        } catch (Throwable ignored) {
        }
        return null;
    }

    private LinearLayout addOptionRowShell(LinearLayout parent, String label) {
        LinearLayout row = new LinearLayout(appContext);
        row.setOrientation(LinearLayout.HORIZONTAL);
        row.setGravity(Gravity.CENTER_VERTICAL);
        row.setPadding(dp(SPACE_TIGHT), dp(ROW_PAD_V), dp(SPACE_TIGHT), dp(ROW_PAD_V));
        LinearLayout.LayoutParams rlp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        row.setLayoutParams(rlp);

        TextView lab = new TextView(appContext);
        lab.setText(label);
        lab.setTextColor(MagicRes.INK);
        setFs(lab, FS_BODY);
        lab.setLayoutParams(new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f));
        row.addView(lab);
        parent.addView(row);
        addRowDivider(parent);
        return row;
    }

    private void addRowDivider(LinearLayout parent) {
        View line = new View(appContext);
        line.setTag(ROW_DIVIDER_TAG);
        line.setBackground(MagicRes.rowDivider(appContext));
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, dp(1));
        // 与功能行左右 padding 一致，避免线比文字/控件更「冒头」
        lp.leftMargin = dp(SPACE_TIGHT);
        lp.rightMargin = dp(SPACE_TIGHT);
        parent.addView(line, lp);
    }

    private TextView addOptionValueRow(LinearLayout parent, String label, String value) {
        LinearLayout row = addOptionRowShell(parent, label);
        TextView tv = new TextView(appContext);
        tv.setText(value);
        tv.setTextColor(MagicRes.BRAND);
        setFs(tv, FS_VALUE);
        tv.setTypeface(Typeface.MONOSPACE);
        tv.setGravity(Gravity.END);
        row.addView(tv);
        return tv;
    }

    private Button execButton(String label, boolean danger) {
        Button b = new Button(appContext);
        b.setText(label);
        b.setAllCaps(false);
        // 镂空描边：字色跟描边走，不用深底 ink
        b.setTextColor(danger ? MagicRes.ERROR : MagicRes.BRAND_SOFT);
        b.setBackground(danger ? MagicRes.btnDanger(appContext) : MagicRes.btnExec(appContext));
        setFs(b, FS_BTN);
        b.setMinHeight(0);
        b.setMinimumHeight(0);
        b.setMinWidth(0);
        b.setMinimumWidth(0);
        b.setIncludeFontPadding(false);
        b.setPadding(dp(14), dp(6), dp(14), dp(6));
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            b.setStateListAnimator(null);
            b.setElevation(0f);
        }
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WRAP_CONTENT, dp(30));
        b.setLayoutParams(lp);
        return b;
    }

    private void styleMagicSwitch(Switch sw) {
        sw.setTextOn("");
        sw.setTextOff("");
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            sw.setShowText(false);
        }
        sw.setTrackDrawable(MagicRes.switchTrack(appContext));
        sw.setThumbDrawable(MagicRes.switchThumb(appContext));
        sw.setPadding(0, 0, 0, 0);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN) {
            sw.setSwitchMinWidth(dp(46));
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            sw.setSplitTrack(false);
            sw.setThumbTintList(null);
            sw.setTrackTintList(null);
        }
    }

    private Button magicButton(String label) {
        Button b = new Button(appContext);
        b.setText(label);
        b.setAllCaps(false);
        // 与 exec 一致：镂空描边胶囊
        b.setTextColor(MagicRes.BRAND_SOFT);
        b.setBackground(MagicRes.btnExec(appContext));
        setFs(b, FS_PRIMARY_BTN);
        b.setMinHeight(0);
        b.setMinimumHeight(0);
        b.setIncludeFontPadding(false);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            b.setStateListAnimator(null);
            b.setElevation(0f);
        }
        LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, dp(36));
        lp.topMargin = dp(8);
        b.setLayoutParams(lp);
        return b;
    }

    private void addHint(LinearLayout parent, String text) {
        TextView t = new TextView(appContext);
        t.setText(text);
        t.setTextColor(MagicRes.INK_3);
        setFs(t, FS_HINT);
        t.setPadding(0, dp(4), 0, dp(4));
        parent.addView(t);
    }

    private String authBlockedToast() {
        return authBlockedToast(-1);
    }

    private String authBlockedToast(int featNum) {
        int left = ModBridge.getAceGateRemainSec();
        if (left > 0) {
            return "过检测中，还剩 " + left + " 秒";
        }
        if (!bridge.hasLiveNativeSession()) {
            return "未授权，请先登录";
        }
        return "功能暂不可用";
    }

    /** 可交互：native session 活着 + 过检门已过 */
    private boolean featureInteractable(int featNum) {
        if (ModBridge.getAceGateRemainSec() > 0) return false;
        return bridge.hasLiveNativeSession();
    }

    private void ensureAceGateTicker() {
        if (aceGateTickerStarted) return;
        if (!"ltzj".equals(Preferences.getAuthGameId())) return;
        aceGateTickerStarted = true;
        aceGateWasBlocking = ModBridge.getAceGateRemainSec() > 0;
        mainHandler.post(aceGateTickRunnable);
    }

    private void addFeatureRow(FeatureListParser.Item item) {
        switch (item.kind) {
            case SKIP:
                return;
            case CATEGORY: {
                addCategorySection(item.name);
                return;
            }
            case TOGGLE:
            case CHECKBOX: {
                LinearLayout host = featureHost();
                LinearLayout row = new LinearLayout(appContext);
                row.setOrientation(LinearLayout.HORIZONTAL);
                row.setGravity(Gravity.CENTER_VERTICAL);
                row.setPadding(dp(SPACE_TIGHT), dp(ROW_PAD_V), dp(SPACE_TIGHT), dp(ROW_PAD_V));
                LinearLayout.LayoutParams rlp = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
                row.setLayoutParams(rlp);

                TextView lab = new TextView(appContext);
                lab.setText(item.name);
                lab.setTextColor(MagicRes.INK);
                setFs(lab, FS_BODY);
                lab.setLayoutParams(new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f));
                row.addView(lab);

                Switch sw = new Switch(appContext);
                styleMagicSwitch(sw);
                boolean canUse = featureInteractable(item.featNum);
                boolean prefOn = Preferences.loadPrefBool(item.featNum, false);
                // 先挂 listener 再 setChecked：prefs 恢复为开时必须回调 native，
                // 否则 UI 显示已开、g_features 仍为 false →「一打开没效果」
                if (!canUse) {
                    sw.setAlpha(0.45f);
                    attachBlockedToggle(sw, item.featNum);
                    sw.setChecked(false);
                } else {
                    attachToggleListener(sw, item.featNum, item.raw);
                    sw.setChecked(prefOn);
                }
                row.addView(sw);
                host.addView(row);
                addRowDivider(host);
                return;
            }
            case BUTTON: {
                LinearLayout host = featureHost();
                LinearLayout row = addOptionRowShell(host, item.name);
                Button btn = execButton("执行", false);
                final int num = item.featNum;
                final String raw = item.raw;
                boolean canUse = featureInteractable(num);
                if (!canUse) {
                    btn.setAlpha(0.45f);
                    btn.setOnClickListener(v -> showToast(authBlockedToast(num)));
                } else {
                    btn.setOnClickListener(v -> {
                        if (!bridge.onButtonClicked(num, raw)) {
                            showToast(authBlockedToast(num));
                        } else {
                            showToast("已执行：" + item.name);
                        }
                    });
                }
                row.addView(btn);
                return;
            }
            case SEEK: {
                LinearLayout host = featureHost();
                int min = 1, max = 5, def = min;
                if (item.extras != null && item.extras.length >= 2) {
                    try {
                        min = Integer.parseInt(item.extras[0]);
                        max = Integer.parseInt(item.extras[1]);
                    } catch (Exception ignored) {}
                }
                if (item.extras != null && item.extras.length >= 3) {
                    try {
                        def = Integer.parseInt(item.extras[2]);
                    } catch (Exception ignored) {
                        def = min;
                    }
                } else {
                    def = min;
                }
                if (max <= min) max = min + 1;
                int cur = FeatureListParser.seekCurrentValue(item);

                LinearLayout wrap = new LinearLayout(appContext);
                wrap.setOrientation(LinearLayout.VERTICAL);
                wrap.setPadding(dp(SPACE_TIGHT), dp(ROW_PAD_V), dp(SPACE_TIGHT), dp(ROW_PAD_V));
                LinearLayout.LayoutParams wlp = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
                wrap.setLayoutParams(wlp);

                LinearLayout top = new LinearLayout(appContext);
                top.setOrientation(LinearLayout.HORIZONTAL);
                top.setGravity(Gravity.CENTER_VERTICAL);
                wrap.addView(top, new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

                TextView lab = new TextView(appContext);
                lab.setTextColor(MagicRes.INK);
                setFs(lab, FS_BODY);
                lab.setText(item.name);
                lab.setLayoutParams(new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f));
                top.addView(lab);

                TextView val = new TextView(appContext);
                val.setText(String.valueOf(cur));
                val.setTextColor(MagicRes.BRAND);
                setFs(val, FS_VALUE);
                val.setTypeface(Typeface.MONOSPACE);
                val.setGravity(Gravity.CENTER);
                val.setMinWidth(dp(28));
                top.addView(val);

                SeekBar sb = new SeekBar(appContext);
                styleSeekBar(sb);
                sb.setMax(max - min);
                sb.setProgress(Math.max(0, Math.min(max - min, cur - min)));
                LinearLayout.LayoutParams sblp = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
                sblp.topMargin = dp(2);
                sb.setLayoutParams(sblp);
                final int fMin = min;
                final int num = item.featNum;
                final String raw = item.raw;
                final TextView fVal = val;
                final boolean canUse = featureInteractable(num);
                if (!canUse) {
                    sb.setEnabled(false);
                    sb.setAlpha(0.45f);
                    wrap.setOnClickListener(v -> showToast(authBlockedToast(num)));
                } else {
                    sb.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
                        @Override public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                            int v = fMin + progress;
                            fVal.setText(String.valueOf(v));
                            if (fromUser && !bridge.onSliderChanged(num, raw, v)) {
                                showToast(authBlockedToast(num));
                            }
                        }
                        @Override public void onStartTrackingTouch(SeekBar seekBar) {}
                        @Override public void onStopTrackingTouch(SeekBar seekBar) {}
                    });
                }
                wrap.addView(sb);
                host.addView(wrap);
                addRowDivider(host);
                return;
            }
            case SPINNER: {
                LinearLayout host = featureHost();
                LinearLayout row = addOptionRowShell(host, item.name);
                final String[] opts = item.extras != null ? item.extras : new String[0];
                final int num = item.featNum;
                final String raw = item.raw;
                int cur = Preferences.loadPrefInt(num);
                if (cur < 0 || cur >= opts.length) cur = 0;
                final int[] curIdx = {cur};

                LinearLayout trigger = new LinearLayout(appContext);
                trigger.setOrientation(LinearLayout.HORIZONTAL);
                trigger.setGravity(Gravity.CENTER_VERTICAL);
                trigger.setBackground(MagicRes.dropdownTrigger(appContext));
                trigger.setPadding(dp(8), dp(4), dp(6), dp(4));
                // 勿设过大 minWidth + weight，否则短文案与箭头会被拉开

                TextView val = new TextView(appContext);
                val.setText(opts.length > 0 ? opts[cur] : "—");
                val.setTextColor(MagicRes.INK);
                setFs(val, FS_VALUE);
                val.setIncludeFontPadding(false);
                val.setPadding(0, 0, 0, 0);
                trigger.addView(val, new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));

                ImageView arrow = new ImageView(appContext);
                arrow.setImageDrawable(MagicRes.dropdownChevron(appContext));
                arrow.setScaleType(ImageView.ScaleType.CENTER);
                arrow.setPadding(0, 0, 0, 0);
                arrow.setAdjustViewBounds(true);
                arrow.setColorFilter(MagicRes.INK_3);
                LinearLayout.LayoutParams alp = new LinearLayout.LayoutParams(dp(8), dp(8));
                alp.leftMargin = dp(3); // 对齐 HTML gap:4px
                trigger.addView(arrow, alp);

                boolean canUse = featureInteractable(num);
                if (!canUse) {
                    trigger.setAlpha(0.45f);
                    trigger.setOnClickListener(v -> showToast(authBlockedToast(num)));
                } else {
                    trigger.setOnClickListener(v -> showMagicDropdown(
                            trigger, arrow, val, opts, curIdx, num, raw));
                }
                row.addView(trigger);
                return;
            }
            case INPUT: {
                LinearLayout host = featureHost();
                LinearLayout row = new LinearLayout(appContext);
                row.setOrientation(LinearLayout.HORIZONTAL);
                row.setGravity(Gravity.CENTER_VERTICAL);
                row.setPadding(dp(SPACE_TIGHT), dp(ROW_PAD_V), dp(SPACE_TIGHT), dp(ROW_PAD_V));
                host.addView(row, new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

                TextView lab = new TextView(appContext);
                lab.setText(item.name);
                lab.setTextColor(MagicRes.INK);
                setFs(lab, FS_BODY);
                lab.setLayoutParams(new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f));
                row.addView(lab);

                boolean isValue = item.extras != null && item.extras.length > 0
                        && "value".equals(item.extras[0]);
                String def = (item.extras != null && item.extras.length > 1) ? item.extras[1] : "";
                String saved = Preferences.loadPrefString(item.featNum);
                if (saved == null || saved.isEmpty()) saved = def;

                EditText et = new EditText(appContext);
                et.setText(saved);
                et.setHint(isValue ? "数字" : "文本");
                et.setHintTextColor(MagicRes.INK_3);
                et.setTextColor(MagicRes.INK);
                et.setBackground(MagicRes.inputBg(appContext));
                setFs(et, FS_VALUE);
                et.setSingleLine(true);
                et.setMinWidth(dp(96));
                et.setPadding(dp(10), dp(6), dp(10), dp(6));
                et.setInputType(isValue
                        ? (InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL)
                        : (InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_FLAG_NO_SUGGESTIONS));
                final int num = item.featNum;
                final String raw = item.raw;
                boolean canUse = featureInteractable(num);
                if (!canUse) {
                    et.setEnabled(false);
                    et.setAlpha(0.45f);
                    et.setOnClickListener(v -> showToast(authBlockedToast(num)));
                } else {
                    et.setOnFocusChangeListener((v, hasFocus) -> {
                        setWindowFocusableForIme(hasFocus);
                        MenuService.notifyOverlayInputFocus(hasFocus);
                        if (!hasFocus) {
                            String text = et.getText() != null ? et.getText().toString() : "";
                            if (!bridge.onInputChanged(num, raw, text)) {
                                showToast(authBlockedToast(num));
                            }
                        }
                    });
                    // 参数输入框同样接自绘剪贴板条（悬浮窗系统长按菜单不可用）
                    enableOverlayNativeEditClipboard(et);
                }
                row.addView(et, new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
                addRowDivider(host);
                return;
            }
            default:
                addHint(featureHost(), item.name + "（暂未支持的控件）");
        }
    }

    private void attachBlockedToggle(Switch sw, int featNum) {
        sw.setOnCheckedChangeListener((buttonView, on) -> {
            if (on) {
                buttonView.setOnCheckedChangeListener(null);
                buttonView.setChecked(false);
                attachBlockedToggle(sw, featNum);
                showToast(authBlockedToast(featNum));
            }
        });
    }

    private void dismissMagicDropdown() {
        if (openDropdown != null) {
            try {
                if (openDropdown.isShowing()) openDropdown.dismiss();
            } catch (Exception ignored) {}
            openDropdown = null;
        }
    }

    /** 对齐 HTML .mod-dropdown：触发器 + 品牌描边菜单，非系统 AlertDialog */
    private void showMagicDropdown(View trigger, ImageView arrow, TextView valueTv,
                                   String[] opts, int[] curIdx, int featNum, String raw) {
        if (opts == null || opts.length == 0) {
            showToast("无选项");
            return;
        }
        if (openDropdown != null && openDropdown.isShowing()) {
            dismissMagicDropdown();
            trigger.setBackground(MagicRes.dropdownTrigger(appContext));
            arrow.animate().rotation(0f).setDuration(160).start();
            arrow.setColorFilter(MagicRes.INK_3);
            return;
        }
        dismissMagicDropdown();

        LinearLayout menu = new LinearLayout(appContext);
        menu.setOrientation(LinearLayout.VERTICAL);
        menu.setBackground(MagicRes.dropdownMenuBg(appContext));
        // 与触发器同宽、左右齐平
        int menuW = trigger.getWidth();
        if (menuW <= 0) menuW = Math.max(trigger.getMeasuredWidth(), dp(72));
        for (int i = 0; i < opts.length; i++) {
            final int idx = i;
            TextView item = new TextView(appContext);
            item.setText(opts[i]);
            setFs(item, FS_VALUE);
            item.setIncludeFontPadding(false);
            // 左内边距与触发器文字对齐（trigger paddingStart=8）
            item.setPadding(dp(8), dp(8), dp(8), dp(8));
            item.setBackground(MagicRes.dropdownItemBg(appContext));
            boolean selected = idx == curIdx[0];
            item.setTextColor(selected ? MagicRes.BRAND : MagicRes.INK_2);
            if (selected) {
                item.setTypeface(Typeface.DEFAULT_BOLD);
            }
            item.setOnClickListener(v -> {
                if (!bridge.onSpinnerChanged(featNum, raw, idx)) {
                    showToast(authBlockedToast(featNum));
                    dismissMagicDropdown();
                    trigger.setBackground(MagicRes.dropdownTrigger(appContext));
                    arrow.animate().rotation(0f).setDuration(160).start();
                    arrow.setColorFilter(MagicRes.INK_3);
                    return;
                }
                curIdx[0] = idx;
                valueTv.setText(opts[idx]);
                dismissMagicDropdown();
                trigger.setBackground(MagicRes.dropdownTrigger(appContext));
                arrow.animate().rotation(0f).setDuration(160).start();
                arrow.setColorFilter(MagicRes.INK_3);
            });
            menu.addView(item, new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
            if (i < opts.length - 1) {
                View line = new View(appContext);
                line.setBackgroundColor(MagicRes.LINE);
                menu.addView(line, new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, dp(1)));
            }
        }

        final int finalW = menuW;
        menu.measure(
                View.MeasureSpec.makeMeasureSpec(finalW, View.MeasureSpec.EXACTLY),
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));
        PopupWindow pw = new PopupWindow(menu, finalW, menu.getMeasuredHeight(), true);
        pw.setBackgroundDrawable(new ColorDrawable(0x00000000));
        pw.setOutsideTouchable(true);
        pw.setFocusable(true);
        pw.setClippingEnabled(false);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            pw.setElevation(dp(10));
        }
        pw.setOnDismissListener(() -> {
            openDropdown = null;
            trigger.setBackground(MagicRes.dropdownTrigger(appContext));
            arrow.animate().rotation(0f).setDuration(160).start();
            arrow.setColorFilter(MagicRes.INK_3);
        });
        openDropdown = pw;
        trigger.setBackground(MagicRes.dropdownTriggerOpen(appContext));
        arrow.setColorFilter(MagicRes.BRAND);
        arrow.animate().rotation(180f).setDuration(160).start();
        try {
            // x=0 与按钮左缘对齐；y=0 贴住底边（开合圆角衔接）
            pw.showAsDropDown(trigger, 0, 0, Gravity.START);
        } catch (Exception e) {
            Log.w(TAG, "dropdown show failed: " + e.getMessage());
            dismissMagicDropdown();
            Context ctx = boundActivity != null && !boundActivity.isFinishing()
                    ? boundActivity : appContext;
            new AlertDialog.Builder(ctx)
                    .setItems(opts, (d, which) -> {
                        if (!bridge.onSpinnerChanged(featNum, raw, which)) {
                            showToast(authBlockedToast(featNum));
                            return;
                        }
                        curIdx[0] = which;
                        valueTv.setText(opts[which]);
                    })
                    .show();
        }
    }

    /** 后台关闭单项：按控件类型复位（勿对滑块写 boolean，否则 loadPrefInt ClassCast 闪退） */
    private void enforceServerFeatureFlags() {
        try {
            JSONObject boot = new JSONObject(Preferences.getAuthBootstrapJson());
            JSONArray list = boot.optJSONArray("features");
            if (list == null || list.length() == 0) return;
            String[] specs = bridge.getFeatureListRaw();
            for (int i = 0; i < list.length(); i++) {
                JSONObject f = list.optJSONObject(i);
                if (f == null || f.optBoolean("enabled", true)) continue;
                int n = f.optInt("feat_num", -1);
                if (n < 0 || n >= 50) continue;
                String name = f.optString("name", "#" + n);
                String spec = (specs != null && n < specs.length) ? specs[n] : null;
                bridge.forceFeatureOff(n, name, spec);
            }
        } catch (Exception e) {
            Log.w(TAG, "enforceServerFeatureFlags: " + e.getMessage());
        }
    }

    /** 手风琴式分类（2026-09-22 重设计）：卡片头（左对齐名称+右侧折叠箭头）+ 缩进竖线内容区 */
    private void addCategorySection(String name) {
        LinearLayout section = new LinearLayout(appContext);
        section.setOrientation(LinearLayout.VERTICAL);
        LinearLayout.LayoutParams secLp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        secLp.topMargin = dp(SPACE);
        secLp.bottomMargin = dp(SPACE_TIGHT);
        featureList.addView(section, secLp);

        // ---- 分类头：整行可点的卡片（底色明示可交互）----
        LinearLayout header = new LinearLayout(appContext);
        header.setOrientation(LinearLayout.HORIZONTAL);
        header.setGravity(Gravity.CENTER_VERTICAL);
        header.setPadding(dp(10), dp(9), dp(10), dp(9));
        header.setClickable(true);
        header.setFocusable(true);
        header.setBackground(MagicRes.catHeaderBg(appContext));
        section.addView(header, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));

        TextView title = new TextView(appContext);
        title.setText(name);
        title.setTextColor(MagicRes.INK);
        title.setTypeface(Typeface.DEFAULT_BOLD);
        setFs(title, FS_SECTION);
        title.setLetterSpacing(0.05f);
        header.addView(title, new LinearLayout.LayoutParams(
                0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f));

        ImageView chevron = new ImageView(appContext);
        chevron.setImageDrawable(MagicRes.catChevron(appContext));
        header.addView(chevron, new LinearLayout.LayoutParams(dp(16), dp(16)));

        // ---- 内容区：左侧从属竖线 + 功能容器（缩进明示归属）----
        LinearLayout indentRow = new LinearLayout(appContext);
        indentRow.setOrientation(LinearLayout.HORIZONTAL);
        LinearLayout.LayoutParams indLp = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        indLp.leftMargin = dp(6);
        indLp.topMargin = dp(4);
        section.addView(indentRow, indLp);

        View guide = new View(appContext);
        guide.setBackground(MagicRes.catIndentLine(appContext));
        indentRow.addView(guide, new LinearLayout.LayoutParams(
                Math.max(2, dp(2)), ViewGroup.LayoutParams.MATCH_PARENT));

        LinearLayout body = new LinearLayout(appContext);
        body.setOrientation(LinearLayout.VERTICAL);
        body.setTag(CATEGORY_BODY_TAG);
        Boolean saved = categoryOpenState.get(name);
        boolean expand = saved != null ? saved : true;
        categoryBuildIndex++;
        body.setVisibility(expand ? View.VISIBLE : View.GONE);
        body.setPadding(dp(8), 0, 0, 0);
        indentRow.addView(body, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        indentRow.setVisibility(expand ? View.VISIBLE : View.GONE);
        currentCategoryBody = body;

        final boolean[] open = {expand};
        header.setSelected(expand);
        // 手风琴惯例：展开 ▼(0°) / 收起 ▸(-90°)
        chevron.setRotation(expand ? 0f : -90f);
        header.setOnClickListener(v -> {
            open[0] = !open[0];
            categoryOpenState.put(name, open[0]);
            body.setVisibility(open[0] ? View.VISIBLE : View.GONE);
            indentRow.setVisibility(open[0] ? View.VISIBLE : View.GONE);
            header.setSelected(open[0]);
            chevron.animate().rotation(open[0] ? 0f : -90f).setDuration(160).start();
            syncTrailingDividers(featureList);
            if (root != null) root.post(this::requestPanelRelayout);
            else requestPanelRelayout();
        });
    }

    private void styleSeekBar(SeekBar sb) {
        Drawable track = MagicRes.seekProgress(appContext).mutate();
        Drawable thumb = MagicRes.seekThumb(appContext).mutate();
        sb.setProgressDrawable(track);
        sb.setThumb(thumb);
        sb.setSplitTrack(false);
        sb.setThumbOffset(0);
        sb.setPadding(dp(6), dp(10), dp(6), dp(10));
        sb.setMaxHeight(dp(4));
        sb.setMinHeight(dp(4));
    }

    private void attachToggleListener(Switch sw, int num, String raw) {
        sw.setOnCheckedChangeListener((buttonView, on) -> {
            boolean ok = bridge.onToggleChanged(num, raw, on);
            if (!ok && on) {
                buttonView.setOnCheckedChangeListener(null);
                buttonView.setChecked(false);
                attachToggleListener(sw, num, raw);
                showToast(authBlockedToast(num));
            }
        });
    }

    private void setExpanded(boolean expand) {
        if (!expand) dismissMagicDropdown();
        expanded = expand;
        if (fab == null || panel == null || layoutParams == null || windowManager == null) return;
        if (expand) {
            fab.setVisibility(View.GONE);
            panel.setVisibility(View.VISIBLE);
            panel.animate().cancel();
            panel.setAlpha(0f);
            panel.setScaleX(0.92f);
            panel.setScaleY(0.92f);
            panel.animate().alpha(1f).scaleX(1f).scaleY(1f).setDuration(220).start();
            layoutParams.width = dp(PANEL_INNER_W_DP + VINE_OVERHANG_DP * 2);
            layoutParams.height = WindowManager.LayoutParams.WRAP_CONTENT;
            layoutParams.flags |= WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE;
            layoutParams.flags |= WindowManager.LayoutParams.FLAG_NOT_TOUCH_MODAL;
            // 展开后按内容写成确切高度，后续折叠才能回缩
            requestPanelRelayout();
            maybeRenewGrant("expand");
        } else {
            panel.animate().cancel();
            panel.setVisibility(View.GONE);
            panel.setAlpha(0f);
            fab.setVisibility(View.VISIBLE);
            fab.setScaleX(0.85f);
            fab.setScaleY(0.85f);
            fab.animate().scaleX(1f).scaleY(1f).setDuration(180).start();
            dismissEditClipboardBar(); // 收面板时收掉剪贴板条，防悬空
            layoutParams.width = dp(FAB_WM_DP);
            layoutParams.height = dp(FAB_WM_DP);
            layoutParams.flags |= WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE;
        }
        if (attachMode == AttachMode.OVERLAY) {
            layoutParams.alpha = OVERLAY_SAFE_ALPHA;
        }
        try {
            windowManager.updateViewLayout(root, layoutParams);
            if (expand) clampWindowOnScreen();
        } catch (Exception e) {
            Log.w(TAG, "updateLayout: " + e.getMessage());
            // BadToken / 宿主已销毁：强制换到当前 resumed Activity
            String msg = e.getMessage() != null ? e.getMessage() : "";
            if (msg.contains("token") || msg.contains("not attached")
                    || msg.contains("BadToken") || msg.contains("dead")) {
                Activity host = findResumedHint(boundActivity);
                ensureAliveOrRebind(host);
            }
        }
    }

    public boolean isExpanded() {
        return expanded;
    }

    /** 卡密输入时短暂允许焦点（由 MenuService 配合忽略 focus-lost 藏窗） */
    public void setWindowFocusableForIme(boolean focusable) {
        if (layoutParams == null || windowManager == null || !attached) return;
        if (focusable) {
            layoutParams.flags &= ~WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE;
            // 便于系统弹出输入法 + 文本选择浮动工具条
            layoutParams.softInputMode =
                    WindowManager.LayoutParams.SOFT_INPUT_STATE_ALWAYS_VISIBLE
                            | WindowManager.LayoutParams.SOFT_INPUT_ADJUST_RESIZE;
        } else {
            layoutParams.flags |= WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE;
            layoutParams.softInputMode =
                    WindowManager.LayoutParams.SOFT_INPUT_STATE_UNCHANGED;
        }
        try {
            windowManager.updateViewLayout(root, layoutParams);
        } catch (Exception e) {
            Log.w(TAG, "setWindowFocusableForIme: " + e.getMessage());
        }
    }

    /**
     * 悬浮窗上系统浮动工具条 / ActionMode 经常直接失败（startActionMode→null）。
     * 用贴在输入框上方的横向 PopupWindow 仿系统条：全选/剪切/复制/粘贴，
     * 操作仍走 {@link EditText#onTextContextMenuItem(int)}。
     * （自 starry-mod-menu_111 迁移，该方案在悬浮窗实测可用）
     */
    private PopupWindow editClipboardPopup;

    private void enableOverlayNativeEditClipboard(EditText et) {
        if (et == null) return;
        et.setLongClickable(true);
        et.setFocusable(true);
        et.setFocusableInTouchMode(true);
        // 不用系统 long-click：ACTION_DOWN 里 updateViewLayout 会打断长按计时
        final Handler h = mainHandler;
        final Runnable[] pending = new Runnable[1];
        final float[] down = new float[2];
        et.setOnTouchListener((v, e) -> {
            switch (e.getActionMasked()) {
                case MotionEvent.ACTION_DOWN:
                    down[0] = e.getRawX();
                    down[1] = e.getRawY();
                    if (pending[0] != null) h.removeCallbacks(pending[0]);
                    pending[0] = () -> {
                        pending[0] = null;
                        if (!et.isAttachedToWindow()) return;
                        et.requestFocus();
                        CharSequence text = et.getText();
                        if (text != null && text.length() > 0) {
                            int s = et.getSelectionStart();
                            int end = et.getSelectionEnd();
                            if (s < 0 || end < 0 || s == end) et.selectAll();
                        }
                        showEditClipboardBar(et);
                    };
                    h.postDelayed(pending[0], ViewConfiguration.getLongPressTimeout());
                    break;
                case MotionEvent.ACTION_MOVE:
                    if (pending[0] != null
                            && Math.hypot(e.getRawX() - down[0], e.getRawY() - down[1])
                            > TAP_SLOP_PX) {
                        h.removeCallbacks(pending[0]);
                        pending[0] = null;
                    }
                    break;
                case MotionEvent.ACTION_UP:
                case MotionEvent.ACTION_CANCEL:
                    if (pending[0] != null) {
                        h.removeCallbacks(pending[0]);
                        pending[0] = null;
                    }
                    break;
                default:
                    break;
            }
            return false;
        });
        et.setOnLongClickListener(v -> true); // 吃掉系统长按，避免再走失败的 ActionMode
    }

    private void dismissEditClipboardBar() {
        if (editClipboardPopup != null) {
            try {
                if (editClipboardPopup.isShowing()) editClipboardPopup.dismiss();
            } catch (Exception ignored) {
            }
            editClipboardPopup = null;
        }
    }

    private void showEditClipboardBar(EditText et) {
        if (et == null || !et.isAttachedToWindow()) return;
        dismissEditClipboardBar();

        Context ctx = et.getContext();
        LinearLayout bar = new LinearLayout(ctx);
        bar.setOrientation(LinearLayout.HORIZONTAL);
        bar.setGravity(Gravity.CENTER_VERTICAL);
        int padH = dp(10);
        int padV = dp(8);
        bar.setPadding(padH, padV, padH, padV);
        GradientDrawable bg = new GradientDrawable();
        bg.setColor(0xF2222222);
        bg.setCornerRadius(dp(8));
        bar.setBackground(bg);
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            bar.setElevation(dp(6));
        }

        boolean hasText = et.getText() != null && et.getText().length() > 0;
        boolean hasSel = et.hasSelection();
        addClipboardBarItem(bar, et, android.R.string.selectAll, android.R.id.selectAll, hasText, false);
        addClipboardBarItem(bar, et, android.R.string.cut, android.R.id.cut, hasSel, true);
        addClipboardBarItem(bar, et, android.R.string.copy, android.R.id.copy, hasSel, true);
        addClipboardBarItem(bar, et, android.R.string.paste, android.R.id.paste, true, true);

        bar.measure(
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED),
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));
        PopupWindow pw = new PopupWindow(
                bar, bar.getMeasuredWidth(), bar.getMeasuredHeight(), true);
        pw.setBackgroundDrawable(new ColorDrawable(0x00000000));
        pw.setOutsideTouchable(true);
        pw.setTouchable(true);
        pw.setClippingEnabled(false);
        pw.setOnDismissListener(() -> {
            if (editClipboardPopup == pw) editClipboardPopup = null;
        });
        editClipboardPopup = pw;

        try {
            // 贴在输入框上方居中（仿系统浮动条位置）
            int xOff = (et.getWidth() - bar.getMeasuredWidth()) / 2;
            int yOff = -(et.getHeight() + bar.getMeasuredHeight() + dp(6));
            pw.showAsDropDown(et, xOff, yOff, Gravity.TOP | Gravity.START);
            Log.i(TAG, "edit clipboard bar shown");
        } catch (Exception e) {
            Log.w(TAG, "showEditClipboardBar: " + e.getMessage());
            editClipboardPopup = null;
        }
    }

    private void addClipboardBarItem(LinearLayout bar, EditText et, int labelRes, int actionId,
                                     boolean enabled, boolean withDivider) {
        if (withDivider) {
            View div = new View(bar.getContext());
            LinearLayout.LayoutParams dlp = new LinearLayout.LayoutParams(dp(1), dp(16));
            dlp.leftMargin = dp(2);
            dlp.rightMargin = dp(2);
            div.setLayoutParams(dlp);
            div.setBackgroundColor(0x55FFFFFF);
            bar.addView(div);
        }
        TextView tv = new TextView(bar.getContext());
        try {
            tv.setText(labelRes);
        } catch (Exception e) {
            tv.setText(actionId == android.R.id.paste ? "粘贴"
                    : actionId == android.R.id.copy ? "复制"
                    : actionId == android.R.id.cut ? "剪切" : "全选");
        }
        tv.setTextColor(enabled ? 0xFFFFFFFF : 0x66FFFFFF);
        tv.setTypeface(Typeface.DEFAULT_BOLD);
        setFs(tv, 13f);
        tv.setPadding(dp(8), dp(2), dp(8), dp(2));
        tv.setEnabled(enabled);
        tv.setClickable(enabled);
        if (enabled) {
            tv.setOnClickListener(v -> {
                try {
                    if (actionId == android.R.id.selectAll) {
                        et.selectAll();
                        dismissEditClipboardBar();
                        // 全选后再亮一次，方便接着复制
                        et.post(() -> showEditClipboardBar(et));
                        return;
                    }
                    if (actionId == android.R.id.copy && !et.hasSelection()
                            && et.getText() != null && et.getText().length() > 0) {
                        et.selectAll();
                    }
                    et.onTextContextMenuItem(actionId);
                } catch (Exception e) {
                    Log.w(TAG, "clipboard item: " + e.getMessage());
                }
                dismissEditClipboardBar();
            });
        }
        bar.addView(tv, new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
    }

    private void applyCollapsedGeometry() {
        if (layoutParams == null) return;
        layoutParams.width = dp(FAB_WM_DP);
        layoutParams.height = dp(FAB_WM_DP);
        try {
            windowManager.updateViewLayout(root, layoutParams);
        } catch (Exception ignored) {}
    }

    private void updateAttachModeLabel() {
        // 设置页已与旧 HTML 对齐（声明/定制/关闭），不再展示挂载调试信息
    }

    private boolean onWindowDrag(View v, MotionEvent e) {
        if (layoutParams == null || windowManager == null) return false;
        switch (e.getActionMasked()) {
            case MotionEvent.ACTION_DOWN:
                dragging = false;
                downRawX = e.getRawX();
                downRawY = e.getRawY();
                downX = layoutParams.x;
                downY = layoutParams.y;
                return true;
            case MotionEvent.ACTION_MOVE: {
                float dx = e.getRawX() - downRawX;
                float dy = e.getRawY() - downRawY;
                if (Math.hypot(dx, dy) > TAP_SLOP_PX) dragging = true;
                if (dragging) {
                    layoutParams.x = downX + (int) dx;
                    layoutParams.y = downY + (int) dy;
                    try {
                        windowManager.updateViewLayout(root, layoutParams);
                    } catch (Exception ignored) {}
                }
                return true;
            }
            case MotionEvent.ACTION_UP:
            case MotionEvent.ACTION_CANCEL:
                if (!dragging && !expanded) setExpanded(true);
                return true;
            default:
                return false;
        }
    }

    private void refreshAuthStatus() {
        if (statusLine == null) return;
        ensureAceGateTicker();
        int aceLeft = ModBridge.getAceGateRemainSec();
        if (aceLeft > 0) {
            statusLine.setText("过检测中 " + aceLeft + "s");
            statusLine.setTextColor(0xFFE0C07E);
        } else {
            // 以 native session 为准（登录后不过期）
            boolean ok = bridge.hasLiveNativeSession();
            // 副标题只保留两态，其它提示走 Toast
            statusLine.setText(ok ? "授权有效" : "未授权");
            statusLine.setTextColor(ok ? MagicRes.OK : MagicRes.ERROR);
        }

        boolean ok = bridge.hasLiveNativeSession();
        boolean cardAuth = Preferences.isCardAuthRequired();
        boolean sessionOk = ok;
        if (accountLoginBox != null && accountMainBox != null) {
            boolean showLoggedIn = cardAuth && sessionOk;
            accountLoginBox.setVisibility(showLoggedIn ? View.GONE : View.VISIBLE);
            accountMainBox.setVisibility(showLoggedIn ? View.VISIBLE : View.GONE);
        }
        if (cardAuth && sessionOk) {
            applyCardMetaToAccountUi();
        }
        syncTrailingDividers(accountMainBox);
        if (root != null) root.post(this::requestPanelRelayout);
        else requestPanelRelayout();
    }

    /** 关卡密：写入联网 bootstrap + grant，同步 native，刷新 UI */
    private void enterOpenNetworkMode(String grant, int cacheSec, String bootstrapJson) {
        Preferences.saveNetworkBootstrap(cacheSec, bootstrapJson, grant);
        if (Preferences.isMenuEnabled()) {
            bridge.syncNativeAuthorizedPublic();
        } else {
            bridge.lockAllFeatures();
        }
    }

    /** 开卡密：清会话与 grant，要求重新登录 */
    private void enterCardAuthMode() {
        Preferences.setCardAuthRequired(true);
        bridge.clearAuthSession();
        Preferences.saveAuthCardMeta("{}");
    }

    /** 按本次 bootstrap 相对上次 prefs 的变化，对齐 HTML enterOpen / enterCard / menu 反馈 */
    private List<String> applyBootstrapAuthGates(JSONObject data) {
        List<String> toasts = new ArrayList<>();
        boolean prevCard = Preferences.isCardAuthRequired();
        boolean prevMenu = Preferences.isMenuEnabled();
        boolean cardAuth = data.has("card_auth_enabled")
                ? data.optBoolean("card_auth_enabled", true) : prevCard;
        boolean menuEnabled = data.has("menu_enabled")
                ? data.optBoolean("menu_enabled", true) : prevMenu;
        int cacheSec = data.optInt("offline_cache_seconds", 600);
        String grant = Preferences.pickGrant(data);
        String bootJson = data.toString();

        Preferences.saveBootstrapJson(bootJson);
        Preferences.setMenuEnabled(menuEnabled);

        if (prevCard && !cardAuth) {
            enterOpenNetworkMode(grant, cacheSec, bootJson);
            if (!menuEnabled) {
                bridge.lockAllFeatures();
                toasts.add("游戏维护中，功能已关闭");
            } else {
                toasts.add("已关闭卡密验证，功能已开放");
            }
        } else if (!prevCard && cardAuth) {
            enterCardAuthMode();
            toasts.add("已开启卡密验证，请登录后使用");
            if (!menuEnabled) {
                bridge.lockAllFeatures();
                toasts.add("游戏维护中，功能已关闭");
            }
        } else if (!cardAuth) {
            enterOpenNetworkMode(grant, cacheSec, bootJson);
            if (!menuEnabled) {
                bridge.lockAllFeatures();
                toasts.add("游戏维护中，功能已关闭");
            } else if (!prevMenu) {
                toasts.add("游戏已恢复，可使用功能");
            }
        } else {
            Preferences.setCardAuthRequired(true);
            if (!menuEnabled) {
                bridge.lockAllFeatures();
                toasts.add("游戏维护中，功能已关闭");
            } else {
                bridge.syncNativeAuthorizedPublic();
                if (!prevMenu) {
                    toasts.add("游戏已恢复，可使用功能");
                }
            }
        }
        return toasts;
    }

    /** 与旧 HTML / 后台列表一致：卡密 expire_at，不是会话 expires_at */
    private String formatCardExpireLabel() {
        try {
            JSONObject card = new JSONObject(Preferences.getAuthCardMetaJson());
            return formatCardExpire(card);
        } catch (Exception e) {
            return formatExpireDisplay(Preferences.getAuthExpireAt());
        }
    }

    private String formatCardExpire(JSONObject card) {
        if (card == null) return "—";
        if ("permanent".equals(card.optString("duration_type"))) return "永久";
        String raw = card.optString("expire_at", "");
        if (raw == null || raw.isEmpty() || "null".equals(raw)) {
            return "count".equals(card.optString("duration_type")) ? "按次数" : "—";
        }
        return formatExpireDisplay(raw);
    }

    /** ISO → 与后台列表相同的 yyyy-MM-dd HH:mm:ss（按东八显示，对齐管理端本地感） */
    private String formatExpireDisplay(String raw) {
        if (raw == null || raw.isEmpty() || "null".equals(raw)) return "—";
        try {
            String s = raw.trim();
            java.time.OffsetDateTime odt;
            try {
                odt = java.time.OffsetDateTime.parse(s);
            } catch (Exception ignore) {
                String norm = s.replace(' ', 'T');
                if (norm.length() >= 19) norm = norm.substring(0, 19);
                java.time.LocalDateTime ldt = java.time.LocalDateTime.parse(norm);
                odt = ldt.atOffset(java.time.ZoneOffset.UTC);
            }
            return odt.atZoneSameInstant(java.time.ZoneId.of("Asia/Shanghai"))
                    .format(java.time.format.DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss"));
        } catch (Exception e) {
            String s = raw.replace('T', ' ');
            int cut = s.indexOf('+');
            if (cut < 0) cut = s.indexOf('Z');
            if (cut > 0) s = s.substring(0, cut).trim();
            if (s.length() > 19) s = s.substring(0, 19);
            return s;
        }
    }

    private void applyCardMetaToAccountUi() {
        String usesText = "不限";
        boolean canUnbind = false;
        String expireText = "—";
        try {
            JSONObject card = new JSONObject(Preferences.getAuthCardMetaJson());
            expireText = formatCardExpire(card);
            int maxUses = card.optInt("max_uses", 0);
            int used = card.optInt("used_count", 0);
            if (maxUses > 0) {
                usesText = Math.max(maxUses - used, 0) + " / " + maxUses;
            } else if ("count".equals(card.optString("duration_type"))) {
                usesText = "按次数";
            }
            canUnbind = card.optBoolean("device_bound", false);  // 解绑不限次数：已绑定即可解绑
        } catch (Exception ignored) {}
        if (accountExpireTv != null) accountExpireTv.setText(expireText);
        if (accountUsesTv != null) accountUsesTv.setText(usesText);
        if (accountUnbindBtn != null) {
            accountUnbindBtn.setEnabled(true);
            accountUnbindBtn.setAlpha(canUnbind ? 1f : 0.45f);
            accountUnbindBtn.setTag(canUnbind ? "1" : "0");
        }
    }

    private void doUnbind() {
        if (accountUnbindBtn != null && !"1".equals(String.valueOf(accountUnbindBtn.getTag()))) {
            showToast("当前卡密未绑定设备，无需解绑");
            return;
        }
        if (!unbindArmed) {
            unbindArmed = true;
            mainHandler.removeCallbacks(unbindArmClear);
            mainHandler.postDelayed(unbindArmClear, 5000);
            showToast("再点一次「解绑」确认（首次免费，之后每次扣 3 天）");
            return;
        }
        unbindArmed = false;
        mainHandler.removeCallbacks(unbindArmClear);
        String token = Preferences.getAuthToken();
        String card = Preferences.getAuthCardKey();
        if (token == null || token.isEmpty()) {
            showToast("未登录");
            return;
        }
        showToast("解绑中…", "info");
        runAuth(() -> {
            try {
                JSONObject body = new JSONObject();
                body.put("device_id", Preferences.getOrCreateDeviceId());
                body.put("game_id", Preferences.getAuthGameId());
                if (card != null && !card.isEmpty()) body.put("card_key", card);
                String res = AuthClient.request(
                        Preferences.getAuthBaseUrl(), "POST", "/v1/auth/unbind",
                        body.toString(), token);
                JSONObject json = new JSONObject(res);
                int code = json.optInt("code", 0);
                if (json.has("error") || code >= 400) {
                    String err = friendlyAuthError(json, "解绑失败");
                    mainHandler.post(() -> showToast(err, "error"));
                    return;
                }
                bridge.clearAuthSession();
                Preferences.saveAuthCardMeta("{}");
                mainHandler.post(() -> {
                    stopGrantBgRenew();
                    updateAccountTabVisibility();
                    refreshAuthStatus();
                    refreshFeatures();
                    showToast(json.optString("message", "已解绑并退出登录"));
                });
            } catch (Exception e) {
                mainHandler.post(() ->
                        showToast(friendlyExceptionMessage(e), "error"));
            }
        });
    }

    /** 启动拉一次 bootstrap：门控 UI + 开屏 toast + 公告队列 */
    private void fetchBootstrapOnce() {
        if (bootstrapFetched) return;
        bootstrapFetched = true;
        runAuth(() -> {
            try {
                ModBridge.syncAuthGameIdFromNative();
                String deviceId = Preferences.getOrCreateDeviceId();
                String gameId = Preferences.getAuthGameId();
                boolean loggedIn = bridge.hasLiveNativeSession();
                String token = loggedIn ? Preferences.getAuthToken() : null;
                String path = "/v1/bootstrap?game_id=" + Uri.encode(gameId)
                        + "&device_id=" + Uri.encode(deviceId)
                        + "&logged_in=" + (loggedIn ? "true" : "false");
                Log.i(TAG, "bootstrap fetch game_id=" + gameId);
                String res = AuthClient.request(
                        Preferences.getAuthBaseUrl(), "GET", path, null, token);
                JSONObject json = new JSONObject(res);
                JSONObject data = json.has("data") ? json.optJSONObject("data") : json;
                if (data == null) data = json;

                final List<String> gateToasts = applyBootstrapAuthGates(data);

                JSONArray anns = data.optJSONArray("announcements");
                JSONArray toasts = data.optJSONArray("startup_toasts");
                final List<JSONObject> annList = new ArrayList<>();
                if (anns != null) {
                    for (int i = 0; i < anns.length(); i++) {
                        JSONObject a = anns.optJSONObject(i);
                        if (a != null) annList.add(a);
                    }
                }
                final List<String> toastTexts = new ArrayList<>();
                if (toasts != null) {
                    for (int i = 0; i < toasts.length(); i++) {
                        JSONObject t = toasts.optJSONObject(i);
                        if (t == null) continue;
                        String c = t.optString("content", "").trim();
                        if (!c.isEmpty()) toastTexts.add(c);
                    }
                }
                mainHandler.post(() -> {
                    enforceServerFeatureFlags();
                    updateAccountTabVisibility();
                    refreshAuthStatus();
                    refreshFeatures();
                    startGrantBgRenew();
                    if (annList.isEmpty()) {
                        // 无公告：维持原节奏，toast 立即排程
                        playGateToasts(gateToasts);
                        playStartupToasts(toastTexts);
                    } else {
                        // 有公告：toast 全部挂起，公告播完后按原节奏补放（releaseAnnounceHold）
                        stagedGateToasts.clear();
                        stagedGateToasts.addAll(gateToasts);
                        stagedStartupToasts.clear();
                        stagedStartupToasts.addAll(toastTexts);
                    }
                    enqueueAnnouncements(annList);
                });
            } catch (Exception e) {
                Log.e(TAG, "bootstrap fetch failed", e);
                mainHandler.post(() -> showToast("bootstrap 失败: " + (e.getMessage() != null ? e.getMessage() : "网络错误")));
            }
        });
    }

    private void playStartupToasts(List<String> texts) {
        if (texts == null || texts.isEmpty()) return;
        for (int i = 0; i < texts.size(); i++) {
            final String msg = texts.get(i);
            mainHandler.postDelayed(() -> showToast(msg), 600L + i * 2300L);
        }
    }

    /** 门控 toast 原节奏：200ms 起，每条隔 1600ms */
    private void playGateToasts(List<String> msgs) {
        if (msgs == null || msgs.isEmpty()) return;
        for (int i = 0; i < msgs.size(); i++) {
            final String msg = msgs.get(i);
            mainHandler.postDelayed(() -> showToast(msg), 200L + i * 1600L);
        }
    }

    private void enqueueAnnouncements(List<JSONObject> list) {
        announceQueue.clear();
        announceAttachRetry = 0;
        if (list != null) {
            for (JSONObject a : list) {
                if (a == null) continue;
                String title = a.optString("title", "").trim();
                String content = a.optString("content", "").trim();
                if (title.isEmpty() && content.isEmpty()) continue;
                // 每设备一次：本地已读则跳过（对齐 HTML playAnnouncementQueue）
                if ("once_device".equals(a.optString("popup_mode", "every_open"))
                        && Preferences.isAnnounceSeen(a.opt("id"))) {
                    continue;
                }
                announceQueue.add(a);
            }
        }
        if (announceQueue.isEmpty()) {
            // 无可播公告（含全部已读）：挂起的 toast 立即按原节奏补放
            releaseAnnounceHold();
        } else {
            // 公告优先：悬浮球视觉收起（状态照常维护），公告播完后恢复
            menuHoldForAnnounce = true;
            applyAnnounceHold();
        }
        drainAnnouncementQueue();
    }

    /** 公告播放期间保持悬浮球视觉隐藏（挂载状态照常，公告播完后 setVisible(true) 恢复） */
    private void applyAnnounceHold() {
        if (menuHoldForAnnounce && attached && root != null && root.getParent() != null) {
            setVisible(false);
        }
    }

    /** 公告全部播完：恢复悬浮球 + 按原节奏补放挂起的 toast（幂等，可随时调用） */
    private void releaseAnnounceHold() {
        if (announceShowing || !announceQueue.isEmpty()) return; // 还有公告在播/待播
        if (!menuHoldForAnnounce
                && stagedGateToasts.isEmpty() && stagedStartupToasts.isEmpty()) {
            return; // 本轮启动无挂起项
        }
        menuHoldForAnnounce = false;
        List<String> gate = new ArrayList<>(stagedGateToasts);
        List<String> startup = new ArrayList<>(stagedStartupToasts);
        stagedGateToasts.clear();
        stagedStartupToasts.clear();
        if (attached && root != null && root.getParent() == null) {
            setVisible(true); // 收起过的悬浮球恢复显示
        }
        playGateToasts(gate);
        playStartupToasts(startup);
    }

    private void drainAnnouncementQueue() {
        if (announceShowing || announceQueue.isEmpty()) return;
        JSONObject a = announceQueue.remove(0);
        currentAnnounce = a;
        announceShowing = true;
        String title = jsonText(a, "title", "公告");
        String content = jsonText(a, "content", "");
        String actionUrl = jsonText(a, "action_url", "");
        String actionLabel = jsonText(a, "action_label", "立即前往");
        mainHandler.post(() -> showMagicAnnounce(title, content, actionUrl, actionLabel));
    }

    /** JSON null / 字面量 "null" / 空白 → 默认值（避免按钮文案显示 null） */
    private static String jsonText(JSONObject o, String key, String fallback) {
        if (o == null || !o.has(key) || o.isNull(key)) return fallback;
        Object raw = o.opt(key);
        if (raw == null || raw == JSONObject.NULL) return fallback;
        String s = String.valueOf(raw).trim();
        if (s.isEmpty() || "null".equalsIgnoreCase(s)) return fallback;
        return s;
    }

    /** 关闭当前公告时：once_device 写入本机已读 */
    private void finishCurrentAnnounce() {
        if (currentAnnounce != null
                && "once_device".equals(currentAnnounce.optString("popup_mode", ""))
                && currentAnnounce.has("id")) {
            Preferences.markAnnounceSeen(currentAnnounce.opt("id"));
        }
        currentAnnounce = null;
        removeMagicAnnounce();
        announceShowing = false;
        drainAnnouncementQueue();
        releaseAnnounceHold();
    }

    /** 安卓自绘公告（对齐 HTML .mod-announce），非系统 AlertDialog */
    private void showMagicAnnounce(String title, String content, String actionUrl, String actionLabel) {
        if (windowManager == null || !attached) {
            // 窗口暂时分离（Activity 切换空窗期）：保持公告状态等待重挂重试，超限才降级 toast
            if (announceAttachRetry < ANNOUNCE_ATTACH_RETRY_MAX) {
                announceAttachRetry++;
                mainHandler.postDelayed(this::reshowCurrentAnnounce, ANNOUNCE_ATTACH_RETRY_MS);
                return;
            }
            announceAttachRetry = 0;
            showToast((title != null ? title + "：" : "") + (content != null ? content : ""), "info");
            // 已用 toast 投递内容，once_device 仍记已读，避免每次冷启刷屏
            if (currentAnnounce != null
                    && "once_device".equals(currentAnnounce.optString("popup_mode", ""))
                    && currentAnnounce.has("id")) {
                Preferences.markAnnounceSeen(currentAnnounce.opt("id"));
            }
            currentAnnounce = null;
            announceShowing = false;
            mainHandler.postDelayed(this::drainAnnouncementQueue, 800);
            releaseAnnounceHold(); // 队列空时立即释放（drain 800ms 后已无任务）
            return;
        }
        announceAttachRetry = 0;
        removeMagicAnnounce();
        try {
            final int overhang = dp(VINE_OVERHANG_DP);
            FrameLayout shell = new FrameLayout(appContext);
            shell.setClipChildren(false);
            shell.setClipToPadding(false);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
                shell.setElevation(dp(12));
            }

            LinearLayout card = new LinearLayout(appContext);
            card.setOrientation(LinearLayout.VERTICAL);
            card.setGravity(Gravity.CENTER_HORIZONTAL);
            card.setBackground(MagicRes.announceBg(appContext));
            card.setPadding(dp(20), dp(16), dp(20), dp(14));
            card.setClickable(true);
            card.setElevation(0f);

            // 先算内容宽，标题截断 / 正文行数对齐都依赖准确宽度
            final int cardW = Math.min(dp(320),
                    appContext.getResources().getDisplayMetrics().widthPixels - dp(32) - overhang * 2);
            final int titleW = cardW - card.getPaddingLeft() - card.getPaddingRight();
            final int bodyPadH = dp(14);
            final int bodyW = Math.max(1, titleW - bodyPadH * 2);

            // eyebrow：与主菜单状态行同级，拉开「公告」与标题层级
            TextView eyebrow = new TextView(appContext);
            eyebrow.setText("公告");
            eyebrow.setTextColor(MagicRes.INK_3);
            eyebrow.setGravity(Gravity.CENTER);
            setFs(eyebrow, FS_STATUS);
            LinearLayout.LayoutParams ebp = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
            ebp.topMargin = dp(2);
            card.addView(eyebrow, ebp);

            TextView titleTv = new TextView(appContext);
            // 标题压成单行再截断（后台换行也当成空格）
            String titleOneLine = (title != null && !title.isEmpty() ? title : "公告")
                    .replace('\r', ' ').replace('\n', ' ').trim();
            titleTv.setText(titleOneLine);
            titleTv.setTextColor(MagicRes.INK);
            titleTv.setTypeface(Typeface.DEFAULT_BOLD);
            titleTv.setGravity(Gravity.CENTER);
            titleTv.setSingleLine(true);
            titleTv.setMaxLines(1);
            titleTv.setEllipsize(TextUtils.TruncateAt.END);
            titleTv.setHorizontallyScrolling(false);
            setFs(titleTv, 18f);
            LinearLayout.LayoutParams tlp = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
            tlp.topMargin = dp(SPACE_TIGHT);
            card.addView(titleTv, tlp);

            card.addView(makeHairline(), hairlineLp(SPACE_TIGHT, 0));

            final String bodyText = content != null ? content.trim() : "";
            if (!bodyText.isEmpty()) {
                ScrollView scroll = new ScrollView(appContext);
                // 外层卡片 + 标题 hairline 已分区，不再套正文灰底（盒中盒）
                // 顶距压紧：靠 hairline 底=0 + 小 padding，避免与分割线空太大
                scroll.setPadding(bodyPadH, dp(SPACE), bodyPadH, dp(SPACE));
                scroll.setOverScrollMode(View.OVER_SCROLL_IF_CONTENT_SCROLLS);
                TextView bodyTv = new TextView(appContext);
                bodyTv.setText(bodyText);
                bodyTv.setTextColor(MagicRes.INK_2);
                setFs(bodyTv, FS_BODY);
                bodyTv.setLineSpacing(dp(3), 1f);
                // 用 StaticLayout 按真实内容宽判行数，避免 ScrollView 里 post 误判
                bodyTv.setTextAlignment(View.TEXT_ALIGNMENT_GRAVITY);
                if (measureTextLineCount(bodyTv, bodyText, bodyW) > 1) {
                    bodyTv.setGravity(Gravity.START | Gravity.TOP);
                } else {
                    bodyTv.setGravity(Gravity.CENTER_HORIZONTAL);
                }
                scroll.addView(bodyTv, new ScrollView.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
                LinearLayout.LayoutParams slp = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
                slp.topMargin = 0;
                slp.height = dp(140); // 保留固定高度，给藤蔓角饰留呼吸
                card.addView(scroll, slp);
            }

            LinearLayout actions = new LinearLayout(appContext);
            actions.setOrientation(LinearLayout.HORIZONTAL);
            actions.setGravity(Gravity.CENTER);
            LinearLayout.LayoutParams alp = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
            alp.topMargin = dp(12);
            card.addView(actions, alp);

            final String url = actionUrl != null ? actionUrl.trim() : "";
            // 无有效链接时不显示「前往」（含后台 JSON null → 字面量 "null"）
            final boolean hasGo = !url.isEmpty() && !"null".equalsIgnoreCase(url);

            Button closeBtn = new Button(appContext);
            closeBtn.setText(hasGo ? "关闭" : "我知道了");
            closeBtn.setAllCaps(false);
            closeBtn.setTextColor(MagicRes.INK_2);
            closeBtn.setBackground(MagicRes.announceBtnClose(appContext));
            setFs(closeBtn, 13f);
            closeBtn.setMinHeight(0);
            closeBtn.setMinimumHeight(0);
            closeBtn.setPadding(0, dp(10), 0, dp(10));
            closeBtn.setOnClickListener(v -> finishCurrentAnnounce());
            LinearLayout.LayoutParams clp = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f);
            actions.addView(closeBtn, clp);

            if (hasGo) {
                Button goBtn = new Button(appContext);
                String label = actionLabel != null ? actionLabel.trim() : "";
                if (label.isEmpty() || "null".equalsIgnoreCase(label)) label = "立即前往";
                goBtn.setText(label);
                goBtn.setAllCaps(false);
                goBtn.setTextColor(0xFFFFFFFF);
                goBtn.setBackground(MagicRes.announceBtnPrimary(appContext));
                setFs(goBtn, 13f);
                goBtn.setMinHeight(0);
                goBtn.setMinimumHeight(0);
                goBtn.setPadding(0, dp(10), 0, dp(10));
                goBtn.setOnClickListener(v -> {
                    try {
                        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                        appContext.startActivity(intent);
                    } catch (Exception e) {
                        showToast("无法打开链接", "error");
                    }
                    finishCurrentAnnounce();
                });
                LinearLayout.LayoutParams glp = new LinearLayout.LayoutParams(0, ViewGroup.LayoutParams.WRAP_CONTENT, 1f);
                glp.leftMargin = dp(10);
                actions.addView(goBtn, glp);
            }

            FrameLayout.LayoutParams cardLp = new FrameLayout.LayoutParams(
                    cardW, ViewGroup.LayoutParams.WRAP_CONTENT);
            cardLp.setMargins(overhang, overhang, overhang, overhang);
            shell.addView(card, cardLp);
            // 与主菜单同尺寸/同外探对齐
            addVineCorner(shell, Gravity.TOP | Gravity.START);
            addVineCorner(shell, Gravity.TOP | Gravity.END);
            addVineCorner(shell, Gravity.BOTTOM | Gravity.START);
            addVineCorner(shell, Gravity.BOTTOM | Gravity.END);

            announceLp = new WindowManager.LayoutParams(
                    cardW + overhang * 2,
                    ViewGroup.LayoutParams.WRAP_CONTENT,
                    0, 0,
                    layoutParams != null ? layoutParams.type : WindowManager.LayoutParams.TYPE_APPLICATION,
                    WindowManager.LayoutParams.FLAG_NOT_FOCUSABLE
                            | WindowManager.LayoutParams.FLAG_LAYOUT_IN_SCREEN
                            | WindowManager.LayoutParams.FLAG_HARDWARE_ACCELERATED,
                    PixelFormat.TRANSLUCENT
            );
            announceLp.gravity = Gravity.CENTER;
            if (layoutParams != null) {
                announceLp.token = layoutParams.token;
            }
            announceOverlay = shell;
            // 先设不可见再挂窗，避免角饰先露、卡片后入
            shell.setAlpha(0f);
            shell.setScaleX(0.94f);
            shell.setScaleY(0.94f);
            shell.setTranslationY(dp(10));
            windowManager.addView(announceOverlay, announceLp);
            shell.animate().alpha(1f).scaleX(1f).scaleY(1f).translationY(0f).setDuration(320).start();
        } catch (Exception e) {
            Log.w(TAG, "announce show failed: " + e.getMessage());
            showToast((title != null ? title + "：" : "") + (content != null ? content : ""), "info");
            finishCurrentAnnounce();
        }
    }

    /** 公告待重显入口：detach/空窗期后按 currentAnnounce 重建，已在展示则跳过 */
    private void reshowCurrentAnnounce() {
        JSONObject a = currentAnnounce;
        if (a == null || !announceShowing) return;
        if (announceOverlay != null && announceOverlay.getParent() != null) return;
        showMagicAnnounce(
                jsonText(a, "title", "公告"),
                jsonText(a, "content", ""),
                jsonText(a, "action_url", ""),
                jsonText(a, "action_label", "立即前往"));
    }

    private void removeMagicAnnounce() {
        if (announceOverlay != null && windowManager != null && announceOverlay.getParent() != null) {
            try {
                windowManager.removeView(announceOverlay);
            } catch (Exception ignored) {}
        }
        announceOverlay = null;
        announceLp = null;
    }

    /** 按给定宽度测量正文会排成几行（ScrollView 内 getLineCount 不可靠） */
    private static int measureTextLineCount(TextView sample, CharSequence text, int widthPx) {
        if (sample == null || text == null || text.length() == 0 || widthPx <= 0) return 1;
        TextPaint paint = sample.getPaint();
        final StaticLayout sl;
        if (Build.VERSION.SDK_INT >= 23) {
            sl = StaticLayout.Builder.obtain(text, 0, text.length(), paint, widthPx)
                    .setAlignment(Layout.Alignment.ALIGN_NORMAL)
                    .setLineSpacing(sample.getLineSpacingExtra(), sample.getLineSpacingMultiplier())
                    .setIncludePad(sample.getIncludeFontPadding())
                    .build();
        } else {
            sl = new StaticLayout(text, paint, widthPx, Layout.Alignment.ALIGN_NORMAL,
                    sample.getLineSpacingMultiplier(), sample.getLineSpacingExtra(),
                    sample.getIncludeFontPadding());
        }
        return Math.max(1, sl.getLineCount());
    }

    /**
     * 展开菜单 / 功能被拦 / 后台定时 续票。
     * 硬失败（会话无效/封禁）→ 清会话锁功能；纯网络错误不踢。
     */
    /**
     * 心跳/续票：只做服务端状态同步（刷新卡密信息、菜单门控、bootstrap）。
     * 设计原则：登录后功能不中断，心跳失败不清功能、不踢登录。
     * 只有进程重启才会真正失去授权。
     */
    private void maybeRenewGrant(String reason) {
        long now = SystemClock.uptimeMillis();
        if (grantRenewInFlight) return;
        if (lastGrantRenewElapsed > 0
                && (now - lastGrantRenewElapsed) < GRANT_RENEW_MIN_INTERVAL_MS) {
            return;
        }
        if (Preferences.isCardAuthRequired()) {
            String token = Preferences.getAuthToken();
            boolean hasToken = token != null && !token.isEmpty();
            // 2026-10-04：token 被上一版误清、但卡密还在时也放行 —— 心跳会以「未登录」拒绝，
            // 正好走静默自愈用卡密重登，把云机从「未授权死状态」救回来。
            String card = Preferences.getAuthCardKey();
            boolean hasCard = card != null && !card.isEmpty();
            if (!hasToken && !hasCard) return;
        } else {
            String g = Preferences.getAuthGrant();
            if (g == null || g.isEmpty()) return;
        }
        grantRenewInFlight = true;
        final String why = reason != null ? reason : "renew";
        runAuth(() -> {
            try {
                ModBridge.syncAuthGameIdFromNative();
                String deviceId = Preferences.getOrCreateDeviceId();
                String gameId = Preferences.getAuthGameId();
                String res;
                if (Preferences.isCardAuthRequired()) {
                    JSONObject body = new JSONObject();
                    body.put("token", Preferences.getAuthToken());
                    body.put("device_id", deviceId);
                    body.put("game_id", gameId);
                    res = AuthClient.request(
                            Preferences.getAuthBaseUrl(), "POST", "/v1/auth/heartbeat",
                            body.toString(), Preferences.getAuthToken());
                } else {
                    String path = "/v1/bootstrap?game_id=" + Uri.encode(gameId)
                            + "&device_id=" + Uri.encode(deviceId)
                            + "&logged_in=false";
                    res = AuthClient.request(
                            Preferences.getAuthBaseUrl(), "GET", path, null, null);
                }
                JSONObject json = new JSONObject(res);
                if (isNetworkSoftFailure(json)) {
                    // 网络问题：不动功能，交给冷启动循环重试
                    Log.w(TAG, "renew soft fail (" + why + "): " + json.optString("message", ""));
                    return;
                }
                JSONObject data = json.has("data") ? json.optJSONObject("data") : json;
                if (data == null) data = json;
                // 认证失败分两档（2026-09-05 审查修正）：
                // isHardAuthFailureJson 关键字命中 = 服务端权威卡密判定（封禁/删除/过期/解绑/设备）；
                // looksLikeAuthFailure 是宽松形态（任意 detail/error，含网关 404/429/限流/非卡密错误）。
                // 只有权威判定才允许在冷启动踢回登录；宽松形态一律不踢，防止把限流/网关错误
                // 误判成封禁而清掉存档会话。进程内（expand）维持旧行为：只刷 UI，不清功能不踢。
                if (isHardAuthFailureJson(json) || looksLikeAuthFailure(json, data)) {
                    boolean coldStart = "cold-start".equals(why);
                    boolean authoritative = isHardAuthFailureJson(json);
                    Log.w(TAG, "renew auth fail (" + why + ", authoritative=" + authoritative
                            + "): " + json.optString("message", ""));
                    if (coldStart && authoritative) {
                        // 2026-10-04：设备漂移/会话失效属可自愈类（本地存着卡密），
                        // 先静默自愈重登；只有权威致命（封禁/用尽/过期/不存在）才踢。
                        // 二次修正：可自愈类失败**永远不踢**——自愈已试过或不可用时也不清档。
                        // 否则「自愈后仍失败」会在下一轮续票重试时落到踢人分支，
                        // 把存档连同卡密一起清掉，变成永久未授权（实测已复现）。
                        if (isSelfHealableFailureJson(json)) {
                            boolean healed = trySilentSelfHeal(why, json);
                            logAuthStoreVerdict(healed ? "relogin" : "heal-deferred");
                            if (!healed) {
                                Log.w(TAG, "[auth] healable-class failure, heal unavailable"
                                        + " -> keep archive, retry next launch: "
                                        + json.optString("message", ""));
                                mainHandler.post(() -> refreshAuthStatus());
                            }
                            grantRenewCycleDone = true;
                            return;
                        }
                        grantRenewCycleDone = true;
                        logAuthStoreVerdict("kick");
                        final String serverMsg = json.optString("message", "");
                        mainHandler.post(() -> hardKickAuth(serverMsg));
                    } else {
                        mainHandler.post(() -> refreshAuthStatus());
                    }
                    return;
                }
                String grant = Preferences.pickGrant(data);
                if (grant.isEmpty()) {
                    Log.w(TAG, "renew no grant (" + why + ")");
                    return;
                }
                int cacheSec = data.optInt("offline_cache_seconds", 180);
                JSONObject boot = data.optJSONObject("bootstrap");
                String bootJson = boot != null ? boot.toString() : Preferences.getAuthBootstrapJson();
                if (Preferences.isCardAuthRequired()) {
                    String token = data.optString("token", Preferences.getAuthToken());
                    String expireAt = data.optString("expires_at", Preferences.getAuthExpireAt());
                    JSONObject cardObj = data.optJSONObject("card");
                    if (cardObj != null) {
                        Preferences.saveAuthCardMeta(cardObj.toString());
                        String cardExpire = cardObj.optString("expire_at", "");
                        if (!cardExpire.isEmpty() && !"null".equals(cardExpire)) {
                            expireAt = cardExpire;
                        }
                    }
                    Preferences.saveAuthSession(
                            token, Preferences.getAuthCardKey(),
                            System.currentTimeMillis(), cacheSec, expireAt, bootJson, grant);
                } else {
                    if (boot == null) bootJson = data.toString();
                    Preferences.saveNetworkBootstrap(cacheSec, bootJson, grant);
                    if (data.has("menu_enabled")) {
                        Preferences.setMenuEnabled(data.optBoolean("menu_enabled", true));
                    }
                    if (data.has("card_auth_enabled")) {
                        Preferences.setCardAuthRequired(data.optBoolean("card_auth_enabled", false));
                    }
                }
                if (boot != null) {
                    Preferences.saveBootstrapJson(boot.toString());
                    if (boot.has("menu_enabled")) {
                        Preferences.setMenuEnabled(boot.optBoolean("menu_enabled", true));
                    }
                }
                mainHandler.post(() -> {
                    // 续票成功即结束冷启动对账（进程内不再续票）
                    grantBgRenewStarted = false;
                    grantRenewCycleDone = true;
                    mainHandler.removeCallbacks(grantBgRenewRunnable);
                    boolean wasLive = bridge.hasLiveNativeSession();
                    // menu_enabled=false 是服务端维护，仍不清功能（用户已登录）
                    // 只更新 UI 状态
                    bridge.syncNativeAuthorizedPublic();
                    // 仅当本次续票把死会话救活（冷启动存档票过期）才按存档恢复开关；
                    // 会话本来就活时不全量重推，避免把运行态功能打回（2026-09-05）
                    if (!wasLive) {
                        bridge.reapplySavedFeaturesToNative();
                    }
                    updateAccountTabVisibility();
                    refreshAuthStatus();
                    refreshFeatures();
                    Log.i(TAG, "renew ok (" + why + ")");
                    logAuthStoreVerdict("ok");
                });
            } catch (Exception e) {
                // 网络异常：不动功能，交给冷启动循环有界重试
                Log.w(TAG, "renew network error (" + why + "): " + e.getMessage());
            } finally {
                lastGrantRenewElapsed = SystemClock.uptimeMillis();
                grantRenewInFlight = false;
            }
        });
    }

    private boolean shouldRunGrantBgRenew() {
        // 存档里有登录态（卡密 token / grant / 卡密本体）才值得对账；与会话是否活着无关
        if (Preferences.isCardAuthRequired()) {
            String token = Preferences.getAuthToken();
            if (token != null && !token.isEmpty()) return true;
            // 只有卡密、没有 token：仍要对账一次，让静默自愈把身份重新对齐（2026-10-04）
            String card = Preferences.getAuthCardKey();
            return card != null && !card.isEmpty();
        }
        String grant = Preferences.getAuthGrant();
        return grant != null && !grant.isEmpty();
    }

    /** 冷启动续票入口：进程内只对一次账（菜单 show / bootstrap 拉取时触发），成功/硬拒/重试耗尽即停 */
    private void startGrantBgRenew() {
        if (grantRenewCycleDone) return;
        if (!shouldRunGrantBgRenew()) return;
        if (grantBgRenewStarted) return;
        grantBgRenewStarted = true;
        mainHandler.removeCallbacks(grantBgRenewRunnable);
        mainHandler.postDelayed(grantBgRenewRunnable, 3_000L);
        Log.i(TAG, "cold-start renew scheduled (bounded, no periodic heartbeat)");
    }

    private void stopGrantBgRenew() {
        grantBgRenewStarted = false;
        mainHandler.removeCallbacks(grantBgRenewRunnable);
    }

    /** 续票失败时：不动功能，等下次心跳重试。登录后功能不中断。 */
    private void applyLocalGrantOrKickIfDead() {
        // 不清功能、不踢登录，只更新 UI
        refreshAuthStatus();
    }

    private void hardKickAuth(String toastMsg) {
        Runnable kick = () -> {
            stopGrantBgRenew();
            // 2026-10-04：清档前会先留一份 auth_backup_*，原因一并写入（云机排障证据）
            bridge.clearAuthSession("kick:" + (toastMsg == null || toastMsg.isEmpty()
                    ? "授权已失效" : toastMsg));
            Preferences.saveAuthCardMeta("{}");
            refreshAuthStatus();
            refreshFeatures();
            updateAccountTabVisibility();
            showToast(toastMsg != null && !toastMsg.isEmpty()
                    ? toastMsg : "授权已失效，请重新登录", "error");
        };
        if (Looper.myLooper() == Looper.getMainLooper()) kick.run();
        else mainHandler.post(kick);
    }

    private static boolean isNetworkSoftFailure(JSONObject json) {
        if (json == null) return true;
        String m = (json.optString("message", "") + " " + json.optString("detail", "")).toLowerCase();
        if (m.contains("超时") || m.contains("网络") || m.contains("证书")
                || m.contains("解析") || m.contains("连接") || m.contains("unreachable")
                || m.contains("timeout") || m.contains("ssl")) {
            return true;
        }
        // AuthClient 包装的失败且无 detail：多为传输层
        if (json.has("ok") && !json.optBoolean("ok", true) && !json.has("detail")
                && json.optJSONObject("data") == null) {
            String msg = json.optString("message", "");
            return msg.contains("失败") || msg.contains("未配置") || msg.contains("HTTPS");
        }
        return false;
    }

    private static boolean isHardAuthFailureJson(JSONObject json) {
        if (json == null) return false;
        if (json.has("detail")) {
            String d = String.valueOf(json.opt("detail"));
            return d.contains("会话") || d.contains("登录") || d.contains("封禁")
                    || d.contains("过期") || d.contains("设备") || d.contains("不存在")
                    || d.contains("权限") || d.contains("用尽");
        }
        if (json.has("ok") && !json.optBoolean("ok", true)) {
            String m = json.optString("message", "");
            return m.contains("会话") || m.contains("登录") || m.contains("过期")
                    || m.contains("封禁") || m.contains("未登录");
        }
        return false;
    }

    /**
     * 可自愈类权威失败判定（2026-10-04 云机「第二次打开未授权」修复）。
     *
     * <p>服务端 heartbeat 的两条权威拒绝都属于「本机当前 device_id 与服务端记录不一致」，
     * 本地存着卡密时可原样重登自愈（login 带 {@code legacy_device_ids[]}，服务端
     * {@code _authorize_device_binding} 在卡密绑定的旧 id 命中时会把绑定迁到新 id）：
     * <ul>
     *   <li>401「会话与设备/游戏不匹配」——云机 SSAID 漂移导致 device_id 变了；</li>
     *   <li>403「设备绑定已变更，请重新登录」——同上，服务端语义更重。</li>
     * </ul>
     * 这两类<b>不得据此清档</b>（清档即用户看到的「保存的地方没有」）。
     *
     * <p>真正致命、必须踢回登录的是：封禁 / 卡密用尽 / 卡密过期 / 卡密不存在 / 无权限。
     */
    private static boolean isSelfHealableFailureJson(JSONObject json) {
        if (json == null) return false;
        String m = (json.optString("detail", "") + " " + json.optString("message", ""))
                .toLowerCase(Locale.ROOT);
        if (m.isEmpty()) return false;
        if (m.contains("封禁") || m.contains("用尽") || m.contains("不存在")
                || m.contains("过期") || m.contains("权限") || m.contains("已删除")) {
            return false;
        }
        if (m.contains("ban") || m.contains("exhaust") || m.contains("expired")
                || m.contains("not found") || m.contains("forbidden")) {
            return false;
        }
        return m.contains("设备") || m.contains("会话") || m.contains("未登录")
                || m.contains("重新登录") || m.contains("device") || m.contains("session");
    }

    /**
     * 冷启动静默自愈：本地还存着卡密就用它重登一次，让服务端把设备绑定重新对齐到本机当前
     * {@code device_id}，并把新会话/新 grant 写回存档。<b>全程不动存档</b>（失败也不清）。
     *
     * @return true 表示已发起重登，调用方应立即 return，不要走硬踢
     */
    private boolean trySilentSelfHeal(String why, JSONObject json) {
        if (authSelfHealAttempted) {
            Log.i(TAG, "[auth] self-heal already attempted this process, skip");
            return false;
        }
        if (!Preferences.isCardAuthRequired()) return false;
        String card = Preferences.getAuthCardKey();
        if (card == null || card.isEmpty()) {
            Log.i(TAG, "[auth] self-heal unavailable: no stored card key");
            return false;
        }
        authSelfHealAttempted = true;
        Log.w(TAG, "[auth] cold-start self-heal: device/session mismatch (" + why
                + ") → relogin with stored card, msg=" + json.optString("message", ""));
        authLoginWithCard(card, true);
        return true;
    }

    /** 静默自愈登录失败后的处置：非致命只记日志，致命（封禁/用尽/过期/卡不存在）才踢。 */
    private void handleAuthLoginFailure(String err, boolean silent, boolean fatal) {
        if (!silent) {
            mainHandler.post(() -> showToast(err, "error"));
            return;
        }
        if (fatal) {
            Log.w(TAG, "[auth] silent self-heal fatal: " + err + " → hard kick");
            mainHandler.post(() -> hardKickAuth(err));
        } else {
            Log.w(TAG, "[auth] silent self-heal non-fatal: " + err + " (keep archive, retry later)");
        }
    }

    /**
     * 存档裁决日志：授权键读写链路每一步都留证据（云机排障靠它定量，不再靠猜）。
     * 输出不含 token/card/grant 明文，只给「有没有、匹不匹配、存在哪个文件」。
     */
    private void logAuthStoreVerdict(String verdict) {
        try {
            String device = Preferences.getOrCreateDeviceId();
            Log.i(TAG, "[auth] store verdict=" + verdict
                    + " device=" + device
                    + " source=" + Preferences.getDeviceIdSource()
                    + " has_grant=" + Preferences.hasStoredGrant()
                    + " grant_match=" + Preferences.grantDeviceMatches(device)
                    + " token=" + (Preferences.getAuthToken() == null
                            || Preferences.getAuthToken().isEmpty() ? "none" : "set")
                    + " card=" + (Preferences.getAuthCardKey() == null
                            || Preferences.getAuthCardKey().isEmpty() ? "none" : "set")
                    + " backup_at=" + Preferences.getAuthBackupAt()
                    + " backup_reason=" + Preferences.getAuthBackupReason()
                    + " path=" + Preferences.getStorePath());
        } catch (Throwable t) {
            Log.w(TAG, "[auth] store verdict log failed: " + t);
        }
    }

    private void doAuthLogin() {
        String card = cardInput != null ? cardInput.getText().toString().trim() : "";
        if (card.isEmpty()) {
            showToast("请输入账号", "error");
            return;
        }
        showToast("登录中…", "info");
        authLoginWithCard(card, false);
    }

    /**
     * 登录主流程。silent=true 时是冷启动静默自愈（设备指纹漂移 / 存档票过期）：
     * 不弹「登录中…/登录成功」，但致命失败（封禁/用尽/过期/卡不存在）仍会踢回登录并提示，
     * 非致命失败（网络/网关）只记日志，交给冷启动重试循环。
     */
    private void authLoginWithCard(final String card, final boolean silent) {
        runAuth(() -> {
            try {
                ModBridge.syncAuthGameIdFromNative();
                String deviceId = Preferences.getOrCreateDeviceId();
                String gameId = Preferences.getAuthGameId();
                Log.i(TAG, "login with game_id=" + gameId);
                JSONObject body = new JSONObject();
                body.put("card_key", card);
                body.put("device_id", deviceId);
                body.put("game_id", gameId);
                JSONArray legacyIds = new JSONArray();
                for (String legacyId : Preferences.getLegacyDeviceIds()) {
                    legacyIds.put(legacyId);
                }
                if (legacyIds.length() > 0) {
                    body.put("legacy_device_ids", legacyIds);
                }
                String loginRes = AuthClient.request(
                        Preferences.getAuthBaseUrl(), "POST", "/v1/auth/login", body.toString(), null);
                JSONObject loginJson = new JSONObject(loginRes);

                // AuthClient / 网关失败：{ok:false,message}
                if (loginJson.has("ok") && !loginJson.optBoolean("ok", true)) {
                    handleAuthLoginFailure(friendlyAuthError(loginJson, "登录失败"), silent,
                            isHardAuthFailureJson(loginJson) && !isSelfHealableFailureJson(loginJson));
                    return;
                }

                JSONObject data = loginJson.optJSONObject("data");
                if (data == null) data = loginJson;

                if (looksLikeAuthFailure(loginJson, data)) {
                    handleAuthLoginFailure(friendlyAuthError(loginJson, "登录失败"), silent,
                            isHardAuthFailureJson(loginJson) && !isSelfHealableFailureJson(loginJson));
                    return;
                }
                String token = data.optString("token", loginJson.optString("token", ""));
                if (token.isEmpty()) {
                    handleAuthLoginFailure(friendlyAuthError(loginJson, "登录失败"), silent, false);
                    return;
                }
                JSONObject cardObj = data.optJSONObject("card");
                if (cardObj != null) {
                    Preferences.saveAuthCardMeta(cardObj.toString());
                } else {
                    Preferences.saveAuthCardMeta("{}");
                }
                // 卡密到期用 card.expire_at；顶层 expires_at 是短会话票，不能当卡密到期
                String cardExpire = "";
                if (cardObj != null) {
                    cardExpire = cardObj.optString("expire_at", "");
                    if ("null".equals(cardExpire)) cardExpire = "";
                }
                String grant = Preferences.pickGrant(data);
                int cacheSec = data.optInt("offline_cache_seconds", data.optInt("cache_sec", 600));

                JSONObject bootData = data.optJSONObject("bootstrap");
                String bootRes = bootData != null ? bootData.toString() : "{}";
                if (bootData == null) {
                    String fetched = AuthClient.request(
                            Preferences.getAuthBaseUrl(), "GET",
                            "/v1/bootstrap?game_id=" + Uri.encode(gameId)
                                    + "&device_id=" + Uri.encode(deviceId)
                                    + "&logged_in=true",
                            null, token);
                    JSONObject boot = new JSONObject(fetched);
                    bootData = boot.has("data") ? boot.optJSONObject("data") : boot;
                    bootRes = fetched;
                }
                if (bootData != null) {
                    if (grant.isEmpty()) {
                        grant = Preferences.pickGrant(bootData);
                        if (grant.isEmpty()) grant = bootData.optString("feature_grant", "");
                    }
                    if (bootData.has("card_auth_enabled")) {
                        Preferences.setCardAuthRequired(bootData.optBoolean("card_auth_enabled", true));
                    }
                    if (bootData.has("menu_enabled")) {
                        Preferences.setMenuEnabled(bootData.optBoolean("menu_enabled", true));
                    }
                }
                if (grant.isEmpty()) {
                    JSONObject gBody = new JSONObject();
                    gBody.put("device_id", deviceId);
                    gBody.put("game_id", gameId);
                    String gRes = AuthClient.request(
                            Preferences.getAuthBaseUrl(), "POST", "/v1/auth/grant",
                            gBody.toString(), token);
                    JSONObject gJson = new JSONObject(gRes);
                    JSONObject gData = gJson.has("data") ? gJson.optJSONObject("data") : gJson;
                    if (gData != null) {
                        grant = Preferences.pickGrant(gData);
                        if (grant.isEmpty()) grant = gData.optString("token", "");
                    }
                }
                Preferences.saveAuthSession(token, card, System.currentTimeMillis(),
                        cacheSec, cardExpire, bootRes, grant);
                if (!Preferences.isMenuEnabled()) {
                    bridge.lockAllFeatures();
                } else {
                    bridge.syncNativeAuthorizedPublic();
                }
                mainHandler.post(() -> {
                    enforceServerFeatureFlags();
                    updateAccountTabVisibility();
                    refreshAuthStatus();
                    refreshFeatures();
                    // 登录即刚拿到新鲜 grant：不再触发冷启动对账，避免登录后立刻重复请求
                    grantRenewCycleDone = true;
                    if (silent) {
                        // 静默自愈成功：不打扰用户，只留证据（云机第二次打开应直接回到「授权有效」）
                        Log.i(TAG, "[auth] cold-start self-heal ok, session restored silently");
                    } else if (!Preferences.isMenuEnabled()) {
                        showToast("游戏维护中，功能已关闭", "error");
                    } else {
                        showToast("登录成功", "success");
                    }
                });
            } catch (Exception e) {
                Log.e(TAG, "auth login", e);
                handleAuthLoginFailure(friendlyExceptionMessage(e), silent, false);
            }
        });
    }

    private static boolean looksLikeAuthFailure(JSONObject root, JSONObject data) {
        if (root == null) return true;
        if (root.has("detail")) return true;
        if (root.has("error")) return true;
        if (root.has("code") && root.optInt("code", 0) >= 400) return true;
        if (data != null && data.has("error")) return true;
        return false;
    }

    /** 把 FastAPI detail / message / 整段 JSON 收成一句人话，绝不把校验堆栈弹给用户 */
    private static String friendlyAuthError(JSONObject json, String fallback) {
        if (json == null) return fallback;
        String msg = json.optString("message", "").trim();
        if (!msg.isEmpty() && !looksLikeRawCode(msg)) return shortenToast(msg);

        String err = json.optString("error", "").trim();
        if (!err.isEmpty() && !looksLikeRawCode(err)) return shortenToast(err);

        Object detail = json.opt("detail");
        if (detail instanceof String) {
            String d = ((String) detail).trim();
            if (!d.isEmpty() && !looksLikeRawCode(d)) return shortenToast(d);
        } else if (detail instanceof JSONArray) {
            JSONArray arr = (JSONArray) detail;
            for (int i = 0; i < arr.length(); i++) {
                JSONObject item = arr.optJSONObject(i);
                if (item == null) continue;
                String m = item.optString("msg", item.optString("message", "")).trim();
                if (m.isEmpty()) continue;
                // 常见：Field required → 请输入账号
                if (m.toLowerCase().contains("field required")
                        || m.toLowerCase().contains("required")) {
                    String loc = item.optJSONArray("loc") != null
                            ? item.optJSONArray("loc").toString() : "";
                    if (loc.contains("card_key")) return "请输入账号";
                    return "请填写完整信息";
                }
                if (!looksLikeRawCode(m)) return shortenToast(m);
            }
            return "请求参数无效";
        } else if (detail instanceof JSONObject) {
            String m = ((JSONObject) detail).optString("msg",
                    ((JSONObject) detail).optString("message", "")).trim();
            if (!m.isEmpty() && !looksLikeRawCode(m)) return shortenToast(m);
        }
        return fallback;
    }

    private static boolean looksLikeRawCode(String s) {
        if (s == null) return true;
        String t = s.trim();
        return t.startsWith("{") || t.startsWith("[") || t.contains("\"loc\"")
                || t.contains("Traceback") || t.contains("Exception");
    }

    private static String shortenToast(String s) {
        if (s == null) return "";
        String t = s.trim();
        return t.length() > 60 ? t.substring(0, 60) + "…" : t;
    }

    private static String friendlyExceptionMessage(Exception e) {
        if (e == null) return "登录异常";
        String m = e.getMessage();
        if (m == null || m.isEmpty() || looksLikeRawCode(m)) return "登录异常";
        return shortenToast(m);
    }

    private int dp(int v) {
        return Math.round(TypedValue.applyDimension(
                TypedValue.COMPLEX_UNIT_DIP, v, appContext.getResources().getDisplayMetrics()));
    }
}
