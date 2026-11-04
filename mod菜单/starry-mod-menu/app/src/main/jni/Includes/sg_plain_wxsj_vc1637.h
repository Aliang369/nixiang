#pragma once
/* 自动生成，勿手改：python 04_scripts/无限升级/gen_plain_header.py
 * 无加固形态：so 内嵌**明文**补丁 DLL（2026-11-04 用户定：加固与载荷 sha 全去）。
 * 变体按运行时下发的官方 DLL 长度 + fnv 选择，选不到一律 PASSTHROUGH 官方。
 */
#include <stdint.h>
static const char* kWxsjvcPlainPolicy = "plain-inline";

/* hf2：targetfuntional_vc1637_hf2_feat3.dll  2028544 B（官方数组 2030080，余量 1536）*/
static const uint32_t kWxsjvcHf2PlainLen = 2028544u;
static const char* kWxsjvcHf2PlainSha = "d16a22c0b5d36c4ea07a89dae8714726b9cba9bd509b5f1708324c0e8cce8f8a";

/* hf4：targetfuntional_vc1637_hf4_feat3.dll  2027008 B（官方数组 2028544，余量 1536）*/
static const uint32_t kWxsjvcHf4PlainLen = 2027008u;
static const char* kWxsjvcHf4PlainSha = "cf00c116632de590cea6b89950a82c08c78c0af37e21d0c26249229ffdcb3413";

/* hf3：targetfuntional_vc1637_hf3_feat3.dll  2027008 B（官方数组 2028544，余量 1536）*/
static const uint32_t kWxsjvcHf3PlainLen = 2027008u;
static const char* kWxsjvcHf3PlainSha = "e3a29c294da8eb408aa47aa5fd6f9110186f4a7e550610585d528625c174a6bb";

/* 三份明文合计 6082560 B */
