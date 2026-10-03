// 敢达争锋对决 · 星空 mod 功能注入 agent（Java 桥版本）
// 目的：不改 UI、不碰 prefs，直接调 ModBridge 的静态 native，等价于用户在菜单里拨动开关。
// 打包：frida.Compiler().build(agent, project_root=<phantom-frida/src>)
// 运行：gdzfdj_javabridge_set.py
import Java from 'frida-java-bridge';

const SPEC = {
  dmg: 'Slider_伤害倍率:1:100:1',
  root: 'Toggle_超级定怪',
  spd: 'Slider_全局加速:1:10:1'
};

function str(o) {
  try {
    if (o === null) return null;
    if (typeof o === 'string') return o;
    if (o.valueOf) return String(o.valueOf());
    return String(o);
  } catch (e) { return '<' + e + '>'; }
}

rpc.exports = {
  // dmg/root/spd: 0/false = 不动该项；返回注入结果
  run(dmg, root, spd) {
    const out = { javaAvailable: false, live: null, listed: [], sets: [], errors: [] };
    Java.perform(() => {
      out.javaAvailable = true;
      let MB = null;
      try {
        MB = Java.use('com.starry.magic.bridge.ModBridge');
      } catch (e) {
        out.errors.push('Java.use(ModBridge): ' + e);
        return;
      }
      try { out.live = MB.nativeHasLiveSession(); } catch (e) { out.errors.push('hasLiveSession: ' + e); }
      try {
        const arr = MB.nativeGetFeatureList();
        const n = arr ? arr.length : 0;
        out.listLength = n;
        for (let i = 0; i < n; i++) out.listed.push(i + '=' + str(arr[i]));
      } catch (e) { out.errors.push('nativeGetFeatureList: ' + e); }

      function callVal(i, spec, v) {
        try {
          MB.nativeSetValue(i, spec, v);
          out.sets.push('nativeSetValue(' + i + ',' + spec + ',' + v + ') ok');
        } catch (e) { out.errors.push('nativeSetValue(' + i + '): ' + e); }
      }
      function callFlag(i, spec, on) {
        try {
          MB.nativeSetFeature(i, spec, on);
          out.sets.push('nativeSetFeature(' + i + ',' + spec + ',' + on + ') ok');
        } catch (e) { out.errors.push('nativeSetFeature(' + i + '): ' + e); }
      }
      // 优先用实测到的下标（1/2/3），同时从 featureList 里按前缀校正
      function idx(prefix, fallback) {
        for (let i = 0; i < out.listed.length; i++) {
          const s = out.listed[i].split('=').slice(1).join('=');
          if (s && s.indexOf(prefix) === 0) return i;
        }
        return fallback;
      }
      if (dmg && dmg > 0) callVal(idx('Slider_伤害倍率', 1), SPEC.dmg, dmg);
      if (root) callFlag(idx('Toggle_超级定怪', 2), SPEC.root, true);
      if (spd && spd > 0) callVal(idx('Slider_全局加速', 3), SPEC.spd, spd);

      try { out.after = MB.nativeGetSettingsList ? str(MB.nativeGetSettingsList()) : null; } catch (e) { }
    });
    return out;
  },
  off() {
    const out = { sets: [], errors: [] };
    Java.perform(() => {
      const MB = Java.use('com.starry.magic.bridge.ModBridge');
      const calls = [
        () => MB.nativeSetValue(1, SPEC.dmg, 1),
        () => MB.nativeSetFeature(2, SPEC.root, false),
        () => MB.nativeSetValue(3, SPEC.spd, 1)
      ];
      const labels = ['dmg=1', 'root=OFF', 'spd=1'];
      for (let i = 0; i < calls.length; i++) {
        try { calls[i](); out.sets.push(labels[i] + ' ok'); }
        catch (e) { out.errors.push(labels[i] + ': ' + e); }
      }
    });
    return out;
  }
};
