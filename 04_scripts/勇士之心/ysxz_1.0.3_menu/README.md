# 勇士之心 1.0.3 菜单源码（新菜单版）

> 2026-10-05 追加（两轮）：本目录含第 4 个文件 `YsxzMod.cpp`（x 段优先 → 可读段兜底 +
> main.js 自愈 + `files/ysxz_native_ok` 生效证明）；另含 `boot_ids.json` 与自举素材生成链
> （`04_scripts/勇士之心/{gen_ysxz_boot_js.py,ysxz_mainjs_patch.py}`，由 `ysxz_build_so_103.sh` 调用）。

这份目录就是**1.0.3 用的菜单源码**，2026-10-04 由旧菜单换成与 1.0.4 一致的新序号。

## 为什么单独放一份

- `mod菜单/starry-mod-menu/app/src/main/jni/` 里那份是 **1.0.4 主线**源码（含过检测）。
- 1.0.3 的出包脚本 `scripts/surgical_ysxz_1.0.3.sh` **不重编 .so**，它只组装 dex，
  `.so` 直接读预置产物 `libs_ysxz/arm64-v8a/libstarry_magic.so`（该目录被 `.gitignore` 忽略）
  ⇒ 换机器后这份 so 不存在，或残留 1.0.3 时代的旧菜单 so。
- 所以把「1.0.3 源码 + 新菜单表 + 可读段兜底」这四个文件入库，配合
  `04_scripts/勇士之心/ysxz_build_so_103.sh` 可复现地重建 1.0.3 的 so。

## 内容

基线：git 提交 `81c70279`（勇士之心 1.0.3 星空版交付）的
`mod菜单/starry-mod-menu/app/src/main/jni`，只改下面 4 个文件：

| 文件 | 改动 |
|---|---|
| `YsxzEntry.cpp` | `kYsxz_Features[]` 换成 7 行新表（1 号位 `Slider_伤害倍率:1:50:1`） |
| `YsxzMod.h` | `enum YsxzFeat` 同步序号（DAMAGE=1…AUTO=6，`YSXZ_FEAT_COUNT=7`） |
| `YsxzPatchJs.h` | JS 侧序号 `feature1=R(2)…feature5=R(6)`、`speed=V(5)`、`damage=V(1)` + 伤害倍率乘算；tryAuto 两级指纹（禁写死混淆名） |
| `YsxzMod.cpp` | `FileOffToVa()` 两轮探测（x 段优先→可读段兜底，选后读回序言校验）；`JNI_OnLoad` 90s 看门狗自愈 `files/res/main.js` 为内嵌自举版；原生真截获才写 `files/ysxz_native_ok`(=pid) |

**不包含** 1.0.4 的任何过检测改动（`YsxzSecShell`、`YsxzExitTrace`、`NoAceStubs`、
`YsxzAppComponentFactory`、`SignSpoof` 等一律不搬）。

## 重建

```bash
bash 04_scripts/勇士之心/ysxz_build_so_103.sh
```

产物 `libs_ysxz/arm64-v8a/libstarry_magic.so`：md5/大小以本轮构建输出为准。
旧版（139160 B / `724095fd…`）在 MuMu/Houdini 上会刷 `evalString rva 0x3138E4 not mapped`，
已由 2026-10-05 的可读段兜底修复（根因与验证见 `10_docs/勇士之心/` 本轮记录）。
