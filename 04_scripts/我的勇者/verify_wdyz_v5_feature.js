// 我的勇者 v5 — 功能级验证：①无敌(GM AddFlag(5)) ②秒杀(MonsterCore.SettleDamage 放大) ③无限技能(TryEnterCd 跳过 + settleMp 归零) ④ACTk 布防观测
// 设计要点：
//  - A2 走官方 GM 链等价路径：piggyback 在 BattleManager.GetMyHero 的 onLeave（主线程）调 HeroGroup.AddFlag(5)/subFlag(5)，不跨线程调 il2cpp
//  - B1 用 Interceptor.replace + NativeCallback 包装（float 参数走 SIMD，attach onEnter 读不到），默认只放大 value<0 分支，保底不改治疗
//  - C2 TryEnterCd 直接跳过（不进 CD）；C3 settleMp 归零只对消耗分支（默认负值=消耗，观测后可切）
//  - ④ 只读观测：CodeStage 检测器 instancesInScene 静态字段 + StartDetection hook；全程不主动碰 Obscured 字段/蜜罐
"use strict";

const RVA = {
  GetMyHero:  0x45F9A3C, // BattleManager.GetMyHero() -> HeroGroup（实例方法）
  hgAddFlag:  0x45F1E64, // HeroGroup.AddFlag(RoleFlagTypeEnum)  反汇编证实 GM 无敌链
  hgSubFlag:  0x45F1E94, // HeroGroup.subFlag
  monSettle:  0x45F8524, // MonsterCore.SettleDamage(f,f,f,f,i) override
  heroSettle: 0x45EE170, // HeroCore.SettleDamage（备用 A1，本脚本只探针不挂）
  settleMp:   0x45EE2B4, // HeroCore.settleMp(int)
  tryEnterCd: 0x4668EC8, // SKillBase.TryEnterCd() private void
};

const S = {
  inv: false, invApplied: false,
  kill: false, killMode: 'neg',   // neg: 只放大 value<0（doc 口径）；pos: 反转口径时用
  skill: false,
  mp: false, mpMode: 'neg',       // neg: 只归零负值消耗；all: 全归零
};
const C = { gh: 0, mon: 0, mp: 0, skillBlocked: 0, actk: 0 };
let obsMon = 0, obsMp = 0;
const NULLPTR = ptr(0);
let bmInst = NULLPTR, hgLast = NULLPTR;

function prologue(addr) {
  try {
    const b = new Uint8Array(addr.readByteArray(8));
    if (b.every(x => x === 0)) return "ALLZERO!";
    return Array.from(b).map(x => ('0' + x.toString(16)).slice(-2)).join(' ');
  } catch (e) { return "UNMAPPED!"; }
}

