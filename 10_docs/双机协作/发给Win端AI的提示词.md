# 发给 Win 端 AI 的交接提示词

> 用法：Win 机器上打开你的 AI 工具（Claude Code / Cursor / Codex / Cline 等），**整段复制下面框内的内容**作为第一条消息发出。
> 提示词是自包含的：Win 端 AI 看不到 Mac 这边的对话，所有它需要的前提都写在里面。
> 路径已按你的 Win 工作区目录固定为 `E:\nixiang`（Git Bash 中为 `/e/nixiang`）。若你后来换了位置，先替换提示词里全部路径再发。

````text
# 任务：把本机（Windows）接入 nixiang 安卓逆向工作区，加入 Mac + Win 双机协作

你正在一台 **Windows** 机器上工作。另一台是 macOS，两台共用同一个 GitHub 仓库
`git@github.com:Aliang369/nixiang.git`（分支 main）。你的任务是：把本机从"只有一个空文件夹"
变成"能正常 pull / 跑脚本 / 提交"的协作状态，并回报实测结果。

## 一、你的机器身份（后面所有命令都以此为准）

| 项 | 值 |
|---|---|
| 主机角色 | WINDOWS 端（另一台记为 mac 端） |
| 仓库根（真实位置） | `E:\nixiang` |
| 仓库根（Git Bash 视角） | `/e/nixiang` |
| 兼容路径（Git Bash 里要能进） | `/Users/aliang/Documents/nixiang` |
| **唯一合法的脚本执行环境** | **Git Bash**（`C:\Program Files\Git\bin\bash.exe`） |
| 系统级操作使用 | PowerShell（仅限初始化/装软件/改注册表） |

## 二、终端红线（违反会导致"看起来失败但原因完全不对"）

1. **绝对不要用 PowerShell 或 cmd 执行 `.sh` 脚本。** 一律：`bash 04_scripts/xxx.sh`，在 Git Bash 里跑。
2. 仓库里所有 `.sh` 写死了 mac 绝对路径 `/Users/aliang/Documents/nixiang`（共 8525 处，
   分布在 364 个脚本里）。**本机会通过一个目录联接（junction）让这个字面量路径在本机也能用**，
   所以你在 Git Bash 里可以原样照抄文档中的 mac 命令 —— 但**不要修改任何现有脚本里的这些路径**。
3. Git Bash 原生**没有 `md5`**（只有 `md5sum` / `sha256sum`），它的 `stat` 也是 GNU 版。
   而仓库是老 mac 脚本：**512 处 `md5 -q`、16 处 `stat -f %z`、1 处 `stat -f %Sm`、234 处 `shasum -a`**。
   步骤 2 会把 `md5` / `stat` / `shasum` 垫成 `~/.bashrc` 里的同名函数并 `export -f`（子 bash 也继承），
   所以 `md5 -q <file>` 在本机 **Git Bash 里能正常用** —— 但**只在 Git Bash 里**，PowerShell 里没有。
   **你绝对不要去改仓库里这 2166 处调用**（理由见第五节第 1 条）。
4. 本机 Git Bash 是 bash 5.x，**但 mac 端是 bash 3.2**。你写的 `.sh` 一旦用了
   `mapfile` / `readarray` / `declare -A` / `local -n` / `${v^^}` / `;;&`，在 mac 上会整套跑不起来。
   提交前必须用 `bash 04_scripts/_common/preflight.sh <你改的文件>` 自检（它带 bash 3.2 门禁）。

## 三、执行步骤（按顺序，每步做完报一次结果）

### 步骤 1：拿到仓库
在【PowerShell 管理员】里：
    git clone git@github.com:Aliang369/nixiang.git E:\nixiang

如果 clone 卡住或报权限错误：本机还没有 GitHub SSH key。执行
`ssh-keygen -t ed25519 -C "win-nixiang"`，把 `C:\Users\<你>\.ssh\id_ed25519.pub` 的内容
交给用户去 GitHub 添加，然后等用户确认再加；**不要自己改成 HTTPS 或删除仓库重来**。
若 `E:` 盘不可用、或它不是本机固定硬盘（U 盘 / 移动硬盘 / 网络映射盘都不行，会影响 git 与 NDK 速度并
让 junction 失效），先停下来问用户，不要自行换路径。

### 步骤 2：跑初始化脚本（幂等，可重复执行）
在【PowerShell 管理员】里：
    powershell -ExecutionPolicy Bypass -File "E:\nixiang\10_docs\双机协作\win_bootstrap.ps1" -RepoRoot E:\nixiang

它会：装 Git/Python3.11/JDK17/platform-tools/7-Zip/jq/Syncthing → 开长路径支持 →
配 git 全局项 → clone/更新 → 写 Git Bash 的 `~/.bashrc`（含 `md5`/`stat`/`shasum` 三个 mac 命令垫片）→
建兼容用 junction → 建本地忽略目录 → 自检。
**把脚本每一步的 OK/WARN/FAIL 原样抄回给我，不要摘要、不要美化。**
若第 6 步（junction）FAIL：报告 `$gitRoot` 实际值和你是否有管理员权限，然后停下等我决定，
不要改用别的路径或自己写替换脚本。

