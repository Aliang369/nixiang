# 真机批量取证（少往返、可复核）

> 📍 路径：`re-analyze/device-forensics-batch.md`
> 🎯 触发场景：需要在真机上「确认补丁装上没有 / 功能有没有生效 / 设备被改了什么 / 清理到什么程度」，
> 或者一次要问设备十个问题。伊莫 1.0.1（2026-09-30）会话复盘 P1-2 固化。

---

## 0. 总原则

> 🚨 **铁律（2026-10-02 定，疾风骑士踩过）：`logcat -c` 之前必须先落盘。**
> 任何 `logcat -c` / `adb logcat -c` 之前，**先**执行
> `adb -s $SERIAL logcat -d > /tmp/<game>_<round>.log`（`<round>` 从 `1` 起**递增，绝不覆盖上一轮**）。
> **证据文件是唯一事后可回溯物，`-c` 不可逆** —— 本轮先清后抓，把上一轮的决定性崩溃栈洗掉了，
> 之后只能靠回忆复述，白跑一次往返。
> 需要"从干净状态开始"时也**先落盘再清**：`adb -s $SERIAL logcat -d > /tmp/<game>_1.log && adb -s $SERIAL logcat -c`；
> 顺序颠倒即违规。落盘文件是本地可再生 scratch（不入库），决定性行再摘进
> `10_docs/{游戏}/evidence_日期/README.md`。

1. **一次 adb 往返拿一屏信息**：把多条查询塞进 `su -c '...'` 的一条 heredoc/多行命令里，
   别十次 `adb shell`（每次 ~200 ms，且中间状态会变）。
2. **取证先分类、再删除**：设备上混着「我们的测试物」和「用户自己的东西」，
   删之前必须能区分（见 §4）。默认只删能明确归因于本项目的文件。
3. **判据写全**：日志/截图/`dumpsys` 输出落到 `/tmp`，结论写进文档，
   别只把输出留在终端里——下一轮还得复现。
4. **`adb logcat -s <tag>` 是前缀匹配**：打印成 `[Yimo] xxx` 的日志，
   `-s Yimo:V` **永远匹配不到**。一律 `adb logcat -d | grep -E "..."`。（真金白银踩过）

---

## 1. 补丁是否装上 / 功能是否生效

```bash
# ① 进程 + 前台
adb shell "ps -A | grep -E 'yimoo|com.x.yimoo'" ; adb shell dumpsys window | grep -E "mCurrentFocus|mFocusedApp"

# ② 启动（不用手点）
adb shell monkey -p com.x.yimoo.official -c android.intent.category.LAUNCHER 1

# ③ 日志（★ 注意 -s 的前缀匹配陷阱；★ 清空前先落盘，见 §0 铁律）
adb -s $SERIAL logcat -d > /tmp/yimoo_1.log && adb -s $SERIAL logcat -c   # 先 dump 再 -c，顺序不可反
grep -E "\[Yimo\]|Mod_Menu|MenuService|StarryTapLogin" /tmp/yimoo_1.log

# ④ 清空「昨天的日志」再启动，才能证明"本轮不再写日志"
adb shell "rm -f /sdcard/Android/data/com.x.yimoo.official/files/ym.log"
```

判据分层：**装上**（框架日志 `patch installed (state=0x…)`、菜单条目注册日志）／
**注入成功**（补丁内自检行）／**功能生效**（业务日志或肉眼截图）。

---

## 2. 一条命令拿一屏设备状态（模板）

```bash
adb -s $SERIAL shell "su -c '
  echo == 进程 ==;      ps -A | grep -c yimoo
  echo == 内存/CPU ==;  top -n 1 -b -o %CPU | head -12
  echo == 网络 ==;      cat /proc/net/dev | grep -E \"wlan0|rmnet\"
  echo == 存储 ==;      df -h /data /sdcard | tail -3
  echo == tmp ==;       ls -la /data/local/tmp | head
  echo == 模块 ==;      ls /data/adb/modules 2>/dev/null
  echo == 证书 ==;      ls /data/misc/keychain/cacerts-added 2>/dev/null | wc -l
'"
```

