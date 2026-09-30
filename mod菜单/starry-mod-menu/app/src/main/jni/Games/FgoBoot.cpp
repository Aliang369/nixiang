// ============================================================
//  FgoBoot.cpp — 官方包内（payload 形态）菜单引导
//
//  背景：FGO 2.129.0 官方包禁新增 dex/so 条目（AB-8/9 实测 KILLED），
//        放行的写入面是 assets/（AB-7 ALIVE）与 AndroidManifest.xml
//        （AB-4 实测不是 ACE kill，而是 Java 异常 ⇒ 清单未被摘要校验）。
//
//  两段式：
//    A) 早期同步注入（AceApplication.attachBaseContext → initialize()）：
//       此时 Application 还没 attach，但 Manifest 里声明的 ContentProvider
//       （com.starry.magic.StarryInitProvider）随时会被系统实例化，
//       所以必须**同步**把菜单 dex 挂进 SystemClassLoader(=PathClassLoader)，
//       否则 CNFE 直接崩 Application（AB-4 就是这样死的）。
//       不依赖 Context：sourceDir/filesDir 由 initialize() 传入，
//       用 java.util.zip.ZipFile 直接读 APK 里的 assets/starry_dex/starry.dex。
//    B) 菜单启动（后台线程，等 ActivityThread.currentApplication() 就绪）：
//       补注册 bridge native（JNI_OnLoad 时 ModBridge 还不存在）→
//       MenuService.start(app)。Manifest 里的 StarryInitProvider 通常
//       也会自己调 start，两边都做了幂等保护。
// ============================================================
#include <jni.h>
#include <pthread.h>
#include <unistd.h>
#include <cstdio>
#include <cstring>
#include <string>
#include <dlfcn.h>
#include <link.h>
#include <android/log.h>

#define FGOBOOT_LOG(...) __android_log_print(ANDROID_LOG_INFO, "Mod_Menu", __VA_ARGS__)

static JavaVM* g_fgo_vm = nullptr;

// FgoBoot_InjectEarlyFromOnLoad 用（定义在文件后段）
extern "C" void FgoBoot_InjectEarlyFromOnLoad(JNIEnv* env);
static bool g_early_injected = false;

extern "C" void Starry_TryRegisterBridge(JNIEnv* env);   // MainModern.cpp 导出
extern "C" void CrashTrap_Start(void);                   // Games/CrashTrap.cpp 导出（诊断）

// AB-22：菜单 dex 直接内嵌在本 so 的填充区里（不新增 zip 条目）。
// 定义在文件后段（依赖 dl_iterate_phdr / PhdrCb）。
static bool CopyDexFromSelfEmbedded(const char* destPath);

namespace {

const char* kAssetDex = "assets/starry_dex/starry.dex";

JNIEnv* GetEnv(bool* detach) {
    *detach = false;
    if (!g_fgo_vm) return nullptr;
    JNIEnv* env = nullptr;
    if (g_fgo_vm->GetEnv((void**)&env, JNI_VERSION_1_6) == JNI_OK) return env;
    if (g_fgo_vm->AttachCurrentThread(&env, nullptr) == JNI_OK) { *detach = true; return env; }
    return nullptr;
}

// UTF 串 → std::string 风格缓冲
bool UtfToBuf(JNIEnv* env, jstring js, char* out, size_t n) {
    if (!js) return false;
    const char* c = env->GetStringUTFChars(js, nullptr);
    if (!c) { env->ExceptionClear(); return false; }
    snprintf(out, n, "%s", c);
    env->ReleaseStringUTFChars(js, c);
    return true;
}

jobject GetApplication(JNIEnv* env) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) { env->ExceptionClear(); return nullptr; }
    jmethodID cur = env->GetStaticMethodID(atCls, "currentApplication", "()Landroid/app/Application;");
    jobject app = cur ? env->CallStaticObjectMethod(atCls, cur) : nullptr;
    env->DeleteLocalRef(atCls);
    if (!app || env->ExceptionCheck()) { env->ExceptionClear(); return nullptr; }
    return app;
}

