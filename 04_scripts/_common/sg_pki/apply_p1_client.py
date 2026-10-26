# -*- coding: utf-8 -*-
"""P1-2 客户端：优先使用 v2 非对称票（grant2），缺失时回落 v1（grant）。

设计取舍：**不做 v1 回退兜底**（v2 验不过就失败 = fail-closed）。
理由：v1 是对称 HMAC，正是我们要退役的东西；若客户端在 v2 失败时偷偷回落 v1，
加固等于没做，而且会把真 bug 藏起来。服务端仍向旧包发 v1，互不影响。
"""
import io
import os

ROOT = r"E:\nixiang"
JAVA = os.path.join(ROOT, "mod菜单", "starry-mod-menu", "app", "src", "main", "java",
                    "com", "starry", "magic")
PREFS = os.path.join(JAVA, "menu", "Preferences.java")
NMC = os.path.join(JAVA, "menu", "NativeMenuController.java")


def sub_once(s, old, new, label):
    n = s.count(old)
    if n != 1:
        raise SystemExit("[%s] 命中 %d 次（要求 1）" % (label, n))
    return s.replace(old, new, 1)


def patch_prefs():
    s = io.open(PREFS, "r", encoding="utf-8").read()
    anchor = "    public static String getAuthGrant() {"
    helper = '''    /** P1：优先 v2 非对称票（grant2），缺失时回落 v1（grant）。全端唯一入口。 */
    public static String pickGrant(org.json.JSONObject data) {
        if (data == null) return "";
        try {
            String g2 = data.optString("grant2", "");
            if (g2 != null && !g2.isEmpty()) return g2;
        } catch (Throwable ignored) {
        }
        return data.optString("grant", "");
    }

'''
    t = sub_once(s, anchor, helper + anchor, "Preferences.pickGrant")
    io.open(PREFS, "w", encoding="utf-8", newline="\n").write(t)
    print("patched Preferences.java +pickGrant")


def patch_nmc():
    s = io.open(NMC, "r", encoding="utf-8").read()
    old = 'data.optString("grant", "")'
    n = s.count(old)
    if n < 2:
        raise SystemExit("NativeMenuController: data.optString(\"grant\") 只命中 %d 次" % n)
    t = s.replace(old, "Preferences.pickGrant(data)")
    print("  NativeMenuController: %d 处 data.grant → Preferences.pickGrant(data)" % n)

    t = sub_once(
        t,
        'grant = bootData.optString("grant", bootData.optString("feature_grant", ""));',
        'grant = Preferences.pickGrant(bootData);\n'
        '                        if (grant.isEmpty()) grant = bootData.optString("feature_grant", "");',
        "bootstrap grant")
    t = sub_once(
        t,
        'grant = gData.optString("grant", gData.optString("token", ""));',
        'grant = Preferences.pickGrant(gData);\n'
        '                        if (grant.isEmpty()) grant = gData.optString("token", "");',
        "grant endpoint")

    io.open(NMC, "w", encoding="utf-8", newline="\n").write(t)
    print("patched NativeMenuController.java")


if __name__ == "__main__":
    patch_prefs()
    patch_nmc()
    print("done")
