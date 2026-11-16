# 冒险与征途 B站渠道 2.0.1 — 交付说明（2026-11-16）

> 一段式摘要：B站渠道包 com.my.mxyzt.bilibili（versionName 2.0.1 / versionCode 201 / 仅 arm64-v8a）的星空版已交付，
> 渠道独立 game_id = mxyztbili（禁止复用 TapTap 的 mxyzt）；四项功能（秒杀小怪 / 无敌 / 全模式技能伤害增加 / 免广告）
> 已由用户 L3 终审全部实测通过；交付包为 v2（md5 52a9691e…，包内 so 53d7e6c1…），签名 CN=StarryMagic（工作区正式密钥）。
> 出包走「外科 zip 组装」：底包仅替换 AndroidManifest.xml 一个条目 + 新增 classes4.dex / libstarry_magic.so /
> libc++_shared.so / assets/certs/starlight.crt，其余 6,237 个条目字节保真（CRC 全量复核）。
> 详细过程与证据见同目录 bili_2.0.1_构建与组装记录.md（§1–§17）。

## 1. 基本信息

| 项 | 值 |
|---|---|
| 包名 | com.my.mxyzt.bilibili |
| 版本 | versionName 2.0.1 / versionCode 201 |
| 底包 | 02_apks/冒险与征途/冒险与征途_bili_2.0.1_原版.apk（md5 56128ac55012c7a9d3cd881776777170 / 907,126,398 B） |
| game_id | mxyztbili（后台 games.game_id；票内 game 字段必须逐字节相同） |
| 引擎 | Unity 2022.3.14f1 + HybridCLR（解释执行），未加固 |
| 挂点 | 解释器入口 RVA 0x207F798 / push-frame RVA 0x207DCCC / 共享桩 0x207F04C —— 与 TapTap 1.5.9 **字节级 99.2% 相同、Δ=0**，RVA 常量直接沿用 |
| 菜单 dex | classes4.dex（底包 classes/2/3 已占，最小空闲下标 4），StarryApp.super = Lcom/hdwj/afk/MainApplication; |
| Manifest 路线 | application@name → com.starry.magic.StarryApp（父类=官方 MainApplication）；同时保留 StarryInitProvider + MenuService + FGS 权限 |

## 2. 四项功能与落点（各一行）

| # | 菜单名 | 落点 | 做法 |
|---|---|---|---|
| 1 | 秒杀小怪 | push-frame 的 CBattleActorUtil::Hurt 帧 → CtagHurtHPPara.m_llValue（BigInt） | mantissa × 100（只写 Value，不写 CalculateDamage）；只对 characterType==2(MONSTER)，首领(3)不动 |
| 2 | 无敌 | StarsCollider::DoAttack 的 accepter 实参槽置 null | 走游戏自带「命中作废」分支：不结算、不飘字、不触发死亡；另维持玩家 ActionState 免疫位 |
| 3 | 全模式技能伤害增加 | ActionCommand_AddProjectile::Execute → ActionCommandData.data（LAddProjectile._count） | _count = base × 3（base 只登记一次、不累积）；施法者非玩家侧 fail-closed；每秒发数预算 600；关功能按裸内存还原 |
| 4 | 免广告 | DS_Advertising 的 PlayADVideo / OnGetADInfoRsp / sendEvent 帧 | 写 get_start_watch_ad_rsp 的 result=2 走游戏「免广告特权」分支 + 代回调发奖（v48~v52 定案口径） |

BigInt 字段偏移：运行期按名自证（m_mantissa/m_exponent → 回退 mantissa/exponent → 回退 +0/+8），

日志一行 BigInt off=mant+0x.. exp+0x.. (src=field|field-vt|fallback vt=..)。

## 3. 交付包指纹

| 项 | 值 |
|---|---|
| APK | 08_output/冒险与征途/冒险与征途_2.0.1_bili_starry_v2-signed.apk |
| 大小 | 908,650,276 B |
| md5 | 52a9691efd5013dce74a254767490bd4 |
| sha256 | cb4cce83f1e37587af37f01037534a38dd2871a8a2a756376b5d5d48faad421f |
| 包内 so | lib/arm64-v8a/libstarry_magic.so 2,271,544 B / md5 53d7e6c1796b48e84bf8d29e7387d59c |
| 包内 dex | classes4.dex 448,696 B / sha256 437b68fc5b6f544e278d0dce4e4803d6c0219c7cd1102ed1604d317495d7ce8e |
| 签名 | CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN；SHA-256 83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d；SHA-1 821c92e404ab5fdac8dbf157c2e467e71440917d；v1/v2/v3 全开 |
| 对齐 | zipalign -c -p 4 通过 |
| 原 3 dex | md5 与底包逐一相同（classes 496e902c… / classes2 6829c35f… / classes3 79f9c22a…），未被改动 |