// ClassLoader.getSystemClassLoader() == 应用 PathClassLoader（app 进程内）
jobject SystemClassLoader(JNIEnv* env) {
    jclass clCls = env->FindClass("java/lang/ClassLoader");
    if (!clCls) { env->ExceptionClear(); return nullptr; }
    jmethodID m = env->GetStaticMethodID(clCls, "getSystemClassLoader", "()Ljava/lang/ClassLoader;");
    jobject cl = m ? env->CallStaticObjectMethod(clCls, m) : nullptr;
    env->DeleteLocalRef(clCls);
    if (env->ExceptionCheck()) { env->ExceptionClear(); return nullptr; }
    return cl;
}

// Android 上 ClassLoader.getSystemClassLoader() 返回的是「系统」PathClassLoader，
// 不是应用自己的！必须从已知应用类反查它真正的 ClassLoader（Class.getClassLoader() 是公开 API）。
jobject AppClassLoader(JNIEnv* env) {
    const char* known[] = {"com/bilibili/fatego/App", "com/ace/gshell/AceApplication", nullptr};
    jclass clsCls = env->FindClass("java/lang/Class");
    if (clsCls) {
        jmethodID gcl = env->GetMethodID(clsCls, "getClassLoader", "()Ljava/lang/ClassLoader;");
        for (int i = 0; known[i]; i++) {
            jclass c = env->FindClass(known[i]);
            if (!c) { env->ExceptionClear(); continue; }
            jobject cl = gcl ? env->CallObjectMethod(c, gcl) : nullptr;
            env->DeleteLocalRef(c);
            if (env->ExceptionCheck()) { env->ExceptionClear(); if (cl) env->DeleteLocalRef(cl); continue; }
            if (cl) {
                jclass clc = env->GetObjectClass(cl);
                jmethodID ts = env->GetMethodID(clc, "toString", "()Ljava/lang/String;");
                jstring js = ts ? (jstring)env->CallObjectMethod(cl, ts) : nullptr;
                if (js && !env->ExceptionCheck()) {
                    const char* cc = env->GetStringUTFChars(js, nullptr);
                    if (cc) { FGOBOOT_LOG("[fgo] 应用 ClassLoader = %s", cc); env->ReleaseStringUTFChars(js, cc); }
                }
                if (js) env->DeleteLocalRef(js);
                if (env->ExceptionCheck()) env->ExceptionClear();
                env->DeleteLocalRef(clc);
                env->DeleteLocalRef(clsCls);
                return cl;
            }
        }
        env->DeleteLocalRef(clsCls);
    } else {
        env->ExceptionClear();
    }
    // 兜底：主线程 context classloader
    jclass thCls = env->FindClass("java/lang/Thread");
    jmethodID curTh = thCls ? env->GetStaticMethodID(thCls, "currentThread", "()Ljava/lang/Thread;") : nullptr;
    jobject th = curTh ? env->CallStaticObjectMethod(thCls, curTh) : nullptr;
    if (thCls) env->DeleteLocalRef(thCls);
    if (th && !env->ExceptionCheck()) {
        jclass thc = env->GetObjectClass(th);
        jmethodID gccl = env->GetMethodID(thc, "getContextClassLoader", "()Ljava/lang/ClassLoader;");
        jobject cl = gccl ? env->CallObjectMethod(th, gccl) : nullptr;
        env->DeleteLocalRef(thc);
        env->DeleteLocalRef(th);
        if (env->ExceptionCheck()) { env->ExceptionClear(); if (cl) env->DeleteLocalRef(cl); cl = nullptr; }
        if (cl) { FGOBOOT_LOG("[fgo] 应用 ClassLoader 取自主线程 contextCL"); return cl; }
    } else if (env->ExceptionCheck()) env->ExceptionClear();
    FGOBOOT_LOG("[fgo] 应用 ClassLoader 反查失败，退回 getSystemClassLoader（可能注入错对象）");
    return SystemClassLoader(env);
}

bool FileReadable(const char* p) { return p && access(p, R_OK) == 0; }

