#pragma once
/* 自动生成，勿手改：python 04_scripts/_common/gen_payload_enc.py --game wxsjdz2 */
/* 明文载荷 sha256 = 658f00c7339d365b824b636683a89aa78a991cbf3db62e7f9b7f147a468fc210（2 段拼接，共 4048896 B），各段密文分别内嵌（K_build 不在此文件内）*/
#include <stdint.h>
static const char* kWxsjdz2PayloadSha = "658f00c7339d365b824b636683a89aa78a991cbf3db62e7f9b7f147a468fc210";
static const int kWxsjdz2PayloadSegs = 2;
/* live：07_patched/无限升级/targetfuntional_wxsjdz2_patched.dll  2023424 B */
static const uint32_t kWxsjdz2LivePayloadLen = 2023424u;
static const uint8_t kWxsjdz2LivePayloadMac[32] = {220,82,214,92,64,66,21,108,231,17,98,255,95,130,57,250,246,79,132,129,178,216,89,164,215,5,108,18,90,74,121,240};
static const char* kWxsjdz2LivePlainSha = "979f9a03315314242e43a4c56c8372aa3afc2e33296d49f91ffd62b67185f9f9";
/* hf2：07_patched/无限升级/targetfuntional_wxsjdz2_hf2_patched.dll  2025472 B */
static const uint32_t kWxsjdz2Hf2PayloadLen = 2025472u;
static const uint8_t kWxsjdz2Hf2PayloadMac[32] = {82,99,64,168,1,228,184,42,108,215,133,166,15,245,180,199,138,97,164,238,143,10,131,133,76,87,141,230,177,138,132,94};
static const char* kWxsjdz2Hf2PlainSha = "783ea7ab65489d736b0207dee3ca5c815162ce4c7230a999cc2e2d0487c63e12";

/* 防封基线（无票也生效，不含任何作弊功能代码）：07_patched/无限升级/targetfuntional_wxsjdz2_antiban.dll  2021376 B */
static const uint32_t kWxsjdz2AntibanLen = 2021376u;
static const uint8_t kWxsjdz2AntibanMac[32] = {21,138,74,23,33,147,185,78,236,214,204,92,41,111,212,199,136,38,77,8,223,116,38,69,56,136,198,204,77,235,201,81};
static const char* kWxsjdz2AntibanSha = "0a468226870d02e83ecd72cb3db42adec76d94f7a7fcf376ed8190174540381d";
