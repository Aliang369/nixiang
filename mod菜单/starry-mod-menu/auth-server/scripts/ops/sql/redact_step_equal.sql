-- 等值脱敏单批（id 区间 [lo,hi)，key_code 唯一索引连接，单事务）
\set ON_ERROR_STOP on
BEGIN;
SET LOCAL statement_timeout = '120s';
WITH upd AS (
  UPDATE auth_events a
     SET detail = 'card#' || coalesce(a.card_id::text, 'unknown')
    FROM card_keys ck
   WHERE a.id >= :lo AND a.id < :hi
     AND ck.key_code = a.detail
  RETURNING a.id
)
SELECT count(*) AS affected FROM upd;
COMMIT;