#pragma once
// Phigros 3.20.0 — 自动跳 v1（探测版）
enum : int {
    PHIGROS_FEAT_AUTOPLAY = 1,  // Toggle_自动跳
};
bool Phigros_IsReady();
bool Phigros_TryInit();
void Phigros_ForceClear();
void Phigros_SecurityTick();
void Phigros_OnFeatureChanged(int featNum, bool enabled);
void Phigros_Tick(const bool* features);
