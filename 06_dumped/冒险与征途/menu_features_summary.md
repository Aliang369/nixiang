# 「冒险与征途」mod 菜单（menu_1194）exp 解密报告

- 目标：`com.mxyzt.gzmy`（斌哥改包）内 WebView mod 菜单 `menu_1194.html`，页面用 `initGame(exp)` 加载功能配置。
- 结论：**exp 已成功解出**（1448 字符密文 → 580 字符明文），共 **6 条记录**，其中 **3 条生效**（后 3 条被 `//` 前缀静态注释）。
- 交付：`menu_features_decrypted.txt`（明文原文）、`menu_features_parsed.json`（结构化）、本文件；复刻脚本 `_decrypt_exp.py`（纯标准库，与 Node 原生调用 `strencode` 结果逐字符一致）。

## 一、功能清单

| # | 生效 | 类型 | id | 名称（原文） | data-info 提示 | data-gold 价格 | data-vip | data-ad | 目标方法 |
|---|------|------|----|--------------|----------------|----------------|----------|---------|----------|
| 0 | ✅ | btn | 0 | 秒杀小怪 | 开启成功 | — | — | — | `injectedObject.startFunction(0, 1/0)` |
| 1 | ✅ | btn | 1 | 超级无敌 | 开启成功 | — | — | — | `injectedObject.startFunction(1, 1/0)` |
| 2 | ✅ | btn | 2 | 全模式秒杀 | 开启成功,伤害根据你手机性能来 他是持续帮你释放技能增伤，并且佩戴有连击的技能伤害高，具体的测伤害 去训练场测试一下那个技能伤害高就装备那个 | 1-99999 | vip | — | `injectedObject.startFunction(2, 1/0)` |
| 3 | ❌ | //btn | 3 | 无限技能 | 开启成功 | — | — | — | 被 `//` 注释，不生成卡片 |
| 4 | ❌ | //btn | 4 | 无限技能 | 开启成功 | — | — | — | 被 `//` 注释，不生成卡片 |
| 5 | ❌ | // input | 3 | 未添加 | 图片地址 / 开启成功 | — | — | — | 被 `// ` 注释，不生成输入框 |

图标 URL（均为 `https://menu-1255602259.cos.ap-nanjing.myqcloud.com/tubiao/` 下）：
`%E7%88%BD%E5%89%91.png`（爽剑.png，功能0）、`%E7%BF%85%E8%86%80.png`（翅膀.png，功能1）、`%E9%BE%99.png`（龙.png，功能2/3/4）。

**关于「目标方法名」**：菜单侧**不含任何方法名**，只携带数字 id。启用/禁用由 `btnSwitch()` 调用原生桥
`window.injectedObject.startFunction(id, 1)`（开）/ `startFunction(id, 0)`（关）——`injectedObject` 是 App 侧
`addJavascriptInterface` 注入的原生对象，id → 游戏内实际 hook 方法的映射表在 Java/native（so）侧，不在 WebView 资源里。

### exp 明文原文（580 字符，逐字照抄）

```
btn_0_秒杀小怪_https://menu-1255602259.cos.ap-nanjing.myqcloud.com/tubiao/%E7%88%BD%E5%89%91.png_开启成功###btn_1_超级无敌_https://menu-1255602259.cos.ap-nanjing.myqcloud.com/tubiao/%E7%BF%85%E8%86%80.png_开启成功###btn_2_全模式秒杀_https://menu-1255602259.cos.ap-nanjing.myqcloud.com/tubiao/%E9%BE%99.png_开启成功,伤害根据你手机性能来 他是持续帮你释放技能增伤，并且佩戴有连击的技能伤害高，具体的测伤害 去训练场测试一下那个技能伤害高就装备那个_1-99999_vip###//btn_3_无限技能_https://menu-1255602259.cos.ap-nanjing.myqcloud.com/tubiao/%E9%BE%99.png_开启成功###//btn_4_无限技能_https://menu-1255602259.cos.ap-nanjing.myqcloud.com/tubiao/%E9%BE%99.png_开启成功###// input_3_未添加_图片地址_开启成功
```

