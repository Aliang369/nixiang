# =============================================================================
#  星光菜单 APK 构建规则（2026-11-10 重写：从「禁止混淆」改成「定向混淆」）
#
#  ── 为什么改 ──
#  旧规则第一行是 `-dontobfuscate`，理由写着「apktool 只拷 com/starry/** smali，
#  R8 若混淆到 a.b 就不进包」。实测结论：**这个担心只对"重打包到平级包"成立，
#  对"改类名"不成立** —— 只要加 `-repackageclasses 'com.starry.magic.o'`，
#  被混淆的类仍留在 com.starry.magic 这棵子树里，`cp -R com/starry` 照样拷得到。
#  真正的硬约束只有一条：**外部按名字找进来的类不能被改名**（Manifest / native
#  FindClass / 注入脚本写进游戏 Manifest 的那些）。
#
#  ── 实测收益（132 个真实类，2026-11-10）──
#  466,588 B → 352,468 B（-24.5%）
#  120 个业务类 → com.starry.magic/o/A…（AuthClient / DeviceFingerprint /
#  NativeMenuController 等语义名全部消失）
#
#  ── 红线：下面这些必须 `-keep` 且**不带** allowobfuscation ──
#  ① AndroidManifest 组件（ClassNotFound 即闪退）
#  ② native FindClass/RegisterNatives 目标（so 里 OBFUSCATE 写死了类名）
#  ③ 注入脚本写进**游戏** Manifest 的 Application/Provider/AppComponentFactory
#  ④ JNI 方法名（与 native 的 RegisterNatives 字符串表对齐）
# =============================================================================

# ---- ① 本项目 Manifest 组件 ----
-keep class com.starry.magic.app.MainActivity { *; }
-keep class com.starry.magic.menu.MenuService { *; }
-keep class com.starry.magic.menu.net.CdnVpnService { *; }
-keep class com.starry.magic.StarryInitProvider { *; }
-keep class com.starry.magic.boot.StarryInitProvider { *; }
-keep class com.starry.magic.StarryApp { *; }
-keep class com.starry.magic.StarryAppCompFactory0 { *; }

# ---- ③ 注入脚本写进游戏 Manifest 的组件（按全名实例化）----
-keep class com.starry.magic.sign.StarryAppComponentFactory { *; }
-keep class com.starry.magic.sign.TapLoginForceWeb { *; }
-keep class com.starry.magic.sign.SignatureKiller { *; }
-keep class com.starry.magic.h5box.StarryH5Provider { *; }
-keep class com.starry.magic.h5box.CzbhzH5Provider { *; }
-keep class com.starry.magic.conch.DcslConchProvider { *; }
-keep class com.starry.magic.dwdlm.DwdlmAppComponentFactory { *; }
-keep class com.starry.magic.dwdlm.DwdlmApplication { *; }
-keep class com.starry.magic.dwdlm.DwdlmKillerFactory { *; }
-keep class com.starry.magic.dwdlm.DwdlmSignBridge { *; }
-keep class com.starry.magic.lvmeng.LvmengPayloadProvider { *; }
-keep class com.starry.magic.ltzj.DexInstaller { *; }
-keep class com.starry.magic.ltzj.NativeLinker { *; }
-keep class com.starry.magic.wgdzz.DexInstaller { *; }
-keep class dev.mars.secure.ProxyApplication { *; }

# ---- ② native 按类名找进来（so 里 OBFUSCATE 写死）----
-keep class com.starry.magic.bridge.ModBridge { *; }

# ---- ③ 构建脚本按名引用：刷新链要往 BuildConfig 注入 MENU_STAMP（starry-menu:<commit>）
#       被 allowobfuscation+repackageclasses 改名成 com.starry.magic.o.a 后，注入脚本按名找不到 ⇒ 出包失败
#       （2026-11-10 实测）
-keep class com.starry.magic.BuildConfig { *; }

# ---- 其余全部：保留（防 shrink 掉反射用到的），但**允许改名** ----
-keep,allowobfuscation class com.starry.magic.** { *; }

# ---- ★ 不重打包到平级包：57 个注入脚本用 `cp -R com/starry` 只拷这棵子树 ----
-repackageclasses 'com.starry.magic.o'

# ---- ④ JNI 方法名与 JS 桥 ----
-keepattributes JavascriptInterface
-keepattributes *Annotation*,Signature,InnerClasses,EnclosingMethod
-keepclassmembers class * {
    @android.webkit.JavascriptInterface <methods>;
}
-keepclasseswithmembernames class * {
    native <methods>;
}

# ---- 构建期生成物（R / BuildConfig 由 aapt2 产出）----
-dontwarn com.starry.magic.R
-dontwarn com.starry.magic.R$*
-dontwarn com.starry.magic.BuildConfig
-dontwarn javax.annotation.**
