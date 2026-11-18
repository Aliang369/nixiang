
/* wxsj_dex_watch - read-only cold-start dex catcher. Polls /proc/<pid>/smaps and scans every
 * READABLE region it has not seen before for dex magics; dumps any header-consistent image.
 * usage: wxsj_dex_watch <pid> <outdir> <seconds> */
#define _GNU_SOURCE
#include <errno.h>
#include <fcntl.h>
#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <sys/stat.h>
#include <unistd.h>
#define CHUNK (1u<<20)
#define OVL 0x100
struct reg{uint64_t start,end;char perms[8];char path[256];uint64_t rss;int seen;};
static uint32_t r32(const unsigned char*p){return (uint32_t)p[0]|((uint32_t)p[1]<<8)|((uint32_t)p[2]<<16)|((uint32_t)p[3]<<24);}
#define MAXR 12000
static struct reg regs[MAXR]; static int nreg=0;
static int parse(pid_t pid){
  char p[64]; snprintf(p,sizeof p,"/proc/%d/smaps",(int)pid); FILE*f=fopen(p,"r"); if(!f) return -1;
  char line[1024]; struct reg cur; int have=0; static struct reg tmp[MAXR]; int nt=0;
  while(fgets(line,sizeof line,f)){
    uint64_t s,e; if(sscanf(line,"%" SCNx64 "-%" SCNx64 " %7s",&s,&e,cur.perms)==3){
      if(have&&nt<MAXR) tmp[nt++]=cur;
      cur.start=s;cur.end=e;cur.path[0]=0;cur.rss=0;have=1;
      char*q=line;int sp=0;for(;*q;q++){if(*q==' '){sp++;if(sp==5){q++;break;}}}
      if(q&&*q){size_t L=strcspn(q,"\n"); if(L>255)L=255; memcpy(cur.path,q,L); cur.path[L]=0;}
      continue;}
    if(have&&strncmp(line,"Rss:",4)==0) cur.rss=strtoull(line+4,NULL,10);
  }
  if(have&&nt<MAXR) tmp[nt++]=cur;
  fclose(f);
  /* merge with seen flags */
  for(int i=0;i<nt;i++){ tmp[i].seen=0; for(int j=0;j<nreg;j++) if(regs[j].start==tmp[i].start&&regs[j].end==tmp[i].end){tmp[i].seen=1;break;} }
  memcpy(regs,tmp,sizeof(struct reg)*nt); nreg=nt; return 0;
}
static int hdrok(const unsigned char*h,size_t avail,uint32_t*fsz){
  if(avail<0x70) return 0; uint32_t f=r32(h+0x20),hs=r32(h+0x24),et=r32(h+0x28),mo=r32(h+0x34),si=r32(h+0x38),so=r32(h+0x3c);
  if(hs!=0x70||(et!=0x12345678u&&et!=0x78563412u)) return 0;
  if(f<0x70u||f>512u*1024u*1024u) return 0;
  if(mo<0x70u||mo>=f||(mo&3)) return 0;
  if(si>(1u<<22)) return 0; if(si&&(so<0x70u||so>f)) return 0;
  *fsz=f; return 1;
}
int main(int argc,char**argv){
  if(argc<4){fprintf(stderr,"usage: %s <pid> <outdir> <sec>\n",argv[0]);return 2;}
  pid_t pid=atoi(argv[1]); const char*od=argv[2]; int secs=atoi(argv[3]); int skipinit=0; if(argc>4){ if(strcmp(argv[4],"skipinit")==0) skipinit=1; else if(strcmp(argv[4],"biganon")==0) skipinit=2; }
  mkdir(od,0755);
  char mp[64]; snprintf(mp,sizeof mp,"/proc/%d/mem",(int)pid);
  unsigned char*buf=malloc(CHUNK+OVL);
  struct timespec ts={0,50*1000*1000};
  time_t t0=time(NULL); int found=0;
  while(time(NULL)-t0<secs){
    if(parse(pid)!=0) break;
    if(skipinit==2){ for(int i=0;i<nreg;i++){ uint64_t len=regs[i].end-regs[i].start;
      int filebacked = (regs[i].path[0]=='/' && strncmp(regs[i].path,"/memfd:",7)!=0);
      if(filebacked || len < (1u<<20)) regs[i].seen=1; } skipinit=0; }
    if(skipinit==1){ for(int i=0;i<nreg;i++) regs[i].seen=1; skipinit=0; }
    int fd=open(mp,O_RDONLY); if(fd<0){ nanosleep(&ts,NULL); continue; }
    for(int i=0;i<nreg;i++){
      if(regs[i].seen||regs[i].perms[0]!='r'||regs[i].rss==0) continue;
      regs[i].seen=1;
      uint64_t pos=regs[i].start; size_t carry=0;
      while(pos<regs[i].end){
        size_t want=CHUNK; if(regs[i].end-pos<want) want=(size_t)(regs[i].end-pos);
        ssize_t got=pread(fd,buf+carry,want,(off_t)pos);
        if(got<=0){ pos+=want; carry=0; continue; }
        size_t total=carry+(size_t)got;
        for(size_t o=0;o+0x80<=total;o++){
          if(buf[o]!='d'||memcmp(buf+o,"dex\n",4)!=0) continue;
          if(!(buf[o+4]>='0'&&buf[o+6]<='9'&&buf[o+7]==0)) continue;
          uint32_t fsz=0; if(!hdrok(buf+o,total-o,&fsz)) continue;
          uint64_t addr=pos-carry+o;
          char op[400]; snprintf(op,sizeof op,"%s/dex_%ld_%" PRIx64 "_%u.dex",od,(long)(time(NULL)-t0),addr,fsz);
          unsigned char*img=malloc(fsz);
          if(img){ ssize_t r=pread(fd,img,fsz,(off_t)addr);
            if(r==(ssize_t)fsz){ FILE*f=fopen(op,"wb"); if(f){fwrite(img,1,fsz,f);fclose(f); printf("[CATCH] t=%ds addr=0x%" PRIx64 " fsz=%u %s path=%s\n",(int)(time(NULL)-t0),addr,fsz,op,regs[i].path[0]?regs[i].path:"(anon)"); found++;} }
            free(img); }
        }
        carry=OVL; memmove(buf,buf+total-carry,carry); pos+=(uint64_t)got;
      }
    }
    close(fd);
    nanosleep(&ts,NULL);
  }
  printf("[watch] pid=%d secs=%d found=%d\n",(int)pid,secs,found);
  return 0;
}
