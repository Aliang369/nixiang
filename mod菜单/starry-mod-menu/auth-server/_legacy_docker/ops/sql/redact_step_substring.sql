-- 子串脱敏单批：**只处理 card_id 已关联的残余**（a.card_id = ck.id 主键连接，每行只比一个 key）
-- 排除已脱敏/固定文案，避免重复处理
\set ON_ERROR_STOP on
BEGIN;
SET LOCAL statement_timeout = '120s';
WITH upd AS (
  UPDATE auth_events a
     SET detail = 'redacted'
    FROM card_keys ck
   WHERE a.id >= :lo AND a.id < :hi
     AND a.card_id = ck.id
     AND a.detail IS NOT NULL
     AND a.detail <> 'redacted'
     AND left(a.detail, 5) <> 'card#'
     AND position(ck.key_code in a.detail) > 0
  RETURNING a.id
)
SELECT count(*) AS affected FROM upd;
COMMIT;