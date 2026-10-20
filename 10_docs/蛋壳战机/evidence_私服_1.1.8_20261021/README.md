# 蛋壳战机私服 1.1.8 星空版 — 2026-10-21 底包再更新轮 证据索引

对象：`02_apks/弹壳战机.apk`（私服 `com.wxdmxga.dkzj`，1.1.8 / vc35，200,582,246B，md5 `eba34100cbd01b67287617ddf44b252b`）
产物：`08_output/蛋壳战机/蛋壳战机_私服_1.1.8_starry-releaseSigned.apk`
（202,868,022B，md5 `12bc010e980bc31f87ece711bb34d370`，sha256 `1f584eb17e6c9ea2833b43ac5b2cbd7698ac4000228483c72018a3fc16fb46ab`，CN=StarryMagic）

## 底包变化与锚点结论

| 项 | 2026-10-18 轮 | 2026-10-21 轮 | 结论 |
|---|---|---|---|
| 整包 | 200,582,246B / `d840bb44…` | 200,582,246B / `eba34100…` | 同体积、不同内容 |
| metadata sha256 | `9d01f13e…` | `9d01f13e…` | **未变** |
| classes5/6/7 + liblgsgift | `d88e34d6…` / `798ccc0e…` / `f4bc35f5…` | 同 | **逐字节未变**（私服自有系统未动） |
| libil2cpp sha256 | `b40cad58…` | `4de22d52631c269a…` | 变了；**但 15 个 hook 的 file offset 命中且序言 4 字节与 kInsn 全符** ⇒ 零重定位复用 DkzjMod |

序言实测（新底包 `lib/arm64-v8a/libil2cpp.so`，8 字节）：

| 方法 | file offset | 实测 8B | 期望 kInsn |
|---|---|---|---|
| EntityCharacter.ChangeHp | 0x6C2A314 | `fe6fbba9 fa6701a9` | 0xA9BB6FFE ✓ |
| EntityHero.ChangeHp | 0x6D20180 | `fe0f1df8 f65701a9` | 0xF81D0FFE ✓ |
| HurtBase.FinalizeAttackResult | 0x6D39C44 | `fe0f1df8 f65701a9` | 0xF81D0FFE ✓ |
| EntityHero.set_CurrentHp | 0x6D227E0 | `fe0f1df8 f65701a9` | 0xF81D0FFE ✓ |
| PostProcessLearnedSkill | 0x6EE4654 | `ff0303d1 fd7b06a9` | 0xD10303FF ✓ |
| GameAdsManager.CheckAndPlayVideo | 0x43C9720 | `ff8301d1 fe6f01a9` | 0xD10183FF ✓ |
| IsNeedUploadSaveData | 0x74E1900 | `fe57bea9 f44f01a9` | 0xA9BE57FE ✓ |

（其余 8 个 set_ShowSkills / AddSkillToCharacter / GetSkill_Main / Game.get_Table / ADModuleMgr 序言同样命中，见 L2 日志 12/12。）

## L1（静态）

- `check_release_clean.sh --tag Dkzj --so …`：违规 0。
- `pre_install_gate.sh`：门禁串 3/3 存在、Manifest 为二进制 AXML、构建戳 `STARRYSTAMP1:EPOCH=1792438962`、
  **查9 so/metadata 与底包一致（4de22d52… / 9d01f13e…，未替换）**。
- 签名阶段守卫：原包 13 个受保护条目（classes.dex..classes7.dex、resources.arsc、libil2cpp/libunity/libmain/liblgsgift、
  global-metadata.dat）sha256 与底包逐一相同；surgical 阶段 2726 个原始条目字节不变。
- 装机：设备上该包此前已不存在（`pm list packages` 无 `com.wxdmxga.dkzj`）⇒ **全新安装**，
  设备 base.apk md5 == 产物 `12bc010e…`，运行 so md5 == 包内 so `07e05b83…`。

## L2（机器预检）

- `09_test/蛋壳战机/precheck_20261021_020723/`（**完整 logcat 不入库，按约定已清理**）
- 结论 **PASS_TO_HUMAN**：RegisterNatives 19/19、12/12 hook 命中、`[dkzjsf] 初始化成功`、
  `native menu shown mode=ACTIVITY`、`auth game_id synced from native: dkzjsf`、空转 3 min 零崩溃。
- 决定性行 → `decisive_l2_20261021.txt`

## L3（用户终审）

- 2026-10-21 用户实测反馈：**四项都正常**（攻击倍率 1-100 / 上帝模式 / 偷盗技能 / 免广告）⇒ L3 = PASS。

## 备注

- so 日志前缀仍为 `[dkzj]`（复用国服 DkzjMod，不影响功能；game_id 已是 `dkzjsf`）。
- 启动时一行 `[OTA] 补丁签名校验失败` 属 OTA 守卫对「无本地 OTA 补丁」的常规判定，非功能性阻断（与前几轮一致）。
- 构建脚本本轮新增：`04_scripts/蛋壳战机/dkzj_sf_build_so.sh`（`libs_dkzjsf`/`obj_dkzjsf` 在 10-20 收尾时被清理过）。
