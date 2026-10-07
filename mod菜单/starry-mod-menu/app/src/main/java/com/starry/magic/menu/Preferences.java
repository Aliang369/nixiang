package com.starry.magic.menu;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;

import com.starry.magic.auth.DeviceFingerprint;

import org.json.JSONObject;

import java.io.File;
import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.LinkedHashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

/**
 * 功能/UI 偏好存储（唯一持久化层：SharedPreferences）
 *
 * 不再通过 JNI Changes() 双写 C 层；C 层状态仅由 ModBridge.nativeXxx 更新。
 *
 * Key 规则：featureNum (int) 转字符串。
 * featNum &lt; 50 为功能项（受「保存功能偏好」开关控制）；
 * featNum &gt;= 50 为 UI 设置（始终持久化）。
 */
public class Preferences {

    private static final String PREFS_NAME = "starry_magic_prefs";
    private static final String TAG = "StarryPrefs";
    private static final String KEY_PERSIST_FEATURES = "persist_features";
    private static final String KEY_DEVICE_ID = "auth_device_id";
    private static final String KEY_DEVICE_ID_VERSION = "auth_device_id_version";
    /** 设备号来源 cached|ssaid|v4|local（2026-10-04 云机存储链路裁决用） */
    private static final String KEY_DEVICE_ID_SOURCE = "auth_device_id_source";
    /** v5/v4 全不可用时的本地兜底号（只在无 SSAID/无 Build 字段的设备上出现） */
    private static final String KEY_DEVICE_ID_LOCAL = "auth_device_id_local";
    private static final String KEY_LEGACY_DEVICE_IDS = "auth_legacy_device_ids";
    /** 内存镜像：判定后立即可读，不必再落盘 */
    private static volatile String lastDeviceIdSource = "";
    private static final String KEY_AUTH_BASE = "auth_base_url";
    private static final String KEY_AUTH_GAME_ID = "auth_game_id";
    private static final String KEY_AUTH_TOKEN = "auth_token";
    private static final String KEY_AUTH_CARD = "auth_card_key";
    private static final String KEY_AUTH_LAST_OK = "auth_last_ok_at";
    private static final String KEY_AUTH_CACHE_SEC = "auth_cache_sec";
    private static final String KEY_AUTH_EXPIRE = "auth_expire_at";
    private static final String KEY_AUTH_BOOTSTRAP = "auth_bootstrap_json";
    /** 服务端 HMAC 短票（功能放行依赖此票，而非仅 last_ok_at） */
    private static final String KEY_AUTH_GRANT = "auth_grant";
    private static final String KEY_AUTH_CARD_META = "auth_card_meta_json";
    /**
     * 2026-10-04：清档前留档。云机「第二次打开未授权」的根因之一是硬拒直接清档 ——
     * 用户既看不到证据也无法恢复。这里把被清的会话整体备一份（auth_backup_*），
     * 备份键不参与任何授权判定，只用于排障与「清档之后仍能回答刚才到底发生了什么」。
     */
    private static final String KEY_AUTH_BACKUP_PREFIX = "auth_backup_";
    private static final String KEY_AUTH_BACKUP_AT = "auth_backup_at";
    private static final String KEY_AUTH_BACKUP_REASON = "auth_backup_reason";
    /** 玩法 OTA 本地已应用版本：auth_ota_ver_{game_id} */
    private static final String KEY_OTA_VER_PREFIX = "auth_ota_ver_";
    /** 上次启动时记录的 APK lastUpdateTime；覆盖安装后变化则应清 starry_ota */
    private static final String KEY_OTA_APK_LAST_UPDATE = "ota_apk_last_update_time";
    /** 后台「卡密启用」：true 需卡密；false 仅需联网 bootstrap */
    private static final String KEY_CARD_AUTH_REQUIRED = "auth_card_required";
    /** 后台游戏「启用」：false 时功能全关（设置 Tab 仍可用） */
    private static final String KEY_MENU_ENABLED = "auth_menu_enabled";
    /** 远程公告 once_device 已读 id 集合（对齐 HTML starry_ann_seen_v1） */
    private static final String KEY_ANN_SEEN = "starry_ann_seen_v1";
    /** 上次见到的功能列表指纹（只有它变了才清功能偏好，见 clearFeaturePrefsIfListChanged） */
    private static final String KEY_FEATURE_SIG = "feature_list_sig";
    /** 内置验证服务器（自签 HTTPS + 证书固定）；改服务器只改这里 */
    public static final String DEFAULT_AUTH_BASE_URL = "https://47.100.216.101:8443";
    private static final int UI_FEAT_MIN = 50;

    private static SharedPreferences sharedPreferences;
    private static Preferences instance;
    private static Context appContext;

