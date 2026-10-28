using System;
using System.IO;
using UnityEngine;
using Bamboo.ECS;
using ACT;
using ACT.Battle;
using ACT.Battle.Configs;

// ============================================================================
//  绿梦：时空之声 —— 独立 mod 程序集（阶段 2：5 功能）
//
//  形态（硬约束 #11 直装包）：游戏侧只有 IL 补丁 + 读状态文件，**没有任何菜单 UI**；
//  菜单在包内（Java 悬浮面板），把开关/数值原子写进 starry_lvmeng_state.txt
//  （独立文件名，避开框架 StateMirror 的 starry_state.txt 8 整数通道）。
//  本程序集由 Assembly-ACT 的 Hello::Run prefix 用 Assembly.Load(File.ReadAllBytes(path)) 加载。
//
//  纪律（全程适用）：
//   · 禁止 [DllImport]（热更程序集的 P/Invoke 表在构建期固定）。
//   · 每个入口方法体 try/catch 包住，异常只记日志，**绝不外抛**。
//   · 只用游戏自己用过的 BCL/反射 API（AOT 裁剪）：文件 IO、string.IndexOf/Substring 都有先例。
//   · 状态文件格式（冻结，见 10_docs/绿梦/助手落地规格.md §2）：
//       S1 <menu> <hp_on> <hp_value> <spd_on> <spd_x100> <dmg_on> <dmg_x100> <skill_on> <jump_on>
// ============================================================================
namespace Starry
{
    public static class Mod
    {
        /// <summary>状态文件 / mod dll 的候选目录，按序尝试（app 私有目录优先，直装包形态）。</summary>
        internal static readonly string[] Dirs =
        {
            "/data/data/com.zhuziplay.lvmengnew.op/files",
            "/storage/emulated/0/Android/media/com.zhuziplay.lvmengnew.op",
            "/storage/emulated/0/Android/data/com.zhuziplay.lvmengnew.op/files",
        };

        const string TAG = "[StarryLvmeng]";

        static int _frame;
        static int _lastStateFrame;
        static int _errLogs;
        static int _jumpLogs;
        static float _appliedSpeed = 1f;

        // 状态缓存（默认全关，hp 默认 0 不生效，输入数字才生效）
        static int _hpOn, _hpVal, _spdOn, _spdX100 = 100,
                   _dmgOn, _dmgX100 = 100, _skillOn, _jumpOn;

        internal static void Ev(string msg)
        {
            try { Debug.LogError(msg); } catch { }
            try
            {
                var dir = FindDir();
                if (!string.IsNullOrEmpty(dir))
                    File.AppendAllText(dir + "/starry_log.txt", msg + "\n");
            }
            catch { }
        }

        internal static string FindDir()
        {
            for (int i = 0; i < Dirs.Length; i++)
                if (File.Exists(Dirs[i] + "/starry_lvmeng_state.txt")) return Dirs[i];
            for (int i = 0; i < Dirs.Length; i++)
                if (File.Exists(Dirs[i] + "/starry_lvmeng.dll")) return Dirs[i];
            for (int i = 0; i < Dirs.Length; i++)
                if (Directory.Exists(Dirs[i])) return Dirs[i];
            return null;
        }

        /// <summary>由 Hello::Run() 的 prefix 反射调用一次。</summary>
        public static void Init()
        {
            try
            {
                Reload();
                Ev(TAG + " stage2 init dir=" + (FindDir() ?? "<none>") + " hp=" + _hpOn + "/" + _hpVal
                   + " spd=" + _spdOn + "/" + _spdX100 + " dmg=" + _dmgOn + "/" + _dmgX100
                   + " skill=" + _skillOn + " jump=" + _jumpOn);
            }
            catch (Exception e)
            {
                Ev(TAG + " Init failed: " + e.Message);
            }
        }

        // ---------------------------------------------------------------- 状态文件
        static void Reload()
        {
            try
            {
                var dir = FindDir();
                if (dir == null) return;
                var p = dir + "/starry_lvmeng_state.txt";
                if (!File.Exists(p)) return;
                var s = File.ReadAllText(p);
                if (string.IsNullOrEmpty(s)) return;

                int i = 0;
                while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n')) i++;
                if (i < s.Length && (s[i] == 'S' || s[i] == 's'))
                {
                    i++;
                    while (i < s.Length && s[i] >= '0' && s[i] <= '9') i++;
                }

                int[] t = new int[9];
                int n = 0;
                while (i < s.Length && n < 9)
                {
                    while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\r' || s[i] == '\n')) i++;
                    if (i >= s.Length) break;
                    int v = 0; bool neg = false;
                    if (s[i] == '-') { neg = true; i++; }
                    bool any = false;
                    while (i < s.Length && s[i] >= '0' && s[i] <= '9') { v = v * 10 + (s[i] - '0'); i++; any = true; }
                    if (!any) { i++; continue; }
                    t[n++] = neg ? -v : v;
                }
                if (n < 9) return;

                _hpOn = t[1]; _hpVal = t[2];
                _spdOn = t[3]; _spdX100 = t[4];
                _dmgOn = t[5]; _dmgX100 = t[6];
                _skillOn = t[7]; _jumpOn = t[8];
            }
            catch (Exception e)
            {
                if (_errLogs < 5) { _errLogs++; Ev(TAG + " state read failed: " + e.Message); }
            }
        }

