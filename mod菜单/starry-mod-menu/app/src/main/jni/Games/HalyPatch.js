(function () {
  var root = (typeof globalThis !== "undefined") ? globalThis : ((typeof window !== "undefined") ? window : this);
  var VERSION = 2;
  var S = root.__starryHalyV2;
  if (!S || S.version !== VERSION) {
    S = root.__starryHalyV2 = {
      version: VERSION,
      noAd: false,
      nuhuo: false,
      xianzu: false,
      leifa: false,
      lastSummon: 0,
      nextInstallAt: 0,
      timer: null,
      warned: {}
    };
  }

  function log() {
    try {
      if (typeof console !== "undefined" && console.log) {
        var args = Array.prototype.slice.call(arguments);
        args.unshift("[Haly2]");
        console.log.apply(console, args);
      }
    } catch (_) {}
  }

  function warnOnce(key, message, error) {
    if (S.warned[key]) return;
    S.warned[key] = 1;
    log("isolated", key, message, error || "");
  }

  function mod(name) {
    if (typeof System === "undefined" || !System.get) return null;
    var paths = [
      name,
      "chunks:///_virtual/" + name + ".ts",
      "chunks:///_virtual/" + name + ".js"
    ];
    for (var i = 0; i < paths.length; i++) {
      try {
        var current = System.get(paths[i]);
        if (current) return current;
      } catch (_) {}
    }
    return null;
  }

  function ctor(name) {
    var m = mod(name);
    if (!m) return null;
    var value = m[name];
    if (typeof value !== "function" && m.default) {
      value = m.default[name] || m.default;
    }
    return (typeof value === "function") ? value : null;
  }

  function installMethod(owner, key, factory, tag) {
    if (!owner || typeof owner[key] !== "function") return 0;
    var current = owner[key];
    if (current.__starryHalyV2 === tag) return 1;
    var wrapped = factory(current);
    if (typeof wrapped !== "function") return 0;
    wrapped.__starryHalyV2 = tag;
    owner[key] = wrapped;
    return 2;
  }

  function installFunction(owner, key, factory, tag) {
    return installMethod(owner, key, factory, tag);
  }

  function installAdHooks() {
    var changed = 0;
    var Pref = ctor("PlayerPrefService");
    if (Pref && Pref.prototype) {
      changed += installMethod(Pref.prototype, "hasAdPrivilege", function (orig) {
        return function () {
          if (S.noAd) return true;
          return orig.apply(this, arguments);
        };
      }, "pref-has-ad");
      changed += installMethod(Pref.prototype, "canSkipAd", function (orig) {
        return function () {
          if (S.noAd) return true;
          return orig.apply(this, arguments);
        };
      }, "pref-can-skip");
    }

    var Ad = ctor("AdSkipService");
    if (Ad && Ad.prototype) {
      changed += installMethod(Ad.prototype, "hasAdPrivilege", function (orig) {
        return function () {
          if (S.noAd) return true;
          return orig.apply(this, arguments);
        };
      }, "ad-has-privilege");
      changed += installMethod(Ad.prototype, "canSkipAd", function (orig) {
        return function () {
          if (S.noAd) return true;
          return orig.apply(this, arguments);
        };
      }, "ad-can-skip");
      changed += installMethod(Ad.prototype, "tryShowSkipPopup", function (orig) {
        return function () {
          if (S.noAd) return Promise.resolve(true);
          return orig.apply(this, arguments);
        };
      }, "ad-try-skip");
    }

    var Guard = ctor("SdkRewardAdGuard");
    if (Guard) {
      changed += installFunction(Guard, "showRewardAd", function (orig) {
        return function () {
          if (S.noAd) return Promise.resolve(true);
          return orig.apply(this, arguments);
        };
      }, "reward-guard-show");
      if (Guard.prototype) {
        changed += installMethod(Guard.prototype, "showRewardAd", function (orig) {
          return function () {
            if (S.noAd) return Promise.resolve(true);
            return orig.apply(this, arguments);
          };
        }, "reward-guard-proto-show");
      }
    }

    var Sdk = ctor("SdkService");
    if (Sdk && Sdk.prototype) {
      changed += installMethod(Sdk.prototype, "showRewardAd", function (orig) {
        return function () {
          if (S.noAd) return Promise.resolve({ rewarded: true });
          return orig.apply(this, arguments);
        };
      }, "sdk-show-reward");
    }
    return changed;
  }

  function invalidateSkills() {
    try {
      var SCM = ctor("SkillConfigManager");
      var inst = SCM && SCM.instance;
      if (inst && typeof inst.clearCache === "function") inst.clearCache();
    } catch (error) {
      warnOnce("skill-cache", "clear cache failed", error);
    }
  }

  function ensureSkillLoaded(id) {
    try {
      var SCM = ctor("SkillConfigManager");
      var inst = SCM && SCM.instance;
      if (!inst) return false;
      if (!inst.getSkillDataSync(id) && typeof inst.preloadSkill === "function") inst.preloadSkill(id);
      return true;
    } catch (error) {
      warnOnce("skill-preload-" + id, "preload failed", error);
      return false;
    }
  }

  function boostSkillData(data) {
    if (!data) return;
    var id = Number(data.id) || 0;
    var groupId = Number(data.skillId) || id;

    if (S.nuhuo && (groupId === 1005 || id === 100501 || id === 100502 || id === 100503)) {
      data.cooldown = 0;
      data.buffData = Array.isArray(data.buffData) ? data.buffData : [];
      var hasInvincible = false;
      for (var i = 0; i < data.buffData.length; i++) {
        var buff = data.buffData[i];
        if (buff && Number(buff.id) === 1002) {
          buff.duration = 100000;
          hasInvincible = true;
        }
      }
      if (!hasInvincible) data.buffData.push({ id: 1002, chance: 10000, duration: 100000, target: "self" });
    }

    if (S.xianzu && groupId === 1002) {
      data.effectData = Array.isArray(data.effectData) ? data.effectData : [];
      var hasSummon = false;
      for (var j = 0; j < data.effectData.length; j++) {
        var effect = data.effectData[j];
        if (!effect || String(effect.type || "") !== "summon") continue;
        effect.summon_count = 3;
        effect.duration = 20000;
        effect.inherit_attack = 100000000;
        effect.share_buff = true;
        hasSummon = true;
      }
      if (!hasSummon) {
        data.effectData.push({ type: "summon", summon_count: 3, duration: 20000, inherit_attack: 100000000, share_buff: true });
      }
    }

    if (S.leifa && groupId === 1009) {
      data.damageMultiplier = [9.9];
      data.effectData = Array.isArray(data.effectData) ? data.effectData : [];
      for (var k = 0; k < data.effectData.length; k++) {
        var damage = data.effectData[k];
        if (!damage || String(damage.type || "") !== "damage") continue;
        damage.damage_percent = 99000;
        damage.count = Math.max(Number(damage.count) || 0, 11);
      }
    }
  }

  function installSkillHooks() {
    var SkillManager = ctor("SkillConfigManager");
    if (!SkillManager || !SkillManager.prototype) return 0;
    return installMethod(SkillManager.prototype, "parseSkillData", function (orig) {
      return function (cfg) {
        var data = orig.call(this, cfg);
        try {
          boostSkillData(data);
        } catch (error) {
          warnOnce("skill-boost", "skill boost failed", error);
        }
        return data;
      };
    }, "skill-parse-v2");
  }

  function installSummonHook() {
    var Summon = ctor("SkillSummonSystem");
    if (!Summon || !Summon.prototype) return 0;
    return installMethod(Summon.prototype, "handleSummonRequest", function (orig) {
      return async function (request) {
        try {
          if (request && request.__starryHalyV2 === true) {
            var forwarded = Object.assign({}, request, {
              skillId: 0,
              __starryHalyV2: false
            });
            return await orig.call(this, forwarded);
          }
          return await orig.apply(this, arguments);
        } catch (error) {
          warnOnce("summon-handler", "summon request isolated", error);
          return false;
        }
      };
    }, "summon-handler-v2");
  }

  function installCombatHook() {
    var Passive = ctor("PassiveSkillTriggerSystem");
    if (!Passive || !Passive.prototype) return 0;
    return installMethod(Passive.prototype, "triggerAttackerPassiveSkills", function (orig) {
      return function (data) {
        var result;
        try {
          result = orig.apply(this, arguments);
        } catch (error) {
          warnOnce("passive-orig", "original passive pass isolated", error);
        }
        if (!S.leifa) return result;
        try {
          triggerThunder(this, data);
        } catch (error) {
          warnOnce("thunder-trigger", "thunder trigger isolated", error);
        }
        return result;
      };
    }, "passive-thunder-v2");
  }

  function installCastSafety() {
    var Cast = ctor("SkillCastSystem");
    if (!Cast || !Cast.prototype) return 0;
    return installMethod(Cast.prototype, "executeSkillEffects", function (orig) {
      return function (caster, skill) {
        var id = Number(skill && skill.id) || 0;
        var groupId = Number(skill && skill.skillId) || id;
        var guarded = (id === 100903 || groupId === 1009 || groupId === 1002);
        if (!guarded) return orig.apply(this, arguments);
        try {
          return orig.apply(this, arguments);
        } catch (error) {
          warnOnce("cast-effect-" + id, "skill effect isolated", error);
          return false;
        }
      };
    }, "cast-effect-v2");
  }

  function getPlayer(scene) {
    return (scene && scene.playerEntity) ? scene.playerEntity : null;
  }

  function isAlive(entity) {
    try {
      if (!entity || typeof entity.getComponent !== "function") return false;
      var Health = ctor("HealthComponent");
      var hp = Health ? entity.getComponent(Health) : null;
      return !hp || (typeof hp.isAlive === "function" ? hp.isAlive() : true);
    } catch (_) {
      return false;
    }
  }

  function removeInvincible(player) {
    try {
      var Status = ctor("StatusEffectComponent");
      var Types = mod("StatusEffectComponent");
      if (!Status || !Types) return false;
      var status = player.getComponent(Status);
      if (!status || !Array.isArray(status.effects)) return false;
      status.effects = status.effects.filter(function (effect) {
        return !(effect && Number(effect.id) === 1002 && effect.type === Types.StatusEffectType.INVINCIBLE);
      });
      return true;
    } catch (error) {
      warnOnce("invincible-remove", "invincible removal isolated", error);
      return false;
    }
  }

  function applyInvincible(player) {
    try {
      var Status = ctor("StatusEffectComponent");
      var Types = mod("StatusEffectComponent");
      if (!Status || !Types || typeof player.getOrCreateComponent !== "function") return false;
      var status = player.getOrCreateComponent(Status);
      if (!status || !Array.isArray(status.effects) || typeof status.addEffect !== "function") return false;
      for (var i = 0; i < status.effects.length; i++) {
        var effect = status.effects[i];
        if (effect && Number(effect.id) === 1002 && effect.type === Types.StatusEffectType.INVINCIBLE && effect.timeLeft > 8) return true;
      }
      removeInvincible(player);
      status = player.getOrCreateComponent(Status);
      status.addEffect(1002, Types.StatusEffectType.INVINCIBLE, 100, 0, player.id);
      return true;
    } catch (error) {
      warnOnce("invincible-apply", "invincible apply isolated", error);
      return false;
    }
  }

  function applySummon(scene, player) {
    try {
      var now = Date.now();
      if (S.lastSummon && now - S.lastSummon < 12000) return false;
      if (!scene || !scene.eventSystem || typeof scene.eventSystem.emitSync !== "function") return false;
      S.lastSummon = now;
      ensureSkillLoaded(100203);
      scene.eventSystem.emitSync("skill_summon_request", {
        casterId: player.id,
        skillId: 100203,
        effect: {
          type: "summon",
          summon_count: 3,
          duration: 20000,
          inherit_attack: 100000000,
          share_buff: true
        },
        __starryHalyV2: true
      });
      return true;
    } catch (error) {
      warnOnce("summon-request", "summon request isolated", error);
      return false;
    }
  }

  function triggerThunder(system, data) {
    if (!system || !system.scene || !data) return false;
    var scene = system.scene;
    var player = getPlayer(scene);
    if (!player || !isAlive(player)) return false;
    var attacker = null;
    try {
      attacker = system.findAliveEntity(Number(data.sourceId) || 0);
    } catch (_) {}
    if (!attacker || attacker.id !== player.id) return false;

    try {
      var SkillBook = ctor("SkillBookComponent");
      var skillBook = SkillBook ? player.getComponent(SkillBook) : null;
      var basicSkillId = Number(skillBook && skillBook.basicSkillId) || 0;
      if (basicSkillId > 0 && Number(data.skillId) !== basicSkillId) return false;
    } catch (_) {}

    var targetId = Number(data.targetId) || 0;
    if (targetId <= 0) return false;
    ensureSkillLoaded(100903);
    scene.eventSystem.emitSync("skill_passive_trigger", {
      casterId: player.id,
      targetId: targetId,
      skillId: 100903
    });
    return true;
  }

  function applyNow() {
    try {
      var ECS = ctor("ECSManager");
      var scene = ECS && ECS.instance && ECS.instance.scene;
      var player = getPlayer(scene);
      if (!player || !isAlive(player)) {
        if (!S.nuhuo && !S.xianzu) S.lastSummon = 0;
        return false;
      }
      if (S.nuhuo) applyInvincible(player);
      else removeInvincible(player);
      if (S.xianzu) applySummon(scene, player);
      else S.lastSummon = 0;
      return true;
    } catch (error) {
      warnOnce("apply-now", "battle tick isolated", error);
      return false;
    }
  }

  function install() {
    var ad = installAdHooks();
    var skill = installSkillHooks();
    var summon = installSummonHook();
    var passive = installCombatHook();
    var cast = installCastSafety();
    if ((skill & 2) !== 0) invalidateSkills();
    return (ad + skill + summon + passive + cast) > 0;
  }

  function tick() {
    try {
      var now = Date.now();
      if (!S.nextInstallAt || now >= S.nextInstallAt) {
        S.nextInstallAt = now + 3000;
        install();
      }
      if (S.nuhuo || S.xianzu) applyNow();
    } catch (error) {
      warnOnce("timer", "timer body isolated", error);
    }
  }

  root.__starryHalyV2Install = install;
  root.__starryHalyV2ApplyNow = applyNow;
  root.__starryHalyV2SetFlags = function (noAd, nuhuo, xianzu, leifa) {
    S.noAd = !!noAd;
    S.nuhuo = !!nuhuo;
    S.xianzu = !!xianzu;
    S.leifa = !!leifa;
    if (!S.xianzu) S.lastSummon = 0;
    invalidateSkills();
    install();
    applyNow();
    log("flags", S.noAd ? 1 : 0, S.nuhuo ? 1 : 0, S.xianzu ? 1 : 0, S.leifa ? 1 : 0);
  };
  root.__starryHalyV2Clear = function () {
    S.noAd = false;
    S.nuhuo = false;
    S.xianzu = false;
    S.leifa = false;
    S.lastSummon = 0;
    invalidateSkills();
    try {
      var ECS = ctor("ECSManager");
      var scene = ECS && ECS.instance && ECS.instance.scene;
      var player = getPlayer(scene);
      if (player) removeInvincible(player);
    } catch (_) {}
    log("cleared");
  };

  if (!S.timer && typeof setInterval === "function") {
    S.timer = setInterval(tick, 500);
  }
  install();
  applyNow();
  log("installed", "ad=" + (S.noAd ? 1 : 0), "nuhuo=" + (S.nuhuo ? 1 : 0), "xianzu=" + (S.xianzu ? 1 : 0), "leifa=" + (S.leifa ? 1 : 0));
})();
