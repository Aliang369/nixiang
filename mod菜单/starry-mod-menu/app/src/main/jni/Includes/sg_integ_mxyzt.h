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
    { "classes4.dex", 528880, { 0xe4, 0x2e, 0xa0, 0xf7, 0x20, 0x32, 0xc9, 0x11, 0xf4, 0xa1, 0xff, 0xf7, 0x18, 0xe0, 0xce, 0x8c, 0x89, 0xdf, 0x65, 0xef, 0x8b, 0x55, 0x6c, 0x65, 0x43, 0x70, 0x3c, 0x9d, 0xd3, 0x54, 0xa8, 0x9c } },
    { "assets/certs/starlight.crt", 1151, { 0x33, 0xbf, 0x4b, 0x50, 0x7a, 0x54, 0x48, 0x54, 0x91, 0x22, 0x3c, 0xce, 0xe0, 0x46, 0xd9, 0x48, 0x3a, 0xb6, 0x5f, 0x11, 0x94, 0x39, 0x79, 0xe6, 0x75, 0x55, 0x3d, 0x5e, 0x33, 0xf1, 0x8b, 0xfb } },
};
static const int kSgIntegCount = 2;

static const char kSgIntegSigHex[] = "a12c279b0c91828e9ec0a1423b773e8df528d8f12403e0374e5346c68256a4462260daf4eb521391fe39d4c070f93515e2b380e2fab667c6a757c0cd4b70552baea681954319c456221b7b6d10a5fa1d55b117e6ba29009dca9784d99a75ebfc7b8061d254ea2b28b1769a634712a4eff5f06b0dc455a6059347363bfce27c0d878aaa504915640e6867ad7c941f36d11621d0f157652d55401ecedb811190c69ef5cc0facf9ffcd22fa5487e92b5d42a48ca1f58de5c27cd2ff724a00e91f327453046ccef34f231e59bb96574cb4276272610bfaebcd65d8d199ffe5805e3746ee5d5044557a26cfa745d4c83a60230564fc28755f2afec4018a8ea3e51804";

#endif  // SG_INTEG_H
