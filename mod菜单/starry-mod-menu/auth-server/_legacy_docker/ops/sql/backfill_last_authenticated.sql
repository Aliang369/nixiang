-- 历史回填 online_presence.last_authenticated_at（durable 认证时间）
-- 来源（只认真实证据，不用 last_seen / bootstrap / login_fail）：
--   A) 现有 online_presence.card_verified_at（未来值不采纳）
--   B) auth_events.event_type='login_ok' 按 (game_id, device_id) 取 max(created_at)
-- 合并：三源 GREATEST(当前值, A, B)，NULL 用 -infinity 哨兵；只在更晚时更新（不倒退）
-- 过滤：仅已登记 game_id + 合法 device 形态；只保留 future guard（created_at <= now()）
-- 幂等：可重复执行；第二次不改任何行
\set ON_ERROR_STOP on

DO $$ BEGIN
  IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                  WHERE table_name='online_presence' AND column_name='last_authenticated_at') THEN
    RAISE EXCEPTION '缺少 online_presence.last_authenticated_at，先完成 schema 迁移';
  END IF;
END $$;

BEGIN;
SET LOCAL statement_timeout = '120s';

SELECT count(*) AS candidates
FROM online_presence p
LEFT JOIN (SELECT game_id, device_id, max(created_at) AS max_ts FROM auth_events
           WHERE event_type='login_ok' GROUP BY 1,2) m
  ON m.game_id=p.game_id AND m.device_id=p.device_id
WHERE p.game_id IN (SELECT game_id FROM games)
  AND (p.device_id ~ '^dev_[0-9a-f]{32}$' OR p.device_id ~ '^dev_[0-9a-f]{40}$')
  AND (p.card_verified_at IS NOT NULL OR m.max_ts IS NOT NULL);

SELECT min(created_at) AS earliest_evidence, max(created_at) AS latest_evidence, now() AS now_utc
FROM auth_events WHERE event_type='login_ok';

WITH m AS (
  SELECT game_id, device_id, max(created_at) AS max_ts
  FROM auth_events
  WHERE event_type = 'login_ok' AND created_at <= now()
  GROUP BY 1,2
), src AS (
  SELECT p.id,
         GREATEST(
           COALESCE(p.last_authenticated_at, '-infinity'::timestamptz),
           COALESCE(CASE WHEN p.card_verified_at <= now() THEN p.card_verified_at END, '-infinity'::timestamptz),
           COALESCE(m.max_ts, '-infinity'::timestamptz)
         ) AS best,
         COALESCE(p.last_authenticated_at, '-infinity'::timestamptz) AS cur
  FROM online_presence p
  LEFT JOIN m ON m.game_id = p.game_id AND m.device_id = p.device_id
  WHERE p.game_id IN (SELECT game_id FROM games)
    AND (p.device_id ~ '^dev_[0-9a-f]{32}$' OR p.device_id ~ '^dev_[0-9a-f]{40}$')
)
UPDATE online_presence p
   SET last_authenticated_at = src.best
  FROM src
 WHERE p.id = src.id
   AND src.best > src.cur;

DO $$
DECLARE total bigint; withauth bigint; fut bigint;
BEGIN
  SELECT count(*),
         count(*) FILTER (WHERE last_authenticated_at IS NOT NULL),
         count(*) FILTER (WHERE last_authenticated_at > now())
    INTO total, withauth, fut
    FROM online_presence;
  RAISE NOTICE 'post: total=% with_auth=% unknown=% future=%', total, withauth, total-withauth, fut;
  IF fut <> 0 THEN  -- clock_timestamp() 为真实当前时刻，仅拦真未来
    RAISE EXCEPTION '存在 % 行未来时间(>+5s)，回滚', fut;
  END IF;
  IF withauth > total THEN
    RAISE EXCEPTION 'with_auth(%) > total(%)，异常回滚', withauth, total;
  END IF;
END $$;

COMMIT;
