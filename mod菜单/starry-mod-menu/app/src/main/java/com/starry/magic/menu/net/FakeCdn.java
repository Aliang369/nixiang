package com.starry.magic.menu.net;

import android.content.Context;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.util.LinkedHashMap;
import java.util.Map;

/**
 * 伪造热更 CDN 的常量表与覆盖文件表。
 *
 * <p>背景（实机取证，2026-10-10）：目标游戏 cn.alltune.endless 2.6.53 每次冷启动都会
 * 明文 HTTP 拉取
 * <pre>
 *   GET /bundle3/2.6.53.13/Android/versions.json   Host: install2.hotune.com
 * </pre>
 * 之后按清单里 data[] 的 file/hash 与**本地文件**比对，缺什么就用扁平基址
 * {@code http://install2.hotune.com/bundle3/} 去下什么。因此只要在手机本地
 * 冒充 install2.hotune.com，就能让游戏自己把补丁 bundle 写进它自己的 Android/data。
 *
 * <p>哈希规则（实测）：
 * <ul>
 *   <li>JSON 清单：文件名尾部 32-hex == md5(内容)</li>
 *   <li>.bundle：文件名尾部 32-hex == 包内 41 字节 xasset 前缀 [1:33] 自带的构建哈希</li>
 * </ul>
 * 三件套由 {@code 04_scripts/邂逅在迷宫/xhzm_forge_cdn.py} 生成，自身 hash 自洽。
 */
final class FakeCdn {

    static final String TAG = "StarryCdn";

    /** 被冒充的 CDN 主机名（DNS 劫持目标）。 */
    static final String CDN_HOST = "install2.hotune.com";

    /** 兜底：即使 DNS 劫持没生效，命中这些网段的 :80 也照样由我们应答。 */
    static final byte[] CDN_CIDR = {(byte) 118, (byte) 183, (byte) 236, 0};

    /** 隧道内部地址（VPN 自己）。 */
    static final byte[] VPN_ADDR = {(byte) 10, (byte) 9, 0, 1};
    /** 假 DNS 服务器地址（必须落在隧道路由 10.9.0.0/24 内）。 */
    static final byte[] VPN_DNS = {(byte) 10, (byte) 9, 0, 2};
    /** 冒充 CDN 的地址（同样落在隧道路由内）。 */
    static final byte[] FAKE_CDN_IP = {(byte) 10, (byte) 9, 9, 9};

    static final int VPN_PREFIX = 24;
    static final String VPN_ADDR_STR = "10.9.0.1";
    static final String VPN_DNS_STR = "10.9.0.2";
    static final String ROUTE_STR = "10.9.0.0";
    static final String CDN_ROUTE_STR = "118.183.236.0";
    static final int CDN_ROUTE_PREFIX = 24;

    static final int CDN_PORT = 80;
    static final int DNS_PORT = 53;

    /** 冒充 CDN 的本机监听端口（用户态 TCP，不是 socket bind）。 */
    static final int MSS = 1400;

    /**
     * 覆盖表：请求 URL 的**文件名** → APK assets 路径。
     * 按文件名匹配，所以 /bundle3/Android/x 与 /bundle3/2.6.53.13/Android/x 都能命中。
     */
    private static final String[][] OVERRIDES = {
            {"versions.json",
                    "xhzm/versions.json"},
            {"buildpatch_v63_ba2deed1a86fd27eb34d6d517cc38962.json",
                    "xhzm/buildpatch_v63_ba2deed1a86fd27eb34d6d517cc38962.json"},
            {"endlessstrikedll_9639893c55f4e0f1495197aaddb1581b.bundle",
                    "xhzm/endlessstrikedll_9639893c55f4e0f1495197aaddb1581b.bundle"},
    };

    private static final Map<String, byte[]> CACHE = new LinkedHashMap<>();

    /** 统计：命中覆盖表（SERVED）/ 回源真实 CDN（PASSED）。给界面显示用。 */
    static final java.util.concurrent.atomic.AtomicInteger SERVED =
            new java.util.concurrent.atomic.AtomicInteger();
    static final java.util.concurrent.atomic.AtomicInteger MISSED =
            new java.util.concurrent.atomic.AtomicInteger();
    static final java.util.concurrent.atomic.AtomicInteger PASSED =
            new java.util.concurrent.atomic.AtomicInteger();

