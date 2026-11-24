-- admin_audit_logs 脱敏：仅 action='cards.delete' 且 detail 含 '卡密=' 的行
-- 保留 ID / actor / action / created_at / id；只把「卡密=<值>」替换为「卡密=redacted」
-- 不删除任何审计行
\set ON_ERROR_STOP on
BEGIN;
SET LOCAL statement_timeout = '60s';
SELECT count(*) AS will_update FROM admin_audit_logs
 WHERE action='cards.delete' AND position('卡密=' in coalesce(detail,'')) > 0;
UPDATE admin_audit_logs
   SET detail = regexp_replace(detail, '卡密=[^[:space:]]*', '卡密=redacted', 'g')
 WHERE action='cards.delete'
   AND position('卡密=' in coalesce(detail,'')) > 0
   AND detail NOT LIKE '%卡密=redacted%';
SELECT count(*) AS remaining_unmasked FROM admin_audit_logs
 WHERE action='cards.delete' AND position('卡密=' in coalesce(detail,'')) > 0
   AND detail NOT LIKE '%卡密=redacted%';
COMMIT;