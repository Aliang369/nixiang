package com.starry.magic.menu;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.VpnService;
import android.os.Build;
import android.os.Environment;
import android.provider.Settings;
import android.text.TextUtils;
import android.util.Log;

import com.starry.magic.BuildConfig;
import com.starry.magic.menu.net.CdnVpnService;

import java.io.File;
import java.io.FileInputStream;
import java.nio.charset.StandardCharsets;

/**
 * 「体检 / 卡点状态」。
 *
 * <p>为什么要有它：这东西是发给别人的，对方手机上出的问题我们看不见、也拿不到日志。
 * 所以把「这条链路走到哪一步了」直接算出来显示在屏幕上 —— 玩家拍屏、或点「复制报告」
 * 把文本发回来，就等于把日志交上来了，且不需要电脑、不需要 root、不需要任何权限。
 */
public final class Diag {

    private static final String TAG = "StarryDiag";

    /** 一次性采集的快照。 */
    private static final class Snap {
        String version;
        String device;
        String now;
        String gameName = "邂逅在迷宫";
        String gamePkg;
        String gameVer;
        boolean gameInstalled;

        boolean allFiles;
        boolean overlay;
        boolean vpnGranted;


        String statePath;
        boolean stateFileExists;
        String stateBody;
        long stateTs;

        boolean clicked;
        long blockedTs;
        String blockedVal;
        long launchTs;

        boolean chRunning;
        String chStatus;

        long chStartTs;
        long chUpTs;
        long chCloseTs;
        String chCloseVal;
        long vpnNullTs;
        long revokeTs;

        long fetchTs;
        String fetchVal;
        int fetchCount;
        long bundleTs;
        String bundleVal;

        long errTs;
        String errVal;

        boolean statePersistFailed;

        String blocker;
        String fix;
    }

    private Diag() {
    }

    /** 屏幕上那句大字：当前卡点。 */
    public static String headline(Context ctx) {
        return collect(ctx).blocker;
    }

    /** 完整报告：可长按复制、可分享。 */
    public static String report(Context ctx) {
        Snap s = collect(ctx);
        StringBuilder sb = new StringBuilder(1024);
        sb.append("【星光助手体检报告】\n");
        sb.append("助手版本 ").append(s.version).append('\n');
        sb.append("体检时间 ").append(s.now).append('\n');
        sb.append("机型 ").append(s.device).append('\n');
        sb.append("游戏 ").append(s.gameName).append(' ').append(s.gamePkg)
                .append(s.gameInstalled ? " ✓ 已安装 " + safe(s.gameVer) : " ✗ 没装")
                .append('\n');
        sb.append("① 所有文件访问权限 ").append(mark(s.allFiles)).append('\n');
        sb.append("② 悬浮窗权限 ").append(mark(s.overlay)).append('\n');
        sb.append("VPN 授权 ").append(s.vpnGranted
                ? "✓ 已授权（点③不会再弹系统窗，属正常）"
                : "✗ 未授权（点③会弹一次系统窗）").append('\n');

        if (s.stateFileExists && s.stateBody != null) {
            sb.append("状态文件 ✓ ").append(s.stateBody)
                    .append("（").append(InjectJournal.fmt(s.stateTs)).append("）\n");
            boolean same = StateMirror.currentState().equals(s.stateBody);
            sb.append("   ↳ 与菜单开关 ")
                    .append(same ? "一致 ✓" : "不一致 ✗（菜单=" + StateMirror.currentState() + "）")
                    .append('\n');
        } else if (s.stateFileExists) {
            sb.append("状态文件 ✗ 存在但读不出来：").append(s.statePath).append('\n');
        } else {
            sb.append("状态文件 ✗ 不存在／读不到")
                    .append(s.statePath == null ? "（拿不到游戏目录）" : "：" + s.statePath)
                    .append('\n');
        }
        if (s.statePersistFailed) {
            sb.append("   ↳ 最近一次写失败 ").append(safe(StateMirror.lastError())).append('\n');
        }

        if (s.chRunning) {
            sb.append("注入通道 ✓ 本次已开启 ").append(safe(s.chStatus)).append('\n');
        } else if (s.chStartTs > 0L) {
            sb.append("注入通道 上次 ").append(InjectJournal.fmt(s.chStartTs)).append(" 建立");
            if (s.chUpTs > 0L) {
                sb.append(" → ").append(InjectJournal.fmt(s.chUpTs)).append(" 隧道就绪");
            }
            if (s.chCloseTs > 0L) {
                sb.append(" → ").append(InjectJournal.fmt(s.chCloseTs)).append(" 关闭"
                        + (s.chCloseVal == null ? "" : "（" + s.chCloseVal + "）"));
            }
            sb.append('\n');
        } else if (s.clicked) {
            sb.append("注入通道 ✗ 点过③但通道没起来 ").append(safe(s.chStatus)).append('\n');
        } else {
            sb.append("注入通道 ✗ 从没点过 ③\n");
        }

        sb.append("   ↳ 游戏来取补丁 ")
                .append(s.fetchTs > 0L
                        ? "✓ " + safe(s.fetchVal) + "，共 " + s.fetchCount + " 个文件"
                        : "✗ 没有取过")
                .append('\n');
        if (s.bundleVal != null) {
            sb.append("     补丁 DLL ✓ ").append(s.bundleVal)
                    .append("（").append(InjectJournal.fmt(s.bundleTs)).append("）\n");
        }
        sb.append("   ↳ 最近错误 ").append(s.errVal == null ? "无" : s.errVal).append('\n');
        if (s.blockedTs > 0L) {
            sb.append("   ↳ 曾被闸门拦下 ").append(safe(s.blockedVal))
                    .append("（").append(InjectJournal.fmt(s.blockedTs)).append("）\n");
        }
        if (s.vpnNullTs > 0L) {
            sb.append("   ↳ VPN 建立失败 ").append(InjectJournal.fmt(s.vpnNullTs)).append('\n');
        }
        if (s.revokeTs > 0L) {
            sb.append("   ↳ 通道被抢占 ").append(InjectJournal.fmt(s.revokeTs)).append('\n');
        }

        sb.append('\n').append("结论 ").append(s.blocker).append('\n');
        sb.append("下一步 ").append(s.fix).append('\n');
        sb.append("（把以上内容整段发给对方即可，不需要日志）");
        return sb.toString();
    }

