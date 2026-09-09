package com.starry.magic.crash;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.Build;
import android.util.Log;
import android.widget.Toast;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.lang.Thread.UncaughtExceptionHandler;
import java.nio.charset.StandardCharsets;
import java.text.SimpleDateFormat;
import java.util.Arrays;
import java.util.Date;
import java.util.Locale;
import java.util.Map;

/**
 * 全局崩溃日志（借鉴 LGL Android-Mod-Menu 5.0 CrashHandler，2026-09-06 适配星光）。
 *
 * <ul>
 *   <li>捕获 Java 层未捕获异常，落盘到沙盒内路径（免存储权限，玩家可用文件管理器直接取）；
 *   <li>报告含：宿主包名/版本、菜单 auth game_id、设备/系统、线程、完整堆栈、全线程转储；
 *   <li>保留最近 10 份，自动清理更旧文件；
 *   <li>只覆盖 Java 层——native 段错误不经过这里（native 稳定性走桩纪律/SafeProbe 体系）。
 * </ul>
 *
 * 挂载点：MenuService.onCreate（游戏进程内唯一入口）+ StarryApp.onCreate（自测壳）。
 */
public final class CrashHandler {

    private static final String TAG = "StarryCrash";
    private static final String LOG_DIR = "crash_logs";
    private static final String FILE_PREFIX = "crash_";
    private static final String FILE_EXTENSION = ".txt";
    private static final int KEEP_LOGS = 10;
    /** 菜单框架构建标识：排查时区分玩家手上是哪一代框架 */
    private static final String MENU_BUILD = "framework-20260906";
    private static final boolean SHOW_TOAST = true;

    private static UncaughtExceptionHandler defaultHandler;
    private static Context appContext;

    private CrashHandler() {}

    public static void init(Context context) {
        if (context == null || defaultHandler != null) return;
        try {
            appContext = context.getApplicationContext();
            defaultHandler = Thread.getDefaultUncaughtExceptionHandler();
            cleanOldLogs();
            Thread.setDefaultUncaughtExceptionHandler(new UncaughtExceptionHandler() {
                @Override
                public void uncaughtException(Thread thread, Throwable throwable) {
                    Log.e(TAG, "Uncaught exception detected", throwable);
                    try {
                        handleException(thread, throwable);
                    } catch (Exception e) {
                        Log.e(TAG, "Crash handler failed", e);
                        if (defaultHandler != null) {
                            defaultHandler.uncaughtException(thread, throwable);
                        } else {
                            android.os.Process.killProcess(android.os.Process.myPid());
                            System.exit(10);
                        }
                    }
                }
            });
            Log.i(TAG, "initialized, log dir: " + getLogDirectory().getAbsolutePath());
        } catch (Throwable t) {
            Log.e(TAG, "init failed", t);
        }
    }

    private static void handleException(Thread thread, Throwable throwable) {
        File crashFile = saveCrashLog(throwable);

        if (SHOW_TOAST && appContext != null) {
            showCrashNotification(crashFile);
        }
        // 留时间给 toast 显示与日志落盘，再交给系统默认处理（保持原有崩溃行为）
        try {
            Thread.sleep(1500);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
        }
        if (defaultHandler != null) {
            defaultHandler.uncaughtException(thread, throwable);
        } else {
            android.os.Process.killProcess(android.os.Process.myPid());
            System.exit(10);
        }
    }

    private static File saveCrashLog(Throwable throwable) {
        if (appContext == null) return null;
        String timeStamp = new SimpleDateFormat("yyyy_MM_dd-HH_mm_ss", Locale.US).format(new Date());
        File logFile = new File(getLogDirectory(), FILE_PREFIX + timeStamp + FILE_EXTENSION);
        try {
            writeCrashLog(logFile, throwable);
            Log.i(TAG, "Crash log saved: " + logFile.getAbsolutePath());
            return logFile;
        } catch (IOException e) {
            Log.e(TAG, "Failed to save crash log", e);
            return null;
        }
    }

    private static void writeCrashLog(File file, Throwable throwable) throws IOException {
        File parentDir = file.getParentFile();
        if (parentDir != null && !parentDir.exists() && !parentDir.mkdirs()) {
            Log.w(TAG, "Failed to create directory: " + parentDir.getAbsolutePath());
        }
        try (FileOutputStream fos = new FileOutputStream(file)) {
            fos.write(buildCrashReport(throwable).getBytes(StandardCharsets.UTF_8));
            fos.flush();
        }
    }

