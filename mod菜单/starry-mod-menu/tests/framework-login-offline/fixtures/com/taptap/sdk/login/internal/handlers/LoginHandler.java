package com.taptap.sdk.login.internal.handlers;
public interface LoginHandler {
    boolean canHandle(com.taptap.sdk.LoginRequest request);
    String name();
}
