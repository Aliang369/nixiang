package com.starry.magic.auth;

import android.content.Context;
import android.util.Log;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileOutputStream;
import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.StandardCharsets;
import java.security.KeyStore;
import java.security.cert.Certificate;
import java.security.cert.CertificateFactory;
import java.security.cert.X509Certificate;

import javax.net.ssl.HostnameVerifier;
import javax.net.ssl.HttpsURLConnection;
import javax.net.ssl.SSLContext;
import javax.net.ssl.SSLSession;
import javax.net.ssl.TrustManagerFactory;

/**
 * HTTP(S) JSON 客户端。HTTPS 使用 so 内烘焙的 starlight 证书固定（ModBridge.nativeGetPinnedCert）。
 * 证书加载失败时 fail-closed：request() 对 HTTPS 一律拒绝，不退回系统信任。
 */
public final class AuthClient {

    private static final String TAG = "AuthClient";
    private static final int TIMEOUT_MS = 15000;
    private static final int DOWNLOAD_TIMEOUT_MS = 120000;

    private static volatile SSLContext pinnedSslContext;
    private static volatile X509Certificate pinnedCert;
    private static volatile boolean initTried;

    private AuthClient() {}

    public static synchronized void init(Context context) {
        if (pinnedSslContext != null) return;
        initTried = true;
        try {
            // 证书固定公钥唯一来源：libstarry_magic.so 编译期烘焙（CertPinned.h）
            byte[] pem = com.starry.magic.bridge.ModBridge.nativeGetPinnedCert();
            if (pem == null || pem.length == 0) {
                throw new IllegalStateException("so 内未烘焙固定证书（native 未加载或 CertPinned.h 缺失）");
            }
            CertificateFactory cf = CertificateFactory.getInstance("X.509");
            Certificate cert = cf.generateCertificate(new ByteArrayInputStream(pem));
            pinnedCert = (X509Certificate) cert;

            KeyStore ks = KeyStore.getInstance(KeyStore.getDefaultType());
            ks.load(null, null);
            ks.setCertificateEntry("starlight", cert);

            TrustManagerFactory tmf = TrustManagerFactory.getInstance(
                    TrustManagerFactory.getDefaultAlgorithm());
            tmf.init(ks);

            SSLContext ctx = SSLContext.getInstance("TLS");
            ctx.init(null, tmf.getTrustManagers(), null);
            pinnedSslContext = ctx;
            Log.i(TAG, "pinned cert loaded from native so: " + pinnedCert.getSubjectDN());
        } catch (UnsatisfiedLinkError e) {
            Log.e(TAG, "native 未加载，无法读取烘焙证书（fail-closed）", e);
            pinnedSslContext = null;
            pinnedCert = null;
        } catch (Exception e) {
            Log.e(TAG, "failed to load pinned cert", e);
            pinnedSslContext = null;
            pinnedCert = null;
        }
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
            conn = (HttpURLConnection) url.openConnection();
            conn.setConnectTimeout(TIMEOUT_MS);
            conn.setReadTimeout(TIMEOUT_MS);
            conn.setRequestMethod(method.toUpperCase());
            conn.setRequestProperty("Accept", "application/json");
            conn.setRequestProperty("Content-Type", "application/json; charset=utf-8");
            if (bearer != null && !bearer.isEmpty()) {
                conn.setRequestProperty("Authorization", "Bearer " + bearer);
            }

            if (conn instanceof HttpsURLConnection) {
                HttpsURLConnection https = (HttpsURLConnection) conn;
                if (pinnedSslContext == null) {
                    Log.e(TAG, "HTTPS aborted (no pinned cert): " + method + " " + urlStr);
                    return errorJson("HTTPS 证书未初始化，请重启应用");
                }
                https.setSSLSocketFactory(pinnedSslContext.getSocketFactory());
                https.setHostnameVerifier(new PinnedHostnameVerifier());
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
        if (lower.contains("ssl") || lower.contains("cert") || lower.contains("trust")) return "证书校验失败，请确认已信任服务器证书";
        if (lower.contains("timeout") || lower.contains("timed out")) return "连接超时，请检查网络";
        if (lower.contains("unable to resolve") || lower.contains("unknownhost")) return "无法解析服务器地址";
        if (lower.contains("connection refused")) return "连接被拒绝，服务器可能未启动";
        if (lower.contains("network is unreachable") || lower.contains("failed to connect")) return "网络不可达，请检查网络";
        if (m.isEmpty()) return "网络请求失败";
        if (m.matches(".*[\\u4e00-\\u9fff].*")) return m;
        return "网络请求失败：" + m;
    }

    private static final class PinnedHostnameVerifier implements HostnameVerifier {
        @Override
        public boolean verify(String hostname, SSLSession session) {
            try {
                if (pinnedCert == null) return false;
                Certificate[] peer = session.getPeerCertificates();
                if (peer == null || peer.length == 0) return false;
                // 证书内容一致即通过（适配 IP / 自签）
                return pinnedCert.equals(peer[0]);
            } catch (Exception e) {
                return false;
            }
        }
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

    /** 下载进度回调：downloaded=已下载字节，total=总字节（Content-Length，未知为 -1） */
    public interface DownloadProgress {
        void onProgress(long downloaded, long total);
    }

    public static String downloadToFile(String baseUrl, String path, File outFile) {
        return downloadToFile(baseUrl, path, outFile, null, null);
    }

    /**
     * 下载二进制到本地文件（证书固定同 JSON 请求）。
     * @param bearer 可选；so 下载需卡密会话 token
     * @return null=成功，否则错误文案
     */
    public static String downloadToFile(String baseUrl, String path, File outFile, String bearer) {
        return downloadToFile(baseUrl, path, outFile, bearer, null);
    }

    public static String downloadToFile(String baseUrl, String path, File outFile, String bearer,
                                        DownloadProgress progress) {
        HttpURLConnection conn = null;
        try {
            String root = baseUrl == null ? "" : baseUrl.trim();
            while (root.endsWith("/")) root = root.substring(0, root.length() - 1);
            if (root.isEmpty()) return "未配置验证服务器地址";
            String urlStr = root + (path.startsWith("/") ? path : "/" + path);
            URL url = new URL(urlStr);
            conn = (HttpURLConnection) url.openConnection();
            conn.setConnectTimeout(TIMEOUT_MS);
            conn.setReadTimeout(DOWNLOAD_TIMEOUT_MS);
            conn.setRequestMethod("GET");
            conn.setRequestProperty("Accept", "*/*");
            if (bearer != null && !bearer.isEmpty()) {
                conn.setRequestProperty("Authorization", "Bearer " + bearer);
            }

            if (conn instanceof HttpsURLConnection) {
                HttpsURLConnection https = (HttpsURLConnection) conn;
                if (pinnedSslContext == null) {
                    return "HTTPS 证书未初始化，请重启应用";
                }
                https.setSSLSocketFactory(pinnedSslContext.getSocketFactory());
                https.setHostnameVerifier(new PinnedHostnameVerifier());
            }

            Log.i(TAG, "GET(bin) " + urlStr);
            int code = conn.getResponseCode();
            if (code >= 400) {
                return httpStatusMessage(code);
            }
            File parent = outFile.getParentFile();
            if (parent != null && !parent.exists() && !parent.mkdirs()) {
                return "无法创建目录";
            }
            long contentLen = conn.getContentLengthLong();
            try (InputStream in = conn.getInputStream();
                 OutputStream os = new FileOutputStream(outFile)) {
                byte[] buf = new byte[8192];
                int n;
                long total = 0;
                while ((n = in.read(buf)) > 0) {
                    os.write(buf, 0, n);
                    total += n;
                    if (progress != null) progress.onProgress(total, contentLen);
                }
                Log.i(TAG, "← bin " + code + " (" + total + " bytes) → " + outFile.getName());
            }
            return null;
        } catch (Exception e) {
            Log.e(TAG, "download failed", e);
            return localizeException(e);
        } finally {
            if (conn != null) conn.disconnect();
        }
    }
}
