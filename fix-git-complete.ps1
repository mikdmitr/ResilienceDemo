# Complete Git fix script - solves both ownership and encoding issues
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "   Git Complete Fix Script" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Fix ownership issue
Write-Host "[1/5] Fixing Git ownership issue..." -ForegroundColor Yellow
$repoPath = "D:/Projects/OTUS/ResilienceDemo/ResilienceDemo.Api"
git config --global --add safe.directory $repoPath
Write-Host "      Repository added to safe directories!" -ForegroundColor Green
Write-Host ""

# Step 2: Configure UTF-8 encoding
Write-Host "[2/5] Configuring Git for UTF-8 support..." -ForegroundColor Yellow
git config --global core.quotepath false
git config --global gui.encoding utf-8
git config --global i18n.commit.encoding utf-8
git config --global i18n.logoutputencoding utf-8
Write-Host "      UTF-8 encoding configured!" -ForegroundColor Green
Write-Host ""

# Step 3: Set console encoding
Write-Host "[3/5] Setting console encoding..." -ForegroundColor Yellow
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$env:LESSCHARSET = "utf-8"
Write-Host "      Console encoding set to UTF-8!" -ForegroundColor Green
Write-Host ""

# Step 4: Check Git status
Write-Host "[4/5] Checking Git status..." -ForegroundColor Yellow
try {
    git status
    Write-Host "      Git is working correctly!" -ForegroundColor Green
} catch {
    Write-Host "      Warning: Git status check failed!" -ForegroundColor Red
    Write-Host "      Error: $_" -ForegroundColor Red
}
Write-Host ""

# Step 5: Instructions
Write-Host "[5/5] Next steps:" -ForegroundColor Yellow
Write-Host "      1. Run: git add ." -ForegroundColor White
Write-Host "      2. Run: git commit -m 'Fix encoding and configuration'" -ForegroundColor White
Write-Host "      3. Run: git push" -ForegroundColor White
Write-Host ""

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "   Configuration completed successfully!" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
