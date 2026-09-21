# 勇士之心 热更修复 证据（2026-09-21）

> 本目录只留决定性证据行；完整 logcat 不入库、已按滚动物清理。
> 复现链：`04_scripts/勇士之心/regrab_ysxz_hotupdate.sh` → `decrypt_hotupdate_ysxz.py`
> → `simulate_ysxz_discovery.py` → `ysxz_build_so.sh` → `ysxz_surgical_rebased.sh` → `ysxz_sign.sh`
> → `install_ysxz.sh`（ALLOW_UNINSTALL=1）。

## 1. 设备与产物身份

| 项目 | 值 |
|---|---|
| 设备 serial | `f9a116a2` |
| 包名 | `com.fightingstars.games.warrior` |
| 安装版本 | `1.0.3 / vc5`，热更 res `1.0.98.10` |
| 底包（真机只读拉取官方） | md5 `c20f70c8ba52032eae10574f846b88d1`，63,864,339 B |
| 底包内 `lib/arm64-v8a/libcocos2djs.so` | sha256 `8459ad50…c719`（与 09-15 原版一致） |
| 星光 SO | md5 `9bf52c07974ff487a03e6889472f36f6` |
| 星光包 | `08_output/勇士之心/勇士之心_1.0.3_surgical-releaseSigned.apk` md5 `4175178fe6f9663556efe21541726f6e` |
| 设备 base.apk | md5 `4175178fe6f9663556efe21541726f6e`（与产物一致） |

## 2. 决定性日志行（装机后冷启动 → 进战斗）

```
I/Mod_Menu: [Ysxz] hooked se::evalString @0x741c33e8e4 rva=0x3138E4
I/Mod_Menu: [Ysxz] early hook ok iter=60
I/StarryMagic: [ysxz] 初始化成功
I/MenuService: overlayVisible=true attached=true
I/Mod_Menu: [Ysxz] logic splice #1 len 6171927->6193278 file=assets/game/index.js
I/Ysxz: [MODMOD] auto-battle privilege hooked = .prototype.fwW
I/Ysxz: [MODMOD] REPORT  formula=undefined apply=undefined gate=undefined cost=undefined death=undefined movesys=NO auto=yes
I/Mod_Menu: [Ysxz] logic splice #2 len 307609->328960 file=assets/battleLogic/index.js
I/Ysxz: [MODMOD] names: hx=hx.bmh  camp=hD.bkR  ctx=bnl/fF/dcf  hp=bgD  world=hi
I/Ysxz: [MODMOD] battle-logic class hit: damage formula = .Gr
I/Ysxz: [MODMOD] feature1 hooked
I/Ysxz: [MODMOD] skill cost = .FA  fields: maxCharge=Do charge=Dn cd=cd mpCost=Dp hpCost=Dq
I/Ysxz: [MODMOD] skill gate = .Fz  unlockedField=Dr
I/Ysxz: [MODMOD] feature3 hooked (gate=true cost=true)
I/Ysxz: [MODMOD] feature2 death-route hooked = .Hf
I/Ysxz: [MODMOD] entity class hit: damage-apply = .prototype.hX
I/Ysxz: [MODMOD] feature2 hooked
I/Ysxz: [MODMOD] movement system hit
I/Ysxz: [MODMOD] feature4 hooked
I/Ysxz: [MODMOD] REPORT  formula=Gr apply=hX gate=Fz cost=FA death=Hf movesys=yes auto=NO
I/StarryMagic: [验证] grant ok exp=1790057171
I/Mod_Menu: [Ysxz] feature #1=ON   （菜单开关同步，1/2/3/4/5 均见 ON/OFF 记录）
```

说明：
- `splice #1` 的 6,171,927 与 `splice #2` 的 307,609 分别等于热更后
  `assets/game/index.js`、`assets/battleLogic/index.js` 的解密字节数 → 两个 bundle 都被捕获。
- `names: hx=hx.bmh camp=hD.bkR ctx=bnl/fF/dcf hp=bgD world=hi` 与离线
  `simulate_ysxz_discovery.py` 对新 res 的预测完全一致（混淆名漂移被动态吸收）。
- 两个 REPORT 分工：game bundle 报 `auto=yes`，battleLogic bundle 报六个战斗钩子；
  合并即「六钩子 + 自动战斗全部挂上」。
- 崩溃哨兵：本轮 logcat 无 `FATAL EXCEPTION` / `ANR in` / `SIGSEGV`。

## 3. 最短复现命令

```bash
bash 04_scripts/勇士之心/regrab_ysxz_hotupdate.sh                 # 只读重抓 23 个热更 JS + base.apk
python3.11 04_scripts/勇士之心/decrypt_hotupdate_ysxz.py          # XXTEA(key 前16B)+zlib 解密
python3.11 04_scripts/勇士之心/simulate_ysxz_discovery.py \
  06_dumped/勇士之心/hotupdate_20260921 --auto-fp NEW --fingerprints
bash 04_scripts/勇士之心/ysxz_build_so.sh                         # 生成 payload + ndk-build
bash 04_scripts/勇士之心/ysxz_surgical_rebased.sh                 # 以真机 base.apk 外科组装
bash 04_scripts/勇士之心/ysxz_sign.sh
ALLOW_UNINSTALL=1 bash 04_scripts/勇士之心/install_ysxz.sh
```

## 4. 人工验收（用户反馈，第一来源）

2026-09-21：用户实机测试完成，**五项功能全部可用**
（秒杀 / 无敌 / 无限技能 / 移动速度 / 自动战斗）。

## 5. NOT PROVEN

- 长时间战斗稳定性（本轮只做短时验收，无长时间压测）。
- 服务端风控结论（本作未做专门联网风控评估）。
