
/* wxsj_memstr - read-only string-locator for a live Android process. Reports, per region,
 * how many times each pattern occurs, plus read failures. Not a dumper. */
#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#define CHUNK (2u<<20)
struct reg { uint64_t start,end; char perms[8]; char path[512]; uint64_t rss_kb; };
static int parse(pid_t pid, struct reg**out,size_t*cnt){
  char p[64]; snprintf(p,sizeof p,"/proc/%d/smaps",(int)pid); FILE*f=fopen(p,"r"); if(!f){perror(p);return -1;}
  size_t cap=8192,n=0; struct reg*r=calloc(cap,sizeof*r); char line[1024]; struct reg cur; int have=0;
  while(fgets(line,sizeof line,f)){
    uint64_t s,e;
    if(sscanf(line,"%" SCNx64 "-%" SCNx64 " %7s",&s,&e,cur.perms)==3){
      if(have&&n<cap) r[n++]=cur;
      cur.start=s;cur.end=e;cur.path[0]=0;cur.rss_kb=0;have=1;
      char*q=line;int sp=0;for(;*q;q++){if(*q==' '){sp++;if(sp==5){q++;break;}}}
      if(q&&*q){size_t L=strcspn(q,"\n"); if(L>511)L=511; memcpy(cur.path,q,L); cur.path[L]=0;}
      continue;
    }
    if(have&&strncmp(line,"Rss:",4)==0) cur.rss_kb=strtoull(line+4,NULL,10);
  }
  if(have&&n<cap) r[n++]=cur; fclose(f); *out=r; *cnt=n; return 0;
}
int main(int argc,char**argv){
  if(argc<3){fprintf(stderr,"usage: %s <pid> <outdir>\n",argv[0]);return 2;}
  pid_t pid=atoi(argv[1]);
  const char* pats[]={"unity3d","xingzuodaoshi","netease/nis","com.starry","cdex001","dex\n03"};
  int np=6;
  struct reg*rs=NULL; size_t n=0; if(parse(pid,&rs,&n)) return 1;
  char mp[64]; snprintf(mp,sizeof mp,"/proc/%d/mem",(int)pid); int fd=open(mp,O_RDONLY); if(fd<0){perror(mp);return 1;}
  unsigned char*buf=malloc(CHUNK+64); if(!buf) return 1;
  uint64_t scanned=0, failbytes=0, failregs=0;
  for(size_t i=0;i<n;i++){
    if(rs[i].perms[0]!='r'||rs[i].rss_kb==0) continue;
    uint64_t pos=rs[i].start; size_t carry=0; int cnt[8]={0}; uint64_t first[8]={0};
    while(pos<rs[i].end){
      size_t want=CHUNK; if(rs[i].end-pos<want) want=(size_t)(rs[i].end-pos);
      ssize_t got=pread(fd,buf+carry,want,(off_t)pos);
      if(got<=0){ failbytes+=want; pos+=want; carry=0; continue; }
      scanned+=got; size_t total=carry+(size_t)got;
      for(int k=0;k<np;k++){ size_t pl=strlen(pats[k]);
        for(size_t off=0; off+pl<=total; off++)
          if(buf[off]==(unsigned char)pats[k][0] && memcmp(buf+off,pats[k],pl)==0){ if(!cnt[k]) first[k]=pos-carry+off; cnt[k]++; }
      }
      carry=64; memmove(buf,buf+total-carry,carry); pos+=(uint64_t)got;
    }
    int any=0; for(int k=0;k<np;k++) if(cnt[k]) any=1;
    if(any){ printf("[R] 0x%" PRIx64 "-0x%" PRIx64 " %s rss=%" PRIu64 "K %s\n",rs[i].start,rs[i].end,rs[i].perms,rs[i].rss_kb,rs[i].path[0]?rs[i].path:"(anon)");
      for(int k=0;k<np;k++) if(cnt[k]) printf("    %-16s x%-6d first=0x%" PRIx64 "\n",pats[k],cnt[k],first[k]); }
  }
  printf("[str] scanned=%" PRIu64 "MB failregs=%" PRIu64 " failbytes=%" PRIu64 "MB\n",scanned>>20,failregs,failbytes>>20);
  return 0;
}
