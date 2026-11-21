# 无域名部署说明（仅 IP）

## 推荐：自签 HTTPS + 证书固定

见 [HTTPS_IP.md](HTTPS_IP.md)。  
对外：`https://47.100.216.101:8443`，App 已 Pinning。

## 旧方案：HTTP + IP（不推荐长期）

`http://IP:8000` 仅适合临时调试。上线请切 HTTPS。
