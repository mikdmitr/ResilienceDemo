# Git Commands Reference for ResilienceDemo

## Initial Setup (Run once)
```powershell
# Fix ownership and encoding issues
.\fix-git-complete.ps1
```

## Daily Git Workflow

### Check status
```bash
git status
```

### Add changes
```bash
# Add all files
git add .

# Add specific file
git add Program.cs

# Add with normalization (fixes encoding)
git add --renormalize .
```

### Commit changes
```bash
# Simple commit
git commit -m "Your commit message"

# Commit with detailed message
git commit -m "Title" -m "Detailed description"
```

### Push to remote
```bash
# Push to current branch
git push

# Push and set upstream
git push -u origin main
```

### Pull changes
```bash
git pull
```

## Branch Management

### Create new branch
```bash
git checkout -b feature/new-feature
```

### Switch branches
```bash
git checkout main
```

### List branches
```bash
git branch
```

### Delete branch
```bash
git branch -d feature/old-feature
```

## Undo Changes

### Discard uncommitted changes
```bash
# Specific file
git checkout -- Program.cs

# All files
git checkout -- .
```

### Undo last commit (keep changes)
```bash
git reset --soft HEAD~1
```

### Undo last commit (discard changes)
```bash
git reset --hard HEAD~1
```

## Stash (Temporary save)

### Save current work
```bash
git stash
git stash save "Work in progress"
```

### List stashes
```bash
git stash list
```

### Apply stash
```bash
git stash pop
git stash apply
```

## Advanced

### View commit history
```bash
git log
git log --oneline --graph --all
```

### Show changes
```bash
# Unstaged changes
git diff

# Staged changes
git diff --cached

# Between branches
git diff main..feature/branch
```

### Remove file from Git (keep local)
```bash
git rm --cached filename
```

### Clean untracked files
```bash
# Preview
git clean -n

# Remove
git clean -f
```

## Configuration

### View current config
```bash
git config --list
git config --global --list
git config --local --list
```

### Set user info
```bash
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

### UTF-8 encoding setup
```bash
git config --global core.quotepath false
git config --global gui.encoding utf-8
git config --global i18n.commit.encoding utf-8
git config --global i18n.logoutputencoding utf-8
```

## Troubleshooting

### If commit fails with encoding error:
```bash
git add --renormalize .
git commit -m "Fix encoding"
```

### If ownership error:
```bash
git config --global --add safe.directory "D:/Projects/OTUS/ResilienceDemo/ResilienceDemo.Api"
```

### Reset everything (CAREFUL!)
```bash
git rm --cached -r .
git reset --hard
git clean -fd
```

## .NET Specific

### Ignore bin/obj folders
Already configured in .gitignore

### Ignore Visual Studio files
Already configured in .gitignore

### Before committing:
1. Build solution: `dotnet build`
2. Run tests: `dotnet test`
3. Check for errors
4. Then commit
