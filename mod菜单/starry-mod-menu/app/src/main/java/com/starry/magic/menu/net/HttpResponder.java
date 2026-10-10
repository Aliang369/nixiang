package com.starry.magic.menu.net;

import android.content.Context;
import android.util.Log;

import com.starry.magic.menu.InjectJournal;

import java.io.ByteArrayOutputStream;

/**
 * 解析客户端 HTTP 请求并生成应答。
 *
 * <p>只做我们需要的三件事：命中覆盖表就整文件返回（支持 Range 206）；
 * **没命中返回 null，交给 {@link TcpRelay} 回源真实 CDN 透传**（v2.5 的 404
 * 会打断游戏正常的资源下载，实测让 loading 卡在 16%）。
 * 应答一律 {@code Connection: close}，避免自己实现 keep-alive 的状态机；
 * 客户端要下一个文件会自己新建连接。
 */
final class HttpResponder {

    private HttpResponder() {
    }

    static final class Request {
        String method = "";
        String path = "";
        String host = "";
        long rangeStart = -1;
        long rangeEnd = -1;
        boolean valid;

        String basename() {
            String p = path;
            int q = p.indexOf('?');
            if (q >= 0) {
                p = p.substring(0, q);
            }
            int slash = p.lastIndexOf('/');
            return slash >= 0 ? p.substring(slash + 1) : p;
        }
    }

    /** 从已收到的字节里尽量解析出一个请求；还没收全返回 null。 */
    static Request parse(byte[] buf, int len) {
        int headEnd = indexOf(buf, len, "\r\n\r\n");
        if (headEnd < 0) {
            return null;
        }
        String head = new String(buf, 0, headEnd, java.nio.charset.StandardCharsets.ISO_8859_1);
        String[] lines = head.split("\r\n");
        if (lines.length == 0) {
            return null;
        }
        Request r = new Request();
        String[] first = lines[0].split(" ");
        if (first.length < 2) {
            return null;
        }
        r.method = first[0];
        r.path = first[1];
        for (int i = 1; i < lines.length; i++) {
            int c = lines[i].indexOf(':');
            if (c <= 0) {
                continue;
            }
            String k = lines[i].substring(0, c).trim().toLowerCase();
            String v = lines[i].substring(c + 1).trim();
            if (k.equals("host")) {
                r.host = v;
            } else if (k.equals("range")) {
                parseRange(v, r);
            }
        }
        r.valid = !r.method.isEmpty() && !r.path.isEmpty();
        return r.valid ? r : null;
    }

    private static void parseRange(String v, Request r) {
        try {
            String s = v.trim();
            int eq = s.indexOf('=');
            if (eq >= 0) {
                s = s.substring(eq + 1);
            }
            int dash = s.indexOf('-');
            if (dash < 0) {
                return;
            }
            String a = s.substring(0, dash).trim();
            String b = s.substring(dash + 1).trim();
            r.rangeStart = a.isEmpty() ? -1 : Long.parseLong(a);
            r.rangeEnd = b.isEmpty() ? -1 : Long.parseLong(b);
        } catch (Throwable ignored) {
        }
    }

