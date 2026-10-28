// ct7.c ? ???houdini ?**????? guest ???**??????? svc ?????????
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <dlfcn.h>
#include <errno.h>
#include <sys/mman.h>

int main(void) {
    void *p = dlsym(RTLD_DEFAULT, "getpid");
    printf("getpid@%p\n", p); fflush(stdout);
    printf("first getpid() = %d\n", (int)getpid()); fflush(stdout);   /* ??????????? */
    unsigned int *w = (unsigned int *)p;
    long pg = (long)w & ~0xFFFL;
    if (mprotect((void *)pg, 0x2000, PROT_READ | PROT_WRITE | PROT_EXEC) != 0) { perror("mprotect"); return 2; }
    unsigned int old = *w;
    *w = 0x14000000u;   /* b . */
    __builtin___clear_cache((char *)w, (char *)w + 4);
    printf("patched getpid first insn %08x -> 14000000, calling again (should hang if re-translated)\n", old);
    fflush(stdout);
    int pid2 = (int)getpid();
    printf("SURVIVED second getpid() = %d  => patch NOT honored\n", pid2); fflush(stdout);
    return 0;
}
