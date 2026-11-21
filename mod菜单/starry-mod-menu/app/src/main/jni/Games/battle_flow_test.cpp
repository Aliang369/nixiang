#include "旷野征途BattleFlow.h"
#include <cstdio>
int fails = 0;
void ck(const char* n, bool ok) { std::printf("%s %s\n", ok ? "PASS" : "FAIL", n); if (!ok) ++fails; }

int main() {
    using kb::RunFrameFlow;
    // 1) 双开关关：orig 一次，Init/work 不跑
    { bool t=false; int o=0,i=0,w=0;
      RunFrameFlow(false,false,t,[&]{return false;},[&]{++i;return true;},[&]{++w;},[&]{++o;return (void*)nullptr;});
      ck("双关：orig=1 init=0 work=0", o==1 && i==0 && w==0); }
    // 2) 开无敌但未 ready：Init 成功后 work 执行，orig 一次
    { bool t=false; int o=0,i=0,w=0;
      RunFrameFlow(true,false,t,[&]{return false;},[&]{++i;return true;},[&]{++w;},[&]{++o;return (void*)nullptr;});
      ck("未ready→Init成功→work，orig=1", o==1 && i==1 && w==1); }
    // 3) Init 失败：work 不跑，orig 一次，且 orig 看到 t 已复位 false
    { bool t=false; int o=0,i=0,w=0; bool seen=true;
      RunFrameFlow(true,false,t,[&]{return false;},[&]{++i;return false;},[&]{++w;},[&]{++o;seen=t;return (void*)nullptr;});
      ck("Init失败：work=0 orig=1 且 orig 时 t=false", o==1 && i==1 && w==0 && seen==false); }
    // 4) Init 内递归再入：内层走 B 直接 orig，不重复 work
    { bool t=false; int o=0,w=0;
      auto orig=[&]{++o;return (void*)nullptr;};
      auto work=[&]{++w;};
      auto init=[&]{ RunFrameFlow(true,false,t,[&]{return false;},[&]{return true;},work,orig); return true; };
      RunFrameFlow(true,false,t,[&]{return false;},init,work,orig);
      ck("Init内递归：外层work=1 内层不work，orig 共 2", w==1 && o==2); }
    // 5) 已 ready：Init 不调用，work 执行
    { bool t=false; int o=0,i=0,w=0;
      RunFrameFlow(false,true,t,[&]{return true;},[&]{++i;return true;},[&]{++w;},[&]{++o;return (void*)nullptr;});
      ck("ready：init=0 work=1 orig=1", o==1 && i==0 && w==1); }
    // 6) 外层 t 已 true（真再入）：直接 orig，work 不跑，orig 时 t 仍 true
    { bool t=true; int o=0,w=0; bool seen=false;
      RunFrameFlow(true,true,t,[&]{return true;},[&]{return true;},[&]{++w;},[&]{++o;seen=t;return (void*)nullptr;});
      ck("外层true再入：orig=1 work=0 且 t 保持 true", o==1 && w==0 && seen==true); }
    std::printf(fails ? "SUMMARY FAIL=%d\n" : "SUMMARY PASS=0\n", fails);
    return fails ? 1 : 0;
}
