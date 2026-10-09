package com.starry.magic.menu.net;

/**
 * IPv4 / TCP / UDP 报文构造与解析的最小工具集。
 *
 * <p>用途：VpnService 的 TUN 口拿到的是**原始 IP 报文**，我们要在用户态自己
 * 组包回给系统协议栈，所以 IP 头校验和与 TCP/UDP 的伪头校验和都必须算对，
 * 否则内核会静默丢弃我们写回去的包。
 *
 * <p>只支持 IPv4、无选项的 20 字节 IP 头与 20 字节 TCP/UDP 头（够用且简单）。
 */
final class TunPackets {

    static final int IP_HDR = 20;
    static final int TCP_HDR = 20;
    static final int UDP_HDR = 8;

    static final int FIN = 0x01;
    static final int SYN = 0x02;
    static final int RST = 0x04;
    static final int PSH = 0x08;
    static final int ACK = 0x10;

    private TunPackets() {
    }

    // ---------------- 读数 ----------------

    static int u8(byte[] b, int i) {
        return b[i] & 0xff;
    }

    static int u16(byte[] b, int i) {
        return ((b[i] & 0xff) << 8) | (b[i + 1] & 0xff);
    }

    static long u32(byte[] b, int i) {
        return ((long) (b[i] & 0xff) << 24)
                | ((long) (b[i + 1] & 0xff) << 16)
                | ((long) (b[i + 2] & 0xff) << 8)
                | (long) (b[i + 3] & 0xff);
    }

    static void put16(byte[] b, int i, int v) {
        b[i] = (byte) (v >> 8);
        b[i + 1] = (byte) v;
    }

    static void put32(byte[] b, int i, long v) {
        b[i] = (byte) (v >> 24);
        b[i + 1] = (byte) (v >> 16);
        b[i + 2] = (byte) (v >> 8);
        b[i + 3] = (byte) v;
    }

    static String ipToStr(byte[] ip, int off) {
        return (ip[off] & 0xff) + "." + (ip[off + 1] & 0xff) + "."
                + (ip[off + 2] & 0xff) + "." + (ip[off + 3] & 0xff);
    }

    // ---------------- 校验和 ----------------

    /** 一补数累加（不折叠、不取反）。 */
    private static int addSum(byte[] d, int off, int len, int sum) {
        int i = off;
        int end = off + len;
        while (i + 1 < end) {
            sum += ((d[i] & 0xff) << 8) | (d[i + 1] & 0xff);
            i += 2;
        }
        if (i < end) {
            sum += (d[i] & 0xff) << 8;
        }
        return sum;
    }

    private static int fold(int sum) {
        while ((sum >>> 16) != 0) {
            sum = (sum & 0xffff) + (sum >>> 16);
        }
        return sum;
    }

    private static int finish(int sum) {
        return (~fold(sum)) & 0xffff;
    }

    /** TCP/UDP 伪头部的累加值（不含数据段本身）。 */
    private static int pseudoSum(byte[] srcIp, int srcOff, byte[] dstIp, int dstOff,
                                 int proto, int payloadLen) {
        int s = 0;
        s = addSum(srcIp, srcOff, 4, s);
        s = addSum(dstIp, dstOff, 4, s);
        s += proto;
        s += payloadLen;
        return s;
    }

    // ---------------- 构造 ----------------

    /**
     * 构造一个 IPv4 + TCP 报文。
     *
     * @param srcIp      源 IP（4 字节，写入 IP 头与 TCP 伪头）
     * @param dstIp      目的 IP（4 字节）
     * @param payload    负载，可为 null
     * @param payloadLen 负载长度
     */
    static byte[] ipTcp(byte[] srcIp, byte[] dstIp,
                        int sport, int dport,
                        long seq, long ack, int flags, int window,
                        byte[] payload, int payloadOff, int payloadLen) {
        int total = IP_HDR + TCP_HDR + (payloadLen > 0 ? payloadLen : 0);
        byte[] p = new byte[total];

        // --- IPv4 头 ---
        p[0] = 0x45;                    // version 4, IHL 5
        p[1] = 0;                       // DSCP/ECN
        put16(p, 2, total);             // total length
        put16(p, 4, 0);                 // identification
        put16(p, 6, 0x4000);            // DF
        p[8] = 64;                      // TTL
        p[9] = 6;                       // TCP
        put16(p, 10, 0);                // checksum 占位
        System.arraycopy(srcIp, 0, p, 12, 4);
        System.arraycopy(dstIp, 0, p, 16, 4);
        put16(p, 10, finish(addSum(p, 0, IP_HDR, 0)));

        // --- TCP 头 ---
        int o = IP_HDR;
        put16(p, o, sport);
        put16(p, o + 2, dport);
        put32(p, o + 4, seq & 0xffffffffL);
        put32(p, o + 8, ack & 0xffffffffL);
        p[o + 12] = (byte) ((TCP_HDR / 4) << 4);   // data offset=5, reserved=0
        p[o + 13] = (byte) flags;
        put16(p, o + 14, window);
        put16(p, o + 16, 0);            // checksum 占位
        put16(p, o + 18, 0);            // urgent
        if (payloadLen > 0 && payload != null) {
            System.arraycopy(payload, payloadOff, p, o + TCP_HDR, payloadLen);
        }
        int ps = pseudoSum(srcIp, 0, dstIp, 0, 6, TCP_HDR + (payloadLen > 0 ? payloadLen : 0));
        put16(p, o + 16, finish(addSum(p, o, TCP_HDR + (payloadLen > 0 ? payloadLen : 0), ps)));
        return p;
    }

    /** 构造一个 IPv4 + UDP 报文（DNS 用）。 */
    static byte[] ipUdp(byte[] srcIp, byte[] dstIp,
                        int sport, int dport,
                        byte[] payload, int payloadOff, int payloadLen) {
        int total = IP_HDR + UDP_HDR + payloadLen;
        byte[] p = new byte[total];

        p[0] = 0x45;
        p[1] = 0;
        put16(p, 2, total);
        put16(p, 4, 0);
        put16(p, 6, 0x4000);
        p[8] = 64;
        p[9] = 17;                       // UDP
        put16(p, 10, 0);
        System.arraycopy(srcIp, 0, p, 12, 4);
        System.arraycopy(dstIp, 0, p, 16, 4);
        put16(p, 10, finish(addSum(p, 0, IP_HDR, 0)));

        int o = IP_HDR;
        put16(p, o, sport);
        put16(p, o + 2, dport);
        put16(p, o + 4, UDP_HDR + payloadLen);
        put16(p, o + 6, 0);
        System.arraycopy(payload, payloadOff, p, o + UDP_HDR, payloadLen);
        int ps = pseudoSum(srcIp, 0, dstIp, 0, 17, UDP_HDR + payloadLen);
        int ck = finish(addSum(p, o, UDP_HDR + payloadLen, ps));
        put16(p, o + 6, ck == 0 ? 0xffff : ck);   // UDP 校验和 0 表示"未计算"
        return p;
    }
}
