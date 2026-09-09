// 我的勇者 v6 — 功能级验证（attach-only 设计，v5 教训：replace+orig 重入在受保护游戏必崩）
//  §9.3 重启配置：
//  ① 无敌：monSettle onEnter 主线程 piggyback（状态变化时 apply）→ GetMyHero(bmInst) → HeroGroup.AddFlag(5)/subFlag(5)（GM 官方链等价）
//  ② 秒杀：MonsterCore.SettleDamage onEnter 捕获怪物核心；KILL 开 → 下一次受击时直接 GoDeath（绕 float 参数不可读/改的符号口径问题）
//  ③a 免蓝：HeroCore.settleMp onEnter 改 arg[1]=0（观测：路径冷，MP 真实消耗点待查）
//  ③b 无CD：SKillBase.TryEnterCd onLeave 直写 CDTime(0x38)=0 / cding(0x3C)=0（基类 FixedUpdate 冷→派生重载，已作废改挂 CD 入口）
//  ④ ACTk：只读 instancesInScene + StartDetection 被动监听
//  无任何 Interceptor.replace / NativeCallback / 被替换地址重入
"use strict";

const RVA = {
  bmGetMyHero: 0x45F9A3C, // BattleManager.GetMyHero() -> HeroGroup（实例方法，直接 NativeFunction 调，不 hook）
  hgAddFlag:   0x45F1E64, // HeroGroup.AddFlag(flagtype)
  hgSubFlag:   0x45F1E94, // HeroGroup.subFlag
  monSettle:   0x45F8524, // MonsterCore.SettleDamage(f,f,f,f,i) — 只 attach 捕获 x0 + 无敌 piggyback
  goDeath:     0x45AD958, // BattleObjectCore.GoDeath(DeathReason)
  settleMp:    0x45EE2B4, // HeroCore.settleMp(int) — attach 改 args[1]
  tryEnterCd:  0x4668EC8, // SKillBase.TryEnterCd() — onLeave 清 CD（基类 FixedUpdate 冷，派生类重载）
};

const S = {
  inv: false, invApplied: false,
  kill: false,
  skill: false,
  mp: false,
};
const C = { monSeen: 0, killed: 0, mpSeen: 0, mpZeroed: 0, cdWrites: 0, actk: 0, invErr: 0 };
const NULLPTR = ptr(0);
let bmInst = NULLPTR;
const killedSet = new Set();
let obsMp = 0;
let A = {}, f = {};

function prologue(addr) {
  try {
    const b = new Uint8Array(addr.readByteArray(8));
    if (b.every(x => x === 0)) return "ALLZERO!";
    return Array.from(b).map(x => ('0' + x.toString(16)).slice(-2)).join(' ');
  } catch (e) { return "UNMAPPED!"; }
}

function resolveBmInst() {
  if (!bmInst.isNull()) return bmInst;
  try {
    const c = f.class_from_name(f.img, Memory.allocUtf8String("Battle"), Memory.allocUtf8String("BattleManager"));
    if (c.isNull()) return NULLPTR;
    const fld = f.field_from_name(c, Memory.allocUtf8String("_instance"));
    if (fld.isNull()) return NULLPTR;
    const buf = Memory.alloc(8);
    f.static_get_value(fld, buf);
    bmInst = buf.readPointer();
  } catch (e) {}
  return bmInst;
}

