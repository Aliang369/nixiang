package android.content.pm;
public abstract class PackageManager {
    public static final int GET_SIGNATURES = 0x00000040;
    public static final int GET_SIGNING_CERTIFICATES = 0x08000000;
    public PackageInfo getPackageInfo(String packageName, int flags) throws NameNotFoundException { return null; }
    public static class NameNotFoundException extends Exception { public NameNotFoundException(){} }
}