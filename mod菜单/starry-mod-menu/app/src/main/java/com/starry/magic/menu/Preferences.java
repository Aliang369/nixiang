package com.starry.magic.menu;

import android.content.Context;
import android.content.SharedPreferences;

import com.starry.magic.auth.DeviceFingerprint;

import org.json.JSONObject;

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
    private static final String KEY_PERSIST_FEATURES = "persist_features";
    private static final String KEY_DEVICE_ID = "auth_device_id";
    private static final String KEY_DEVICE_ID_VERSION = "auth_device_id_version";
    private static final String KEY_LEGACY_DEVICE_IDS = "auth_legacy_device_ids";
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
            // 门控/锁功能误把滑块等写成 boolean 后，读 int 会崩；清掉脏键
            sharedPreferences.edit().remove(k).apply();
            return 0;
        }
    }

    public boolean readBoolean(int key, boolean def) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getBoolean(k, def);
        } catch (ClassCastException e) {
            sharedPreferences.edit().remove(k).apply();
            return def;
        }
    }

    public String readString(int key) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getString(k, "");
        } catch (ClassCastException e) {
            sharedPreferences.edit().remove(k).apply();
            return "";
        }
    }

    public long readLong(int key) {
        String k = String.valueOf(key);
        try {
            return sharedPreferences.getLong(k, 0L);
        } catch (ClassCastException e) {
            sharedPreferences.edit().remove(k).apply();
            return 0L;
        }
    }

    public void clearAll() {
        sharedPreferences.edit().clear().apply();
    }

    // ===== 网络验证会话 =====

    /**
     * v5 优先，v4 兜底：两个版本每次都采集；命中任一稳定 ID 就放行。
     * v5 可用时保存 v5；否则用 v4。两者都不可用时不造本地身份，让上层走解绑。
     */
    public static synchronized String getOrCreateDeviceId() {
        with(null);
        String stable = DeviceFingerprint.get(appContext);
        String cached = sharedPreferences.getString(KEY_DEVICE_ID, "");
        String v4 = DeviceFingerprint.getV4();

        if (DeviceFingerprint.isValidDevId(stable)) {
            saveDeviceId(stable, DeviceFingerprint.VERSION, legacyCandidates(cached, stable));
            return stable;
        }

        if (DeviceFingerprint.isValidDevId(v4)) {
            saveDeviceId(v4, DeviceFingerprint.VERSION_LEGACY_V4, legacyCandidates(cached, v4));
            return v4;
        }

        throw new IllegalStateException("设备标识不可用：v5/v4 均缺失，需要解绑设备后重新绑定");
    }

    private static LinkedHashSet<String> legacyCandidates(String cached, String preferred) {
        LinkedHashSet<String> legacy = new LinkedHashSet<>();
        legacy.addAll(getLegacyDeviceIds());
        if (DeviceFingerprint.isValidDevId(cached)) legacy.add(cached);
        legacy.addAll(DeviceFingerprint.getLegacyIds(appContext));
        legacy.remove(preferred);
        return legacy;
    }

    private static void saveDeviceId(String id, String version, Set<String> legacy) {
        saveLegacyDeviceIds(legacy);
        sharedPreferences.edit()
                .putString(KEY_DEVICE_ID, id)
                .putString(KEY_DEVICE_ID_VERSION, version)
                .apply();
    }

    /** v5 首次登录时上报的旧 ID 候选；服务端只在卡密当前绑定命中时接受迁移。 */
    public static List<String> getLegacyDeviceIds() {
        with(null);
        Set<String> stored = sharedPreferences.getStringSet(KEY_LEGACY_DEVICE_IDS, null);
        if (stored == null || stored.isEmpty()) return new ArrayList<>();
        return new ArrayList<>(stored);
    }

    private static void saveLegacyDeviceIds(Set<String> ids) {
        LinkedHashSet<String> clean = new LinkedHashSet<>();
        if (ids != null) {
            for (String id : ids) {
                if (DeviceFingerprint.isValidDevId(id)) clean.add(id);
                if (clean.size() >= 24) break;
            }
        }
        sharedPreferences.edit().putStringSet(KEY_LEGACY_DEVICE_IDS, clean).apply();
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
        sharedPreferences.edit()
                .putString(KEY_AUTH_BOOTSTRAP, bootstrapJson == null ? "{}" : bootstrapJson)
                .apply();
    }

    public static String getAuthCardMetaJson() {
        with(null);
        String s = sharedPreferences.getString(KEY_AUTH_CARD_META, "{}");
        return s == null || s.isEmpty() ? "{}" : s;
    }

    public static void saveAuthCardMeta(String cardJson) {
        with(null);
        sharedPreferences.edit()
                .putString(KEY_AUTH_CARD_META, cardJson == null ? "{}" : cardJson)
                .apply();
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
        SharedPreferences.Editor ed = sharedPreferences.edit()
                .putBoolean(KEY_CARD_AUTH_REQUIRED, false)
                .putLong(KEY_AUTH_LAST_OK, System.currentTimeMillis())
                .putInt(KEY_AUTH_CACHE_SEC, sec)
                .putString(KEY_AUTH_BOOTSTRAP, bootstrapJson == null ? "{}" : bootstrapJson);
        if (grant != null && !grant.isEmpty()) {
            ed.putString(KEY_AUTH_GRANT, grant);
        } else {
            ed.remove(KEY_AUTH_GRANT);
        }
        ed.apply();
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
        ed.apply();
    }

    public static void saveAuthSession(String token, String cardKey, long lastOkAt, int cacheSec,
                                       String expireAt, String bootstrapJson) {
        saveAuthSession(token, cardKey, lastOkAt, cacheSec, expireAt, bootstrapJson, null);
    }

    public static void clearAuthSession() {
        with(null);
        // 保留 card_auth_required，冷启动仍知是否显示账号 Tab
        sharedPreferences.edit()
                .remove(KEY_AUTH_TOKEN)
                .remove(KEY_AUTH_CARD)
                .remove(KEY_AUTH_LAST_OK)
                .remove(KEY_AUTH_EXPIRE)
                .remove(KEY_AUTH_BOOTSTRAP)
                .remove(KEY_AUTH_GRANT)
                .remove(KEY_AUTH_CARD_META)
                .apply();
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
