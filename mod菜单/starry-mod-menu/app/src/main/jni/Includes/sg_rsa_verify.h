#ifndef SG_RSA_VERIFY_H
#define SG_RSA_VERIFY_H

// ============================================================
//  sg_rsa_verify.h — 星光授权票 / 完整性清单的**非对称验签**（只验证，不签名）
//
//  为什么要有它（2026-10-26 防御加固 P1-2）：
//    旧票是 HMAC-SHA256(pepper, body)，pepper 烘焙在 so 里 ⇒ 谁剥出 pepper，
//    谁就拿到「铸币权」：可以给任意 device/game 自签票，于是「删掉我的卡密、
//    挂他自己的卡密卖」零成本成立。
//    改成 RSA-2048 PKCS#1 v1.5（SHA-256）后，so 里只有**公钥模数**，
//    私钥只在服务器 / 出包机 ⇒ 他剥不动、签不出，这条路被物理掐断。
//
//  为什么是 RSA 而不是 Ed25519：
//    出包环境无网络（取不到经过审计的 curve25519 实现）、无 Windows C 编译器；
//    而 RSA 验签只需要「bignum 乘 + 模幂 + 逐字节 memcmp」，实现面小、可被
//    JDK 完整对拍（python 复现的签名与 JDK 逐字节相同）。安全性质等价：
//    都是「只有私钥能签」的非对称。
//
//  **header-only**：所有实现都是 static。这样任何游戏的 Android_*.mk 都不用
//    加源文件就不会 undefined symbol（公共 Android.mk 属禁改文件，不为它开后门）。
//    代价：每个 so 多约 3 KB 代码，可接受。
//
//  安全口径（勿改）：
//    - 指数固定 65537；验签 = 还原 EM 后**逐字节 memcmp 期望 EM**，不做宽松解析
//    - 签名长度必须严格等于模数长度；sig >= n 直接判失败
//    - 模数长度上限 2048 bit
//
//  对拍：04_scripts/_common/sg_pki/selftest_vectors.py（x86_64 模拟器 + arm64 真机）
// ============================================================

#include <stddef.h>
#include <stdint.h>
#include <string.h>

#include "hmac_sha256.h"   // sha256()

#define SG_MAX_BYTES  256  /* 2048 bit */
#define SG_MAX_LIMBS   64  /* 256 / 4 */
#define SG_PROD_LIMBS 128  /* 2 * SG_MAX_LIMBS */

/* RFC 8017 §9.2：DigestInfo(DER) for SHA-256 —— 19 字节 */
static const uint8_t sg_sha256_digestinfo[19] = {
    0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01,
    0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20
};

/* ---------- bignum：小端 32-bit limb ---------- */

static void sgrv_zero(uint32_t* a, int n) {
    for (int i = 0; i < n; i++) a[i] = 0;
}

static void sgrv_from_be(uint32_t* out, const uint8_t* be, int nbytes) {
    int limbs = nbytes / 4;
    sgrv_zero(out, limbs);
    for (int i = 0; i < limbs; i++) {
        const uint8_t* p = be + nbytes - 4 * (i + 1);
        out[i] = ((uint32_t)p[0] << 24) | ((uint32_t)p[1] << 16) |
                 ((uint32_t)p[2] << 8) | (uint32_t)p[3];
    }
}

static void sgrv_to_be(const uint32_t* a, uint8_t* be, int nbytes) {
    int limbs = nbytes / 4;
    for (int i = 0; i < limbs; i++) {
        uint8_t* p = be + nbytes - 4 * (i + 1);
        p[0] = (uint8_t)(a[i] >> 24);
        p[1] = (uint8_t)(a[i] >> 16);
        p[2] = (uint8_t)(a[i] >> 8);
        p[3] = (uint8_t)a[i];
    }
}

static int sgrv_cmp(const uint32_t* a, const uint32_t* b, int n) {
    for (int i = n - 1; i >= 0; i--) {
        if (a[i] != b[i]) return (a[i] > b[i]) ? 1 : -1;
    }
    return 0;
}

static void sgrv_sub(uint32_t* a, const uint32_t* b, int n) {
    int64_t borrow = 0;
    for (int i = 0; i < n; i++) {
        int64_t cur = (int64_t)a[i] - (int64_t)b[i] - borrow;
        if (cur < 0) { cur += ((int64_t)1 << 32); borrow = 1; } else borrow = 0;
        a[i] = (uint32_t)cur;
    }
}

