package com.starry.magic.ota;

import android.content.Context;
import android.util.Log;

import com.starry.magic.menu.Preferences;

import java.io.File;

/**
 * 本地玩法 OTA 目录：{@code files/starry_ota/{game_id}/}
 * 含 manifest.json / patch.json / libstarry_magic.so（有则覆盖启动加载）。
 */
public final class PlayOtaPaths {
    private static final String TAG = "PlayOta";
    public static final String DIR_NAME = "starry_ota";

    private PlayOtaPaths() {}

    public static File root(Context ctx) {
        return new File(ctx.getFilesDir(), DIR_NAME);
    }

    public static File gameDir(Context ctx, String gameId) {
        String gid = gameId == null ? "" : gameId.trim();
        return new File(root(ctx), gid);
    }

    /** 已落盘的 OTA so；无则 null。优先 prefs 的 game_id，否则扫唯一子目录。 */
    public static File resolveOtaSo(Context ctx) {
        if (ctx == null) return null;
        try {
            Preferences.with(ctx);
            String gid = Preferences.getAuthGameId();
            if (gid != null && !gid.isEmpty()) {
                File so = new File(gameDir(ctx, gid), "libstarry_magic.so");
                if (so.isFile()) return so;
            }
        } catch (Throwable t) {
            // attachBaseContext 等极早路径 prefs 可能未就绪；退回目录扫描
            Log.w(TAG, "resolveOtaSo prefs skip: " + t.getMessage());
        }
        File root = root(ctx);
        if (!root.isDirectory()) return null;
        File[] kids = root.listFiles();
        if (kids == null) return null;
        File best = null;
        long bestM = -1;
        for (File d : kids) {
            if (!d.isDirectory()) continue;
            File so = new File(d, "libstarry_magic.so");
            if (!so.isFile()) continue;
            long m = so.lastModified();
            if (m >= bestM) {
                bestM = m;
                best = so;
            }
        }
        if (best != null) {
            Log.i(TAG, "resolveOtaSo: " + best.getAbsolutePath());
        }
        return best;
    }

    public static File patchJson(Context ctx, String gameId) {
        return new File(gameDir(ctx, gameId), "patch.json");
    }

    public static File manifest(Context ctx, String gameId) {
        return new File(gameDir(ctx, gameId), "manifest.json");
    }
}
