# 冒险战争 (mxzz) 专用 R8 附加规则（随 --keep 传入 obfuscate_menu_dex.sh）
#
# 背景：HEAD(946f44f4) 的源码引用了不存在的 com.starry.magic.core.TapTapWebLogin。
# 我们用 04_scripts/冒险战争/menu_stub/ 的编译期 stub 让 javac 能过，但 stub **不进 jar**，
# 于是 R8 仍会看到「missing class」。R8 在 --lib 只给 android.jar 的情况下把它当错误。
# 这里只对它放行（不 keep、不生成实现）：mxzz 运行期不会走 wxsj/qxzxcz 的 GameHook 路径。
-dontwarn com.starry.magic.core.TapTapWebLogin
