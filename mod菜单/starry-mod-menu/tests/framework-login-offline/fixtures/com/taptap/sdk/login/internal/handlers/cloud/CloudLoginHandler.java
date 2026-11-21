package com.taptap.sdk.login.internal.handlers.cloud;
import com.taptap.sdk.LoginRequest;
import com.taptap.sdk.login.internal.handlers.LoginHandler;
public class CloudLoginHandler implements LoginHandler {
    @Override public boolean canHandle(LoginRequest request) { return false; }
    @Override public String name() { return "cloud"; }
}
