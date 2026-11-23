-- 2026-11-23 游客统计迁移（MySQL 5.7 / 生产库 starlight_mod）
-- 执行位置：bt 154.94.232.231，站点目录 /www/wwwroot/starlight-mod-auth
-- 只加可空列，不改既有列；identity 由应用写入，历史行保持 NULL（不推测填充）。
-- 注意：MySQL 5.7 不支持 ADD COLUMN IF NOT EXISTS，重复执行会报 1060，属正常（列已存在）。
-- 应用启动时 ensure_schema() 也会幂等补列（查 information_schema），本文件供手工执行/审计。

ALTER TABLE online_presence
  ADD COLUMN identity VARCHAR(8) NULL COMMENT '身份快照：card=认证上报 / guest=未认证上报 / NULL=历史无证据',
  ADD COLUMN guest_seen_at DATETIME NULL COMMENT '最近一次游客身份上报时间';

ALTER TABLE auth_events
  ADD COLUMN identity VARCHAR(8) NULL COMMENT '身份快照：card / guest / NULL=历史无证据';

-- ---------------------------------------------------------------------------
-- 从零开始（lead 2026-11-23 决定：客户端还没上线，自测数据全部可清理）
-- 执行顺序：先备份，再清空；清空后看板从 0 起算。
--   mysqldump --single-transaction starlight_mod online_presence auth_events \
--     > /www/wwwroot/starlight-mod-auth/backups/presence_events_$(date +%Y%m%d_%H%M%S).sql
--   TRUNCATE TABLE online_presence;
--   TRUNCATE TABLE auth_events;
-- ---------------------------------------------------------------------------