static void sgrv_mul_full(uint32_t* prod, const uint32_t* a, const uint32_t* b, int n) {
    sgrv_zero(prod, 2 * n);
    uint32_t row[SG_MAX_LIMBS + 1];
    for (int i = 0; i < n; i++) {
        uint64_t carry = 0;
        for (int j = 0; j < n; j++) {
            uint64_t cur = (uint64_t)a[i] * (uint64_t)b[j] + carry;
            row[j] = (uint32_t)cur;
            carry = cur >> 32;
        }
        row[n] = (uint32_t)carry;
        uint64_t c2 = 0;
        for (int j = 0; j <= n; j++) {
            uint64_t sum = (uint64_t)prod[i + j] + (uint64_t)row[j] + c2;
            prod[i + j] = (uint32_t)sum;
            c2 = sum >> 32;
        }
    }
}

/** r[n+1] = a[2n] mod m[n]（二进制长除法：高位到低位，边移边减） */
static void sgrv_mod(uint32_t* r, const uint32_t* a, const uint32_t* m, int n) {
    sgrv_zero(r, n + 1);
    for (int bit = 2 * n * 32 - 1; bit >= 0; bit--) {
        uint32_t in = (a[bit >> 5] >> (bit & 31)) & 1u;
        for (int j = n; j > 0; j--) r[j] = (r[j] << 1) | (r[j - 1] >> 31);
        r[0] = (r[0] << 1) | in;
        if (r[n] != 0 || sgrv_cmp(r, m, n) >= 0) {
            sgrv_sub(r, m, n);
            r[n] = 0;
        }
    }
}

static void sgrv_mul_mod(uint32_t* out, const uint32_t* a, const uint32_t* b,
                         const uint32_t* m, int n) {
    uint32_t prod[SG_PROD_LIMBS];
    uint32_t r[SG_MAX_LIMBS + 1];
    sgrv_mul_full(prod, a, b, n);
    sgrv_mod(r, prod, m, n);
    memcpy(out, r, (size_t)n * 4);
}

static void sgrv_modexp(uint32_t* out, const uint32_t* base, const uint32_t* m,
                        uint32_t e, int n) {
    uint32_t res[SG_MAX_LIMBS];
    uint32_t x[SG_MAX_LIMBS];
    sgrv_zero(res, n);
    res[0] = 1;
    memcpy(x, base, (size_t)n * 4);
    int msb = 31;
    while (msb > 0 && ((e >> msb) & 1u) == 0) msb--;
    for (int i = msb; i >= 0; i--) {
        sgrv_mul_mod(res, res, res, m, n);
        if ((e >> i) & 1u) sgrv_mul_mod(res, res, x, m, n);
    }
    memcpy(out, res, (size_t)n * 4);
}

/** 验签：msg 的 SHA-256 是否被 sig 用「n 对应私钥」签过。1=有效，0=无效。 */
static int sg_rsa_verify_sha256(const uint8_t* n_be, size_t n_len,
                                const uint8_t* sig_be, size_t sig_len,
                                const uint8_t* msg, size_t msg_len) {
    if (!n_be || !sig_be || !msg) return 0;
    if (n_len == 0 || n_len > SG_MAX_BYTES || (n_len & 3) != 0) return 0;
    if (sig_len != n_len) return 0;

    const int n = (int)(n_len / 4);
    const int k = (int)n_len;

    uint32_t mod[SG_MAX_LIMBS];
    uint32_t sig[SG_MAX_LIMBS];
    sgrv_from_be(mod, n_be, k);
    sgrv_from_be(sig, sig_be, k);

    if (mod[n - 1] == 0) return 0;
    if (sgrv_cmp(sig, mod, n) >= 0) return 0;

    uint8_t em[SG_MAX_BYTES];
    const int t_len = (int)sizeof(sg_sha256_digestinfo) + 32;
    const int ps_len = k - t_len - 3;
    if (ps_len < 8) return 0;
    em[0] = 0x00;
    em[1] = 0x01;
    memset(em + 2, 0xff, (size_t)ps_len);
    em[2 + ps_len] = 0x00;
    memcpy(em + 3 + ps_len, sg_sha256_digestinfo, sizeof(sg_sha256_digestinfo));
    sha256(msg, msg_len, em + 3 + ps_len + (int)sizeof(sg_sha256_digestinfo));

    uint32_t m[SG_MAX_LIMBS];
    uint8_t got[SG_MAX_BYTES];
    sgrv_modexp(m, sig, mod, 65537u, n);
    sgrv_to_be(m, got, k);

    uint8_t diff = 0;
    for (int i = 0; i < k; i++) diff |= (uint8_t)(got[i] ^ em[i]);
    return diff == 0 ? 1 : 0;
}

#endif  // SG_RSA_VERIFY_H
