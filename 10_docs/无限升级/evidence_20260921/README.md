# 无限升级 20260921 证据

## 决定性日志行

```text
[Wxsj] hook installed ...
[Wxsj] patched dll injected (hit #1, patched=1953792 pad=7168)
TapTapLoginActivity sanitized=true noopProxy=true
```

## 版本与产物

```text
官方：11.266020 / vc1641
运行时热更 DLL：1960960 B
DLL SHA-256：1cecb4703ca3dd5ac7558a0eb63ac1367904594bac5717dea35d0756f3ae4cf3
正式包 MD5：2c76afeee3c900cd0896e4a6a6e206f9
正式包 SHA-256：1d8fcb3a070f57f1973da2047916d7d145bd3bab8636d9e0487bff369eca8f32
```

## 复现命令

```bash
bash 04_scripts/_common/verify_installed_apk.sh f9a116a2 \
  com.xingzuodaoshi.infinityUpgrade \
  08_output/无限升级/无限升级_11.266020_surgical-final-releaseSigned.apk --su
```

## 说明

- 用户确认游戏测试通过，作为本轮功能行为证据。
- 正式包与设备当前 `base.apk` MD5 一致。
- 完整 logcat 未入库；以上为收尾保留的关键行。
