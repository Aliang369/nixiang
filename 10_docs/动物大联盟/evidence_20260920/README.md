# 动物大联盟 1.0.23 决定性证据（2026-09-20）

本目录只保留复现「登录页达成」结论所需的最小证据。原始冷启动长日志
（81MB 等）与中间探针日志已按会话收尾清理，需要时可重新抓取。

## 决定性证据

| 文件 | 说明 |
|---|---|
| `login_reached_evidence_20260920.txt` | 关键行摘录：`evalString splice` 命中、三份 `index.jsc` 加载、`version_info`/`version_update_md5` 均 `code:0`、`MOD 已就绪 ready=1` |
| `logcat_probe_20260920_1422.txt` | 探针日志：壳状态归一化后无 `SecShell_Require+228 -> _exit(0)` |
| `logcat_reopen_clean_20260920.txt` | 重开复核：`force-stop` 后重拉 PID `3041` 完整到登录页，无 FATAL/SIGSEGV/ANR |
| `native_backtrace_20260920.txt` | 原生闪退期回溯（`resourceEntry` AAPCS64 sret 问题），用于说明该根因已修 |
| `window_pid31460.xml` / `window_reopen_clean.xml` | 窗口层级：前台为 `org.cocos2dx.javascript.AppActivity` |

## 最短复现

```bash
adb -s f9a116a2 logcat -c
adb -s f9a116a2 shell am force-stop com.fightingstars.games.animal
adb -s f9a116a2 shell monkey -p com.fightingstars.games.animal -c android.intent.category.LAUNCHER 1
adb -s f9a116a2 logcat -d | grep -E "evalString|MOD 已就绪|FATAL|SIGSEGV"
```

前置条件：需已安装本轮星光候选包。本作已终止，原包、候选包与
`04_scripts/动物大联盟/` 构建脚本均已删除（脚本可从 Git 历史恢复，参考提交 `dcb02f8`）；
若要重启需重新获取官方 1.0.23 包并重建环境。

## NOT PROVEN

- 六项功能（攻击倍率/上帝模式/无限技能/自动战斗/移速/免广告）的对局效果。
- 菜单运营授权 `grant rejected` 之后的开关放行。
- 结算、联机与服务端风控接受度。
