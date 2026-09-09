// 我的勇者 v7 — 功能级验证·对照实验（attach-only 设计，v5 教训：replace+orig 重入在受保护游戏必崩）
//  背景：v6 本次新增 TryEnterCd attach 后，进怪物房即被保护主动 SIGILL 自杀（tombstone_09，crash PC=TryEnterCd+8，
//        信号为 SI_USER，保护在信号处理里 kill(0,SIGILL)，崩溃前全部计数器=0）。上次会话战斗存活时未挂 TryEnterCd。
//  v7 对照 = 移除 tryEnterCd attach（RVA 保留在注释），其余同 v6：
//  ① 无敌：monSettle onEnter piggyback（状态翻转时）→ GetMyHero → AddFlag(5)/SubFlag(5)
//  ② 秒杀：MonsterCore.SettleDamage onEnter；KILL 开 → 下一次受击 GoDeath(self,0)
//  ③a 免蓝：settleMp onEnter 改 arg[1]=0（观测：路径冷，MP 真实消耗点待查）
//  SKILL/无CD：挂点已移除，本轮不测（SKILL_ON 为 no-op）
//  ④ ACTk：只读 instancesInScene + StartDetection 被动监听
//  判定：进怪物房存活 + monSeen 增长 → TryEnterCd attach 为死刑触发点；否则继续缩小挂点集
"use strict";

const RVA = {
  bmGetMyHero: 0x45F9A3C, // BattleManager.GetMyHero() -> HeroGroup（实例方法，直接 NativeFunction 调，不 hook）
  hgAddFlag:   0x45F1E64, // HeroGroup.AddFlag(flagtype)
  hgSubFlag:   0x45F1E94, // HeroGroup.subFlag
  monSettle:   0x45F8524, // MonsterCore.SettleDamage(f,f,f,f,i) — 只 attach 捕获 x0 + 无敌 piggyback
  goDeath:     0x45AD958, // BattleObjectCore.GoDeath(DeathReason)
  settleMp:    0x45EE2B4, // HeroCore.settleMp(int) — attach 改 args[1]
  // tryEnterCd: 0x4668EC8, // v7 对照：已移除 attach（v6 崩溃点疑犯）
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
let lastSelf = NULLPTR;

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
    ["class_get_fields", "il2cpp_class_get_fields", "pointer", ["pointer"]],
    ["class_from_name", "il2cpp_class_from_name", "pointer", ["pointer", "pointer", "pointer"]],
    ["field_from_name", "il2cpp_class_get_field_from_name", "pointer", ["pointer", "pointer"]],
    ["field_get_name", "il2cpp_field_get_name", "pointer", ["pointer"]],
    ["field_get_offset", "il2cpp_field_get_offset", "int", ["pointer"]],
    ["field_get_next", "il2cpp_field_get_next", "pointer", ["pointer"]],
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
      lastSelf = self;
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

  // （v7 对照：无 CD 挂点已移除）

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
      + " kill=" + S.kill + " (cd removed) mp=" + S.mp + "(zeroed=" + C.mpZeroed + ")"
      + " actk=" + C.actk);
  }, 15000);

  // ---------- REPL 控制命令 ----------
  globalThis.INV_ON = function () { S.inv = true; console.log("[*] INV armed（下一次怪物受击生效）"); };
  globalThis.INV_OFF = function () { S.inv = false; console.log("[*] INV disarmed（下一次怪物受击生效）"); };
  globalThis.KILL_ON = function () { S.kill = true; console.log("[*] KILL armed（下一次怪物受击即 GoDeath）"); };
  globalThis.KILL_OFF = function () { S.kill = false; console.log("[*] KILL disarmed"); };
  globalThis.SKILL_ON = function () { console.log("[*] SKILL(无CD) 本轮不测（v7 对照去掉 TryEnterCd attach）"); };
  globalThis.SKILL_OFF = function () { console.log("[*] SKILL OFF"); };
  globalThis.MP_ON = function () { S.mp = true; console.log("[*] MP(免蓝) ON"); };
  globalThis.MP_OFF = function () { S.mp = false; console.log("[*] MP OFF"); };
  globalThis.GOD = function () {
    const t = lastSelf;
    console.log("[G] lastSelf=" + t + " isNull=" + t.isNull());
    if (t.isNull()) return;
    try { fnGoDeath(t, 0); console.log("[G] GoDeath 调用返回 OK"); }
    catch (e) { console.log("[G] GoDeath 异常: " + e); }
  };
  globalThis.STAT = function () { console.log(JSON.stringify({ S: S, C: C, bmInst: String(bmInst), killed: killedSet.size })); };
  globalThis.CHECK = function () {
    const bm = resolveBmInst();
    console.log("[C] bmInst=" + bm);
    if (!bm.isNull()) {
      try {
        // GetMyHero 入口读 this+0x60
        const hg = bm.add(0x60).readPointer();
        console.log("[C] bm+0x60(heroRef)=" + hg + " +0x10=" + hg.add(0x10).readPointer()
          + " +0x20=" + hg.add(0x20).readPointer() + " +0x28=" + hg.add(0x28).readPointer()
          + " +0x30=" + hg.add(0x30).readU8() + " +0x84(cacheFlag)=" + hg.add(0x84).readU32());
        // BattleManager 字段名表（offset -> name）
        const c = f.class_from_name(f.img, Memory.allocUtf8String("Battle"), Memory.allocUtf8String("BattleManager"));
        if (!c.isNull()) {
          let fl = f.class_get_fields(c);
          let cnt = 0;
          while (!fl.isNull() && cnt < 60) {
            cnt++;
            const nm = f.field_get_name(fl).readUtf8String();
            const off = f.field_get_offset(fl);
            console.log("[C] BM_field +0x" + off.toString(16) + " " + nm);
            fl = f.field_get_next(fl);
          }
        }
        const hg = fnGetMyHero(bm);
        console.log("[C] GetMyHero -> hg=" + hg);
        if (!hg.isNull()) {
          console.log("[C] hg.isMyHero(0x30)=" + hg.add(0x30).readU8()
            + " hg._cacheFlag(0x84)=" + hg.add(0x84).readU32());
        }
      } catch (e) { console.log("[C] chk err: " + e); }
    }
  };

  console.log("[*] v7 READY — INV_ON/INV_OFF/KILL_ON/KILL_OFF/SKILL_ON(no-op)/MP_ON/MP_OFF/STAT");

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