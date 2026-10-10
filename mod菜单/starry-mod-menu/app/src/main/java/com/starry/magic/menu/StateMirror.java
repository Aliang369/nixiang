package com.starry.magic.menu;

import android.content.Context;
import android.util.Log;

import java.io.File;
import java.io.FileOutputStream;
import java.io.OutputStreamWriter;
import java.nio.charset.StandardCharsets;

/**
 * 功能状态镜像文件：把菜单功能值写成一个极小文本文件，供热更 C# 代码读取。
 *
 * 背景：HybridCLR 解释执行的热更程序集里 DllImport/P-Invoke 解析不到符号
 * （宝石战争 6.0.012 实测：调用必然抛异常），所以 C# 侧不能反向调用 native。
 * 这条通道完全走文件，C# 只用 File.ReadAllText / String.Split / int.Parse。
 *
 * 文件：{filesDir}/starry_state.txt，单行，空格分隔 feature 0..7 的当前值。
 *
 * 独立助手模式（免 root）：菜单不在游戏进程里，游戏读的是它自己的外部目录。
 * Android 11+ 的 FUSE 只对 Android/data 与 Android/obb 做跨包隔离，所以
 * /storage/emulated/0/Android/data/<游戏包名>/files/ 即便持有 MANAGE_EXTERNAL_STORAGE
 * 也会被 EACCES 拒绝（Android 16 实测）；而 /storage/emulated/0/Android/media/<游戏包名>/
 * 是应用自己的外部目录，宿主游戏天然可读，助手凭 All files access 也能跨包写入。
 * 因此主路径 = media，次路径 = Android/data（旧 adb push 通道），兜底 = 本应用 filesDir。
 */
public final class StateMirror {

    private static final String TAG = "StateMirror";
    private static final String FILE_NAME = "starry_state.txt";
    private static final int FEATURE_COUNT = 8;

    private static final int[] VALUES = new int[FEATURE_COUNT];
    private static Context context;

    /** 体检用：最近一次写失败的原因（成功时清空）。 */
    private static volatile String lastError;
    /** 体检用：最近一次写成功的路径与时间。 */
    private static volatile String lastGoodPath;
    private static volatile long lastGoodTs;

    private StateMirror() {
    }

    public static synchronized void attach(Context ctx) {
        if (context != null || ctx == null) {
            return;
        }
        Context app = ctx.getApplicationContext();
        context = app != null ? app : ctx;
        // 独立助手模式：先把目标游戏已有的状态读进来，避免覆盖式清空
        seedFrom(externalStateFile());
        // 伤害倍率滑条最小值 1：未初始化时保持 1 倍而不是 0
        if (VALUES[1] <= 0) {
            VALUES[1] = 1;
        }
    }

    /** 记录某个功能值；超出镜像范围的 featNum 直接忽略。 */
    public static synchronized void mirror(int featNum, int value) {
        if (featNum < 0 || featNum >= FEATURE_COUNT) {
            return;
        }
        VALUES[featNum] = value;
        write();
    }

    /**
     * 重新落一次当前值：用于「刚授予所有文件访问权限」这类场合 ——
     * 此时玩家并没有再动开关，但状态文件必须马上出现在游戏目录里，
     * 否则要等到玩家下一次拨开关游戏侧才生效（玩家会以为「还是没反应」）。
     */
    public static synchronized void resync() {
        write();
    }

    /**
     * 独立助手模式：目标游戏的外部状态文件；不可用时为 null。
     *
     * <p>目录来自 {@link ModTarget#EXTERNAL_DIRS}（按游戏包名拼出来的），
     * 不再硬编码字面量，换游戏只改 {@link ModTarget}。
     */
    private static File externalStateFile() {
        for (String dir : ModTarget.EXTERNAL_DIRS) {
            File d = new File(dir);
            if (!d.isDirectory()) {
                // media 目录可能尚未被游戏创建过，助手凭 All files access 直接建
                //noinspection ResultOfMethodCallIgnored
                d.mkdirs();
            }
            if (d.isDirectory()) {
                return new File(d, FILE_NAME);
            }
        }
        return null;
    }