### 记录格式（initGame 的解析逻辑）

- 记录分隔符 `###`，字段分隔符 `_`，字段顺序 `类型_id_名称_图标URL_提示[_价格金币][_vip][_ad]`。
- `类型 == "btn"` → `addBoxBtn(id, 名称, 图标URL, 提示, 价格, vip, ad)`；`类型 == "input"` → `addBoxInput(...)`。
- 生成卡片 DOM：`data-id / data-info / data-gold / data-vip / data-ad`，按钮 `data-state="off"`、`onclick="btnSwitch(this)"`。
  非空 `vip="vip"` → class 追加 `vipbox` + 角标「VIP专属」；`ad="ad"` → 角标「广告功能」；`gold` 非 0 → 打赏价。
- 后 3 条以 `//` 开头，`split('_')[0]` 得到 `//btn`、`// input`，既不等于 `btn` 也不等于 `input`，**因此被静默跳过**（开发者用注释方式下架的草稿功能）。

## 二、解密算法（一句话）

`initGame(exp)` 的第一步是 `exp = strencode(exp)`，`strencode` 是**「URL 解码 → 自带 Base64 解码 → 与固定密钥 XOR → 再 Base64 解码」的自定义链，固定异或密钥 `vjashjrbgouqhorqwe1235xac`**：

1. `key = Base64.encode("vjashjrbgouqhorqwe1235xac")` → `dmphc2hqcmJnb3VxaG9ycXdlMTIzNXhhYw==`（36 字符，这才是真正参与异或的密钥）。
2. `data = Base64.decode(unescape(exp))`：先 URL 解码（exp 里有 `%2B`/`%3D` 等），再标准 base64 解码，再 `utf8_decode`。
3. `code[i] = chr(data.charCodeAt(i) ^ key.charCodeAt(i % 36))`。
4. `return Base64.decode(code)`：过滤 `[^A-Za-z0-9+/=]` 后 base64 解码，再 `utf8_decode`。

控制流平坦化的顺序表为 `"2|0|3|1|6|5|4"`（即上面的执行序）。`Base64` 是 webtoolkit 老版实现
（keyStr `ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=`，encode 先 `utf8_encode`，decode 先过滤再 `_utf8_decode`，只处理 1–3 字节序列）。

- 复刻与校验：`_decrypt_exp.py`（纯 Python 标准库）输出与 Node 里直接调用页面自带的 `win.strencode(exp)` **完全一致（580 字符，MATCH=True）**。
- 同一套 `strencode` 也用于服务端返回体的解密，例如 `getAdData()` 中 `JSON.parse(strencode(_0xdeb650))`。

## 三、showAD / ad_ok / hy_vip 的性质（明确结论）

**属于「菜单自身看广告解锁 mod 功能」，与「游戏内看广告得奖励」无关。**

- `btnSwitch()`（`_app1.deob.js` @22881）：当卡片 `data-ad == "ad"`、且当前 `!window.hy_vip && !window.ad_ok` 时，点「开启」不会直接生效，
  而是弹出菜单自己的 `hyConfirm` 对话框：标题「广告功能」，正文「此功能是广告商赞助的功能 观看一条广告 即可24小时内 免广告使用此功能
  （vip大佬在用户用心登录一下，即可免广告使用此功能）」，按钮「观看」→ `showAD()`，「取消」→ 关闭。**这是菜单的功能解锁闸门，不涉及游戏内奖励发放。**
- `showAD()`（@123610）：显示 `.ad_box`，把 `#ad-iframe` 的 `src` 设为 `https://mod777.hyphp.cn/game/getKuaizhan?url=` + `window.ad_url`；
  按 `window.ad_time` 秒倒计时（1 秒一跳）；倒计时归零 → `window.ad_ok = true`，`localStorage.setItem("adTime", NOW_TIME() + 0x15180)`（`0x15180` = 86400 秒 = **24 小时**），并清除定时器。
  若进来时 `NOW_TIME() < localStorage["adTime"]`，直接 `ad_ok = true` 返回（不播广告）。
