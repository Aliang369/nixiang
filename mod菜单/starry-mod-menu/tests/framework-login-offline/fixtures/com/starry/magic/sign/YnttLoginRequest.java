package com.starry.magic.sign;
import android.os.Parcel;
import com.taptap.sdk.LoginRequest;
/**
 * 夹具：等价于 yntt_smali 里的 Lcom/starry/magic/sign/YnttLoginRequest;
 * （super=LoginRequest，ctor=(String,String[])，getRequestCode 抛异常）。
 * 与真 smali 的一致性由 OfflineLoginTest 的 smali 断言单独校验，两处不一致就红。
 */
public class YnttLoginRequest extends LoginRequest {
    public YnttLoginRequest(String state, String[] permissions) { super(state, permissions); }
    public YnttLoginRequest(Parcel p) { super(p); }
    @Override public int getRequestCode() {
        throw new IllegalStateException("starry_force_web_login");
    }
}
