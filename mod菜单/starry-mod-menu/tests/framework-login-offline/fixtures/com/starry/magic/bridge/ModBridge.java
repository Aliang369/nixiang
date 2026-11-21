package com.starry.magic.bridge;
/** 夹具：native 烘焙 game_id 的等价物（null=未加载）。 */
public final class ModBridge {
    public static String nativeGameId = null;
    public static String nativeGetAuthGameIdPublic() { return nativeGameId; }
    private ModBridge() {}
}