    /** 命中覆盖表时生成完整应答；未命中返回 {@code null}（由上层回源透传）。 */
    static byte[] buildOverride(Context ctx, Request req) {
        String base = req.basename();
        byte[] body = FakeCdn.content(ctx, base);
        if (body == null) {
            // ★ v2.6：不再 404，交回上层去真实 CDN 取
            return null;
        }
        FakeCdn.SERVED.incrementAndGet();
        // 体检用：游戏真的来取补丁了 —— 这条是「注入到底有没有被游戏用上」的唯一硬证据
        InjectJournal.served(ctx, base, body.length);

        boolean ranged = req.rangeStart >= 0 || req.rangeEnd >= 0;
        if (ranged) {
            long total = body.length;
            long start = req.rangeStart < 0 ? 0 : req.rangeStart;
            long end = req.rangeEnd < 0 ? total - 1 : Math.min(req.rangeEnd, total - 1);
            if (start > end || start >= total) {
                return simple("416 Range Not Satisfiable", "text/plain", "bad range\n".getBytes());
            }
            int n = (int) (end - start + 1);
            ByteArrayOutputStream bos = new ByteArrayOutputStream(n + 256);
            head(bos, "206 Partial Content", FakeCdn.contentTypeOf(base), n,
                    "Content-Range: bytes " + start + "-" + end + "/" + total + "\r\n");
            bos.write(body, (int) start, n);
            Log.i(FakeCdn.TAG, "206 " + req.path + " bytes=" + start + "-" + end + "/" + total);
            return bos.toByteArray();
        }

        ByteArrayOutputStream bos = new ByteArrayOutputStream(body.length + 256);
        head(bos, "200 OK", FakeCdn.contentTypeOf(base), body.length, null);
        bos.write(body, 0, body.length);
        Log.i(FakeCdn.TAG, "200 " + req.path + " (Host:" + req.host + ") size=" + body.length
                + " base=" + base);
        return bos.toByteArray();
    }

    /**
     * 透传给真实 CDN 的请求头：保留原请求行与大部分头，但强制
     * {@code Connection: close}（我们不做 keep-alive，客户端每个文件一条连接）。
     */
    static byte[] forwardRequest(byte[] req, int len) {
        int headEnd = indexOf(req, len, "\r\n\r\n");
        int bodyOff = headEnd < 0 ? len : headEnd;
        String head = new String(req, 0, bodyOff, java.nio.charset.StandardCharsets.ISO_8859_1);
        StringBuilder sb = new StringBuilder(head.length() + 64);
        String[] lines = head.split("\r\n");
        for (int i = 0; i < lines.length; i++) {
            String line = lines[i];
            if (line.isEmpty()) {
                continue;
            }
            int c = line.indexOf(':');
            if (c > 0) {
                String k = line.substring(0, c).trim().toLowerCase();
                if (k.equals("connection") || k.equals("proxy-connection")
                        || k.equals("keep-alive")) {
                    continue;
                }
            }
            sb.append(line).append("\r\n");
        }
        sb.append("Connection: close\r\n\r\n");
        byte[] out = sb.toString().getBytes(java.nio.charset.StandardCharsets.ISO_8859_1);
        if (bodyOff < len) {
            byte[] full = new byte[out.length + (len - bodyOff)];
            System.arraycopy(out, 0, full, 0, out.length);
            System.arraycopy(req, bodyOff, full, out.length, len - bodyOff);
            return full;
        }
        return out;
    }

    private static byte[] simple(String status, String ctype, byte[] body) {
        ByteArrayOutputStream bos = new ByteArrayOutputStream(body.length + 256);
        head(bos, status, ctype, body.length, null);
        bos.write(body, 0, body.length);
        return bos.toByteArray();
    }

    private static void head(ByteArrayOutputStream bos, String status, String ctype,
                             int len, String extra) {
        StringBuilder sb = new StringBuilder(256);
        sb.append("HTTP/1.1 ").append(status).append("\r\n");
        // 客户端用的是 HTTP/1.0 + keep-alive；我们明确 close，语义最简单且一定正确
        sb.append("Content-Type: ").append(ctype).append("\r\n");
        sb.append("Content-Length: ").append(len).append("\r\n");
        sb.append("Accept-Ranges: bytes\r\n");
        if (extra != null) {
            sb.append(extra);
        }
        sb.append("Connection: close\r\n");
        sb.append("\r\n");
        byte[] h = sb.toString().getBytes(java.nio.charset.StandardCharsets.ISO_8859_1);
        bos.write(h, 0, h.length);
    }

    private static int indexOf(byte[] b, int len, String needle) {
        byte[] n = needle.getBytes(java.nio.charset.StandardCharsets.ISO_8859_1);
        outer:
        for (int i = 0; i + n.length <= len; i++) {
            for (int j = 0; j < n.length; j++) {
                if (b[i + j] != n[j]) {
                    continue outer;
                }
            }
            return i;
        }
        return -1;
    }
}
