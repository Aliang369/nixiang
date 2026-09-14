# 网易易盾新版 / InMemoryDexFile 精确脱壳与无壳重建

> 📍 `re-analyze/protection/nesec-inmemory-dex.md`
> 🎯 适用于网易易盾 NIS `7.6.1_857` 一类“签名绑密钥 + 运行时不落盘真实 DEX”的壳。
> 实机基线：无悔华夏 4399 `com.yhhy.whrhx.m4399` 3.8.280（2026-09-14 全链验收）。

---

## 0. 结论先行

新版易盾不要把时间花在 `SignSpoof`、替换旧 `libnesec.so`、保留 v1 证书 payload 这些回灌绕过上。真实 DEX 通常以 ART `InMemoryDexFile` 存在，正确路线是：

```text
定位正确 ClassLoader
  -> 读每个 InMemoryDexFile 的 mCookie
  -> 从 DexFile 对象拿真实 dex begin_ 指针和 file_size
  -> 精确导出真实 DEX
  -> 用运行时热更底包重建无壳 APK
```

不要只依赖全内存搜 `dex\n035\0`：无悔华夏 4399 实测 root 全局盲扫命中 0，而 Frida 枚举 ClassLoader 后能直接看到 3 个 `InMemoryDexFile`；用 cookie 全量导出后 dexdump/jadx 均正常。

## 1. 适用与不适用

### 适用

- Manifest Application 是 `com.netease.nis.wrapper.MyApplication` 或同类 NIS stub。
- APK 只有壳 `classes.dex`，真实业务类在运行时由 `PathClassLoader` / `DexClassLoader` 的 `InMemoryDexFile` 提供。
- 壳在重签后静默退出，但原签包可以正常启动。

### 不适用

- native `OpenMemory` 直接改 ART 内部 DexFile、没有 Java `DexFile` 对象可见的壳。
- 抽取壳/VMP：DEX 能 dump，但方法体不在普通 `code_item` 中。
- 需要还原 so 加密的情况：本方法只解决 DEX 层，so 另走运行时页 dump / 热更副本提取。

## 2. 第一步：找“正确”的 ClassLoader

不要默认进程第一个 loader 就是业务 loader。先枚举所有 ClassLoader，用真实业务类名筛选：

```javascript
Java.perform(function () {
  Java.enumerateClassLoaders({
    onMatch: function (loader) {
      try {
        loader.loadClass("com.excelliance.open.LBApplication");
        console.log("business loader=" + loader);
        var b = Java.cast(loader, Java.use("dalvik.system.BaseDexClassLoader"));
        var elements = b.pathList.value.dexElements.value;
        for (var i = 0; i < elements.length; i++) {
          var df = elements[i].dexFile.value;
          console.log("  [" + i + "] " + df);
        }
      } catch (e) {}
    },
    onComplete: function () {}
  });
});
```

实测会出现两类 loader：

- `base.apk` + InMemoryDexFile。
- `base.odex.apk` + InMemoryDexFile。

两者可能暴露同一份 DEX 的重复副本，后续按真实 DEX 指针去重，不要只按 loader 名去重。

## 3. 第二步：从 `mCookie` 拿真实 DEX 起始指针

ART `dalvik.system.DexFile` 的 `mCookie` 是 Java `long[]`。常见形态：

```text
mCookie[0] = 0
mCookie[1] = DexFile* 对象指针
```

注意两个坑：

1. Frida JS 直接读 `long` 会丢精度，不要用普通 Number 拼接地址。
2. `DexFile.getName()` 对 InMemoryDexFile 可能是 `null`，用 `String(df)` 判断显示名。

可靠写法：

```javascript
var f = Java.use("dalvik.system.DexFile").class.getDeclaredField("mCookie");
f.setAccessible(true);
var Arrays = Java.use("java.util.Arrays");
var cookie = f.get(df);
var text = Arrays.toString.overload("[J").call(Arrays, cookie); // 精确十进制
var values = text.replace(/\[|\]/g, "").split(",");
var cookiePtr = ptr("0x" + BigInt.asUintN(64, BigInt(values[1].trim())).toString(16));
```

然后从 `DexFile*` 对象中找 DEX 起始指针。无悔华夏 Android 16 实测：

- `DexFile* + 0x8` = `begin_`。
- `begin_` 起始 8 字节必须是 `64 65 78 0A 30 33 35 00`（`dex\n035\0`）。
- DEX 头 `+0x20` 是 `file_size`（u32 little-endian）。

为防 ART 版本变化，推荐扫描 `DexFile* + 0x8 .. +0x80`，找第一个指向 `dex\n035\0` 的指针，而不是硬编码单点。

