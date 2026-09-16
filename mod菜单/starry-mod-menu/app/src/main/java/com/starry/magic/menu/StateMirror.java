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
 */
public final class StateMirror {

    private static final String TAG = "StateMirror";
    private static final String FILE_NAME = "starry_state.txt";
    private static final int FEATURE_COUNT = 8;

    private static final int[] VALUES = new int[FEATURE_COUNT];
    private static Context context;

    private StateMirror() {
    }

    public static synchronized void attach(Context ctx) {
        if (context != null || ctx == null) {
            return;
        }
        Context app = ctx.getApplicationContext();
        context = app != null ? app : ctx;
    }

    /** 记录某个功能值；超出镜像范围的 featNum 直接忽略。 */
    public static synchronized void mirror(int featNum, int value) {
        if (featNum < 0 || featNum >= FEATURE_COUNT) {
            return;
        }
        VALUES[featNum] = value;
        write();
    }

    private static void write() {
        if (context == null) {
            return;
        }
        File dir = context.getFilesDir();
        if (dir == null) {
            return;
        }
        StringBuilder sb = new StringBuilder(FEATURE_COUNT * 4);
        for (int i = 0; i < FEATURE_COUNT; i++) {
            if (i > 0) {
                sb.append(' ');
            }
            sb.append(VALUES[i]);
        }
        File tmp = new File(dir, FILE_NAME + ".tmp");
        File dst = new File(dir, FILE_NAME);
        try (OutputStreamWriter writer =
                     new OutputStreamWriter(new FileOutputStream(tmp), StandardCharsets.UTF_8)) {
            writer.write(sb.toString());
        } catch (Exception e) {
            Log.w(TAG, "mirror write failed: " + e);
            return;
        }
        if (!tmp.renameTo(dst)) {
            Log.w(TAG, "mirror rename failed");
        }
    }
}
