# 百度 Sagittarius 加固

> 📍 re-analyze/protection/baidu-shell.md
> 🎯 APK 使用百度加固（StubApplication + libbaiduprotect.so），需要识别并正确处理

---

## 识别特征

1. `AndroidManifest.xml` 中 `android:name="com.sagittarius.v6.StubApplication"`（或其子类）
2. `lib/` 目录存在 `libbaiduprotect.so`
3. jadx 打开 `classes.dex` 只看到 stub 代码，真实业务代码不可见

## DEX 被加密的形态

- `classes.dex` 是壳的 stub，只负责加载 `libbaiduprotect.so`
- 真实 DEX 在运行时由壳解密，加载到内存
- 静态反编译看不到真实业务逻辑
- 可能有多层 DEX（如 `classes.dex` 是壳，`classes2.dex` 是部分真实代码）

## 改包规则：保留壳链

**核心原则：不要动壳加密的 DEX。**

1. `StubApplication` 继承链**完全不动**
2. 只在最外层包自己的 Application（如 `StarryApp extends StubApplication`）
3. `libbaiduprotect.so` **不替换、不修改**
4. 被壳加密的 `classes.dex` **不修改**
5. 新增的代码放到新的 DEX（如 `classes3.dex`）或未被加密的 DEX 中
6. Manifest 可改（Application/权限/Service）

## 验证方法

```bash
# 看壳是否正常初始化
adb shell su -c 'cat /data/data/<pkg>/shared_prefs/shell_config.xml'
```

壳正常初始化后会有配置文件输出。如果文件不存在或内容异常，说明壳链被破坏。

## 坑/弯路

- **不要试图改已被百度壳加密的 DEX**：改了会导致壳解密失败 → 闪退
- **不要替换 `libbaiduprotect.so`**：壳的版本与 DEX 加密方案绑定
- 如果需要修改被壳加密的代码（如 TapTap 登录逻辑），静态改不可行，需运行时 Pine/ART hook（参考虚无之刃案例：mocika 壳加密 DEX 时用 Pine hook 运行时 patch）
- 部分游戏百度壳 + 斌哥壳叠加，需同时处理两层保护

## 外部已公开方案（检索入口，2026-10-15 新增）

- **已核验公开项目：暂无**（2026-10-15 首查 `百度加固 脱壳 dex dump` 得 0 条，疑搜索引擎频控，**不等于无解**；空结果已记台账 §3）。
- **检索词**：`百度加固 脱壳 dex` / `libbaiduprotect 分析` / `<so 名> 壳 初始化`。
- **注意**：本页铁律（**不替换 `libbaiduprotect.so`、不改被加密的 classes.dex、新增代码进新 DEX**）高于任何外部「脱壳后重打包」方案——脱壳只用于**看清代码**，交付仍走外科组装。
- **台账**：`10_docs/_state/外部方案台账.md`；协议见 `skill/diagnosis-index.md`「外部检索协议」。