`/proc/<pid>/task/*/stat` 看「到底哪个线程在烧 CPU」：伊莫登录界面单核 ~65% 是
Unity 自己的 `XJobSys::loop` 自旋线程，**不是**我们的 hook（`wlan0 RX=0` 也说明不是网络）。
先归因再优化，别急着改自己的代码。

---

## 3. 判断"改动到底是什么"

| 问题 | 命令 |
|------|------|
| 装的是哪一版 | `adb shell "pm dump <pkg> \| grep -E 'versionName\|lastUpdateTime'"` |
| 签名 CN 是谁 | `adb shell "dumpsys package <pkg> \| grep -i -A1 signature"` 或本地 `apksigner verify --print-certs` |
| 装上了但没生效 | 先看框架日志自检行，再看补丁日志；**"没装上" 与 "没命中" 是两回事** |
| 覆盖安装失败 | `INSTALL_FAILED_UPDATE_INCOMPATIBLE` = CN 不一致（原版 vs 改包）；只能卸载重装，**会清 `/sdcard/Android/data/<pkg>`（热更数据）** |
| 游戏数据还在不在 | `adb shell "ls /sdcard/Android/data/<pkg>/files \| head"` |
| 卸载能不能保住热更数据 | **能**——用 `pm uninstall -k --user 0 <pkg>`（保留 data/cache；2026-09-30 在 BlackDex 上实测：卸载后 T+0 与 T+3min 的 `/sdcard/Android/data/<pkg>/files/` 与标记文件都在，重装后仍复用同一目录）。**`pm uninstall`（不带 `-k`）会清掉**：伊莫第一轮就是这样丢了 12.98 GB 热更数据 |

---

## 4. 设备清理（分类 → 保护 → 删除）

```bash
# ① 盘点：按大小排序，看清都是什么
adb shell "su -c 'ls -la /sdcard | head -60'"
adb shell "su -c 'du -h -d 1 /sdcard/Download | sort -h | tail -20'"
adb shell /data/local/tmp … # 见下

# ② 只删"今天以前"的临时物（保护当天正在用的）
adb shell "su -c 'cd /data/local/tmp && find . -maxdepth 1 ! -newermt 2026-09-30 ! -name . -exec rm -rf {} +'"
```

**必须留手的（默认不删，先问用户）**：

- 用户自己的照片/录屏/回收站：`/sdcard/Pictures/Screenshots/.trashed-*`（伊莫机上 8.7 GB！）
- 用户下载的 APP/备份包：`/sdcard/Download/*.apk`、`*.zip`、`*.tar.gz`
- 菜单框架身份文件：`/sdcard/.starry_magic/device_id`
- 已安装工具（BlackDex 等）、游戏本体与其热更数据
- 系统目录（`/sdcard` 根下的 `Android/ DCIM/ Documents/ …`）

**可以删的**：本项目的测试物——`/sdcard` 根下的 `*.png` 截图/`*.xml` UI dump、
pull 出来的 so、`/sdcard/Download/` 里我们的脚本与抓包证书、
`/data/local/tmp` 的历史 scratch（伊莫机上 19 GB → 17 MB）。

清理前后各跑一次 `df -h /data` 记录回收量，写进会话复盘。

---

## 5. 常用一行速查

```bash
adb devices -l                                  # 设备与型号
adb -s $S -s …                                  # 多设备必须带 -s（仓库设备号复用，别搞错机器）
adb shell am force-stop <pkg>                   # 杀进程（改包验证前必做）
adb shell getprop ro.build.version.release      # Android 版本（厂商差异见 android-version-gotchas.md）
adb shell "su -c 'cat /proc/<pid>/status | head -5'"   # 进程 uid/gid（确认没跑在别的 profile）
adb logcat -d -v time | grep -E "FATAL|tombstone|DEBUG" | tail -50   # 崩溃取证
```
