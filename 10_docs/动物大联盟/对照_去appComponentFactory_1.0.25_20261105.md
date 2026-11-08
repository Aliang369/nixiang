# 对照：去掉 `android:appComponentFactory`（provider-only） — 动物大联盟 1.0.25 / code25

> 日期：2026-11-05（接手轮，主代理只做判断，设备动作走上一个子代理已建好的私有脚本）
> 对照包：`08_output/动物大联盟/动物大联盟_1.0.25_starry-env-provideronly-releaseSigned.apk`
> sha256 `9fd9fc0fdc4c4805d2b1e3bf34b1064bb40a84d72a7b54674e0976741647d5ad` / 85,402,750 B；基线 `8898e01d…9c12f`
> 唯一变量：Manifest 去掉新增的 `android:appComponentFactory`；DEX/SO/签名与基线相同（门禁 6 PASS 0 FAIL 3 SKIP、独立 verify 18/18）

## 0. 结论（分级）

| 项 | 结果 |
|---|---|
| **L2 机器验收** | **FAIL_A**（启动即崩，`post_install_precheck` rc=1） |
| **唯一变量的结论** | **工厂早加载被排除**：去掉 `appComponentFactory` 后崩溃帧、进程终态、Provider 缺类条数、壳 app-ready 计时全部不变 |
| **未排除（本轮不叠变量）** | ① Provider 阶段（`StarryInitProvider`）加载 native；② dex 注入；③ 重签 |
| 是否排除「native 可用性」 | 否 —— 本轮 `isolated 4B probe ok`、`watchdog active`、`wrote dirty=0@0ms` 与基线一致，**环境层仍正常** |
| 设备落地 | 同签覆盖（**未卸载、未清数据**），设备 base.apk 与包内 so 均 == 本地候选 |

## 1. 接手核对：上个子代理做到哪、失败在哪

- **已完成**（有产物，可直接引用）：v2 构建 + 门禁 + 独立 verify（`assemble_env_provideronly.json` / `gate_env_provideronly.json` / `artifact_verify_env_provideronly.json`）、差异证据（`09_test/动物大联盟/provideronly_20261105/diff_v1_v2_provideronly.json`、`manifest_application_tag.txt`）。
- **设备动作实际也已完成**（子代理未回报）：22:14:39 同签覆盖安装（`dumpsys lastUpdateTime`，firstInstallTime 仍是 21:53:18 ⇒ update 不是重装）→ 22:15:47 `post_install_precheck` 冷启一次（monkey 1 event）→ 22:15:54 崩溃、`precheck_l2_provideronly.json` 记 `PRECHECK_RC=1`。
- **真正的失败点＝回报与记录，不是游戏也不是设备**：子代理停在前一步的「接下来同签覆盖→一次冷启」话术上，没有把已发生的安装/启动/崩溃写成证据与文档；另有一处**记录错误**：`clean_install_dwdlm.sh --precheck-only` 在 P7 rc=1 时仍 `finish 0`，导致管线 `post_install_l2` 被记成 `done`（21:54:21）而实际是 FAIL（本轮已修，见 §5）。
- **本轮接手动作为纯只读**：0 次安装、0 次启动；仅 `pm path`/`dumpsys`/`su sha256sum`/`pidof`。

## 2. 设备上的实际效果（本轮只读复核，22:21）

- 设备 base.apk sha256 = `9fd9fc0f…7d5ad` == 候选（`apk_match=yes`）；包内 `lib/arm64/libstarry_magic.so` sha256 = `ea5ba328…849d17` == 本地 env so（`so_match=yes`）。
- `versionCode=25` / `versionName=1.0.25`；`firstInstallTime=2026-11-05 21:53:18`、`lastUpdateTime=2026-11-05 22:14:39`。
- `process=not-running`；外置资源根 `Android/data`、`Android/obb` 均 absent(0KB)（崩在资源下载前，**未新建资源根**）。
- 设备事实源：`04_scripts/动物大联盟/device_verify.txt`（`verify_device_state_dwdlm.sh`，只读）。

## 3. L2 判定与最内层 cause

**判定：FAIL_A**。最内层可用证据（不是 app 代码帧，是系统帧）：

```
E AndroidRuntime: FATAL EXCEPTION: main  (22:15:54.398, pid 27535)
E AndroidRuntime: java.lang.NullPointerException: Attempt to invoke virtual method
    'android.content.res.Resources android.content.Context.getResources()' on a null object reference
E AndroidRuntime:  at android.app.ConfigurationController.updateLocaleListFromAppContext(ConfigurationController.java:342)
E AndroidRuntime:  at android.app.ConfigurationController.handleConfigurationChangedInner(ConfigurationController.java:230)
E AndroidRuntime:  at android.app.ConfigurationController.handleConfigurationChanged(ConfigurationController.java:167)
E AndroidRuntime:  at android.app.ActivityThread.handleLaunchActivity(ActivityThread.java:4835)
```

