# 基础设施与工作区历史

> 由 2026-09-20 `current.md` 索引拆分生成。后续只更新本文件；
> 原始完整历史见 `../history/current_before_split_20260920.md`。

## 2026-09-20 索引迁移记录

> 来源：`../history/current_before_split_20260920.md` 的原始行。

<!-- source-line:72 -->
> 更新：2026-09-12（晚），**全游戏收尾 + 工作区收官归档清理完成**：用户宣告**所有游戏收尾**（含城主别慌张/月圆之夜/弹力果冻/无限升级 OTA 维护期结束类）。按 TOOLBOOK「收官归档清单」执行全量清理：`03_decompiled` 清空、`06_dumped` 大二进制清空（git 文本 ~211M 留存=dump.cs/反编译/解密产物版本档案）、`07_patched` 仅留无限升级 134M（**不可再生**：热更历史 dll=OTA 复现链 + 设备装机备份）、`05_ida`/`09_test` 清空；`02_apks`/`08_output` 用户手动管理（各 ~2.0G）；`01_tools`/`04_scripts`/`10_docs` 常驻。工作区 22G→约 6.1G。**游戏重启路径**=clone→重取官包→管线重跑（check_apk→dump 重建 03/06→锚点从 git 文本/Mod 源码恢复，详见 TOOLBOOK「Git 资产分层」「收官归档清单」）。

<!-- source-line:110 -->
| [mod菜单框架](../../../mod菜单/starry-mod-menu/SWITCH_GAME.md) | starry-mod-menu | **✅ 20260909 注册表双轨制上线（阶段 0 收官）**：新游戏改走 `MainModern.cpp`（Games/ModEntry.h 注册表 + `new_game_mod.sh` 四件套生成 + 私有 `Android_<tag>.mk` + 独立 `obj_<tag>/`），禁止再碰 `Main.cpp`/`GameConfig.h`/公共 `Android.mk`；存量 20+ 游戏维持 legacy（`Main.cpp` #if 路径），重新出包时顺手迁移（写 Entry + 切 mk + 删 #if 分支组）。**验收**：①CWJH legacy 产物字节零漂移（md5 e7bf89d5…前后一致，含 mk 加注释复验）②SELF modern 壳构建过（桥接 19 项/全控件）③E2E 生成器→构建→注册表链接全通。**新实锤**：`OBFUSCATE` 用 `__LINE__` 做混淆种子，`Main.cpp` 加注释即产物漂移——legacy 主文件连注释都禁改；Main.cpp 堆积病史（Wxsj 残留混入 Endfield×2、CWJH 死分支、WHRHXKB 重复声明）已写入 Android.mk 头部告示。**同日盘点**：KittyMemory/Keystone 全仓零调用实锤（21 文件从未编译、Keystone=每个 so 43KB 死重，modern mk 已移除省 43KB；legacy 不动）；skill/starry-mod SKILL.md+surgical-build.md 已同步双轨（用户授权）；阶段 1 入口=下个新游戏实战（验收标准：MainModern.cpp/ModEntry.h 零修改） |

<!-- source-line:117 -->
| [Git 资产分层](../../../TOOLBOOK.md) | GitHub 私仓 | **✅ 20260909 Git 化上线（https://github.com/Aliang369/nixiang）**：工作区 19G→9.6G。**Git 资产**=skill/10_docs/04_scripts/mod菜单三套项目源码/规则文档/06_dumped 文本白名单（~490M，含 Dobby/Keystone 静态库=clone 即构建；最大单文件 32.7M<100M 限制）；**滚动物按活跃清理**（删：03 收官游戏 我的勇者5.2G+遗弃+雷霆、99_archive 渠道样品650M+mod快照212M、07 蜀汉传 ab1 187M、06 收官大二进制，保留：月圆之夜/超维进化 全部+雷霆 BSS 串表）；**敏感**=jks/keystore/.env 永不入库（add 前验证过零泄漏）；嵌套 git（starry-mod-menu/_111 各 1 个陈旧 init 提交）已删并入主仓。**教训**：中文路径操作前必须 `git config core.quotepath false`，否则 git ls-files 八进制转义致路径匹配失败误删 git 文本（已发生已恢复）。**复测恢复路径**：clone→管线 check_apk→dump→锚点从 git 文本/Mod 源码恢复→build_so→surgical→装机。规范见 TOOLBOOK「清理规则·Git 资产分层」 |

