<#
  libclosure.ps1 —— APK 原生库依赖闭包门禁（只读）
  背景：2026-11-19 冒险战争 b1/b2 漏打 libc++_shared.so，libstarry_magic.so 的 DT_NEEDED 含它，
        System.load 时 dlopen 失败 → ModBridge.nativeInit 无实现 → 菜单「暂无功能列表（native 未就绪）」。
  本脚本：对 APK 内 lib/<abi>/ 下每个 .so 读真实 DT_NEEDED（llvm-readelf -d），
        计算闭包 = NEEDED 必须命中「同 ABI 目录内的 app 库」或「系统库白名单」，缺一即 FAIL。

  用法：
    pwsh -File 04_scripts/冒险战争/verification/libclosure.ps1 -Path <apk>[,<apk2>] [-Readelf <llvm-readelf.exe>] [-Quiet]
  退出码：0 全部闭包完整 / 1 有缺失 / 2 用法或环境错误
#>
param(
    [string[]]$Path,          # 被测 APK，可多个：-Path a.apk,b.apk
    [string]$Readelf = "",
    [string[]]$Ignore = @(),  # 已知可豁免的缺失库名（如动态 dlopen 的插件依赖），命中则记 WARN 不 FAIL
    [switch]$Quiet
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
if (-not $Path -or $Path.Count -eq 0) {
    Write-Host "usage: libclosure.ps1 -Path <apk>[,<apk2>] [-Readelf <exe>] [-Ignore <lib1,lib2>] [-Quiet]"
    exit 2
}

# 系统库白名单（设备 /system/lib64 提供，不要求打进 APK）
$SystemLibs = @(
    'libc.so','libm.so','libdl.so','liblog.so','libandroid.so','libz.so','libstdc++.so',
    'libEGL.so','libGLESv1_CM.so','libGLESv2.so','libGLESv3.so','libjnigraphics.so',
    'libOpenSLES.so','libOpenMAXAL.so','libvulkan.so','libnativewindow.so','libsync.so',
    'libmediandk.so','libcamera2ndk.so','libaaudio.so','libneuralnetworks.so','libnativehelper.so',
    'libicui18n.so','libicuuc.so','libicudata.so','libsqlite.so','libwebviewchromium_loader.so',
    'libhwui.so','libandroid_runtime.so','libbinder.so','libutils.so','libcutils.so','libui.so',
    'libhidlbase.so','libbase.so','libhardware.so','libprocessgroup.so','libRSSupport.so',
    'libc++.so','libc++_static.so'
)

if (-not $Readelf) {
    $cand = Get-ChildItem (Join-Path $PSScriptRoot '..\..\..\01_tools\android-sdk\ndk') -Recurse -Filter 'llvm-readelf.exe' -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
    if ($cand) { $Readelf = $cand }
}
if (-not $Readelf -or -not (Test-Path $Readelf)) { Write-Error "找不到 llvm-readelf.exe（用 -Readelf 指定）"; exit 2 }

function Get-DynInfo([string]$soPath) {
    $out = & $Readelf -d $soPath 2>&1
    $needed = @(); $soname = $null
    foreach ($line in $out) {
        if ($line -match '\(NEEDED\).*\[(.+?)\]') { $needed += $Matches[1] }
        elseif ($line -match '\(SONAME\).*\[(.+?)\]') { $soname = $Matches[1] }
    }
    return [pscustomobject]@{ Needed = $needed; Soname = $soname }
}

function Test-Apk([string]$apk) {
    Write-Host ""
    Write-Host "=== $(Split-Path -Leaf $apk) ==="
    if (-not (Test-Path $apk)) { Write-Host "  FAIL 文件不存在"; return 1 }
    $h = (Get-FileHash $apk -Algorithm SHA256).Hash.ToLower()
    Write-Host "  sha256=$h  size=$((Get-Item $apk).Length)"

    $tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("libclosure_" + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Force -Path $tmp | Out-Null
    $zip = $null; $fail = 0
    try {
        $zip = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $apk).Path)
        $groups = $zip.Entries | Where-Object { $_.FullName -match '^lib/[^/]+/.+\.so$' } |
                  Group-Object { ($_.FullName -split '/')[1] }
        if (-not $groups) { Write-Host "  (无 lib/*/*.so)"; return 0 }

        foreach ($g in $groups) {
            $abi = $g.Name
            $present = @{}
            foreach ($e in $g.Group) { $present[(Split-Path -Leaf $e.FullName)] = $e.Length }

            # 解出到临时目录后 readelf（只解 .so，避免整包）
            $abiDir = Join-Path $tmp $abi
            New-Item -ItemType Directory -Force -Path $abiDir | Out-Null
            $info = @{}
            foreach ($e in $g.Group) {
                $leaf = Split-Path -Leaf $e.FullName
                $p = Join-Path $abiDir $leaf
                $in = $e.Open(); $o = [System.IO.File]::Create($p); $in.CopyTo($o); $o.Close(); $in.Close()
                $info[$leaf] = Get-DynInfo $p
                Remove-Item -LiteralPath $p -Force
            }

            Write-Host "  -- ABI $abi : $($g.Group.Count) 个 .so --"
            foreach ($leaf in ($info.Keys | Sort-Object)) {
                $need = $info[$leaf].Needed
                $missing = @()
                foreach ($n in $need) {
                    if ($present.ContainsKey($n)) { continue }
                    if ($SystemLibs -contains $n) { continue }
                    $missing += $n
                }
                $realMissing = @($missing | Where-Object { $Ignore -notcontains $_ })
                $waived = @($missing | Where-Object { $Ignore -contains $_ })
                if ($waived.Count -gt 0) {
                    Write-Host ("  WARN {0}  缺=[{1}]（按 -Ignore 豁免）" -f $leaf, ($waived -join ',')) -ForegroundColor Yellow
                }
                if ($realMissing.Count -gt 0) {
                    $fail++
                    Write-Host ("  FAIL {0}  NEEDED=[{1}]  缺=[{2}]" -f $leaf, ($need -join ','), ($realMissing -join ',')) -ForegroundColor Red
                } elseif (-not $Quiet) {
                    Write-Host ("  ok   {0}  NEEDED=[{1}]" -f $leaf, ($need -join ','))
                }
            }
        }
    }
    finally {
        if ($zip) { $zip.Dispose() }
        Remove-Item -LiteralPath $tmp -Recurse -Force -ErrorAction SilentlyContinue
    }
    if ($fail -eq 0) { Write-Host "  RESULT: PASS（依赖闭包完整）" -ForegroundColor Green; return 0 }
    Write-Host "  RESULT: FAIL（$fail 个 so 依赖缺失 → 运行期 dlopen 必失败）" -ForegroundColor Red
    return 1
}

$rc = 0
foreach ($a in $Path) { $rc += (Test-Apk $a) }
if ($rc -gt 0) { Write-Host ""; Write-Host "GATE: FAIL" -ForegroundColor Red; exit 1 }
Write-Host ""; Write-Host "GATE: PASS" -ForegroundColor Green; exit 0
