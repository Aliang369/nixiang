# 雷霆：去掉 PC Florida 晚载（设备端自动出菜单）

> 日期：2026-08-16  
> 状态：**真机已验收**（冷启 → watch ≥48s → `ltzj_ld` 一次性 inject → 菜单/卡密 OK）  
> 前提：Root；已装 zip-assets-menu 包；**不**常驻 florida-server、**不**需电脑 `frida -U`

---

## 交付主线（本机）

```bash
# 一次安装（推 inject + watch + 战斗 so）
bash 04_scripts/雷霆战机/ltzj_install_device_autoload.sh

# 之后：冷启游戏，过隐私进界面，约 50s 菜单自弹
# 日志：adb shell su -c 'tail -30 /data/local/tmp/ltzj_autoload/watch.log'
```

| 文件 | 作用 |
|------|------|
| `ltzj_install_device_autoload.sh` | 推工具、种子、拉起 watch |
| `ltzj_device_autoload_watch.sh` | 等 pid 稳定 + `libcocos` → 注入一次 |
| `ltzj_autoload_load.js` | `Module.load` + `JNI_GetCreatedJavaVMs` → `JNI_OnLoad` |
| `ltzj_ld`（设备上） | florida-inject 17.9.1 改名，**无**常驻 server |

战斗 so 优先：`$REMOTE_DIR/libstarry_magic.so`（安装脚本从 intermediates 推），避免 APK assets 旧 blob 盖掉。

---

## 验收证据（2026-08-16 18:18）

```
watch: SUCCESS maps has starry
inject: JNI_OnLoad rc=65542
StarryMagic: grant ok
Mod_Menu: feature #1/#2/#3 … ace_remain=57s
NativeMenu: heartbeat ok
auth game_id synced from native: ltzj
```

无电脑 attach；`ltzj_ld` 跑完由 watch 杀掉，不留 inject 进程。

---

## 仍禁止 / 仍非目标

| 项 | 状态 |
|----|------|
| 纯 APK 冷启自动 `JNI_OnLoad`（JS `System.load` / 改 dex / so 进 `lib/`） | **仍败**（ACE） |
| PC `ltzj_late_load_starry.sh` + florida-server | **开发旁路**，非主线 |
| 无 Root 单包直装出菜单 | **未解** |

---

## 踩坑（本轮）

1. `florida-inject -e`（eternalize）→ `timed out while waiting for stop`（ACE 吞 stop）  
2. 不带 `-e` 时 inject 常挂着不退 → watch **限时 kill**  
3. `Java.perform` 在 inject 上不跑 / 挂起 → 改用 **`JNI_GetCreatedJavaVMs`** 取 `JavaVM*`  
4. watch `seed_from_apk` 曾用 assets 旧 so（141600）盖掉战斗 so（136656）→ 现优先 DIR override  
5. Python `frida.create_script` 有时无 `Java` 全局；验收用 inject / frida CLI

---

## 停用 / 清理

```bash
adb shell su -c 'kill $(cat /data/local/tmp/ltzj_autoload/watch.pid); rm -f /data/local/tmp/ltzj_autoload/watch.pid'
# 会话结束（可保留 watch 若还要玩）：
./04_scripts/_common/cleanup_device_re_residue.sh   # 勿删 starry_magic 设备指纹
```
