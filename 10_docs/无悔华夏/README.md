# 无悔华夏 3.8.305（九游）— 无壳星光

> 入口一句话：九游 `com.yhhy.whrhx.aligames` 3.8.305（versionCode 34）无壳星光版，**路线 = 无壳重建**。
> 原包只有 1 份 58,140 B 壳 stub，业务 DEX 与明文 `libil2cpp.so` 只在运行时存在 ⇒ 换 dex 走不通，只能「取运行时明文对 → 捞内存业务 DEX → 本地编 mod so 与菜单 dex → 外科 zip 组装 → 卸载重装」。

## 文档导航

| 文件 | 内容 |
|---|---|
| [3.8.305_无壳星光_20261127.md](./3.8.305_无壳星光_20261127.md) | **唯一技术文档**：原包与壳结构、运行时明文对、业务 DEX、六段重建、RVA 与字段偏移、日志判据、装机验收 |
| [硬约束.md](./硬约束.md) | A 区约束（开工必读）：A#4 本游戏只许卸载重装、A#5 文档只写当前有效方法 |
| [台账.md](./台账.md) | append-only 记录：已成立 / 已否证 / 未验证 |
| [正式包裁剪清单_20261105.json](./正式包裁剪清单_20261105.json) | 九游（`whhx`）正式包裁剪清单 |
| [正式包裁剪清单_bili_20261106.json](./正式包裁剪清单_bili_20261106.json) | B 站（`whhxbili`）正式包裁剪清单 |
| [whhx_menu_template.json](./whhx_menu_template.json) | 菜单模板 11 项 · 九游 `whhx` |
| [whhxbili_menu_template.json](./whhxbili_menu_template.json) | 菜单模板 11 项 · B 站 `whhxbili` |
| [whhxkb_menu_template.json](./whhxkb_menu_template.json) | 菜单模板 6 项 · 快爆 `whhxkb` |

菜单项号（九游 / B 站同为 11 项）：`1` 无限金币 · `2` 无限食物 · `3` 无限军队 · `4` 无限人口 · `6` 瞬间胜利 · `8` 游戏加速(1–5) · `9` 科研立即完成 · `10` 政策技能无CD · `11` 名臣好感度 · `12` 名臣技能无CD · `13` 军队规模加成(1–100)。菜单文案由 native `g_featureList` 下发，dex 内无中文串。

## 关键入口命令

```bash
# 装机（唯一合规入口：force-stop → pm uninstall → 全新 install → 拉起）
WHRHX_APK=08_output/无悔华夏/无悔华夏_3.8.305_无壳星光_20261127-releaseSigned.apk \
  bash 04_scripts/无悔华夏/whrhx_clean_reinstall.sh --uninstall

# 组装（manifest 补 StarryApp/MenuService/StarryInitProvider + zip 合并 + zipalign -p 4）
bash 04_scripts/无悔华夏/whrhx305_unshelled_repack_20261127.sh

# mod so 构建（BUILD_OK + 字面量门禁）
bash 04_scripts/无悔华夏/whrhx_build_so.sh

# 菜单 dex（父类必须覆盖为 LBApplication，否则无壳后壳初始化丢失）
WHRHX_SUPER_CLASS='Lcom/excelliance/open/LBApplication;' WHRHX_DEX_NAME=classes15.dex \
  bash 04_scripts/无悔华夏/whrhx_build_latest_dex.sh
```

## 当前产物

```text
08_output/无悔华夏/无悔华夏_3.8.305_无壳星光_20261127-releaseSigned.apk
1,644,893,691 B / sha256 76709f25… / md5 e5724fdf… / 15 个 dex（14 份业务 + classes15.dex 菜单）
签名 v1/v2/v3 全过，CN=StarryMagic
```

## 三条不能忘

1. **装机只走 `--uninstall`**：覆盖安装保留乐变 `currVer` 且不重建负载 ⇒ 退回加密 so、32 条 RVA 全失效（硬约束 A#4）。
2. **明文 so 与 metadata 必须从运行时取**：磁盘 payload 优先，内存映射兜底；本版明文 so 与 B 站/4399 同版本一致，`WhrhxRvaGen.h` 零重标。
3. **菜单 dex 父类要显式覆盖**：脚本默认值是易盾父类，不覆盖则无壳后壳初始化丢失、菜单不起。
