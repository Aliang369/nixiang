# 弹壳战机 1.1.7 第三方 mod 改包 — 注入功能单

- 分析对象：`02_apks/弹壳战机_1.1.7.apk`（201,817,533 B，md5 `9d2c15ab5efe009e55de351d3c6a3fc4`，包名 `com.survivor.acecn`，签名 `CN=Android Debug` 的第三方调试证书）
- 结论：这是**卡密制（付费授权）外挂注入包**，注入框架自称 **DSHK / UnityHelper**，与「星光版」无关，也不是单纯的功能补丁 —— 它带**云端卡密验证中心 + 悬浮窗面板 + 云控下发**。
- 一句话功能单：**英雄攻击倍率、锁血、免广告、全屏吸取、全局加速、一键胜利** 共 6 项，全部由卡密验证后从云端解锁并下发数值。

---

## 一、注入物清单（与原包的差异）

原包所有原始条目时间戳统一为 `01-01-1981 01:01`；被替换/新增的 6 个条目时间戳为 `01-01-2024 00:00`：

| 条目 | 大小 | 性质 | 说明 |
|---|---|---|---|
| `classes5.dex` | 10,792 B | 新增 | 引导层：FileProvider 启动链 |
| `assets/dshk_payload.bin` | 19,776 B | 新增 | AES-GCM 加密载荷 → 云控加载器 dex |
| `lib/arm64-v8a/libunityhelper.so` | 13,440 B | 新增 | 自写 JNI 库，实际改游戏内存/logic |
| `META-INF/ANDROIDD.SF` | 365,440 B | 替换 | 重签名 |
| `META-INF/ANDROIDD.RSA` | 1,167 B | 替换 | 重签名 |
| `META-INF/MANIFEST.MF` | 365,313 B | 替换 | 重签名 |

校验值：
- `libunityhelper.so` sha256 `6d29033653248a8bc3263b40ef6681f3b0c38724cca83c5e0d1698220adf08dc`
- `classes5.dex` sha256 `986c6bc1ce32d489e028d914df8ea3393bbf4cb64bb5f1b8d70f92f96ee6998c`

Manifest 唯一改动：新增 provider
```xml
<provider android:name="com.unity.plugin.FileProviderImpl"
          android:authorities="com.survivor.acecn.sigprovider"
          android:exported="false"
          android:initOrder="0xc8"
          android:grantUriPermissions="true">
  <meta-data android:name="android.support.FILE_PROVIDER_PATHS" android:resource="@.../#7f10000b"/>
</provider>
```
`application` 元素**未**设置自定义 `android:name` —— 利用「ContentProvider.onCreate 早于 Application」的启动顺序完成注入，属于隐蔽手法。

## 二、启动链

```
ContentProvider(com.unity.plugin.FileProviderImpl).onCreate
  → classes5.dex: com.dshk.NativeBridge (extends com.unity.plugin.UnityNative)
  → nativeGetKey()  (libunityhelper.so, 返回 32 字节硬编码密钥)
  → 读 assets/dshk_payload.bin → AES-GCM 解密
  → InMemoryDexClassLoader → 反射 com.unity.plugin.LoaderEntry.start()
  → HTTP 拉取云控配置与云端面板 → 卡密验证 → 显示悬浮面板
```
内置反取证：`LoaderEntry.purgeAnonymousVdex()` 会清理 `/data/data/<pkg>/.../oat/arm64` 下的 `.vdex` 与内存 dex 缓存；日志写入 `dshk_panel.log` / `dshk_boot.log`。

## 三、功能单（native 层能力，即真实修改项）

`libunityhelper.so` 的导出符号就是功能表，8 个修改入口：

| 导出符号 | 文件偏移 | 功能（面板标签） | 说明 |
|---|---|---|---|
| `nativeSetEnabled` | 0x604c | 总开关 | 关闭即恢复原版 |
| `nativeSetDamage` | 0x602c | **英雄攻击**（伤害倍率） | 面板默认上限 `0x1869f`=99999 |
| `nativeSetHpLock` | 0x606c | **锁血** | 血量锁定 |
| `nativeSetAdSkip` | 0x6120 | **免广告** | hook 广告 onShow/onClose |
| `nativeSetPick` | 0x61d4 | **全屏吸取** | 拾取半径 |
| `nativeSetSpeed` | 0x6288 | **全局加速** | 倍率档位 1x / 5x / 10x，`speedlv` clamp 1..10 |
| `nativeTriggerWin` | 0x63a8 | **一键胜利** | 另有 `__`/`__I` 重载 |
| `nativeBase` | 0x63d4 | — | 取基址，配合 `nativePublish` |
| `FileProviderImpl.nativeGetKey` | 0x69a4 | — | 返回解密密钥 |