// 用 java.util.zip.ZipFile 从 APK 里抽出 assets/starry_dex/starry.dex → dest
bool CopyAssetFromApk(JNIEnv* env, const char* apkPath, const char* destPath) {
    jclass zfCls = env->FindClass("java/util/zip/ZipFile");
    jmethodID zfInit = env->GetMethodID(zfCls, "<init>", "(Ljava/lang/String;)V");
    jstring japk = env->NewStringUTF(apkPath);
    jobject zf = env->NewObject(zfCls, zfInit, japk);
    env->DeleteLocalRef(japk);
    if (!zf || env->ExceptionCheck()) {
        FGOBOOT_LOG("[fgo] ZipFile(%s) 打开失败", apkPath);
        env->ExceptionClear();
        env->DeleteLocalRef(zfCls);
        return false;
    }
    jmethodID getEntry = env->GetMethodID(zfCls, "getEntry", "(Ljava/lang/String;)Ljava/util/zip/ZipEntry;");
    jstring jname = env->NewStringUTF(kAssetDex);
    jobject entry = getEntry ? env->CallObjectMethod(zf, getEntry, jname) : nullptr;
    env->DeleteLocalRef(jname);
    if (!entry || env->ExceptionCheck()) {
        FGOBOOT_LOG("[fgo] APK 内缺 %s", kAssetDex);
        env->ExceptionClear();
        env->DeleteLocalRef(zf);
        env->DeleteLocalRef(zfCls);
        return false;
    }
    jmethodID getIn = env->GetMethodID(zfCls, "getInputStream", "(Ljava/util/zip/ZipEntry;)Ljava/io/InputStream;");
    jobject in = getIn ? env->CallObjectMethod(zf, getIn, entry) : nullptr;
    if (!in || env->ExceptionCheck()) {
        env->ExceptionClear();
        env->DeleteLocalRef(entry); env->DeleteLocalRef(zf); env->DeleteLocalRef(zfCls);
        return false;
    }

    jclass fosCls = env->FindClass("java/io/FileOutputStream");
    jmethodID fosInit = env->GetMethodID(fosCls, "<init>", "(Ljava/lang/String;)V");
    jstring jd = env->NewStringUTF(destPath);
    jobject out = env->NewObject(fosCls, fosInit, jd);
    env->DeleteLocalRef(jd);
    if (!out || env->ExceptionCheck()) {
        env->ExceptionClear();
        env->DeleteLocalRef(in); env->DeleteLocalRef(entry); env->DeleteLocalRef(zf);
        env->DeleteLocalRef(fosCls); env->DeleteLocalRef(zfCls);
        return false;
    }

    jclass isCls = env->FindClass("java/io/InputStream");
    jmethodID read = env->GetMethodID(isCls, "read", "([B)I");
    jmethodID closeIn = env->GetMethodID(isCls, "close", "()V");
    jmethodID writeB = env->GetMethodID(fosCls, "write", "([BII)V");
    jmethodID closeOut = env->GetMethodID(fosCls, "close", "()V");
    jbyteArray buf = env->NewByteArray(65536);
    bool ok = true;
    long total = 0;
    while (true) {
        jint n = env->CallIntMethod(in, read, buf);
        if (env->ExceptionCheck()) { env->ExceptionClear(); ok = false; break; }
        if (n <= 0) break;
        env->CallVoidMethod(out, writeB, buf, 0, n);
        if (env->ExceptionCheck()) { env->ExceptionClear(); ok = false; break; }
        total += n;
    }
    env->CallVoidMethod(in, closeIn);
    env->CallVoidMethod(out, closeOut);
    if (env->ExceptionCheck()) env->ExceptionClear();
    env->DeleteLocalRef(buf);
    env->DeleteLocalRef(in);
    env->DeleteLocalRef(out);
    env->DeleteLocalRef(entry);
    env->DeleteLocalRef(isCls);
    env->DeleteLocalRef(fosCls);
    env->DeleteLocalRef(zf);
    env->DeleteLocalRef(zfCls);
    if (!ok) FGOBOOT_LOG("[fgo] 抽取 dex 中断");
    return ok && total > 0;
}

