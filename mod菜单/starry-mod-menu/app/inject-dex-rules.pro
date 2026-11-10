# =============================================================================
# 注入 dex 混淆通道（2026-11-10 建立）
#
# 背景：注入进游戏包的 dex 有两条产出路径 ——
#   ① `apktool d` → 拷 `com/starry/**` smali → `apktool b`（57 个存量游戏走这条）
#   ② `javac + d8` 裸编译直接产出 classesN.dex 塞进 zip（农场守护者等走这条）
# 两条路径**都不经过 R8**（app/build.gradle 的 minifyEnabled 只作用于 release APK，
# 而注入链根本不跑 gradle），所以注入 dex 一直是**明文**：
#   用 strings 就能读出 /auth/grant、https://<IP>、AuthClient、DeviceFingerprint、
#   NativeMenuController、GRANT_RENEW_* 等 —— 等于把卡密协议的说明书交给破解者。
#
# 本文件给两条路径共用一份 `-keep` 规则，交给 R8 混淆。
#
# ★ 必须 keep 的只有「外部按名字找进来」的类，四类：
#   1. AndroidManifest 里的组件（ClassNotFound 直接闪退）
#   2. native FindClass / RegisterNatives 目标（so 里写死的类名）
#   3. 注入脚本写进**游戏** AndroidManifest 的组件与 Application/Provider/Factory
#   4. 反射 / 序列化（org.json 反射 setter、Bundle key 对应的 bean）
# 其余（业务实现、网络、界面细节）全部可混淆。
#
# ★ JNI 方法名本身**不改**（`-keepclasseswithmembernames class * { native <methods>; }`
#   保证 native 方法名对齐 RegisterNatives 的字符串表）。混淆的收益是**类名与人可读
#   的业务字符串**，不是 JNI 方法名 —— 后者一旦改了，so 里的 string 表也得同步改，
#   收益远小于风险。
# =============================================================================

# ---- 1. 组件：Manifest 按全名实例化，混淆即崩 ----
-keep class com.starry.magic.app.MainActivity { *; }
-keep class com.starry.magic.menu.MenuService { *; }
-keep class com.starry.magic.menu.net.CdnVpnService { *; }
-keep class com.starry.magic.StarryInitProvider { *; }
-keep class com.starry.magic.boot.StarryInitProvider { *; }
-keep class com.starry.magic.StarryApp { *; }
-keep class com.starry.magic.StarryAppCompFactory0 { *; }

# 注入脚本写进游戏 Manifest 的 Application / ContentProvider / AppComponentFactory
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
-keep class com.starry.magic.sign.SignatureKillerBootstrap { *; }
# 11 个 build_dex 脚本的字节自检都断言本类按原名存在（`b'com/starry/magic/ota/StarryNativeLoad'`），
# 且它是 native 加载入口被 StarryApp / StarryInitProvider 早期调用 —— 改名后自检失败即挡发布。
-keep class com.starry.magic.ota.StarryNativeLoad { *; }
-keep class com.starry.magic.ltzj.DexInstaller { *; }
-keep class com.starry.magic.ltzj.NativeLinker { *; }
-keep class com.starry.magic.wgdzz.DexInstaller { *; }
-keep class dev.mars.secure.ProxyApplication { *; }

# ---- 2. native 按类名找进来（so 里 OBFUSCATE 写死，不能改） ----
-keep class com.starry.magic.bridge.ModBridge { *; }

# ---- 3. 反射 / 序列化 / JS 桥 ----
-keepattributes JavascriptInterface
-keepattributes *Annotation*,Signature,InnerClasses,EnclosingMethod
-keepclassmembers class * {
    @android.webkit.JavascriptInterface <methods>;
}
# org.json 反射写 bean（admin/portal 相关响应解析）
-keepclassmembers class * {
    public void set*(...);
}

# ---- 4. JNI 方法名对齐（只保成员名，不保其余部分）----
-keepclasseswithmembernames class * {
    native <methods>;
}

# ---- 5. 不警告三方可缺依赖 ----
-dontwarn javax.annotation.**
-dontwarn org.slf4j.**
-dontwarn kotlin.**
-dontwarn com.google.**
-dontwarn okhttp3.**
-dontwarn okio.**

# ---- 6. 构建期生成物（R 类由 aapt2/gradle 产出，离线跑 R8 时不存在）----
#      注入链用 javac+d8 离线编译，R 由构建脚本补桩；R8 只要不因缺类报错即可。
-dontwarn com.starry.magic.R
-dontwarn com.starry.magic.R$*
-dontwarn com.starry.magic.BuildConfig

# ---- 7. ★ 关键：禁止重打包到平级单字母包 ----
# 注入链有 57 个脚本用 `apktool d` 后 `cp -R <smali>/com/starry` 只拷 **com/starry 这一棵子树**。
# R8 默认会把类重打包到 `k/`、`d/`、`c/` 这类顶层短包 ⇒ 拷不进游戏包 ⇒ 运行时 ClassNotFound。
# 收窄重打包：强制被混淆的类仍落在 com.starry.magic.** 之下，只混淆最后一段类名。
# 代价＝包路径仍暴露 "starry/magic"，但那是包名（已知），收益仍成立：
# 业务类名、方法名、字段名、字符串常量该混淆的照混淆。
-repackageclasses 'com.starry.magic.o'

# ---- 8. 只混淆，不裁剪 ----
# 注入包里有大量「按名/反射/字符串"找进来」的调用（JS 桥、prefs key、
# 部分 H5/Cocos 动态入口），静态可达性分析看不到这些引用。
# R8 默认会 shrink ⇒ 静默删掉"看起来没人用"的类 ⇒ 进游戏才 ClassNotFound。
# 注入 dex 体积本来就只有几百 KB，省这点不如保命。
-dontshrink
-dontoptimize
