APP_ABI := arm64-v8a
APP_PLATFORM := android-21
APP_STL := c++_shared
APP_CPPFLAGS := -std=c++17 -fexceptions -fpermissive

# 星光包构建戳（Includes/StarryStamp.h）：OTA 守卫用它判「包内 so 是否比已下载的 OTA so 新」。
# 取 git commit 时间 → 同源重建戳相同，不会误清玩家已下载的补丁。
# 必须把 git 输出过滤成纯数字：Windows/MSYS 下中文提交信息会连带进 -DSTARRY_BUILD_EPOCH，
# clang++ 随即报 "The system cannot find the file specified."；非 git 目录则退化为固定戳。
# 用 ?= 以便命令行/CI 直接覆盖。
STARRY_BUILD_EPOCH ?= $(shell git log -1 --format=%ct 2>/dev/null | tr -cd '0-9')
ifeq ($(strip $(STARRY_BUILD_EPOCH)),)
STARRY_BUILD_EPOCH := 1790184000
endif
APP_CPPFLAGS += -DSTARRY_BUILD_EPOCH=$(STARRY_BUILD_EPOCH)