    private Preferences(Context context) {
        // attachBaseContext 阶段 getApplicationContext() 可能仍为 null，直接用传入 Context
        Context store = context.getApplicationContext();
        if (store == null) {
            store = context;
        }
        sharedPreferences = store.getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);
    }

    public static synchronized Preferences with(Context context) {
        if (instance == null) {
            Context ctx = null;
            if (context != null) {
                ctx = context.getApplicationContext();
                if (ctx == null) {
                    ctx = context;
                }
            } else {
                ctx = appContext;
            }
            if (ctx == null) {
                throw new IllegalStateException("Preferences 未初始化：请先调用 Preferences.with(context)");
            }
            Context app = ctx.getApplicationContext();
            appContext = app != null ? app : ctx;
            instance = new Preferences(ctx);
        }
        return instance;
    }

    public static boolean isPersistFeatures() {
        with(null);
        return sharedPreferences.getBoolean(KEY_PERSIST_FEATURES, true);
    }

    public static void setPersistFeatures(boolean enabled) {
        with(null);
        sharedPreferences.edit().putBoolean(KEY_PERSIST_FEATURES, enabled).apply();
        if (!enabled) {
            clearFeaturePrefs();
        }
    }

    /** 清除全部偏好（含 UI），用于换游戏/换功能列表后丢掉旧 key */
    public static void clearAllPrefs() {
        with(null);
        sharedPreferences.edit().clear().apply();
    }

    /** 清除功能项偏好（保留 UI 设置、persist 开关与 auth_*） */
    public static void clearFeaturePrefs() {
        with(null);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        for (String key : sharedPreferences.getAll().keySet()) {
            if (KEY_PERSIST_FEATURES.equals(key)) continue;
            if (key != null && key.startsWith("auth_")) continue;
            try {
                int num = Integer.parseInt(key);
                if (num < UI_FEAT_MIN) editor.remove(key);
            } catch (NumberFormatException ignored) {
                // 非数字且非 auth_ 的历史脏 key 不在此清理，避免误删
            }
        }
        editor.apply();
    }

    /**
     * 功能列表**确实变化**时才清功能偏好。
     *
     * <p>为什么不能每次构造都清（2026-10-07 炁术行者实测事故）：{@code ModBridge}
     * 每次构造（菜单浮层每次重开都会新建）都调 {@link #clearFeaturePrefs()}，会把玩家
     * 刚拖的滑块值立刻抹掉。走 native hook 的游戏无感（值在 so 里），但 H5/JS 补丁是
     * 经 {@code StarryH5.getState()} 读这里的偏好取值的 —— 现象是「拖完滑块游戏没反应」。
     *
     * @param sig 当前功能列表指纹；空串 = 读不到（此时保留偏好，宁可不更新也不误清，
     *            因为清了之后玩家上一次的设置再也回不来）
     */
    public static void clearFeaturePrefsIfListChanged(String sig) {
        with(null);
        String s = canonSig(sig);
        if (s.length() == 0) {
            return;
        }
        String prev = canonSig(sharedPreferences.getString(KEY_FEATURE_SIG, ""));
        if (s.equals(prev)) {
            return;
        }
        clearFeaturePrefs();
        sharedPreferences.edit().putString(KEY_FEATURE_SIG, s).apply();
        Log.i(TAG, "功能列表变化 → 已清功能偏好；prev=[" + prev + "] new=[" + s + "]");
    }

    /**
     * 指纹规范化：去掉全部空白字符后再比较/落盘。
     *
     * <p>2026-10-07 炁术行者实测：某些 ROM 的偏好 XML 往返会给字符串补尾随空白
     * （实测末尾多出 4 个空格），直接 {@code equals} 会把「同一份列表」判成「变了」，
     * 于是每次启动都清一遍功能偏好 —— 玩家刚拖的滑块值永远存不住，H5/JS 补丁
     * 读到的永远是 0（现象：拖了滑块游戏没反应）。
     */
    private static String canonSig(String s) {
        if (s == null) {
            return "";
        }
        StringBuilder sb = new StringBuilder(s.length());
        for (int i = 0; i < s.length(); i++) {
            char c = s.charAt(i);
            if (c == ' ' || c == '\t' || c == '\n' || c == '\r') {
                continue;
            }
            sb.append(c);
        }
        return sb.toString();
    }

    private static boolean shouldWrite(int featureNum) {
        if (featureNum >= UI_FEAT_MIN) return true;
        return isPersistFeatures();
    }

    // ===== 写入（仅 SharedPreferences，不调 JNI）=====

    public static void changeFeatureBool(String featureName, int featureNum, boolean enabled) {
        if (!shouldWrite(featureNum)) return;
        with(null).writeBoolean(featureNum, enabled);
    }

    public static void changeFeatureInt(String featureName, int featureNum, int value) {
        if (!shouldWrite(featureNum)) return;
        with(null).writeInt(featureNum, value);
    }

    public static void changeFeatureString(String featureName, int featureNum, String text) {
        if (!shouldWrite(featureNum)) return;
        with(null).writeString(featureNum, text);
    }

    public static void changeFeatureLong(String featureName, int featureNum, long value) {
        if (!shouldWrite(featureNum)) return;
        with(null).writeLong(featureNum, value);
    }

    // ===== 读取 =====

    public static int loadPrefInt(int featureNum) {
        return with(null).readInt(featureNum);
    }

    public static boolean loadPrefBool(int featureNum, boolean def) {
        return with(null).readBoolean(featureNum, def);
    }

    public static String loadPrefString(int featureNum) {
        return with(null).readString(featureNum);
    }

    public static long loadPrefLong(int featureNum) {
        return with(null).readLong(featureNum);
    }

    public static boolean contains(int featureNum) {
        with(null);
        return sharedPreferences.contains(String.valueOf(featureNum));
    }

    /**
     * 导出全部偏好为 JSON，供 HTML 启动时恢复 UI。
     * 布尔/数字/字符串原样输出；另含 persist_features。
     */
    public static String exportJSON() {
        with(null);
        JSONObject obj = new JSONObject();
        try {
            obj.put(KEY_PERSIST_FEATURES, isPersistFeatures());
            Map<String, ?> all = sharedPreferences.getAll();
            for (Map.Entry<String, ?> e : all.entrySet()) {
                if (KEY_PERSIST_FEATURES.equals(e.getKey())) continue;
                Object v = e.getValue();
                if (v instanceof Boolean) obj.put(e.getKey(), (Boolean) v);
                else if (v instanceof Integer) obj.put(e.getKey(), (Integer) v);
                else if (v instanceof Long) obj.put(e.getKey(), (Long) v);
                else if (v instanceof String) obj.put(e.getKey(), (String) v);
                else if (v instanceof Float) obj.put(e.getKey(), ((Float) v).doubleValue());
            }
        } catch (Exception ignored) {}
        return obj.toString();
    }

    // ===== 底层读写 =====

    public void writeInt(int key, int value) {
        String k = String.valueOf(key);
        // 先 remove：避免同 key 曾是 boolean/string 导致后续 getInt ClassCast
        sharedPreferences.edit().remove(k).putInt(k, value).apply();
    }

    public void writeBoolean(int key, boolean value) {
        String k = String.valueOf(key);
        sharedPreferences.edit().remove(k).putBoolean(k, value).apply();
    }

    public void writeString(int key, String value) {
        String k = String.valueOf(key);
        sharedPreferences.edit().remove(k).putString(k, value == null ? "" : value).apply();
    }

    public void writeLong(int key, long value) {
        String k = String.valueOf(key);
        sharedPreferences.edit().remove(k).putLong(k, value).apply();
    }

    public int readInt(int key) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getInt(k, 0);
        } catch (ClassCastException e) {
            // 读取绝不删数据：类型不符只回默认值（2026-10-07 炁术行者事故：
            // 旧实现在此 remove，误读一次就把玩家刚拖的滑块值抹掉）
            return 0;
        }
    }

    public boolean readBoolean(int key, boolean def) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getBoolean(k, def);
        } catch (ClassCastException e) {
            // 同上：绝不在读取路径删键
            return def;
        }
    }

    public String readString(int key) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getString(k, "");
        } catch (ClassCastException e) {
            return "";
        }
    }

    public long readLong(int key) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getLong(k, 0L);
        } catch (ClassCastException e) {
            return 0L;
        }
    }

    public void clearAll() {
        sharedPreferences.edit().clear().apply();
    }

    // ===== 网络验证会话 =====

    /**
     * 设备标识解析（2026-10-04 云机修复 · 「存了不认」主因）。
     *
     * <p>旧实现（v5 起）每次都采集 SSAID 重算，<b>只把持久化的 cached 丢进
     * legacyCandidates()</b>，返回值永远是当次采集值 ⇒ 一旦 SSAID 漂移（云实例重建、
     * ROM 抹 ANDROID_ID、虚拟化实例不固定），本地「存了却认不出自己」：
     * 本地 grant 的设备字段与当次 device 不等（`MainModern.cpp:143` 直接判否）、
     * 心跳被判「会话与设备不匹配」⇒ 客户端当权威硬拒 ⇒ 清存档 ⇒ 菜单未授权。
     *
     * <p>现在的裁决顺序：
     * <ol>
     *   <li><b>cached 权威</b>：本地已存的合法 {@code auth_device_id} 直接作为结果返回，
     *       不再采集、不再覆盖（存了就必须用），且此路径零 I/O 零 su 调用；</li>
     *   <li>无缓存才采集：v5（SSAID）→ v4（Build 字段）；</li>
     *   <li>都不行退本地兜底号（source=local），<b>绝不抛异常</b>——旧实现在
     *       {@code :292} 抛 IllegalStateException，云机上等于整条授权链路直接崩。</li>
     * </ol>
     */
    public static synchronized String getOrCreateDeviceId() {
        with(null);
        String cached = sharedPreferences.getString(KEY_DEVICE_ID, "");

        // 1) 存了就必须认：持久化值即权威，绝不因当次重算而漂移
        if (DeviceFingerprint.isValidDevId(cached)) {
            String ver = sharedPreferences.getString(KEY_DEVICE_ID_VERSION, "");
            markDeviceIdSource("cached", ver);
            return cached;
        }

        // 2) 首次定身份：v5 优先
        String stable = DeviceFingerprint.get(appContext);
        if (DeviceFingerprint.isValidDevId(stable)) {
            saveDeviceId(stable, DeviceFingerprint.VERSION,
                    legacyCandidates(cached, stable), "ssaid");
            DeviceFingerprint.writeSeed(appContext, stable);
            return stable;
        }

        // 2b) SSAID 不可用：v4（Build 字段）兜底
        String v4 = DeviceFingerprint.getV4();
        if (DeviceFingerprint.isValidDevId(v4)) {
            saveDeviceId(v4, DeviceFingerprint.VERSION_LEGACY_V4,
                    legacyCandidates(cached, v4), "v4");
            DeviceFingerprint.writeSeed(appContext, v4);
            return v4;
        }

        // 3) 终局兜底：本地随机号（不抛异常）
        Log.w(TAG, "v5/v4 均不可用 → 使用本地兜底设备号 source=local");
        String local = localFallbackDeviceId();
        saveDeviceId(local, DeviceFingerprint.VERSION_LOCAL,
                legacyCandidates(cached, local), "local");
        return local;
    }

    /** 只在来源标记变化时落盘（cached 快路径：零 I/O）。 */
    private static void markDeviceIdSource(String source, String version) {
        lastDeviceIdSource = source;
        String cur = sharedPreferences.getString(KEY_DEVICE_ID_SOURCE, "");
        if (source.equals(cur)) return;
        SharedPreferences.Editor ed = sharedPreferences.edit()
                .putString(KEY_DEVICE_ID_SOURCE, source);
        if (version != null && !version.isEmpty()) {
            ed.putString(KEY_DEVICE_ID_VERSION, version);
        }
        commitChecked(ed, KEY_DEVICE_ID_SOURCE, source);
    }

    /** 设备号来源 cached|ssaid|v4|local；供 [auth] 存储链路裁决日志。 */
    public static String getDeviceIdSource() {
        String s = lastDeviceIdSource;
        if (s != null && !s.isEmpty()) return s;
        with(null);
        String stored = sharedPreferences.getString(KEY_DEVICE_ID_SOURCE, "");
        return stored == null ? "" : stored;
    }

    /** 本包授权存档 XML 绝对路径（排障日志用，不含敏感值）。 */
    public static String getStorePath() {
        try {
            with(null);
            String dir = appContext.getApplicationInfo().dataDir;
            return new File(dir, "shared_prefs/" + PREFS_NAME + ".xml").getAbsolutePath();
        } catch (Throwable t) {
            return "shared_prefs/" + PREFS_NAME + ".xml";
        }
    }

    /** 本地兜底设备号：同一安装内稳定（持久化），仅作 v5/v4 均缺失时的最后手段。 */
    private static String localFallbackDeviceId() {
        String stored = sharedPreferences.getString(KEY_DEVICE_ID_LOCAL, "");
        if (DeviceFingerprint.isValidDevId(stored)) return stored;
        String raw = java.util.UUID.randomUUID().toString().replace("-", "");
        String id = "dev_" + raw + raw.substring(0, 8);
        commitChecked(sharedPreferences.edit().putString(KEY_DEVICE_ID_LOCAL, id),
                KEY_DEVICE_ID_LOCAL, id);
        return id;
    }

    /** 已存 grant 里的设备字段（grant 格式 `1|device|game|exp|menu|card|kid.sig`）；无则空串。
     *  这是服务端亲自签发过的设备号 ⇒ 一定是服务端认识的身份，最适合当迁移候选。 */
    public static String grantDeviceId() {
        with(null);
        String grant = sharedPreferences.getString(KEY_AUTH_GRANT, "");
        if (grant == null || grant.isEmpty()) return "";
        int dot = grant.lastIndexOf('.');
        String body = dot > 0 ? grant.substring(0, dot) : grant;
        String[] parts = body.split("\\|");
        if (parts.length < 2) return "";
        String dev = parts[1] == null ? "" : parts[1].trim();
        return DeviceFingerprint.isValidDevId(dev) ? dev : "";
    }

    /** 已存 grant 里的设备字段是否等于给定设备号（纯解析，判否只影响日志/自愈决策）。 */
    public static boolean grantDeviceMatches(String deviceId) {
        String dev = grantDeviceId();
        return !dev.isEmpty() && deviceId != null && dev.equals(deviceId);
    }

    public static boolean hasStoredGrant() {
        with(null);
        String grant = sharedPreferences.getString(KEY_AUTH_GRANT, "");
        return grant != null && !grant.isEmpty();
    }

    private static LinkedHashSet<String> legacyCandidates(String cached, String preferred) {
        LinkedHashSet<String> legacy = new LinkedHashSet<>();
        legacy.addAll(getLegacyDeviceIds());
        if (DeviceFingerprint.isValidDevId(cached)) legacy.add(cached);
        String fromGrant = grantDeviceId();
        if (!fromGrant.isEmpty()) legacy.add(fromGrant);
        legacy.addAll(DeviceFingerprint.getLegacyIds(appContext));
        legacy.remove(preferred);
        return legacy;
    }
    private static void saveDeviceId(String id, String version, Set<String> legacy, String source) {
        with(null);
        String curId = sharedPreferences.getString(KEY_DEVICE_ID, "");
        String curVer = sharedPreferences.getString(KEY_DEVICE_ID_VERSION, "");
        String curSrc = sharedPreferences.getString(KEY_DEVICE_ID_SOURCE, "");
        Set<String> curLegacy = sharedPreferences.getStringSet(KEY_LEGACY_DEVICE_IDS, null);
        boolean sameLegacy = (curLegacy == null && (legacy == null || legacy.isEmpty()))
                || (curLegacy != null && legacy != null && curLegacy.equals(legacy));
        if (id.equals(curId) && version.equals(curVer) && source.equals(curSrc) && sameLegacy) {
            lastDeviceIdSource = source;  // 值未变：不落盘
            return;
        }
        saveLegacyDeviceIds(legacy);
        SharedPreferences.Editor ed = sharedPreferences.edit()
                .putString(KEY_DEVICE_ID, id)
                .putString(KEY_DEVICE_ID_VERSION, version)
                .putString(KEY_DEVICE_ID_SOURCE, source);
        commitChecked(ed, KEY_DEVICE_ID, id);
        lastDeviceIdSource = source;
    }

    /**
     * 授权类键必须<b>同步</b>落盘（commit）+ 写后回读校验。
     *
     * <p>旧实现在 {@code :309} device_id、（legacy）{@code :328}、token/card {@code :576}、
     * grant {@code :488}、bootstrap/card_meta {@code :420/:433} 全用 {@code apply()}：
     * 异步刷盘，云机/低端机上进程被回收时可能还没落盘 ⇒ 下一次打开「像没存过」。
     * 这里改为 commit + 回读，失败重试一次并留日志。
     */
    private static boolean commitChecked(SharedPreferences.Editor ed, String verifyKey,
                                        Object expect) {
        try {
            if (!ed.commit()) {
                Log.w(TAG, "commit failed key=" + verifyKey + " retry");
                if (!ed.commit()) {
                    Log.e(TAG, "commit FAILED after retry key=" + verifyKey);
                    return false;
                }
            }
        } catch (Throwable t) {
            Log.e(TAG, "commit threw key=" + verifyKey, t);
            return false;
        }
        try {
            Object got = sharedPreferences.getAll().get(verifyKey);
            if (expect == null) {
                // expect=null 语义：该键应当已被移除
                if (got != null) {
                    Log.e(TAG, "remove not applied key=" + verifyKey + " got=" + got);
                    return false;
                }
                return true;
            }
            if (!expect.equals(got)) {
                Log.e(TAG, "write-back mismatch key=" + verifyKey
                        + " expect=" + expect + " got=" + got);
                return false;
            }
        } catch (Throwable ignored) {
        }
        return true;
    }

    /** v5 首次登录时上报的旧 ID 候选；服务端只在卡密当前绑定命中时接受迁移。
     *  2026-10-04 补充：把「已存 grant 里的设备号」也算候选 —— 那是服务端亲自签发过的身份，
     *  云机指纹漂移时正是它躺在卡密绑定上，不带它自愈必然 403（实测：缺这条时 login 报
     *  「卡密已绑定其他设备」）。 */
    public static List<String> getLegacyDeviceIds() {
        with(null);
        LinkedHashSet<String> all = new LinkedHashSet<>();
        Set<String> stored = sharedPreferences.getStringSet(KEY_LEGACY_DEVICE_IDS, null);
        if (stored != null) all.addAll(stored);
        String fromGrant = grantDeviceId();
        if (!fromGrant.isEmpty()) all.add(fromGrant);
        return new ArrayList<>(all);
    }

    private static void saveLegacyDeviceIds(Set<String> ids) {
        LinkedHashSet<String> clean = new LinkedHashSet<>();
        if (ids != null) {
            for (String id : ids) {
                if (DeviceFingerprint.isValidDevId(id)) clean.add(id);
                if (clean.size() >= 24) break;
            }
        }
        commitChecked(sharedPreferences.edit().putStringSet(KEY_LEGACY_DEVICE_IDS, clean),
                KEY_LEGACY_DEVICE_IDS, clean);
    }

    public static String getAuthBaseUrl() {
        with(null);
        String url = sharedPreferences.getString(KEY_AUTH_BASE, "");
        if (url == null || url.isEmpty()) {
            return DEFAULT_AUTH_BASE_URL;
        }
        return url;
    }

    public static boolean isCardAuthRequired() {
        with(null);
        return sharedPreferences.getBoolean(KEY_CARD_AUTH_REQUIRED, true);
    }

    public static void setCardAuthRequired(boolean required) {
        with(null);
        sharedPreferences.edit().putBoolean(KEY_CARD_AUTH_REQUIRED, required).apply();
    }

    public static boolean isMenuEnabled() {
        with(null);
        return sharedPreferences.getBoolean(KEY_MENU_ENABLED, true);
    }

    public static void setMenuEnabled(boolean enabled) {
        with(null);
        sharedPreferences.edit().putBoolean(KEY_MENU_ENABLED, enabled).apply();
    }

    /**
     * 后台单项功能门控（bootstrap.features[].enabled）。
     * 列表为空或不含该 feat_num → 放行（与 HTML AuthState.featureEnabled 一致）。
     */
    public static boolean isServerFeatureEnabled(int featNum) {
        if (featNum >= UI_FEAT_MIN) return true;
        try {
            JSONObject boot = new JSONObject(getAuthBootstrapJson());
            org.json.JSONArray list = boot.optJSONArray("features");
            if (list == null || list.length() == 0) return true;
            for (int i = 0; i < list.length(); i++) {
                JSONObject f = list.optJSONObject(i);
                if (f == null) continue;
                if (f.optInt("feat_num", -1) != featNum) continue;
                return f.optBoolean("enabled", true);
            }
            return true;
        } catch (Exception e) {
            return true;
        }
    }

    /** 功能是否允许执行：游戏启用 + 短票存在 + 时间窗（最终以 Native 验签为准） */
    public static boolean isFeaturesAllowed() {
        if (!isMenuEnabled()) return false;
        if (!isAuthSessionValid()) return false;
        String grant = getAuthGrant();
        return grant != null && !grant.isEmpty();
    }

    public static String getAuthGrant() {
        with(null);
        return sharedPreferences.getString(KEY_AUTH_GRANT, "");
    }

    public static String getAuthExpireAt() {
        with(null);
        return sharedPreferences.getString(KEY_AUTH_EXPIRE, "");
    }

    public static String getAuthToken() {
        with(null);
        return sharedPreferences.getString(KEY_AUTH_TOKEN, "");
    }

    public static String getAuthCardKey() {
        with(null);
        return sharedPreferences.getString(KEY_AUTH_CARD, "");
    }

    public static String getAuthBootstrapJson() {
        with(null);
        String s = sharedPreferences.getString(KEY_AUTH_BOOTSTRAP, "{}");
        return s == null || s.isEmpty() ? "{}" : s;
    }

    public static void saveBootstrapJson(String bootstrapJson) {
        with(null);
        String v = bootstrapJson == null ? "{}" : bootstrapJson;
        commitChecked(sharedPreferences.edit().putString(KEY_AUTH_BOOTSTRAP, v),
                KEY_AUTH_BOOTSTRAP, v);
    }

    public static String getAuthCardMetaJson() {
        with(null);
        String s = sharedPreferences.getString(KEY_AUTH_CARD_META, "{}");
        return s == null || s.isEmpty() ? "{}" : s;
    }

    public static void saveAuthCardMeta(String cardJson) {
        with(null);
        String v = cardJson == null ? "{}" : cardJson;
        commitChecked(sharedPreferences.edit().putString(KEY_AUTH_CARD_META, v),
                KEY_AUTH_CARD_META, v);
    }

    /** 会话是否在离线缓存窗口内有效（Native / Bridge 门控用） */
    public static boolean isAuthSessionValid() {
        with(null);
        long lastOk = sharedPreferences.getLong(KEY_AUTH_LAST_OK, 0L);
        int cacheSec = sharedPreferences.getInt(KEY_AUTH_CACHE_SEC, 600);
        if (cacheSec <= 0) cacheSec = 600;
        // 防篡改：客户端缓存上限 600s
        if (cacheSec > 600) cacheSec = 600;
        long now = System.currentTimeMillis();
        boolean within = lastOk > 0 && (now - lastOk) <= (cacheSec * 1000L);
        if (!within) return false;
        // 关闭卡密：联网 bootstrap + grant；开启卡密：仍需 token
        if (!isCardAuthRequired()) {
            String grant = sharedPreferences.getString(KEY_AUTH_GRANT, "");
            return grant != null && !grant.isEmpty();
        }
        String token = sharedPreferences.getString(KEY_AUTH_TOKEN, "");
        return token != null && !token.isEmpty();
    }

    /** 禁止态强制关功能偏好（不受「保存功能偏好」开关影响） */
    public static void forceFeatureBool(int featureNum, boolean enabled) {
        if (featureNum >= UI_FEAT_MIN) return;
        with(null).writeBoolean(featureNum, enabled);
    }

    /** 强制写 int（滑块/下拉复位） */
    public static void forceFeatureInt(int featureNum, int value) {
        if (featureNum >= UI_FEAT_MIN) return;
        with(null).writeInt(featureNum, value);
    }

    public static void forceFeatureString(int featureNum, String text) {
        if (featureNum >= UI_FEAT_MIN) return;
        with(null).writeString(featureNum, text == null ? "" : text);
    }

    /** 关闭卡密模式：记录最近一次成功的联网配置拉取 */
    public static void saveNetworkBootstrap(int cacheSec, String bootstrapJson, String grant) {
        with(null);
        int sec = cacheSec > 0 ? cacheSec : 600;
        if (sec > 600) sec = 600;
        long nowMs = System.currentTimeMillis();
        SharedPreferences.Editor ed = sharedPreferences.edit()
                .putBoolean(KEY_CARD_AUTH_REQUIRED, false)
                .putLong(KEY_AUTH_LAST_OK, nowMs)
                .putInt(KEY_AUTH_CACHE_SEC, sec)
                .putString(KEY_AUTH_BOOTSTRAP, bootstrapJson == null ? "{}" : bootstrapJson);
        if (grant != null && !grant.isEmpty()) {
            ed.putString(KEY_AUTH_GRANT, grant);
        } else {
            ed.remove(KEY_AUTH_GRANT);
        }
        commitChecked(ed, KEY_AUTH_LAST_OK, nowMs);
    }

    /** @deprecated 请传 grant；保留重载避免旧 JS 直接崩 */
    public static void saveNetworkBootstrap(int cacheSec, String bootstrapJson) {
        saveNetworkBootstrap(cacheSec, bootstrapJson, null);
    }

    public static void setAuthBaseUrl(String url) {
        with(null);
        sharedPreferences.edit().putString(KEY_AUTH_BASE, url == null ? "" : url.trim()).apply();
    }

    /**
     * 运行时 game_id 只认 so 烘焙值（GameConfig.h ← Android.mk 的 STARRY_GAME_*）。
     * 此处不再提供「默认游戏名」占位，避免切游戏后仍用错 id。
     */
    public static String getAuthGameId() {
        with(null);
        String id = sharedPreferences.getString(KEY_AUTH_GAME_ID, "");
        return id == null ? "" : id;
    }

    public static void setAuthGameId(String gameId) {
        with(null);
        String v = gameId == null ? "" : gameId.trim();
        sharedPreferences.edit().putString(KEY_AUTH_GAME_ID, v).apply();
    }

    public static int getOtaLocalVer(String gameId) {
        with(null);
        String gid = gameId == null ? "" : gameId.trim();
        if (gid.isEmpty()) return 0;
        return sharedPreferences.getInt(KEY_OTA_VER_PREFIX + gid, 0);
    }

    public static void setOtaLocalVer(String gameId, int ver) {
        with(null);
        String gid = gameId == null ? "" : gameId.trim();
        if (gid.isEmpty()) return;
        sharedPreferences.edit().putInt(KEY_OTA_VER_PREFIX + gid, Math.max(0, ver)).apply();
    }

    public static long getOtaApkLastUpdateTime() {
        with(null);
        return sharedPreferences.getLong(KEY_OTA_APK_LAST_UPDATE, 0L);
    }

    public static void setOtaApkLastUpdateTime(long lastUpdateTime) {
        with(null);
        sharedPreferences.edit().putLong(KEY_OTA_APK_LAST_UPDATE, Math.max(0L, lastUpdateTime)).apply();
    }

    /** 清除所有 game_id 的本地 OTA ver 记录（不清 KEY_OTA_APK_LAST_UPDATE，由调用方决定）。 */
    public static void clearAllOtaLocalVers() {
        with(null);
        SharedPreferences.Editor ed = sharedPreferences.edit();
        for (String k : sharedPreferences.getAll().keySet()) {
            if (k != null && k.startsWith(KEY_OTA_VER_PREFIX)) {
                ed.remove(k);
            }
        }
        ed.apply();
    }

    public static void saveAuthSession(String token, String cardKey, long lastOkAt, int cacheSec,
                                       String expireAt, String bootstrapJson, String grant) {
        with(null);
        int sec = cacheSec > 0 ? cacheSec : 600;
        if (sec > 600) sec = 600;
        SharedPreferences.Editor ed = sharedPreferences.edit()
                .putBoolean(KEY_CARD_AUTH_REQUIRED, true)
                .putString(KEY_AUTH_TOKEN, token == null ? "" : token)
                // 不再长期存明文卡密（防备份泄露）；仅会话期可选
                .putString(KEY_AUTH_CARD, "")
                .putLong(KEY_AUTH_LAST_OK, lastOkAt)
                .putInt(KEY_AUTH_CACHE_SEC, sec)
                .putString(KEY_AUTH_EXPIRE, expireAt == null ? "" : expireAt)
                .putString(KEY_AUTH_BOOTSTRAP, bootstrapJson == null ? "{}" : bootstrapJson);
        if (grant != null && !grant.isEmpty()) {
            ed.putString(KEY_AUTH_GRANT, grant);
        } else {
            ed.remove(KEY_AUTH_GRANT);
        }
        // 解绑仍需要卡密：若传入则短存（可后续改内存）
        if (cardKey != null && !cardKey.isEmpty()) {
            ed.putString(KEY_AUTH_CARD, cardKey);
        }
        // 会话键同步落盘 + 回读校验：云机上一次打开「存了但读不回」正是 apply() 未刷盘
        boolean ok = commitChecked(ed, KEY_AUTH_TOKEN, token == null ? "" : token);
        if (!ok) {
            Log.e(TAG, "saveAuthSession write-back failed; device=" + getDeviceIdSource());
        }
    }

    public static void saveAuthSession(String token, String cardKey, long lastOkAt, int cacheSec,
                                       String expireAt, String bootstrapJson) {
        saveAuthSession(token, cardKey, lastOkAt, cacheSec, expireAt, bootstrapJson, null);
    }

    public static void clearAuthSession() {
        clearAuthSession("unspecified");
    }

    /**
     * 清档（带原因）：<b>先留档再清</b>。备份写 auth_backup_*，记录清档时间与触发原因，
     * 然后才逐条 remove 会话键。备份失败不阻断清档（清档本身是安全动作）。
     */
    public static void clearAuthSession(String reason) {
        with(null);
        try {
            backupAuthSession(reason);
        } catch (Throwable t) {
            Log.w(TAG, "backup before clear failed: " + t);
        }
        // 保留 card_auth_required，冷启动仍知是否显示账号 Tab
        commitChecked(sharedPreferences.edit()
                .remove(KEY_AUTH_TOKEN)
                .remove(KEY_AUTH_CARD)
                .remove(KEY_AUTH_LAST_OK)
                .remove(KEY_AUTH_EXPIRE)
                .remove(KEY_AUTH_BOOTSTRAP)
                .remove(KEY_AUTH_GRANT)
                .remove(KEY_AUTH_CARD_META), KEY_AUTH_TOKEN, null);
    }

    /** 把当前会话快照写进 auth_backup_*（空值也写空串：键在即证明「当时看过」）。 */
    public static void backupAuthSession(String reason) {
        with(null);
        long at = System.currentTimeMillis();
        SharedPreferences.Editor ed = sharedPreferences.edit()
                .putString(KEY_AUTH_BACKUP_PREFIX + "token", readStr(KEY_AUTH_TOKEN))
                .putString(KEY_AUTH_BACKUP_PREFIX + "grant", readStr(KEY_AUTH_GRANT))
                .putString(KEY_AUTH_BACKUP_PREFIX + "card", readStr(KEY_AUTH_CARD))
                .putString(KEY_AUTH_BACKUP_PREFIX + "device_id", readStr(KEY_DEVICE_ID))
                .putString(KEY_AUTH_BACKUP_PREFIX + "device_source", getDeviceIdSource())
                .putString(KEY_AUTH_BACKUP_PREFIX + "expire_at", readStr(KEY_AUTH_EXPIRE))
                .putString(KEY_AUTH_BACKUP_PREFIX + "card_meta", readStr(KEY_AUTH_CARD_META))
                .putLong(KEY_AUTH_BACKUP_PREFIX + "last_ok_at",
                        sharedPreferences.getLong(KEY_AUTH_LAST_OK, 0L))
                .putString(KEY_AUTH_BACKUP_REASON, reason == null ? "" : reason)
                .putLong(KEY_AUTH_BACKUP_AT, at);
        commitChecked(ed, KEY_AUTH_BACKUP_AT, at);
    }

    /** 上次清档时间（0 = 从未清过）；排障用，不参与判定。 */
    public static long getAuthBackupAt() {
        with(null);
        return sharedPreferences.getLong(KEY_AUTH_BACKUP_AT, 0L);
    }

    /** 上次清档原因；排障用，不参与判定。 */
    public static String getAuthBackupReason() {
        with(null);
        return readStr(KEY_AUTH_BACKUP_REASON);
    }

    private static String readStr(String key) {
        String v = sharedPreferences.getString(key, "");
        return v == null ? "" : v;
    }

    public static String exportAuthJSON() {
        with(null);
        JSONObject obj = new JSONObject();
        try {
            obj.put("base_url", getAuthBaseUrl());
            obj.put("game_id", getAuthGameId());
            obj.put("device_id", getOrCreateDeviceId());
            obj.put("token", sharedPreferences.getString(KEY_AUTH_TOKEN, ""));
            obj.put("card_key", sharedPreferences.getString(KEY_AUTH_CARD, ""));
            obj.put("last_ok_at", sharedPreferences.getLong(KEY_AUTH_LAST_OK, 0L));
            obj.put("cache_sec", sharedPreferences.getInt(KEY_AUTH_CACHE_SEC, 600));
            obj.put("expire_at", sharedPreferences.getString(KEY_AUTH_EXPIRE, ""));
            obj.put("card_auth_enabled", isCardAuthRequired());
            obj.put("menu_enabled", isMenuEnabled());
            obj.put("grant", sharedPreferences.getString(KEY_AUTH_GRANT, ""));
            String boot = sharedPreferences.getString(KEY_AUTH_BOOTSTRAP, "{}");
            obj.put("bootstrap", new JSONObject(boot == null || boot.isEmpty() ? "{}" : boot));
        } catch (Exception ignored) {}
        return obj.toString();
    }

    /** popup_mode=once_device：本机是否已读过该公告 id */
    public static boolean isAnnounceSeen(Object id) {
        String key = normalizeAnnounceId(id);
        if (key == null) return false;
        with(null);
        Set<String> set = sharedPreferences.getStringSet(KEY_ANN_SEEN, Collections.emptySet());
        return set != null && set.contains(key);
    }

    public static void markAnnounceSeen(Object id) {
        String key = normalizeAnnounceId(id);
        if (key == null) return;
        with(null);
        Set<String> old = sharedPreferences.getStringSet(KEY_ANN_SEEN, Collections.emptySet());
        Set<String> next = new HashSet<>(old != null ? old : Collections.emptySet());
        if (!next.add(key)) return;
        sharedPreferences.edit().putStringSet(KEY_ANN_SEEN, next).apply();
    }

    private static String normalizeAnnounceId(Object id) {
        if (id == null || id == JSONObject.NULL) return null;
        String key = String.valueOf(id).trim();
        if (key.isEmpty() || "null".equalsIgnoreCase(key)) return null;
        return key;
    }
}
