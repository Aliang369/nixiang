package android.content;
import android.os.Parcelable;
import java.util.LinkedHashMap;
import java.util.Map;
/** 夹具：最小 intent extra 容器（只实现框架用到的两个方法）。 */
public class Intent {
    public final Map<String, Object> extras = new LinkedHashMap<>();
    public void putExtra(String k, Parcelable v) { extras.put(k, v); }
    public <T extends Parcelable> T getParcelableExtra(String k) {
        Object o = extras.get(k);
        @SuppressWarnings("unchecked") T t = (T) o;
        return t;
    }
}