    /** 外部 + 内部都写：外部是游戏真正读的那份，内部留一份便于排查。 */
    private static void writeTo(File dir, String body) {
        if (dir == null) {
            return;
        }
        File tmp = new File(dir, FILE_NAME + ".tmp");
        File dst = new File(dir, FILE_NAME);
        try (OutputStreamWriter writer =
                     new OutputStreamWriter(new FileOutputStream(tmp), StandardCharsets.UTF_8)) {
            writer.write(body);
        } catch (Exception e) {
            lastError = "写 " + dir + " 失败：" + e;
            Log.w(TAG, "mirror write failed " + dir + ": " + e);
            return;
        }
        if (!tmp.renameTo(dst)) {
            lastError = "改名失败 " + dst;
            Log.w(TAG, "mirror rename failed " + dir);
            return;
        }
        // 让外部目录里的文件对宿主游戏可读：FUSE 会按真实权限位判定，
        // 而宿主 App 与该文件不同 uid，默认 0660 会被拒。
        try {
            android.system.Os.chmod(dst.getAbsolutePath(), 0666);
        } catch (Throwable e) {
            Log.w(TAG, "chmod failed " + dst + ": " + e);
        }
        Log.i(TAG, "mirrored -> " + dst + " : " + body);
        lastError = null;
        lastGoodPath = dst.getAbsolutePath();
        lastGoodTs = System.currentTimeMillis();
    }

    /** 体检用：游戏真正读的那个状态文件的绝对路径；拿不到目录时为 null。 */
    public static synchronized String externalPath() {
        File f = externalStateFile();
        return f == null ? null : f.getAbsolutePath();
    }

    /** 体检用：助手内存里的开关值（玩家在菜单里拨的那组）。 */
    public static synchronized String currentState() {
        StringBuilder sb = new StringBuilder(FEATURE_COUNT * 3);
        for (int i = 0; i < FEATURE_COUNT; i++) {
            if (i > 0) {
                sb.append(' ');
            }
            sb.append(VALUES[i]);
        }
        return sb.toString();
    }

    /** 体检用：最近一次写失败原因；正常时为 null。 */
    public static String lastError() {
        return lastError;
    }

    /** 体检用：最近一次写成功的路径与时间。 */
    public static String lastGoodPath() {
        return lastGoodPath;
    }

    public static long lastGoodTs() {
        return lastGoodTs;
    }

    /** 从已有状态文件回填，避免只改一个开关就把其它功能清成 0。 */
    private static void seedFrom(File f) {
        if (f == null || !f.isFile()) {
            return;
        }
        try {
            java.io.InputStream in = new java.io.FileInputStream(f);
            byte[] buf = new byte[512];
            int n = in.read(buf);
            in.close();
            if (n <= 0) {
                return;
            }
            String text = new String(buf, 0, n, StandardCharsets.UTF_8);
            String[] tok = text.split("[ \t\r\n]+");
            for (int i = 0; i < FEATURE_COUNT && i < tok.length; i++) {
                try {
                    VALUES[i] = Integer.parseInt(tok[i].trim());
                } catch (NumberFormatException ignored) {
                }
            }
        } catch (Exception e) {
            Log.w(TAG, "seed read failed: " + e);
        }
    }

    private static void write() {
        if (context == null) {
            return;
        }
        StringBuilder sb = new StringBuilder(FEATURE_COUNT * 4);
        for (int i = 0; i < FEATURE_COUNT; i++) {
            if (i > 0) {
                sb.append(' ');
            }
            sb.append(VALUES[i]);
        }
        String body = sb.toString();
        File ext = externalStateFile();
        if (ext != null && ext.getParentFile() != null) {
            writeTo(ext.getParentFile(), body);
        }
        File internal = context.getFilesDir();
        writeTo(internal, body);
    }
}
