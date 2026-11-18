
/* wxsj_dex_scan2 - read-only multi-magic DEX/CDEX/VDEX scanner for a live Android process.
 * Reads /proc/<pid>/mem region by region (never ptrace, never write).
 * Magics: dex\n035..039 (any "dex\n"+3 digits), cdex001, vdex, dey\n036.
 * Aligned (4K page start) and unaligned byte-wise checks; header self-consistency for dex.
 * Build: aarch64-linux-android24-clang -O2 -o wxsj_dex_scan2 wxsj_dex_scan2.c
 * Run  : ./wxsj_dex_scan2 <pid> <outdir> [--all]
 */
#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/stat.h>
#include <unistd.h>

#define CHUNK (2u << 20)
#define OVL   0x100

static uint32_t rd32(const unsigned char *p){return (uint32_t)p[0]|((uint32_t)p[1]<<8)|((uint32_t)p[2]<<16)|((uint32_t)p[3]<<24);}
static uint16_t rd16(const unsigned char *p){return (uint16_t)((uint32_t)p[0]|((uint32_t)p[1]<<8));}

struct reg { uint64_t start,end; char perms[8]; char path[512]; uint64_t rss_kb; };

static int parse_smaps(pid_t pid, struct reg **out, size_t *cnt){
    char p[64]; snprintf(p,sizeof p,"/proc/%d/smaps",(int)pid);
    FILE *f=fopen(p,"r"); if(!f){fprintf(stderr,"open %s: %s\n",p,strerror(errno));return -1;}
    size_t cap=4096,n=0; struct reg *r=calloc(cap,sizeof *r); if(!r){fclose(f);return -1;}
    char line[1024]; struct reg cur; int have=0;
    while(fgets(line,sizeof line,f)){
        uint64_t s,e;
        if(sscanf(line,"%" SCNx64 "-%" SCNx64 " %7s",&s,&e,cur.perms)==3){
            if(have && n<cap){ r[n++]=cur; }
            cur.start=s;cur.end=e;cur.path[0]=0;cur.rss_kb=0;have=1;
            /* path follows 5 fields */
            char *q=line; int sp=0; for(;*q;q++){ if(*q==' '){ sp++; if(sp==5){ q++; break; } } }
            if(q && *q){ size_t L=strcspn(q,"\n"); if(L>511)L=511; memcpy(cur.path,q,L); cur.path[L]=0; }
            if(n+2>=cap){ cap*=2; struct reg*t=realloc(r,cap*sizeof *r); if(!t){free(r);fclose(f);return -1;} r=t; }
            continue;
        }
        if(have && strncmp(line,"Rss:",4)==0){ cur.rss_kb=strtoull(line+4,NULL,10); }
    }
    if(have && n<cap) r[n++]=cur;
    fclose(f); *out=r; *cnt=n; return 0;
}

static int hdr_ok(const unsigned char *h, size_t avail, uint32_t *fsz_out){
    if(avail<0x70) return 0;
    uint32_t fsz=rd32(h+0x20), hsz=rd32(h+0x24), etag=rd32(h+0x28);
    uint32_t sids=rd32(h+0x38), sidoff=rd32(h+0x3c), moff=rd32(h+0x34);
    if(hsz!=0x70) return 0;
    if(etag!=0x12345678u && etag!=0x78563412u) return 0;
    if(fsz<0x70u || fsz>512u*1024u*1024u) return 0;
    if(moff<0x70u || moff>=fsz || (moff&3)) return 0;
    if(sids> (1u<<22)) return 0;
    if(sids && (sidoff<0x70u || sidoff>fsz)) return 0;
    *fsz_out=fsz; return 1;
}

static void hexline(const unsigned char*p,int n,char*out){
    out[0]=0; for(int i=0;i<n;i++) sprintf(out+strlen(out),"%02x",p[i]);
}

