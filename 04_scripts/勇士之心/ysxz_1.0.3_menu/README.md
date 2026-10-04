# 勇士之心 1.0.3 菜单源码（新菜单版）

这份目录就是**1.0.3 用的菜单源码**，2026-10-04 由旧菜单换成与 1.0.4 一致的新序号。

## 为什么单独放一份

- `mod菜单/starry-mod-menu/app/src/main/jni/` 里那份是 **1.0.4 主线**源码（含过检测）。
- 1.0.3 的出包脚本 `scripts/surgical_ysxz_1.0.3.sh` **不重编 .so**，它只组装 dex，
  `.so` 直接读预置产物 `libs_ysxz/arm64-v8a/libstarry_magic.so`（该目录被 `.gitignore` 忽略）
  ⇒ 换机器后这份 so 不存在，或残留 1.0.3 时代的旧菜单 so。
- 所以把「1.0.3 源码 + 新菜单表」这三个文件入库，配合
  `04_scripts/勇士之心/ysxz_build_so_103.sh` 可复现地重建 1.0.3 的 so。

## 内容

基线：git 提交 `81c70279`（勇士之心 1.0.3 星空版交付）的
`mod菜单/starry-mod-menu/app/src/main/jni`，只改下面 3 个文件：

| 文件 | 改动 |
|---|---|
| `YsxzEntry.cpp` | `kYsxz_Features[]` 换成 7 行新表（1 号位 `Slider_伤害倍率:1:50:1`） |
| `YsxzMod.h` | `enum YsxzFeat` 同步序号（DAMAGE=1…AUTO=6，`YSXZ_FEAT_COUNT=7`） |
| `YsxzPatchJs.h` | JS 侧序号 `feature1=R(2)…feature5=R(6)`、`speed=V(5)`、`damage=V(1)` + 伤害倍率乘算 |

**不包含** 1.0.4 的任何过检测改动（`YsxzSecShell`、`YsxzExitTrace`、`NoAceStubs`、
`YsxzAppComponentFactory`、`SignSpoof` 等一律不搬）。

## 重建

```bash
bash 04_scripts/勇士之心/ysxz_build_so_103.sh
```

产物 `libs_ysxz/arm64-v8a/libstarry_magic.so`：139160 B，
MD5 `724095fda27414b10e28dfdba5a01884`（与已在 MuMu 实测通过的那份逐字节相同）。
