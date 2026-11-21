package com.taptap.sdk.login.internal.handlers.phone;
import com.taptap.sdk.LoginRequest;
import com.taptap.sdk.login.internal.handlers.LoginHandler;
/** 夹具：手机号快登，改签包同样会失败，必须一并失效。 */
public class PhoneLoginHandler implements LoginHandler {
    @Override public boolean canHandle(LoginRequest request) { return true; }
    @Override public String name() { return "phone"; }
}
