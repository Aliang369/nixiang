/* 对拍用最小测试器：读向量文件，逐条调 sg_rsa_verify_sha256，打印 OK/FAIL。
 *
 * 向量文件格式（文本，一行一条）：
 *   N <modulus_hex>              # 公钥模数（大端）
 *   V <msg_hex|-> <sig_hex> <1|0> # 期望结果：1=有效，0=无效
 *
 * 退出码 = 失败条数（0 = 全过）。由 selftest_vectors.py 生成向量并调度。
 */
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>

#include "sg_rsa_verify.h"

static int hexval(char c) {
    if (c >= '0' && c <= '9') return c - '0';
    if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    return -1;
}

/* 返回解出的字节数；hex 为 "-" 表示空消息 */
static int unhex(const char* s, uint8_t* out, int cap) {
    if (strcmp(s, "-") == 0) return 0;
    int n = 0;
    size_t len = strlen(s);
    if ((len & 1) != 0) return -1;
    for (size_t i = 0; i < len; i += 2) {
        int hi = hexval(s[i]), lo = hexval(s[i + 1]);
        if (hi < 0 || lo < 0 || n >= cap) return -1;
        out[n++] = (uint8_t)((hi << 4) | lo);
    }
    return n;
}

int main(int argc, char** argv) {
    if (argc < 2) { fprintf(stderr, "usage: %s <vectors.txt>\n", argv[0]); return 99; }
    FILE* f = fopen(argv[1], "r");
    if (!f) { fprintf(stderr, "open fail\n"); return 99; }

    static uint8_t g_n[256];
    int g_n_len = 0;
    static uint8_t msg[4096], sig[1024];
    char line[16384];
    int total = 0, failed = 0;

    while (fgets(line, sizeof(line), f)) {
        char* nl = strchr(line, '\n');
        if (nl) *nl = 0;
        nl = strchr(line, '\r');
        if (nl) *nl = 0;
        if (line[0] == 'N' && line[1] == ' ') {
            g_n_len = unhex(line + 2, g_n, (int)sizeof(g_n));
            if (g_n_len <= 0) { fprintf(stderr, "bad N line\n"); return 99; }
            continue;
        }
        if (line[0] == 'V' && line[1] == ' ') {
            char* m = line + 2;
            char* sp1 = strchr(m, ' ');
            if (!sp1) continue;
            *sp1 = 0;
            char* s = sp1 + 1;
            char* sp2 = strchr(s, ' ');
            if (!sp2) continue;
            *sp2 = 0;
            int want = atoi(sp2 + 1);
            int msg_len = unhex(m, msg, (int)sizeof(msg));
            int sig_len = unhex(s, sig, (int)sizeof(sig));
            if (msg_len < 0 || sig_len < 0) { fprintf(stderr, "bad hex\n"); return 99; }
            int got = sg_rsa_verify_sha256(g_n, (size_t)g_n_len, sig, (size_t)sig_len,
                                           msg, (size_t)msg_len);
            total++;
            if (got != want) {
                failed++;
                printf("FAIL #%d want=%d got=%d msglen=%d siglen=%d\n", total, want, got, msg_len, sig_len);
            } else {
                printf("OK   #%d (%s, msglen=%d)\n", total, got ? "valid" : "invalid", msg_len);
            }
            continue;
        }
    }
    fclose(f);
    printf("SUMMARY total=%d failed=%d\n", total, failed);
    return failed;
}
