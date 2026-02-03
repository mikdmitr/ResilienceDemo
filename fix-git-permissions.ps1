# Fix Git ownership and permissions issue
Write-Host "Fixing Git ownership issue..." -ForegroundColor Green

# Add the directory to Git safe directories
$repoPath = "D:/Projects/OTUS/ResilienceDemo/ResilienceDemo.Api"
git config --global --add safe.directory $repoPath

Write-Host "Repository added to safe directories!" -ForegroundColor Green
Write-Host ""

# Also configure UTF-8 encoding
Write-Host "Configuring Git for UTF-8 support..." -ForegroundColor Green
git config --global core.quotepath false
git config --global gui.encoding utf-8
git config --global i18n.commit.encoding utf-8
git config --global i18n.logoutputencoding utf-8

Write-Host ""
Write-Host "Git configuration completed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Now you can work with Git normally:" -ForegroundColor Yellow
Write-Host "  git status" -ForegroundColor White
Write-Host "  git add ." -ForegroundColor White
Write-Host "  git commit -m 'Your message'" -ForegroundColor White
