package com.starry.magic.menu.net;

import android.net.VpnService;
import android.util.Log;

import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/**
 * DNS 拦截：把 {@link FakeCdn#CDN_HOST} 应答成**真实 CDN IP**（{@link FakeCdn#primaryCdnIp()}），
 * **其余查询一律原样转发到上游 DNS**（保证其它 App 与游戏登录不受影响）。
 *
 * <p>v3.2 修正：早先应答成隧道内假 IP（10.9.9.9），注入完成隧道自动关闭并撤掉
 * {@code 10.9.9.9/32} 路由后，系统解析器/游戏侧仍缓存那个假地址 ⇒ 之后按需下载的
 * 活动资源、广告面板直接 ConnectFailure(110) 卡死。现在只回真实 IP，并且
 * CdnVpnService 会给每个真实 IP 加 /32 路由：隧道开着照样拦到，隧道关掉也能连通。
 *
 * <p>转发用的 socket 会被 {@code protect()} 排除在 VPN 之外，避免自己转自己。
 */
final class DnsInterceptor {

    interface Sink {
        void sendToTun(byte[] pkt);
    }

    private static final String[] UPSTREAMS = {"223.5.5.5", "119.29.29.29", "114.114.114.114"};

    private final VpnService vpn;
    private final Sink sink;
    private final ExecutorService pool = Executors.newFixedThreadPool(3, r -> {
        Thread t = new Thread(r, "starry-dns");
        t.setDaemon(true);
        return t;
    });

    private volatile boolean alive = true;

    DnsInterceptor(VpnService vpn, Sink sink) {
        this.vpn = vpn;
        this.sink = sink;
    }

    void shutdown() {
        alive = false;
        pool.shutdownNow();
    }

    /** 收到一个完整 IP 报文（UDP:53）。 */
    void onUdpPacket(byte[] pkt, int len) {
        int ipHdr = (pkt[0] & 0x0f) * 4;
        int udpOff = ipHdr;
        int sport = TunPackets.u16(pkt, udpOff);
        int dport = TunPackets.u16(pkt, udpOff + 2);
        int dnsOff = udpOff + TunPackets.UDP_HDR;
        int dnsLen = len - dnsOff;
        if (dport != FakeCdn.DNS_PORT || dnsLen < 12) {
            return;
        }
        byte[] sip = new byte[4];
        System.arraycopy(pkt, 12, sip, 0, 4);
        // 应答的源地址必须回填客户端原本查询的那台 DNS，否则解析器可能丢弃应答
        byte[] dip = new byte[4];
        System.arraycopy(pkt, 16, dip, 0, 4);

        String qname = null;
        int qend = 0;
        int qtype = 0;
        try {
            int[] end = new int[1];
            qname = parseName(pkt, dnsOff + 12, end);
            qend = end[0];
            qtype = TunPackets.u16(pkt, qend);
        } catch (Throwable t) {
            Log.w(FakeCdn.TAG, "dns parse failed: " + t);
            return;
        }

        if (qname != null && qname.equalsIgnoreCase(FakeCdn.CDN_HOST)) {
            // v3.2：不再应答假 IP。假 IP（10.9.9.9）在隧道自动关闭后会变成黑洞
            // （系统/Mono 仍缓存，/32 路由已撤销）⇒ 活动资源、广告面板卡死。
            // 改为应答「真实 CDN IP + 该 IP 走隧道」：隧道开着照样拦到，
            // 隧道关掉缓存的也是真能连通的地址。
            byte[] realIp = FakeCdn.primaryCdnIp();
            if (realIp != null) {
                byte[] resp = buildAnswer(pkt, dnsOff, qend, dnsLen, qtype, realIp);
                if (resp != null) {
                    sink.sendToTun(TunPackets.ipUdp(dip, sip,
                            FakeCdn.DNS_PORT, sport, resp, 0, resp.length));
                    Log.i(FakeCdn.TAG, "DNS " + qname + " -> " + TunPackets.ipToStr(realIp, 0)
                            + " (真实 CDN,qtype=" + qtype + ")");
                    return;
                }
            }
            // 没解析到真实 IP：不要瞎回答，交给上游；下面的转发分支会兜住。
            Log.w(FakeCdn.TAG, "DNS " + qname + " 无真实 CDN IP，改走上游转发");
        }

        // 其余域名：异步转发上游，别阻塞 TUN 读循环
        final byte[] query = new byte[dnsLen];
        System.arraycopy(pkt, dnsOff, query, 0, dnsLen);
        final byte[] clientIp = sip;
        final byte[] serverIp = dip;
        final int clientPort = sport;
        final String name = qname;
        try {
            pool.execute(() -> forward(query, clientIp, serverIp, clientPort, name));
        } catch (Throwable t) {
            // 线程池已关闭
        }
    }

