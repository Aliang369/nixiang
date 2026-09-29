# 无限升级 2026-09-29 无壳重建 jOOR 修复证据

## CLAIM

真机安装无壳重建包后出现的“菜单崩溃”，根因不是 Starry 菜单，而是重建 DEX 集缺少
TapTap SDK 的运行期依赖 `org.jooq:joor-java-8:0.9.13`。

TapTap `ServiceFactoryCollector.collect()` 通过 `org.joor.Reflect` 按类名反射调用各
`*_ServiceFactory.provide()`；`ReflectUtils.provide()` 捕获并吞掉所有异常，因此 jOOR
缺失时服务工厂全部静默返回空表。随后初始化读取 `DBService` / `OpenLogService` 时强转空值，
触发主线程 NPE，最终由 Starry 菜单的崩溃提示暴露。

## 决定性证据

本轮真机抓到的原始崩溃栈关键行如下；设备上的完整 crash 文件后续已不存在，本文件保留
决定性行，不提交完整 logcat。

```text
E TapTapSdk: null cannot be cast to non-null type com.taptap.sdk.core.DBService
E TapTapSdk: java.lang.NullPointerException
    at TapTapSdkInitializer$special$$inlined$inject$2.invoke(ServiceManagerComponent.kt:11)
    at TapTapSdkInitializer.getDbService(TapTapSdkInitializer.kt:42)
    at TapTapSdkInitializer.initialize(TapTapSdkInitializer.kt:125)

FATAL EXCEPTION: main
java.lang.NullPointerException:
null cannot be cast to non-null type com.taptap.sdk.common.services.OpenLogService
    at TapTapSdkInitializer$special$$inlined$inject$1.invoke
    at TapTapSdkInitializer.getOpenLogService
    at TapTapSdkInitializer$initialize$1$1.onGateKeeperChange
```

静态链路证据：

- `ServiceFactoryCollector.collect()` 固定反射 14 个 `com.taptap.sdk.services.*_ServiceFactory`。
- `ReflectUtils.provide()` 的 `catchall` 直接返回 `MapsKt.emptyMap()`。
- `Sdk_tap_db_ServiceFactory.provide()` 注册 `DBServiceImpl`。
- `Sdk_tap_openlog_ServiceFactory.provide()` 注册 `OpenLogServiceImpl`。
- 修复前 6 个运行时 DEX 只有 `Lorg/joor/Reflect;` 引用，没有任何 `Lorg/joor/*` 定义。

## 修复

TapTap `tap-servicemanager:4.8.1-beta.7` POM 锁定依赖：

```text
org.jooq:joor-java-8:0.9.13
```

处理方式：

```bash
curl -fsSL -o /tmp/joor-java-8-0.9.13.jar \
  https://repo1.maven.org/maven2/org/jooq/joor-java-8/0.9.13/joor-java-8-0.9.13.jar
~/Library/Android/sdk/build-tools/36.0.0/d8 \
  --min-api 23 --output /tmp/joor_d8 \
  01_tools/wxsj-runtime-libs/joor-java-8-0.9.13.jar
cp /tmp/joor_d8/classes.dex \
  06_dumped/无限升级/runtime_20260928_unshelled/joor.dex
```

固定指纹：

| 输入/产物 | SHA-256 |
|---|---|
| `joor-java-8-0.9.13.jar` | `caa2ff6d5eae3794714ce7dc45d96279ca9abaf5dca2d9e554b577a8c4c635a8` |
| `joor.dex` | `cafb2f539b65f19da71d81b917f4fca2376f023aa194dfd2bee812bb70119d52` |
| `wxsj_surgical_unshelled.sh` | `99eacec45d7d716494597bad5aa5de78b081efdba93c779c2bf643e1946f12c7` |

`04_scripts/无限升级/wxsj_surgical_unshelled.sh` 已把 `joor.dex` 列为固定输入和固定哈希，
并把 `Lorg/joor/Reflect;` 加入构建期类断言。

## 正式包

> 2026-09-29 更新：本轮 B 线（11.266018/vc1637）收官时按「收尾+正式包」口令重新裁剪了
> `08_output/无限升级`，本段这份 12.10701 包已被删除（目录只保留当日唯一终包）。删除记录见
> `10_docs/无限升级/正式包裁剪清单_vc1637_20260929.json`；该包仍可由
> `04_scripts/无限升级/wxsj_surgical_unshelled.sh` 从 `07_patched/无限升级` 输入复现。

- 路径：`08_output/无限升级/无限升级_12.10701_unshelled-menu-releaseSigned.apk`
- 大小：`121243296` B
- SHA-256：`a730e987a4a0c7949b7aab87947188457bbc185f016f2e5fb5a73915a319c39b`
- 签名：v2 + v3，`CN=StarryMagic`
- 包内 DEX：`classes.dex` 到 `classes7.dex` 连续；`classes6.dex` 为 jOOR
- 设备 f9a116a2 已覆盖安装，设备 `base.apk` SHA-256 与本地正式包一致

## 收尾清理

- `08_output/无限升级` 已裁剪为唯一正式 APK；被删除的 `.idsig` 记录在 `10_docs/无限升级/正式包裁剪清单_20260929.json`。
- 本机 `/tmp/wxsj*` 临时产物释放约 `2.45 GiB`。
- 设备 f9a116a2 删除 `/data/local/tmp/wxsj_dex*`、`wxsj_mem_dex_scan`、`wxsj_new.dll`，约 `620 MB`；`wxsj_new.dll` 删除前已确认与 `07_patched/无限升级/targetfuntional_OFFICIAL_12.10701_hotfix3_2028544.dll` hash 一致。
- 保留 `wxsj_salvage_current.sav`、`starry_magic` 设备种子、官方底包、热更 DLL 和无壳重建 DEX。

## VERDICT

- 静态修复与正式包组装：通过。
- 真机启动后 TapTap 服务注册是否恢复、菜单是否稳定：NOT PROVEN，本轮未取得用户启动后的新日志。
- 冲鸭变速器容器内启动：NOT PROVEN；此前易盾 native 跳板 SIGILL 卡点仍未解除。
