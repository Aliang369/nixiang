#ifndef SG_ZIP_H
#define SG_ZIP_H

// ============================================================
//  SgZip.h — 从 APK(zip) 里按条目名读取**明文**的最小实现（header-only）
//
//  谁在用：
//    - SgIntegrity.h      自完整性：比对 dex / main.js / 证书
//    - SgBuildManifest.h  服务端签名 build 清单：还要比对 so / 全部 dex
//
//  为什么不用 libzip/minizip：本仓 jni 只有 zlib（-lz），够用；代码越小越好审。
//  为什么读"明文"而不是"压缩流哈希"：换压缩方式（重打包）不该影响判定，
//  判定的语义应该是"这个文件的内容是不是我发出去的那份"。
//
//  安全口径：所有偏移/长度都做边界校验；任何畸形输入一律返回失败（不崩、不越界）。
//  仅支持 zip32（本包 989 条目，无 zip64 需要）。
// ============================================================

#include <stdio.h>
#include <stdint.h>
#include <string.h>
#include <stdlib.h>

#include <zlib.h>

namespace sgzip {

static inline uint32_t rd16(const uint8_t* p) { return (uint32_t)p[0] | ((uint32_t)p[1] << 8); }
static inline uint32_t rd32(const uint8_t* p) {
    return (uint32_t)p[0] | ((uint32_t)p[1] << 8) | ((uint32_t)p[2] << 16) | ((uint32_t)p[3] << 24);
}

struct EntryInfo {
    long     local_off;
    uint32_t comp_size;
    uint32_t uncomp_size;
    uint32_t method;      // 0=stored 8=deflate
};

/** 在中央目录里按名字找条目。0=找到，非 0=没找到/畸形。 */
static inline int find(FILE* fp, long file_len, const char* want, EntryInfo* out) {
    if (!fp || !want || !out || file_len <= 0) return 1;

    long tail = file_len > (65557 + 22) ? 65557 + 22 : file_len;
    long base = file_len - tail;
    uint8_t* buf = (uint8_t*)malloc((size_t)tail);
    if (!buf) return 1;
    if (fseek(fp, base, SEEK_SET) != 0 || fread(buf, 1, (size_t)tail, fp) != (size_t)tail) {
        free(buf); return 1;
    }
    long eocd = -1;
    for (long i = tail - 22; i >= 0; i--) {
        if (rd32(buf + i) == 0x06054b50u) { eocd = i; break; }
    }
    if (eocd < 0) { free(buf); return 1; }
    uint32_t total = rd16(buf + eocd + 10);
    uint32_t cd_size = rd32(buf + eocd + 12);
    uint32_t cd_off = rd32(buf + eocd + 16);
    free(buf);
    if (total == 0xffffu || cd_off == 0xffffffffu || cd_size == 0 || cd_size > (32u << 20)) return 1;
    if ((long)cd_off + (long)cd_size > file_len) return 1;

    uint8_t* cd = (uint8_t*)malloc(cd_size);
    if (!cd) return 1;
    if (fseek(fp, (long)cd_off, SEEK_SET) != 0 || fread(cd, 1, cd_size, fp) != cd_size) {
        free(cd); return 1;
    }
    int rc = 1;
    uint32_t p = 0;
    size_t want_len = strlen(want);
    for (uint32_t i = 0; i < total; i++) {
        if (p + 46 > cd_size || rd32(cd + p) != 0x02014b50u) break;
        uint32_t namelen = rd16(cd + p + 28);
        uint32_t extralen = rd16(cd + p + 30);
        uint32_t commentlen = rd16(cd + p + 32);
        if (p + 46 + namelen > cd_size) break;
        if (want_len == namelen && memcmp(cd + p + 46, want, want_len) == 0) {
            out->method = rd16(cd + p + 10);
            out->comp_size = rd32(cd + p + 20);
            out->uncomp_size = rd32(cd + p + 24);
            out->local_off = (long)rd32(cd + p + 42);
            rc = 0;
            break;
        }
        p += 46 + namelen + extralen + commentlen;
    }
    free(cd);
    return rc;
}

/** 读出条目**明文**（method 0/8）。成功返回 0 并置 *out（malloc，调用方 free）。 */
static inline int read_plain(FILE* fp, long file_len, const char* name,
                             uint8_t** out, uint32_t* out_len) {
    *out = nullptr;
    *out_len = 0;
    EntryInfo ei;
    if (find(fp, file_len, name, &ei) != 0) return 1;
    if (ei.uncomp_size == 0 || ei.uncomp_size > (16u << 20)) return 1;   // 16MB 上限（dex 最大 566KB）
    if (ei.comp_size == 0 || ei.comp_size > (16u << 20)) return 1;
    if (ei.local_off <= 0 || ei.local_off + 30 > file_len) return 1;

    uint8_t lh[30];
    if (fseek(fp, ei.local_off, SEEK_SET) != 0 || fread(lh, 1, 30, fp) != 30) return 1;
    if (rd32(lh) != 0x04034b50u) return 1;
    long data_off = ei.local_off + 30 + (long)rd16(lh + 26) + (long)rd16(lh + 28);
    if (data_off <= 0 || data_off + (long)ei.comp_size > file_len) return 1;

    uint8_t* in = (uint8_t*)malloc(ei.comp_size);
    uint8_t* dst = (uint8_t*)malloc(ei.uncomp_size);
    if (!in || !dst) { free(in); free(dst); return 1; }
    if (fseek(fp, data_off, SEEK_SET) != 0 || fread(in, 1, ei.comp_size, fp) != ei.comp_size) {
        free(in); free(dst); return 1;
    }

    int rc = 1;
    if (ei.method == 0) {
        if (ei.comp_size == ei.uncomp_size) { memcpy(dst, in, ei.uncomp_size); rc = 0; }
    } else if (ei.method == 8) {
        z_stream s;
        memset(&s, 0, sizeof(s));
        if (inflateInit2(&s, -15) == Z_OK) {
            s.next_in = in; s.avail_in = ei.comp_size;
            s.next_out = dst; s.avail_out = ei.uncomp_size;
            int zr = inflate(&s, Z_FINISH);
            if ((zr == Z_STREAM_END || zr == Z_OK) && s.total_out == ei.uncomp_size) rc = 0;
            inflateEnd(&s);
        }
    }
    free(in);
    if (rc != 0) { free(dst); return 1; }
    *out = dst;
    *out_len = ei.uncomp_size;
    return 0;
}

}  // namespace sgzip

#endif  // SG_ZIP_H
