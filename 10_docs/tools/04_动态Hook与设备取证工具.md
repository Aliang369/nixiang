# 动态 Hook 与设备取证工具
> 来源：TOOLBOOK.md 原 §二、工具使用速查 · Frida / adb / scrcpy / mitmproxy / frida-dexdump（2026-10-27 拆分，正文逐字节保留）
> 上位：../../AGENTS.md §6/§7

### Frida — 动态 Hook（使用矩阵）

适用边界见 `WORKFLOW.md` §5（5.1 自注入 Trace 默认 / 5.2 Frida 备选）；授权、用户协作与清理见 `AGENTS.md`。参见 `01_tools/phantom-frida/README.md`、`.cursor/rules/frida-florida.mdc` 指针。

**使用矩阵**（2026-09-10 定稿；本地只留一个魔改 server = phantom-frida 17.16.4，官方版与 Florida 已移除——二进制均可随时重下，删除不省成本，留多版反而误用）：

| 游戏检测强度 | 用什么 | 判断方式 |
|------|------|------|
| 强（ACE/TSS/MHP/HTProtect/已知 frida 秒断） | 星光 so 自注入 Trace（唯一路线，见 WORKFLOW §5.1） | WORKFLOW §5 名单 + 历史记录 |
| 中/弱/无（其余全部） | phantom-frida 17.16.4 短时探针；attach 上=赚一轮，被杀=立即撤回走 so | 无保护特征 |

架构依据：so 路线的分析能力与交付功能同载体——用 so Trace 分析通的方案天然已过检测、可直接交付；frida 分析通了还要翻译回 so 再过一遍检测（等于过两次）。强检测游戏 frida 是「先偷懒后还债」。

**phantom-frida 实测结论**（2026-09-10，`grep -a -o` 静态特征计数，SHA256 与发布清单核对一致）：

| 检测特征 | 官方 17.16.4 | phantom 17.16.4 | 结论 |
|------|------|------|------|
| frida:rpc（协议 magic） | 20 | 0 | ✅ 全清 |
| frida-agent / frida-server / frida-gadget（名） | 9/6/2 | 0/0/0 | ✅ 全清 |
| gum-js-loop / gmain（线程名） | 2/4 | 0/0 | ✅ 全清 |
| frida_agent_main（符号） | 3 | 0 | ✅ 全清 |
| re.frida（仅剩 GLib 错误域名串） | 79 | 76 | ◐ 残 GLib 内部串，非路径/memfd 名 |
| gdbus（仅剩 GLib 内部串） | 42 | 38 | ◐ 同上 |
| 默认端口 | 27042 | 39571（随机构建） | ✅ 防默认端口扫描；**`-U` 使用时必须 `-l 127.0.0.1:27042` 覆盖**（见下） |

定性（静态实测 + 项目自述）：~90 补丁覆盖 16 类检测向量，周构建随机改名+随机端口+出包「禁止标记」门禁+真机冒烟验证，build-info.json 全程溯源。静态隐蔽性全面优于 Florida（Florida 留 frida:rpc 16 处/frida-agent 10 处未清）。仍是「症状级」对抗——中级（协议行为）与高级（inline hook 行为特征、ptrace 检测）不保证，强检测游戏一律走 so。

**实机验证 2026-09-10（四项 NOT PROVEN 全转 PROVEN，详见 `01_tools/phantom-frida/README.md`）**：真机 OPD2515/Android 16/KSU Enforcing，目标=魂坠深境 1.1.1 原版——①`-U` 直连 ②spawn+探针 61s 存活且 agent 可响应 ③objection 1.12.5 attach+列 activity ④frida-dexdump 43 dex 落盘，全部通过。**端口铁律**：phantom 出厂随机端口会让客户端 `-U` 静默降级为「jailed adb 兜底」——`frida-ps -U` 假阳性能列进程，spawn/attach 全灭（`need Gadget on jailed Android`）；server 必须以 `-l 127.0.0.1:27042` 启动（start_frida_server.sh 已固化为默认）；隐匿备选=随机端口+`adb forward`+`-H`。**MuMu 12 同日四项同口径全过**（aarch64 原生内核无 houdini，arm64 server 直接可用；多设备用 `-D`/objection `-S`，见 README）。

**魔改谱系落选记录**（选型时实测/核实）：strongR-frida-android（hluwa）仓库已 404 下架；zer0def/undetected-frida 构建流程有 bug——未打补丁的 server 被上传、anti-anti 处理产物未发布（实测线程名全在≈裸奔）；Florida（Ylarod）上游 17.16.x 起只发空 release（CI 停产二进制），且 17.9.1 须降级 pip 客户端，2026-09-10 已从 `01_tools/` 移除。gadget 版按需从 phantom 同 release 下载。

**客户端版本现状**（2026-09-10 实测）：两个 CLI（`$HOME/Library/Python/3.11/bin/frida`、`~/.local/bin/frida`）均为 **17.16.4**，与 phantom server **原生匹配，无需任何 pip 降级**（Florida 时代的降级流程作废）。

```bash
# 启动魔改 server（推到 /data/local/tmp/fs-arm64，phantom 17.16.4，端口随机构建）
04_scripts/_common/start_frida_server.sh

# 客户端版本核对（应输出 17.16.4）
export PATH="$HOME/Library/Python/3.11/bin:$PATH"
python3.11 -c "import frida; print(frida.__version__)"

frida-ps -U
frida -U -n "应用包名" -l 04_scripts/hook.js
frida -U -f "应用包名" -l 04_scripts/hook.js --no-pause

# objection -g "应用包名" explore
```

升级新档（代理示例，详见 `01_tools/phantom-frida/README.md`）：`https_proxy=http://127.0.0.1:7897 curl -sL -o /tmp/phantom-server.gz "https://github.com/TheQmaks/phantom-frida/releases/download/<tag>/ipcmodule-server-<ver>-android-arm64.gz"`，替换后重跑同口径扫描（关键字串应为 0）。

### adb — 设备控制

```bash
# 已授权且通过可测试门禁后覆盖安装；失败不自动卸载
adb install -r 08_output/应用名_版本_modified.apk

# 推送文件
adb push frida-server /data/local/tmp/

# 查看日志
adb logcat | grep -i "关键词"

# 截图（⚠️ 仅用户/人工执行：AI 禁止 `screencap` 猜坐标或判界面状态 —— AGENTS.md §4「真机交互零截图」）
adb exec-out screencap -p > 09_test/screenshot.png
```

### scrcpy — 投屏

```bash
# 基本投屏
scrcpy

# 只投屏不控制
scrcpy --no-control

# 录屏
scrcpy --record 09_test/demo.mp4
```

### mitmproxy — 抓包

```bash
# 启动代理
mitmproxy -p 8080

# Web 界面（更直观）
mitmweb -p 8080

# 手机端设置 WiFi 代理指向电脑 IP:8080
# 安装证书：手机浏览器访问 mitm.it
```

### frida-dexdump — 脱壳

```bash
# dump 当前运行应用的 DEX
frida-dexdump -U -n "应用包名"

# 指定输出目录
frida-dexdump -U -n "应用包名" -o 06_dumped/应用名_版本/
```