int main(int argc,char**argv){
    if(argc<3){fprintf(stderr,"usage: %s <pid> <outdir> [--all]\n",argv[0]);return 2;}
    pid_t pid=(pid_t)atoi(argv[1]); const char*outdir=argv[2];
    int all = (argc>3 && strcmp(argv[3],"--all")==0);
    mkdir(outdir,0755);
    struct reg *rs=NULL; size_t n=0;
    if(parse_smaps(pid,&rs,&n)) return 1;
    char mp[64]; snprintf(mp,sizeof mp,"/proc/%d/mem",(int)pid);
    int fd=open(mp,O_RDONLY); if(fd<0){fprintf(stderr,"open %s: %s\n",mp,strerror(errno));return 1;}
    unsigned char *buf=malloc(CHUNK+OVL); if(!buf) return 1;
    uint64_t scanned=0; int hits=0, dexok=0;
    for(size_t i=0;i<n;i++){
        if(rs[i].perms[0]!='r') continue;
        if(!all && rs[i].rss_kb==0) continue;
        uint64_t len=rs[i].end-rs[i].start;
        if(len==0) continue;
        uint64_t pos=rs[i].start; size_t carry=0;
        while(pos<rs[i].end){
            size_t want=CHUNK; if(rs[i].end-pos<want) want=(size_t)(rs[i].end-pos);
            ssize_t got=pread(fd,buf+carry,want,(off_t)pos);
            if(got<=0){ pos+=want; carry=0; continue; }
            scanned+=got;
            size_t total=carry+(size_t)got;
            for(size_t off=0; off+8<=total; off++){
                const unsigned char*p=buf+off;
                int kind=0; /* 1=dex 2=cdex 3=vdex 4=dey */
                if(memcmp(p,"dex\n",4)==0 && p[4]>='0'&&p[4]<='9'&&p[5]>='0'&&p[5]<='9'&&p[6]>='0'&&p[6]<='9'&&p[7]==0) kind=1;
                else if(memcmp(p,"cdex001",7)==0) kind=2;
                else if(memcmp(p,"vdex",4)==0) kind=3;
                else if(memcmp(p,"dey\n036",7)==0) kind=4;
                if(!kind) continue;
                uint64_t addr=pos-carry+off;
                char hex[130]; hexline(p,64,hex); char mg[20]; for(int z=0;z<8;z++) sprintf(mg+z*2,"%02x",p[z]);
                uint32_t fsz=0; int ok=0;
                if(kind==1){ ok=hdr_ok(p,total-off,&fsz); if(ok)dexok++; }
                else if(kind==2){ fsz=rd32(p+0x20); }
                else { fsz=0; }
                printf("[HIT] kind=%s magic=%s addr=0x%" PRIx64 " reg=0x%" PRIx64 "-0x%" PRIx64 " %s rss=%" PRIu64 "K path=%s fsz=%u hdrok=%d align4k=%d\n  hex=%s\n",
                    kind==1?"dex":kind==2?"cdex":kind==3?"vdex":"dey",
                    mg, addr, rs[i].start, rs[i].end, rs[i].perms, rs[i].rss_kb,
                    rs[i].path[0]?rs[i].path:"(anon)", fsz, ok, (int)((addr&0xfff)==0), hex);
                hits++;
                if(ok){
                    char op[600]; snprintf(op,sizeof op,"%s/%s_%" PRIx64 "_%u.bin",outdir,kind==1?"dex":"cdex",addr,fsz);
                    FILE*o=fopen(op,"wb"); if(o){ unsigned char*img=malloc(fsz); if(img){ ssize_t r=pread(fd,img,fsz,(off_t)addr); if(r>0) fwrite(img,1,(size_t)r,o); free(img);} fclose(o); }
                }
            }
            carry = total<OVL? total: OVL;
            memmove(buf,buf+total-carry,carry);
            pos += (uint64_t)got;
        }
    }
    printf("[scan2] pid=%d regs=%zu scanned=%" PRIu64 "MB hits=%d dex_hdr_ok=%d\n",(int)pid,n,scanned>>20,hits,dexok);
    free(buf); close(fd); free(rs); return 0;
}
