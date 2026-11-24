-- 清理注入 presence：**固定 id 清单**（来自已取证 CSV），事务内断言，不符即回滚
-- 前置：runner 已把 id 清单 COPY 到 db 容器 /tmp/purge_ids.csv 并建 temp 表
\set ON_ERROR_STOP on
BEGIN;
CREATE TEMP TABLE tmp_ids (id bigint) ON COMMIT DROP;
\copy tmp_ids FROM '/tmp/purge_ids.csv' CSV
DO $$
DECLARE n int; d int;
BEGIN
  SELECT count(*) INTO n FROM tmp_ids;
  IF n <> 357 THEN RAISE EXCEPTION 'precheck ids=% <> 357，中止', n; END IF;
  DELETE FROM online_presence p USING tmp_ids t WHERE p.id = t.id;
  GET DIAGNOSTICS d = ROW_COUNT;
  IF d <> 357 THEN RAISE EXCEPTION 'deleted=% <> 357，回滚', d; END IF;
END $$;
SELECT count(*) AS bhzf_total_after,
       count(*) FILTER (WHERE device_id ~ $re$['"();=<>,]$re$
                          OR device_id ~* $kw$(sleep|waitfor|select|union|benchmark|order by|pg_|generate_series)$kw$) AS bhzf_payload_after
  FROM online_presence WHERE game_id='bhzf';
COMMIT;