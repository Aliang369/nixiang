# evidence_20261005_自动战斗回退 — 决定性证据

## 结论

mac 侧四级指纹试验（为兼容模拟器旧 res）整体回退，交付恢复 **真机可用两级指纹基线**；
回退产物与 2026-10-05 16:23 真机实测通过的那一版**逐字节一致**（so 与正式包 md5 均同值）。

## 决定性证据

| # | 证据 | 入口 |
|---|---|---|
| 1 | 回退后 so 重建 md5 = `da8784699193b0f81208334a23aaed86`，与 16:23 真机通过版一致 | `04_scripts/勇士之心/ysxz_build_so_103.sh` 输出（本目录 diff 为四级试验、已作废） |
| 2 | 回退后正式包 md5 = `046d04f82a60f94c75055d90271b96cf`；设备 `f9a116a2` 装机后 base.apk md5 同值 | `08_output/勇士之心/勇士之心_1.0.3_starry-final-releaseSigned.apk` |
| 3 | 装机预检三行：菜单 so 加载 / `native menu shown` / `auth game_id synced` | `09_test/勇士之心/precheck_20261005_170931/logcat.txt` |
| 4 | 用户实测终审：「功能没问题了，我把游戏关了」（2026-10-05 17:1x） | 会话消息 |
| 5 | 四级试验完整 diff（已作废，仅作 win 侧参考） | `自动战斗四级指纹试验_20261005.diff`（本目录） |

## 最短复现（回退验收）

```bash
bash 04_scripts/勇士之心/ysxz_build_so_103.sh          # 期望 so md5 da8784699193b0f81208334a23aaed86
STAGE=20261005 ORIG_APK="$PWD/02_apks/勇士之心_1.0.3.apk" \
  bash 04_scripts/勇士之心/ysxz_release_103.sh          # 期望包 md5 046d04f82a60f94c75055d90271b96cf
adb -s f9a116a2 install -r 08_output/勇士之心/勇士之心_1.0.3_starry-final-releaseSigned.apk
```

## NOT PROVEN

- 模拟器（MuMu）侧自动战斗仍失效：两级指纹在旧 res 落空已离线复现，但**修法未验证**（移交 win，
  见 `../win移交_自动战斗双端兼容_20261005.md`）。
- 四级试验在真实模拟器上的失败点（哪一级落空 / 目标函数是否在被扫描的类里）未取到运行时日志；
  仅知 09-30 与 10-05 两份离线语料四级全 OK。
