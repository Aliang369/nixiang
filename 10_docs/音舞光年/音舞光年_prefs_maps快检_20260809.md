# 音舞光年 — prefs / maps 快检（2026-08-09）

> 包名：`com.uniugame.ywgn`（0.300.41）  
> APK：`02_apks/音舞光年/音舞光年_0.300.41_mod.apk`  
> 产物：`09_test/音舞光年/memdiff/`（未做完整 mem dump / il2cpp diff）

## maps

| 库 | 观察 |
|----|------|
| `libSDKloading.so` | **小壳** ≈3.88MB（设备 `3877368`），多段 r-xp |
| `libil2cpp.so` / `libunity.so` | 已加载（Activity：`com.uniugame.bridge.BridgeActivity`） |

## prefs 要点

| 文件 | keys |
|------|------|
| `shell_config.xml` | 空 map |
| `shell_protected.xml` | `last_url_index`, `last_url_source=post` |
| `shell_protectedother.xml` | `message_system_SystemText1834`, `html_Html5722` |
| `dialog_close_log.xml` | 斌哥条款 `SystemText1834` |
| `com.uniugame.ywgn.v2.playerprefs.xml` | `Screenmanager Fullscreen mode`, Resolution W/H, `install_id`, `First_Open_Time` |
| `umeng_*` | 统计 session / version |
| `AwOriginVisitLoggerPrefs.xml` / `WebViewChromiumPrefs.xml` | WebView |

完整：`09_test/音舞光年/memdiff/prefs_all.xml`、`maps_libs.txt`