function main() {
  const mod = Process.getModuleByName("libil2cpp.so");
  for (const [k, rva] of Object.entries(RVA)) {
    A[k] = mod.base.add(rva);
    console.log("[P] " + k + " @" + A[k] + "  " + prologue(A[k]));
  }
  globalThis.__A = A; globalThis.__S = S; globalThis.__C = C;

  const ex = (n) => { try { return mod.findExportByName(n); } catch (e) { return null; } };
  for (const [k, n, ret, args] of [
    ["domain_get", "il2cpp_domain_get", "pointer", []],
    ["domain_get_assemblies", "il2cpp_domain_get_assemblies", "pointer", ["pointer", "pointer"]],
    ["assembly_get_image", "il2cpp_assembly_get_image", "pointer", ["pointer"]],
    ["image_get_name", "il2cpp_image_get_name", "pointer", ["pointer"]],
    ["image_get_class_count", "il2cpp_image_get_class_count", "int", ["pointer"]],
    ["image_get_class", "il2cpp_image_get_class", "pointer", ["pointer", "int"]],
    ["class_get_name", "il2cpp_class_get_name", "pointer", ["pointer"]],
    ["class_from_name", "il2cpp_class_from_name", "pointer", ["pointer", "pointer", "pointer"]],
    ["field_from_name", "il2cpp_class_get_field_from_name", "pointer", ["pointer", "pointer"]],
    ["method_from_name", "il2cpp_class_get_method_from_name", "pointer", ["pointer", "pointer", "int"]],
    ["static_get_value", "il2cpp_field_static_get_value", "void", ["pointer", "pointer"]],
  ]) { const p = ex(n); if (p) f[k] = new NativeFunction(p, ret, args); }

  // Assembly-CSharp image（供 resolveBmInst 用）
  try {
    const szp = Memory.alloc(8);
    const arr = f.domain_get_assemblies(f.domain_get(), szp);
    const n = szp.readULong();
    for (let i = 0; i < n; i++) {
      const a = arr.add(i * Process.pointerSize).readPointer();
      const im = f.assembly_get_image(a);
      if (f.image_get_name(im).readUtf8String() === "Assembly-CSharp.dll") { f.img = im; break; }
    }
  } catch (e) {}
  console.log("[P] Assembly-CSharp img=" + (f.img || "NULL"));

  const fnGetMyHero = new NativeFunction(A.bmGetMyHero, 'pointer', ['pointer']);
  const fnAddFlag = new NativeFunction(A.hgAddFlag, 'void', ['pointer', 'int']);
  const fnSubFlag = new NativeFunction(A.hgSubFlag, 'void', ['pointer', 'int']);
  const fnGoDeath = new NativeFunction(A.goDeath, 'void', ['pointer', 'int']);

  // ---------- ② 秒杀 + ① 无敌 piggyback（monSettle 主线程，状态变化时 apply） ----------
  Interceptor.attach(A.monSettle, {
    onEnter(args) {
      const self = args[0];
      C.monSeen++;
      // ② 秒杀
      if (S.kill && !killedSet.has(self)) {
        killedSet.add(self);
        try { fnGoDeath(self, 0); C.killed++; console.log("[F] KILL -> GoDeath(self=" + self + ")"); }
        catch (e) { console.log("[!] GoDeath 异常: " + e); }
      }
      // ① 无敌：镜像 GM OnInvincible 链（GetMyHero -> AddFlag/SubFlag(5)），仅状态翻转时执行
      if (S.inv !== S.invApplied) {
        try {
          const bm = resolveBmInst();
          if (!bm.isNull()) {
            const hg = fnGetMyHero(bm);
            if (!hg.isNull()) {
              if (S.inv) { fnAddFlag(hg, 5); S.invApplied = true; console.log("[F] INV ON  -> AddFlag(5) hg=" + hg); }
              else { fnSubFlag(hg, 5); S.invApplied = false; console.log("[F] INV OFF -> subFlag(5) hg=" + hg); }
            }
          }
        } catch (e) { C.invErr++; if (C.invErr < 3) console.log("[!] INV piggyback 异常: " + e); }
      }
    }
  });

  // ---------- ③a 免蓝：settleMp 改寄存器 ----------
  Interceptor.attach(A.settleMp, {
    onEnter(args) {
      C.mpSeen++;
      if (obsMp < 20) { obsMp++; console.log("[O] settleMp#" + obsMp + " v=" + args[1].toInt32()); }
      if (S.mp && args[1].toInt32() !== 0) { args[1] = ptr(0); C.mpZeroed++; }
    }
  });

  // ---------- ③b 无 CD：TryEnterCd onLeave 清 CD（基类 FixedUpdate 冷→派生类重载，改挂 CD 进入入口） ----------
  Interceptor.attach(A.tryEnterCd, {
    onEnter(args) { this.self = args[0]; },
    onLeave() {
      const self = this.self;
      if (!self || self.isNull() || !S.skill) return;
      try {
        self.add(0x38).writeFloat(0);   // CDTime
        self.add(0x3C).writeU8(0);      // cding
        C.cdWrites++;
      } catch (e) {}
    }
  });

  // ---------- ④ ACTk 观测（只读） ----------
  try {
    const szp = Memory.alloc(8);
    const arr = f.domain_get_assemblies(f.domain_get(), szp);
    const n = szp.readULong();
    let img = null;
    for (let i = 0; i < n; i++) {
      const a = arr.add(i * Process.pointerSize).readPointer();
      const im = f.assembly_get_image(a);
      const nm = f.image_get_name(im).readUtf8String();
      if (nm.indexOf("CodeStage") >= 0 || nm.indexOf("AntiCheat") >= 0) { img = im; break; }
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
          if (fld && !fld.isNull()) {
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
  } catch (e) { console.log("[A] ACTk recon 异常: " + e); }

  // ---------- 心跳 ----------
  setInterval(function () {
    console.log("[H] alive | monSeen=" + C.monSeen + " killed=" + C.killed
      + " | inv=" + (S.invApplied ? "ON" : (S.inv ? "arming" : "off"))
      + " kill=" + S.kill + " skill=" + S.skill + "(cdW=" + C.cdWrites + ") mp=" + S.mp + "(zeroed=" + C.mpZeroed + ")"
      + " actk=" + C.actk);
  }, 15000);

  // ---------- REPL 控制命令 ----------
  globalThis.INV_ON = function () { S.inv = true; console.log("[*] INV armed（下一帧 FixedUpdate 生效）"); };
  globalThis.INV_OFF = function () { S.inv = false; console.log("[*] INV disarmed（下一帧生效）"); };
  globalThis.KILL_ON = function () { S.kill = true; console.log("[*] KILL armed（下一次怪物受击即 GoDeath）"); };
  globalThis.KILL_OFF = function () { S.kill = false; console.log("[*] KILL disarmed"); };
  globalThis.SKILL_ON = function () { S.skill = true; console.log("[*] SKILL(无CD) ON"); };
  globalThis.SKILL_OFF = function () { S.skill = false; console.log("[*] SKILL OFF"); };
  globalThis.MP_ON = function () { S.mp = true; console.log("[*] MP(免蓝) ON"); };
  globalThis.MP_OFF = function () { S.mp = false; console.log("[*] MP OFF"); };
  globalThis.STAT = function () { console.log(JSON.stringify({ S: S, C: C, bmInst: String(bmInst), killed: killedSet.size })); };

  console.log("[*] v6 READY — INV_ON/INV_OFF/KILL_ON/KILL_OFF/SKILL_ON/SKILL_OFF/MP_ON/MP_OFF/STAT");

  // python 驱动 RPC 桥
  rpc.exports = {
    cmd: function (name) {
      const fn = globalThis[name];
      if (typeof fn === 'function') { fn(); return "ok"; }
      return "no fn: " + name;
    },
    stat: function () { return JSON.stringify({ S: S, C: C, bmInst: String(bmInst), killed: killedSet.size }); }
  };
}

setImmediate(main);