        // ---------------------------------------------------------------- ① 自定义血量
        /// <summary>注入在 CompAbility::ServerSetHP 入口：返回 >=0 则把参数改写为该值（封顶 maxHP）。</summary>
        public static int OnSetHP(CompAbility a)
        {
            try
            {
                if (a == null || _hpOn != 1) return -1;
                var me = MonoECS.instance;
                if (me == null) return -1;
                var e = a.owner;
                if (!e.IsValid || e != me.mainEntity) return -1;
                int v = _hpVal;
                int mx = a.maxHP;
                if (mx > 0 && v > mx) v = mx;
                if (v < 1) v = 1;
                return v;
            }
            catch { return -1; }
        }

        // ---------------------------------------------------------------- ③ 伤害倍率
        /// <summary>注入在 SystemAbility::ServerResolveDamage 入口（actualDamage 由 damage 派生）。</summary>
        public static void OnResolveDamage(AttackData ad)
        {
            try
            {
                if (ad == null || _dmgOn != 1) return;
                if (!ad.attacker.IsValid || !ad.attacker.IsCharacter()) return;
                float k = _dmgX100 / 100f;
                if (k <= 1f) return;
                ad.damage *= k;
            }
            catch { }
        }

        // ---------------------------------------------------------------- ⑤ 无限跳跃
        public static void OnSetJumpCount(ActionMemory memory)
        {
            try
            {
                if (_jumpOn != 1) return;
                var me = MonoECS.instance;
                if (me == null) return;
                var e = memory.target.entity;
                if (!e.IsValid || e != me.mainEntity) return;
                var m = e.GetCompMotion();
                if (m != null) m.JumpInAirCount = 99;
            }
            catch { }
        }

        public static void OnRecoverMotion(CompMotion m)
        {
            try
            {
                if (_jumpOn != 1 || m == null) return;
                var me = MonoECS.instance;
                if (me == null) return;
                if (_jumpLogs < 3)
                {
                    _jumpLogs++;
                    Ev(TAG + " recover-probe ownerEqMain=" + (m.owner.IsValid && m.owner == me.mainEntity)
                       + " jc=" + m.JumpInAirCount + " on=" + _jumpOn);
                }
                if (!m.owner.IsValid || m.owner != me.mainEntity) return;
                m.JumpInAirCount = 99;
            }
            catch { }
        }

        // ---------------------------------------------------------------- 每帧驱动
        /// <summary>由 ACT.Battle.SystemTime::FixedUpdate() 的 postfix 每固定帧驱动。</summary>
        public static void Tick()
        {
            try
            {
                _frame++;
                if (_frame - _lastStateFrame >= 30) { _lastStateFrame = _frame; Reload(); ApplySpeed(); }
                if (_jumpOn == 1 && (_frame % 10) == 0) ApplyJump();
                if (_skillOn == 1) ApplySkills();
            }
            catch (Exception e)
            {
                if (_errLogs < 5) { _errLogs++; Ev(TAG + " tick failed: " + e.Message); }
            }
        }

        // ---------------------------------------------------------------- ② 全局变速
        static void ApplySpeed()
        {
            try
            {
                var st = GameManager.timeSystem;
                if (st == null) return;
                float want = _spdOn == 1 ? (_spdX100 / 100f) : 1f;
                if (want < 0.1f) want = 0.1f;
                if (want > 10f) want = 10f;
                if (!Mathf.Approximately(want, _appliedSpeed))
                {
                    st.DebugChangeSpeed(want);
                    _appliedSpeed = want;
                }
                else if (_spdOn == 1 && !Mathf.Approximately(st.debugSpeed, want))
                {
                    st.DebugChangeSpeed(want);
                }
            }
            catch { }
        }

        // ---------------------------------------------------------------- ⑤ 无限跳跃（每帧兜底）
        static void ApplyJump()
        {
            try
            {
                var me = MonoECS.instance;
                if (me == null) return;
                var e = me.mainEntity;
                if (!e.IsValid) return;
                var m = e.GetCompMotion();
                if (m == null) return;
                if (m.JumpInAirCount != 99) m.JumpInAirCount = 99;
            }
            catch (Exception e)
            {
                if (_jumpLogs < 5) { _jumpLogs++; Ev(TAG + " jump failed: " + e.Message); }
            }
        }

        // ---------------------------------------------------------------- ④ 无限技能
        static void ApplySkills()
        {
            try
            {
                var me = MonoECS.instance;
                if (me == null) return;
                var e = me.mainEntity;
                if (!e.IsValid) return;

                var a = e.GetCompAbility();
                if (a != null)
                {
                    a.CurrentUltimateSkillValue = float.MaxValue;
                    if (a.CurNormalEnergyPercent < 0.999f) a.ModifyNormalEnergy(a.MaxNormalEnergy);
                    if (a.CurSpecialEnergyPercent < 0.999f) a.ModifySpecialEnergy(a.MaxSpecialEnergy);
                }

                var m = e.GetCompMotion();
                if (m == null || m.skills == null) return;
                for (int i = 0; i < m.skills.Count; i++)
                {
                    var s = m.skills[i];
                    if (s == null) continue;
                    s.SetSkillCd(0f, false);
                    s.SetSkillCount(s.MaxCount);
                }
            }
            catch (Exception e)
            {
                if (_errLogs < 5) { _errLogs++; Ev(TAG + " skills failed: " + e.Message); }
            }
        }
    }
}
