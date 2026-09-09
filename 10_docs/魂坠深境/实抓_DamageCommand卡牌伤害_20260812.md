# 实抓更正：改的是 DamageCommand 卡牌伤害（2026-08-12 22:13）

## 用户

开齐功能打完：斌哥改的是**卡牌伤害**。

## 斌哥包 Frida（`binge_recapture_2212`）

跳板 `0x28BD0B0` 命中名 `GenerateDamage` 时：

| params | 类 |
|--------|-----|
| [0] | **`DamageCommand`** |
| [1] | `GameStateInfo` |
| [2] | bool/int |

此前星光把 params[0] 当 `Damage`，klass 校验直接丢掉 → `isP=255`、无敌/秒杀不生效。

## DamageCommand 字段

| off | 字段 |
|-----|------|
| **+0x10** | `baseDamage` ← 斌哥乘/置零 |
| **+0x38** | `targetBFT` ← 取 isPlayer |
| BFT+0x5c | isPlayer |

与壳 `str [x24,#0x10]` / `ldr [x24,#0x38]` **完全对齐**（对象是 Command 不是 Damage）。

## 星光

`HzsjMod.cpp` 按 `DamageCommand` 改 `baseDamage`；装机后日志应见 `GD# p0=DamageCommand base=… isP=0/1`。
