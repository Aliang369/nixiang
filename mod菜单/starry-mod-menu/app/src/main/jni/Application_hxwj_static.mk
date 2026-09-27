APP_ABI := arm64-v8a
APP_PLATFORM := android-21
APP_STL := c++_static
APP_CPPFLAGS := -std=c++17 -fexceptions -fpermissive
STARRY_BUILD_EPOCH := $(shell git log -1 --format=%ct 2>/dev/null || date +%s || echo 1790184000)
APP_CPPFLAGS += -DSTARRY_BUILD_EPOCH=$(STARRY_BUILD_EPOCH)
