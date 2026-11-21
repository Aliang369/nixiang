package com.starry.magic.auth;

import android.content.Context;
import android.util.Log;

import java.io.BufferedReader;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;

/**
 * HTTPS JSON 客户端。信任链 = Android 系统 CA（平台默认 TrustManager）+ 平台主机名校验，
 * 不再内置任何自签证书/证书固定 —— 服务器换发标准证书（含 Let's Encrypt 90 天轮换）无需改客户端。
 * 依旧 fail-closed：只接受 HTTPS，协议不符或证书校验失败一律返回错误，不降级为明文、不放宽校验。
 */
public final class AuthClient {

    private static final String TAG = "AuthClient";
    private static final int TIMEOUT_MS = 15000;

    /** 仅为兼容既有调用点保留的幂等标记；信任链不在客户端注入。 */
    private static volatile boolean initialized;

    /** 固定自有验证服务域名：除它以外的主机一律在发请求前拒绝（含重定向目标）。 */
    private static final String ALLOWED_HOST = "auth.xgmfyx.cn";

    /**
     * 发请求之前的准入判定。返回 null = 通过，否则返回给用户的拒绝原因。
     * 只做三件事：强制 HTTPS、主机必须是固定域名、端口必须是 443/缺省。
     * 证书校验仍交给平台默认 TrustManager 与默认 HostnameVerifier。
     */
    private static String rejectReason(URL url) {
        if (!"https".equalsIgnoreCase(url.getProtocol())) {
            return "仅允许 HTTPS 连接";
        }
        String host = url.getHost();
        if (host == null || !host.equalsIgnoreCase(ALLOWED_HOST)) {
            return "服务器地址不受信任";
        }
        int port = url.getPort();
        if (port != -1 && port != 443) {
            return "服务器端口不受信任";
        }
        return null;
    }

    private AuthClient() {}

    public static synchronized void init(Context context) {
        if (initialized) return;
        initialized = true;
        // 不安装自定义 TrustManager / SSLSocketFactory：https 走平台默认信任链（系统 CA）
        // 与平台主机名校验。缺 native 也能初始化 —— 联网不再依赖 so。
        Log.i(TAG, "TLS: system CA + platform hostname verification");
    }

    public static String request(String baseUrl, String method, String path, String bodyJson, String bearer) {
        HttpURLConnection conn = null;
        try {
            String root = baseUrl == null ? "" : baseUrl.trim();
            while (root.endsWith("/")) root = root.substring(0, root.length() - 1);
            if (root.isEmpty()) {
                return errorJson("未配置验证服务器地址");
            }
            String urlStr = root + (path.startsWith("/") ? path : "/" + path);
            URL url = new URL(urlStr);
            String reject = rejectReason(url);
            if (reject != null) {
                Log.e(TAG, "拒绝请求 (" + method + "): " + reject + " <- " + urlStr);
                return errorJson(reject);
            }
            conn = (HttpURLConnection) url.openConnection();
            // 发请求之前就关掉自动重定向：任何 3xx 都不跟随（不跨源、不降级明文）。
            conn.setInstanceFollowRedirects(false);
            conn.setConnectTimeout(TIMEOUT_MS);
            conn.setReadTimeout(TIMEOUT_MS);
            conn.setRequestMethod(method.toUpperCase());
            conn.setRequestProperty("Accept", "application/json");
            conn.setRequestProperty("Content-Type", "application/json; charset=utf-8");
            if (bearer != null && !bearer.isEmpty()) {
                conn.setRequestProperty("Authorization", "Bearer " + bearer);
            }

            Log.i(TAG, method.toUpperCase() + " " + urlStr);
            if (bodyJson != null && ("POST".equalsIgnoreCase(method)
                    || "PATCH".equalsIgnoreCase(method)
                    || "PUT".equalsIgnoreCase(method))) {
                conn.setDoOutput(true);
                byte[] bytes = bodyJson.getBytes(StandardCharsets.UTF_8);
                conn.setFixedLengthStreamingMode(bytes.length);
                try (OutputStream os = conn.getOutputStream()) {
                    os.write(bytes);
                }
            }

            int code = conn.getResponseCode();
            if (code >= 300 && code < 400) {
                Log.e(TAG, "3xx 已拒绝跟随: " + method + " " + urlStr + " -> " + code);
                return errorJson("服务器返回重定向，已拒绝跟随");
            }
            InputStream stream = code >= 400 ? conn.getErrorStream() : conn.getInputStream();
            String raw = readAll(stream);
            Log.i(TAG, "← " + code + " (" + (raw == null ? 0 : raw.length()) + " bytes)");
            if (raw == null || raw.isEmpty()) {
                return errorJson(httpStatusMessage(code));
            }
            return raw;
        } catch (Exception e) {
            Log.e(TAG, "request failed", e);
            return errorJson(localizeException(e));
        } finally {
            if (conn != null) conn.disconnect();
        }
    }

    private static String httpStatusMessage(int code) {
        if (code == 401) return "未登录或登录已过期";
        if (code == 403) return "没有权限或证书校验失败";
        if (code == 404) return "接口不存在";
        if (code == 429) return "请求过于频繁，请稍后再试";
        if (code >= 500) return "服务器错误（" + code + "）";
        if (code >= 400) return "请求失败（" + code + "）";
        return "网络异常（" + code + "）";
    }

    private static String localizeException(Exception e) {
        String m = e.getMessage() == null ? "" : e.getMessage();
        String lower = m.toLowerCase();
        if (lower.contains("cleartext") || lower.contains("plaintext")) return "禁止明文 HTTP，请使用 HTTPS";
        if (lower.contains("ssl") || lower.contains("cert") || lower.contains("trust")) return "证书校验失败（请检查服务器证书与域名）";
        if (lower.contains("timeout") || lower.contains("timed out")) return "连接超时，请检查网络";
        if (lower.contains("unable to resolve") || lower.contains("unknownhost")) return "无法解析服务器地址";
        if (lower.contains("connection refused")) return "连接被拒绝，服务器可能未启动";
        if (lower.contains("network is unreachable") || lower.contains("failed to connect")) return "网络不可达，请检查网络";
        if (m.isEmpty()) return "网络请求失败";
        if (m.matches(".*[\\u4e00-\\u9fff].*")) return m;
        return "网络请求失败：" + m;
    }

    private static String readAll(InputStream stream) throws Exception {
        if (stream == null) return "";
        StringBuilder sb = new StringBuilder();
        try (BufferedReader br = new BufferedReader(new InputStreamReader(stream, StandardCharsets.UTF_8))) {
            String line;
            while ((line = br.readLine()) != null) sb.append(line);
        }
        return sb.toString();
    }

    private static String errorJson(String message) {
        String safe = message == null ? "error" : message.replace("\\", "\\\\").replace("\"", "\\\"");
        return "{\"ok\":false,\"message\":\"" + safe + "\",\"data\":null}";
    }

}
