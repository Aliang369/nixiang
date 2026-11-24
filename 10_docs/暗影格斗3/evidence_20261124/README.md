# 暗影格斗3 1.45.3 星光版复刻轮 — 逐段证据（2026-11-24，雷电模拟器）

本目录是 `13_scratch/sf3_remake/`（临时草稿区，不入库）里 7 份逐段报告的**入库副本**，
逐字节相同（md5 见下表）。原目录在收尾时被清理，本目录是唯一留存证据。

范围：雷电 `emulator-5554` 上重做星光包 → 三挂点静态校验 → 出包签名 → 装机 → 4 次冷启 → game_id 改 `aygd3` → L3。
结论口径与指纹汇总见 `10_docs/暗影格斗3/星空落地_1.45.3_20261124.md`（当前有效）；本目录只放逐段命令与原始输出。

## 报告索引

| 文件 | md5 | 大小 | 内容 |
|---|---|---|---|
| `00_baseline.md` | `f93edb9ab4cad758c23522524b0e17b3` | 13,640 B | 设备/底包/指纹对照（底包与 2026-09 归档逐字节同一份） |
| `01_static.md` | `fa1683b4415e42e47d6b5d6c9e195f72` | 23,170 B | v39 dump、三挂点序言逐字节校验、Java 四锚点 |
| `02_build.md` | `fe329e4bf77f0a9138c1ecf71f2e9d49` | 17,200 B | so / classes8.dex / 外科组装 / 签名 |
| `03_verify.md` | `1359d39feee2bf3558e9945b8aab2fb1` | 23,568 B | 独立验收 8 PASS / 1 FAIL / 1 UNPROVEN（FAIL＝12 字节序言死代码） |
| `04_install.md` | `2f63f3d52c68e34bf81d55737c37922c` | 12,393 B | 卸载官方包 → 全新装机 → 3 次冷启 |
| `05_prologue_fix.md` | `f6ac60a39c67f6cdc4fc3992cb9f93d3` | 16,168 B | 序言加固修复 v2（三证）＋ 覆盖安装 ＋ 4 次冷启 |
| `06_gameid_aygd3.md` | `ea907ceeda9a8722cf032c588ee98f9e` | 9,332 B | game_id 改 `aygd3`（`.authGameId`）与最终正式包 `a06cbff0…` |

## 关键输入指纹（复现用）

- 底包 `02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk`：2,713,267,683 B，md5 `7ac7591436e92136f22b715c91b5b930`，sha256 `1b17ab1c1503dbf5eee3aeb1917948e1c5e80f3ce0b3c3a6c4d682966d166087`
- 包内 `lib/arm64-v8a/libil2cpp.so`：127,366,992 B，md5 `53345cdb4d31dff336fdd575fb87b667`
- 包内 `assets/bin/Data/Managed/Metadata/global-metadata.dat`：26,448,516 B，md5 `809ec525f0875c2092dc7f21e73fb288`（metadata version **39**）
- dump 工具：`xy0-dev/Il2cppDumper_v39` @ commit `cff612af1331bf46d1eabb2390f8a46dbec7ea27`
- `dump.cs`：50,644,804 B / 1,448,586 行 / sha256 `3860c32a595f58e9eccb781b2c0e680eb3a67389dfa246e947b5ecc5a617e5ee`
- 最终正式包：md5 `a06cbff00dc7204574f240fa186edbcd`，sha256 `3f748503a8356052bd73a85a54ef1c3b3d8e8943450dd758ce299fe0592ab14c`，so `e0f31dc163d00bb7d029f75e9985b03f`

复现判据：重跑 dump 后 `dump.cs` 的 sha256 必须仍为 `3860c32a…e5ee`；否则说明底包或工具版本漂移。