    private FakeCdn() {
    }

    /** 这个文件名是不是「补丁 bundle」本体（决定注入是否算完成）。 */
    static boolean isPatchBundle(String basename) {
        for (String[] kv : OVERRIDES) {
            if (kv[0].endsWith(".bundle") && kv[0].equals(basename)) {
                return true;
            }
        }
        return false;
    }

    private static volatile java.net.InetAddress realCdn;

    /**
     * 真实 CDN 的地址：覆盖表以外的请求要**回源透传**，不能 404
     * （v2.5 实测 404 会让游戏 loading 卡在 16%）。
     *
     * <p>助手 App 自己用 {@code addDisallowedApplication} 排除在隧道之外，
     * 所以这里的解析走的是系统真实 DNS，不会自环。
     */
    static java.net.InetAddress realCdnAddress() throws java.net.UnknownHostException {
        java.net.InetAddress hit = realCdn;
        if (hit != null) {
            return hit;
        }
        java.net.InetAddress a = null;
        try {
            a = java.net.InetAddress.getByName(CDN_HOST);
        } catch (Throwable t) {
            Log.w(TAG, "DNS 解析 " + CDN_HOST + " 失败，回落固定 IP: " + t);
        }
        if (a == null) {
            byte[] ip = new byte[4];
            System.arraycopy(CDN_CIDR, 0, ip, 0, 4);
            ip[3] = (byte) 59;   // 实测 install2.hotune.com = 118.183.236.59
            a = java.net.InetAddress.getByAddress(CDN_HOST, ip);
        }
        realCdn = a;
        Log.i(TAG, "真实 CDN = " + a.getHostAddress());
        return a;
    }

    static boolean isOverride(String basename) {
        for (String[] kv : OVERRIDES) {
            if (kv[0].equals(basename)) {
                return true;
            }
        }
        return false;
    }

    /** 取覆盖文件内容；未命中返回 null。带缓存（bundle 约 9.8 MB，只读一次）。 */
    static synchronized byte[] content(Context ctx, String basename) {
        String asset = null;
        for (String[] kv : OVERRIDES) {
            if (kv[0].equals(basename)) {
                asset = kv[1];
                break;
            }
        }
        if (asset == null) {
            return null;
        }
        byte[] hit = CACHE.get(asset);
        if (hit != null) {
            return hit;
        }
        try (InputStream in = ctx.getAssets().open(asset)) {
            ByteArrayOutputStream bos = new ByteArrayOutputStream(1 << 20);
            byte[] buf = new byte[1 << 16];
            int n;
            while ((n = in.read(buf)) > 0) {
                bos.write(buf, 0, n);
            }
            byte[] data = bos.toByteArray();
            CACHE.put(asset, data);
            Log.i(TAG, "asset loaded " + asset + " size=" + data.length);
            return data;
        } catch (Throwable t) {
            Log.e(TAG, "asset load failed: " + asset, t);
            return null;
        }
    }

    static String contentTypeOf(String basename) {
        return basename.endsWith(".json")
                ? "application/json; charset=utf-8"
                : "application/octet-stream";
    }

    /** 这个目的地址是否该由我们冒充 CDN 应答。 */
    static boolean isCdnDest(byte[] pkt, int ipOff) {
        if (u8Eq(pkt, ipOff + 16, FAKE_CDN_IP)) {
            return true;
        }
        // 兜底网段：DNS 劫持失败、游戏拿到真实 CDN IP 时仍然由我们应答
        return (pkt[ipOff + 16] & 0xff) == (CDN_CIDR[0] & 0xff)
                && (pkt[ipOff + 17] & 0xff) == (CDN_CIDR[1] & 0xff)
                && (pkt[ipOff + 18] & 0xff) == (CDN_CIDR[2] & 0xff);
    }

    private static boolean u8Eq(byte[] b, int off, byte[] cmp) {
        for (int i = 0; i < 4; i++) {
            if (b[off + i] != cmp[i]) {
                return false;
            }
        }
        return true;
    }

    static String describe() {
        StringBuilder sb = new StringBuilder();
        for (String[] kv : OVERRIDES) {
            sb.append(kv[0]).append(' ');
        }
        return sb.toString().trim();
    }
}