**云端面板默认行顺序（`o/Q.smali` 静态初始化 `Lo/Q;->h0`）**：
`damage`(英雄攻击) → `hplock`(锁血) → `adskip`(免广告) → `pick`(全屏吸取) → `speed`(全局加速) → `win`(一键胜利)

实现方式：`dlopen("libil2cpp.so")` 后按**字符串名**解析 IL2CPP 导出 API（`il2cpp_domain_get` / `il2cpp_domain_get_assemblies` / `il2cpp_assembly_get_image` / `il2cpp_class_from_name` / `il2cpp_class_get_method_from_name` / `il2cpp_runtime_invoke` / `il2cpp_thread_attach` / `il2cpp_object_get_class` / `il2cpp_image_get_name`），做通用字段/方法改写；另有 `runInBackground -> true`、`frameprobe`、`arm thread timed out waiting for libil2cpp` 等日志。**不依赖固定 RVA**，故对版本有一定抗性。

诊断日志串（可用于真机确认命中）：`CTRL armed: ctrl=0x%lx dmg=0x%lx hp=0x%lx ad=0x%lx pick=0x%lx (enabled=%d hplock=%d adskip=%d pick=%d)`、`damage -> 0x%llx`、`hplock -> %d`、`adskip -> %d`、`pick -> %d`、`speed -> %dx %s`、`ad skip: onShow=%p onClose=%p`。

## 四、卡密验证中心（付费授权机制）

- 面板类：`com.unity.plugin.CloudPanel`（`show(Context,String)`），UI 由混淆库 ALGUI（`com.bytecat.algui`，smali 包名 `o/`）绘制。
- 授权接口（明文 HTTP/HTTPS 表单 POST，`application/x-www-form-urlencoded`）：
  - `POST https://wy.llua.cn/api/?id=kmlogon` — 卡密登录
  - `POST https://wy.llua.cn/api/?id=notice` — 公告
  - 参数：`app=67134`、`kami=<卡密>`、`markcode=<机器码>`、`sign=<MD5>`、`t=<时间戳>`
  - UA 伪造：`Dalvik/2.1.0 (Linux; U; Android 12)`；本地态串 `dshk-auth`、`dshk_auth`、`dshk-sugrant`
- 面板文案：「卡密验证中心」「在此输入卡密」「登 录」「验证成功 · 到期 <时间>」「长期有效」「验证失败：」「登录失败：数据已过期」「登录失败：非法操作」「正在提交验证，请稍候...」「网络授权 · 验证成功后解锁功能菜单」；未验证时 `not authed -> features stay vanilla`（功能保持原版）。
- 面板可被云端 JSON 覆盖：标题 `panel spec: title=`、副标题 `sub`、行列表 `rows`；解析失败 `panel spec parse failed, use default order`，为空 `panel spec empty, use default rows`，未知行 `panel spec: unknown row type ignored:`。状态行文案「● 全部关闭 · 原版状态」「● 已开启 」「● 未验证 · 请先输入卡密」「按住顶部可拖动面板 · 配置由云端下发」「隐藏」「已触发」。

## 五、云控下发（js.sheng15.site）

两个远端资源（HTTP 明文，可直连拉取）：

1. `http://js.sheng15.site/dshk.cfg`（774 B，**明文**），当前内容：
```ini
damage=1
on=0
hplock=0
adskip=0
pick=0
speedon=0
speedlv=5
```
文件内注释强调 **ALL FEATURES MUST STAY OFF**。原因是：**该 cfg 的数值会由 provider 直接推给 native，且与卡密验证无关** —— 所以作者把云端配置全部归零，真实数值只在卡密验证通过后由云端面板按需下发（防止白嫖）。
副作用：改这个文件即可免卡密生效任意数值，且**无需重装、重启游戏即生效**。

