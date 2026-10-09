package com.starry.magic.app;

import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
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

/**
 * 主 Activity — 应用入口
 *
 * 原生菜单主路径不需要悬浮窗权限即可出球；备用 Overlay 可在菜单「设置」里开启。
 */
public class MainActivity extends Activity {

    private static final String TAG = "StarryMagic";
    private static final int REQUEST_OVERLAY = 1001;
    private static final String NATIVE_LIB_NAME = "starry_magic";

    private int passThroughHits = 0;
    private TextView passThroughCount;

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
            setupPassThroughTestUi();
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

    /** 状态通道自检：验证「星光助手 → 游戏外部目录状态文件」是否可读写。 */
    private void runStateChannelSelfTest() {
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
            String[] probes = {
                    com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[0],
                    "/storage/emulated/0/Android/media/" + getPackageName(),
                    "/storage/emulated/0/Android/media",
                    com.starry.magic.menu.ModTarget.EXTERNAL_DIRS[1],
                    "/storage/emulated/0/Android/data/" + getPackageName() + "/files",
                    "/storage/emulated/0/Download",
                    "/storage/emulated/0/Documents",
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

    private void setupPassThroughTestUi() {
        Button btn = findViewById(R.id.btn_pass_through_test);
        passThroughCount = findViewById(R.id.tv_pass_through_count);
        if (btn == null) return;
        btn.setOnClickListener(v -> {
            passThroughHits++;
            if (passThroughCount != null) {
                passThroughCount.setText("已点到：" + passThroughHits + " 次");
            }
            Toast.makeText(this, "穿透正常：点到了 Activity 按钮", Toast.LENGTH_SHORT).show();
            Log.i(TAG, "pass-through test hit #" + passThroughHits);
        });
        Button overlay = findViewById(R.id.btn_request_overlay);
        if (overlay != null) {
            overlay.setOnClickListener(v -> requestOverlayPermission());
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
            Toast.makeText(this, "可选：开启悬浮窗作为菜单备用路径", Toast.LENGTH_LONG).show();
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
