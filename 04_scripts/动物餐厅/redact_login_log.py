import sys,re
need=re.compile(r'checkLogin|updateAppDlg|TapInitializer|TapSign|Gatekeeper|Dwct|Starry|Cocos|cocos|signature|Signature|正版|版本似乎|JS:|jswrapper',re.I)
for line in sys.stdin:
 if not need.search(line):continue
 line=re.sub(r'(?i)(openid|access_token|token|recordSign|authentication|signature|code)([\s"\x27:=]+)[^&\s,}]+',r'\1\2[REDACTED]',line)
 line=re.sub(r'https?://[^\s"<>]+', '[URL_REDACTED]',line)
 if len(line)>1200:line=line[:1200]+' [TRUNCATED]\n'
 sys.stdout.write(line)
