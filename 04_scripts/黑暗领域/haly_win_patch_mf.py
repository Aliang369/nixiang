
import sys
from pathlib import Path
p = Path(sys.argv[1])
t = p.read_text(encoding='utf-8')
perms = []
if 'SYSTEM_ALERT_WINDOW' not in t:
    perms.append('    <uses-permission android:name="android.permission.SYSTEM_ALERT_WINDOW"/>')
if 'name="android.permission.FOREGROUND_SERVICE"' not in t and 'android.permission.FOREGROUND_SERVICE/' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE"/>')
if 'FOREGROUND_SERVICE_SPECIAL_USE' not in t:
    perms.append('    <uses-permission android:name="android.permission.FOREGROUND_SERVICE_SPECIAL_USE"/>')
if perms:
    i = t.find('<uses-permission')
    t = t[:i] + '\n'.join(perms) + '\n' + t[i:]
    print('perms added:', len(perms))
print('has_application_name=', 'android:name="com.starry.magic.StarryApp"' in t)
if '<application' in t and 'android:name="com.starry.magic.StarryApp"' not in t:
    t = t.replace('<application', '<application android:name="com.starry.magic.StarryApp"', 1)
    print('application name injected')
if 'com.starry.magic.menu.MenuService' not in t:
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
    t = t.replace('</application>', svc + '\n    </application>', 1)
    print('MenuService injected')
p.write_text(t, encoding='utf-8')
import re
m = re.search(r'<application[^>]*', t)
print('APP_TAG:', (m.group(0) if m else 'n/a')[:220])
print('MF_PATCH_DONE')
