-- 大表索引（CONCURRENTLY，不锁写；不可在事务块/启动流程执行）
-- 前提：device_id / created_at 两列均已存在（现网已有）
\set ON_ERROR_STOP on
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_auth_events_device_created
  ON auth_events (device_id, created_at);
SELECT indexname FROM pg_indexes WHERE tablename='auth_events' AND indexname='ix_auth_events_device_created';