<!-- 20260921 星光后台 -->
| [星光后台 auth-server](../../../mod菜单/starry-mod-menu/auth-server/README.md) | FastAPI+Docker@宝塔 | **✅ 20260921 门户前端 + 后台 UI 重构上线（宝塔服务器 47.100.216.101:8443，ssh 别名 `starlight`）**：①新增 `/portal/` 玩家门户新增（游戏卡片/搜索/最新最热排序/多渠道下载弹窗/渠道标签，games 表新增 `download_url`(JSON≤3渠道)/`portal_visible`/`updated_at`/`channel` 字段，ensure_schema 双方言自动补列）②后台 UI 重构（紫色体系、CSS/JS 从 index.html 拆成 9 js+2 css、`?v=N` 版本号破缓存、全列表 20 条/页、星空增强=5 流星右上→左下+2 亮星组）③卡密批量删除（batch-delete 接口）/公告排序改编辑弹窗数字（旧 move/reorder 接口已删）④看板统计只计已登记游戏（过滤测试 game_id）+ 清理库内 19 条未登记测试记录⑤**UI 错位教训**：表格 td 禁止改 display（flex/block 脱离 table-cell 致行分隔线错位，踩坑两次才定位；规范见 auth-server/docs/ADMIN_UI_SPEC.md 表格铁律）⑥**部署方式**：整目录上传服务器后 `cd /www/wwwroot/starlight-auth/auth-server && docker compose up -d --build --force-recreate api`（db/nginx 不动）；本地 8765 调试服务已停 |

<!-- source-line:117 -->
| [机械操作管线](../../../04_scripts/_common/stages.md) | pipeline.sh | **✅ 20260909 骨架上线（SELF 试点验收通过）**：统一入口 `04_scripts/pipeline.sh <游戏> status/next/run/verify/reset` + 每游戏 `{游戏}/pipeline.json` 状态机（9 标准阶段：官包核对→dump→锚点→实现→构建→外科→签名→装机→收官），针对弱模型三大跑偏形态（即兴命令/Mac 命令报错试错、改错文件返工、改脚本参数）的对策=命令收进管线只填游戏+阶段、产物 md5 指纹检测（改错文件 verify 硬失败）、pipeline.sh sha256 自检锁（被改动拒绝执行）。**操作二分铁律**：机械操作只走管线入口，分析操作照常自由推理（产出落 10_docs）。**同日加 preflight 机器关**（`_common/preflight.sh`，改完文件必跑：sh→bash -n/py→compile/js→node --check/json→json.tool，cpp/mk→REMIND 须重跑构建阶段；管线阶段可声明 preflight 文件列表，执行命令前强制过检查 FAIL 即拦，实测坏脚本被拒）+ **（已退役）弱 AI 系统提示模板** `_common/weak-ai-system-prompt.md`（2026-10-15 删除；其有效规则已按唯一落点并入 `AGENTS.md` 与 `_common/stages.md`，详见 `../规范变更日志.md` 同批落地节）。113 个存量脚本=各阶段后端不重写；下个新游戏全流程实战接入；存量游戏出包时顺手接入。**同日覆盖审计补齐**：9→10 阶段（+device_ready 设备准备：MuMu connect/MT 端口转发）+「调试内循环」章节（最高频场景 reset build_so→next 连跑到 install；logcat/dropbox 观测=分析操作自由执行）+ WORKFLOW §4/§5 加管线优先指引（堵住手工流程旁路，含 KittyMemory 零调用修正）；运营侧（JSON 上传/game_id/OTA 发布）与设备残留清理**有意不入管线**（服务端写/高风险，归授权边界管）。验收证据：SELF 试点 clean+build_so 全通（构建+标签校验+指纹记录），防改脚本/篡改产物/preflight 拦截三项防呆实测有效。**（注：本行「弱模型」字样为 2026-09-09 上线当时口径；2026-10-15 起规则不再按模型档次分层，见 `../规范变更日志.md` 同批落地节）** |

<!-- source-line:121 -->
- [证书嵌入 so 经验](../../_templates/经验_证书嵌入so_20260906.md)：**已批准并合并**（2026-09-06）——证书固定改 so 烘焙（RegisterNatives 19/19），自测壳实测 200；已写入 SKILL.md 分发自检、surgical-build.md、auth-chain.md。跨样本 3 项 NOT PROVEN 随首个新游戏外科包回归，存量包/旧脚本不追溯。

<!-- source-line:125 -->
- 新实验使用 [逆向实验记录](../../_templates/逆向实验记录.md)；新经验使用 [经验候选卡](../../_templates/经验候选卡.md)，审批前保持候选状态。

<!-- source-line:129 -->
- [整理前 current 完整快照](../history/current_before_rules_optimization_20260905.md)：20260905 逐字节保留，含旧判断与相互冲突的阶段记载；仅作历史，不作现行指令。

<!-- source-line:131 -->
- 本次仅整理索引、游戏摘要及两个模板；未验证样本 hash、设备状态、产物可用性或根因，未新增绕过方案。下一步先选游戏、核对样本与授权，再记录最小实验。