2. `http://js.sheng15.site/dshk_helper.bin`（54,588 B，加密）→ 解密后为 54,540 B 的 `helper.dex`（即云控面板本体）。加密方案（v2）：`[16B salt][12B IV][4B 大端明文长度][体]`，`prk=HMAC-SHA256(passphrase, salt)`，`kEnc=HKDF(prk,"dshk-enc-v2",32)`、`kPerm=HKDF(prk,"dshk-perm-v2",32)`，体先按 `kPerm` 派生的 Fisher-Yates 置换逆置换、再 AES-256-GCM 解密。`passphrase` 由 8 个 8 字节常量数组前后两组逐字节 XOR 得到。

## 六、系统级越权行为（需要重点注意）

面板为了常驻显示，会尝试绕过悬浮窗限制，日志串直接暴露逻辑：

- `overlay appop missing -> root grant (silent)`、`appops set ... allow`、`root appops grant exit=`、`root appops grant failed:` —— **检测到无悬浮窗权限时会用 root 静默授予**。
- `activity lifecycle hook installed` / `activity resumed` — hook Activity 生命周期，保证面板在游戏界面之上。
- 硬编码令牌串：`5MJCkFxBCzcP1dO`、`Q92kC22zMmL60VY`。
- FileProvider 还能把应用私有文件导出到 Downloads（`MediaStore$Downloads` / `writeDownload`）。

## 七、防护/风险小结

- 该包**未加壳**，引导层与 native 层均完全静态可读；但**真实面板在云端**，功能表可被作者随时增删 —— 静态能看到的只有 6 项默认行，云端可下发新行（未知行会被忽略，说明行类型受限于客户端已实现的视图工厂）。
- 载荷与面板 dex 均对称加密且密钥内置在 so 里（`nativeGetKey` 直接拷贝常量，文件偏移 0x11e0/0x11f0），可静态提取，等同于无保护。
- 卡密验证与云控均为 **明文 HTTP/HTTPS 且无证书校验或完整性保护**（`dshk.cfg` 直接 HTTP 明文），中间人可完全接管功能与数值下发。
- 与正版/官方包差异：缺 TapTap 库、缺 `libSDKloading`/`libtoolChecker`、体积比官方底包小约 5 MB —— 说明其底包本身即另一渠道再分发包，再叠加了本 mod。

## 八、复现步骤

```bash
# 1. 定位注入物（按时间戳）
unzip -l 02_apks/弹壳战机_1.1.7.apk            # 1981 时间戳=原包, 2024 时间戳=mod

# 2. 引导层反编译
bash 01_tools/smali/baksmali d /tmp/dkjmod/classes5.dex -o /tmp/dkjmod/smali5

# 3. 提取 native 密钥（常量在文件偏移 0x11e0 / 0x11f0）
xxd -s 0x11e0 -l 32 /tmp/dkjmod/libunityhelper.so
# key = 7c147cf1dd58d7be623a4ba2c9d03728a057ca9ddda9770bcdf46c95ba31148b

# 4. 解密 assets/dshk_payload.bin（v1: salt+iv+GCM，HKDF info="dshk-payload-v1"）
java /tmp/dkjmod/j/Decrypt.java /tmp/dkjmod/dshk_payload.bin /tmp/dkjmod/payload.dex
bash 01_tools/smali/baksmali d /tmp/dkjmod/payload.dex -o /tmp/dkjmod/smali_payload

# 5. 拉云控配置 + 云端面板
curl -A dshk -s http://js.sheng15.site/dshk.cfg
curl -A dshk -s -o /tmp/dkjmod/helper.bin http://js.sheng15.site/dshk_helper.bin
java /tmp/dkjmod/j/Dshk.java /tmp/dkjmod/helper.bin /tmp/dkjmod/helper.dex   # v2 方案
bash 01_tools/smali/baksmali d /tmp/dkjmod/helper.dex -o /tmp/dkjmod/smali_helper
# 默认功能行：grep -n 'h0:' /tmp/dkjmod/smali_helper/o/Q.smali
```

## 九、对既有工作的影响

本 mod 的 6 项功能与本仓「星光版」四项功能（攻击倍率 / 上帝模式 / 偷盗技能 / 免广告）**部分重叠但实现独立**：本 mod 靠名字解析 IL2CPP API（无固定 RVA，抗版本），星光版走固定 file offset 打补丁。其 `nativeSetDamage` 的落点与星光版 1.1.7 的 `EntityCharacter.ChangeHp` / `PostProcessLearnedSkill` 锚点属同一套伤害体系，可作为交叉验证参考。
