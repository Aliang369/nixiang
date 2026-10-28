package com.starry.magic.sign;

import android.content.Context;
import android.util.Log;

import java.io.File;
import java.io.FileOutputStream;

/**
 * TapTap/TapSDK 类「签名校验」过检引导（MT ApkSignatureKillerEx 形态）。
 *
 * 背景（2026-10-29 双轨制，见 AGENTS §3 路由与
 * 10_docs/_templates/经验_TapTap改包法_验签弹窗与网页登录_20261028.md「2026-10-29 增补」）：
 *   签名真伪属「结果类校验」⇒ 走 B 运行时替换；登录/入口流程 ⇒ 走 A 改包法。
 *   本类只做 B：触发包内 `bin.mt.signature.KillerApplication193` 的静态初始化，
 *   由它包装 `PackageInfo.CREATOR`（内部用 Unsafe 绕过 final）+ 清 Parcel/PackageManager 缓存，
 *   并对目标包名回灌官方证书 DER ⇒ 之后任何走 PackageManager 的签名查询都拿到官方值。
 *
 * 为什么不用 SignSpoof：`SignSpoof`/`SignSpoofAppComponentFactory` 已于 2026-10-28 按用户指令
 * 整体删除且**禁止复活**（依赖隐藏 API 豁免，Android 14 user 版必失败）。
 * 本类不碰 Application、不做隐藏 API 豁免，只在 Provider 早期触发一次 Killer 的 `<clinit>`。
 *
 * 安全特性：包内没有 killer dex 的游戏（绝大多数）走 catch 分支，只打一行日志，零副作用。
 */
public final class SignatureKillerBootstrap {
    private static final String TAG = "StarrySigKiller";
    private static final String KILLER_CLASS = "bin.mt.signature.KillerApplication193";
    private static volatile boolean sDone;

    private SignatureKillerBootstrap() {}

    /** 幂等；失败不抛（没有 killer dex 是正常情况）。 */
    public static void install(Context ctx) {
        if (sDone) return;
        sDone = true;
        String result;
        try {
            Class.forName(KILLER_CLASS);
            result = "ok " + KILLER_CLASS + " t=" + System.currentTimeMillis();
            Log.i(TAG, "signature killer installed (" + KILLER_CLASS + ")");
        } catch (Throwable t) {
            result = "fail: " + t;
            Log.i(TAG, "no signature killer dex (ignored): " + t);
        }
        mark(ctx, result);
    }

    /** 取证标记：killer 是否装载（无日志缓冲时用它判定）。 */
    private static void mark(Context ctx, String text) {
        if (ctx == null) return;
        FileOutputStream fos = null;
        try {
            File f = new File(ctx.getFilesDir(), "sigkiller.txt");
            fos = new FileOutputStream(f, false);
            fos.write((text + "\n").getBytes("UTF-8"));
            fos.flush();
        } catch (Throwable ignored) {
        } finally {
            if (fos != null) try { fos.close(); } catch (Throwable ignored) {}
        }
    }
}
