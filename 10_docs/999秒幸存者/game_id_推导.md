# 999秒幸存者 — game_id 推导（2026-11-15）

全名 999秒!幸存者 → game_id `999mxcz`（数字保留，秒/幸/存/者 取首字母 mxcz；标点 `!` 不计）

- 依据：`skill/starry-mod/SKILL.md`「game_id 命名规则」（2026-08-27 起：官方中文全名拼音首字母，虚词计入，数字保留）
- 取全名来源：`aapt2 dump badging` 应用名 = `999秒!幸存者`（见 `设备与底包_20261115.md` §2）
- 同步四点：`{Entry}.cpp` 的 `authGameId` ＝ 后台 `games.game_id` ＝ 菜单 `template_id` ＝ 文档 JSON
- 注意（给 Lead/运营）：本 id **以数字开头**。规则要求「数字保留」⇒ 不可改写成 `mxcz999`；但 C++ 标识符不能以数字开头，
  故 native 侧 `Game`/tag 采用 `Mxcz999`、文件名 `Games/Mxcz999Mod.cpp`、`Android_mxcz999.mk`，
  **鉴权字符串仍严格为 `999mxcz`**（宏与 id 分离，见 SKILL.md「宏与 id 分离」；同类先例 `Lmq20fz`→`lmq20fz`）。
