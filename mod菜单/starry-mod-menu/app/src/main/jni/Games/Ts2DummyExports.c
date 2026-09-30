// Games/Ts2DummyExports.c —— 由 04_scripts/命运/gen_ts2_stuball.py --mode=mod 生成，请勿手改。
// 作用：星光 mod 库替身 libtersafe2.so 时，必须补齐真实 libtersafe2 的全部导出面，
// 否则 libtprt / libil2cpp 里按名字 dlsym 拿函数指针的代码会拿到 NULL 并调用 → 原生崩。
// 本文件只提供哑实现（不含 JNI_OnLoad / 不含 18 个 Java 桩 / 不含心跳日志——那些由框架与 AceStub.cpp 负责）。
#include <stdint.h>

__attribute__((visibility("default"), used))
long TssSDKIoctlDummyCounter = 0;

// 真实 libtersafe2 共 70 个 T 导出；这里补 67 个哑实现。
__attribute__((visibility("default"), used, noinline))
long GetTssExportFunc2(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssJavaMethod_SendCmd(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKDelReportData3(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKDelReportData4(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKDelReportData(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKForExport(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKFree(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKGetReportData2(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKGetReportData3(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKGetReportData4(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKGetReportData(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKInit(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKInitEx(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKOnPause(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKOnRecvData(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKOnRecvSignature(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKOnResume(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKRegistInfoListener(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKSetUserInfo(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKSetUserInfoWithLicense(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long _ZN3tp210gen_randomEv(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long _ZN6TssSdk10gen_randomEv(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long _ZN6TssSdk11gen_random2Ev(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long _ZN6TssSdk16sdt_report_errorEv(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_dec_tss_info(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_free_anti_data(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_getver(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_regist_tss_info_receiver(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_sdk_init(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_sdk_init_ex(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_sdk_ioctl(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_setgamestatus(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_setoptions(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_setuserinfo(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tp2_setuserinfowithlicense(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_del_report_data3(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_del_report_data4(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_del_report_data(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_enable_get_report_data(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_get_report_data2(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_get_report_data3(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_get_report_data4(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_get_report_data(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_jni_cmd(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_log_str(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_recv_sec_signature(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_dec_tss_info(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_decryptpacket(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_encryptpacket(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_gen_session_data(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_init(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_ioctl(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_ischeatpacket(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_rcv_anti_data(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_regist_tss_info_receiver(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_set_token(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_setgamestatus(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_setuserinfo(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_setuserinfo_ex(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_setuserinfo_with_license(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdk_wait_verify(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdt_double2uint64(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdt_float2uint(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdt_uint2float(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_sdt_uint642double(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_unity_is_enable(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long tss_unity_str(long a0, long a1, long a2, long a3, long a4, long a5, long a6, long a7) {
  (void)a0;(void)a1;(void)a2;(void)a3;(void)a4;(void)a5;(void)a6;(void)a7;
  return 0;
}

// libtprt 里以字符串 "TssSDKIoctl" / "TssSDKIoctlOld" dlsym 后调用；返回 0 = 「一切正常」
__attribute__((visibility("default"), used, noinline))
long TssSDKIoctl(long a0, long a1, long a2, long a3) {
  (void)a0; (void)a1; (void)a2; (void)a3;
  TssSDKIoctlDummyCounter++;
  return 0;
}
__attribute__((visibility("default"), used, noinline))
long TssSDKIoctlOld(long a0, long a1, long a2, long a3) {
  (void)a0; (void)a1; (void)a2; (void)a3;
  TssSDKIoctlDummyCounter++;
  return 0;
}
