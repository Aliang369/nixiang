package com.taptap.sdk.login.impl.ui.activity;
import android.app.Activity;
import com.taptap.sdk.LoginRequest;
/** 夹具：新版 TapTapLoginActivity（intent request + provider 分流 + fallbackToWeb）。 */
public class TapTapLoginActivity extends Activity {
    public LoginRequest request;
    public int fallbackCalls;
    public LoginRequest fallbackArg;
    public void fallbackToWeb(LoginRequest r) {
        fallbackCalls++;
        fallbackArg = r;
    }
}
