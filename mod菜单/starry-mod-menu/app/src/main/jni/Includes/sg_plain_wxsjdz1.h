#pragma once
/* 自动生成，勿手改：python3 04_scripts/无限升级_断网版/gen_plain_header_dz1.py
 * 无限升级·断网版 wxsjdz1（vc1647）明文补丁载荷常量。
 * 官方基线 1995776 B / fnv 0xaca5988be25f5119；补丁 2009600 B（超出官方数组 -13824 B）。
 * 超长 ⇒ native 侧自建数组注入（il2cpp_array_new_specific + pinned gchandle）。
 */
#include <stdint.h>
static const char* kWxsjdz1PlainPolicy = "plain-inline-selfarray";

/* 2009600 B，sha256 ec6a41e51cda78ae4511fa7c6c73aeb266a9b42de949e94de195f86011948cbc */
static const uint32_t kWxsjdz1PayloadLen = 2009600u;
static const char* kWxsjdz1PayloadSha = "ec6a41e51cda78ae4511fa7c6c73aeb266a9b42de949e94de195f86011948cbc";
static const uint64_t kWXSJDZ1_PATCHED_FNV = 0xee3f41c4ec9ef29aULL;
/* pad = 官方数组 - 补丁 = -13824（负数=需自建更大数组）*/
static const int32_t kWxsjdz1PayloadPad = -13824;
