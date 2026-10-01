#Requires -Version 5.1
<#
  win_bootstrap.ps1 - nixiang workspace / Windows host initialisation
  ------------------------------------------------------------------
  Run (recommended, Administrator):
      powershell -ExecutionPolicy Bypass -File .\win_bootstrap.ps1

  Safe to re-run: every step checks state first.

  ENCODING: this file is deliberately PURE ASCII, no BOM.
  Windows PowerShell 5.1 reads a BOM-less .ps1 as the system ANSI code page,
  so any non-ASCII character here (CJK comments, CJK file names in messages)
  risks mojibake and can even break parsing - and a later editor may silently
  drop the BOM that would have protected it. All Chinese explanations live in
  the sibling markdown doc in this same folder. Do not add non-ASCII here.

  Parameters (edit for your machine):
      -RepoRoot   real checkout location, short path, no spaces
      -GitHubRepo SSH clone URL
      -MacRoot    the mac host's literal root. It must match the paths
                  hardcoded inside 364 repo scripts; step 6 replays it as an
                  MSYS junction so those scripts run verbatim on Windows.
#>
param(
    [string]$RepoRoot    = "E:\nixiang",
    [string]$GitHubRepo  = "git@github.com:Aliang369/nixiang.git",
    [string]$MacRoot     = "/Users/aliang/Documents/nixiang",
    [switch]$SkipPackages,
    [switch]$SkipClone,
    [switch]$SkipJunction
)

$ErrorActionPreference = "Stop"

function Step($n, $t) { Write-Host ""; Write-Host ("=== [{0}] {1} ===" -f $n, $t) -ForegroundColor Cyan }
function Ok($m)   { Write-Host ("  [OK]   " + $m) -ForegroundColor Green }
function Warn($m) { Write-Host ("  [WARN] " + $m) -ForegroundColor Yellow }
function Fail($m) { Write-Host ("  [FAIL] " + $m) -ForegroundColor Red }
function Has($c)  { return [bool](Get-Command $c -ErrorAction SilentlyContinue) }

# ------------------------------------------------------------------
Step 0 "Preflight: privileges, drive, path shape"
# ------------------------------------------------------------------
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()
           ).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($isAdmin) { Ok "running elevated" } else { Warn "NOT elevated - registry and junction steps may fail" }

if ($RepoRoot -match ' ') { Fail "RepoRoot contains a space: $RepoRoot - pick a short path like E:\nixiang"; exit 1 }
if ($RepoRoot.Length -gt 40) { Warn "RepoRoot is long ($($RepoRoot.Length) chars); Gradle/NDK paths may exceed MAX_PATH" }

$drive = (Split-Path $RepoRoot -Qualifier)
if (-not (Test-Path $drive)) { Fail "drive $drive does not exist"; exit 1 }
try {
    $vol = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='$drive'"
    switch ($vol.DriveType) {
        3       { Ok "drive $drive is a fixed local disk" }
        2       { Warn "drive $drive is REMOVABLE media - a junction into it breaks when unplugged, and git/NDK will crawl" }
        4       { Warn "drive $drive is a NETWORK drive - never keep a git checkout or an IDA database here" }
        5       { Warn "drive $drive is an optical drive" }
        default { Warn "drive $drive has DriveType=$($vol.DriveType), not a plain fixed disk" }
    }
} catch { Warn "could not query the drive type for $drive (Get-CimInstance unavailable?)" }
Ok "target root: $RepoRoot"

# ------------------------------------------------------------------
Step 1 "Install base toolchain via winget"
# ------------------------------------------------------------------
$pkgs = @(
    @{ Id = "Git.Git";                  Cmd = "git";      Why = "repo + Git Bash (the only shell used for .sh here)" },
    @{ Id = "Python.Python.3.11";       Cmd = "python";   Why = "04_scripts/**/*.py (repo targets 3.11)" },
    @{ Id = "Microsoft.OpenJDK.17";     Cmd = "java";     Why = "apktool / apksigner / gradle all need JDK 17" },
    @{ Id = "Google.PlatformTools";     Cmd = "adb";      Why = "device access (if this host owns a phone)" },
    @{ Id = "7zip.7zip";                Cmd = "7z";       Why = "archive surgery" },
    @{ Id = "jqlang.jq";                Cmd = "jq";       Why = "a few scripts pipe JSON through jq" },
    @{ Id = "Syncthing.Syncthing";      Cmd = "syncthing";Why = "LAN sync of the gitignored bulk dirs" }
)

