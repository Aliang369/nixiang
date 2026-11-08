# 动物大联盟 1.0.25 —— res 脚本（.jsc）解密与再加密

> 2026-11-06 第十五轮产物。结论来自静态反汇编 + 运行期内存核对 + 逐文件解密验证。

## 1. 结论（已实测）

```
文件(.jsc) = XXTEA_encrypt( gzip(js源码) )    # uint32 小端字数组，末尾按 4 字节对齐（补 0）
XXTEA key  = "d03398ba-c326-44"              # 16 字节，NUL 补齐
XXTEA 参数 = 标准实现：rounds = 6 + 52/n，DELTA = 0x9E3779B9
```

- key 是**引擎里的静态常量**，不是每台设备/每次启动随机：解密后的 `libcocos2djs.so` 文件偏移 `0x19419ab`（.rodata）处就在 `Cocos Game\0d03398ba-c326-44\0jsb-adapter/jsb-builtin.js\0main.js` 这一串里。
- 因此**离线可复现**，不需要再上机取内存。

## 2. 逆向路径（可复查）

| 环节 | 位置 | 说明 |
|---|---|---|
| require 入口 | `libcocos2djs.so:0x2ff5c8` | 先拼 `.jsc`（字符串 @0x1967a10），`FileUtils.isFileExist`（vtable+0x100）→ 读文件 → 解密 |
| 解密包装 | `0x317a28` | 签名 `(data_ptr, len, key_ptr, key_len, out_len)`；key 长度 <16 时补 0 到 16 |
| XXTEA 核心 | `0x317af8` | 字节→uint32 小端打包；`movz/movk` 构造 `0x9E3779B9`；初始 `sum = (6+52/n)*DELTA` |
| key 全局 | vaddr `0x222a158`（.bss） | libc++ `std::string`（短串布局：byte0 = len<<1，数据在 +1）；运行期实测值 = `d03398ba-c326-44` |

运行期地址换算（本例 pid 14272，引擎映射 `0x78a341b000` = vaddr 0）：
`.bss` 段载入点 `0x78a5644000`（对应 vaddr `0x2229000`）⇒ key 串在 `0x78a5644000 + (0x222a158-0x2229000) = 0x78a5645158`。

## 3. 验证（明文对，逐字节一致）

| 文件 | 密文 | 明文 | 校验 |
|---|---|---|---|
| `src/settings.jsc` | 220 B | 306 B | 与运行期内存里 `window._CCSettings=...` 逐字一致 |
| `src/cocos2d-jsb.jsc` | 385,984 B | 1,683,557 B | gzip 头 `1f8b08`，JS 正常 |
| `assets/main/index.jsc` | 3,508 B | 11,562 B | 以 `window.__require = function...` 开头 |
| `assets/scripts/index.jsc` | — | 2,321,617 B | 含游戏逻辑（`攻击力` 等中文键） |
| `assets/subScripts/index.jsc` | — | 4,096,747 B | 同上 |
| `assets/battleScript/index.jsc` | — | 568,930 B | 同上 |

## 4. 未解的三个文件（不阻塞）

`main.js`(1685 B)、`jsb-adapter/jsb-engine.js`(35,269 B)、`jsb-adapter/jsb-builtin.js`(62,985 B) 三者长度均 ≡1 (mod 4)，且都以 `2a 7a 65 2a 00`（ASCII `*ze*`）开头，走**另一条分支**（`0x322a18` 起：先 `strncmp("*e*",4)`/`"*ze*"`(5) 判标记，再用一个运行期 C 字符串当 key 调同一个 `0x317a28`）。
APK 的 `assets/main.js` / `assets/jsb-adapter/*.js` 是**同名明文**，需要时直接用；不影响玩法分析，也不影响改包。

## 5. 工具

```bash
# 单文件解密 / 判型
python3 04_scripts/动物大联盟/jsc_codec_dwdlm.py dec  <in.jsc> [out.js]
python3 04_scripts/动物大联盟/jsc_codec_dwdlm.py info <in.jsc>
# 整棵树解密（.jsc -> .js）
python3 04_scripts/动物大联盟/jsc_codec_dwdlm.py dec-tree <in_dir> <out_dir>
# 反向：JS -> .jsc（带 roundtrip 自校验，用于打补丁后回封）
python3 04_scripts/动物大联盟/jsc_codec_dwdlm.py enc  <in.js> [out.jsc]
```

本轮产物：`09_test/动物大联盟/res_js_20261106/**`（8.3 MB 解密后 JS，可用上面命令随时重建，不必长留）。

## 6. 战略意义（下一轮用）

1. **res 全可读**：玩法逻辑、6 项功能（攻击倍率/上帝模式/无限技能/自动战斗/移速/免广告）的挂点都能直接看 JS 了。
2. **可再加密**：打完补丁用 `enc` 回封即可，格式一致 ⇒ **不需要重签 APK**。
3. res 在 `/data/data/<pkg>/files/res/`（app 私有目录，root 可写）⇒ 官方包原样装着、壳照常工作、签名不变，只替换对应 `.jsc` 就能改逻辑。
4. 待确认：游戏启动时的热更 md5 校验（`version` 里的 `file_md5`）会不会把改过的文件判为过期并重下；若会，就在 `scripts/index.js` 或启动 JS 里改校验分支（现在能改也能回封）。
