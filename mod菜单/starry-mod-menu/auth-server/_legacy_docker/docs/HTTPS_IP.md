# 自签 HTTPS 部署（无域名）

当前证书绑定 IP：`47.100.216.101`  
对外访问：`https://47.100.216.101:8443`  
App 已内置同证书做 **Pinning**（`assets/certs/starlight.crt`）。

## 当前架构

- `api`：容器内 `8000`（不映射公网）
- `nginx`：公网 `8443` HTTPS，反代到 `api:8000`
- 证书：`certs/starlight.crt` + `starlight.key`


### 1. 上传这些目录/文件到服务器

确保服务器上有：

```text
auth-server/
  docker-compose.yml
  nginx/starlight.conf
  certs/starlight.crt
  certs/starlight.key    ← 私钥，必须上传，勿泄露
  ...
```

本机私钥在：`auth-server/certs/starlight.key`  
用宝塔上传到：`/www/wwwroot/starlight-auth/auth-server/certs/`

### 2. 重启容器

```bash
cd /www/wwwroot/starlight-auth/auth-server
docker compose down
docker compose up -d --build
docker compose ps
curl -k https://127.0.0.1:8443/health
```

应返回 `{"ok":true,...}`。

### 3. 防火墙

- 放行 **8443**
- **关闭/删除** 公网 **8000**（API 已不映射公网）

### 4. 管理台

浏览器打开（会提示证书不受信任，点继续访问即可）：

`https://47.100.216.101:8443/admin/`

菜单 App 不会弹这个提示，因为只信任内置证书。

## 换 IP 时

```bash
cd auth-server/certs
bash generate.sh 新IP
cp starlight.crt ../../app/src/main/assets/certs/
# 改 Preferences.DEFAULT_AUTH_BASE_URL
# 重新编译 App，并把新 crt/key 上传服务器
```

## 指纹（当前）

`SHA256 Fingerprint=C2:D8:11:6C:C2:98:6B:08:99:4E:4D:E4:6F:4D:9A:56:F8:C4:3A:EC:0E:A7:41:12:58:51:3C:83:72:EF:2E:E5`
