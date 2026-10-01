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

# Some entries must be a SPECIFIC version: the repo targets Python 3.11 and
# JDK 17. A bare "is there a command named python/java" test silently accepts
# 3.13 or JDK 25 and then the package is NEVER installed - which is exactly
# what happened on the first real Win host (python3 resolved to the Store
# stub, JAVA_HOME to JDK 25). $Want is matched against the tool's own
# --version output; "" means any version will do.
function ToolVersion($c) {
    # $ErrorActionPreference is "Stop" at the top of this script, and in
    # PowerShell 5.1 a native exe writing to stderr is turned into a
    # terminating error - java -version writes to stderr, so without this
    # the probe would always come back empty.
    $prev = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try {
        switch ($c) {
            "python" { return ((& python --version 2>&1) | Out-String) }
            "java"   { return ((& java -version 2>&1) | Out-String) }
            default  { return "" }
        }
    } catch { return "" } finally { $ErrorActionPreference = $prev }
}
# C:\Users\x\a.sh -> /c/Users/x/a.sh  (needed whenever a Windows path is handed to bash.exe)
function ToPosix($p) {
    $q = $p -replace '\\', '/'
    if ($q -match '^([A-Za-z]):/(.*)$') { return "/" + $Matches[1].ToLower() + "/" + $Matches[2] }
    return $q
}

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
    @{ Id = "Git.Git";                  Cmd = "git";      Want = "";     Why = "repo + Git Bash (the only shell used for .sh here)" },
    @{ Id = "Python.Python.3.11";       Cmd = "python";   Want = "3.11"; Why = "04_scripts/**/*.py (repo targets 3.11)" },
    @{ Id = "Microsoft.OpenJDK.17";     Cmd = "java";     Want = "17.";  Why = "apktool / apksigner / gradle all need JDK 17" },
    @{ Id = "Google.PlatformTools";     Cmd = "adb";      Want = "";     Why = "device access (if this host owns a phone)" },
    @{ Id = "7zip.7zip";                Cmd = "7z";       Want = "";     Why = "archive surgery" },
    @{ Id = "jqlang.jq";                Cmd = "jq";       Want = "";     Why = "a few scripts pipe JSON through jq" },
    @{ Id = "Syncthing.Syncthing";      Cmd = "syncthing";Want = "";     Why = "LAN sync of the gitignored bulk dirs" }
)

