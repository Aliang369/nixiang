APP_ABI := arm64-v8a
APP_PLATFORM := android-21
APP_STL := c++_static
APP_CPPFLAGS := -std=c++17 -fexceptions -fpermissive
# 构建戳必须过滤成纯数字（否则中文提交信息进 -D 宏，clang++ 报找不到文件）；非 git 退化为固定戳。
STARRY_BUILD_EPOCH ?= $(shell git log -1 --format=%ct 2>/dev/null | tr -cd '0-9')
ifeq ($(strip $(STARRY_BUILD_EPOCH)),)
STARRY_BUILD_EPOCH := 1790184000
endif
APP_CPPFLAGS += -DSTARRY_BUILD_EPOCH=$(STARRY_BUILD_EPOCH)
