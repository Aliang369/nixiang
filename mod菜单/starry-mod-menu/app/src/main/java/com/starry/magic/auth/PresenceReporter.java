package com.starry.magic.auth;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.Signature;
import android.content.pm.SigningInfo;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;

import com.starry.magic.menu.Preferences;

import org.json.JSONObject;

import java.security.MessageDigest;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * 在线心跳上报（2026-11-23 游客统计）。
 *
 * <p>五条硬约束（改动前必读 docs/STATS_DEFINITION.md）：
 * <ol>
 *   <li>只刷新服务端在线表，**不参与任何授权判断**：失败、超时、被丢弃都不动功能、不清存档；</li>
 *   <li>游客（未登录）与卡密共用同一接口，token 为空即游客；</li>
 *   <li>5 分钟一次；进程启动后先补一次，之后按周期；进程死即停（不注册广播/闹钟）；</li>
 *   <li>上报内容只含统计所需字段，不含卡密明文；</li>
 *   <li>任何异常都在本类内部吞掉，绝不能让菜单/游戏崩溃。</li>
 * </ol>
 */
public final class PresenceReporter {

    private static final String TAG = "PresenceReporter";

    /** 周期：5 分钟（服务端在线窗口 360 秒，必须大于本值）。 */
    public static final long INTERVAL_MS = 5 * 60 * 1000L;
    /** 首报延迟：等进程/native 加载稳定，避免开局抢资源。 */
    private static final long FIRST_DELAY_MS = 8_000L;

    private static final Handler HANDLER = new Handler(Looper.getMainLooper());
    private static final ExecutorService POOL = Executors.newSingleThreadExecutor(r -> {
        Thread t = new Thread(r, "starry-presence");
        t.setDaemon(true);
        return t;
    });

    private static volatile boolean started;
    private static volatile String certSha256;
    private static Context appContext;

    private static final Runnable TICK = new Runnable() {
        @Override
        public void run() {
            try {
                pushAsync();
            } catch (Throwable t) {
                Log.w(TAG, "tick failed: " + t);
            } finally {
                if (started) {
                    HANDLER.postDelayed(this, INTERVAL_MS);
                }
            }
        }
    };

    private PresenceReporter() {
    }

    /** 幂等启动：重复调用不会叠加定时器。 */
    public static void start(Context context) {
        if (context == null) return;
        appContext = context.getApplicationContext() != null
                ? context.getApplicationContext() : context;
        if (started) return;
        started = true;
        HANDLER.removeCallbacks(TICK);
        HANDLER.postDelayed(TICK, FIRST_DELAY_MS);
        Log.i(TAG, "presence started interval=" + INTERVAL_MS + "ms");
    }

    public static void stop() {
        started = false;
        HANDLER.removeCallbacks(TICK);
        Log.i(TAG, "presence stopped");
    }

    private static void pushAsync() {
        final Context ctx = appContext;
        if (ctx == null) return;
        POOL.execute(() -> {
            try {
                pushOnce(ctx);
            } catch (Throwable t) {
                // 统计失败永不上浮：网络/证书/服务端波动都不影响游戏
                Log.w(TAG, "push failed (ignored): " + t);
            }
        });
    }

    private static void pushOnce(Context ctx) throws Exception {
        String gameId = Preferences.getAuthGameId();
        String deviceId = Preferences.getOrCreateDeviceId();
        if (gameId == null || gameId.isEmpty()) return;
        if (deviceId == null || deviceId.isEmpty()) return;

        JSONObject body = new JSONObject();
        body.put("game_id", gameId);
        body.put("device_id", deviceId);
        String token = Preferences.getAuthToken();
        if (token != null && !token.isEmpty()) {
            body.put("token", token);
        }
        String[] ver = versionOf(ctx);
        body.put("client_version", ver[0]);
        body.put("client_version_code", Integer.parseInt(ver[1]));
        String cert = certSha256 != null ? certSha256 : (certSha256 = computeCertSha256(ctx));
        if (cert != null && !cert.isEmpty()) {
            body.put("cert_sha256", cert);
        }

        String res = AuthClient.request(
                Preferences.getAuthBaseUrl(), "POST", "/v1/presence",
                body.toString(), token);
        JSONObject json = new JSONObject(res);
        JSONObject data = json.optJSONObject("data");
        boolean counted = data != null && data.optBoolean("counted", false);
        Log.i(TAG, "presence game=" + gameId + " counted=" + counted
                + " identity=" + (data != null ? data.optString("identity", "") : ""));
    }

    private static String[] versionOf(Context ctx) {
        try {
            PackageInfo pi = ctx.getPackageManager().getPackageInfo(ctx.getPackageName(), 0);
            String name = pi.versionName != null ? pi.versionName : "0";
            long code = Build.VERSION.SDK_INT >= 28 ? pi.getLongVersionCode() : pi.versionCode;
            return new String[]{name, String.valueOf(code)};
        } catch (Throwable t) {
            return new String[]{"0", "0"};
        }
    }

    /** APK 签名证书 SHA-256（小写 hex）；取不到返回 null（服务端白名单为空时不校验）。 */
    private static String computeCertSha256(Context ctx) {
        try {
            PackageManager pm = ctx.getPackageManager();
            String pkg = ctx.getPackageName();
            Signature[] sigs = null;
            if (Build.VERSION.SDK_INT >= 28) {
                PackageInfo pi = pm.getPackageInfo(pkg, PackageManager.GET_SIGNING_CERTIFICATES);
                SigningInfo info = pi.signingInfo;
                if (info != null) {
                    sigs = info.hasMultipleSigners()
                            ? info.getApkContentsSigners()
                            : info.getSigningCertificateHistory();
                }
            } else {
                PackageInfo pi = pm.getPackageInfo(pkg, PackageManager.GET_SIGNATURES);
                sigs = pi.signatures;
            }
            if (sigs == null || sigs.length == 0) return null;
            byte[] digest = MessageDigest.getInstance("SHA-256").digest(sigs[0].toByteArray());
            StringBuilder sb = new StringBuilder(digest.length * 2);
            for (byte b : digest) {
                sb.append(Character.forDigit((b >> 4) & 0xF, 16));
                sb.append(Character.forDigit(b & 0xF, 16));
            }
            return sb.toString();
        } catch (Throwable t) {
            Log.w(TAG, "cert digest unavailable: " + t);
            return null;
        }
    }
}