// DexClassLoader(dex, optDir, null, parentCL) → DexInstaller.install(parentCL, dexFile, optDir)
bool InjectDex(JNIEnv* env, const char* dexPath, const char* optDir, jobject parentCl) {
    jclass dclCls = env->FindClass("dalvik/system/DexClassLoader");
    if (!dclCls) { env->ExceptionClear(); return false; }
    jmethodID dclInit = env->GetMethodID(dclCls, "<init>",
        "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/ClassLoader;)V");
    jstring jdex = env->NewStringUTF(dexPath);
    jstring jopt = env->NewStringUTF(optDir);
    jobject dcl = env->NewObject(dclCls, dclInit, jdex, jopt, nullptr, parentCl);
    env->DeleteLocalRef(jdex);
    env->DeleteLocalRef(jopt);
    env->DeleteLocalRef(dclCls);
    if (!dcl || env->ExceptionCheck()) {
        FGOBOOT_LOG("[fgo] DexClassLoader 失败");
        env->ExceptionClear();
        return false;
    }

    jclass clCls = env->GetObjectClass(dcl);
    jmethodID loadClass = env->GetMethodID(clCls, "loadClass", "(Ljava/lang/String;)Ljava/lang/Class;");
    jstring jname = env->NewStringUTF("com.starry.magic.ltzj.DexInstaller");
    jclass instCls = loadClass ? (jclass)env->CallObjectMethod(dcl, loadClass, jname) : nullptr;
    env->DeleteLocalRef(jname);
    env->DeleteLocalRef(clCls);
    env->DeleteLocalRef(dcl);
    if (!instCls || env->ExceptionCheck()) {
        FGOBOOT_LOG("[fgo] 菜单 dex 里没有 DexInstaller");
        env->ExceptionClear();
        return false;
    }

    jclass fileCls = env->FindClass("java/io/File");
    jmethodID fileInit = env->GetMethodID(fileCls, "<init>", "(Ljava/lang/String;)V");
    jstring jdp = env->NewStringUTF(dexPath);
    jobject dexFile = env->NewObject(fileCls, fileInit, jdp);
    env->DeleteLocalRef(jdp);
    jstring jcp = env->NewStringUTF(optDir);
    jobject optDirObj = env->NewObject(fileCls, fileInit, jcp);
    env->DeleteLocalRef(jcp);
    env->DeleteLocalRef(fileCls);

    jmethodID installMid = env->GetStaticMethodID(instCls, "install",
        "(Ljava/lang/ClassLoader;Ljava/io/File;Ljava/io/File;)V");
    bool ok = false;
    if (installMid) {
        env->CallStaticVoidMethod(instCls, installMid, parentCl, dexFile, optDirObj);
        if (env->ExceptionCheck()) {
            FGOBOOT_LOG("[fgo] DexInstaller.install 抛异常");
            env->ExceptionDescribe();
            env->ExceptionClear();
        } else {
            ok = true;
        }
    } else {
        env->ExceptionClear();
        FGOBOOT_LOG("[fgo] DexInstaller.install 签名不匹配");
    }
    env->DeleteLocalRef(dexFile);
    env->DeleteLocalRef(optDirObj);
    env->DeleteLocalRef(instCls);
    return ok;
}

// 一次「确保 dex 已注入」；early=1 表示当前在 attachBaseContext（无 Context）
bool EnsureDexInjected(JNIEnv* env, const char* apkPath, const char* filesDir) {
    char dexPath[700];
    snprintf(dexPath, sizeof(dexPath), "%s/starry.dex", filesDir);
    if (!FileReadable(dexPath)) {
        // 优先内嵌（AB-22：不新增 assets 条目）；没有内嵌才退回 assets 抽取
        if (CopyDexFromSelfEmbedded(dexPath)) {
            FGOBOOT_LOG("[fgo] 内嵌 dex 已释放 → %s", dexPath);
        } else if (!CopyAssetFromApk(env, apkPath, dexPath)) {
            return false;
        } else {
            FGOBOOT_LOG("[fgo] assets/%s → %s", kAssetDex, dexPath);
        }
    }
    jobject scl = AppClassLoader(env);
    if (!scl) { FGOBOOT_LOG("[fgo] 取应用 ClassLoader 失败"); return false; }
    bool ok = InjectDex(env, dexPath, filesDir, scl);
    // 注入后立刻验证：能 FindClass 到菜单类才算真的挂上
    if (ok) {
        jclass mb = env->FindClass("com/starry/magic/bridge/ModBridge");
        if (mb) { env->DeleteLocalRef(mb); }
        else { env->ExceptionClear(); ok = false; FGOBOOT_LOG("[fgo] 注入后 FindClass ModBridge 仍失败 → 注入未生效"); }
    }
    env->DeleteLocalRef(scl);
    if (ok) {
        g_early_injected = true;
        FGOBOOT_LOG("[fgo] 菜单 dex 已注入应用 ClassLoader（%s）", dexPath);
    }
    return ok;
}

