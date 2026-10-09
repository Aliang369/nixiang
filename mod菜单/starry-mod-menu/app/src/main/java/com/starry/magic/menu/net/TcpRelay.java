package com.starry.magic.menu.net;

import android.content.Context;
import android.util.Log;

import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * 用户态最小 TCP：接管「客户端 → 我们冒充的 CDN:80」这条连接。
 *
 * <p>两类请求：
 * <ul>
 *   <li><b>覆盖命中</b>：直接回 APK assets 里的伪造清单/补丁 bundle（{@link FakeCdn}）</li>
 *   <li><b>其它</b>：**透传回真实 CDN**（{@code protect()} 过的 socket 直连），
 *       保证游戏真正的资源下载不受影响——这是 v2.6 相对 v2.5 的关键修正</li>
 * </ul>
 *
 * <p>v2.6 相对 v2.5 的发送侧修正（v2.5 实测把游戏下载拖慢 100 倍：真实 CDN 3.2 s，
 * 我们 333 s）：
 * <ul>
 *   <li>对端通告窗口为 0 时**真正停发**（v2.5 里 {@code if (window > 0)} 让零窗口被忽略，
 *       于是持续灌包被丢，只能靠 800 ms 一个报文的重传定时器爬）</li>
 *   <li>重传从 sndUna 起**整批重发**（≥1 个 MSS），RTO 200 ms 起指数退避，不再 800 ms 只发一段</li>
 *   <li>三次重复 ACK 触发快速重传</li>
 *   <li>每连接独立工作线程 + 发送缓冲，边收边发（支持大文件流式透传，不再整体驻留内存）</li>
 *   <li>单次最多连发 {@link #BURST_SEGS} 段，避免一次灌满 TUN 队列引发批量丢包</li>
 * </ul>
 */
final class TcpRelay {

    interface Sink {
        void sendToTun(byte[] pkt);
    }

    /** 让回源 socket 不被自己的隧道再抓回来。 */
    interface Protector {
        boolean protect(Socket s);
    }

    private static final int MSS = FakeCdn.MSS;
    private static final int SND_BUF = 512 * 1024;
    private static final int MAX_INFLIGHT = 128 * 1024;
    private static final int BURST_SEGS = 20;
    private static final long RTO_MIN_MS = 200L;
    private static final long RTO_MAX_MS = 3000L;
    private static final int DUPACK_N = 3;
    private static final long IDLE_MS = 30000L;
    private static final int MAX_RETX = 24;
    private static final int REQ_MAX = 32 * 1024;
    private static final int OUR_WINDOW = 65535;
    private static final int CONNECT_TIMEOUT_MS = 8000;

    private final Context ctx;
    private final Sink sink;
    private final Protector protector;
    private final Map<String, Conn> conns = new HashMap<>();
    private final Object lock = new Object();

    /** 补丁 bundle 是否已完整送达（自动关闭 VPN 的判据）。 */
    private volatile boolean injectionDone;
    private volatile long lastActivity = android.os.SystemClock.elapsedRealtime();

    TcpRelay(Context ctx, Sink sink, Protector protector) {
        this.ctx = ctx;
        this.sink = sink;
        this.protector = protector;
    }

    boolean isInjectionDone() {
        return injectionDone;
    }

    long idleMs() {
        return android.os.SystemClock.elapsedRealtime() - lastActivity;
    }

    int activeConns() {
        synchronized (lock) {
            return conns.size();
        }
    }

    private static final class Conn {
        final byte[] cip = new byte[4];
        final byte[] dip = new byte[4];
        int cport;
        long isn;
        long sndUna;
        long sndNxt;
        long theirSeq;
        long finSeq;
        int peerWindow = 65535;
        boolean established;
        boolean eof;
        boolean finSent;
        boolean finAcked;
        boolean peerFin;
        boolean closed;
        boolean isInjection;
        byte[] buf = new byte[SND_BUF];
        int bufOff;
        int bufLen;
        final byte[] req = new byte[REQ_MAX];
        int reqLen;
        InputStream src;
        Socket sock;
        Thread worker;
        long lastRx;
        long lastAckMs;
        long rto = RTO_MIN_MS;
        int dupAcks;
        int retx;

        int inflight() {
            return (int) (sndNxt - sndUna);
        }

        int unsent() {
            return bufLen - inflight();
        }
    }

    private static long now() {
        return android.os.SystemClock.elapsedRealtime();
    }

    private static String keyOf(byte[] cip, int cport) {
        return (cip[0] & 0xff) + "." + (cip[1] & 0xff) + "." + (cip[2] & 0xff) + "."
                + (cip[3] & 0xff) + ":" + cport;
    }

    private static String keyOf(Conn c) {
        return keyOf(c.cip, c.cport);
    }

    // ---------------------------------------------------------------- 收包

    void onTcpPacket(byte[] pkt, int len) {
        int ipHdr = (pkt[0] & 0x0f) * 4;
        int tcp = ipHdr;
        if (len < tcp + TunPackets.TCP_HDR) {
            return;
        }
        int sport = TunPackets.u16(pkt, tcp);
        int dport = TunPackets.u16(pkt, tcp + 2);
        long seq = TunPackets.u32(pkt, tcp + 4);
        long ack = TunPackets.u32(pkt, tcp + 8);
        int dataOff = ((pkt[tcp + 12] & 0xf0) >> 4) * 4;
        if (dataOff < TunPackets.TCP_HDR) {
            dataOff = TunPackets.TCP_HDR;
        }
        int flags = pkt[tcp + 13] & 0xff;
        int window = TunPackets.u16(pkt, tcp + 14);
        int payloadOff = tcp + dataOff;
        int payloadLen = len - payloadOff;
        if (payloadLen < 0) {
            payloadLen = 0;
        }
        if (dport != FakeCdn.CDN_PORT) {
            return;
        }

        byte[] cip = new byte[4];
        System.arraycopy(pkt, 12, cip, 0, 4);
        byte[] dip = new byte[4];
        System.arraycopy(pkt, 16, dip, 0, 4);

        synchronized (lock) {
            long t = now();
            lastActivity = t;
            String key = keyOf(cip, sport);
            Conn c = conns.get(key);

            if ((flags & TunPackets.SYN) != 0 && (flags & TunPackets.ACK) == 0) {
                if (c != null) {
                    conns.remove(key);
                    closeConn(c);
                }
                Conn nc = new Conn();
                System.arraycopy(cip, 0, nc.cip, 0, 4);
                System.arraycopy(dip, 0, nc.dip, 0, 4);
                nc.cport = sport;
                // ★ SYN 占用一个序号：数据必须从 isn+1 开始，否则首段会被对端当作重传丢弃
                nc.isn = (android.os.SystemClock.elapsedRealtime() + sport) & 0x7fffffffL;
                nc.sndUna = nc.isn + 1;
                nc.sndNxt = nc.isn + 1;
                nc.theirSeq = seq + 1;
                nc.lastRx = t;
                nc.lastAckMs = t;
                conns.put(key, nc);
                send(nc, TunPackets.SYN | TunPackets.ACK, nc.isn, nc.theirSeq, null, 0, 0);
                startWorker(nc);
                Log.i(FakeCdn.TAG, "SYN " + key + " isn=" + nc.isn);
                return;
            }

            if (c == null) {
                long ackNum = seq + payloadLen
                        + (((flags & TunPackets.SYN) != 0) ? 1 : 0)
                        + (((flags & TunPackets.FIN) != 0) ? 1 : 0);
                sink.sendToTun(TunPackets.ipTcp(dip, cip, FakeCdn.CDN_PORT, sport,
                        ack, ackNum, TunPackets.RST | TunPackets.ACK, 0, null, 0, 0));
                return;
            }

            c.lastRx = t;

            if ((flags & TunPackets.RST) != 0) {
                conns.remove(key);
                closeConn(c);
                Log.i(FakeCdn.TAG, "RST from " + key);
                return;
            }

            if ((flags & TunPackets.ACK) != 0) {
                // ★ 三次握手完成：SYN-ACK 被确认（ack 达到 isn+1）即视为已建连，
                //   否则 tick() 里的 established 门槛会挡住重传
                if (!c.established && ack >= c.isn + 1) {
                    c.established = true;
                }
                if (ack > c.sndUna) {
                    long dataEnd = c.sndUna + c.bufLen;
                    long newUna = Math.min(ack, dataEnd);
                    int adv = (int) (newUna - c.sndUna);
                    c.sndUna = newUna;
                    c.bufOff += adv;
                    c.bufLen -= adv;
                    if (c.bufOff > SND_BUF / 2) {
                        compact(c);
                    }
                    c.dupAcks = 0;
                    c.retx = 0;
                    c.rto = RTO_MIN_MS;
                    c.lastAckMs = t;
                    if (!c.established && ack >= c.isn + 1) {
                        c.established = true;
                    }
                    if (c.finSent && ack > c.finSeq) {
                        c.finAcked = true;
                    }
                    // ★ 补丁 bundle 全部字节已被游戏确认（连接可能在 tick() 之前就被回收，
                    //   所以这里必须立刻置位，否则自动关闭判据永远不成立）
                    if (c.isInjection && c.eof && c.bufLen <= 0) {
                        injectionDone = true;
                    }
                    lock.notifyAll();
                } else if (c.sndUna < c.sndNxt) {
                    if (++c.dupAcks >= DUPACK_N) {
                        c.dupAcks = 0;
                        fastRetransmit(c);
                    }
                }
                // ★ 零窗口也必须认，否则会一直往窗口外灌包被丢
                c.peerWindow = window;
            }

            if (payloadLen > 0) {
                if (seq == c.theirSeq) {
                    appendReq(c, pkt, payloadOff, payloadLen);
                    c.theirSeq += payloadLen;
                    send(c, TunPackets.ACK, c.sndNxt, c.theirSeq, null, 0, 0);
                    lock.notifyAll();
                } else {
                    send(c, TunPackets.ACK, c.sndNxt, c.theirSeq, null, 0, 0);
                }
            }

            if ((flags & TunPackets.FIN) != 0) {
                c.theirSeq = seq + payloadLen + 1;
                c.peerFin = true;
                send(c, TunPackets.ACK, c.sndNxt, c.theirSeq, null, 0, 0);
            }

            pump(c);

            if (c.peerFin && c.finAcked) {
                conns.remove(key);
                closeConn(c);
            }
        }
    }

    private void appendReq(Conn c, byte[] pkt, int off, int len) {
        int room = c.req.length - c.reqLen;
        int n = Math.min(len, Math.max(0, room));
        if (n > 0) {
            System.arraycopy(pkt, off, c.req, c.reqLen, n);
            c.reqLen += n;
        }
    }

    private static void compact(Conn c) {
        if (c.bufOff == 0 || c.bufLen <= 0) {
            c.bufOff = c.bufLen <= 0 ? 0 : c.bufOff;
            return;
        }
        System.arraycopy(c.buf, c.bufOff, c.buf, 0, c.bufLen);
        c.bufOff = 0;
    }

    // ---------------------------------------------------------------- 发送

    /** 在对端窗口与在途上限内，把已缓冲但未发送的数据推出去。 */
    private void pump(Conn c) {
        if (c.closed) {
            return;
        }
        int burst = 0;
        while (burst < BURST_SEGS) {
            long cap = Math.min(c.peerWindow, MAX_INFLIGHT);
            long room = cap - c.inflight();
            int unsent = c.unsent();
            if (room <= 0 || unsent <= 0) {
                break;
            }
            int n = (int) Math.min(MSS, Math.min(room, unsent));
            int off = c.bufOff + c.inflight();
            send(c, TunPackets.ACK | TunPackets.PSH, c.sndNxt, c.theirSeq, c.buf, off, n);
            c.sndNxt += n;
            burst++;
        }
        if (!c.finSent && c.eof && c.unsent() <= 0) {
            c.finSeq = c.sndNxt;
            send(c, TunPackets.FIN | TunPackets.ACK, c.sndNxt, c.theirSeq, null, 0, 0);
            c.sndNxt = c.finSeq + 1;
            c.finSent = true;
            if (c.isInjection) {
                injectionDone = true;   // 兜底：FIN 已发出即视为补丁数据已下发完毕
            }
            Log.i(FakeCdn.TAG, "FIN " + keyOf(c));
        }
    }

    private void fastRetransmit(Conn c) {
        if (c.inflight() <= 0) {
            return;
        }
        c.rto = RTO_MIN_MS;
        c.lastAckMs = now();
        retransmit(c);
    }

    private void retransmit(Conn c) {
        int inflight = c.inflight();
        int n = Math.min(Math.min(inflight, MSS * BURST_SEGS), c.bufLen);
        if (n > 0) {
            send(c, TunPackets.ACK | TunPackets.PSH, c.sndUna, c.theirSeq, c.buf, c.bufOff, n);
            return;
        }
        if (c.finSent && !c.finAcked) {
            send(c, TunPackets.FIN | TunPackets.ACK, c.finSeq, c.theirSeq, null, 0, 0);
        }
    }

    /** 零窗口探测：只发 1 字节且不推进 sndNxt，逼对端回报当前窗口。 */
    private void zeroWindowProbe(Conn c) {
        int unsent = c.unsent();
        if (unsent > 0) {
            send(c, TunPackets.ACK | TunPackets.PSH, c.sndNxt, c.theirSeq,
                    c.buf, c.bufOff + c.inflight(), 1);
        } else {
            send(c, TunPackets.ACK, c.sndUna, c.theirSeq, null, 0, 0);
        }
    }

    private void send(Conn c, int flags, long seq, long ack,
                      byte[] payload, int off, int len) {
        sink.sendToTun(TunPackets.ipTcp(c.dip, c.cip, FakeCdn.CDN_PORT, c.cport,
                seq, ack, flags, OUR_WINDOW, payload, off, len));
    }

    // ---------------------------------------------------------------- 定时

    void tick() {
        long t = now();
        synchronized (lock) {
            List<String> kill = new ArrayList<>();
            for (Map.Entry<String, Conn> e : conns.entrySet()) {
                Conn c = e.getValue();
                if (c.closed || t - c.lastRx > IDLE_MS) {
                    kill.add(e.getKey());
                    continue;
                }
                if (c.isInjection && c.eof && c.finSent && c.bufLen <= 0 && !injectionDone) {
                    injectionDone = true;
                    Log.i(FakeCdn.TAG, "补丁 bundle 已完整送达游戏");
                }
                if (!c.established) {
                    continue;
                }
                if (c.sndUna >= c.sndNxt && (!c.finSent || c.finAcked)) {
                    if (c.eof && c.finAcked && c.peerFin) {
                        kill.add(e.getKey());
                    }
                    continue;
                }
                if (t - c.lastAckMs < c.rto) {
                    continue;
                }
                if (++c.retx > MAX_RETX) {
                    Log.w(FakeCdn.TAG, "放弃连接 " + e.getKey());
                    send(c, TunPackets.RST | TunPackets.ACK, c.sndNxt, c.theirSeq, null, 0, 0);
                    kill.add(e.getKey());
                    continue;
                }
                if (c.peerWindow <= 0) {
                    zeroWindowProbe(c);
                } else {
                    Log.i(FakeCdn.TAG, "重传 " + e.getKey() + " una=" + (c.sndUna - c.isn - 1)
                            + " inflight=" + c.inflight() + " win=" + c.peerWindow
                            + " try=" + c.retx);
                    retransmit(c);
                }
                c.rto = Math.min(c.rto * 2, RTO_MAX_MS);
                c.lastAckMs = t;
            }
            for (String k : kill) {
                Conn c = conns.remove(k);
                if (c != null) {
                    closeConn(c);
                }
            }
        }
    }

    void shutdown() {
        synchronized (lock) {
            for (Conn c : conns.values()) {
                closeConn(c);
            }
            conns.clear();
            lock.notifyAll();
        }
    }

    private void closeConn(Conn c) {
        c.closed = true;
        closeSrc(c);
        lock.notifyAll();
    }

    private void closeSrc(Conn c) {
        if (c.src != null) {
            try {
                c.src.close();
            } catch (Throwable ignored) {
            }
            c.src = null;
        }
        if (c.sock != null) {
            try {
                c.sock.close();
            } catch (Throwable ignored) {
            }
            c.sock = null;
        }
    }

    // ---------------------------------------------------------------- 工作线程

    private void startWorker(Conn c) {
        Thread th = new Thread(() -> workerLoop(c), "starry-src");
        th.setDaemon(true);
        c.worker = th;
        th.start();
    }

    private void workerLoop(Conn c) {
        byte[] tmp = new byte[16 * 1024];
        try {
            prepare(c);
            while (true) {
                int room;
                synchronized (lock) {
                    while (!c.closed && c.bufLen >= SND_BUF - MSS && !c.eof) {
                        lock.wait();
                    }
                    if (c.closed) {
                        return;
                    }
                    room = Math.min(tmp.length, SND_BUF - c.bufLen);
                }
                if (room <= 0) {
                    continue;
                }
                int n = c.src.read(tmp, 0, room);
                synchronized (lock) {
                    if (c.closed) {
                        return;
                    }
                    if (n <= 0) {
                        c.eof = true;
                        pump(c);
                        lock.notifyAll();
                        return;
                    }
                    if (c.bufOff + c.bufLen + n > SND_BUF) {
                        compact(c);
                    }
                    System.arraycopy(tmp, 0, c.buf, c.bufOff + c.bufLen, n);
                    c.bufLen += n;
                    pump(c);
                    lock.notifyAll();
                }
            }
        } catch (Throwable t) {
            if (!c.closed) {
                Log.w(FakeCdn.TAG, "数据源结束 " + keyOf(c) + ": " + t);
            }
        } finally {
            synchronized (lock) {
                closeSrc(c);
                lock.notifyAll();
            }
        }
    }

    /** 等请求头收全 → 决定覆盖命中还是回源透传。 */
    private void prepare(Conn c) throws IOException {
        synchronized (lock) {
            while (!c.closed && !hasHead(c)) {
                try {
                    lock.wait();
                } catch (InterruptedException e) {
                    throw new IOException("interrupted while waiting request head");
                }
            }
            if (c.closed) {
                throw new IOException("closed");
            }
        }
        HttpResponder.Request r = HttpResponder.parse(c.req, c.reqLen);
        if (r == null) {
            throw new IOException("bad http request");
        }

        // 只在我们的目标主机上做覆盖，避免误伤其它 Host
        boolean ourHost = r.host == null || r.host.isEmpty()
                || r.host.toLowerCase().contains("hotune");
        if (ourHost && FakeCdn.isOverride(r.basename())) {
            byte[] resp = HttpResponder.buildOverride(ctx, r);
            if (resp != null) {
                // ★ 任何一条伪造清单（versions.json / buildpatch / 补丁 bundle）下发完毕，
                //   即视为本轮注入已完成 —— 游戏已缓存补丁时只会取 versions.json，
                //   此时也必须能触发自动关闭，否则 VPN 会白白常驻数分钟
                c.isInjection = true;
                Log.i(FakeCdn.TAG, "覆盖 " + r.method + " " + r.path + " -> " + resp.length + " B"
                        + (FakeCdn.isPatchBundle(r.basename()) ? " [补丁bundle]" : ""));
                c.src = new ByteArrayInputStream(resp);
                return;
            }
        }
        openUpstream(c, r);
    }

    private void openUpstream(Conn c, HttpResponder.Request r) throws IOException {
        InetAddress addr = FakeCdn.realCdnAddress();
        Socket s = new Socket();
        try {
            s.setTcpNoDelay(true);
            if (protector != null) {
                protector.protect(s);
            }
            s.connect(new InetSocketAddress(addr, FakeCdn.CDN_PORT), CONNECT_TIMEOUT_MS);
            OutputStream os = s.getOutputStream();
            os.write(HttpResponder.forwardRequest(c.req, c.reqLen));
            os.flush();
        } catch (Throwable t) {
            try {
                s.close();
            } catch (Throwable ignored) {
            }
            throw new IOException("upstream failed: " + t);
        }
        c.sock = s;
        c.src = s.getInputStream();
        FakeCdn.PASSED.incrementAndGet();
        Log.i(FakeCdn.TAG, "透传 " + r.method + " " + r.path + " -> " + addr.getHostAddress());
    }

    private static boolean hasHead(Conn c) {
        byte[] b = c.req;
        for (int i = 0; i + 3 < c.reqLen; i++) {
            if (b[i] == '\r' && b[i + 1] == '\n' && b[i + 2] == '\r' && b[i + 3] == '\n') {
                return true;
            }
        }
        return false;
    }

    // 供 CdnVpnService 在 SYN 之后拉起工作线程
    void onSynAccepted(ConnHolder h) {
        // 占位：连接创建即由 onTcpPacket 内部启动线程，这里不用
    }

    /** 供外部持有的最小句柄，避免把 Conn 暴露出去。 */
    interface ConnHolder {
    }
}
