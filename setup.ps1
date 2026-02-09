# ============================================================
#  VibeCoding 環境自動安裝腳本
#  用法：在 PowerShell 中執行 .\setup.ps1
# ============================================================

$ErrorActionPreference = "Stop"

# ------------------------------------------------------------
#  輔助函式
# ------------------------------------------------------------

function Write-Step  { param($msg) Write-Host "`n▶ $msg" -ForegroundColor Cyan }
function Write-Ok    { param($msg) Write-Host "  ✔ $msg" -ForegroundColor Green }
function Write-Skip  { param($msg) Write-Host "  ⏭ $msg（已安裝，跳過）" -ForegroundColor Yellow }
function Write-Err   { param($msg) Write-Host "  ✘ $msg" -ForegroundColor Red }

function Refresh-Path {
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" +
                [System.Environment]::GetEnvironmentVariable("Path", "User")
}

function Test-Command {
    param($cmd)
    $null = Get-Command $cmd -ErrorAction SilentlyContinue
    return $?
}

# ------------------------------------------------------------
#  開始
# ------------------------------------------------------------

Write-Host ""
Write-Host "========================================" -ForegroundColor Magenta
Write-Host "  VibeCoding 環境自動安裝" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

# 檢查 winget
if (-not (Test-Command "winget")) {
    Write-Err "找不到 winget，請確認 Windows 版本為 10 (1809+) 或 11"
    Write-Err "或前往 https://aka.ms/getwinget 手動安裝"
    exit 1
}
Write-Ok "winget 可用"

# ------------------------------------------------------------
#  1. 安裝軟體
# ------------------------------------------------------------

Write-Step "安裝必要軟體（已安裝的會自動跳過）..."

$packages = @(
    @{ Id = "Microsoft.VisualStudioCode"; Name = "Visual Studio Code" },
    @{ Id = "Microsoft.DotNet.SDK.10";    Name = ".NET 10 SDK" },
    @{ Id = "OpenJS.NodeJS.LTS";          Name = "Node.js LTS" }
)

foreach ($pkg in $packages) {
    $installed = winget list --id $pkg.Id --accept-source-agreements 2>$null |
                 Select-String $pkg.Id
    if ($installed) {
        Write-Skip $pkg.Name
    } else {
        Write-Host "  ⏳ 正在安裝 $($pkg.Name)..." -ForegroundColor White
        winget install --id $pkg.Id --accept-source-agreements --accept-package-agreements --silent
        if ($LASTEXITCODE -eq 0) {
            Write-Ok "$($pkg.Name) 安裝完成"
        } else {
            Write-Err "$($pkg.Name) 安裝失敗（錯誤碼：$LASTEXITCODE）"
            Write-Err "請參考「安裝指引.md」中的手動安裝方式"
            exit 1
        }
    }
}

# 重新載入 PATH，讓新安裝的指令立即可用
Refresh-Path

# ------------------------------------------------------------
#  2. 驗證安裝
# ------------------------------------------------------------

Write-Step "驗證安裝結果..."

$checks = @(
    @{ Cmd = "dotnet";  Arg = "--version"; Label = ".NET SDK" },
    @{ Cmd = "node";    Arg = "--version"; Label = "Node.js" },
    @{ Cmd = "npm";     Arg = "--version"; Label = "npm" },
    @{ Cmd = "code";    Arg = "--version"; Label = "VS Code" }
)

$allPassed = $true
foreach ($c in $checks) {
    if (Test-Command $c.Cmd) {
        $ver = & $c.Cmd $c.Arg 2>$null | Select-Object -First 1
        Write-Ok "$($c.Label): $ver"
    } else {
        Write-Err "$($c.Label) 找不到"
        $allPassed = $false
    }
}

if (-not $allPassed) {
    Write-Host ""
    Write-Host "⚠ 部分軟體找不到，這通常是因為 PATH 尚未生效。" -ForegroundColor Yellow
    Write-Host "  請關閉此視窗，重新開啟 PowerShell，再執行一次：.\setup.ps1" -ForegroundColor Yellow
    exit 1
}

# ------------------------------------------------------------
#  3. 安裝 VS Code 延伸套件
# ------------------------------------------------------------

Write-Step "安裝 VS Code 延伸套件..."

$extensions = @(
    @{ Id = "ms-dotnettools.csdevkit"; Name = "C# Dev Kit" },
    @{ Id = "Vue.volar";               Name = "Vue - Official" }
)

foreach ($ext in $extensions) {
    Write-Host "  ⏳ $($ext.Name)..." -ForegroundColor White
    code --install-extension $ext.Id --force 2>$null
    Write-Ok $ext.Name
}

# ------------------------------------------------------------
#  完成
# ------------------------------------------------------------

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ 環境安裝完成！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "  已安裝：" -ForegroundColor White
Write-Host "    Visual Studio Code（編輯器）"
Write-Host "    .NET 10 SDK（後端環境）"
Write-Host "    Node.js LTS（前端環境）"
Write-Host "    C# Dev Kit（VS Code 套件）"
Write-Host "    Vue - Official（VS Code 套件）"
Write-Host ""
Write-Host "  環境已就緒，可以開始 VibeCoding 了！" -ForegroundColor Cyan
Write-Host ""
