package com.taptap.sdk.login.internal.handlers.client;
import com.taptap.sdk.LoginRequest;
import com.taptap.sdk.login.internal.handlers.LoginHandler;
/** 夹具：装了 TapTap 客户端时 canHandle 恒 true —— 改签包必须让它失效。 */
public class ClientLoginHandler implements LoginHandler {
    @Override public boolean canHandle(LoginRequest request) { return true; }
    @Override public String name() { return "client"; }
}
