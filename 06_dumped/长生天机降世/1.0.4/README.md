# 长生天机降世 1.0.4 分析产物（收官后保留）

2026-10-28 收官：`06_dumped/长生天机降世/1.0.4` 只保留**文本证据**，其余大二进制已按
`10_docs/tools/05_目录结构脚本环境与交付清理.md`「收官归档清单」隔离清理（清单
`10_docs/长生天机降世/滚动物清理清单_20261028.json`）。

## 保留

| 路径 | 内容 |
|---|---|
| `decomp/decomp_hot_login.cs` | **运行时**热更 `Code.dll`（v1.0.4.5）里 `ET.DlgNetkuuLoginSystem` 的反编译原文 —— 「非法登录」单判据在第 460 行 |
| `decomp/decomp_ET_DlgNetkuuLoginSystem.cs` | APK 内置旧版 `Code.dll` 的同类型反编译（1.0.3 时代的「体积+三条黑名单」双条件，第 1404 行） |

## 已清理（可再生，重跑命令如下）

| 原路径 | 再生方式 |
|---|---|
| `static/libil2cpp.so`、`static/global-metadata.dat` | `unzip -p 02_apks/长生天机降世/长生天机降世_1.0.4_official.apk lib/arm64-v8a/libil2cpp.so > …`；metadata 同法取 `assets/bin/Data/Managed/Metadata/global-metadata.dat`（`04_scripts/长生天机降世/dump104.sh` 已脚本化） |
| `dump/dump.cs`（38 MB） | `cd 01_tools/il2cppdumper && dotnet Il2CppDumper.dll <libil2cpp.so> <global-metadata.dat> <out>` |
| `code_apk/`（APK 内 `assets/code.unity3d` 解出的 Code.dll/Code.pdb） | `D:/Python313/python.exe 04_scripts/长生天机降世/unpack_bundle.py <解出的 code.unity3d> <out>` |
| `code_hot/`（设备热更缓存解出的 10 个运行时 dll） | 设备侧 `…/files/yoo/DefaultPackage/CacheFiles/28/2881b558…/__data` → `04_scripts/长生天机降世/pull_hot_code.sh` 拉回 → 同上解包 |
| `manifests/`（Yoo 清单 v1.0.4.1/.5） | 设备同目录 `ManifestFiles/PackageManifest_*.bytes` 拉回 |
| `classes.dex`、`classes3.dex` | `unzip -p <官方 apk> classes.dex` 等 |

反编译工具（ILSpy 8.2，免安装）：`bash 04_scripts/长生天机降世/get_ilspy.sh`（下载 nupkg 到 `_cs104` 时代的临时目录并直接用 `dotnet ilspycmd.dll` 运行；脚本内 `ilspycmd_pkg` 路径按需重建）。
