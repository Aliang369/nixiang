package com.taptap.sdk;
import android.os.Parcel;
import android.os.Parcelable;
/**
 * 夹具：TapSDK LoginRequest 的最小等价物。
 * 字段/方法名与真 SDK 一致（TapTapWebLogin 全靠反射按名取值，名字错了测试就会红）。
 */
public class LoginRequest implements Parcelable {
    public static final String MOBILE_CLIENT = "mobile_client";
    public static final String EMBEDDED_WEB = "embedded_web";
    public String loginType = MOBILE_CLIENT;
    private String state;
    private String[] permissions;
    private String codeChallenge;
    private String codeChallengeMethod;
    private String info;
    private String loginVersion;
    private String phoneVerifyToken;
    private String preferredLoginType;
    private String redirectUri;
    private String responseType;
    private String versionCode;
    private boolean preApproved;

    public LoginRequest(String state, String[] permissions) {
        this.state = state;
        this.permissions = permissions;
    }
    public LoginRequest(Parcel p) {}

    public String getState() { return state; }
    public String[] getPermissions() { return permissions; }
    public String getCodeChallenge() { return codeChallenge; }
    public void setCodeChallenge(String v) { codeChallenge = v; }
    public String getCodeChallengeMethod() { return codeChallengeMethod; }
    public void setCodeChallengeMethod(String v) { codeChallengeMethod = v; }
    public String getInfo() { return info; }
    public void setInfo(String v) { info = v; }
    public String getLoginVersion() { return loginVersion; }
    public void setLoginVersion(String v) { loginVersion = v; }
    public String getPhoneVerifyToken() { return phoneVerifyToken; }
    public void setPhoneVerifyToken(String v) { phoneVerifyToken = v; }
    public String getPreferredLoginType() { return preferredLoginType; }
    public void setPreferredLoginType(String v) { preferredLoginType = v; }
    public String getRedirectUri() { return redirectUri; }
    public void setRedirectUri(String v) { redirectUri = v; }
    public String getResponseType() { return responseType; }
    public void setResponseType(String v) { responseType = v; }
    public String getVersionCode() { return versionCode; }
    public void setVersionCode(String v) { versionCode = v; }
    public boolean isPreApproved() { return preApproved; }
    public void setPreApproved(boolean v) { preApproved = v; }

    /** 真 SDK 里这个方法是登录路由的关键；基类不抛，子类（本游戏 smali）抛。 */
    public int getRequestCode() { return 0; }

    @Override public int describeContents() { return 0; }
    @Override public void writeToParcel(Parcel dest, int flags) {}
}