// ---- A) 早期同步注入：必须在 Manifest 里的 ContentProvider 被实例化之前完成 ----
bool EarlyInject(JNIEnv* env, jstring jSourceDir, jstring jFilesDir) {
    char src[600] = {0}, files[600] = {0};
    if (!UtfToBuf(env, jSourceDir, src, sizeof(src)) || !UtfToBuf(env, jFilesDir, files, sizeof(files))) {
        FGOBOOT_LOG("[fgo] initialize() 未拿到 sourceDir/filesDir");
        return false;
    }
    bool ok = EnsureDexInjected(env, src, files);
    if (ok) Starry_TryRegisterBridge(env);   // ModBridge 此刻已可见
    return ok;
}

// ---- B) 菜单启动线程 ----
bool StartMenuWithApplication(JNIEnv* env) {
    jobject app = GetApplication(env);
    if (!app) return false;

    if (!g_early_injected) {
        // 早期注入没成功（例如 filesDir 不可写）→ 退化为「Application 就绪后补注入」
        char src[600] = {0}, files[600] = {0};
        jclass ctxCls = env->GetObjectClass(app);
        jmethodID getApk = env->GetMethodID(ctxCls, "getPackageCodePath", "()Ljava/lang/String;");
        jstring jpk = getApk ? (jstring)env->CallObjectMethod(app, getApk) : nullptr;
        jmethodID getFiles = env->GetMethodID(ctxCls, "getFilesDir", "()Ljava/io/File;");
        jobject fdir = getFiles ? env->CallObjectMethod(app, getFiles) : nullptr;
        env->DeleteLocalRef(ctxCls);
        bool havePk = jpk && !env->ExceptionCheck();
        env->ExceptionClear();
        bool haveFd = false;
        if (fdir) {
            jclass fCls = env->GetObjectClass(fdir);
            jmethodID gp = env->GetMethodID(fCls, "getAbsolutePath", "()Ljava/lang/String;");
            jstring jp = gp ? (jstring)env->CallObjectMethod(fdir, gp) : nullptr;
            haveFd = jp && !env->ExceptionCheck() && UtfToBuf(env, jp, files, sizeof(files));
            if (jp) env->DeleteLocalRef(jp);
            env->DeleteLocalRef(fCls);
            env->DeleteLocalRef(fdir);
        }
        env->ExceptionClear();
        if (havePk) UtfToBuf(env, jpk, src, sizeof(src));
        if (jpk) env->DeleteLocalRef(jpk);
        if (havePk && haveFd) EnsureDexInjected(env, src, files);
    }

    Starry_TryRegisterBridge(env);   // 幂等

    jclass msCls = env->FindClass("com/starry/magic/menu/MenuService");
    if (!msCls) {
        FGOBOOT_LOG("[fgo] MenuService 仍不可见（注入未生效？）");
        env->ExceptionClear();
        env->DeleteLocalRef(app);
        return false;
    }
    jmethodID startMid = env->GetStaticMethodID(msCls, "start", "(Landroid/content/Context;)V");
    if (startMid) {
        env->CallStaticVoidMethod(msCls, startMid, app);
        if (env->ExceptionCheck()) {
            FGOBOOT_LOG("[fgo] MenuService.start 抛异常");
            env->ExceptionDescribe();
            env->ExceptionClear();
        } else {
            FGOBOOT_LOG("[fgo] MenuService.start 已调用 ✔");
        }
    } else {
        env->ExceptionClear();
    }
    env->DeleteLocalRef(msCls);
    env->DeleteLocalRef(app);
    return true;
}

