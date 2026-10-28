# 绿梦：E:\_build 迁入与清理清单（2026-10-28）

> 缘起：用户 2026-10-28 指令「E:\_build（6.4 G，绿梦）是迁进工作区，然后清理，到工作区后，按照清理规范在进行清理缩减」。
> 规范依据：`10_docs/tools/05_目录结构脚本环境与交付清理.md`「清理规则」「收官归档清单」（可再生滚动物删、源码/文档永留、08_output 裁剪需具体口令）。

## 落点（迁入后）

| 原位置 | 现位置 | 内容 |
|---|---|---|
| `E:\_build\lvmeng\*.c/*.py/*.sh/*.txt`、`assemble.py`、`classes2_patched.dex`、`payload2___*`、`jitblk.bin`、`dllman.bin`、图标 | `04_scripts/绿梦/probes_20261028/` | FairGuard cage 探针源码与构建输入（自写工具，按规范保留） |
| `E:\_build\lvmeng_mod\StarryLvmeng.csproj` 等 | `04_scripts/绿梦/lvmeng_mod/` | C# 注入工具工程 |
| `E:\_build\lvmeng\lvmeng_v25.apk`、`lvmeng_v32.apk` | `08_output/绿梦/_import_20261028/` | 现存候选包（v24 已不在磁盘，文档口径过期） |
| `E:\_build\lvmeng\ft3.log`、`ft4.log` | `09_test/绿梦/_import_20261028/` | 过程日志（不入库） |
| `E:\_build\dotnet\`（727 M，.NET SDK）、`dotnet-install.ps1` | `01_tools/dotnet/sdk-9/`、`01_tools/dotnet/` | 环境工具，非游戏产物；不属 01_tools 清理白名单，保留 |

## 已删（可再生滚动物，逐条列出）

| 相对路径 | 体积 |
|---|---|
| `lvmeng/lvmeng_cage_v1.apk.idsig` | 1400 KB |
| `lvmeng/lvmeng_cage_v2.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v3.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v4.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v5.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v6.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v7.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v8.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_cage_v9.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v10.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v11.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v12.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v13.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v14.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v15.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v16.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v17.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v18.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v19.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v20.apk.idsig` | 2780 KB |
| `lvmeng/lvmeng_v21.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v22.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v23.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v24.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v25.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v26.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v27.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v28.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v29.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v30.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v31.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v32.apk.idsig` | 2864 KB |
| `lvmeng/lvmeng_v33.apk.idsig` | 2864 KB |
| `lvmeng/orig_dec` | 509259 KB |
| `lvmeng/osmali1` | 126230 KB |
| `lvmeng/osmali2` | 128464 KB |
| `lvmeng/osmali3` | 24316 KB |
| `lvmeng/official_dex` | 16872 KB |
| `lvmeng/starry_dec` | 13840 KB |
| `lvmeng/assets` | 188357 KB |
| `lvmeng/cagetest2_arm64` | 8 KB |
| `lvmeng/cagetest2_x64` | 8 KB |
| `lvmeng/cagetest_arm64` | 12 KB |
| `lvmeng/cagetest_x64` | 8 KB |
| `lvmeng/ct10` | 12 KB |
| `lvmeng/ct11` | 8 KB |
| `lvmeng/ct3_arm64` | 8 KB |
| `lvmeng/ct3_x64` | 8 KB |
| `lvmeng/ct4_arm64` | 8 KB |
| `lvmeng/ct4_x64` | 8 KB |
| `lvmeng/ct5_arm64` | 8 KB |
| `lvmeng/ct6_arm64` | 8 KB |
| `lvmeng/ct7_arm64` | 8 KB |
| `lvmeng/ct8_arm64` | 8 KB |
| `lvmeng/ct9` | 12 KB |
| `lvmeng/fgdump` | 8 KB |
| `lvmeng/fgexit` | 12 KB |
| `lvmeng/fgjit` | 12 KB |
| `lvmeng/fgpoke` | 12 KB |
| `lvmeng/payload2___data` | 12904 KB |
| `lvmeng/payload2___info` | 1 KB |
| `starry` | 147203 KB |
| `ilpatch/bin` | 573 KB |
| `ilpatch/obj` | 277 KB |
| `ilpatch/Assembly-ACT.p2.dll` | 14244 KB |
| `ilpatch/rt.dll` | 14136 KB |
| `lvmeng/fg_dump.bin` | 4336 KB |
| `lvmeng/fg_after.bin` | 4336 KB |
| `lvmeng/fg_live.bin` | 4336 KB |
| `lvmeng/host_libc.so` | 1308 KB |
| `lvmeng/host_houdini.so` | 9184 KB |
| `lvmeng/libtapsdkcore.so` | 4116 KB |
| `lvmeng/aligned.apk` | 362300 KB |
| `lvmeng/lvmeng_cage_v1.apk` | 176248 KB |
| `lvmeng/lvmeng_cage_v2.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v3.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v4.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v5.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v6.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v7.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v8.apk` | 351688 KB |
| `lvmeng/lvmeng_cage_v9.apk` | 351688 KB |
| `lvmeng/mf_only.apk` | 175428 KB |
| `lvmeng/unsigned.apk` | 362300 KB |
| `lvmeng/dexdec/（smali 反编译输出，可再生）` | 3980 KB |

## 保留 APK 身份

| 体积 | 文件 | md5 |
|---|---|---|
| 370704022 | `lvmeng_v25.apk` | `md5=fdda9d83f1669fb376a7d2fe2ea4a8d4` |
| 371105430 | `lvmeng_v32.apk` | `md5=775f8017babcd182a586d3c8492c27a7` |

## 未做/待裁决

- `08_output/绿梦/_import_20261028/` 仍有两个候选包：按规范「08_output 裁剪为唯一正式 APK」需用户「收尾+正式包+清理」口令，本轮未裁。
- 绿梦文档里「当前交付候选 = `E:\_build\lvmeng\lvmeng_v24.apk`」的口径已过期（v24 不在磁盘），本轮只把路径改成新位置，不替绿梦判正式包。

## 补充：构建输入归档（assemble.py 直接引用的那一份，按规范保留）

`04_scripts/绿梦/build_inputs_20261028/` 内含：

`smali_in/`（菜单注入 smali 树）、`lib/arm64-v8a/{libstarry_magic.so,libc++_shared.so}`、`classes4.dex`（+3 份 .bak）、`classes2_patched.dex`、`payload2___data/__info`、`jitblk.bin`、`dllman.bin`。
判据：`assemble.py` 第 92 行 `for need in ["classes4.dex", "lib/arm64-v8a/libstarry_magic.so", "AndroidManifest.xml", "assets/lvmeng/fg_orig.apk"]` 与第 12 行 `classes2_patched.dex`。
体积：5.8M

## 补充：Windows 侧 ilpatch 工程变体

E 盘副本的 `ilpatch.csproj` 与工作区模板不同（net10.0→net8.0、改本地 `01_tools/il2cppdumper/Mono.Cecil.dll` 引用、加 AllowUnsafeBlocks），已存为 `04_scripts/绿梦/ilpatch/ilpatch.win.csproj`，其余 4 个 .cs 与工作区逐文件 md5 相同。

## 最终落点与体积（2026-10-28）

| 落点 | 体积 | 内容 |
|---|---|---|
| `04_scripts/绿梦/probes_20261028/` | 677K | FairGuard/cage 探针源码（*.c/*.py/*.sh）、地址与笔记 txt、`assemble.py`、`patch_*.py`、图标 |
| `04_scripts/绿梦/build_inputs_20261028/` | 20M | `assemble.py` 直接引用的构建输入：`smali_in/`、`lib/arm64-v8a/*.so`、`classes4.dex`(+3 bak)、`classes2_patched.dex`、`jitblk.bin`、`dllman.bin` |
| `04_scripts/绿梦/ilpatch/ilpatch.win.csproj` | 617 B | Windows 变体工程（net8.0 + 本地 Mono.Cecil） |
| `08_output/绿梦/_import_20261028/` | 708M | `lvmeng_v25.apk`(370,704,022 B, md5 `fdda9d83f1669fb376a7d2fe2ea4a8d4`)、`lvmeng_v32.apk`(371,105,430 B, md5 `775f8017babcd182a586d3c8492c27a7`) |
| `09_test/绿梦/_import_20261028/` | 3.7 M | `ft3.log`/`ft4.log` 过程日志 |
| `01_tools/dotnet/` | 728 M | `.NET SDK`（`sdk-9/`）与 `dotnet-install.ps1`，环境工具 |

**缩容结果**：`E:\_build` 原 6.4 G → 工作区保留 ≈ 0.75 G（dotnet 728 M 属环境工具 + 候选包 708 M），删掉的可再生滚动物 ≈ 5.6 G（33 个 .idsig、7 个反编译/解包目录、11 个候选 APK、整份 mod 工程副本 144 M、ilpatch 构建输出、内存 dump 与设备库）。

**注**：`payload2___data`（13 M）在「无扩展名＝编译产物」规则下被删；它由 `make_payload.sh` 从源码+官方 dll 重出，属可再生（规范 §200「热更样本/补丁产物可删」）。
