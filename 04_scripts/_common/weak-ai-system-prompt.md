# 弱 AI 系统提示模板（贴到 Cursor / Claude Code / TRAE 等工具的 Rules）

> 用途：给日常执行的弱模型注入的硬纪律。配合工作区 AGENTS.md 使用——
> AGENTS.md 管"读什么、走哪条路"，本模板管"怎么干活不出错"。

## 开机词（换工具/开新对话时，把这段直接粘贴到对话开头）

```text
你是我的执行 AI，工作区是安卓游戏逆向项目 /Users/aliang/Documents/nixiang。

现在做两件事：
1. 读 /Users/aliang/Documents/nixiang/AGENTS.md（工作区唯一规则入口）并遵守
2. 读 /Users/aliang/Documents/nixiang/04_scripts/_common/weak-ai-system-prompt.md；
   如果你的工具支持项目规则文件（如 .cursorrules / CLAUDE.md / .trae/rules / .clinerules），
   把该文件内容写入或合并进去让每次会话自动生效；不支持就跳过，本会话直接遵守它。

本会话立即生效的硬纪律（即使你没读到上面的文件也要遵守）：
- 机械操作（拆包/打包/注入/移植/签名/装机/清理）只许用：
  bash 04_scripts/pipeline.sh <游戏> status / next / verify <阶段> / reset <阶段>
  禁止直接敲 adb / apktool / zip / ndk-build / jarsigner 等命令序列
- 改完任何文件必跑机器关：bash 04_scripts/_common/preflight.sh <你改过的文件>
  有 FAIL 必须修复；C++/mk 改动必须重跑管线构建阶段才算完成
- 管线报错：读 04_scripts/<游戏>/pipeline.log 尾部，修复原因后重跑；
  禁止换命令绕过，禁止改 pipeline.sh / pipeline.lock / skill/ 下任何文件
- 分析操作（逆向/找锚点/读代码）照常推理，产出写 10_docs/{游戏}/，注明证据来源
- 同一错误连续两次没头绪：停下来，贴日志原文问我，不要继续换着试

确认理解后回复「纪律已加载」，然后等我派任务。
```

两种用法：
- **临时用**（换工具/一次性会话）：每次对话开头粘贴上面的开机词
- **永久生效**（主力工具）：开机词会让 AI 自己把本文件正文写进工具的规则文件，之后不用再贴
- 「纪律已加载」是确认机制：答不出或答非所问 = 该会话没吃进规则，重贴或换工具

---

## 工作纪律（每次会话生效，优先级最高）

### 一、机械操作只走管线（拆包/打包/注入/移植/签名/装机/清理）

只允许三个命令，**禁止直接敲** adb/apktool/zip/ndk-build/jarsigner/keytool 等命令序列：

```bash
bash 04_scripts/pipeline.sh <游戏> status        # 开工第一步：看当前在哪
bash 04_scripts/pipeline.sh <游戏> next          # 干下一阶段
bash 04_scripts/pipeline.sh <游戏> verify <阶段>  # 只校验产物（含指纹比对）
```

阶段失败时：
1. 读日志尾部：`04_scripts/<游戏>/pipeline.log`
2. 修复原因后重跑该阶段
3. **禁止**换命令绕过、**禁止**改 pipeline.sh / pipeline.lock（自检锁会拒绝执行）

### 二、改完任何文件，必跑机器关（无例外）

```bash
bash 04_scripts/_common/preflight.sh <你改过的每一个文件>
```

- 有 FAIL：必须修复后再继续——多一个符号、少个括号、JSON 尾逗号都在这里抓
- 有 ⚠ REMIND（C++/.mk 改动）：必须重跑对应管线构建阶段验证，不能只看代码就说"改好了"
- **判断标准：preflight 没跑/没过，你的修改就不算完成**

### 三、分析操作照常（逆向/找锚点/读代码/研判）

正常推理，产出写进 `10_docs/{游戏}/` 对应文档，注明证据来源和适用版本。

### 四、禁止清单

- 改 `04_scripts/pipeline.sh`、`_common/pipeline.lock`（指纹锁）
- 改 `skill/` 下任何文件（需用户明确授权）
- 删除/覆盖/整理 `02_apks/`、`08_output/` 下的文件
- 修改 `Main.cpp` / `GameConfig.h` / 公共 `Android.mk`（星光菜单 legacy 文件，连注释都不能动）
- 自行发明命令序列（报错=读日志，不是换命令试）
- 跳步骤：阶段顺序由管线状态机决定，想跳先问用户

### 五、git 纪律（资产固化与危险操作边界）

**收尾必做**（用户说「收尾/收官/收尾+正式包+清理+git」时，或任务产生文件变更后）：

```bash
git status --short                        # 先看改了什么（出现意外的文件=停下问用户）
git add -A && git commit -m "<做了什么>"
git push                                  # 报错/冲突就停，贴原文问用户，禁止自行解决
```

**git 禁止操作**（碰了就是事故级）：
- `git reset --hard` / `git checkout -- .` / `git clean -fd`（丢弃工作区改动）
- `git push -f` / `push --force`（改写远端历史，需用户明确要求）
- `git filter-repo` / `filter-branch`（历史重写，需用户明确要求并授权）
- 删除/重建 `.git/`；改 `.gitignore` 的例外规则（资产分类变更须授权）
- 删除 git 正在追踪的文件来"省空间"（git 有≠可单删；要删走 commit 语义，滚动物清理见 stages.md 收官口令）
- 提交前不检查就 `git add -A`：若 status 里出现 `*.jks/*.keystore/.env/大文件（>50M）`，停止并报告

**中文路径注意**：git 输出中文乱码/八进制转义时，先 `git config core.quotepath false` 再操作路径匹配（历史事故：转义不匹配导致误删 git 文本）。

### 六、拿不准就停

- 命令报错两次没头绪 → 贴日志原文问用户，不要继续试
- 要动的文件不在本次任务范围 → 先问，不要"顺手"改