    private static Snap collect(Context ctx) {
        Snap s = new Snap();
        s.version = BuildConfig.VERSION_NAME + "（vc" + BuildConfig.VERSION_CODE + "）";
        s.now = InjectJournal.fmt(System.currentTimeMillis());
        s.device = Build.MANUFACTURER + " " + Build.MODEL + " · Android "
                + Build.VERSION.RELEASE + " · " + Build.DISPLAY;
        s.gamePkg = ModTarget.GAME_PKG;
        PackageManager pm = ctx.getPackageManager();
        try {
            PackageInfo pi = pm.getPackageInfo(s.gamePkg, 0);
            s.gameInstalled = true;
            s.gameVer = pi.versionName;
        } catch (Throwable ignored) {
            s.gameInstalled = false;
        }

        s.allFiles = hasAllFiles();
        s.overlay = Settings.canDrawOverlays(ctx);
        s.vpnGranted = vpnGranted(ctx);

        s.statePath = StateMirror.externalPath();
        if (s.statePath != null) {
            File f = new File(s.statePath);
            if (f.isFile()) {
                s.stateFileExists = true;
                s.stateTs = f.lastModified();
                s.stateBody = readTrim(f);
            }
        }
        s.statePersistFailed = StateMirror.lastError() != null;

        s.clicked = InjectJournal.ts(ctx, InjectJournal.K_CLICK) > 0L;
        s.blockedTs = InjectJournal.ts(ctx, InjectJournal.K_BLOCKED);
        s.blockedVal = InjectJournal.val(ctx, InjectJournal.K_BLOCKED);
        s.launchTs = InjectJournal.ts(ctx, InjectJournal.K_LAUNCH);
        s.chStartTs = InjectJournal.ts(ctx, InjectJournal.K_CH_START);
        s.chUpTs = InjectJournal.ts(ctx, InjectJournal.K_CH_UP);
        s.chCloseTs = InjectJournal.ts(ctx, InjectJournal.K_CH_CLOSE);
        s.chCloseVal = InjectJournal.val(ctx, InjectJournal.K_CH_CLOSE);
        s.vpnNullTs = InjectJournal.ts(ctx, InjectJournal.K_VPN_NULL);
        s.revokeTs = InjectJournal.ts(ctx, InjectJournal.K_REVOKE);
        s.fetchTs = InjectJournal.ts(ctx, InjectJournal.K_FETCH);
        s.fetchVal = InjectJournal.val(ctx, InjectJournal.K_FETCH);
        s.fetchCount = InjectJournal.count(ctx, InjectJournal.K_FETCH);
        s.bundleTs = InjectJournal.ts(ctx, InjectJournal.K_BUNDLE);
        s.bundleVal = InjectJournal.val(ctx, InjectJournal.K_BUNDLE);
        s.errTs = InjectJournal.ts(ctx, InjectJournal.K_ERR);
        s.errVal = InjectJournal.val(ctx, InjectJournal.K_ERR);
        s.chRunning = CdnVpnService.isRunning();
        s.chStatus = CdnVpnService.statusText();

        conclude(s);
        return s;
    }

