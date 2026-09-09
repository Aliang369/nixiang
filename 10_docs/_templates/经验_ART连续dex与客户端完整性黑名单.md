# 经验卡片：ART 连续 dex · 客户端 APK 完整性黑名单

> 状态：可汇总素材  
> 首例：长生：天机降世 `0.9.9` / `com.netkuu.changsheng.tjjs`（2026-08-13）  
> 详记：`10_docs/长生天机降世/星光落地_0.9.9_20260813.md`

---

## 1. 两个独立坑（勿混为一谈）

| 坑 | 现象 | 根因 |
|----|------|------|
| **A. ART 连续加载** | 启动闪退 `ClassNotFound` / `NoClassDefFoundError`（如 `SignSpoof`） | 有 `classes5.dex` 无 `classes4.dex` 时，ART **不加载**更高号 |
| **B. 客户端完整性** | 登录弹「警告！检测到非法登录」等后退出 | 热更逻辑：`包体积 > 阈值` **且** APK 内命中**黑名单路径** |

修 A 时若人为补出 `classes4.dex`，可能正好踩进 B。

---

## 2. ART：注入必须落在「已有最高连续」dex

1. 原包若只有 `classes` + `classes2` → 拆包/星光最多新建 **`classes3`**。  
2. **禁止**为「给星光单独一页」跳到 `classes5` 再回头造 `classes4` 填洞——除非确认游戏**没有**把 `classes4.dex` 当黑名单。  
3. 注入脚本自检：签名后 `unzip -l` / Python 列 `classes*.dex` 必须连续无洞。

---

## 3. 完整性：先反编译登录点击，再定黑名单

长生 0.9.9 例（`DlgNetkuuLoginSystem.OnMobileLoginClickHandler`）：

```csharp
if (SafeGetFileSize() > 2181850710u
    && Scan({"classes4.dex",
             "assets/overlay_integrity.dat",
             "lib/arm64-v8a/libbattlehook.so"}).Count > 0)
    → 弹窗 + Exit
```

作业单：

1. 热更 `Code.dll`（Yoo/HybridCLR）里搜弹窗文案 / `SafeGetFileSize` / `classes4`。  
2. 记死：**体积阈值 + 路径列表**（版本一变要重算）。  
3. 正式包构建后断言：黑名单路径 **全部不存在**；体积可大于阈值，只要 Count=0。  
4. 注入脚本把断言写进 sign 之后（长生：`inject_cstjjs_0.9.9.sh`）。

其它游戏同类：字符串搜「非法」「完整性」「overlay」「battlehook」等。

---

## 4. 与死签 / TapSign 的关系

- TapSign / SignSpoof **过了**仍可被 B 拦；不要一见登录警告就只查 MD5。  
- 先 `logcat` / 反编译确认是 **UI/热更弹窗** 还是 SDK 签失败。

---

## 5. 以后汇总时怎么用

新游戏出星光包：

1. 启动能否加载 SignSpoof（坑 A）  
2. 登录点击是否有体积+路径双条件（坑 B）  
3. 注入脚本写死「最高连续 dex」+「黑名单断言」
