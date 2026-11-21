package com.taptap.sdk.login.impl;
import android.app.Activity;
import com.taptap.sdk.LoginRequest;
/** 夹具：旧版 TapTapActivity 内部的 impl（loginHandlers 数组 + fallbackToWeb）。 */
public class LoginActivityImpl {
    public Object[] loginHandlers = new Object[0];
    public LoginRequest fallbackArg;
    public int fallbackCalls;
    public void fallbackToWeb(Activity activity, LoginRequest request) {
        fallbackCalls++;
        fallbackArg = request;
    }
}
