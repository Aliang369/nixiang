#ifndef SG_AUTH_H
#define SG_AUTH_H

// ============================================================
//  SgAuth.h — 星光授权链**单一实现**（2026-10-26 防御加固 P1）
//
//  背景（实测证据见 10_docs/勇士之心/防御加固_自我逆向与方案_20261026.md）：
//    旧实现是 Main.cpp 与 MainModern.cpp **各复制一份**（pepper / apply_grant_utf8 /
//    session_live / g_rt_ok），而勇士之心实际编译的是 MainModern.cpp ⇒ 改一处必漏
//    另一处，等于历史上「修好一边另一边失效」的同一个坑。这里把逻辑收成一份，
//    两份主文件只留 3 行委托。
//
//  相比旧实现改了四件事：
//    ① 票改**非对称**（RSA-2048 PKCS#1 v1.5，见 sg_rsa_verify.h）：so 里只有公钥，
//       剥出来也签不出票 ⇒ 破解者拿不到「铸币权」，二次加卡密卖不成立。
//       旧 v1(HMAC) 票保留一个版本做过渡，由版本号前缀区分。
//    ② 票里带 **per-feature 位掩码**：不再是「游戏开/关」一个开关，
//       每个功能一位，`feat_ok(i)` 才是功能级门禁。
//    ③ 门禁从「一个 bool」拆成**三条互不相同的判据**（票标志 / 票派生密钥 K 的自检 /
//       完整性摘要），任一条不满足都不放行 —— 单点 patch 一个 bool 不再能全开。
//    ④ 缺票/被篡改时 **静默失效**（不是闪退）：破解者得不到「你被发现了」的信号。
//
//  票格式：
//    v2（新）: 2|device|game|exp|mask|kid.<512hex RSA 签名>
//    v1（旧）: 1|device|game|exp|menu|card|kid.<64hex HMAC>   ← 过渡期兼容，一版后退役
//
//  状态寿命（沿用现网口径，别改）：登录成功后**进程内永不过期**，只有进程重启 /
//    nativeClearGrant / 停用票才清 —— 玩家体验优先，破解窗口靠上面的加密层收。
// ============================================================

#include <stdint.h>
#include <string.h>
#include <stdlib.h>
#include <time.h>

#include "hmac_sha256.h"
#include "sg_rsa_verify.h"
#include "sg_pubkey.h"
#include "obfuscate.h"