    /**
     * 按「链路顺序」找第一个卡点 —— 顺序不能乱：先装没装、再权限、再状态文件、最后才是注入，
     * 因为后面的步骤都要靠前面的成立。
     */
    private static void conclude(Snap s) {
        if (!s.gameInstalled) {
            s.blocker = "卡点 1／这台手机没装《邂逅在迷宫》";
            s.fix = "先装好游戏，再回来点 ③";
            return;
        }
        if (!s.allFiles) {
            s.blocker = "卡点 2／「所有文件访问权限」没开 —— 功能必然全部无效";
            s.fix = "点 ① 开启（系统会跳到设置页，打开那一项再返回）";
            return;
        }
        if (!s.overlay) {
            s.blocker = "卡点 3／悬浮窗权限没开（游戏里的球出不来）";
            s.fix = "点 ② 开启悬浮窗权限";
            return;
        }
        if (!s.stateFileExists || s.stateBody == null) {
            s.blocker = "卡点 4／游戏目录里没有状态文件（权限没真正生效）";
            s.fix = "先点 ① 再开一次权限；仍不行就回游戏里拨一次开关，看本页「状态文件」是否变 ✓";
            return;
        }
        if (!s.stateBody.equals(StateMirror.currentState())) {
            s.blocker = "卡点 5／菜单里的开关没写进游戏目录（游戏读到的还是旧值）";
            s.fix = "在游戏里的球上把开关拨一次；或点 ① 让它重新写一次";
            return;
        }
        if (s.vpnNullTs > 0L && s.vpnNullTs >= s.fetchTs) {
            s.blocker = "卡点 6／VPN 通道建立失败（授权被撤销或被系统拦了）";
            s.fix = "点 ③ 重来一次；若仍失败，把本页发回去（很可能是这台系统在拦 VPN）";
            return;
        }
        if (s.revokeTs > 0L && s.revokeTs >= s.fetchTs) {
            s.blocker = "卡点 7／注入通道被系统或别的 VPN 抢占了";
            s.fix = "关掉其它 VPN／加速器，再点 ③ 重来一次";
            return;
        }
        if (!s.clicked) {
            s.blocker = "卡点 8／还没点过 ③（注入从未跑过）";
            s.fix = "点 ③ 一键注入并打开游戏";
            return;
        }
        if (s.chStartTs == 0L) {
            s.blocker = "卡点 9／点过 ③ 但注入通道没起来";
            s.fix = "点 ③ 再试一次；仍不行把本页发回去";
            return;
        }
        if (s.fetchTs == 0L) {
            s.blocker = "卡点 10／通道开了，但游戏没有来取补丁 —— 游戏不是冷启动";
            s.fix = "在最近任务里把游戏划掉（真划掉，不是按返回），再点 ③";
            return;
        }
        s.blocker = "✅ 全链路都通了（权限、状态文件、补丁下发都正常）";
        s.fix = "若游戏里还是没效果，请把这一屏发回去（附上游戏里看到了什么）";
    }

    private static boolean hasAllFiles() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.R) {
            return true;
        }
        try {
            return Environment.isExternalStorageManager();
        } catch (Throwable t) {
            return false;
        }
    }

    private static boolean vpnGranted(Context ctx) {
        try {
            return VpnService.prepare(ctx) == null;
        } catch (Throwable t) {
            return false;
        }
    }

    /** 读不出内容时返回 null（＝没有真正可用的状态文件）。 */
    private static String readTrim(File f) {
        try (FileInputStream in = new FileInputStream(f)) {
            byte[] buf = new byte[512];
            int n = in.read(buf);
            if (n <= 0) {
                return "";
            }
            return new String(buf, 0, n, StandardCharsets.UTF_8).trim();
        } catch (Throwable t) {
            Log.w(TAG, "diag read state failed: " + t);
            return null;
        }
    }

    private static String mark(boolean ok) {
        return ok ? "✓ 已开" : "✗ 没开";
    }

    private static String safe(String s) {
        return TextUtils.isEmpty(s) ? "—" : s;
    }
}