void* MenuThread(void* arg) {
    g_fgo_vm = (JavaVM*)arg;
    for (int i = 0; i < 120; i++) {           // 250ms × 120 ≈ 30s
        if (i > 0) usleep(250 * 1000);
        bool detach = false;
        JNIEnv* env = GetEnv(&detach);
        if (!env) continue;
        bool ok = StartMenuWithApplication(env);
        if (detach) g_fgo_vm->DetachCurrentThread();
        if (ok) return nullptr;
    }
    FGOBOOT_LOG("[fgo] 菜单启动线程放弃（30s 未成功）");
    return nullptr;
}

} // namespace

// ============================================================
//  JNI_OnLoad 内调用：比 initialize() 更早的唯一窗口
//
//  AB-18 实测：installProvider() 在 AceApplication.initialize() **之前**执行，
//  而 Manifest 里声明的 StarryInitProvider 会在那一刻被系统实例化 →
//  codeCacheDir/PathClassLoader 里还没有 com.starry.magic.* → CNFE → 进程死。
//  所以注入必须发生在 JNI_OnLoad（= <clinit> 里 loadLibrary 的返回点），
//  此时 Raw Application 还没建，但能通过 ActivityThread.currentActivityThread()
//  .getApplicationInfo() 拿到 sourceDir，filesDir 用包名拼 /data/user/0/<pkg>/files。
// ============================================================
// 我们自己（libtersafe2.so 的替身）就映射在 base.apk 同级目录：
//   /data/app/~~X==/com.bilibili.fatego-Y==/lib/arm64/libtersafe2.so
// ⇒ 用 dl_iterate_phdr 找到本模块路径，截到 /lib/ 之前再拼 /base.apk。
// 全程公开 API，不依赖 ActivityThread（隐藏 API 在本机未豁免）。
struct PhdrFind { uintptr_t addr; const char* name; };

static int PhdrCb(struct dl_phdr_info* info, size_t, void* data) {
    PhdrFind* f = (PhdrFind*)data;
    for (int i = 0; i < info->dlpi_phnum; i++) {
        const ElfW(Phdr)* ph = &info->dlpi_phdr[i];
        if (ph->p_type != PT_LOAD) continue;
        uintptr_t start = (uintptr_t)info->dlpi_addr + (uintptr_t)ph->p_vaddr;
        if (f->addr >= start && f->addr < start + (uintptr_t)ph->p_memsz) {
            f->name = info->dlpi_name;
            return 1;
        }
    }
    return 0;
}

// 本模块（我们的 libtersafe2.so 替身）在磁盘上的真实路径
static bool SelfLibPath(char* out, size_t n) {
    PhdrFind f;
    f.addr = (uintptr_t)(void*)&SelfLibPath;
    f.name = nullptr;
    dl_iterate_phdr(PhdrCb, &f);
    if (!f.name || !f.name[0]) return false;
    snprintf(out, n, "%s", f.name);
    return true;
}

static bool ApkPathFromSelf(char* out, size_t n) {
    char lib[600] = {0};
    if (!SelfLibPath(lib, sizeof(lib))) return false;
    const char* p = strstr(lib, "/lib/");
    if (!p) return false;
    size_t len = (size_t)(p - lib);
    if (len + 10 > n) return false;
    memcpy(out, lib, len);
    strcpy(out + len, "/base.apk");
    return access(out, R_OK) == 0;
}

// ---- AB-22：内嵌菜单 dex ----
// 布局（写在替换用的 libtersafe2.so 填充区里，偏移由构建脚本决定）：
//   "STARRYDEX" (9B) + uint32 dexLen + uint32 dexCrc32 + dex 字节
// 运行时从自身 so 尾部向前扫描该 magic，校验 dex 头与 CRC32 后落盘。
static const char kEmbedMagic[9] = {'S','T','A','R','R','Y','D','E','X'};