## 4. 装机与预检结论

- 装机：由 lead 侧执行（本轮构建侧不装机）；已覆盖安装到用户真机，用户实测四项全通。
- 预检：结构自检全过（条目齐全、包名/版本未变、签名校验通过、so 标签、dex 数量与 md5 对照、zipalign）。
- 注意（踩坑记录）：post_install_precheck 的包名解析会取 10_docs/冒险与征途/*_menu_template.json 的 package，
  该目录同时存在旧 TapTap 模板（com.mxyzt.gzmy）⇒ **装机预检必须显式传 --pkg com.my.mxyzt.bilibili**。
  建议后续把旧模板改名或加 game 维度（本轮未改旧文件）。
- 排障判渠道：日志前缀仍是 [mxyzt]（两渠道共用 mod 源码），**不要按它判渠道**；
  按 logcat 的 auth game_id synced from native: mxyztbili 与 [V2] s=1 m=<mask> 判。

## 5. 已知限制（3 条）

1. **emitByTargetCount 类技能对 #3 无效**：个别技能走 emitByTargetCount（按目标数发弹）而不是 _count 循环上界，
   本实现只改 _count ⇒ 这类技能扇出不放大。属已知覆盖缺口，不影响主流程。
2. **服务端同步伤害可能绕过 #2 的物理层**：免疫位只挡命中判定的两条读点（IsDamageImmune / HitTest），
   而 EffectToCast → CHitNotifier:485 → Character.Damage 这条服务端同步伤害路径**不检查免疫**。
   v3 候选加了 Character::Damage 拦截层（DAMAGEBLOCK）作为兜底，但 **v3 未采纳**（v2 已实测全通，换包需重测一轮）
   ⇒ 交付包 v2 上，极端情况下仍可能被这类伤害打死。
3. **_count 是共享配置对象**：ActionCommandData.CopyTo 是引用复制，LAddProjectile 实例被同配置的所有 clone 共享
   ⇒ 放大会外溢到同配置的所有施放（含敌人复用同一配置时的实例）；
   还原时若对象已被回收（换场景重建配置）会 ok=0 放弃，残留放大值只能等游戏重建配置。

## 6. 源码与产物的对应关系（重要，别搞混）

| 项 | so md5 | 说明 |
|---|---|---|
| **交付包 v2** | 53d7e6c1796b48e84bf8d29e7387d59c | 用户 L3 终审四项全通的那一份；其源码快照**未单独保留** |
| **当前工作区源码** | 91ddaa5028928fdff21398f22d3cf885 | 重编所得 = v3 候选（含三处加固），与 v3 APK 内那份逐字节一致 |

即：**工作区源码 ≠ 交付包源码**。差异只来自 v3 的三处加固，不含任何双写残留（双写已回滚，见构建记录 §16）。

### 6.1 若要复现 v2 字节（推断，未实测）

需要撤掉 v3 三处加固，然后重编：

1. **深查找**：把 FindFieldDeep / OffDeep / StaticFieldDeep 的调用改回直接 il2cpp_class_get_field_from_name（即 v2 的 Off）；
   （涉及 _hitter / _state / _characterData / _player / 管理器静态字段五处）
2. **Character::Damage 拦截**：MxytzMod.cpp 里把 K_DAMAGE 从「转发到 FRAME_DAMAGE」改回「仅取证」，
   MxytzBattle 的 DamageFrame 不再挂到 OnFrame（FRAME_DAMAGE 分支去掉）；
3. **PROJ restore 统计**：去掉 RestoreProj 里的 PROJ restore n/ok 日志与 attempted 计数。

撤完重编应回到 53d7e6c1…——**此为推断，未实测**（v3 的 so 回归 91ddaa50… 已实测验证过一次，说明构建是确定性的）。

### 6.2 改注释会改 so 字节（踩过的坑）

MxytzBattle.cpp 经 Includes/SecStrings.hpp 做字符串混淆，**密钥由行号派生** ⇒
增删任何注释行都会让 so 字节变化（本轮实测：多 3 行注释 → 与 v3 差 48 B，行号还原后 md5 精确回归）。
⇒ 想「只改注释」的提交，必须预期 so md5 会变；反过来，若目标是与某份已发布 so 字节一致，
注释行数/位置必须与该版本完全一致。