### 步骤 3：读工作区规则（按这个顺序，只读必要部分）
1. `AGENTS.md`（全仓唯一政策入口，必须完整读）
2. `CLAUDE.md`
3. `10_docs/双机协作/双机协作同步方案.md`（双机协作规则，重点读 §2 §3 §4 §6 §7）
4. `10_docs/_state/current.md`（**只用 `rg -n -m 40` 看摘要索引，不要整读**）

读取纪律（规则里有，先照做）：
- 禁止为了"了解全仓"通读所有游戏摘要、完整 logcat、`06_dumped` dump。
- 禁止 `cat 04_scripts/{游戏}/pipeline.json`（会灌入整个历史阶段），只取目标 stage。
- 禁止 `cat 06_dumped/**/dump.cs`、`meta/dump_all.txt` 等 MB 级文件，一律 `rg -n -m N` 切片。
- `git status` / `git diff` 必须带游戏 pathspec，禁止裸跑。

### 步骤 4：跑验收清单（Git Bash 里，逐条贴原始输出）
    cd /Users/aliang/Documents/nixiang && pwd
    git rev-parse --short HEAD && git status -sb
    git ls-files --eol | grep -E 'i/(crlf|mixed)'
    bash 04_scripts/_common/preflight.sh 04_scripts/_common/env.sh
    bash -c 'source 04_scripts/_common/env.sh; echo "$NIXIANG_HOST|$NIXIANG_PYTHON|$NIXIANG_BUILD_TOOLS|$NIXIANG_ZIPALIGN|$NIXIANG_APKSIGNER|$NIXIANG_JAVA17"'
    f=/tmp/.nx_probe; printf x > $f
    md5 -q $f; stat -f %z $f; shasum -a 256 $f; bash -c "md5 -q $f"; rm -f $f
    python3 --version
    adb devices

期望：① pwd 输出 `/Users/aliang/Documents/nixiang`（不是 No such file）② HEAD 与 mac 端一致
（2026-10-01 交付时 Mac 端基线与跟踪文件数见文末「Mac 端基线值」；HEAD 会随新提交变化，以实时 `git rev-parse` 为准）、
工作区干净无 ahead/behind ③ 第 3 条**最多只有 15 行**（都是 `06_dumped/**/apk_assets`、
`06_dumped/**/il2cpp_dump_*/**`、`10_docs/掌门下山/lua_dump/**` 里的 dump 原始字节，
`.gitattributes` 已豁免；若多出别的路径，停下并原文报告，**不要自己改换行**）
④ preflight PASS ⑤ 六项全部非空、首项为 `win`（`NIXIANG_JAVA17` 也必须非空；若为空，报告
`JAVA_HOME` 与 `/c/Program Files/Microsoft/jdk-17*` 是否存在）⑥ 垫片组：`md5 -q` 输出
**`9dd4e461268c8034f5c8564e155c67a6`**（这是 `md5("x")`，Mac 端已对照验证过），`stat -f %z` 输出 `1`，
`shasum -a 256` 打印 sha256，**最后那条 `bash -c "md5 -q $f"` 也必须是同一串 32 位 hash**
（证明 `export -f` 对非交互子 shell 生效；如果这条为空而前面有值，说明垫片没继承，停下报告）
⑦ python 可用 ⑧ 若本机不管设备，`adb devices` 显示空列表也算通过。
**任何一条不符：原文报告，不要"修复"后只报结果。**

### 步骤 5：配置本机设备归属（若本机要接手机）
    export ANDROID_SERIAL=<你的设备序列号>     # 追加到 ~/.bashrc
一台手机同时只由一台电脑做 adb / frida 操作。frida-server 由本机自己推自己那份，
不要用 mac 端的版本。**不要**清数据、不要卸载、不要覆盖安装 —— 这些需要用户当轮点名授权。

### 步骤 6：回报

## 四、硬约束（来自 AGENTS.md，你必须遵守）

- **不扩张功能边界**：只做用户当轮点名的功能与菜单项，"顺手/顺便"新增一律禁止。
- **真机交互零截图**：禁止 `screencap` 猜坐标、禁止 `input tap`；界面点按/输入全部由用户完成。
- **破坏性操作需授权**：清数据、卸载、不可逆覆盖、影响其他游戏/设备、上传/分发、服务端写操作，
  一律先说明范围+影响+恢复方案并取得确认。