    private static String buildCrashReport(Throwable throwable) {
        StringBuilder report = new StringBuilder();
        report.append("=============== CRASH REPORT ===============\n");
        report.append("Time: ").append(getCurrentTime()).append("\n");
        report.append("Menu: 星光魔法框架 (").append(MENU_BUILD).append(")\n");
        report.append("Host Package: ").append(appContext != null ? appContext.getPackageName() : "unknown").append("\n");
        report.append("Host Version: ").append(getAppVersion()).append("\n");
        report.append("Auth Game ID: ").append(getAuthGameIdSafe()).append("\n");
        report.append("Android API: ").append(Build.VERSION.SDK_INT).append("\n");
        report.append("Android Version: ").append(Build.VERSION.RELEASE).append("\n");
        report.append("Device: ").append(Build.MANUFACTURER).append(" ").append(Build.MODEL).append("\n");
        report.append("Product: ").append(Build.PRODUCT).append("\n");
        report.append("Board: ").append(Build.BOARD).append("\n");
        report.append("Fingerprint: ").append(Build.FINGERPRINT).append("\n");
        report.append("Thread: ").append(Thread.currentThread().getName()).append("\n");
        report.append("=============== STACK TRACE ===============\n");

        StringWriter sw = new StringWriter();
        PrintWriter pw = new PrintWriter(sw);
        throwable.printStackTrace(pw);
        report.append(sw);

        // 全线程转储：定位是被哪个线程带崩的
        report.append("\n=============== ALL THREADS ===============\n");
        Map<Thread, StackTraceElement[]> allThreads = Thread.getAllStackTraces();
        for (Map.Entry<Thread, StackTraceElement[]> entry : allThreads.entrySet()) {
            Thread thread = entry.getKey();
            report.append("\nThread: ").append(thread.getName())
                    .append(" [ID: ").append(thread.getId())
                    .append(", State: ").append(thread.getState())
                    .append(", Priority: ").append(thread.getPriority())
                    .append("]\n");
            for (StackTraceElement element : entry.getValue()) {
                report.append("    at ").append(element).append("\n");
            }
        }
        pw.close();
        return report.toString();
    }

    /** 日志目录：优先 Android/media/<包名>/files/crash_logs（玩家文件管理器可直接访问），失败退回 app 外部目录 */
    private static File getLogDirectory() {
        File standardAppDir = appContext.getExternalFilesDir(null);
        if (standardAppDir == null) {
            return new File(appContext.getFilesDir(), LOG_DIR);
        }
        String targetPath = standardAppDir.getAbsolutePath().replace(
                "Android/data/" + appContext.getPackageName(),
                "Android/media/" + appContext.getPackageName());
        File logDir = new File(targetPath, LOG_DIR);
        if (!logDir.exists() && !logDir.mkdirs()) {
            return new File(standardAppDir, LOG_DIR);
        }
        return logDir;
    }

    private static void showCrashNotification(File crashFile) {
        String message;
        if (crashFile != null && crashFile.exists()) {
            String abs = crashFile.getAbsolutePath();
            int idx = abs.indexOf("/Android/");
            message = "菜单崩溃！日志：\n" + (idx >= 0 ? abs.substring(idx + 1) : abs);
        } else {
            message = "菜单崩溃！日志保存失败。";
        }
        try {
            Toast.makeText(appContext, message, Toast.LENGTH_LONG).show();
        } catch (Throwable ignored) {}
    }

    private static String getAppVersion() {
        if (appContext == null) return "unknown";
        try {
            PackageInfo pInfo = appContext.getPackageManager().getPackageInfo(appContext.getPackageName(), 0);
            long versionCode = Build.VERSION.SDK_INT >= Build.VERSION_CODES.P
                    ? pInfo.getLongVersionCode() : pInfo.versionCode;
            return pInfo.versionName + " (" + versionCode + ")";
        } catch (PackageManager.NameNotFoundException e) {
            return "unknown";
        }
    }

    /** 菜单鉴权 game_id（标识玩家玩的是哪款接了菜单的游戏）；native 未就绪时降级 unknown */
    private static String getAuthGameIdSafe() {
        try {
            Class<?> bridge = Class.forName("com.starry.magic.bridge.ModBridge");
            Object v = bridge.getMethod("getAuthGameId").invoke(null);
            return v != null ? String.valueOf(v) : "unknown";
        } catch (Throwable t) {
            return "unknown";
        }
    }

    private static String getCurrentTime() {
        return new SimpleDateFormat("yyyy-MM-dd HH:mm:ss Z", Locale.US).format(new Date());
    }

    /** 手动记录异常（供关键 catch 块调用），不影响当前流程 */
    public static void logException(Throwable throwable) {
        Log.e(TAG, "Manual exception logging", throwable);
        saveCrashLog(throwable);
    }

    /** 保留最近 KEEP_LOGS 份崩溃日志，清理更旧的 */
    public static void cleanOldLogs() {
        try {
            File logDir = getLogDirectory();
            if (!logDir.exists() || !logDir.isDirectory()) return;
            File[] logs = logDir.listFiles((dir, name) ->
                    name.startsWith(FILE_PREFIX) && name.endsWith(FILE_EXTENSION));
            if (logs == null || logs.length <= KEEP_LOGS) return;
            Arrays.sort(logs, (f1, f2) -> Long.compare(f2.lastModified(), f1.lastModified()));
            for (int i = KEEP_LOGS; i < logs.length; i++) {
                if (!logs[i].delete()) {
                    Log.w(TAG, "Failed to delete old log: " + logs[i].getName());
                }
            }
        } catch (Throwable t) {
            Log.w(TAG, "cleanOldLogs failed", t);
        }
    }
}
