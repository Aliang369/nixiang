#pragma once
/* 自动生成，勿手改：python 04_scripts/_common/gen_payload_enc.py --game wxsj_vc1637 */
/* 明文载荷 sha256 = 097336613a86349205c150744a8985bb08257ccc63bfe554fecaf9fe839c8b18（3 段拼接，共 6080512 B），各段密文分别内嵌（K_build 不在此文件内）*/
#include <stdint.h>
static const char* kWxsjvcPayloadSha = "097336613a86349205c150744a8985bb08257ccc63bfe554fecaf9fe839c8b18";
static const int kWxsjvcPayloadSegs = 3;
/* hf2：07_patched/无限升级/targetfuntional_vc1637_hf2_patched.dll  2028032 B */
static const uint32_t kWxsjvcHf2PayloadLen = 2028032u;
static const uint8_t kWxsjvcHf2PayloadMac[32] = {19,207,164,133,233,227,202,229,179,229,169,254,138,141,239,143,176,85,195,188,235,249,49,7,191,172,164,102,100,11,248,58};
static const char* kWxsjvcHf2PlainSha = "3d04ec7b9af5da40f003c373769642a37e6338d8c041c9c58689f9ed613ce097";
/* hf4：07_patched/无限升级/targetfuntional_vc1637_hf4_patched.dll  2026496 B */
static const uint32_t kWxsjvcHf4PayloadLen = 2026496u;
static const uint8_t kWxsjvcHf4PayloadMac[32] = {214,37,214,63,229,136,74,64,104,145,231,248,13,133,120,181,98,48,0,147,112,255,157,78,62,255,77,219,3,90,234,181};
static const char* kWxsjvcHf4PlainSha = "dd569c7b2b73e63b7d02c23a635576743da1986d44ae3ee4d588542306df062c";
/* hf3：07_patched/无限升级/targetfuntional_vc1637_hf3_patched.dll  2025984 B */
static const uint32_t kWxsjvcHf3PayloadLen = 2025984u;
static const uint8_t kWxsjvcHf3PayloadMac[32] = {160,82,173,39,61,208,24,145,230,82,254,14,245,178,74,216,29,202,252,81,174,105,242,166,199,183,112,221,197,22,247,74};
static const char* kWxsjvcHf3PlainSha = "a5f5402960498a0ca0a8d3d809610b78a0dedcebc80a3c83027cfb5a11b80408";

/* 防封基线（无票也生效，不含任何作弊功能代码）：07_patched/无限升级/targetfuntional_vc1637_antiban.dll  2021376 B */
static const uint32_t kWxsjvcAntibanLen = 2021376u;
static const uint8_t kWxsjvcAntibanMac[32] = {207,57,38,76,237,233,160,69,44,186,153,68,116,105,112,224,1,9,92,180,177,178,226,7,61,15,210,66,222,254,80,114};
static const char* kWxsjvcAntibanSha = "0a468226870d02e83ecd72cb3db42adec76d94f7a7fcf376ed8190174540381d";
