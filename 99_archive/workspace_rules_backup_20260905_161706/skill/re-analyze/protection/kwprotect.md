# Kiwi/kadp VM + KwProtectSDK + Q1 SDK（加固保护族）

> 📍 `re-analyze/protection/kwprotect.md`
> 🎯 适用：命中「kiwi `StubApplication` + `libkadp.so`（内嵌 Kiwi VM）+ `libKwProtectSDK.so`」或 Q1 SDK「当前游戏包不合法，请到应用市场下载！」弹窗的**任何游戏**
> 📖 行为特征来自 2026-08-28 首次实机测绘（王国大作战前线，`10_docs/王国大作战前线/实机重验_保护面测绘_20260828.md`）。本页描述的是**保护产品本身的行为**，同族游戏命中特征即适用；但每款游戏的可动文件/容忍度可能不同，按总纲规则逐款实测，不能抄结论

---

## 保护栈（产品结构，跨游戏一致）

```
StubApplication (classes.dex, kiwi 加固)
  └─ loadLibrary("kadp")
       └─ Kiwi VM 从 base.apk 抽 dex → 内存 DexFile → files/.jiagu/classes.dex
            ├─ load libkwsdataenc.so / libKwProtectSDK.so
            └─ Q1 SDK → PM 证书校验（弹窗=终局门：点「确认」立即退出；离线/断网不跳过，只换弹窗）
```

- 保护 SDK 的类**不在** APK 任何 dex 的 `class_defs`：密文型类串嵌在 classes.dex 数据区，运行时才解出；磁盘 `.jiagu/` 只有一个占位 mini.dex
- 改签后 `ClassNotFound: <SDK 类>` 是 KwProtect 校验失败的**下游症状**，不是根因；零改动重签同样复现

## KwProtect 完整性网（产品行为，首测于 8/28）

| 对象 | 行为 |
|------|------|
| `base.apk` | 字节级校验（签名块/CD 结构敏感，**追加一条 dex 即 ~2s 死**） |
| split APK | 存在即杀（~15s signal 9） |
| lib 目录 | **新增**容忍（星光 so 放行）；**替换/修改**既有 lib → MD5 校验 → 杀 |
| kill 手段 | direct `svc` kill（libc open/openat/mmap hook **不可见**）；patch kill 只会变死转（校验失败主线程自旋 ANR） |
| 时序 | 改签 ~0.5-2s 退；文件违规 ~15s 杀 |

换游戏必复测：上表阈值/时序/容忍项可能随产品版本变化，用最小改动 A/B 逐项确认。

## 核心教训：正解是「让校验通过」，不是 patch kill

- **实锤**：外科包装机后 root 原地把 `base.apk` 换回官方字节（md5 一致）→ 游戏正常起、无 signal 9、加固链路全通，只剩 PM 证书门
- SignSpoof / 安装来源伪装（`adb install -i <市场包名>`）对 PM 证书弹窗**无效**；libc 层文件重定向**无效**（直连 syscall）
- PM 证书门的候选解法（设备级操作，需用户选择）：`packages.xml` 证书改写 / Zygisk Next 注入 / 卸载重装官方包+数据还原 / 纯分析路线

## 运维命令备忘

```bash
# 大包必须 --no-incremental：incremental-fs 安装后 root 也写不了 /data/app
adb install -r -d --no-incremental <apk>

# root 原地换 base.apk 为官方字节
BASE=$(adb shell pm path <pkg> | head -1 | sed 's/package://')
adb shell "su -c 'cat /data/local/tmp/<官方ref>.apk > $BASE && chmod 644 $BASE && chown root:root $BASE'"

# 无 hook 抓 ANR / native 死转栈
adb shell "su -c 'kill -3 <pid>'"   # 输出在 /data/anr/trace_00
```

## 方法论：静态推测必须被实机 A/B 检验

- 首测前一版的「VM 读签」「渠道 SigningBlock」全是静态推测，实机 8 组 A/B 逐条证实/证伪（含 `install -i`、split 安装、lib 替换、libc 重定向、K1 桩、SignSpoof）
- 推测结论进文档必须带 PROVEN / FAILED 标记；**实机 A/B 优先于静态推理**，过时推测要显式标注作废，防止下次接手照抄