- `window.ad_url` / `ad_url2` / `ad_time` 由 `getAdData()`（@96501）从服务端拉取：`POST https://mod777.hyphp.cn/game/getAdData`，参数 `{id: window.game_id, cookie: localStorage.cookie}`，
  响应体用同一套 `strencode` 解密后取 `info.ad_url / info.ad_url2 / info.ad_time`。
- `window.ad_ok` 是**纯前端、本地 24 小时免广告凭证**（内存 + localStorage，`initGame` 里会依据 `localStorage.adTime` 重新置位），只控制菜单功能是否放行。
- `window.hy_vip` 是**用户中心登录返回的服务端 VIP 标记**（`window['hy_vip'] = parseInt(info.vip)`，登录/校验用户信息时赋值，默认 `false`）。
  它为真时：① VIP 专属（`data-vip="vip"`）功能放行；② 广告功能**自动 `ad_ok=true`** 并提示「因为你是黄金会员可以免广告使用此功能，再次点击开启即可使用！」；③ 跳过 `data-gold` 打赏校验。
- 另一条收费链（与广告无关）：`data-gold` 非 0（如功能 2「全模式秒杀」= `1-99999`）且非 VIP 且 `!btnBuyState[id]` 时，
  未登录 → 切到用户中心标签并提示「使用该功能需要登录用户中心」；已登录未打赏 → 「使用该功能到用户中心打赏开通」。
  此外 `getHack() == '1' && !window.is_login` 时也强制跳转登录。

## 四、菜单 → 原生桥接口全集（可用于继续定位 hook 目标）

| JS 调用 | 作用 |
|---|---|
| `injectedObject.startFunction(id, 1/0)` | 开关某项功能（**id 即 exp 里的数字 id**）；HTML 里还有包装 `function btn(id,val){...}` @14561 |
| `injectedObject.openInput(id, val)` / `openInput2(a,b,c)` | 文本输入类功能（input 卡） |
| `injectedObject.saveInputValue(id, val)` | 保存输入值 |
| `injectedObject.btnSwitchPx(bool)` | 像素/开关型控件 |
| `injectedObject.getHack()` / `getHackType()` | 查询是否已注入 hack 与 hack 类型（@45835） |
| `injectedObject.exitApp()` / `exitMenu()` / `openUrl(url)` | 退出 App / 关菜单 / 打开外链 |

## 五、关键文件与偏移（便于复核）

| 内容 | 路径 / 偏移 |
|---|---|
| 菜单 HTML（明文） | `webview_payload/49117e943b200192_0.txt`，payload 偏移 `0x82`；清洗版 `_menu1194.raw.html`（8050B） |
| 菜单自身 JS（app1.js?var=0517） | `webview_payload/f451acbca312ea2c_0.txt`，payload 偏移 `0x90`（123014B）；清洗版 `_app1.raw.js`；反混淆 `_app1.deob.js`（126323B） |
| `strencode` | `_app1.deob.js` @28341 |
| `initGame` | `_app1.deob.js` @50290 |
| `addBoxBtn` / `addBoxInput` | `_app1.deob.js` @54286 / @57097 |
| `btnSwitch` | `_app1.deob.js` @22881 |
| `getAdData` | `_app1.deob.js` @96501 |
| `hyConfirm`（菜单自制弹窗） | `_app1.deob.js` @119290 |
| `showAD` | `_app1.deob.js` @123610 |
| `getHack` / `isLogin` | `_app1.deob.js` @45835 / @116098 |
| simple-cache 解析脚本 | `_cache_extract.py` → `webview_payload/_index.txt` |
| 解密复刻（Python/Node） | `_decrypt_exp.py` / `_decrypt_exp.js` |
