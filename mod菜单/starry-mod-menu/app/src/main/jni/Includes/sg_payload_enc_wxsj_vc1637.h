#pragma once
/* 自动生成，勿手改：python 04_scripts/_common/gen_payload_enc.py --game wxsj_vc1637 */
/* 明文载荷 sha256 = dce70bf7e8f16d6d9d84a99c5d2ffba5ad7100739839e31d820330c9122518c4（3 段拼接，共 6080000 B），各段密文分别内嵌（K_build 不在此文件内）*/
#include <stdint.h>
static const char* kWxsjvcPayloadSha = "dce70bf7e8f16d6d9d84a99c5d2ffba5ad7100739839e31d820330c9122518c4";
static const int kWxsjvcPayloadSegs = 3;
/* hf2：07_patched/无限升级/targetfuntional_vc1637_hf2_patched.dll  2028032 B */
static const uint32_t kWxsjvcHf2PayloadLen = 2028032u;
static const uint8_t kWxsjvcHf2PayloadMac[32] = {36,25,226,239,22,23,36,85,238,109,20,81,54,37,139,181,169,188,36,138,97,142,107,190,51,49,11,62,243,236,226,204};
static const char* kWxsjvcHf2PlainSha = "3d04ec7b9af5da40f003c373769642a37e6338d8c041c9c58689f9ed613ce097";
/* hf4：07_patched/无限升级/targetfuntional_vc1637_hf4_patched.dll  2025984 B */
static const uint32_t kWxsjvcHf4PayloadLen = 2025984u;
static const uint8_t kWxsjvcHf4PayloadMac[32] = {123,133,186,10,112,250,223,191,184,21,147,20,159,242,170,191,229,189,38,118,69,180,173,16,47,44,209,164,132,130,43,201};
static const char* kWxsjvcHf4PlainSha = "d87a7f9419f8049fe0cae723356e4635f99e7f4717e8f4c4fd544121f7b62c3b";
/* hf3：07_patched/无限升级/targetfuntional_vc1637_hf3_patched.dll  2025984 B */
static const uint32_t kWxsjvcHf3PayloadLen = 2025984u;
static const uint8_t kWxsjvcHf3PayloadMac[32] = {182,221,164,180,194,76,150,155,71,175,14,11,234,127,47,51,114,160,92,138,133,9,32,201,143,2,182,106,177,49,197,122};
static const char* kWxsjvcHf3PlainSha = "a5f5402960498a0ca0a8d3d809610b78a0dedcebc80a3c83027cfb5a11b80408";

/* 防封基线（无票也生效，不含任何作弊功能代码）：07_patched/无限升级/targetfuntional_vc1637_antiban.dll  2021376 B */
static const uint32_t kWxsjvcAntibanLen = 2021376u;
static const uint8_t kWxsjvcAntibanMac[32] = {236,90,98,195,244,96,84,78,46,252,122,40,49,202,226,131,20,166,122,91,185,41,24,130,70,129,145,146,133,147,219,54};
static const char* kWxsjvcAntibanSha = "0a468226870d02e83ecd72cb3db42adec76d94f7a7fcf376ed8190174540381d";
