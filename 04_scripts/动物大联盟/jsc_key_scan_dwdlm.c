
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <zlib.h>

#define DELTA 0x9E3779B9UL
#define MX (((z>>5^y<<2) + (y>>3^z<<4)) ^ ((sum^y) + (k[(p&3)^e] ^ z)))

static void btea(uint32_t *v, int n, const uint32_t k[4]) {
    uint32_t y, z, sum; unsigned p, rounds, e;
    if (n > 1) {
        rounds = 6 + 52/n; sum = 0; z = v[n-1];
        do {
            sum += DELTA; e = (sum >> 2) & 3;
            for (p = 0; p < (unsigned)(n-1); p++) { y = v[p+1]; z = v[p] += MX; }
            y = v[0]; z = v[n-1] += MX;
        } while (--rounds);
    } else if (n < -1) {
        n = -n;
        rounds = 6 + 52/n; sum = rounds * DELTA; y = v[0];
        do {
            e = (sum >> 2) & 3;
            for (p = n-1; p > 0; p--) { z = v[p-1]; y = v[p] -= MX; }
            z = v[n-1]; y = v[0] -= MX;
            sum -= DELTA;
        } while (--rounds);
    }
}

static int has(const unsigned char *b, int n, const char *needle) {
    int m = strlen(needle);
    for (int i = 0; i + m <= n; i++) if (!memcmp(b+i, needle, m)) return 1;
    return 0;
}

int main(int argc, char **argv) {
    {
        const uint32_t k[4] = {0x01020304,0x05060708,0x090a0b0c,0x0d0e0f10};
        unsigned char msg[32] = "the quick brown fox jumps ov";
        uint32_t v[8]; memcpy(v, msg, 32);
        btea(v, 8, k); btea(v, -8, k);
        printf("selftest %s\n", memcmp(v,msg,32)==0 ? "OK" : "FAIL");
    }
    FILE *f = fopen(argv[1],"rb"); fseek(f,0,SEEK_END); long clen=ftell(f); fseek(f,0,SEEK_SET);
    unsigned char *cipher = malloc(clen); fread(cipher,1,clen,f); fclose(f);

    FILE *kf = fopen(argv[2],"rb");
    char line[512]; int found=0; long tried=0;
    unsigned char *out = malloc(1<<20), *dec = malloc(1<<20);
    while (fgets(line, sizeof line, kf)) {
        int kl = strlen(line); while (kl && (line[kl-1]=='\n'||line[kl-1]=='\r')) line[--kl]=0;
        if (kl < 16) continue;
        uint32_t k[4];
        for (int t=0;t<4;t++)
            k[t]=(uint32_t)(unsigned char)line[t*4] | ((uint32_t)(unsigned char)line[t*4+1]<<8)
                | ((uint32_t)(unsigned char)line[t*4+2]<<16) | ((uint32_t)(unsigned char)line[t*4+3]<<24);
        for (int hdr=0; hdr<=16; hdr+=4) {
            int n = (clen-hdr)/4; if (n < 2) continue;
            memcpy(out, cipher+hdr, n*4);
            btea((uint32_t*)out, -n, k);
            tried++;
            uLongf dl = 1<<20;
            if (uncompress(dec, &dl, out, n*4) == Z_OK && dl > 16) {
                if (has(dec, (int)dl, "CCSettings") || has(dec, (int)dl, "window.")) {
                    printf("[HIT-zlib] hdr=%d dlen=%lu key=%.24s\n%.200s\n", hdr, (unsigned long)dl, line, (char*)dec);
                    found++;
                    continue;
                }
            }
            if (has(out, n*4, "CCSettings") || has(out, n*4, "window.boot")) {
                printf("[HIT-text] hdr=%d key=%.24s\n%.200s\n", hdr, line, (char*)out);
                found++;
            }
        }
    }
    printf("tried=%ld found=%d\n", tried, found);
    return 0;
}
