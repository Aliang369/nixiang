/* host 侧单测：把 Games/ZxrDerive.h 的「结构派生」直接编进 mac 可执行文件，
 * 对多份历史热更明文跑一遍，输出 JSON plan（可直接喂 node test_zxr_patch.js 做语义回归）。
 *
 * 用法： test_zxr_derive <decrypted.js> [version-tag]
 * 退出码： 0 = 三个钩子全部派生成功；1 = 有钩子失败（stderr 打 why）
 */
#include <cstdio>
#include <cstdlib>
#include <string>
#include <vector>

#include "ZxrDerive.h"

static std::string JsonEscape(const std::string& s) {
    std::string out;
    out.reserve(s.size() + 16);
    for (char c : s) {
        switch (c) {
            case '"': out += "\\\""; break;
            case '\\': out += "\\\\"; break;
            case '\n': out += "\\n"; break;
            case '\r': out += "\\r"; break;
            case '\t': out += "\\t"; break;
            default:
                if ((unsigned char)c < 0x20) {
                    char buf[8];
                    snprintf(buf, sizeof(buf), "\\u%04x", (unsigned char)c);
                    out += buf;
                } else {
                    out += c;
                }
        }
    }
    return out;
}

int main(int argc, char** argv) {
    if (argc < 2) {
        fprintf(stderr, "usage: %s <decrypted.js> [tag]\n", argv[0]);
        return 2;
    }
    const char* tag = argc >= 3 ? argv[2] : "unknown";
    FILE* f = fopen(argv[1], "rb");
    if (!f) {
        fprintf(stderr, "open failed: %s\n", argv[1]);
        return 2;
    }
    std::string src;
    {
        char buf[1 << 16];
        size_t n;
        while ((n = fread(buf, 1, sizeof(buf), f)) > 0) src.append(buf, n);
    }
    fclose(f);

    struct Item {
        const char* name;
        bool (*fn)(const std::string&, zxrderive::Plan*, std::string*);
    };
    const Item items[] = {
        {"damage_and_invincible", zxrderive::DeriveDamage},
        {"attack_speed", zxrderive::DeriveAttackSpeed},
        {"full_screen_kill", zxrderive::DeriveFullScreenKill},
    };

    printf("{\"tag\":\"%s\",\"size\":%zu,\"hooks\":{", tag, src.size());
    int okCount = 0;
    for (size_t i = 0; i < sizeof(items) / sizeof(items[0]); ++i) {
        zxrderive::Plan plan;
        std::string why;
        const bool ok = items[i].fn(src, &plan, &why);
        if (ok) ++okCount;
        // 命中次数必须是 1（防止多命中导致 splice 打歪）
        int hits = 0;
        if (ok && !plan.anchor.empty()) {
            size_t p = 0;
            while ((p = src.find(plan.anchor, p)) != std::string::npos) { ++hits; p += plan.anchor.size(); }
        }
        printf("%s\"%s\":{\"ok\":%s,\"hits\":%d,\"anchor\":\"%s\",\"insert\":\"%s\",\"why\":\"%s\"}",
               i ? "," : "", items[i].name, ok ? "true" : "false", hits,
               JsonEscape(plan.anchor).c_str(), JsonEscape(plan.insert).c_str(), JsonEscape(why).c_str());
        if (!ok) fprintf(stderr, "[%s] %s FAILED: %s\n", tag, items[i].name, why.c_str());
        else if (hits != 1) fprintf(stderr, "[%s] %s anchor hits=%d (expect 1)\n", tag, items[i].name, hits);
    }
    printf("}}\n");
    fprintf(stderr, "[%s] derive ok=%d/3\n", tag, okCount);
    return okCount == 3 ? 0 : 1;
}
