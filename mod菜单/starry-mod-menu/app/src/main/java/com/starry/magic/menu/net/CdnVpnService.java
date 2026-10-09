package com.starry.magic.menu.net;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.net.ConnectivityManager;
import android.net.LinkProperties;
import android.net.Network;
import android.net.VpnService;
import android.os.Build;
import android.os.ParcelFileDescriptor;
import android.util.Log;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.net.InetAddress;

/**
 * 助手自带的「热更注入」通道：在手机本地冒充明文 HTTP 热更 CDN。
 *
 * <p>它做三件事：
 * <ol>
 *   <li>把 {@code install2.hotune.com} 的 DNS 解析劫持到隧道内的假地址（其余域名原样转发）</li>
 *   <li>兜底路由真实 CDN 网段 118.183.236.0/24，DNS 劫持失效也照样接管</li>
 *   <li>用用户态 TCP 应答 HTTP 请求，把 APK assets 里带的三件套（versions.json /
 *       buildpatch json / 补丁 bundle）当成 CDN 的响应发给游戏</li>
 * </ol>
 * 于是游戏在下一次冷启动时会「自己」把补丁 bundle 写进它自己的
 * {@code Android/data/cn.alltune.endless/files/Bundles/}——全程不需要电脑、不需要 root。
 *
 * <p>只接管上述目标；其它流量不进隧道（分应用/分网段路由），所以不影响别的 App。
 */
public class CdnVpnService extends VpnService implements Runnable {

    public static final String ACTION_START = "com.starry.magic.action.CDN_START";
    public static final String ACTION_STOP = "com.starry.magic.action.CDN_STOP";

    private static final String CHANNEL_ID = "starry_cdn";
    private static final int NOTIF_ID = 0x5CD1;
    private static final String TAG = FakeCdn.TAG;

    /** 补丁数据下发完成后，隧道路由空闲多久就自动关闭（毫秒）。 */
    private static final long AUTO_CLOSE_IDLE_MS = 15000L;
    /** 兜底：一直没等到注入、又长时间没流量，也关掉，避免 VPN 卡住整机网络。 */
    private static final long STALE_IDLE_MS = 150000L;
    private static final long STALE_UPTIME_MS = 180000L;

    private static volatile boolean sRunning;
    /** 是否「注入完成后自动关闭」；由界面写入（SharedPreferences key cdn_auto_close）。 */
    private static volatile boolean sAutoClose = true;

    public static void setAutoClose(boolean on) {
        sAutoClose = on;
    }

    public static boolean autoClose() {
        return sAutoClose;
    }

    /** 界面与服务的共享偏好：注入完成后是否自动关闭。 */
    public static final String PREF = "starry_prefs";
    public static final String KEY_AUTO_CLOSE = "cdn_auto_close";

    public static boolean autoClosePref(Context c) {
        return c.getSharedPreferences(PREF, Context.MODE_PRIVATE)
                .getBoolean(KEY_AUTO_CLOSE, true);
    }

    public static void setAutoClosePref(Context c, boolean v) {
        c.getSharedPreferences(PREF, Context.MODE_PRIVATE)
                .edit().putBoolean(KEY_AUTO_CLOSE, v).apply();
        sAutoClose = v;
    }

    private ParcelFileDescriptor tunFd;
    private FileInputStream tunIn;
    private FileOutputStream tunOut;
    private final Object writeLock = new Object();
    private Thread readerThread;
    private Thread tickThread;
    private volatile boolean running;
    private TcpRelay relay;
    private DnsInterceptor dns;

    public static boolean isRunning() {
        return sRunning;
    }

    /** 给界面用的状态文案。 */
    public static String statusText() {
        if (!sRunning) {
            int served = FakeCdn.SERVED.get();
            if (served > 0) {
                return "热更注入：已完成（本次会话下发 " + served + " 个文件）";
            }
            return "热更注入：未开启";
        }
        StringBuilder sb = new StringBuilder("热更注入：已开启 · 已下发补丁数据 ")
                .append(FakeCdn.SERVED.get()).append(" 次");
        if (FakeCdn.PASSED.get() > 0) {
            sb.append(" · 回源透传 ").append(FakeCdn.PASSED.get()).append(" 个");
        }
        return sb.toString();
    }

