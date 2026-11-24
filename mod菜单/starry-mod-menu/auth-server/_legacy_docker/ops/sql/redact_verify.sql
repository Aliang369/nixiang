-- 复核（只读）：三项分别单列，不合并、不声称「其他全清」
\set ON_ERROR_STOP on
SELECT
  (SELECT count(*) FROM auth_events e JOIN card_keys ck ON ck.key_code = e.detail) AS remaining_exact,
  (SELECT count(*) FROM auth_events a JOIN card_keys ck ON a.card_id = ck.id
     WHERE a.detail IS NOT NULL AND a.detail <> 'redacted' AND left(a.detail,5) <> 'card#'
       AND position(ck.key_code in a.detail) > 0) AS remaining_contains_with_card,
  (SELECT count(*) FROM auth_events WHERE detail IS NOT NULL AND detail <> 'redacted'
       AND left(detail,5) <> 'card#' AND left(detail,7) <> 'cardkey') AS remaining_other;