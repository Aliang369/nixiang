from pathlib import Path
p = Path(r"E:\_build\lvmeng\orig_dec\AndroidManifest.xml")
t = p.read_text(encoding="utf-8")

perms = []
for perm in ("android.permission.SYSTEM_ALERT_WINDOW",
             "android.permission.FOREGROUND_SERVICE",
             "android.permission.FOREGROUND_SERVICE_SPECIAL_USE"):
    if perm not in t:
        perms.append('    <uses-permission android:name="%s"/>' % perm)
if perms:
    i = t.find("<uses-permission")
    if i < 0: i = t.find("<application")
    t = t[:i] + "\n".join(perms) + "\n" + t[i:]
print("+permissions:", len(perms))

if "com.starry.magic.menu.MenuService" not in t:
    svc = '''
        <service
            android:name="com.starry.magic.menu.MenuService"
            android:enabled="true"
            android:exported="false"
            android:foregroundServiceType="specialUse"
            android:stopWithTask="false">
            <property
                android:name="android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE"
                android:value="MOD menu overlay" />
        </service>
'''
    t = t.replace("</application>", svc + "\n    </application>", 1)
    print("+MenuService")

if "com.starry.magic.StarryInitProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.StarryInitProvider"
            android:authorities="com.zhuziplay.lvmengnew.op.starryinit"
            android:exported="false"
            android:initOrder="100" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("+StarryInitProvider")

if "com.starry.magic.lvmeng.LvmengPayloadProvider" not in t:
    prov = '''
        <provider
            android:name="com.starry.magic.lvmeng.LvmengPayloadProvider"
            android:authorities="com.zhuziplay.lvmengnew.op.lvmengpayload"
            android:exported="false"
            android:initOrder="101" />
'''
    t = t.replace("</application>", prov + "\n    </application>", 1)
    print("+LvmengPayloadProvider")

p.write_text(t, encoding="utf-8")
print("manifest patched")