## 4. 第三步：精确导出 DEX

推荐让目标进程把数据写到自己可写目录，再由 root 拉出：

```javascript
var dexPtr = null;
for (var off = 8; off < 0x80; off += 8) {
  var p = cookiePtr.add(off).readPointer();
  try {
    var h = new Uint8Array(p.readByteArray(8));
    if (h[0] === 0x64 && h[1] === 0x65 && h[2] === 0x78 && h[3] === 0x0a &&
        h[4] === 0x30 && h[5] === 0x33 && h[6] === 0x35 && h[7] === 0x00) {
      dexPtr = p;
      break;
    }
  } catch (e) {}
}
var header = new Uint8Array(dexPtr.readByteArray(0x40));
var size = new DataView(header.buffer).getUint32(0x20, true);
var file = new File("/data/user/0/<pkg>/files/starry_dex_" + dexPtr + ".dex", "wb");
file.write(dexPtr.readByteArray(size));
file.close();
```

之后：

```bash
adb shell su -c 'find /data/user/0/<pkg>/files -name "starry_dex_*.dex" -exec chmod 644 {} \;'
adb pull /data/user/0/<pkg>/files/<导出文件>
```

导出后必须验货：

- `dexdump -f` 能显示 `class_defs_size`、`string_ids_size`。
- `jadx` 能反编译出业务类，不能只有壳类。
- 多个 loader 导出的相同 DEX 应做 SHA-256 去重。

## 5. 第四步：无壳重建

优先使用游戏运行时已经落地/更新的 `base.odex.apk` 作为底包；它通常已经包含热更资源、运行时 `libil2cpp.so`、runtime metadata。

重建最小规则：

1. 用真实 DEX 依序替换/补齐 `classes.dex`、`classes2.dex`、`classes3.dex`。
2. 将星光/业务 wrapper 编成最后一个连续 DEX；wrapper `.super` 指向真实 Application。
3. Manifest 的 `android:name` 从 NIS stub 改为 wrapper，wrapper 再继承真实 Application。
4. 保留官方 `resources.arsc`、`libil2cpp.so`、`libunity.so`、`global-metadata.dat` 等字节不变。
5. 不保留壳 Application 加载链；新证书只签重建包。
6. 重新签名后先静态验包，再真机验证。

无悔华夏 4399 实机链：

```text
真实 Application: com.excelliance.open.LBApplication
              -> com.tpf.sdk.TPFApplication
wrapper:      com.starry.magic.StarryApp -> LBApplication
底包:         base.odex.apk
真实 DEX:     3 个 InMemoryDexFile
```

## 6. 验收证据

- 真实 DEX：`dexdump`/`jadx` 可读，业务类存在。
- 无壳包：Manifest Application 已变成 wrapper，壳类不再被入口引用。
- 字节不变量：`resources.arsc`、游戏 so、runtime metadata 与底包逐文件同 hash。
- 新 so：包内 so hash == 构建产物 hash，且有目标游戏标记。
- 签名：v1/v2/v3 与 zipalign 通过。
- 运行：启动日志、菜单开关日志、功能人工验收。

## 7. 本次实机参考实现

- 精确 dump：`04_scripts/无悔华夏4399/whrhx4399_dump_inmemory_dex.js`
- 无壳重建：`04_scripts/无悔华夏4399/whrhx4399_surgical_unshelled.sh`
- 完整记录：`10_docs/无悔华夏4399/实现进度_3.8.280_20260914.md`
- 落地包：`08_output/无悔华夏4399/无悔华夏4399_3.8.280_无壳七功能-releaseSigned.apk`

## 8. 迁移到好游快爆/其他 NIS 包

1. 先确认壳版本和真实 Application（不假设与 4399 相同）。
2. 如果是 `7.6.1_857` 且启动后有 InMemoryDexFile，直接套本方法。
3. 运行时热更 IL2CPP 如果 hash 与既有游戏一致，可复用现有 RVA/按名链；否则先重新 dump metadata + hotfix so。
4. 仍然要重新验：
   - Manifest 真实 Application 链；
   - DEX 连续编号；
   - 官方资源/so 不变量；
   - 新包签名和启动；
   - 游戏 SDK 的签名/渠道校验是否走 `PackageManager`（可配 SignSpoof）还是读 APK 文件（要另处理）。

不要把“4399 成功”直接外推成“所有易盾都能 cookie dump”：先验证是否真的存在 `InMemoryDexFile`。若没有，回到 native OpenMemory / FART 路线。
