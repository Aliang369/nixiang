package android.os;
/** 夹具：SDK_INT 可写（真 android.jar 里恒 0，测不了 >=29 分支）。 */
public final class Build {
    public static final class VERSION { public static int SDK_INT = 29; }
    private Build() {}
}