    /** 命中目标域名：直接在本地组应答，不去上游。answerIp 是真实 CDN IP。 */
    private byte[] buildAnswer(byte[] pkt, int dnsOff, int qEnd, int dnsLen, int qtype, byte[] answerIp) {
        try {
            byte[] q = new byte[dnsLen];
            System.arraycopy(pkt, dnsOff, q, 0, dnsLen);
            // 问题段 = 名字 + QTYPE(2) + QCLASS(2)；qEnd 只指向 QTYPE 起点，
            // 少加这 4 字节会让应答的问题段残缺，解析器会直接丢弃整条应答。
            int qSectionEnd = (qEnd - dnsOff) + 4;

            boolean isA = (qtype == 1);
            int ancount = isA ? 1 : 0;
            byte[] out = new byte[qSectionEnd + (isA ? 16 : 0)];
            System.arraycopy(q, 0, out, 0, qSectionEnd);
            out[2] = (byte) 0x81;                  // QR=1, opcode=0, AA=0, TC=0, RD=1
            out[3] = (byte) 0x80;                  // RA=1, RCODE=0
            TunPackets.put16(out, 4, 1);           // QDCOUNT
            TunPackets.put16(out, 6, ancount);     // ANCOUNT
            TunPackets.put16(out, 8, 0);           // NSCOUNT
            TunPackets.put16(out, 10, 0);          // ARCOUNT（丢掉 EDNS OPT）
            if (isA) {
                int o = qSectionEnd;
                TunPackets.put16(out, o, 0xC00C);      // 名字指针 → 偏移 12
                TunPackets.put16(out, o + 2, 1);       // TYPE A
                TunPackets.put16(out, o + 4, 1);       // CLASS IN
                TunPackets.put32(out, o + 6, 60);      // TTL 60s
                TunPackets.put16(out, o + 10, 4);      // RDLENGTH
                System.arraycopy(answerIp, 0, out, o + 12, 4);
            }
            return out;
        } catch (Throwable t) {
            Log.w(FakeCdn.TAG, "dns build failed: " + t);
            return null;
        }
    }

    /** 非目标域名：原样问上游，再把应答按原路送回。 */
    private void forward(byte[] query, byte[] clientIp, byte[] serverIp,
                         int clientPort, String name) {
        for (String up : UPSTREAMS) {
            if (!alive) {
                return;
            }
            DatagramSocket s = null;
            try {
                s = new DatagramSocket();
                protect(s);
                s.setSoTimeout(3000);
                InetAddress addr = InetAddress.getByName(up);
                s.send(new DatagramPacket(query, query.length, addr, FakeCdn.DNS_PORT));
                byte[] rb = new byte[2048];
                DatagramPacket rp = new DatagramPacket(rb, rb.length);
                s.receive(rp);
                sink.sendToTun(TunPackets.ipUdp(serverIp, clientIp,
                        FakeCdn.DNS_PORT, clientPort, rb, 0, rp.getLength()));
                return;
            } catch (Throwable t) {
                Log.w(FakeCdn.TAG, "dns forward via " + up + " failed for " + name + ": " + t);
            } finally {
                if (s != null) {
                    s.close();
                }
            }
        }
        Log.w(FakeCdn.TAG, "dns forward give up: " + name);
    }

    private void protect(DatagramSocket s) {
        try {
            if (vpn != null) {
                vpn.protect(s);
            }
        } catch (Throwable ignored) {
        }
    }

    /** 解析 DNS 名字；end[0] 回填"名字之后的偏移"。 */
    private static String parseName(byte[] d, int off, int[] end) {
        StringBuilder sb = new StringBuilder();
        int i = off;
        while (true) {
            if (i >= d.length) {
                break;
            }
            int l = d[i] & 0xff;
            if (l == 0) {
                i++;
                break;
            }
            if ((l & 0xc0) == 0xc0) {
                i += 2;
                break;
            }
            i++;
            for (int j = 0; j < l && i + j < d.length; j++) {
                sb.append((char) (d[i + j] & 0xff));
            }
            sb.append('.');
            i += l;
        }
        end[0] = i;
        String s = sb.toString();
        return s.endsWith(".") ? s.substring(0, s.length() - 1) : s;
    }
}