function main() {
  const mod = Process.getModuleByName("libil2cpp.so");
  const A = {};
  for (const [k, rva] of Object.entries(RVA)) {
    A[k] = mod.base.add(rva);
    console.log("[P] " + k + " @" + A[k] + "  " + prologue(A[k]));
  }
  globalThis.__A = A; globalThis.__S = S; globalThis.__C = C;

  // ---------- il2cpp 导出（观测用） ----------
  const ex = (n) => { try { return mod.findExportByName(n); } catch (e) { return null; } };
  const f = {};
  for (const [k, n, ret, args] of [
    ["domain_get", "il2cpp_domain_get", "pointer", []],
    ["domain_get_assemblies", "il2cpp_domain_get_assemblies", "pointer", ["pointer", "pointer"]],
    ["assembly_get_image", "il2cpp_assembly_get_image", "pointer", ["pointer"]],
    ["image_get_name", "il2cpp_image_get_name", "pointer", ["pointer"]],
    ["image_get_class_count", "il2cpp_image_get_class_count", "int", ["pointer"]],
    ["image_get_class", "il2cpp_image_get_class", "pointer", ["pointer", "int"]],
    ["class_get_name", "il2cpp_class_get_name", "pointer", ["pointer"]],
    ["method_from_name", "il2cpp_class_get_method_from_name", "pointer", ["pointer", "pointer", "int"]],
    ["field_from_name", "il2cpp_class_get_field_from_name", "pointer", ["pointer", "pointer"]],
    ["static_get_value", "il2cpp_field_static_get_value", "void", ["pointer", "pointer"]],
  ]) { const p = ex(n); if (p) f[k] = new NativeFunction(p, ret, args); }
  globalThis.__f = f;

  // ---------- ① 无敌：piggyback GetMyHero（主线程执行 AddFlag/subFlag） ----------
  const fnAddFlag = new NativeFunction(A.hgAddFlag, 'void', ['pointer', 'int']);
  const fnSubFlag = new NativeFunction(A.hgSubFlag, 'void', ['pointer', 'int']);
  Interceptor.attach(A.GetMyHero, {
    onEnter(args) {
      C.gh++;
      if (bmInst.isNull()) { bmInst = args[0]; console.log("[O] BattleManager inst=" + bmInst); }
    },
    onLeave(ret) {
      const hg = ret.returnValue;
      if (hg.isNull()) return;
      hgLast = hg;
      if (S.inv && !S.invApplied) {
        S.invApplied = true; fnAddFlag(hg, 5);
        console.log("[F] INV ON  -> HeroGroup.AddFlag(5) hg=" + hg);
      } else if (!S.inv && S.invApplied) {
        S.invApplied = false; fnSubFlag(hg, 5);
        console.log("[F] INV OFF -> subFlag(5) hg=" + hg);
      }
    }
  });

  // ---------- ② 秒杀：MonsterCore.SettleDamage 包装 ----------
  const origMon = new NativeFunction(A.monSettle, 'void', ['pointer', 'float', 'float', 'float', 'float', 'int']);
  Interceptor.replace(A.monSettle, new NativeCallback(function (self, value, shield, tgh, energy, grp) {
    C.mon++;
    if (obsMon < 40) {
      obsMon++;
      let g = -1; try { g = self.add(0x74).readS32(); } catch (e) {}
      console.log("[O] monSettle#" + obsMon + " v=" + value.toFixed(1) + " shield=" + shield.toFixed(1)
        + " tgh=" + tgh.toFixed(1) + " en=" + energy.toFixed(1) + " grp=" + grp + " thisgrp=" + g);
    }
    if (S.kill) {
      if (S.killMode === 'neg' && value < 0) value = -1e9;
      else if (S.killMode === 'pos' && value > 0) value = 1e9;
    }
    origMon(self, value, shield, tgh, energy, grp);
  }, 'void', ['pointer', 'float', 'float', 'float', 'float', 'int']));

  // ---------- ③a 免蓝：HeroCore.settleMp 包装 ----------
  const origMp = new NativeFunction(A.settleMp, 'void', ['pointer', 'int']);
  Interceptor.replace(A.settleMp, new NativeCallback(function (self, v) {
    C.mp++;
    if (obsMp < 20) { obsMp++; console.log("[O] settleMp#" + obsMp + " v=" + v); }
    if (S.mp) {
      if (S.mpMode === 'all') v = 0;
      else if (S.mpMode === 'neg' && v < 0) v = 0;
      else if (S.mpMode === 'pos' && v > 0) v = 0;
    }
    origMp(self, v);
  }, 'void', ['pointer', 'int']));

  // ---------- ③b 无 CD：SKillBase.TryEnterCd 跳过 ----------
  const origTry = new NativeFunction(A.tryEnterCd, 'void', ['pointer']);
  Interceptor.replace(A.tryEnterCd, new NativeCallback(function (self) {
    if (S.skill) { C.skillBlocked++; return; }
    origTry(self);
  }, 'void', ['pointer']));

  // ---------- ④ ACTk 布防观测（只读） ----------
  try {
    if (f.domain_get && f.image_get_class_count) {
      const szp = Memory.alloc(8);
      const arr = f.domain_get_assemblies(f.domain_get(), szp);
      const n = szp.readULong();
      let img = null;
      for (let i = 0; i < n; i++) {
        const a = arr.add(i * Process.pointerSize).readPointer();
        const im = f.assembly_get_image(a);
        const nm = f.image_get_name(im).readUtf8String();
        if (nm.indexOf("CodeStage") >= 0 || nm.indexOf("AntiCheat") >= 0) { img = im; console.log("[A] image: " + nm); break; }
      }
      if (!img) console.log("[A] CodeStage image NOT found");
      else {
        const want = { InjectionDetector: 1, ObscuredCheatingDetector: 1, SpeedHackDetector: 1, TimeCheatingDetector: 1 };
        const cnt = f.image_get_class_count(img);
        for (let i = 0; i < cnt; i++) {
          const c = f.image_get_class(img, i);
          if (c.isNull()) continue;
          const nm = f.class_get_name(c).readUtf8String();
          if (!want[nm]) continue;
          try {
            const fld = f.field_from_name(c, Memory.allocUtf8String("instancesInScene"));
            if (fld && !fld.isNull() && f.static_get_value) {
              const buf = Memory.alloc(8);
              f.static_get_value(fld, buf);
              console.log("[A] " + nm + ".instancesInScene=" + buf.readS32());
            }
          } catch (e) { console.log("[A] " + nm + ".instancesInScene 读取失败"); }
          for (let pc = 0; pc <= 4; pc++) {
            try {
              const m = f.method_from_name(c, Memory.allocUtf8String("StartDetection"), pc);
              if (m && !m.isNull()) {
                const fp = m.readPointer();
                if (!fp.isNull()) {
                  Interceptor.attach(fp, { onEnter() { C.actk++; console.log("[A] ARMED#" + C.actk + " " + nm + ".StartDetection(args=" + pc + ") <<< 检测器布防"); } });
                }
              }
            } catch (e) {}
          }
        }
        console.log("[A] ACTk 观测就绪（classes=" + cnt + "）");
      }
    }
  } catch (e) { console.log("[A] ACTk recon 异常: " + e); }

  // ---------- 心跳 ----------
  setInterval(function () {
    console.log("[H] alive | gh=" + C.gh + " mon=" + C.mon + " mp=" + C.mp
      + " | inv=" + (S.invApplied ? "ON" : "off") + " kill=" + S.kill + "(" + S.killMode + ")"
      + " skill=" + S.skill + "(blocked=" + C.skillBlocked + ") mp=" + S.mp);
  }, 15000);

  // ---------- REPL 控制命令 ----------
  globalThis.INV_ON = function () { S.inv = true; console.log("[*] INV armed（下一次 GetMyHero 主线程生效；10s 无 [F] 日志则调 INV_DIRECT）"); };
  globalThis.INV_OFF = function () { S.inv = false; console.log("[*] INV disarmed（下一次 GetMyHero 生效）"); };
  globalThis.INV_DIRECT = function () {
    if (bmInst.isNull()) return console.log("no bmInst captured");
    const hg = new NativeFunction(A.GetMyHero, 'pointer', ['pointer'])(bmInst);
    if (hg.isNull()) return console.log("GetMyHero returned NULL");
    fnAddFlag(hg, 5); S.invApplied = true;
    console.log("[F] INV DIRECT ON hg=" + hg + "（跨线程直调，注意与主线程竞争）");
  };
  globalThis.KILL_ON = function (mode) { S.killMode = mode || 'neg'; S.kill = true; console.log("[*] KILL ON mode=" + S.killMode); };
  globalThis.KILL_OFF = function () { S.kill = false; console.log("[*] KILL OFF"); };
  globalThis.SKILL_ON = function () { S.skill = true; console.log("[*] SKILL(无CD) ON"); };
  globalThis.SKILL_OFF = function () { S.skill = false; console.log("[*] SKILL OFF"); };
  globalThis.MP_ON = function (mode) { S.mpMode = mode || 'neg'; S.mp = true; console.log("[*] MP ON mode=" + S.mpMode); };
  globalThis.MP_OFF = function () { S.mp = false; console.log("[*] MP OFF"); };
  globalThis.STAT = function () { console.log(JSON.stringify({ S: S, C: C, bmInst: String(bmInst), hgLast: String(hgLast) })); };

  console.log("[*] v5 READY — INV_ON/INV_OFF/KILL_ON(mode)/KILL_OFF/SKILL_ON/SKILL_OFF/MP_ON(mode)/MP_OFF/STAT");
}

setImmediate(main);
