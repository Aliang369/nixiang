// ============================================================
//  AceStub.cpp — ACE/TSS native 中性化桩（直装包路线核心）
//
//  用途：把 libtersafe2.so 整条替换成我们的 so（尺寸补齐到原长 5,608,784 B）。
//        官方 classes4.dex 的 com.ace.gshell.AceApplication.<clinit> 会
//        System.loadLibrary("tersafe2") → 加载的就是我们的 so；
//        其 attachBaseContextShell() 调用 native initialize(...)（返回 0 即通过）。
//        只要本文件补齐全部 Java 声明的 native，ACE 的 native 层就永远不会启动
//        → 没有完整性校验、没有 kill 链。实测（AB-13）替换后 ACE 一次都没来得及校验：
//        exit-info = reason=4 (APP CRASH(EXCEPTION)) status=0，SIGNALED_exits=0。
//
//  native 清单来源（baksmali 官方 base.apk 的 dex）：
//    classes4.dex  com/ace/gshell/AceApplication.smali      7 个
//    classes9.dex  com/tencent/tp/TssSdk.smali             11 个
//  （Bugly NativeCrashHandler 的 11 个 native 属 libBugly_Native.so，不动）
// ============================================================
#include <jni.h>
#include <android/log.h>

#define ACESTUB_LOG(...) __android_log_print(ANDROID_LOG_INFO, "Mod_Menu", __VA_ARGS__)

extern "C" void FgoBoot_StartMenu(JNIEnv* env);   // Games/FgoBoot.cpp
extern "C" void FgoBoot_EarlyInject(JNIEnv* env, jstring sourceDir, jstring filesDir);

extern "C" {

// ---------- com/ace/gshell/AceApplication (classes4.dex) ----------

JNIEXPORT jint JNICALL
Java_com_ace_gshell_AceApplication_initialize(JNIEnv* env, jobject thiz,
        jstring packageName, jstring nativeLibDir, jstring sourceDir,
        jstring filesDir, jobjectArray splitSourceDirs) {
    const char* pkg = packageName ? (*env).GetStringUTFChars(packageName, nullptr) : "(null)";
    ACESTUB_LOG("[acestub] ACE initialize() 已中和 → return 0  (pkg=%s)", pkg ? pkg : "(null)");
    if (pkg && packageName) (*env).ReleaseStringUTFChars(packageName, pkg);
    // Phase A：此刻 Application 尚未 attach，但 Manifest 里的 StarryInitProvider
    //          随时会被系统实例化 → 必须**同步**把菜单 dex 挂进 PathClassLoader
    FgoBoot_EarlyInject(env, sourceDir, filesDir);
    FgoBoot_StartMenu(env);   // Phase B：等 Application 就绪后 MenuService.start
    return 0;   // 0 = 官方认为初始化成功；非 0 会抛 "initialize failed."
}

JNIEXPORT void JNICALL
Java_com_ace_gshell_AceApplication_gp6ioctl(JNIEnv* env, jclass clazz, jstring s) { (void)env; (void)clazz; (void)s; }

JNIEXPORT void JNICALL
Java_com_ace_gshell_AceApplication_gp7ioctl(JNIEnv* env, jclass clazz, jstring s) { (void)env; (void)clazz; (void)s; }

JNIEXPORT jobject JNICALL
Java_com_ace_gshell_AceApplication_handleLoad__Ljava_lang_Object_2I(JNIEnv* env, jclass clazz, jobject o, jint i) {
    (void)env; (void)clazz; (void)o; (void)i; return nullptr;
}

JNIEXPORT jobject JNICALL
Java_com_ace_gshell_AceApplication_handleLoad__BII(JNIEnv* env, jclass clazz, jbyteArray b, jint off, jint len) {
    (void)env; (void)clazz; (void)b; (void)off; (void)len; return nullptr;
}

JNIEXPORT jlong JNICALL
Java_com_ace_gshell_AceApplication_handleLoadV22__BII(JNIEnv* env, jclass clazz, jbyteArray b, jint off, jint len) {
    (void)env; (void)clazz; (void)b; (void)off; (void)len; return 0;
}

JNIEXPORT jint JNICALL
Java_com_ace_gshell_AceApplication_ioctl(JNIEnv* env, jclass clazz, jint code, jstring s) {
    (void)env; (void)clazz; (void)code; (void)s; return 0;
}

// ---------- com/tencent/tp/TssSdk (classes9.dex) ----------

JNIEXPORT jint JNICALL
Java_com_tencent_tp_TssSdk_getsdkantidata(JNIEnv* env, jclass clazz, jobject result) {
    (void)env; (void)clazz; (void)result; return 0;
}

JNIEXPORT jint JNICALL
Java_com_tencent_tp_TssSdk_hasMatchRate(JNIEnv* env, jclass clazz, jint x) {
    (void)env; (void)clazz; (void)x; return 0;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_init(JNIEnv* env, jclass clazz, jobject info) {
    (void)env; (void)clazz; (void)info; ACESTUB_LOG("[acestub] TssSdk.init() 已中和（no-op）");
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_onruntimeinfo(JNIEnv* env, jclass clazz, jbyteArray b, jint i) {
    (void)env; (void)clazz; (void)b; (void)i;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_senddatatosdk(JNIEnv* env, jclass clazz, jbyteArray b, jint i) {
    (void)env; (void)clazz; (void)b; (void)i;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_senddatatosvr(JNIEnv* env, jclass clazz, jbyteArray b, jint i) {
    (void)env; (void)clazz; (void)b; (void)i;
}

JNIEXPORT jint JNICALL
Java_com_tencent_tp_TssSdk_setappobject(JNIEnv* env, jclass clazz, jint which, jobject obj) {
    (void)env; (void)clazz; (void)which; (void)obj; return 0;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_setgamestatus(JNIEnv* env, jclass clazz, jobject info) {
    (void)env; (void)clazz; (void)info;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_setsenddatatosvrcb(JNIEnv* env, jclass clazz, jobject cb) {
    (void)env; (void)clazz; (void)cb;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_setuserinfo(JNIEnv* env, jclass clazz, jobject info) {
    (void)env; (void)clazz; (void)info;
}

JNIEXPORT void JNICALL
Java_com_tencent_tp_TssSdk_setuserinfoex(JNIEnv* env, jclass clazz, jobject info) {
    (void)env; (void)clazz; (void)info;
}

} // extern "C"
