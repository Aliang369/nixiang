#ifndef SG_PAYLOAD_JS_H
#define SG_PAYLOAD_JS_H

// ============================================================
//  SgPayloadJs.h — **跨游戏共享**的"取功能载荷"入口（P2-2 通用化）
//
//  为什么要有这一层：
//    每个游戏的 JS 注入点都在自己的 `<Tag>Mod.cpp` 里（勇士之心在 HookedEvalString），
//    但"解密成功才用真载荷、否则喂诱饵、诱饵不缓存可重试"这套逻辑是**所有游戏一样的**。
//    把逻辑放这里，各游戏只需一行调用，不必复制粘贴，也不会各写一版走样。
//
//  用法（任何游戏的 Mod.cpp）：
//      #include "Includes/SgPayloadJs.h"
//      #include "Includes/sg_payload_enc_<tag>.h"   // 本游戏的密文（生成物）
//      #include "Games/SgDecoyJs.h"                // 共享诱饵
//      ...
//      patched.assign(sgpjs::get(kTagPayloadCt, kTagPayloadLen, kTagPayloadMac, kDecoyJs));
//
//  约定：
//    · 载荷密钥由 `extern "C" int sgp_payload_key(uint8_t[32])` 提供
//      （实现在 MainModern.cpp，因为 SgAuth 的 static 状态是每个 TU 一份）
//    · 返回 const std::string&：成功结果缓存；**诱饵不缓存**（票晚到时可重试）
//    · 任何失败都不抛、不崩，只回落诱饵
// ============================================================

#ifdef SG_PAYLOAD_ENC

#include <stdint.h>
#include <string.h>
#include <string>

#include <android/log.h>
#include "SgPayload.h"

/* 关键日志（也给出门禁 C7 的字符串证据：加密层真的编进来了） */
#ifndef SG_PJS_TAG
#define SG_PJS_TAG "StarryMagic"
#endif
#define SG_PJS_LOGI(...) __android_log_print(ANDROID_LOG_INFO, SG_PJS_TAG, __VA_ARGS__)
#define SG_PJS_LOGW(...) __android_log_print(ANDROID_LOG_WARN, SG_PJS_TAG, __VA_ARGS__)

extern "C" int sgp_payload_key(uint8_t out[32]);

namespace sgpjs {

static std::string g_real;        // 解密成功后的真载荷（缓存）
static int g_decoyHits = 0;

/** 本次是否已经拿到真载荷（供上层打点/上报用） */
static inline bool ready() { return !g_real.empty(); }
static inline int decoy_hits() { return g_decoyHits; }

/**
 * @param ct     本游戏的载荷密文
 * @param len    密文长度
 * @param mac    32 字节 MAC
 * @param decoy  诱饵 JS（共享的惰性 stub）
 * @return 真载荷（成功并缓存）或诱饵（不缓存，下次可重试）
 */
static inline const std::string& get(const uint8_t* ct, uint32_t len,
                                    const uint8_t mac[32], const char* decoy) {
    if (!g_real.empty()) return g_real;

    static const std::string s_decoy(decoy ? decoy : "");

    uint8_t pk[32];
    if (!sgp_payload_key(pk)) {
        ++g_decoyHits;
        SG_PJS_LOGW("payload key absent -> decoy (%d)", g_decoyHits);
        return s_decoy;                       // 不缓存：票可能比 bundle 晚到
    }
    std::string out;
    bool ok = sgpayload::unseal(ct, len, mac, pk, &out);
    memset(pk, 0, sizeof(pk));
    if (!ok) {
        ++g_decoyHits;
        SG_PJS_LOGW("payload unseal failed -> decoy (%d)", g_decoyHits);
        return s_decoy;
    }
    SG_PJS_LOGI("payload decrypted %zu B", out.size());
    g_real.swap(out);
    return g_real;
}

}  // namespace sgpjs

#endif  // SG_PAYLOAD_ENC
#endif  // SG_PAYLOAD_JS_H
