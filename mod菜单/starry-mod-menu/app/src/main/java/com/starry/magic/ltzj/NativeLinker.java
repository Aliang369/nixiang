package com.starry.magic.ltzj;

/**
 * 仅通过 PathClassLoader 加载本类后调用 System.load，
 * 使 so 绑定到应用 PathCL（避免 DexInstaller 所在 DexCL 抢先绑定）。
 */
public final class NativeLinker {
    private NativeLinker() {}

    public static void load(String absolutePath) {
        System.load(absolutePath);
    }
}
