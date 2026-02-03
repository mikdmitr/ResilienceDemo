# Fix Git encoding issues with Cyrillic characters
Write-Host "Configuring Git for UTF-8 support..." -ForegroundColor Green

# Configure Git to handle UTF-8 properly
git config --local core.quotepath false
git config --local gui.encoding utf-8
git config --local i18n.commit.encoding utf-8
git config --local i18n.logoutputencoding utf-8

# Set console encoding to UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$env:LESSCHARSET = "utf-8"

Write-Host "Git configuration updated successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Now try to commit your changes again." -ForegroundColor Yellow
Write-Host ""
Write-Host "If the issue persists, run this command:" -ForegroundColor Cyan
Write-Host "git add --renormalize ." -ForegroundColor White