- **不做数据备份**（未当轮明确要求时）；**不默认卸载/清数据**。
- **`skill/` 目录下任何新增/修改/删除都要用户明确授权。**
- **`02_apks/` 与 `08_output/` 由用户手工管理**，AI 不默认删除、整理、覆盖；也不要靠扫目录猜有没有底包。
- **APK 不留本地**，构建产物/中间件收尾要清理（详见 AGENTS.md「技术与清理入口」）。
- 需要用户操作时按「操作请求卡」输出三项：① 请你做什么 ② 成功标志 ③ 我接着看哪条日志/文件。
- 你**不要**自行推断用户是否已授权；不确定就问一句，但不要连续追问。

## 五、你不被允许做的事（本轮明确禁止）

1. 批量替换仓库里 8525 处 `/Users/aliang/Documents/nixiang` 字面量，或批量改写 2166 处 `md5`/`stat -f`
   （前者靠 junction 垫片、后者靠 `.bashrc` 函数垫片已解决；改动会让 mac 端每条提交都要重新对齐，
   而且 `_state` 里存的是已算好的 md5 值，换算法会让历史 hash 失去可比性）。
2. 修改 `04_scripts/_common/env.sh`（Mac 端 2026-10-01 已应用双机补丁，你只需 `source` 使用它；
   如果发现它在 Win 上解析不对，**报告 + 给最小改动建议**，等用户授权后再动）。
3. 修改 `skill/` 下任何文件。
4. 删除或"整理" `02_apks/`、`08_output/`、`01_tools/`。
5. 用 WSL 替代 Git Bash 作为默认环境（除非某个游戏明确需要 Linux 工具链，且先问过用户）。
6. 把 `.git/`、`01_tools/`、`05_ida/` 放进任何文件同步器。
7. 对远端 `push --force`。

## 六、回报格式（严格按此输出，不要写散文）

### 1. 环境事实
- 主机名 / Windows 版本 / Git Bash 路径 / bash 版本（`bash --version | head -1`）
- 仓库真实路径 / 兼容路径是否生效
- 与 mac 端 HEAD 是否一致（贴 `git rev-parse --short HEAD`）

### 2. 已执行命令与原始输出
逐条列出，输出原样粘贴（失败的命令也要贴，包括完整报错）

### 3. CLAIM / VERDICT / EVIDENCE 表
| 断言 | 判定(PROVEN/NOT PROVEN) | 证据（命令或文件行号） |
|---|---|---|
| 兼容路径垫片生效 | | |
| 与 mac 端同 HEAD、工作区干净 | | |
| 全仓换行符为 LF | | |
| preflight 通过 | | |
| env.sh 六项解析非空（含 JAVA17） | | |
| `md5 -q` 垫片输出正确（32 位 hash） | | |
| 垫片能被非交互子 bash 继承（`bash -c`） | | |

### 4. 已知未解决 / 阻塞项
各写明：现象、已试方法、需要的授权或用户动作

### 5. 下一步建议
最多 3 条，按优先级排序；不要自行开工
````

---

## Mac 端配套（你自己在本机做的事）

Win 端 AI 卡在步骤 1、2 时通常需要你出手（加 SSH key、提权、确认路径）。同时你在 Mac 端保持：

```bash
# 【Mac · zsh】开工
cd /Users/aliang/Documents/nixiang && git status -sb && git pull --rebase --autostash

# 【Mac · zsh】Win 端提交后确认拿到
git log --oneline -5
```

**双机首轮对接的顺序建议**：Mac 先 `git push` 一次干净的 main → Win 端 clone + bootstrap → Win 端回报验收清单 → 你确认 Win 能 `pull` 到 Mac 的新提交 → 再开始分游戏（`双机协作同步方案.md` §2.1）。

## Mac 端基线值（2026-10-01 实测，Win 端用来对照验收）

| 项 | Mac 端实测值 | Win 端应为 |
|---|---|---|
| `git rev-parse --short HEAD` | `507183a1`（会随新提交变化，以实时为准） | **完全相同** |
| 跟踪文件数 `git ls-files \| wc -l` | **2319** | **完全相同** |
| `git ls-files --eol \| grep -cE 'i/(crlf\|mixed)'` | **15** | **完全相同**（且必须都是 `attr/-text`，即 dump 原始资产） |
| `preflight.sh` | `PASS=1 FAIL=0 REMIND=0 SKIP=0`，退出码 0 | **相同** |
| env.sh 六项 | `mac\|/opt/homebrew/bin/python3.11\|.../build-tools/36.0.0\|.../zipalign\|.../apksigner\|（空）` | 首项为 **`win`**，**六项全部非空**（含 JAVA17，这是唯一与 Mac 不同的地方） |
| `printf x > f; md5 -q f` | `9dd4e461268c8034f5c8564e155c67a6` | **完全相同的 32 位 hash** |
| `stat -f %z f` | `1` | `1` |
| `shasum -a 256 f` | `2d711642b726b044...` | 同一个 sha256 |

> Mac 端换行符那 15 行不是问题，是**故意的**：它们是 dump/反编译出来的原始字节，
> `.gitattributes` 用 `-text -eol` 明确豁免（共 4 条规则）。Win 端只要数量一致、路径一致就算过。
