//
// SecStrings.hpp — 敏感字符串集中管理（2026-09-06，借鉴 LGL OBFUSCATE）
//
// 原则（装机自检不受影响）：
//   - 混淆：目标库名、鉴权密钥、JNI 桥接类名等敏感字面量；
//   - 明文保留：各游戏标签（[Gjc2] 等 LOGI 锚点，装机自检靠它）、功能列表文本。
//
// 用法：static 局部变量只解密一次（C++17 magic statics，线程安全），
//       返回稳定 c_str 指针——不要在热循环里直接写 OBFUSCATE()（每次调用都解密）。
//
#pragma once

#include <string>

#include "obfuscate.h"

inline const char* sec_lib_il2cpp() {
    static const std::string s = OBFUSCATE("libil2cpp.so");
    return s.c_str();
}
inline const char* sec_lib_cocos() {
    static const std::string s = OBFUSCATE("libcocos.so");
    return s.c_str();
}
inline const char* sec_lib_cocosjs() {
    static const std::string s = OBFUSCATE("libcocos2djs.so");
    return s.c_str();
}
inline const char* sec_lib_mygame() {
    static const std::string s = OBFUSCATE("libMyGame.so");
    return s.c_str();
}
inline const char* sec_lib_cocos2dcpp() {
    static const std::string s = OBFUSCATE("libcocos2dcpp.so");
    return s.c_str();
}