if ($SkipPackages) {
    Warn "SkipPackages set - not touching winget"
} elseif (-not (Has winget)) {
    Fail "winget not available. Install 'App Installer' from Microsoft Store, then re-run."
} else {
    foreach ($p in $pkgs) {
        if (Has $p.Cmd) { Ok ("already present: {0} ({1})" -f $p.Id, $p.Cmd); continue }
        Write-Host ("  installing {0} ... ({1})" -f $p.Id, $p.Why)
        try {
            winget install --id $p.Id -e --accept-source-agreements --accept-package-agreements --silent | Out-Null
            Ok "installed $($p.Id)"
        } catch {
            Warn "winget failed for $($p.Id): $($_.Exception.Message)"
        }
    }
    Warn "New PATH entries appear only in NEW shells. Close and reopen this terminal before step 2."
}

# ------------------------------------------------------------------
Step 2 "Long path support (06_dumped/** and gradle caches are deep)"
# ------------------------------------------------------------------
try {
    New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" `
        -Name LongPathsEnabled -Value 1 -PropertyType DWORD -Force | Out-Null
    Ok "HKLM LongPathsEnabled = 1"
} catch { Warn "could not set LongPathsEnabled (need Administrator): $($_.Exception.Message)" }

# ------------------------------------------------------------------
Step 3 "Git global config - must be identical on mac and win"
# ------------------------------------------------------------------
$gitCfg = @{
    "core.autocrlf"    = "false"   # let .gitattributes decide; never let git guess
    "core.ignorecase"  = "false"   # case-only renames must not be silently swallowed
    "core.longpaths"   = "true"    # Windows only, harmless elsewhere
    "core.quotepath"   = "false"   # keep CJK paths readable in git output
    "pull.rebase"      = "true"
    "rebase.autoStash" = "true"
    "push.default"     = "simple"
    "fetch.prune"      = "true"
    "init.defaultBranch" = "main"
}
foreach ($k in $gitCfg.Keys) { git config --global $k $gitCfg[$k]; }
Ok "git global config applied:"
git config --global --list | Select-String -Pattern "autocrlf|ignorecase|longpaths|quotepath|rebase" | ForEach-Object { Write-Host ("     " + $_) }

# ------------------------------------------------------------------
Step 4 "Checkout / update the repository"
# ------------------------------------------------------------------
if ($SkipClone) {
    Warn "SkipClone set"
} elseif (Test-Path (Join-Path $RepoRoot ".git")) {
    Ok "repo already present, fetching"
    git -C $RepoRoot fetch --prune
    git -C $RepoRoot status -sb
} else {
    if (Test-Path $RepoRoot) {
        $contents = @(Get-ChildItem -LiteralPath $RepoRoot -Force)
        if ($contents.Count -gt 0) {
            Fail "target dir exists, is not a git repo and is NOT empty: $RepoRoot"
            $contents | Select-Object -First 10 | ForEach-Object { Write-Host "      found: $($_.Name)" }
            Warn "Not touching it. Rename/move it yourself, then re-run, or pass -RepoRoot <other path>."
            exit 1
        }
        Ok "target dir exists and is empty - cloning into it"
    }
    New-Item -ItemType Directory -Force -Path (Split-Path $RepoRoot -Parent) | Out-Null
    Write-Host "  cloning $GitHubRepo"
    Write-Host "  (if this hangs: no SSH key yet - see step 4b)"
    try {
        git clone $GitHubRepo $RepoRoot
        Ok "cloned to $RepoRoot"
    } catch {
        Warn "clone failed. Run:  ssh-keygen -t ed25519 -C win-nixiang   then add the PUBLIC key to GitHub."
        Warn "If you prefer HTTPS:  git clone https://github.com/Aliang369/nixiang.git $RepoRoot"
    }
}

