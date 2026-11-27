# 魂坠深境 — 文档索引

包名：`com.chillyroom.containerofsoul.yw` · 现行 `1.1.7` / vc45
成品：`08_output/魂坠深境/魂坠深境_1.1.7_starry-newmenu-releaseSigned.apk`（949,566,587 B，md5 `aeafe4bfc03c372e0bce69835be92027`）
源码：`mod菜单/starry-mod-menu/app/src/main/jni/Games/HzsjMod.{h,cpp}`（六功能运行时按名解析，正式包零 hook）
组装：`mod菜单/starry-mod-menu/scripts/surgical_hzsj_1.1.1.sh`
底包：`02_apks/魂坠深境/魂坠深境_1.1.7_device_official.apk`（947,889,995 B，md5 `4c8f1192…`，勿删）

## 必读

| 文档 | 用途 |
|------|------|
| [版本适配_1.1.7_20261127.md](./版本适配_1.1.7_20261127.md) | 唯一现行技术文档：底包指纹、出包三连与注入面、静态验证、货币/无敌两个重构、二进制判据、装机与 L3 |
| [路线锁定.md](./路线锁定.md) | 六功能挂点表（类/字段/写入值/运行期判据）与换版本必查三处 |
| [硬约束.md](./硬约束.md) | A/B 现行约束 + D 区技术事实（货币 999、无敌 maxHealth） |
| [hzsj_menu_template.json](./hzsj_menu_template.json) | 后台菜单模板（template_id=hzsj，六项 feat_num 1–5、7） |
| [轮次台账.tsv](./轮次台账.tsv) | 同一功能「改代码 → 装机 → 用户实测」逐轮结果 |
| [正式包裁剪清单_20261127.json](./正式包裁剪清单_20261127.json) | 唯一正式包的裁剪留档 |

## 脚本

| 路径 | 用途 |
|------|------|
| `04_scripts/魂坠深境/hzsj_build_dex.sh` | 菜单 dex（javac+R8，版本桩 1.1.7/45；须每次从 HEAD 重编） |
| `04_scripts/魂坠深境/hzsj_build_so.sh` | native so（私有 `Android_hzsj.mk`，产物 `libs_hzsj/arm64-v8a/libstarry_magic.so`） |
| `mod菜单/.../scripts/surgical_hzsj_1.1.1.sh` | 出包唯一入口：manifest 注入 + classes3 A 轨改包 + classes5/6 + so/libc++ + 签名对齐 |
