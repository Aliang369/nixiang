<#
  build.ps1 —— 用仓库内便携 SDK 构建 advwar_static（无需系统安装 .NET SDK）
  用法:
    pwsh -File 04_scripts/冒险战争/verification/build.ps1 [-Cecil <Mono.Cecil.dll>] [-Config Release]
  退出码: 0 构建成功 / 1 失败 / 2 环境缺失
#>
param(
    [string]$Cecil = "",
    [string]$Config = "Release"
)
$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$root = (Resolve-Path (Join-Path $here '..\..\..')).Path

# 1) 找 dotnet：优先仓库便携 SDK（本机无系统 SDK，dotnet --list-sdks 为空）
$cands = @(
    (Join-Path $root '01_tools\dotnet-sdk8\dotnet.exe'),
    (Join-Path $root '01_tools\dotnet\sdk-9\dotnet.exe')
)
$dotnet = $cands | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $dotnet) {
    $cmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($cmd) { $dotnet = $cmd.Source }
}
if (-not $dotnet) { Write-Error "找不到 dotnet（便携 SDK 与 PATH 都没有）"; exit 2 }

# 2) 构建
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$proj = Join-Path $here 'advwar_static.csproj'
$args = @('build', $proj, '-c', $Config, '-v', 'minimal', '-nologo')
if ($Cecil) { $args += "-p:CecilPath=$Cecil" }
& $dotnet @args
if ($LASTEXITCODE -ne 0) { Write-Error "build 失败 exit=$LASTEXITCODE"; exit 1 }
$out = Join-Path $here "bin\$Config\net8.0\advwar_static.dll"
if (-not (Test-Path $out)) { Write-Error "产物不存在: $out"; exit 1 }
Write-Host "OK  $out"
exit 0
