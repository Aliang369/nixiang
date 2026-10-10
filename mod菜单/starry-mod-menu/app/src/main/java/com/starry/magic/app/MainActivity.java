package com.starry.magic.app;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.net.VpnService;
import android.os.Build;
import android.os.Bundle;
import android.provider.Settings;
import android.util.Log;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.starry.magic.BuildConfig;
import com.starry.magic.R;
import com.starry.magic.menu.MenuService;
import com.starry.magic.menu.net.CdnVpnService;

/**
 * 主 Activity — 应用入口
 *
 * 原生菜单主路径不需要悬浮窗权限即可出球；备用 Overlay 可在菜单「设置」里开启。
 */
public class MainActivity extends Activity {

    private static final String TAG = "StarryMagic";
    private static final int REQUEST_OVERLAY = 1001;
    private static final int REQUEST_VPN = 1002;
    private static final String NATIVE_LIB_NAME = "starry_magic";

    private TextView cdnStatus;

    static {
        try {
            System.loadLibrary(NATIVE_LIB_NAME);
            Log.i(TAG, "Native library loaded: " + NATIVE_LIB_NAME);
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "Failed to load native library: " + NATIVE_LIB_NAME, e);
        }
    }

    /** 注入模式：填游戏主 Activity 全限定名；独立 APK 保持空串 */
    public String gameActivity = "";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (!gameActivity.isEmpty()) {
            try {
                startActivity(new Intent(this, Class.forName(gameActivity)));
            } catch (ClassNotFoundException e) {
                Log.e(TAG, "Game activity not found: " + gameActivity);
            }
        } else {
            setContentView(R.layout.activity_main);
            setupHomeUi();
            firstRunGuide();
        }

        // 自检入口：am start -n com.starry.magic/.app.MainActivity --ez starry_selftest true
        // 仅 debug 包可用：自检会真的写状态文件（把伤害倍率改成 42 之类），
        // release 包一旦带出去，任何人一条 am start 就能改用户游戏状态。
        if (BuildConfig.DEBUG
                && getIntent() != null
                && getIntent().getBooleanExtra("starry_selftest", false)) {
            runStateChannelSelfTest();
        }

        // 原生菜单：无权限也启动；Overlay 仅备用
        startMenu();
    }

    /**
     * 首启引导（分发版）：把两件「不点就不好使」的事自动走完。
     * 第 1 优先是悬浮窗权限——独立助手的菜单只能靠悬浮窗显示在游戏画面上，
     * 没这个权限玩家会以为「装上没反应」。引导只做提示与跳转，不改任何状态。
     */
    private void firstRunGuide() {
        try {
            if (!checkOverlayPermission()) {
                Toast.makeText(this,
                        "第 1 步：请给「星光助手」开启悬浮窗权限（菜单球要显示在游戏画面上）",
                        Toast.LENGTH_LONG).show();
                requestOverlayPermission();
                return;
            }
            android.content.SharedPreferences sp =
                    getSharedPreferences("starry_boot", MODE_PRIVATE);
            if (!sp.getBoolean("guide_done", false)) {
                sp.edit().putBoolean("guide_done", true).apply();
                Toast.makeText(this,
                        "悬浮窗已就绪 ✓\n下一步：点「② 一键注入」，游戏会自动打开",
                        Toast.LENGTH_LONG).show();
            }
        } catch (Throwable t) {
            Log.w(TAG, "firstRunGuide failed: " + t);
        }
    }

    /** 状态通道自检：验证「星光助手 → 游戏外部目录状态文件」是否可读写。 */    private void runStateChannelSelfTest() {
        new android.os.Handler(android.os.Looper.getMainLooper()).postDelayed(() -> {
            // 1) 所有文件访问权限是否真的生效
            boolean esm = false;
            try {
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                    esm = android.os.Environment.isExternalStorageManager();
                }
            } catch (Throwable ignored) {
            }
            Log.i(TAG, "SELFTEST isExternalStorageManager=" + esm
                    + " uid=" + android.os.Process.myUid());
            // 2) 逐个探测路径可写性（路径全部由 ModTarget 的目标包名拼出）
            String gamePkg = com.starry.magic.menu.ModTarget.GAME_PKG;
            String selfPkg = getPackageName();
            // 关键探测矩阵：目标游戏 Android/data 的各种等价路径都要试，
            // 因为 FUSE 的跨包隔离判定可能对 /sdcard、/storage/emulated/0、
            // /mnt/user/0/primary 走不同分支（历史上有过只有某一条能写的情况）。
            String[] probes = {
                    com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[0],
                    "/storage/emulated/0/Android/media/" + selfPkg,
                    "/storage/emulated/0/Android/media",
                    com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[1],
                    "/storage/emulated/0/Android/data/" + gamePkg,
                    "/storage/emulated/0/Android/data/" + gamePkg + "/files",
                    "/storage/emulated/0/Android/data/" + gamePkg + "/files/Bundles",
                    "/sdcard/Android/data/" + gamePkg + "/files",
                    "/sdcard/Android/data/" + gamePkg + "/files/Bundles",
                    "/mnt/user/0/primary/Android/data/" + gamePkg + "/files",
                    "/mnt/user/0/emulated/0/Android/data/" + gamePkg + "/files",
                    "/data/media/0/Android/data/" + gamePkg + "/files",
                    "/storage/emulated/0/Android/data/" + selfPkg + "/files",
                    "/storage/emulated/0/Download",
                    "/storage/emulated/0",
            };
            // 先尝试 mkdir（跨 App 创建 media 子目录）
            java.io.File mk = new java.io.File(com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[0]);
            Log.i(TAG, "SELFTEST mkdir game media = " + mk.mkdirs() + " isDir=" + mk.isDirectory());
            for (String dir : probes) {
                java.io.File d = new java.io.File(dir);
                java.io.File t = new java.io.File(d, "starry_probe.txt");
                String r;
                try {
                    java.io.FileOutputStream out = new java.io.FileOutputStream(t);
                    out.write("probe".getBytes("UTF-8"));
                    out.close();
                    r = "WRITE_OK";
                    //noinspection ResultOfMethodCallIgnored
                    t.delete();
                } catch (Throwable e) {
                    r = e.getClass().getSimpleName() + ":" + e.getMessage();
                }
                Log.i(TAG, "SELFTEST dir=" + dir + " isDir=" + d.isDirectory()
                        + " canRead=" + d.canRead() + " canWrite=" + d.canWrite()
                        + " → " + r);
            }
            // 2b) 进程身份：uid + 补充组（FUSE 的 DAC 判定会看组，如 ext_data_rw/media_rw）
            try (java.io.BufferedReader br = new java.io.BufferedReader(new java.io.InputStreamReader(
                    new java.io.FileInputStream("/proc/self/status"), "UTF-8"))) {
                String ln;
                while ((ln = br.readLine()) != null) {
                    if (ln.startsWith("Groups:") || ln.startsWith("Uid:")) {
                        Log.i(TAG, "SELFTEST proc " + ln);
                    }
                }
            } catch (Throwable t) {
                Log.w(TAG, "SELFTEST proc status failed: " + t);
            }
            // 2c) 直接 shell 一下，看 app 自己 fork 出来的进程是否也是同一套权限
            try {
                Process pr = Runtime.getRuntime().exec(new String[]{
                        "sh", "-c", "ls -ld /sdcard/Android/data/" + gamePkg + " 2>&1"});
                java.io.BufferedReader br = new java.io.BufferedReader(
                        new java.io.InputStreamReader(pr.getInputStream(), "UTF-8"));
                String ln = br.readLine();
                Log.i(TAG, "SELFTEST sh ls game data dir: " + ln);
                br.close();
            } catch (Throwable t) {
                Log.w(TAG, "SELFTEST sh failed: " + t);
            }
            // 2d) 试试软链接：如果 media 下能建一个指向游戏 data 目录的链接，
            //     说明 FUSE 至少有写权限（本机历史上 FUSE 不支持 symlink，仅作记录）
            try {
                java.io.File link = new java.io.File(
                        com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[0], "probe_link");
                //noinspection ResultOfMethodCallIgnored
                link.delete();
                android.system.Os.symlink(
                        "/storage/emulated/0/Android/data/" + gamePkg + "/files",
                        link.getAbsolutePath());
                Log.i(TAG, "SELFTEST symlink ok -> " + link.getAbsolutePath());
            } catch (Throwable t) {
                Log.i(TAG, "SELFTEST symlink failed: " + t);
            }

            // 3) 目标状态文件
            try {
                com.starry.magic.menu.StateMirror.attach(getApplicationContext());
                com.starry.magic.menu.StateMirror.mirror(1, 42);
            } catch (Throwable t) {
                Log.e(TAG, "SELFTEST mirror failed", t);
            }
            java.io.File f = new java.io.File(
                    com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[1], "starry_state.txt");
            String body = "<unreadable>";
            try (java.io.InputStream in = new java.io.FileInputStream(f)) {
                byte[] b = new byte[64];
                int n = in.read(b);
                if (n > 0) body = new String(b, 0, n, java.nio.charset.StandardCharsets.UTF_8);
            } catch (Throwable ignored) {
            }
            Log.i(TAG, "SELFTEST targetFile exists=" + f.exists()
                    + " canWrite=" + f.canWrite() + " body=[" + body + "]");
        }, 1500L);
    }

    /** 独立助手主页：① 悬浮窗权限 → ② 一键注入（注入后自动打开游戏）。 */
    private void setupHomeUi() {
        Button overlay = findViewById(R.id.btn_request_overlay);
        if (overlay != null) {
            overlay.setOnClickListener(v -> requestOverlayPermission());
        }
        cdnStatus = findViewById(R.id.tv_cdn_status);
        Button cdn = findViewById(R.id.btn_cdn_inject);
        if (cdn != null) {
            cdn.setOnClickListener(v -> toggleCdnInject());
        }
        android.widget.CheckBox autoClose = findViewById(R.id.cb_cdn_auto_close);
        if (autoClose != null) {
            autoClose.setChecked(CdnVpnService.autoClosePref(this));
            autoClose.setOnCheckedChangeListener((v, checked) -> {
                CdnVpnService.setAutoClosePref(MainActivity.this, checked);
                Log.i(TAG, "cdn inject: auto close = " + checked);
            });
        }
        refreshCdnStatus();
    }

    /** 热更注入开关：点一下（必要时先过系统 VPN 授权）。注入完成后会自动关闭。 */
    private void toggleCdnInject() {
        if (CdnVpnService.isRunning()) {
            Log.i(TAG, "cdn inject: stop requested by user");
            CdnVpnService.stop(this);
            Toast.makeText(this, "热更注入已关闭（游戏下次启动会回到官方数据）",
                    Toast.LENGTH_LONG).show();
            refreshCdnStatus();
            return;
        }
        Intent prep = VpnService.prepare(this);
        if (prep != null) {
            Log.i(TAG, "cdn inject: need vpn permission");
            Toast.makeText(this, "请允许「星光助手」建立本地 VPN 连接", Toast.LENGTH_LONG).show();
            startActivityForResult(prep, REQUEST_VPN);
            return;
        }
        startCdnInject();
    }

    private void startCdnInject() {
        CdnVpnService.start(this);
        Toast.makeText(this,
                "热更注入已开启：正在打开游戏，请照常进到主界面即可",
                Toast.LENGTH_LONG).show();
        refreshCdnStatus();
        launchGameForInjection();
    }

    /** 通道就绪后自动拉起游戏：发给别人的包里不能让玩家自己找图标点进来。 */
    private void launchGameForInjection() {
        try {
            Intent i = getPackageManager()
                    .getLaunchIntentForPackage(com.starry.magic.menu.ModTarget.GAME_PKG);
            if (i == null) {
                Log.w(TAG, "cdn inject: 未找到 " + com.starry.magic.menu.ModTarget.GAME_PKG);
                Toast.makeText(this,
                        "没找到游戏「邂逅在迷宫」：确认已安装且属同一渠道",
                        Toast.LENGTH_LONG).show();
                return;
            }
            i.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
            startActivity(i);
            Log.i(TAG, "cdn inject: 自动拉起游戏 " + com.starry.magic.menu.ModTarget.GAME_PKG);
        } catch (Throwable t) {
            Log.w(TAG, "cdn inject: auto launch game failed: " + t);
        }
    }

    private void refreshCdnStatus() {
        if (cdnStatus == null) {
            return;
        }
        cdnStatus.setText(CdnVpnService.isRunning()
                ? CdnVpnService.statusText()
                : "注入：未做过（做完一次就永久生效）");
        Button b = findViewById(R.id.btn_cdn_inject);
        if (b != null) {
            b.setText(CdnVpnService.isRunning()
                    ? "通道已开启（注入完成会自动关闭）"
                    : "② 一键注入并打开游戏（只需一次）");
        }
    }

    private void startMenu() {
        Log.i(TAG, "Starting menu service...");
        MenuService.start(this);
        if (!gameActivity.isEmpty()) {
            finish();
        }
    }

    private boolean checkOverlayPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            return Settings.canDrawOverlays(this);
        }
        return true;
    }

    private void requestOverlayPermission() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
            Toast.makeText(this,
                    "请开启「悬浮窗权限」：菜单球要靠它显示在游戏画面上（关掉就看不到菜单）",
                    Toast.LENGTH_LONG).show();
            Intent intent = new Intent(
                Settings.ACTION_MANAGE_OVERLAY_PERMISSION,
                Uri.parse("package:" + getPackageName())
            );
            startActivityForResult(intent, REQUEST_OVERLAY);
        }
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == REQUEST_OVERLAY) {
            Toast.makeText(this,
                    checkOverlayPermission() ? "备用悬浮窗已开启" : "未开启备用权限（主路径仍可用）",
                    Toast.LENGTH_SHORT).show();
        } else if (requestCode == REQUEST_VPN) {
            if (resultCode == RESULT_OK) {
                Log.i(TAG, "cdn inject: vpn permission granted");
                startCdnInject();
            } else {
                Log.w(TAG, "cdn inject: vpn permission denied");
                Toast.makeText(this, "未授权 VPN，无法注入热更数据", Toast.LENGTH_LONG).show();
                refreshCdnStatus();
            }
        }
    }

    @Override
    protected void onStart() {
        super.onStart();
        if (gameActivity.isEmpty()) {
            setMenuOverlayVisible(true);
        }
    }

    @Override
    protected void onStop() {
        super.onStop();
        if (gameActivity.isEmpty()) {
            setMenuOverlayVisible(false);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        refreshCdnStatus();
        MenuService service = MenuService.getInstance();
        if (service != null && !gameActivity.isEmpty()) {
            finish();
        }
    }

    private void setMenuOverlayVisible(boolean visible) {
        MenuService service = MenuService.getInstance();
        if (service == null) return;
        service.setOverlayVisible(visible);
        Log.i(TAG, "overlay visible=" + visible);
    }

    @Override
    public void onConfigurationChanged(android.content.res.Configuration newConfig) {
        super.onConfigurationChanged(newConfig);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (gameActivity.isEmpty() && isFinishing()) {
            MenuService service = MenuService.getInstance();
            if (service != null) {
                Log.i(TAG, "Activity finishing, stop MenuService");
                stopService(new Intent(this, MenuService.class));
            }
        }
    }
}
