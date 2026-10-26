#ifndef SG_BUILD_MANIFEST_H
#define SG_BUILD_MANIFEST_H

// ============================================================
//  SgBuildManifest.h — **服务端签名**的 build 清单校验（P2-1，header-only）
//
//  跟 SgIntegrity.h 的区别（为什么还要这一层）：
//    SgIntegrity 的清单编在 so 里 ⇒ **so 自己没法进清单**（自指）。
//    这一层的清单是**打包后**生成、由私钥签名、由服务端下发（客户端可缓存），
//    所以它能把 `lib/*/libstarry_magic.so` 和全部 dex 一起覆盖 ——
//    也就是「换掉 my so（把门禁整个删掉）也会被抓到」。
//
//  清单格式（纯文本，行式，便于 C 侧零依赖解析）：
//      SGB1
//      build <build_id>
//      created <unix>
//      E <apk 内路径> <明文大小> <sha256hex>
//      ...
//      SIG <512 hex RSA 签名>
//    签名覆盖 SIG 行之前的**全部字节**（含换行）。
//
//  用途：
//    ① 客户端：拿到服务端下发的清单 → 校验本地 APK（含 so）→ 决定功能是否放行；
//    ② 服务端：只有"发过清单的 build_id"才发票（build 白名单，见 auth-server）。
//
//  仅定义 SG_HAVE_ZLIB 时启用。
// ============================================================

#ifdef SG_HAVE_ZLIB

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdlib.h>

#include "hmac_sha256.h"
#include "sg_rsa_verify.h"
#include "sg_pubkey.h"
#include "SgZip.h"

namespace sgbuild {

static int hex2bin_(const char* hex, uint8_t* out, size_t out_cap, size_t* out_len) {
    size_t len = strlen(hex);
    if ((len & 1) != 0 || len / 2 > out_cap) return 1;
    for (size_t i = 0; i < len / 2; i++) {
        char a = hex[i * 2], b = hex[i * 2 + 1];
        int hi = (a >= '0' && a <= '9') ? a - '0' : (a >= 'a' && a <= 'f') ? a - 'a' + 10
               : (a >= 'A' && a <= 'F') ? a - 'A' + 10 : -1;
        int lo = (b >= '0' && b <= '9') ? b - '0' : (b >= 'a' && b <= 'f') ? b - 'a' + 10
               : (b >= 'A' && b <= 'F') ? b - 'A' + 10 : -1;
        if (hi < 0 || lo < 0) return 1;
        out[i] = (uint8_t)((hi << 4) | lo);
    }
    *out_len = len / 2;
    return 0;
}

static int eq32_(const uint8_t* a, const uint8_t* b) {
    uint8_t d = 0;
    for (int i = 0; i < 32; i++) d |= (uint8_t)(a[i] ^ b[i]);
    return d == 0;
}

/** 校验：清单签名 + APK 内每个条目（含 so）的明文哈希。
 *  @param build_id_out 可选，输出 build_id（容量 >= 48）
 *  @return 1 通过 / 0 失败
 */
static int verify(const char* apk_path,
                  const char* manifest, size_t manifest_len,
                  char* build_id_out, size_t build_id_cap) {
    if (!apk_path || !manifest || manifest_len < 64 || manifest_len > (1u << 20)) return 0;

    /* 1) 找最后一行 "SIG <hex>"（取最后一处，允许前面有别的行） */
    size_t sig_line = (size_t)-1;
    {
        size_t ls = 0;
        while (ls + 4 <= manifest_len) {
            if (memcmp(manifest + ls, "SIG ", 4) == 0) sig_line = ls;
            size_t ne = ls;
            while (ne < manifest_len && manifest[ne] != '\n') ne++;
            ls = ne + 1;
        }
    }
    if (sig_line == (size_t)-1) return 0;

    /* 2) 取签名 hex（到行尾） */
    const char* p = manifest + sig_line + 4;
    const char* end = manifest + manifest_len;
    const char* eol = p;
    while (eol < end && *eol != '\n' && *eol != '\r') eol++;
    char sig_hex[1024];
    size_t sl = (size_t)(eol - p);
    if (sl == 0 || sl >= sizeof(sig_hex)) return 0;
    memcpy(sig_hex, p, sl);
    sig_hex[sl] = 0;

    uint8_t sig[SG_MAX_BYTES];
    size_t sig_len = 0;
    if (hex2bin_(sig_hex, sig, sizeof(sig), &sig_len) != 0) return 0;
    if (sig_len != sizeof(kSgPubN)) return 0;

    /* 3) 验签名：覆盖 [0, sig_line) */
    if (!sg_rsa_verify_sha256(kSgPubN, sizeof(kSgPubN), sig, sig_len,
                              (const uint8_t*)manifest, sig_line)) {
        return 0;
    }

    /* 4) 解析 build_id + 逐条比对 APK */
    FILE* fp = fopen(apk_path, "rb");
    if (!fp) return 0;
    if (fseek(fp, 0, SEEK_END) != 0) { fclose(fp); return 0; }
    long file_len = ftell(fp);

    int ok = 1;
    int seen_entry = 0;
    size_t off = 0;
    while (off < sig_line) {
        size_t line_end = off;
        while (line_end < sig_line && manifest[line_end] != '\n') line_end++;
        size_t line_len = line_end - off;
        const char* line = manifest + off;

        if (line_len > 2 && memcmp(line, "E ", 2) == 0) {
            char path[160];
            unsigned long long want_size = 0;
            char want_hex[80];
            /* E <path> <size> <sha256hex> —— 路径不含空格（APK 内路径都无空格） */
            int n = sscanf(line, "E %159s %llu %79s", path, &want_size, want_hex);
            if (n != 3) { ok = 0; break; }
            uint8_t* plain = nullptr;
            uint32_t sz = 0;
            if (sgzip::read_plain(fp, file_len, path, &plain, &sz) != 0) { ok = 0; break; }
            if ((unsigned long long)sz != want_size) { free(plain); ok = 0; break; }
            uint8_t h[32];
            sha256(plain, sz, h);
            free(plain);
            uint8_t want[32];
            size_t wl = 0;
            if (hex2bin_(want_hex, want, sizeof(want), &wl) != 0 || wl != 32) { ok = 0; break; }
            if (!eq32_(h, want)) { ok = 0; break; }
            seen_entry = 1;
        } else if (line_len > 6 && memcmp(line, "build ", 6) == 0 && build_id_out && build_id_cap) {
            size_t cl = line_len - 6;
            if (cl >= build_id_cap) cl = build_id_cap - 1;
            memcpy(build_id_out, line + 6, cl);
            build_id_out[cl] = 0;
        }
        off = line_end + 1;
    }
    fclose(fp);
    if (!seen_entry) return 0;   /* 空清单＝不放行（别让"没有条目"变成"全通过"） */
    return ok;
}

}  // namespace sgbuild

#endif  // SG_HAVE_ZLIB
#endif  // SG_BUILD_MANIFEST_H
