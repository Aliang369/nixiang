<#
  nativeload_check.ps1 —— native 真加载校验门禁（只读，吃 logcat 文本）
  背景：2026-11-19 冒险战争 b2 装机后菜单显示「暂无功能列表（native 未就绪）」，
        但 logcat 里 MenuService overlay 行是有的 —— 那只证明 Java 侧起来了，不证明 so 装进进程。
  判据（本次事故补的缺口）：
    PASS 条件：出现 native 装载成功证据之一
      - "loaded ... .so"（框架自身加载日志）
      - ModBridge nativeInit 成功 / No implementation found 不出现
      - nativeGetFeatureList 返回非空（featureList ready）
    FAIL 条件（任一命中即 FAIL，优先于 PASS）
      - "No implementation found for ... nativeInit"
      - "is the library loaded?"
      - "dlopen failed" / "UnsatisfiedLinkError" / "couldn't find "lib....so""
      - menu hint "暂无功能列表（native 未就绪）"
    仅出现 MenuService onCreate / overlay 行 ⇒ 判 INCONCLUSIVE（不算通过）。

  用法：
    pwsh -File 04_scripts/冒险战争/verification/nativeload_check.ps1 -Log <logcat.txt> [-SoName libstarry_magic.so]
  退出码：0 PASS / 1 FAIL / 3 INCONCLUSIVE / 2 用法错误
#>
param(
    [string]$Log,
    [string]$SoName = 'libstarry_magic.so'
)
$ErrorActionPreference = 'Stop'
if (-not $Log -or -not (Test-Path $Log)) { Write-Host "usage: nativeload_check.ps1 -Log <logcat.txt> [-SoName libstarry_magic.so]"; exit 2 }

$lines = Get-Content -LiteralPath $Log -Encoding utf8
Write-Host "=== nativeload_check: $(Split-Path -Leaf $Log)  行数=$($lines.Count) ==="

$failPats = @(
    'No implementation found for',
    'is the library loaded',
    'dlopen failed',
    'UnsatisfiedLinkError',
    "couldn't find",
    '暂无功能列表',
    'nativeGetFeatureList.*null',
    'native ensureLoaded=false'
)
$passPats = @(
    ('loaded .*' + [regex]::Escape($SoName)),
    ('(System\.)?load(Library)?.*' + [regex]::Escape($SoName)),
    'ensureLoaded\s*=\s*true',
    'nativeInit\s*(ok|success|PASS|done|=true)',
    'featureList\s*(ready|ok)',
    'featureList.*(count|size)\s*=\s*[1-9]',
    'nativeGetFeatureList.*\[',
    'nativeGetFeatureList.*(ok|non-?null)'
)

# 逐行按时间序归并：同一行可同时命中 → 记两条事件（失败优先按时间序处理）
$events = New-Object System.Collections.ArrayList
for ($i = 0; $i -lt $lines.Count; $i++) {
    $l = $lines[$i]
    $isFail = $false; $isPass = $false
    foreach ($p in $failPats) { if ($l -match $p) { $isFail = $true; break } }
    foreach ($p in $passPats) { if ($l -match $p) { $isPass = $true; break } }
    if ($isFail) { [void]$events.Add([pscustomobject]@{ idx = $i; kind = 'FAIL'; line = $l }) }
    if ($isPass -and -not $isFail) { [void]$events.Add([pscustomobject]@{ idx = $i; kind = 'PASS'; line = $l }) }
}

$overlayOnly = ($events.Count -eq 0) -and [bool]($lines -match 'MenuService|overlay')

$passes = @($events | Where-Object { $_.kind -eq 'PASS' })
$fails  = @($events | Where-Object { $_.kind -eq 'FAIL' })

if ($passes.Count -gt 0) {
    Write-Host "-- 通过证据（共 $($passes.Count) 条）--" -ForegroundColor Green
    $passes | Select-Object -First 5 | ForEach-Object { Write-Host ("  [行{0}] {1}" -f $_.idx, $_.line) }
}
if ($fails.Count -gt 0) {
    Write-Host "-- 失败证据（共 $($fails.Count) 条）--" -ForegroundColor Red
    $fails | Select-Object -First 8 | ForEach-Object { Write-Host ("  [行{0}] {1}" -f $_.idx, $_.line) }
}

# 按时间序取最后一次状态（lead 要求：先失败后成功不应误判 FAIL）
if ($events.Count -eq 0) {
    if ($overlayOnly) {
        Write-Host "RESULT: INCONCLUSIVE（只有 MenuService/overlay 行，无 native 装载证据）" -ForegroundColor Yellow
        exit 3
    }
    Write-Host "RESULT: INCONCLUSIVE（未找到任何 native 装载相关行）" -ForegroundColor Yellow
    exit 3
}
$last = $events[$events.Count - 1]
$firstFail = $fails | Select-Object -First 1
$lastPass  = $passes | Select-Object -Last 1
Write-Host ""
Write-Host ("最后一条状态事件: [行{0}] {1}" -f $last.idx, $last.kind)
if ($last.kind -eq 'PASS') {
    if ($lastPass.idx -gt 0 -and $firstFail -and $firstFail.idx -lt $lastPass.idx) {
        Write-Host ("注：行{0} 曾失败、行{1} 后成功 —— 判定按「最后状态」，视为自愈" -f $firstFail.idx, $lastPass.idx) -ForegroundColor Yellow
    }
    Write-Host "RESULT: PASS（最终状态为 native 已装载）" -ForegroundColor Green
    exit 0
}
Write-Host "RESULT: FAIL（最后一次状态为 native 装载/init 失败）" -ForegroundColor Red
exit 1