namespace sgauth {

// ---------------- 状态 ----------------

struct State {
    int      ticket_ok;   // ① 票验签通过（真票才置 1）
    uint8_t  key[32];     // ② 票派生密钥 K（全零 = 未授权）
    uint8_t  canary[8];   // ② K 的自检指纹（apply 时算，live 时重算比对）
    uint32_t mask;        // per-feature 位掩码（bit i = 功能 i）
    int      integ;       // ③ 完整性：1 通过 / 0 失败 / -1 还没检
};

static State s_state = {0, {0}, {0}, 0, -1};

/** 旧 v1 票的 HMAC 密钥（过渡期保留；一版后连同服务端一起退役） */
static inline const char* legacy_pepper() {
    return OBFUSCATE("sg-grant-v1-7f3a9c2e");
}

// ---------------- 小工具 ----------------

static inline int sg_hex_eq(const char* hex, const uint8_t* dig, size_t n) {
    static const char* lut = "0123456789abcdef";
    if (!hex || strlen(hex) < n * 2) return 0;
    for (size_t i = 0; i < n; i++) {
        char a = hex[i * 2], b = hex[i * 2 + 1];
        if (a >= 'A' && a <= 'F') a = (char)(a - 'A' + 'a');
        if (b >= 'A' && b <= 'F') b = (char)(b - 'A' + 'a');
        if (a != lut[(dig[i] >> 4) & 0xf] || b != lut[dig[i] & 0xf]) return 0;
    }
    return 1;
}

static inline int sg_hex2bin(const char* hex, uint8_t* out, size_t out_cap) {
    if (!hex) return -1;
    size_t len = strlen(hex);
    if ((len & 1) != 0 || len / 2 > out_cap) return -1;
    for (size_t i = 0; i < len / 2; i++) {
        int hi, lo;
        char a = hex[i * 2], b = hex[i * 2 + 1];
        hi = (a >= '0' && a <= '9') ? a - '0' : (a >= 'a' && a <= 'f') ? a - 'a' + 10
           : (a >= 'A' && a <= 'F') ? a - 'A' + 10 : -1;
        lo = (b >= '0' && b <= '9') ? b - '0' : (b >= 'a' && b <= 'f') ? b - 'a' + 10
           : (b >= 'A' && b <= 'F') ? b - 'A' + 10 : -1;
        if (hi < 0 || lo < 0) return -1;
        out[i] = (uint8_t)((hi << 4) | lo);
    }
    return (int)(len / 2);
}

/** K = SHA256("sgk1|" + 票签名hex + "|" + device)
 *  签名是服务器私钥产物，攻击者拿不到私钥就得不到 K ⇒ 后面所有加密载荷都解不开。 */
static inline void sg_derive_key(const char* sig_hex, const char* device,
                                 uint8_t out[32], uint8_t canary[8]) {
    uint8_t buf[640];
    size_t n = 0;
    const char* p1 = "sgk1|";
    memcpy(buf, p1, 5); n = 5;
    size_t sl = strlen(sig_hex);
    if (sl > 560) sl = 560;
    memcpy(buf + n, sig_hex, sl); n += sl;
    buf[n++] = '|';
    size_t dl = strlen(device);
    if (dl > 64) dl = 64;
    memcpy(buf + n, device, dl); n += dl;
    uint8_t tmp[32];              // sha256 输出固定 32 字节，先落到 32B 缓冲再截 8B
    sha256(buf, n, out);
    sha256(out, 32, tmp);
    memcpy(canary, tmp, 8);
    memset(tmp, 0, sizeof(tmp));
}

// ---------------- 对外 ----------------

/** 完整性判据（③）。P1-6 由 SgIntegrity 模块写入；未接该模块的构建不过此判据。 */
static inline void set_integrity(int v) { s_state.integ = v; }
static inline int  integrity() { return s_state.integ; }
static inline uint32_t feature_mask() { return s_state.mask; }

/** 票派生密钥（32B，未验证票时为全 0）。
 *  P2-2：功能载荷用它派生解密密钥 —— 签不出票就拿不到正确密钥，只能落到诱饵层。 */
static inline const uint8_t* session_key() { return s_state.key; }
static inline bool session_key_ready() {
    uint8_t d = 0;
    for (int i = 0; i < 32; i++) d |= s_state.key[i];
    return d != 0;
}

// ---------------- P2-2 载荷密钥（由 v3 票的 kx 字段还原）----------------
// K_build = kx XOR sha256("sgpay"||sig_hex)；只有验签通过的票才会写入这里，
// 拿不到就返回 false ⇒ 上层落到诱饵层（静默无效）。
static uint8_t s_paykey[32];
static inline void set_payload_key(const uint8_t k[32]) {
    if (k) memcpy(s_paykey, k, 32); else memset(s_paykey, 0, 32);
}
static inline const uint8_t* payload_key() { return s_paykey; }
static inline bool payload_key_ready() {
    uint8_t d = 0;
    for (int i = 0; i < 32; i++) d |= s_paykey[i];
    return d != 0;
}

static inline void reset() {
    s_state.ticket_ok = 0;
    memset(s_state.key, 0, sizeof(s_state.key));
    memset(s_state.canary, 0, sizeof(s_state.canary));
    s_state.mask = 0;
    s_state.integ = -1;
}

/** 三条判据都成立才算会话存活。任何一条不满足 ⇒ 功能全部静默失效。 */
static inline bool live() {
#ifdef SG_REQUIRE_INTEGRITY
    /* ③ 完整性：未检(-1)也算不通过 —— fail-closed，别给「模块没跑」留后门 */
    if (s_state.integ != 1) return false;
#endif
    /* ① 票标志 */
    if (!s_state.ticket_ok) return false;
    /* ② 派生密钥自检：K 非零 + 重算指纹一致（防内存里被直接写 key） */
    uint8_t chk[32];                 // 必须是 32：sha256 会写满 32 字节
    sha256(s_state.key, 32, chk);
    uint8_t diff = 0;
    for (int i = 0; i < 8; i++) diff |= (uint8_t)(chk[i] ^ s_state.canary[i]);
    if (diff != 0) return false;
    return true;
}

/** 功能级门禁：第 idx 位是否被这张票授权（idx 0..31） */
static inline bool feat_ok(int idx) {
    if (idx < 0 || idx > 31) return false;
    if (!live()) return false;
    return ((s_state.mask >> idx) & 1u) != 0;
}

/** 应用一张票：成功返回 1 并激活会话；失败返回 0 且**不清**已有会话（续票语义）。 */
static inline int apply(const char* ticket, const char* device, const char* game) {
    if (!ticket || !device || !game || !*ticket || !*device) return 0;

    const char* dot = strrchr(ticket, '.');
    if (!dot || dot == ticket || !*(dot + 1)) return 0;
    size_t body_len = (size_t)(dot - ticket);
    if (body_len < 8 || body_len > 640) return 0;

    char body[648];
    memcpy(body, ticket, body_len);
    body[body_len] = 0;
    const char* sig_hex = dot + 1;

    const int is_v3 = (body[0] == '3' && body[1] == '|');
    const int is_v2 = is_v3 || (body[0] == '2' && body[1] == '|');

    if (is_v2) {
        /* ---- v2：非对称验签（so 里没有私钥 ⇒ 签不出来） ---- */
        uint8_t sig[SG_MAX_BYTES];
        int sig_len = sg_hex2bin(sig_hex, sig, sizeof(sig));
        if (sig_len != (int)sizeof(kSgPubN)) return 0;
        if (!sg_rsa_verify_sha256(kSgPubN, sizeof(kSgPubN), sig, (size_t)sig_len,
                                  (const uint8_t*)body, body_len)) {
            return 0;
        }
    } else {
        /* ---- v1：旧 HMAC（过渡期），签名长度固定 64 hex ---- */
        uint8_t mac[32];
        const char* pepper = legacy_pepper();
        hmac_sha256((const uint8_t*)pepper, strlen(pepper),
                    (const uint8_t*)body, body_len, mac);
        if (!sg_hex_eq(sig_hex, mac, 32)) return 0;
    }

    /* ---- 字段校验（两种版本共用前四段：ver|device|game|exp） ---- */
    char tmp[648];
    memcpy(tmp, body, body_len + 1);
    char* save = nullptr;
    char* ver = strtok_r(tmp, "|", &save);
    char* dev = strtok_r(nullptr, "|", &save);
    char* gm = strtok_r(nullptr, "|", &save);
    char* exp_s = strtok_r(nullptr, "|", &save);
    char* f5 = strtok_r(nullptr, "|", &save);
    char* f6 = strtok_r(nullptr, "|", &save);   /* v3: kid */
    char* f7 = strtok_r(nullptr, "|", &save);   /* v3: kx（载荷密钥包裹） */
    if (!ver || !dev || !gm || !exp_s || !f5) return 0;
    if (strcmp(dev, device) != 0 || strcmp(gm, game) != 0) return 0;

    int64_t exp = (int64_t)atoll(exp_s);
    int64_t now = (int64_t)time(nullptr);
    if (now > exp + 45) return 0;          /* 首次验签不接受过期票 */

    uint32_t mask;
    if (is_v2) {
        if (strcmp(ver, is_v3 ? "3" : "2") != 0) return 0;
        mask = (uint32_t)strtoul(f5, nullptr, 10);
        if (mask == 0) return 0;           /* 掩码 0 = 后台停用，不放行 */
        if (is_v3 && (!f6 || !f7 || strlen(f7) != 64)) return 0;
    } else {
        if (strcmp(ver, "1") != 0) return 0;
        if (f5[0] == '0') return 0;        /* 旧票：menu=0 停用 */
        mask = 0xffffffffu;                /* 旧票无掩码 → 视作全开（兼容） */
    }

    /* ---- 激活：三条判据的原料一次写全 ---- */
    uint8_t k[32], canary[8];
    sg_derive_key(sig_hex, device, k, canary);
    memcpy(s_state.key, k, 32);
    memcpy(s_state.canary, canary, 8);
    s_state.mask = mask;
    s_state.ticket_ok = 1;
    memset(k, 0, sizeof(k));

    /* ---- P2-2：v3 票里带 kx，还原功能载荷密钥 K_build ----
       wrap = sha256("sgpay|device|game|exp|mask|kid")（只依赖被签名覆盖的字段）
       K_build = kx XOR wrap；拿不到就保持全 0 ⇒ 载荷层落诱饵。 */
    if (is_v3) {
        char wk[256];
        int n = snprintf(wk, sizeof(wk), "sgpay|%s|%s|%s|%s|%s", dev, gm, exp_s, f5, f6);
        if (n > 0 && (size_t)n < sizeof(wk)) {
            uint8_t wrap[32], kx[32], kb[32];
            sha256((const uint8_t*)wk, (size_t)n, wrap);
            if (sg_hex2bin(f7, kx, sizeof(kx)) == 32) {
                for (int i = 0; i < 32; i++) kb[i] = (uint8_t)(kx[i] ^ wrap[i]);
                set_payload_key(kb);
                memset(kb, 0, sizeof(kb));
            }
            memset(wrap, 0, sizeof(wrap));
            memset(kx, 0, sizeof(kx));
            memset(wk, 0, sizeof(wk));
        }
    }
    return 1;
}

}  // namespace sgauth

#endif  // SG_AUTH_H
