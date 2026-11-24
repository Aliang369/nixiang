-- session_tokens 清理：默认只统计（dry-run），不删、不调度
-- 规则：仅「revoked 或 已过期」且 created_at 早于保留期；有效会话绝不动
\set ON_ERROR_STOP on
\if :{?retention_days}
\else
\set retention_days 90
\endif
SELECT
  count(*) FILTER (WHERE (revoked OR expires_at < now())
                     AND coalesce(last_seen_at, created_at) < now() - (:'retention_days' || ' days')::interval) AS deletable,
  count(*) FILTER (WHERE NOT revoked AND expires_at >= now()) AS live_keep,
  count(*) FILTER (WHERE (revoked OR expires_at < now())
                     AND coalesce(last_seen_at, created_at) >= now() - (:'retention_days' || ' days')::interval) AS within_retention_keep
FROM session_tokens;
-- 实际删除段（默认注释，确认后单独执行）
-- BEGIN;
-- DELETE FROM session_tokens
--  WHERE (revoked OR expires_at < now())
--    AND coalesce(last_seen_at, created_at) < now() - (:'retention_days' || ' days')::interval;
-- COMMIT;