# ------------------------------------------------------------------
Step 5 "Git Bash convenience layer (~/.bashrc)"
# ------------------------------------------------------------------
$gitBashRoot = $null
foreach ($c in @("C:\Program Files\Git\bin\bash.exe", "$env:LOCALAPPDATA\Programs\Git\bin\bash.exe")) {
    if (Test-Path $c) { $gitBashRoot = $c; break }
}
if (-not $gitBashRoot) { Warn "bash.exe not found - install Git first, then re-run"; }
else {
    Ok "Git Bash: $gitBashRoot"
    # E:\nixiang  ->  /e/nixiang
    $driveLetter   = $RepoRoot.Substring(0,1).ToLower()
    $restOfPath    = $RepoRoot.Substring(2).Replace('\','/')
    $bashRootPosix = "/$driveLetter/$restOfPath"
    $bashrcPath = Join-Path $env:USERPROFILE ".bashrc"
    $marker = "# --- nixiang dual-host block (win_bootstrap.ps1) ---"
    $block = @'

# --- nixiang dual-host block (win_bootstrap.ps1) ---
export NIXIANG_ROOT="__ROOT__"
export NIXIANG_HOST=win
# mac keeps its absolute root; win uses this one. Never commit host paths.
alias nixst='git -C "$NIXIANG_ROOT" status -sb'
alias nxpull='git -C "$NIXIANG_ROOT" pull --rebase --autostash'
alias nxfetch='git -C "$NIXIANG_ROOT" fetch --prune'
alias nxlog='git -C "$NIXIANG_ROOT" log --oneline -10'
alias nxclean='git -C "$NIXIANG_ROOT" status --porcelain'

# --- mac-command shims (2026-10-01) ---
# This repo was written on macOS. Measured call sites in 04_scripts + mod_menu:
#   512 x `md5 -q <file>`      16 x `stat -f %z`     1 x `stat -f %Sm`
#   234 x `shasum -a <n>`       35 x `stat -c %s` (GNU, already fine here)
# Git Bash ships md5sum / sha256sum and a GNU stat, but there is NO `md5`, and its
# `stat` rejects the BSD `-f` form. Shimming the three commands below keeps every
# existing repo script working verbatim on Windows, with ZERO edits to the repo.
md5() {
    case "$1" in
        -q) shift; md5sum "$@" | awk '{print $1}' ;;
        -r) shift; md5sum "$@" ;;
        *)  md5sum "$@" ;;
    esac
}
stat() {
    if [ "$1" = "-f" ] && [ -n "$2" ]; then
        _bsd_fmt="$2"
        shift 2
        case "$_bsd_fmt" in
            %z)  wc -c < "$1" | tr -d ' ' ;;
            %m)  date -r "$1" +%s ;;
            %N)  echo "$1" ;;
            %Sm) command stat -c %y "$1" ;;
            *)   echo "stat shim: unsupported BSD format $_bsd_fmt" >&2; return 2 ;;
        esac
        unset _bsd_fmt
    else
        command stat "$@"
    fi
}
if ! command -v shasum >/dev/null 2>&1; then
    shasum() {
        case "$1" in
            -a) _alg="$2"; shift 2 ;;
            *)  _alg=256 ;;
        esac
        case "$_alg" in
            1|sha1)     sha1sum "$@" ;;
            256|sha256) sha256sum "$@" ;;
            512|sha512) sha512sum "$@" ;;
            *) echo "shasum shim: unsupported -a $_alg" >&2; unset _alg; return 2 ;;
        esac
        unset _alg
    }
fi
# export -f so that non-interactive children (`bash 04_scripts/xxx.sh`, which does
# NOT source .bashrc) still inherit the shims. Verified by step 8.
export -f md5 stat 2>/dev/null || true
command -v shasum >/dev/null 2>&1 || export -f shasum 2>/dev/null || true
# endif
'@.Replace('__ROOT__', $bashRootPosix)

    if ((Test-Path $bashrcPath) -and (Select-String -Path $bashrcPath -SimpleMatch $marker -Quiet)) {
        Ok "~/.bashrc already has the nixiang block (left untouched)"
        Ok "expected NIXIANG_ROOT = $bashRootPosix"
    } else {
        Add-Content -Path $bashrcPath -Value $block -Encoding UTF8
        Ok "appended nixiang block to $bashrcPath (NIXIANG_ROOT=$bashRootPosix)"
    }
}

