#pragma once
/* 自动生成，勿手改：python3 04_scripts/汉克旅店/gen_hkld_payload_header.py
 * 汉克旅店 1.1.0 (vc337) 热更程序集补丁载荷（明文内嵌，无加固/无票）。
 * native 侧策略：运行时装配长度 == kHkldOfficialLen 才替换；否则 PASSTHROUGH 官方版。
 */
#include <stdint.h>

static const char* kHkldPayloadPolicy = "plain-inline";

/* 官方基线 Assembly-CSharp.dll：5328384 B */
static const uint32_t kHkldOfficialLen = 5328384u;
static const char* kHkldOfficialSha = "263e11ebfca73cfc3a014a4dbae170ffafb20c0a30bef4964fac1f8d92ca1d31";

/* 包内 bundle 版本（离线首启可能命中）。补丁载荷按运行时基线制作，
 * 命中该长度时同样替换，但日志会标 alt —— 两者内容不同，需真机复核。 */
static const uint32_t kHkldBundledLen = 5328896u;
static const char* kHkldBundledSha = "d2e7edf03f24f276db78d240e147ed3398263ee81923db47d9cb2e94ded33277";

/* 补丁产物：5292544 B（比官方 -35840 B）sha256=f1f698bd95ac9b23e88a00dcade3cc2ad6a210194b317f2f5b4d1508ccf1205b */
static const uint32_t kHkldPatchedLen = 5292544u;
static const char* kHkldPatchedSha = "f1f698bd95ac9b23e88a00dcade3cc2ad6a210194b317f2f5b4d1508ccf1205b";
