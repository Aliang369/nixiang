package com.starry.magic.core;

/**
 * 通过显式 ClassLoader 调 {@code System.load} —— 框架公共设施。
 *
 * <p>为什么需要它：{@code System.load} 会把 so 绑到<b>调用方</b>的 ClassLoader 上。
 * 注入链里常有「先由 DexClassLoader 加载一个 installer，再由它拉起真正的 MenuService」
 * 的场景，直接在 installer 里调 {@code System.load} 会让 so 绑到 DexCL，
 * 随后 PathCL 里的 {@code ModBridge} / {@code MenuService} 就会失败
 * （{@code already opened by ClassLoader} / {@code No implementation found}）。
 * 用一个只由 PathCL 加载的小类做转接，就能把绑定目标钉死。
 *
 * <p>历史：本类原叫 {@code com.starry.magic.ltzj.NativeLinker}（雷霆战机的包里孵化出来的）。
 * 它在框架层被 3 处反射调用，但归属放在单游戏包下 ⇒ 单游戏包被裁掉时框架就断了。
 * 2026-11-11 迁到 {@code core}。旧类保留（存量包仍引用），新代码一律用本类。
 */
public final class NativeLinker {

    private NativeLinker() {}

    /** 在调用方 ClassLoader 上加载指定绝对路径的 so。 */
    public static void load(String absolutePath) {
        System.load(absolutePath);
    }
}
