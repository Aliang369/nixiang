# 悬浮球 FAB — 定稿（V4 云 masc）

## 定稿资源

| 文件 | 说明 |
|------|------|
| `fab_anime_v4_cloud_mascot_256.png` | 抠图源（256×256 透明 PNG） |
| `fab_icon_final_256.png` | 装机 PNG（与上同源） |
| `fab_icon_q88.webp` | 旧 V4 云 masc，WebP 88×88（~17KB，md5 `47d4e305…`）——**已不在包内，留作回滚** |
| `fab_icon_logo_176.webp` | **当前在用**：`logo.png` 转 WebP 176×176（25,580B，md5 `cec9e131…`） |

## 进包

已内嵌 `MagicRes.FAB_ICON_WEBP_B64`（WebP → Base64，34,108 字符，2026-11-22 起为 `fab_icon_logo_176.webp`）。

回滚到旧云 masc：把 `fab_icon_q88.webp` 重新 Base64 写回 `MagicRes.java:48`。

重编后生效：

```bash
./gradlew clean assembleDebug
```

## 运行时

```
72dp 窗口 → ImageView 72dp → MagicRes.fabIcon()
纯静态 ImageView，无动画。
```
