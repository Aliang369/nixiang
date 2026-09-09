# =============================================================================
# 注入场景：apktool 只拷贝 com/starry/** smali。
# R8 若混淆/重打包到 a.b、d.b，不会进游戏包 → ClassNotFound 闪退。
# 策略：允许 shrink 删未用代码，但禁止混淆与改包名。
# =============================================================================

-dontobfuscate

-keepattributes JavascriptInterface
-keepattributes *Annotation*
-dontwarn javax.annotation.**

-keep class com.starry.magic.** { *; }

-keepclassmembers class * {
    @android.webkit.JavascriptInterface <methods>;
}
-keepclasseswithmembernames class * {
    native <methods>;
}

-keep class com.starry.magic.ltzj.NativeLinker { public static void load(java.lang.String); }
-keep class com.starry.magic.ltzj.DexInstaller { *; }
