-- result 列索引：**必须等迁移（ensure_schema 加列）上线并确认列存在后再建**
-- 前置检查：SELECT column_name FROM information_schema.columns
--           WHERE table_name='auth_events' AND column_name='result';  → 必须有 1 行
\set ON_ERROR_STOP on
DO $chk$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                  WHERE table_name='auth_events' AND column_name='result') THEN
    RAISE EXCEPTION 'auth_events.result 不存在：迁移未完成，禁止建索引';
  END IF;
END $chk$;
CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_auth_events_result_created
  ON auth_events (result, created_at);
SELECT indexname FROM pg_indexes WHERE tablename='auth_events' AND indexname='ix_auth_events_result_created';