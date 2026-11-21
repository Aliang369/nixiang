package android.content;
public class Context {
    private String pkg = "com.fixture.host";
    public String getPackageName() { return pkg; }
    public void setPackageNameForTest(String p) { this.pkg = p; }
    public ClassLoader getClassLoader() { return getClass().getClassLoader(); }
}