# ------------------------------------------------------------------
Step 6 "MSYS compat junction: make the repo's mac-hardcoded paths resolve"
# ------------------------------------------------------------------
# Why: 8525 literal occurrences of "/Users/aliang/Documents/nixiang" live inside
# 364 tracked scripts. Git Bash maps "/" to the Git installation prefix, so that
# literal path resolves to <GitRoot>\Users\aliang\Documents\nixiang. Pointing that
# at the real checkout fixes all of them at once with ZERO repo diff.
if ($SkipJunction) {
    Warn "SkipJunction set"
} elseif (-not $gitBashRoot) {
    Warn "no Git Bash, skipping"
} else {
    $gitRoot = Split-Path (Split-Path $gitBashRoot -Parent) -Parent
    if (-not $MacRoot.StartsWith("/")) { Fail "-MacRoot must be a posix path like /Users/aliang/Documents/nixiang"; exit 1 }
    # "/Users/aliang/Documents/nixiang" -> "Users\aliang\Documents\nixiang"
    $msysRelative = $MacRoot.TrimStart('/').Replace('/', '\')
    $linkPath   = Join-Path $gitRoot $msysRelative
    $linkParent = Split-Path $linkPath -Parent
    Write-Host "  mac literal root : $MacRoot"
    Write-Host "  replaying it as  : $linkPath"
    try {
        New-Item -ItemType Directory -Force -Path $linkParent | Out-Null
        if (Test-Path $linkPath) {
            Ok "junction path already exists: $linkPath"
        } else {
            New-Item -ItemType Junction -Path $linkPath -Target $RepoRoot | Out-Null
            Ok "junction created: $linkPath  ->  $RepoRoot"
        }
    } catch {
        Warn "junction failed (Program Files needs Administrator): $($_.Exception.Message)"
        Warn "Fallback: install Git per-user, or accept that old scripts need path rewriting."
    }
}

# ------------------------------------------------------------------
Step 7 "Create the gitignored local directories (never synced via git)"
# ------------------------------------------------------------------
if (Test-Path $RepoRoot) {
    $localDirs = @(
        "01_tools",            # host-local toolchains: install per machine, never shared
        "02_apks",             # user-managed, gitignored
        "08_output",           # user-managed, gitignored
        "06_dumped",           # regenerated per machine
        "07_patched",          # regenerated
        "09_test",             # regenerated
        "03_decompiled",       # regenerated
        "05_ida",              # host-local IDA databases
        "11_proj_tools",       # regenerated
        "12_images\cache",     # regenerated
        "99_archive"
    )
    foreach ($d in $localDirs) { New-Item -ItemType Directory -Force -Path (Join-Path $RepoRoot $d) | Out-Null }
    Ok "local dirs ensured (11 paths)"
}

# ------------------------------------------------------------------
Step 8 "Verify: git state, line endings, mac-path shim, mac-command shims"
# ------------------------------------------------------------------
if (Test-Path $RepoRoot) {
    Push-Location $RepoRoot
    Write-Host "  git status -sb:";            git status -sb | ForEach-Object { Write-Host ("    " + $_) }
    Write-Host "  eol check (expect NO i/crlf or i/mixed lines):"
    $bad = git ls-files --eol | Where-Object { $_ -match "i/(crlf|mixed)" }
    if ($bad) {
        Warn "files with CRLF in the index:"
        $bad | Select-Object -First 15 | ForEach-Object { Write-Host ("    " + $_) -ForegroundColor Yellow }
        Write-Host "    (unpacked/dumped assets are whitelisted in .gitattributes; others need review)"
    } else { Ok "no CRLF in the index" }
    Pop-Location
}
if ($gitBashRoot) {
    Write-Host "  mac-path shim inside Git Bash:"
    & $gitBashRoot -lc "cd /Users/aliang/Documents/nixiang 2>/dev/null && pwd && git rev-parse --short HEAD"
    Write-Host "  mac-command shims (expect: 32-hex / digits / sha256 / 32-hex from a child bash):"
    $shimCmd = '. "$HOME/.bashrc" 2>/dev/null; f=/tmp/.nx_shim_probe; printf x > $f; ' +
               'echo "    md5 -q     -> $(md5 -q $f)"; ' +
               'echo "    stat -f %z -> $(stat -f %z $f)"; ' +
               'echo "    shasum     -> $(shasum -a 256 $f | cut -c1-16)..."; ' +
               'echo "    child bash -> $(bash -c "md5 -q $f")"; ' +
               'command -v shasum >/dev/null 2>&1 && echo "    (native shasum present)" || echo "    (shasum shim active)"; ' +
               'rm -f $f'
    & $gitBashRoot -c $shimCmd
}
Write-Host ""
Write-Host "  Remaining manual steps:" -ForegroundColor Cyan
Write-Host "    1. ssh-keygen -t ed25519 -C win-nixiang   -> add public key to GitHub"
Write-Host "    2. close and reopen the terminal so PATH is fresh"
Write-Host "    3. run the acceptance checklist in section 7 of the sibling markdown doc"
