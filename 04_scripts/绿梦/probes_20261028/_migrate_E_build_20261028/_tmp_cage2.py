p = r"E:\nixiang\04_scripts\绿梦\fg_cage.c"
s = open(p, encoding="utf-8").read()

# 在 BLOCK 时扫栈，找出落在 guest 库区间里的返回地址（定位"到底是谁在退出"）
blk = r'''
/* ---- 栈扫描：把栈上落在 guest 映射区间里的指针挑出来，用于定位退出的发起库 ---- */
typedef struct { unsigned long lo, hi; char nm[96]; } MapEnt;
static MapEnt g_maps[256];
static int g_nmaps = 0;

static void load_maps(pid_t pid) {
    g_nmaps = 0;
    char mp[64]; snprintf(mp, sizeof(mp), "/proc/%d/maps", pid);
    FILE *f = fopen(mp, "r");
    if (!f) return;
    char line[512];
    while (fgets(line, sizeof(line), f) && g_nmaps < 256) {
        unsigned long lo, hi; char perms[8] = {0};
        if (sscanf(line, "%lx-%lx %7s", &lo, &hi, perms) != 3) continue;
        char *slash = strrchr(line, '/');
        if (!slash) continue;
        slash[strcspn(slash, "\n")] = 0;
        const char *base = strrchr(slash, '/'); base = base ? base + 1 : slash;
        if (!strstr(base, "lib")) continue;
        g_maps[g_nmaps].lo = lo; g_maps[g_nmaps].hi = hi;
        snprintf(g_maps[g_nmaps].nm, sizeof(g_maps[g_nmaps].nm), "%s", base);
        g_nmaps++;
    }
    fclose(f);
}

static void dump_stack_hits(pid_t tid, unsigned long rsp, unsigned long lr) {
    if (!g_nmaps) return;
    for (int pass = 0; pass < 1; pass++) {
        /* lr 本身 */
        for (int i = 0; i < g_nmaps; i++)
            if (lr >= g_maps[i].lo && lr < g_maps[i].hi)
                logline("      lr=%#lx -> %s+%#lx\n", lr, g_maps[i].nm, lr - g_maps[i].lo);
        /* 栈上 512 个字 */
        for (int w = 0; w < 512; w++) {
            errno = 0;
            long v = ptrace(PTRACE_PEEKDATA, tid, (void *)(rsp + w * 8), 0);
            if (v == -1 && errno) break;
            unsigned long u = (unsigned long)v;
            for (int i = 0; i < g_nmaps; i++)
                if (u >= g_maps[i].lo && u < g_maps[i].hi) {
                    logline("      stk[%d]=%#lx -> %s+%#lx\n", w, u, g_maps[i].nm, u - g_maps[i].lo);
                    break;
                }
        }
    }
}
'''
s = s.replace("static void add_tid(pid_t tid) {", blk + "\nstatic void add_tid(pid_t tid) {")
s = s.replace('''                        char mod[128]; mod_of(pid, (unsigned long)r.rip, mod, sizeof(mod));''',
'''                        char mod[128]; mod_of(pid, (unsigned long)r.rip, mod, sizeof(mod));
                        if (load_maps_once) { load_maps(pid); load_maps_once = 0; }
                        dump_stack_hits(tid, (unsigned long)r.rsp, (unsigned long)r.rax /* placeholder */);''')
s = s.replace("static int cage = 0;", "static int cage = 0;\nstatic int load_maps_once = 1;")
open(p, "w", encoding="utf-8").write(s)
print("stack recon added")
