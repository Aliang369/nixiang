package android.app;
import android.content.Intent;
/** 夹具：可注入 intent / 包名 / FragmentManager。 */
public class Activity extends android.content.Context {
    public Intent intent;
    public FragmentManager fm = new FragmentManager();
    public Intent getIntent() { return intent; }
    public void setIntent(Intent i) { this.intent = i; }
    public FragmentManager getFragmentManager() { return fm; }
}