static uint32_t Crc32Of(const uint8_t* p, size_t n) {
    static uint32_t tab[256];
    static bool ready = false;
    if (!ready) {
        for (uint32_t i = 0; i < 256; i++) {
            uint32_t c = i;
            for (int k = 0; k < 8; k++) c = (c & 1) ? (0xEDB88320u ^ (c >> 1)) : (c >> 1);
            tab[i] = c;
        }
        ready = true;
    }
    uint32_t c = 0xFFFFFFFFu;
    for (size_t i = 0; i < n; i++) c = tab[(c ^ p[i]) & 0xFF] ^ (c >> 8);
    return c ^ 0xFFFFFFFFu;
}

static bool CopyDexFromSelfEmbedded(const char* destPath) {
    char lib[600] = {0};
    if (!SelfLibPath(lib, sizeof(lib))) { FGOBOOT_LOG("[fgo] 内嵌 dex：取自身路径失败"); return false; }
    FILE* fp = fopen(lib, "rb");
    if (!fp) { FGOBOOT_LOG("[fgo] 内嵌 dex：打不开 %s", lib); return false; }
    fseek(fp, 0, SEEK_END);
    long sz = ftell(fp);
    fseek(fp, 0, SEEK_SET);
    if (sz <= 64) { fclose(fp); return false; }
    uint8_t* buf = (uint8_t*)malloc((size_t)sz);
    if (!buf) { fclose(fp); return false; }
    size_t got = fread(buf, 1, (size_t)sz, fp);
    fclose(fp);
    if (got != (size_t)sz) { free(buf); return false; }

    long dexOff = -1;
    uint32_t dexLen = 0, dexCrc = 0;
    for (long i = (long)got - 17; i >= 0; i--) {          // 从尾部往前找：.rodata 里的同名字面量不会抢先命中
        if (memcmp(buf + i, kEmbedMagic, 9) != 0) continue;
        uint32_t len, crc;
        memcpy(&len, buf + i + 9, 4);
        memcpy(&crc, buf + i + 13, 4);
        if (len == 0 || i + 17 + (long)len > (long)got) continue;
        const uint8_t* d = buf + i + 17;
        if (len < 0x70 || memcmp(d, "dex\n", 4) != 0) continue;   // 校验 dex 魔数
        if (Crc32Of(d, len) != crc) continue;                     // 校验 CRC32
        dexOff = i + 17; dexLen = len; dexCrc = crc;
        break;
    }
    if (dexOff < 0) { free(buf); FGOBOOT_LOG("[fgo] 内嵌 dex：未找到有效载荷（%s）", lib); return false; }

    FILE* out = fopen(destPath, "wb");
    if (!out) { free(buf); return false; }
    size_t w = fwrite(buf + dexOff, 1, dexLen, out);
    fclose(out);
    free(buf);
    if (w != dexLen) { FGOBOOT_LOG("[fgo] 内嵌 dex：写入不完整"); return false; }
    FGOBOOT_LOG("[fgo] 内嵌 dex 已释放 %u 字节 CRC32=0x%08x（源偏移=0x%lx）", dexLen, dexCrc, dexOff);
    return true;
}

// optDir：优先 code_cache，退回 files（都属于本进程）
static void FilesDirFromSelf(char* out, size_t n) {
    char pkg[256] = {0};
    FILE* fp = fopen("/proc/self/cmdline", "r");
    if (fp) {
        size_t got = fread(pkg, 1, sizeof(pkg) - 1, fp);
        pkg[got] = 0;
        fclose(fp);
        char* colon = strchr(pkg, ':');
        if (colon) *colon = 0;
    }
    if (!pkg[0]) snprintf(pkg, sizeof(pkg), "com.bilibili.fatego");
    snprintf(out, n, "/data/user/0/%s/code_cache", pkg);
    if (access(out, R_OK | W_OK) != 0) snprintf(out, n, "/data/user/0/%s/files", pkg);
}

