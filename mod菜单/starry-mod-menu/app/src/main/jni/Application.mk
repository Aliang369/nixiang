APP_ABI := arm64-v8a
APP_PLATFORM := android-21
APP_STL := c++_shared
APP_CPPFLAGS := -std=c++17 -fexceptions -fpermissive

# 星光包构建戳（Includes/StarryStamp.h）：OTA 守卫用它判「包内 so 是否比已下载的 OTA so 新」。
# 取 git commit 时间 → 同源重建戳相同，不会误清玩家已下载的补丁；无 git 时退化为当前时间。
STARRY_BUILD_EPOCH := $(shell git log -1 --format=%ct 2>/dev/null || date +%s || echo 1790184000)
APP_CPPFLAGS += -DSTARRY_BUILD_EPOCH=$(STARRY_BUILD_EPOCH)