if ($SkipPackages) {
    Warn "SkipPackages set - not touching winget"
} elseif (-not (Has winget)) {
    Fail "winget not available. Install 'App Installer' from Microsoft Store, then re-run."
} else {
    foreach ($p in $pkgs) {
        if (Has $p.Cmd) {
            if ((-not $p.Want) -or ((ToolVersion $p.Cmd) -match [regex]::Escape($p.Want))) {
                Ok ("already present: {0} ({1})" -f $p.Id, $p.Cmd)
                continue
            }
            Warn ("{0} is on PATH but NOT version {1}: {2}" -f $p.Cmd, $p.Want, (ToolVersion $p.Cmd).Trim())
        }
        Write-Host ("  installing {0} ... ({1})" -f $p.Id, $p.Why)
        try {
            winget install --id $p.Id -e --accept-source-agreements --accept-package-agreements --silent | Out-Null
            $wingetCode = $LASTEXITCODE
            if ($wingetCode -eq 0) {
                Ok "installed $($p.Id)"
            } else {
                # winget is a native exe: a non-zero exit does NOT throw, and
                # PS 5.1 never turns it into a catchable error. Reporting [OK]
                # here is how jq/Syncthing were silently "installed" on the
                # first Win host while winget had actually failed to download.
                Warn ("winget FAILED for {0} (exit 0x{1:X}) - '{2}' is still missing" -f $p.Id, $wingetCode, $p.Cmd)
            }
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
    $restOfPath    = $RepoRoot.Substring(2).TrimStart('\','/').Replace('\','/')
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
    # Export it HERE, inside the guard. Once the function exists, `command -v
    # shasum` answers with the function itself, so doing the export after this
    # block always looks like "a native shasum is present" and the export is
    # skipped - which silently cost every non-interactive child shell
    # (bash 04_scripts/xxx.sh) its 234 `shasum -a` call sites.
    export -f shasum 2>/dev/null || true
fi
# export -f so that non-interactive children (`bash 04_scripts/xxx.sh`, which does
# NOT source .bashrc) still inherit the shims. Verified by step 8.
export -f md5 stat 2>/dev/null || true
# endif
'@.Replace('__ROOT__', $bashRootPosix)

    # Encoding / line endings matter here. Windows PowerShell 5.1's `-Encoding UTF8`
    # writes a UTF-8 BOM, and Add-Content writes CRLF. A BOM on line 1 makes every
    # Git Bash start print   line 1: $'\357\273\277': command not found   and CRLF
    # makes this host-private file differ from the mac side for no reason.
    # Write explicit UTF-8 (no BOM), LF only.
    $blockLf   = $block -replace "`r`n", "`n"
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)

    if ((Test-Path $bashrcPath) -and (Select-String -Path $bashrcPath -SimpleMatch $marker -Quiet)) {
        Ok "~/.bashrc already has the nixiang block (left untouched)"
        Ok "expected NIXIANG_ROOT = $bashRootPosix"
    } else {
        [System.IO.File]::AppendAllText($bashrcPath, $blockLf, $utf8NoBom)
        Ok "appended nixiang block to $bashrcPath (NIXIANG_ROOT=$bashRootPosix)"
    }

    # Repair whatever older runs of this script left behind, in place, preserving all
    # other content: strip a leading UTF-8 BOM, normalise CRLF -> LF, and fix a stale
    # NIXIANG_ROOT line (builds before 2026-10-01 wrote the double-slash "/e//nixiang").
    if (Test-Path $bashrcPath) {
        $rcBytes  = [System.IO.File]::ReadAllBytes($bashrcPath)
        $hadBom   = ($rcBytes.Length -ge 3 -and $rcBytes[0] -eq 0xEF -and $rcBytes[1] -eq 0xBB -and $rcBytes[2] -eq 0xBF)
        $rcText   = [System.IO.File]::ReadAllText($bashrcPath)
        if ($hadBom) { $rcText = $rcText.TrimStart([char]0xFEFF) }
        $rcLfOnly = $rcText -replace "`r`n", "`n"
        $hadCrlf  = ($rcLfOnly -ne $rcText)
        $wantLine = 'export NIXIANG_ROOT="' + $bashRootPosix + '"'
        $haveLine = ([regex]::Match($rcLfOnly, '(?m)^export NIXIANG_ROOT=.*$')).Value
        $rootBad  = ($haveLine -ne '' -and $haveLine -ne $wantLine)
        if ($rootBad) { $rcLfOnly = [regex]::Replace($rcLfOnly, '(?m)^export NIXIANG_ROOT=.*$', $wantLine.Replace('$','$$')) }
        # Builds before this one exported the shasum shim AFTER its guard, where
        # `command -v shasum` already matched the function itself - so the export
        # never happened and non-interactive children silently lost shasum.
        $oldShasum = 'command -v shasum >/dev/null 2>&1 || export -f shasum 2>/dev/null || true'
        $newShasum = 'export -f shasum 2>/dev/null || true'
        $shasumBad = $rcLfOnly.Contains($oldShasum)
        if ($shasumBad) { $rcLfOnly = $rcLfOnly.Replace($oldShasum, $newShasum) }
        if ($hadBom -or $hadCrlf -or $rootBad -or $shasumBad) {
            [System.IO.File]::WriteAllText($bashrcPath, $rcLfOnly, $utf8NoBom)
            if ($hadBom)    { Ok "~/.bashrc repaired: UTF-8 BOM stripped" }
            if ($hadCrlf)   { Ok "~/.bashrc repaired: CRLF -> LF" }
            if ($rootBad)   { Ok ("~/.bashrc repaired: NIXIANG_ROOT  " + $haveLine + "  ->  " + $wantLine) }
            if ($shasumBad) { Ok "~/.bashrc repaired: 'export -f shasum' now runs inside the guard" }
        } else {
            Ok "~/.bashrc clean (no BOM, LF only, NIXIANG_ROOT correct)"
        }
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
    # Do NOT pass this as `bash -c "<multiline string>"`: Windows PowerShell 5.1 does
    # not escape embedded double quotes when it builds a native command line, so bash
    # receives a mangled command and silently prints nothing. That made this
    # self-check a no-op which would hide real shim regressions. Write a temp .sh
    # file (LF, no BOM) and run the file instead - no quoting involved.
    $shimProbe = @'
. "$HOME/.bashrc" 2>/dev/null
f=/tmp/.nx_shim_probe
printf x > $f
echo "    md5 -q     -> $(md5 -q $f)"
echo "    stat -f %z -> $(stat -f %z $f)"
echo "    shasum     -> $(shasum -a 256 $f | cut -c1-16)..."
echo "    child bash -> $(bash -c "md5 -q $f") $(bash -c "shasum -a 256 $f | cut -c1-8")"
# `command -v shasum` answers with the FUNCTION name once the shim is defined, so it
# can never tell a shim from a real binary. type -t can.
case "$(type -t shasum 2>/dev/null)" in
    function) echo "    (shasum shim active, inherited by child bash)" ;;
    file)     echo "    (native shasum present)" ;;
    *)        echo "    (WARN: no shasum shim AND no native shasum)" ;;
esac
rm -f $f
'@ -replace "`r`n", "`n"
    $probeWin = Join-Path $env:TEMP ".nx_shim_probe.sh"
    [System.IO.File]::WriteAllText($probeWin, $shimProbe, (New-Object System.Text.UTF8Encoding($false)))
    & $gitBashRoot (ToPosix $probeWin)
    Remove-Item -LiteralPath $probeWin -Force -ErrorAction SilentlyContinue
}
Write-Host ""
Write-Host "  Remaining manual steps:" -ForegroundColor Cyan
$sshPub = @(Get-ChildItem -Path (Join-Path $env:USERPROFILE ".ssh") -Filter "id_*.pub" -ErrorAction SilentlyContinue)
$n = 0
if ($sshPub.Count -eq 0) {
    $n++
    Write-Host ("    {0}. ssh-keygen -t ed25519 -C win-nixiang   -> add the PUBLIC key to GitHub" -f $n)
} else {
    Write-Host ("    -  SSH key already present ({0} found) - clone/pull over SSH works as-is" -f $sshPub.Count) -ForegroundColor DarkGray
}
$n++; Write-Host ("    {0}. close and reopen the terminal so PATH is fresh" -f $n)
$n++; Write-Host ("    {0}. run the acceptance checklist in section 7 of the sibling markdown doc" -f $n)
