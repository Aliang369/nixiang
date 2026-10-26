#ifndef SG_INTEG_H
#define SG_INTEG_H

// ============================================================
//  sg_integ_mxyzt.h — 冒险与征途 完整性清单（**自动生成，勿手改**）
//  生成：04_scripts/_common/sg_pki/gen_integ_header.py --game mxyzt
//  覆盖：APK 内条目明文哈希；清单由私钥签名，C 侧先验签名再比对。
//  改这张表没有私钥 ⇒ 验签失败 ⇒ 会话不成立（fail-closed）。
// ============================================================

#include <stdint.h>

typedef struct {
    const char* path;        // APK 内路径
    uint32_t    size;        // 明文长度
    uint8_t     sha256[32];  // 明文 sha256
} sg_integ_entry;

static const sg_integ_entry kSgInteg[2] = {
    { "classes4.dex", 528788, { 0x00, 0x97, 0x00, 0xc7, 0xad, 0x8d, 0xa7, 0x14, 0x19, 0x45, 0x6b, 0x93, 0xc4, 0x73, 0x34, 0x23, 0xe7, 0x73, 0xb9, 0x67, 0x67, 0x3c, 0xfc, 0xd0, 0x7e, 0x0e, 0x21, 0xbc, 0xd6, 0xf0, 0x0f, 0xe2 } },
    { "assets/certs/starlight.crt", 1151, { 0x33, 0xbf, 0x4b, 0x50, 0x7a, 0x54, 0x48, 0x54, 0x91, 0x22, 0x3c, 0xce, 0xe0, 0x46, 0xd9, 0x48, 0x3a, 0xb6, 0x5f, 0x11, 0x94, 0x39, 0x79, 0xe6, 0x75, 0x55, 0x3d, 0x5e, 0x33, 0xf1, 0x8b, 0xfb } },
};
static const int kSgIntegCount = 2;

static const char kSgIntegSigHex[] = "4226bb53872d992b0f115af3db6d172d138f8b5032e805dc363b6f8b2a87c478abfe0cb4ad75936a7f8dff2b0a1b4913d4d91886c5eb80a6cb71c7cbb7e7f45c3f0d1d549d4bc48e8e172f76f4cc75032247c3583c70f9c341e6c6d911f3c641d5c9b8c2003b86cbb156c7d96ca6a9e6f77aa3700f0a0a995733cf543b80557ef57e043ff34608f777354242d76ab55d9c39d2233f65686838b4aec0e09f07adc0c21414d2d0702e11c166a90d37c114e57c7098773ed1ecdd33ece25d70428fd94d89c5ee7e4cbdf75403a629d1849c6a3da5efabe45ad1c2299af23be01558fbf18ca2ecbc8efc60f210e1662cb606e47d16cfdfcdf44927428f1dd688ca26";

#endif  // SG_INTEG_H
