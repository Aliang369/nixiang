// 2026-11-21（用户指令「so 里的旧自签证书也要清理干净」）：
//   服务器已换发官方证书（auth.xgmfyx.cn，Let's Encrypt），客户端信任链改为
//   「系统 CA + 平台主机名校验」，旧 starlight 自签证书（CN=47.100.216.101）已下线并删除，
//   因此 so 内**不再烘焙任何证书内容** —— 本文件不再包含 PEM 字节。
//
//   为什么还留两个符号：MainModern.cpp 的 RegisterNatives 是**整表注册**，
//   删掉 nativeGetPinnedCert 一项会让同一批桥接方法一起注册失败（菜单/授权全挂），
//   所以方法壳保留、内容清空。mb_nativeGetPinnedCert 见 len == 0 直接返回 null。
//
//   历史来源：xxd -i ../assets/certs/starlight.crt > CertPinned.h（该 .crt 已于同日删除，
//   留档见 13_scratch/_retired_starlight_cert_20261121/）。
unsigned char starry_pinned_crt[] = { 0 };
unsigned int starry_pinned_crt_len = 0;
