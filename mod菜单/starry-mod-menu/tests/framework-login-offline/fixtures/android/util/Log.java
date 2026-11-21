package android.util;
import java.util.ArrayList;
import java.util.List;
/** 夹具：把日志留在内存里供断言，不写 stdout 噪声。 */
public final class Log {
    public static final List<String> LINES = new ArrayList<>();
    public static int i(String t, String m) { LINES.add("I/" + t + ": " + m); return 0; }
    public static int w(String t, String m) { LINES.add("W/" + t + ": " + m); return 0; }
    public static int e(String t, String m) { LINES.add("E/" + t + ": " + m); return 0; }
    public static int e(String t, String m, Throwable th) { LINES.add("E/" + t + ": " + m); return 0; }
    public static int w(String t, String m, Throwable th) { LINES.add("W/" + t + ": " + m); return 0; }
    public static int d(String t, String m) { return 0; }
    private Log() {}
}
