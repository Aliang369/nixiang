# 我在神界刷装备 157.0 决定性证据（2026-09-29）

## 正式产物

- APK：`08_output/我在神界刷装备/我在神界刷装备_157.0_surgical-releaseSigned.apk`
- 大小：416,576,568 B
- md5：`a76696281dee1dfff2eb8b224b70d082`
- sha256：`b3e7ef73d134f2008f48a8b39a105b75ea01318c1b5e4c8075791f4571000fdc`
- `classes3.dex`：456,568 B，md5 `3a01f84f4d45fb2bf7dd87005fa79b00`
- 包内 so md5：`05f79723454724fcbe646be4ffecef7f`

## 门禁与安装

- 新 dex 字符串回读命中：`|ssaid=`、`android_id`、`v5`、`v4`、`auth_device_id_version`、`legacy_device_ids`。
- `pre_install_gate.sh`：JS 语法与菜单控件检查通过，汇总 `PASS=2 / FAIL=0 / WARN=0`；JNI 检查因该 native 实现无 `regOne` 动态注册而 SKIP。
- apksigner：v2、v3 有效；`zipalign -c -p 4` 通过。
- 设备 serial：`f9a116a2`；`pm install -r -d --user 0` 成功，base.apk md5 与本包一致。
- 安装态：versionCode 157、versionName 157.0、primaryCpuAbi arm64-v8a、lastUpdateTime 2026-09-29 19:09:21。

## 最短复现命令

```bash
bash 04_scripts/我在神界刷装备/sjzz_build_so.sh
bash mod菜单/starry-mod-menu/scripts/surgical_sjzz_157.0.sh
bash 04_scripts/_common/pre_install_gate.sh \
  --js /tmp/sjzz_eval_patched_delay.js \
  --cpp mod菜单/starry-mod-menu/app/src/main/jni/Games/SjzzMod.cpp \
  --dex /tmp/sjzz_classes3.dex \
  --so mod菜单/starry-mod-menu/libs_sjzz/arm64-v8a/libstarry_magic.so \
  --menu-json 10_docs/我在神界刷装备/wzsjszb_menu_template.json
```

## NOT PROVEN

- 旧设备绑定迁移到 v5 的实际服务端登录结果未验证。
- 远端 auth-server 是否已部署本轮 `legacy_device_ids` 支持未验证。
- 免广告、伤害倍率、无敌、x4 加速的最终实机结论以用户反馈为准；AI 未操作屏幕。
