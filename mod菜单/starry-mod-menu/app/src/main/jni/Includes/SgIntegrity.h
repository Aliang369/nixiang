#ifndef SG_INTEGRITY_H
#define SG_INTEGRITY_H

// ============================================================
//  SgIntegrity.h — 自完整性校验（P1-6，header-only）
//
//  做什么：
//    1) 先验 `sg_integ.h` 里那张清单的**私钥签名**（没私钥改不动表）；
//    2) 再从**自己的 APK** 里按条目名取出 classes4.dex / assets/main.js / 证书，
//       算 sha256、逐条比对；
//    3) 全对 ⇒ sgauth::set_integrity(1)；任一不符/读不到 ⇒ 0（fail-closed）。
//
//  为什么能挡住「删我卡密 dex 重打包」：
//    dex 被删/被换 ⇒ 条目缺失或哈希不符 ⇒ 完整性 0 ⇒ session 不成立 ⇒ 功能全静默失效。
//
//  为什么不是「打包后生成清单」：
//    清单要编进 so、so 又在 APK 里，打包后生成是死循环。这里覆盖的是**明文内容**，
//    在编译 so 之前就已定稿（见 04_scripts/_common/sg_pki/gen_integ_header.py）。
//    更强的形态（连 so 自身一起覆盖、且由服务端签名）见 SgBuildManifest.h（P2）。
//
//  失败行为：**静默降级**（功能 no-op，不闪退、不弹窗）——闪退等于告诉破解者"你被识别了"。
//
//  仅在定义 SG_HAVE_ZLIB 的游戏里启用（需要 -lz）。
//
//  按游戏选清单（2026-10-27 通用化）：清单头由
//    `gen_integ_header.py --game <tag>` 生成 `sg_integ_<tag>.h`，
//    该游戏 mk 里加 `-DSG_INTEG_HEADER_FILE='"sg_integ_<tag>.h"'` 指过来；
//    不定义宏 ⇒ 用历史全局名 `sg_integ.h`（勇士之心旧构建链，勿删）。
// ============================================================

#ifdef SG_HAVE_ZLIB

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#include <time.h>

#include "hmac_sha256.h"
#include "sg_rsa_verify.h"
#ifdef SG_INTEG_HEADER_FILE
#include SG_INTEG_HEADER_FILE
#else
#include "sg_integ.h"
#endif
#include "SgZip.h"
#include "SgAuth.h"

namespace sginteg {

static int s_last_verdict = -1;      // -1 未跑 / 0 失败 / 1 通过
static time_t s_last_check = 0;
static int s_runs = 0;

/** 清单签名验证：canonical = "SGI1|<path>=<size>:<sha256hex>|..." */
static inline int verify_table() {
    char buf[2048];
    size_t n = 0;
    memcpy(buf, "SGI1", 4); n = 4;
    static const char* lut = "0123456789abcdef";
    for (int i = 0; i < kSgIntegCount; i++) {
        if (n + 200 > sizeof(buf)) return 0;
        buf[n++] = '|';
        size_t pl = strlen(kSgInteg[i].path);
        if (n + pl + 80 > sizeof(buf)) return 0;
        memcpy(buf + n, kSgInteg[i].path, pl); n += pl;
        n += (size_t)snprintf(buf + n, sizeof(buf) - n, "=%u:", (unsigned)kSgInteg[i].size);
        for (int j = 0; j < 32; j++) {
            buf[n++] = lut[(kSgInteg[i].sha256[j] >> 4) & 0xf];
            buf[n++] = lut[kSgInteg[i].sha256[j] & 0xf];
        }
    }
    uint8_t sig[SG_MAX_BYTES];
    size_t sl = strlen(kSgIntegSigHex);
    if (sl != sizeof(kSgPubN) * 2) return 0;
    for (size_t i = 0; i < sl / 2; i++) {
        char a = kSgIntegSigHex[i * 2], b = kSgIntegSigHex[i * 2 + 1];
        int hi = (a >= '0' && a <= '9') ? a - '0' : (a >= 'a' && a <= 'f') ? a - 'a' + 10
               : (a >= 'A' && a <= 'F') ? a - 'A' + 10 : -1;
        int lo = (b >= '0' && b <= '9') ? b - '0' : (b >= 'a' && b <= 'f') ? b - 'a' + 10
               : (b >= 'A' && b <= 'F') ? b - 'A' + 10 : -1;
        if (hi < 0 || lo < 0) return 0;
        sig[i] = (uint8_t)((hi << 4) | lo);
    }
    return sg_rsa_verify_sha256(kSgPubN, sizeof(kSgPubN), sig, sl / 2,
                                (const uint8_t*)buf, n);
}

/** 跑一次完整校验：1 通过 / 0 失败。结果同步进授权链（三判据的第 ③ 条）。 */
static inline int run(const char* apk_path) {
    s_last_check = time(nullptr);
    s_runs++;
    if (!apk_path || !*apk_path) { s_last_verdict = 0; sgauth::set_integrity(0); return 0; }
    if (!verify_table()) { s_last_verdict = 0; sgauth::set_integrity(0); return 0; }

    FILE* fp = fopen(apk_path, "rb");
    if (!fp) { s_last_verdict = 0; sgauth::set_integrity(0); return 0; }
    if (fseek(fp, 0, SEEK_END) != 0) { fclose(fp); s_last_verdict = 0; sgauth::set_integrity(0); return 0; }
    long file_len = ftell(fp);

    int ok = 1;
    for (int i = 0; i < kSgIntegCount; i++) {
        uint8_t* plain = nullptr;
        uint32_t sz = 0;
        if (sgzip::read_plain(fp, file_len, kSgInteg[i].path, &plain, &sz) != 0) { ok = 0; break; }
        if (sz != kSgInteg[i].size) { free(plain); ok = 0; break; }
        uint8_t h[32];
        sha256(plain, sz, h);
        free(plain);
        uint8_t diff = 0;
        for (int j = 0; j < 32; j++) diff |= (uint8_t)(h[j] ^ kSgInteg[i].sha256[j]);
        if (diff != 0) { ok = 0; break; }
    }
    fclose(fp);
    s_last_verdict = ok;
    sgauth::set_integrity(ok ? 1 : 0);
    return ok;
}

/** 低频复验：最小间隔 min_sec 秒（防"启动时通过、之后被换"）。返回当前判决。 */
static inline int maybe_recheck(const char* apk_path, int min_sec) {
    time_t now = time(nullptr);
    if (s_last_verdict < 0 || (now - s_last_check) >= min_sec) {
        return run(apk_path);
    }
    sgauth::set_integrity(s_last_verdict);
    return s_last_verdict;
}

static inline int verdict() { return s_last_verdict; }
static inline int runs() { return s_runs; }

}  // namespace sginteg

#endif  // SG_HAVE_ZLIB
#endif  // SG_INTEGRITY_H