// 从 ActivityThread.currentActivityThread().getApplicationInfo() 取 sourceDir
static bool SourceDirFromActivityThread(JNIEnv* env, char* out, size_t n) {
    jclass atCls = env->FindClass("android/app/ActivityThread");
    if (!atCls) { env->ExceptionClear(); return false; }
    jmethodID cur = env->GetStaticMethodID(atCls, "currentActivityThread", "()Landroid/app/ActivityThread;");
    jobject at = cur ? env->CallStaticObjectMethod(atCls, cur) : nullptr;
    env->DeleteLocalRef(atCls);
    if (!at || env->ExceptionCheck()) { env->ExceptionClear(); return false; }
    jclass atObjCls = env->GetObjectClass(at);
    jmethodID getAi = env->GetMethodID(atObjCls, "getApplicationInfo", "()Landroid/content/pm/ApplicationInfo;");
    jobject ai = getAi ? env->CallObjectMethod(at, getAi) : nullptr;
    env->DeleteLocalRef(atObjCls);
    env->DeleteLocalRef(at);
    if (!ai || env->ExceptionCheck()) { env->ExceptionClear(); return false; }
    jclass aiCls = env->GetObjectClass(ai);
    jfieldID f = env->GetFieldID(aiCls, "sourceDir", "Ljava/lang/String;");
    jstring js = f ? (jstring)env->GetObjectField(ai, f) : nullptr;
    env->DeleteLocalRef(aiCls);
    env->DeleteLocalRef(ai);
    if (!js || env->ExceptionCheck()) { env->ExceptionClear(); return false; }
    bool ok = UtfToBuf(env, js, out, n);
    env->DeleteLocalRef(js);
    return ok;
}

// 在 JNI_OnLoad 里调用：自己解析路径并注入，不依赖 initialize() 的参数
extern "C" void FgoBoot_InjectEarlyFromOnLoad(JNIEnv* env) {
    if (!env || g_early_injected) return;
#ifndef MYGWZD_NO_CRASHTRAP
    CrashTrap_Start();   // 尽量早装信号处理器（libtprt.so 还没装载，我们先占位）
#endif
    JavaVM* vm = nullptr;
    if (env->GetJavaVM(&vm) == JNI_OK && vm) g_fgo_vm = vm;

    char src[600] = {0}, files[600] = {0};
    if (!ApkPathFromSelf(src, sizeof(src))) {
        if (!SourceDirFromActivityThread(env, src, sizeof(src))) {
            FGOBOOT_LOG("[fgo] 早期注入：sourceDir 解析失败（dl_iterate_phdr + ActivityThread 都不行）");
            return;
        }
    }
    FilesDirFromSelf(files, sizeof(files));
    FGOBOOT_LOG("[fgo] 早期注入 sourceDir=%s optDir=%s", src, files);
    if (EnsureDexInjected(env, src, files)) {
        Starry_TryRegisterBridge(env);   // ModBridge 现已可见
        FGOBOOT_LOG("[fgo] 早期注入完成 ✔（installProvider 之前）");
    } else {
        FGOBOOT_LOG("[fgo] 早期注入失败（回退 Phase B 线程）");
    }
}

// 由 AceStub.cpp 的 initialize() 调用：早期同步注入（无 Context，靠 sourceDir/filesDir）
extern "C" void FgoBoot_EarlyInject(JNIEnv* env, jstring sourceDir, jstring filesDir) {
    if (!env || g_early_injected) return;
    JavaVM* vm = nullptr;
    if (env->GetJavaVM(&vm) == JNI_OK) g_fgo_vm = vm;
    EarlyInject(env, sourceDir, filesDir);
}

// 由 AceStub.cpp 的 initialize() 调用：起线程等 Application 就绪后启动菜单
extern "C" void FgoBoot_StartMenu(JNIEnv* env) {
    static bool thread_started = false;
    if (thread_started || !env) return;
    JavaVM* vm = nullptr;
    if (!g_fgo_vm) {
        if (env->GetJavaVM(&vm) != JNI_OK || !vm) return;
        g_fgo_vm = vm;
    }
    pthread_t th;
    if (pthread_create(&th, nullptr, MenuThread, g_fgo_vm) == 0) {
        thread_started = true;
        pthread_detach(th);
        FGOBOOT_LOG("[fgo] 菜单启动线程已启动");
    } else {
        FGOBOOT_LOG("[fgo] 菜单启动线程创建失败");
    }
}
