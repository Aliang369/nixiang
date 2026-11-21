package com.taptap.sdk.ui;
import android.app.Activity;
import com.taptap.sdk.LoginRequest;
import com.taptap.sdk.login.impl.LoginActivityImpl;
/** 夹具：TapSDK 3.30.2 旧版 TapTapActivity（onCreate 内同步 sendLoginRequest）。 */
public class TapTapActivity extends Activity {
    public LoginActivityImpl impl = new LoginActivityImpl();
    public LoginRequest loginRequest;
}
