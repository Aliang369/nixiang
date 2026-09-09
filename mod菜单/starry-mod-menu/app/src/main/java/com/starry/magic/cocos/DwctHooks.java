package com.starry.magic.cocos;

/**
 * 动物餐厅开关标志（供 smali 钩子 + native 同步）。
 */
public final class DwctHooks {
    private static volatile boolean noAd = false;
    private static volatile boolean noIap = false;

    private DwctHooks() {}

    public static void setNoAd(boolean v) { noAd = v; }
    public static void setNoIap(boolean v) { noIap = v; }
    public static boolean shouldSkipAd() { return noAd; }
    public static boolean shouldSkipIap() { return noIap; }

    /** wxPay 被拦截时：通知 JS 支付成功回调（走 wechatPayCallback） */
    public static void fakeWxPaySuccess() {
        CocosJsBridge.evalOnGLThread(
                "try{if(typeof wechatPlatformManager!==\"undefined\"&&wechatPlatformManager.wechatPay){wechatPlatformManager.wechatPay(\"true\");}"
                        + "else if(window.wechatPlatformManager&&window.wechatPlatformManager.wechatPay){window.wechatPlatformManager.wechatPay(\"true\");}}catch(e){}"
        );
    }
}
