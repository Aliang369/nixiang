# 热更指纹漂移重校（功能突然失效的第一诊断路径）

> 路径：`re-extract/hotupdate-fingerprint-recheck.md`
> 触发场景：**已有星光/斌哥包在跑，游戏内热更后功能失效**（用户报"功能失效/不生效"），
> 或换版本后菜单能开但功能无效果。
> 通用政策见 `../../AGENTS.md`；Hook 侧铁律见 `../re-implement/hook-rules.md` §24。

---

## 0. 先判是不是"热更漂移"，别急着改 native

热更只换 JS/资源时，**native 侧不该动**。先做三查，一次就能分流：

| 查什么 | 命令/位置 | 结论 |
|---|---|---|
| 热更资源版本 | 设备 `files/res/version` → `localResVsersion` | 与上次交付记录不同 = 资源漂移 |
| 核心 so 是否同源 | 从当前安装包取 `libcocos2djs.so`（或 `libil2cpp.so`）算 SHA-256，比历史绑定表 | **相同 → native RVA/序言仍有效，别改 native** |
| gameplay bundle 在不在 APK 里 | `unzip -l base.apk \| grep -E 'assets/assets/[a-z]+/index\.js'` | 只有 `internal`/`main` = 玩法分包**只能靠热更**，指纹输入就是热更产物 |

> 先例：勇士之心 的实测基线（so 同源⇒RVA/序言仍有效、失效点全在 JS 指纹）见
> `10_docs/勇士之心/技能先例.md` §1。

## 1. 只读重抓（先量后拉，禁止逐文件 exec-out）

```bash
# 先量：文件数与总量
adb -s $SERIAL shell "su -c 'cd /data/data/$PKG/files/res && find . -name \"*.js\" | wc -l; du -sh .'"
# 再拉：>50 个文件或 >20MB 一律 tar 单流（逐条 adb exec-out 会超时且丢 stdout）
adb -s $SERIAL exec-out "su -c 'cd /data/data/$PKG/files/res && tar -cf - \$(find . -name \"*.js\")'" > res_js.tar
tar -xf res_js.tar -C <out>
```

- **只拉 `*.js` + 清单**（`version`、`.cocos-project.json`、`find -exec md5sum`）；
  `import/*.json` 是资源序列化数据，动辄两万个，不纳入分析拉取。
- 拉完必须**逐文件比对设备端 md5**，数量与指纹全对上再往下走。
- 重抓脚本按本作命名（先例：`10_docs/勇士之心/技能先例.md` §2）。

## 2. 解密（key 优先复用历史）

- 先查该游戏 `10_docs/{游戏}/` 是否已记录热更解密链与 key，**不要重新逆**。
- 解密链与 key 属单游戏事实（先例：`10_docs/勇士之心/技能先例.md` §2），技能里不复制。
- 解密产物落 `06_dumped/{游戏}/{轮次}/`（文本，入 git，作为版本档案与指纹对照）。

## 3. 定位漂移：离线复现发现逻辑（**先校准基线**）

写/跑离线复现器时必须守两条，否则会得到误导结论：

1. **先校准**：在**已知可用的旧 res + 旧指纹**上跑，必须得出 `OK`；
   跑不出 OK 说明复现器本身错了，不是游戏变了。
2. **粒度与运行时一致**：运行时 `findFn(exports)` 是**按函数**遍历源码字符串；
   离线按"整文件/整模块"判指纹会因为模块上万行而必然失败（长度约束、跨函数串味）。

> 先例：本作复现器命令与三组对照的真实输出见 `10_docs/勇士之心/技能先例.md` §1。

判定表（三组对照一次定位根因）：

| 输入 | 结果 | 含义 |
|---|---|---|
| 旧 res + 旧指纹 | `OK` | 复现器可信 |
| 新 res + 旧指纹 | `MISSING:auto` | **复现了用户报的失效** |
| 新 res + 新指纹 | `OK` | 修复方向正确 |

## 4. 常见漂移形态（按出现频率）

1. **指纹写死了会被改名的短标识符**（最高频）：`bVs(` → `bXP`。
   → 见 `../re-implement/hook-rules.md` §24，改用枚举成员/字符串字面量/明文属性名。
2. **混淆名整体漂移**（`bjY`→`bmh`、`beu`→`bgD`）：只要指纹是明文锚点，运行时发现逻辑自动吸收，**无需改代码**。
3. **bundle 归属变化**：某方法从 `battleLogic` 挪到 `game` bundle → 需要 native 侧同时接受两个 bundle 路径。
4. **bundle 外壳/注册调用形态变化**：拼接点 pattern（如 `\n}({\n`、`\n}, {}, [`）消失 → 拼接失败需留日志。

## 5. 修复与验证顺序（装门前闭环）

1. 改指纹（主指纹用明文 + 兜底指纹用结构，两级）。
2. **指纹命中失败必须留日志**（候选名 + 长度）：静默跳过是本次排查耗时最长的原因。
3. 静态断言 + 语法关：`verify_*.py` 断言（禁止再写死被改名标识符）+ `node --check`。
4. 离线复现器跑出 `OK`。
5. 才进构建 → 外科 → 签名 → 装机。
6. 装机后判据（两个 REPORT 分工要看懂）：
   - bundle 各自 splice 的 `len` 应等于该 bundle **解密后字节数**；
   - 六钩子 `REPORT formula=… apply=… gate=… cost=… death=… movesys=yes`；
   - 自动战斗在**另一个 bundle** 的 REPORT 里 `auto=yes`（不是同一个 REPORT）。

## 6. 别忘的收尾

- 热更轮次完成后，把新 res 版本号、新指纹、判定表结论写进 `10_docs/{游戏}/`，
  并在 `10_docs/_state/games/{游戏}.md` 更新"当前摘要"。
- 若本轮换过外科底包（原包被移走/换构建），记下新底包身份与**不可再生性**：
  设备装了星光包后，官方包无法再从设备拉取。