    public static void start(Context ctx) {
        Intent i = new Intent(ctx, CdnVpnService.class).setAction(ACTION_START);
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                ctx.startForegroundService(i);
            } else {
                ctx.startService(i);
            }
        } catch (Throwable t) {
            Log.e(TAG, "start vpn service failed", t);
        }
    }

    public static void stop(Context ctx) {
        try {
            ctx.startService(new Intent(ctx, CdnVpnService.class).setAction(ACTION_STOP));
        } catch (Throwable t) {
            Log.e(TAG, "stop vpn service failed", t);
        }
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        String action = (intent == null) ? ACTION_START : intent.getAction();
        sAutoClose = autoClosePref(this);
        if (ACTION_STOP.equals(action)) {
            Log.i(TAG, "stop requested");
            teardown();
            stopSelf();
            return START_NOT_STICKY;
        }
        startForegroundNotice();
        if (!running) {
            startTunnel();
        } else {
            Log.i(TAG, "already running");
        }
        return START_STICKY;
    }

    private void startForegroundNotice() {
        try {
            NotificationManager nm = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O && nm != null) {
                NotificationChannel ch = new NotificationChannel(CHANNEL_ID, "热更注入",
                        NotificationManager.IMPORTANCE_LOW);
                ch.setShowBadge(false);
                nm.createNotificationChannel(ch);
            }
            Intent launch = getPackageManager().getLaunchIntentForPackage(getPackageName());
            PendingIntent pi = null;
            if (launch != null) {
                launch.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                pi = PendingIntent.getActivity(this, 0, launch,
                        Build.VERSION.SDK_INT >= Build.VERSION_CODES.M
                                ? PendingIntent.FLAG_IMMUTABLE : 0);
            }
            Notification.Builder b = Build.VERSION.SDK_INT >= Build.VERSION_CODES.O
                    ? new Notification.Builder(this, CHANNEL_ID)
                    : new Notification.Builder(this);
            b.setContentTitle("星光助手 · 热更注入中")
                    .setContentText("打开一次游戏即可完成注入，注入完成后本通道会自动关闭")
                    .setSmallIcon(android.R.drawable.stat_sys_download_done)
                    .setOngoing(true);
            if (pi != null) {
                b.setContentIntent(pi);
            }
            startForeground(NOTIF_ID, b.build());
        } catch (Throwable t) {
            Log.w(TAG, "foreground notice failed: " + t);
        }
    }

    private void startTunnel() {
        try {
            Builder b = new Builder();
            b.setSession("StarryCdn");
            b.setMtu(1500);
            b.addAddress(FakeCdn.VPN_ADDR_STR, FakeCdn.VPN_PREFIX);
            b.addDnsServer(FakeCdn.VPN_DNS_STR);
            b.addRoute(FakeCdn.ROUTE_STR, FakeCdn.VPN_PREFIX);
            b.addRoute(FakeCdn.FAKE_CDN_IP[0] + "." + FakeCdn.FAKE_CDN_IP[1] + "."
                    + FakeCdn.FAKE_CDN_IP[2] + "." + FakeCdn.FAKE_CDN_IP[3], 32);
            // 兜底：即使 DNS 没劫持到，真实 CDN 网段的明文 HTTP 也进隧道
            b.addRoute(FakeCdn.CDN_ROUTE_STR, FakeCdn.CDN_ROUTE_PREFIX);
            addUnderlyingDnsRoutes(b);
            // 把自己排除在隧道外，避免 DNS 转发/日志回环
            try {
                b.addDisallowedApplication(getPackageName());
            } catch (Throwable t) {
                Log.w(TAG, "disallow self failed: " + t);
            }

            tunFd = b.establish();
            if (tunFd == null) {
                Log.e(TAG, "establish() returned null (permission revoked?)");
                stopSelf();
                return;
            }
            tunIn = new FileInputStream(tunFd.getFileDescriptor());
            tunOut = new FileOutputStream(tunFd.getFileDescriptor());
            relay = new TcpRelay(this, this::sendToTun, this::protectSocket);
            dns = new DnsInterceptor(this, this::sendToTun);
            running = true;
            sRunning = true;

            readerThread = new Thread(this, "starry-tun");
            readerThread.setDaemon(true);
            readerThread.start();

            final long upAt = android.os.SystemClock.elapsedRealtime();
            tickThread = new Thread(() -> {
                while (running) {
                    try {
                        Thread.sleep(200L);
                    } catch (InterruptedException e) {
                        return;
                    }
                    try {
                        relay.tick();
                    } catch (Throwable t) {
                        Log.w(TAG, "tick failed: " + t);
                    }
                    try {
                        // ★ 注入完成后自动关闭：不再长期占着 VPN 影响游戏正常下载
                        if (sAutoClose && shouldAutoClose(upAt)) {
                            Log.i(TAG, "注入完成，自动关闭热更注入通道（不再影响游戏正常下载）");
                            teardown();
                            stopSelf();
                            return;
                        }
                    } catch (Throwable t) {
                        Log.w(TAG, "auto close check failed: " + t);
                    }
                }
            }, "starry-tick");
            tickThread.setDaemon(true);
            tickThread.start();

            Log.i(TAG, "VPN up: addr=" + FakeCdn.VPN_ADDR_STR + " dns=" + FakeCdn.VPN_DNS_STR
                    + " routes=" + FakeCdn.ROUTE_STR + "/" + FakeCdn.VPN_PREFIX + ","
                    + FakeCdn.CDN_ROUTE_STR + "/" + FakeCdn.CDN_ROUTE_PREFIX
                    + " overrides=" + FakeCdn.describe());
        } catch (Throwable t) {
            Log.e(TAG, "startTunnel failed", t);
            teardown();
            stopSelf();
        }
    }

    private boolean protectSocket(java.net.Socket s) {
        try {
            return protect(s);
        } catch (Throwable t) {
            return false;
        }
    }

    /**
     * 该不该自动关闭：
     * <ul>
     *   <li>补丁 bundle 已完整下发 + 没有活连接 + 空闲够久 → 关</li>
     *   <li>兜底：起来很久了却没等到注入、也没流量 → 关（避免 VPN 卡住整机网络）</li>
     * </ul>
     */
    private boolean shouldAutoClose(long upAt) {
        TcpRelay r = relay;
        if (r == null) {
            return false;
        }
        long idle = r.idleMs();
        if (r.isInjectionDone() && r.activeConns() == 0 && idle > AUTO_CLOSE_IDLE_MS) {
            return true;
        }
        long uptime = android.os.SystemClock.elapsedRealtime() - upAt;
        return idle > STALE_IDLE_MS && uptime > STALE_UPTIME_MS;
    }

    /** 把当前底层网络的 DNS 服务器也 /32 路由进来，这样 DNS 一定经过我们。 */    private void addUnderlyingDnsRoutes(Builder b) {
        try {
            ConnectivityManager cm = (ConnectivityManager) getSystemService(CONNECTIVITY_SERVICE);
            Network n = cm.getActiveNetwork();
            LinkProperties lp = cm.getLinkProperties(n);
            if (lp == null) {
                return;
            }
            for (InetAddress a : lp.getDnsServers()) {
                String h = a.getHostAddress();
                if (h != null && h.indexOf(':') < 0) {
                    b.addRoute(h, 32);
                    Log.i(TAG, "dns route " + h + "/32");
                }
            }
        } catch (Throwable t) {
            Log.w(TAG, "addUnderlyingDnsRoutes failed: " + t);
        }
    }

    @Override
    public void run() {
        byte[] buf = new byte[32767];
        while (running) {
            int n;
            try {
                n = tunIn.read(buf);
            } catch (Throwable t) {
                if (running) {
                    Log.w(TAG, "tun read ended: " + t);
                }
                break;
            }
            if (n <= 0) {
                continue;
            }
            try {
                dispatch(buf, n);
            } catch (Throwable t) {
                Log.w(TAG, "dispatch failed: " + t);
            }
        }
        Log.i(TAG, "tun loop exit");
    }

    private void dispatch(byte[] buf, int len) {
        if (len < 20 || (buf[0] >> 4) != 4) {
            return;
        }
        int ihl = (buf[0] & 0x0f) * 4;
        int proto = buf[9] & 0xff;
        if (proto == 6) {
            if (FakeCdn.isCdnDest(buf, 0)) {
                relay.onTcpPacket(buf, len);
            }
        } else if (proto == 17) {
            if (FakeCdn.isCdnDest(buf, 0) || TunPackets.u16(buf, ihl + 2) == FakeCdn.DNS_PORT) {
                dns.onUdpPacket(buf, len);
            }
        }
    }

    private void sendToTun(byte[] pkt) {
        FileOutputStream out = tunOut;
        if (out == null || pkt == null || pkt.length == 0) {
            return;
        }
        synchronized (writeLock) {
            try {
                out.write(pkt, 0, pkt.length);
            } catch (Throwable t) {
                if (running) {
                    Log.w(TAG, "tun write failed: " + t);
                }
            }
        }
    }

    @Override
    public void onRevoke() {
        Log.w(TAG, "onRevoke: another VPN took over");
        teardown();
        stopSelf();
        super.onRevoke();
    }

    @Override
    public void onDestroy() {
        teardown();
        super.onDestroy();
    }

    private void teardown() {
        running = false;
        sRunning = false;
        if (dns != null) {
            dns.shutdown();
            dns = null;
        }
        if (relay != null) {
            relay.shutdown();
            relay = null;
        }
        try {
            if (tunIn != null) {
                tunIn.close();
            }
        } catch (Throwable ignored) {
        }
        try {
            if (tunOut != null) {
                tunOut.close();
            }
        } catch (Throwable ignored) {
        }
        tunIn = null;
        tunOut = null;
        try {
            if (tunFd != null) {
                tunFd.close();
            }
        } catch (Throwable ignored) {
        }
        tunFd = null;
        readerThread = null;
        tickThread = null;
        try {
            stopForeground(true);
        } catch (Throwable ignored) {
        }
    }
}
