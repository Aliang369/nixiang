package com.taptap.sdk.login.ui;
import android.app.Activity;
import com.taptap.sdk.LoginRequest;
import com.taptap.sdk.login.impl.LoginActivityImpl;
/**
 * 夹具：TapSDK 4.7.x 那一代 com.taptap.sdk.login.ui.TapTapLoginActivity
 * （loginHandlers 数组路由 + impl.fallbackToWeb）。
 * 它是框架 sanitize / forceOldImplWeb 的守卫目标，与 3.30.2 的
 * com.taptap.sdk.ui.TapTapActivity（request 注入路）不是同一个类，夹具不可混用。
 */
public class TapTapLoginActivity extends Activity {
    public LoginActivityImpl impl = new LoginActivityImpl();
    public LoginRequest loginRequest;
}
