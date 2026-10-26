#ifndef SG_PAYLOAD_H
#define SG_PAYLOAD_H

// ============================================================
//  SgPayload.h — 功能载荷解密 + 诱饵层（P2-2 / P2-3，header-only）
//
//  要解决的问题（用户第 2 点原话）：
//    "root 自己逆自己用" 允许；**"改包给别人用（给无 root 真机）"** 必须堵死。
//    光看门禁挡不住：改包的人把 feat_ok/live 改成恒真就行。所以要让**载荷本身**解不出来。
//
//  机制（Encrypt-then-MAC，只用手上已有的 HMAC-SHA256，不引第三方库）：
//    出包时：ct = payload XOR keystream(K_build)，mac = HMAC(K_build, "sgm1"|ct)
//    运行时：K_build 不硬编码在包里，而是由**服务器签的票**里带一个被签名字段 kx：
//              K_build = kx XOR sha256("sgpay"||sig_hex)
//            sig_hex 是票里的 RSA 签名 —— 没有私钥就签不出票 ⇒ 拿不到正确 K_build
//            ⇒ MAC 失败 ⇒ 落到**诱饵层**（合法 JS、静默无效、不上屏不闪退）。
//
//  边界（诚实）：有正版卡密的人在自己机器上仍能解出来（用户允许）；要拿去给别人用，
//  必须把 K_build 硬编码进改过的包并绕过派发 —— 成本和风险都上一个台阶。
//
//  失败行为：任何异常一律返回诱饵，绝不抛、绝不崩。
// ============================================================

#ifdef SG_PAYLOAD_ENC

#include <stdint.h>
#include <string.h>
#include <string>

#include "hmac_sha256.h"
#include "SgAuth.h"

namespace sgpayload {

/** 票签名 → 包裹密钥：sha256("sgpay" || sig_hex[0..559]) */
static inline void wrap_key(const char* sig_hex, size_t sig_len, uint8_t out[32]) {
    uint8_t buf[6 + 560];
    size_t n = 0;
    memcpy(buf, "sgpay", 5);
    n = 5;
    if (sig_len > 560) sig_len = 560;
    if (sig_hex && sig_len) { memcpy(buf + n, sig_hex, sig_len); n += sig_len; }
    sha256(buf, n, out);
    memset(buf, 0, sizeof(buf));
}

/** K_build = kx XOR wrap_key(sig) */
static inline bool build_key_from_ticket(const char* sig_hex, size_t sig_len,
                                        const uint8_t* kx, size_t kx_len, uint8_t out[32]) {
    if (!kx || kx_len != 32) return false;
    uint8_t wk[32];
    wrap_key(sig_hex, sig_len, wk);
    for (int i = 0; i < 32; i++) out[i] = (uint8_t)(kx[i] ^ wk[i]);
    memset(wk, 0, sizeof(wk));
    return true;
}

/** keystream(K) = HMAC-SHA256(K, "sgp1" || ctr_be32)，逐块 32B 异或 */
static inline void keystream_xor(const uint8_t key[32], uint8_t* buf, size_t n) {
    size_t off = 0;
    uint32_t ctr = 0;
    while (off < n) {
        uint8_t msg[8];
        memcpy(msg, "sgp1", 4);
        msg[4] = (uint8_t)(ctr >> 24);
        msg[5] = (uint8_t)(ctr >> 16);
        msg[6] = (uint8_t)(ctr >> 8);
        msg[7] = (uint8_t)ctr;
        uint8_t blk[32];
        hmac_sha256(key, 32, msg, 8, blk);
        size_t m = (n - off) < 32 ? (n - off) : 32;
        for (size_t i = 0; i < m; i++) buf[off + i] ^= blk[i];
        memset(blk, 0, sizeof(blk));
        off += m;
        ctr++;
    }
}

/** 解封：先验 MAC（对密文），再解出明文。失败**不写** out。 */
static inline bool unseal(const uint8_t* ct, size_t n, const uint8_t mac[32],
                          const uint8_t key[32], std::string* out) {
    if (!ct || n == 0 || !mac || !key || !out) return false;
    std::string msg;
    msg.reserve(4 + n);
    msg.assign("sgm1", 4);
    msg.append((const char*)ct, n);
    uint8_t want[32];
    hmac_sha256(key, 32, (const uint8_t*)msg.data(), msg.size(), want);
    uint8_t d = 0;
    for (int i = 0; i < 32; i++) d |= (uint8_t)(want[i] ^ mac[i]);
    if (d != 0) return false;
    out->assign((const char*)ct, n);
    if (n) keystream_xor(key, (uint8_t*)&(*out)[0], n);
    return true;
}

}  // namespace sgpayload

#endif  // SG_PAYLOAD_ENC
#endif  // SG_PAYLOAD_H
