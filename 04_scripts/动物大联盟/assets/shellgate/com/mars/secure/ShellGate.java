package dev.mars.secure;

/**
 * 无壳包补丁类（本游戏私有）：替代官方加固壳的 dev.mars.secure.ShellGate。
 *
 * 官方原实现（02_apks/动物大联盟/动物大联盟_1.0.25_official.apk 内 classes.dex）：
 *   .class public final Ldev/mars/secure/ShellGate;
 *   .method static constructor <clinit>()V  -> System.loadLibrary("secret")
 *   .method public static native require()V     <-- native，实现在 libsecret.so
 *   .method public static native apiTable()J
 *
 * SL6 口径（定性排除实验，本轮）：
 *   - <clinit> 里 System.loadLibrary("secret") 不加任何 try/catch，让失败自然抛给调用方；
 *   - require() / apiTable() 与官方一致声明为 native，不写 Java 方法体。
 * 目的：让 libsecret.so 的 JNI_OnLoad 真正注册成功，判定「数据检查」是否依赖壳。
 */
public final class ShellGate {

    static {
        System.loadLibrary("secret");
    }

    private ShellGate() {
    }

    public static native void require();

    public static native long apiTable();
}
