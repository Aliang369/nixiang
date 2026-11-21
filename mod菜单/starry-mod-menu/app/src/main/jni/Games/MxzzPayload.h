/* ================================================================
 *  MxzzPayload.h —— 【生成物·勿手改】
 *  生成者：04_scripts/冒险战争/gen_mxzz_payload_header.py
 *  生成依据：04_scripts/冒险战争/mxzz_payload_spec.json
 *
 *  载荷表语义：官方 sha256 → 同版本的 Cecil 补丁体（dev 优先 / APK 基线兜底）。
 *  运行期不在表里的 sha 一律 PASSTHROUGH（不崩、不闪退）。
 * ================================================================ */
#ifndef STARRY_MXZZ_PAYLOAD_H
#define STARRY_MXZZ_PAYLOAD_H

extern "C" const unsigned char _binary_mxzz_dev_bin_start[];
extern "C" const unsigned char _binary_mxzz_base_bin_start[];

struct MxzzPayloadEntry {
    const char* sha;            // 官方程序集 sha256（小写 hex）
    const unsigned char* data;  // 补丁体（objcopy 内嵌）
    unsigned len;               // 补丁体长度（托管堆按此重新分配容器）
};

#define MXZZ_PAYLOAD_GENERATED 1
#define MXZZ_PAYLOAD_POLICY "dev+base"
#define MXZZ_PAYLOAD_COUNT 2
static const MxzzPayloadEntry kMxzzPayloads[] = {
    { "9802e7ed91c9ec86a4cfc4862c30da7a79df346834b88411a59d5e5c206411a8", _binary_mxzz_dev_bin_start, 18738688u },   // dev
    { "e58aa7ef725f776003404022f12a570eab33da32eb9fc83def44ada0f56d5bfb", _binary_mxzz_base_bin_start, 17925120u },   // base
};

// 补丁体自身的 sha256：缓冲区已是补丁体时静默放行（否则会误报「未知版本」并误弹 toast）
#define MXZZ_SELF_SHA_COUNT 2
static const char* const kMxzzSelfShas[] = {
    "1777bd031f44603ff516451b409fa4cc9a7913832d6f6e233b2208d382dd2b26",   // dev
    "f5adec68f7c40659fad31e0d68b0a9a229f57b7c7a5ee35ee03309ec9b50005d",   // base
};

#endif  // STARRY_MXZZ_PAYLOAD_H
