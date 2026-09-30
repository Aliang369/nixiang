// 勇士之心：退出探针（定位 5~13s 后进程 exit(0) 的调用者）
#pragma once
#include <jni.h>

// 装上 libc exit/_exit/_Exit/abort/kill/pthread_exit 与 JNI System.exit/Runtime.exit/
// Process.killProcess 的钩子，命中时打印 native 调用栈 + Java 栈。需传 JavaVM 以取 Java 栈。
void YsxzExitTrace_Start(JavaVM* vm);
