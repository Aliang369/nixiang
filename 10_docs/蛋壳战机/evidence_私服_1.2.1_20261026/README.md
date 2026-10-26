# 蛋壳战机私服 1.2.1 星空版 — 2026-10-26 证据索引

对象：`02_apks/弹壳战机.apk`（私服 `com.wxdmxga.dkzj`，1.2.1 / vc35，204,717,484 B，md5 `72cbc50b60d259297828d4c3de3e3e48`，CN=Android Debug）
产物：`08_output/蛋壳战机/蛋壳战机_私服_1.2.1_starry-releaseSigned.apk`
（207,015,548 B，md5 `f91c44b5a0adb6fac4ae8c36d6f07a32`，sha256 `8884dcfa40441e2c4ce7bf91dcc6b2d7f066baa68769da4e3e54a5c8ab13fd96`，CN=StarryMagic）

## 底包变化

| 项 | 1.1.8 私服 | 1.2.1 私服 | 结论 |
|---|---|---|---|
| versionName / vc | 1.1.8 / 35 | **1.2.1 / 35** | 版本名更新 |
| libil2cpp sha256 | `4de22d52…` | `ff308eea…` | **变化，必须重定位** |
| metadata sha256 | `9d01f13e…` | `def5f032…` | **变化** |
| classes5/6/7 | 旧版 | 5/7 变化、6 未变 | 私服自有系统逐字节保留新包版 |
| liblgsgift | `31eb84c0…` | `31eb84c0…` | 未变 |

## L1（静态）

- `check_release_clean.sh --tag Dkzj --so …`：PASS，违规 0、so 二进制干净。
- `pre_install_gate.sh`：PASS=5 FAIL=0；JNI 19/19、控件 0 冲突、Manifest 为二进制 AXML、so/metadata 与底包一致（`ff308eea…` / `def5f032…`）。
- 构建脚本 `dkzj_sf_build_so.sh`：菜单串 5/5、`dkzjsf`、`[dkzj]` 命中；反向坏串 4/4 = 0。
- 签名阶段：13 个受保护条目 sha256 与原包逐一相同；surgical 阶段 2729 个原始条目字节不变。
- 包内 so md5 `3c41bc9c9ac8ee15da7296988d368543` / 200,760 B；classes8.dex md5 `95019f4087f566477cd41341be4d72ee` / 571,068 B。

## L2（机器预检）

- 目录：`09_test/蛋壳战机/precheck_20261026_132606/`（完整 logcat 不入库，按约定保留在本机测试目录）
- 结论 **PASS_TO_HUMAN**：RegisterNatives 19/19、**12/12 hook 命中（全部为 1.2.1 新 file offset）**、`[dkzjsf] 初始化成功`、`native menu shown mode=ACTIVITY`、`auth game_id synced from native: dkzjsf`、空转 3 分钟零 SIGSEGV/tombstone/FATAL/ANR。
- 决定性行 → `decisive_l2_20261026.txt`
- 设备：`f9a116a2`（Android 16，arm64-v8a）；设备 base.apk md5 = 产物 `f91c44b5…`，运行 so md5 = 包内 so `3c41bc9c…`。

## L3（用户终审）

- ⏳ **待用户实测**：攻击倍率 1-100 / 上帝模式 / 偷盗技能 / 免广告。
- 本轮为全新安装（设备上此前无该包），私服账号需重新登录；未创建数据备份（AGENTS 默认不做）。

## 备注

- 1.2.1 的锚点与字段偏移见 `10_docs/蛋壳战机/抓点_锚点_1.2.1_20261026.json` 与 `私服拆解/03_1.2.1底包差异与锚点重定位_20261026.md`。
- `DkzjMod.cpp` 同时保留 1.1.8 旧表（`#else`）；私服 1.2.1 通过 `Android_dkzjsf.mk` 的 `-DDKZJ_TARGET_1_2_1` 选择新表。