- 证据：`09_test/动物大联盟/precheck_20261105_221547/chunk.48168.txt:237-243`（NPE 顶帧 :240，launch 入口 :243）。
- 含义（**实测**）：启动事务执行配置分发时 `ConfigurationController.mAppContext == null` ⇒ 系统侧 NPE。两条对照轮**同一帧、同一时序位置**。
- 时序（实测）：native 载入 `.372` → 14 条壳 stub 缺类 `.388-.393` → `phase app-ready 3817 ms` `.396` → `Ignoring finishAttachApplication` `.397` → CRASH `.398`。
- 旁证（实测）：`StubProvider class missing` 两组**都是 14 条、同一集合** ⇒ 这属 APKSecure 壳的正常输出，**不是**本轮差异，也不能当「真实 Provider 全缺失」的证据（此处更正上一轮报告的表述）。
- **推断（未证实，标待验证）**：壳 `dev.mars.secure.ProxyApplication` 接管 attach 时 appContext 未回填到 `ConfigurationController`，使 launch 事务在配置分发阶段拿到 null。

## 4. 单变量结论

- **已排除**：`android:appComponentFactory`（含其静态块在壳解密前抢先载入 native）不是该 NPE 的必要原因 —— 去掉后崩溃完全不变；provider-only 日志里 `DwdlmAppFactory` 出现 **0 次**，属性确已移除。
- **仍在嫌疑位、但本轮不动**（避免叠第二变量）：native 在 Provider 阶段被 `StarryInitProvider` 载入（`.372`，早于 Application.onCreate）；dex 注入；重签。
- **下一步建议（须用户重新授权出包+装机，本轮不执行）**：`provider-only + 空 Provider（保留 Manifest 结构、不载 native）` 单变量；或先做「官方原版在本机冷启日志」作为零基线，确认 14 条 stub 缺类在官方包同样出现。二选一，一次只做一个。

## 5. 本轮改动清单（只本游戏私有范围）

| 文件 | 改动 | 原因 |
|---|---|---|
| `04_scripts/动物大联盟/verify_device_state_dwdlm.sh` | **新增**（只读：包 hash / so hash / 进程 / 安装时间） | 接手需核设备落地；不手敲 adb，也不重复安装 |
| `04_scripts/动物大联盟/device_verify.txt` / `device_verify.json` | 新增产物 | 证据 |
| `04_scripts/动物大联盟/clean_install_dwdlm.sh` | ① `--precheck-only` 由 `finish 0` 改为 `finish "$PRECHECK_RC"`；② `finish()` 在 `MODE=plan` 时直接退出、不写 journal | **记录错误修复**：① 门禁 rc=1 却退出 0，管线把 FAIL 记成 done；② `--plan` 自称零写却覆盖 `clean_install.json/txt` |
| `04_scripts/动物大联盟/clean_install_reconstructed_20261105.json` | 新增（重建件，`reconstructed:true`） | 原 apply 记录被 `--plan` 覆盖，按报告 §4／`precheck_l2.json`／pipeline md5 重建，不冒充原件 |
| `09_test/动物大联盟/provideronly_20261105/l2_run_evidence.md` | 新增 | 最小日志切片 + 行号索引 |
| `10_docs/_state/games/动物大联盟.md`、`10_docs/_state/current.md` | 本轮摘要 + 索引行 | 状态维护 |
| 未改 | 任何 native/Java 源、`_common`/共享管线、其他游戏 | 边界 |

### 5.1 本轮操作事故与恢复（自查，如实记）

- **事故**：22:25 我跑 `clean_install_dwdlm.sh --plan` 想做「零写」回归，但该脚本 `finish()` 无条件写 journal，把上一轮 `clean_install.json/txt`（apply、`INSTALLED_VERIFIED_NOT_LAUNCHED`）覆盖成 `mode=plan / result=FAIL`（FAIL 只是因为该次候选默认 env、现装是 provider-only，P2 直接拦下，**设备零写入**）。
- **恢复**：① 覆盖后内容归档 `09_test/动物大联盟/provideronly_20261105/clean_install_overwritten_by_planrun_20261105.{json,txt}`；② 按报告 §4、`precheck_l2.json`、pipeline 记录的 md5 重建 `clean_install_reconstructed_20261105.json` 并标 `reconstructed:true`；③ 修 `finish()` 的 plan 分支，复测：`--plan` 输出计划后 journal mtime 不变。
- **影响**：不影响任何结论（安装/启动/崩溃均由 dumpsys、logcat、device_verify 独立佐证）；仅少了一份原始 journal 的字节级原件。

## 6. 本轮明确未做

- 未重新构建候选、未改 Manifest/DEX/SO；未卸载、未清数据、未备份、未移动资源。
- 未二次冷启（L2 已有一次冷启证据，重复启动会污染归因）；未截图、未 tap、未输入卡密、未外发鉴权。
- 未提交、未推送。

## 7. 证据索引

- 设备落地：`04_scripts/动物大联盟/device_verify.txt`
- provider-only 冷启：`09_test/动物大联盟/precheck_20261105_221547/{logcat.txt,monkey.log,chunk.48168.txt}`
- 本轮切片：`09_test/动物大联盟/provideronly_20261105/l2_run_evidence.md`
- 基线冷启：`09_test/动物大联盟/precheck_20261105_215414/{chunk.44904.txt,evidence_key_lines.txt}`
- 差异/门禁/verify：`09_test/动物大联盟/provideronly_20261105/diff_v1_v2_provideronly.json`、`04_scripts/动物大联盟/{gate_env_provideronly.json,artifact_verify_env_provideronly.json,precheck_l2_provideronly.json}`
