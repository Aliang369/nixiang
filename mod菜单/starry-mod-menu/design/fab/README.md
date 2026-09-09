# 悬浮球 FAB — 定稿（V4 云 masc）

## 定稿资源

| 文件 | 说明 |
|------|------|
| `fab_anime_v4_cloud_mascot_256.png` | 抠图源（256×256 透明 PNG） |
| `fab_icon_final_256.png` | 装机 PNG（与上同源） |
| `fab_icon_q88.webp` | WebP q88（~17KB） |

## 进包

已内嵌 `MagicRes.FAB_ICON_WEBP_B64`（WebP → Base64）。

重编后生效：

```bash
./gradlew clean assembleDebug
```

## 运行时

```
72dp 窗口 → ImageView 72dp → MagicRes.fabIcon()
纯静态 ImageView，无动画。
```
