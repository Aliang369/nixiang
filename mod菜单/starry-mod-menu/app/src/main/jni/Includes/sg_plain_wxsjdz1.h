#pragma once
/* 自动生成，勿手改：python3 04_scripts/无限升级_断网版/gen_plain_header_dz1.py
 * 无限升级·断网版 wxsjdz1（vc1647）明文补丁载荷常量。
 * 官方基线 1995776 B / fnv 0xaca5988be25f5119；补丁 2010112 B（超出官方数组 -14336 B）。
 * 超长 ⇒ native 侧自建数组注入（il2cpp_array_new_specific + pinned gchandle）。
 */
#include <stdint.h>
static const char* kWxsjdz1PlainPolicy = "plain-inline-selfarray";

/* 2010112 B，sha256 68424f32a49c5fd47ca7f7f916175a5849ea72411fd379c4e5869b3d381d75fb */
static const uint32_t kWxsjdz1PayloadLen = 2010112u;
static const char* kWxsjdz1PayloadSha = "68424f32a49c5fd47ca7f7f916175a5849ea72411fd379c4e5869b3d381d75fb";
static const uint64_t kWXSJDZ1_PATCHED_FNV = 0x8090fba5d572d3eeULL;
/* pad = 官方数组 - 补丁 = -14336（负数=需自建更大数组）*/
static const int32_t kWxsjdz1PayloadPad = -14336;
