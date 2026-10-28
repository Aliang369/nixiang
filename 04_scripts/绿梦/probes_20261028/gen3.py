import re
s = open(r"E:\_build\lvmeng\cagetest2.c", encoding="utf-8").read()
# 用简单办法：把三个 arch 分支的退出码各加一条（exit）
s = s.replace("BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 231, 15, 1),                           /* 3 */",
              "BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 231, 15, 0),                           /* 3  exit_group */\n    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 60, 14, 1),                            /* 3b exit */")
s = s.replace("BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 252, 11, 1),                           /* 7 */",
              "BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 252, 11, 0),                           /* 7  exit_group(i386) */\n    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 1, 10, 1),                             /* 7b exit(i386) */")
s = s.replace("BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 94, 3, 1),                             /* 15 */",
              "BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 94, 3, 0),                             /* 15 exit_group(arm64) */\n    BPF_JUMP(BPF_JMP | BPF_JEQ | BPF_K, 93, 2, 1),                             /* 15b exit(arm64) */")
open(r"E:\_build\lvmeng\cagetest3.c","w",encoding="utf-8").write(s)
print("written")
