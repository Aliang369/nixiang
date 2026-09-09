/* 王国大作战：前线 — HybridCLR + xLua 桥接 Trace */
#ifndef STARRY_WGDZZ_MOD_H
#define STARRY_WGDZZ_MOD_H

struct _JavaVM;
typedef _JavaVM JavaVM;

enum WgdzzFeat : int {
    WGDZZ_FEAT_TRACE = 1,
};

bool Wgdzz_TryInit();
bool Wgdzz_IsReady();
void Wgdzz_ForceClear();
void Wgdzz_Tick(const bool* features);
void Wgdzz_OnFeatureChanged(int featNum, bool enabled);
void Wgdzz_StartEarlyHooks(JavaVM* jvm);
void Wgdzz_StartMenuBootstrap(JavaVM* jvm);

#endif
