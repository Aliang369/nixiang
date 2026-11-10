package com.starry.magic.ltzj;

import android.util.Log;

/**
 * 旧名 {@code com.starry.magic.ltzj.NativeLinker} —— <b>已迁移到
 * {@link com.starry.magic.core.NativeLinker}</b>，本类只作存量兼容。
 *
 * <p>为什么保留：已交付的包（雷霆战机等）里有 so 侧写死的 OBFUSCATE 类名字符串，
 * 以及若干反射调用点，删除会导致旧包 {@code No implementation found}。新代码一律用
 * {@code com.starry.magic.core.NativeLinker}；框架内的反射已改为双候选（先 core，再本类）。
 *
 * <p>删除条件：所有引用 {@code com.starry.magic.ltzj.NativeLinker} 的存量包下线后。
 */
public final class NativeLinker {

    private static final String TAG = "LtzjLinker";

    private NativeLinker() {}

    /** @deprecated 用 {@link com.starry.magic.core.NativeLinker#load(String)} */
    @Deprecated
    public static void load(String absolutePath) {
        com.starry.magic.core.NativeLinker.load(absolutePath);
    }
}
