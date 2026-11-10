package com.starry.magic.core;

import android.content.Context;
import android.util.Log;

import java.io.File;
import java.lang.reflect.Method;

/**
 * 框架公共的 so 加载/落盘路径解析 —— 取代散落在共用类里的
 * {@code /data/local/tmp/ltzj_st/...} 硬编码。
 *
 * <p>历史问题（2026-11-11 收口）：{@code ModBridge} / {@code MenuService} /
 * {@code ltzj.DexInstaller} 三处直接写死 {@code /data/local/tmp/ltzj_st/libstarry_magic.so}。
 * 这个目录是雷霆战机当年免 root 落盘的产物，却被所有游戏共用 —— 属性归错包，
 * 且一旦 ltzj 包被裁掉，其它游戏的备选路径就指向一个不存在的目录。
 *
 * <p>现在的口径：
 * <ul>
 *   <li><b>首选</b> {@link #appPrivateDir(Context)}：{@code filesDir/starry_native}，私有、免权限。</li>
 *   <li><b>次选</b> {@link #TMP_LIBS}：{@code /data/local/tmp/starry_libs}，免 root 助手形态落盘处。</li>
 *   <li><b>兼容</b> {@link #LEGACY_TMP_LIBS}：旧的 {@code ltzj_st}，只读不写，存量包还在用。</li>
 * </ul>
 */
public final class NativeLoader {

    private static final String TAG = "NativeLoader";

    /** 通用免 root 落盘目录（新游戏用这个）。 */
    public static final String TMP_LIBS = "/data/local/tmp/starry_libs";

    /** 旧目录：只作兼容读取，禁止新代码写入。 */
    public static final String LEGACY_TMP_LIBS = "/data/local/tmp/ltzj_st";

    private NativeLoader() {}

    /** {@code filesDir/starry_native}，应用私有目录；ctx 为空时返回 null。 */
    public static File appPrivateDir(Context ctx) {
        if (ctx == null) return null;
        try {
            return new File(ctx.getFilesDir(), "starry_native");
        } catch (Throwable t) {
            return null;
        }
    }

    /**
     * 把一个 so 的候选路径拼成数组（顺序 = 优先级），已去重、只保留存在的文件。
     * <p>返回可能为空数组，调用方自行决定兜底（通常是 {@code System.loadLibrary}）。
     *
     * @param ctx      上下文，可为 null（纯 tmp 形态）
     * @param fileName 文件名，如 {@code libstarry_magic.so}
     */
    public static String[] candidates(Context ctx, String fileName) {
        java.util.LinkedHashSet<String> set = new java.util.LinkedHashSet<>();
        File priv = appPrivateDir(ctx);
        if (priv != null) set.add(new File(priv, fileName).getAbsolutePath());
        set.add(TMP_LIBS + "/" + fileName);
        set.add(LEGACY_TMP_LIBS + "/" + fileName);
        java.util.ArrayList<String> out = new java.util.ArrayList<>();
        for (String p : set) {
            if (new File(p).isFile()) out.add(p);
        }
        return out.toArray(new String[0]);
    }

    /**
     * 通过显式 ClassLoader 调 {@code System.load}（把绑定目标钉到 PathCL）。
     * <p>优先 {@link NativeLinker}；被裁掉时退回反射自身 {@code System.load}。
     */
    public static boolean loadInto(ClassLoader cl, String absolutePath) {
        if (absolutePath == null || absolutePath.isEmpty()) return false;
        try {
            Class<?> linker = Class.forName(
                    "com.starry.magic.core.NativeLinker", true,
                    cl != null ? cl : NativeLoader.class.getClassLoader());
            Method m = linker.getMethod("load", String.class);
            m.invoke(null, absolutePath);
            return true;
        } catch (Throwable t) {
            Throwable c = t.getCause() != null ? t.getCause() : t;
            String msg = c.getMessage() != null ? c.getMessage() : String.valueOf(c);
            // so 已被同一 CL 打开：等价于成功
            if (msg.contains("already opened") && msg.contains("PathClassLoader")) return true;
            Log.w(TAG, "System.load failed: " + absolutePath + " → " + msg);
            return false;
        }
    }
}
