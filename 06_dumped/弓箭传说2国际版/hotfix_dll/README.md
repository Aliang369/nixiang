# 弓箭传说2国际版 / hotfix_dll — 证据说明（2026-10-17 收尾）

## 这是什么

`catalog.json`＝Addressables 目录（3.9 MB 文本，与设备
`/sdcard/Android/data/com.facai.archeroii/files/com.unity.addressables/catalog_2026.03.18.05.55.21.json`
同源）。保留它是因为它**回答了「热更程序集从哪来」**，是本轮挂点定位路线的复现依据：

- 热更 DLL **不在 APK 内**（APK 1626 条目中 0 个 `hotfix`/`updategroup` 条目）。
- 热更以 bundle 下发，catalog 内可见命名：
  - `updategroup_20260320064456_dll_hotfix.dll_9_assets_all.bundle`
  - `updategroup_20260320064456_dll_hotfixbattle.dll_12_assets_all.bundle` ← `HotFix.BattleLogic`
  - `updategroup_20260320064456_dll_hotfixfalcon.dll_10_assets_all.bundle`
  - `updategroup_20260320064456_dll_sdkhubv2.dll_8_assets_all.bundle`（另各有 `.mv` 变体）
- CDN 基址在 catalog 内为占位 `http://127.0.0.1/Android_CN_1.0.1.2_Release/…`，真实基址运行时注入。
- 下载落盘为 **UnityCache 哈希命名**（`files/UnityCache/Shared/<h1>/<h2>/__data`），
  `__info` 不含原始 bundle 名 ⇒ **无法按名反查**，且 bundle 压缩态 grep 不到类名。

## 已删除的部分（可再生，按归档正文 D 清理）

`block0.bin` / `hotfix_bundle.bin` / `stream.bin`（约 12 MB，UnityFS/LZ4，内含 BSJB 1 处但整包压缩）
——属官方 CDN 可重新下发的热更样本，非不可再生输入。

## 若将来仍想拿 HotFix.BattleLogic.dll

两条可行路线（都需重抓）：
1. 让游戏跑起来下载热更后，直接解 `files/UnityCache/Shared/**/__data`（UnityFS），在解出的
   TextAsset 里找 `.NET` 元数据（`BSJB`）。
2. 若 DLL 在下发链路上有加密，则转运行时反射路线（本轮实际采用并成功）：
   `il2cpp_class_get_methods`/`class_get_fields` + `image_class_count`/`image_class`
   枚举热更 image（2940 类 / 1349 个 `HotFix*` 命名空间类），用 `TraceDumpClass` 打成员。
   注意仓库硬约束：**install 期禁止调 `il2cpp_field_get_type`/`il2cpp_type_get_name`**（会 SIGABRT）。

本轮结论：**不需要这个 DLL 也能定位挂点**（运行时反射已足够，见 `10_docs/弓箭传说2国际版/功能落地_20261017.md`）。
