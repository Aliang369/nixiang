# 斌哥 GenerateDamage 抄法钉死（2026-08-12 21:57）

## 星光失败日志

```
combat raw=60 cls=43 seen=0 heal=0 kill=0 off hp@88 isP@92
```

→ 堆扫 BFT **认得出类、认不出可写实例**（SoftBft 全灭）。**扫血路线对本局无效。**

此前钩 `0x28BD0B0` 时有 `GD# dmg+0x10=4/7/13`，说明 **跳板名过滤 GenerateDamage 是通的**；闪退来自乱写 params/BFT，不是跳板本身不能碰。

## 斌哥死条件（shell handler @ `0x758c0c2894`）

| 项 | 值 |
|----|-----|
| 钩点 | `libil2cpp` RVA **`0x28BD0B0`**（file `0x28B90B0`） |
| ABI | `x1=MethodInfo*`, `x2=params` |
| Damage | `params[0]` |
| BFT | **`[Damage+0x38]`**（不是先用 Frida 的 +0x68） |
| isPlayer | `ldrb [BFT+0x5c]`，**`cbnz`=玩家**（非 0 即是） |
| 改什么 | **只 `str` `Damage+0x10`**：秒杀 `*100`（`cmp≥1`），无敌 `=0` |
| 不改 | BFT.health 堆扫（星光曾误走） |

旗标：壳数据 `+0x728` 秒杀 / `+0x72c` 无敌（星光用菜单 Toggle 代替）。

## 星光实现

`HzsjMod.cpp`：只 Dobby `0x28BD0B0` + 按上表改 `+0x10`；去掉战斗堆扫。

验：`adb logcat -s Mod_Menu` → `GD# val+0x10=… isP=0/1`（开秒杀后敌方 val 应变大，开无敌打自己 val→0）。
