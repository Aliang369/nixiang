# 过环境最小路径（第一阶段：菜单进游戏且能正常玩）

> 路径：`re-analyze/environment-min-path.md`
> 触发场景：用户说「先过环境 / 先别直接做功能 / 把 mod 菜单组装进游戏内、能正常玩才算完成」。
> 政策见 [AGENTS.md](../../AGENTS.md)「审查与分阶段门禁」；外科组装与门禁串见 [../starry-mod/surgical-build.md](../starry-mod/surgical-build.md)。

---

## 0. 这条路径在防什么

用户说「先过环境，不要直接做功能」的真实含义是 **别做与「能正常进游戏」无关的任何分析**，
不是「先别写 hook，其它分析随便做」。

历史事故（2026-10-01 失落城堡）：拿到底包后顺手做了大块第二阶段分析——脱壳派生 +
静态 IL2CPP 符号/RVA 定位，两条静态工具路线（Il2CppDumper / Cpp2IL）**全部失败**，
环境交付却没往前推，用户体感「30 多分钟什么都没看到」。
**正确顺序：环境闭环交付 → 用户验收「能正常玩」→ 才开第二阶段。**

---

## 1. 阶段闸门（开工先声明）

动手前先把下面两句话写进回复：

1. **本阶段最小充分集** = 下表五步，别的不做。
2. **推迟清单**（明确声明「以下推迟到第二阶段」）：IL2CPP 符号恢复 / RVA 定位 /
   metadata 类型与字段枚举 / ABI 分析 / 玩法 hook 逆向 / 反作弊体系梳理。

**未收到用户明确的「开始做功能」口令前，禁止启动推迟清单里的任何一项。**
（口令形态如「开始第二阶段的做功能吧」「做功能吧」。用户只是问「能不能改」不算口令。）

---

## 2. 五步最小充分集

| 步 | 做什么 | 退出条件（拿到什么才算过） | 允许的产物 |
|----|--------|--------------------------|-----------|
| 1 | **底包身份** | 包名 / versionName / versionCode / ABI / 体积 / md5 / sha256 记录在案；判定是否 split、是否单 base.apk | `10_docs/_state/底包再获取台账.md` 一行 |
| 2 | **壳与注入点判定** | 能否替换 `application@name`？有壳（易盾/ACE/TSS…）⇒ 走 ContentProvider 注入（`StarryInitProvider` + `android:initOrder="2147483647"`），**绝不改 `application@name`** | `10_docs/{游戏}/` 一段结论 + 依据 |
| 3 | **签名信息采集**（记录用，不再做运行时伪造） | 官方证书 MD5/SHA1/SHA256/DN 导出（`META-INF/*.RSA|EC|DSA` 走 `openssl pkcs7 -print_certs`，**`keytool -exportcert -jarfile` 对渠道包不可用**）并落 `10_docs/{游戏}/` 记录；**2026-10-28 起运行时签名伪装 `SignSpoof` 已删除**，验签/登录一律走改包法（见 `login/taptap-web-login.md` 现行口径） | 证书指纹已入库 + 成品包 `framework_entry_audit.sh [B]` 判 `dex 无 Lcom/starry/magic/sign/SignSpoof;` |
| 4 | **登录 patch 点** | 按 SDK 选 patch 类：TapTap → [login/taptap-web-login.md](login/taptap-web-login.md)；QQ/微信 → [login/qq-wechat-sso.md](login/qq-wechat-sso.md)。**只取「证书 + patch 类名」这两样** | 登录走到网页/豁免分支的日志 |
| 5 | **组包装机** | `surgical_*` 出包 → 覆盖安装 → 菜单可见 → 能进游戏玩 | 用户口头确认「可以正常玩」 |

**脱壳只在前置步骤确实需要时做**（本例：真实 dex 只在内存里，登录 patch 类名必须从中取），
且**只取「证书与 patch 类名」这两项**，不做全量 dex 分析。做法见
[protection/nesec-inmemory-dex.md](protection/nesec-inmemory-dex.md)。

---

## 3. 每步的禁止项

- 第 1 步：禁止逐个 `ls`/`find`/`md5` 探测 `02_apks/` 与 `08_output/`；用户声明即事实。
- 第 2 步：禁止因为「先看看到底能不能过校验」而整包 `apktool b`。
- 第 3/4 步：禁止自己抓授权链路（见 [../starry-mod/auth-chain.md](../starry-mod/auth-chain.md)「排查前置条件」）；
  账号先由**用户人工登录**。
- 第 5 步：禁止用截图或 `input tap` 判断结果（见 AGENTS.md「真机交互零截图」）；
  要看的是 `logcat` 里 `Mod_Menu` / `StarryMagic` / `spoof hit` / `grant ok` 几条锚点。
- 全程：禁止为「以后反正要做」而提前枚举 metadata、dump 符号表、算 RVA。

---

## 4. 交付话术（操作请求卡）

每步需要用户动手时，按 AGENTS.md 固定格式输出三项，一次只请求最小一组：

```text
【操作请求卡】
① 请你做什么：<进哪个界面 / 点哪个开关 / 输入什么>
② 成功标志：<界面上会看到什么>
③ 我接着看什么：<哪条日志 tag / 哪个文件>
```

不要连着追问，也不要在用户还没做之前假设已完成。

---

## 5. 退出条件与下一阶段

- **退出条件**：用户明确说「可以正常玩了 / 没问题」。此时环境阶段才算完成，写进
  `10_docs/_state/games/{游戏}.md` 的当前摘要 + `current.md` 一行。
- **进入第二阶段**：只认用户口令。进入后先读 [re-implement/SKILL.md](../re-implement/SKILL.md)，
  再按 [hook-rules.md](../re-implement/hook-rules.md) 走。
- **环境阶段没跑通就报错时**：先按 §2 逐步定位是哪一步没过，不要转而扩大分析面。
