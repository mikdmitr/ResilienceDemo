# Git Issues Fix

## Проблемы

1. **Ownership Issue**: `'D:/Projects/OTUS/ResilienceDemo/ResilienceDemo.Api' is owned by another user`
2. **Encoding Issue**: `Не удается преобразовать байты [CB]`

---

## ?? Быстрое решение (Рекомендуется)

Запустите универсальный скрипт:
```powershell
.\fix-git-complete.ps1
```

Затем выполните:
```bash
git add .
git commit -m "Fix encoding and configuration issues"
```

---

## ?? Пошаговое решение

### Проблема 1: Ownership Issue

**Причина**: Репозиторий создан под пользователем `??????`, а вы работаете под `Elena`

**Решение**:
```bash
git config --global --add safe.directory D:/Projects/OTUS/ResilienceDemo/ResilienceDemo.Api
```

Или запустите:
```powershell
.\fix-git-permissions.ps1
```

### Проблема 2: Encoding Issue

**Причина**: Специальные Unicode символы (?, ?, ?) в исходном коде

**Что было исправлено**:
- ? Удалены проблемные Unicode символы из `Program.cs`
- ? Обновлен `.gitattributes` для правильной обработки CRLF
- ? Настроена UTF-8 кодировка в Git

---

## ?? Дополнительные команды

### Проверить статус Git
```bash
git status
```

### Пересоздать индекс (если нужно)
```bash
git rm --cached -r .
git add .
```

### Нормализовать окончания строк
```bash
git add --renormalize .
```

---

## ?? Что было исправлено

### Изменения в коде:
1. ? **Program.cs** - заменены Unicode символы рамки на ASCII
2. ? **.gitattributes** - настроена обработка текстовых файлов с CRLF

### Созданные файлы:
- `fix-git-complete.ps1` - универсальный скрипт (решает все проблемы)
- `fix-git-permissions.ps1` - исправление прав доступа
- `fix-git-encoding.ps1` - настройка кодировки
- `GIT-ENCODING-FIX.md` - эта инструкция

### Настройки Git для UTF-8:
- ? `core.quotepath = false` - отображение UTF-8 имен файлов
- ? `gui.encoding = utf-8` - кодировка GUI
- ? `i18n.commit.encoding = utf-8` - кодировка коммитов
- ? `i18n.logoutputencoding = utf-8` - кодировка вывода логов
- ? `safe.directory` - добавлена директория в исключения

---

## ? Если проблемы остались

### Попробуйте полный reset:
```bash
# 1. Сохраните изменения
git stash

# 2. Очистите индекс
git rm --cached -r .

# 3. Верните изменения
git stash pop

# 4. Добавьте файлы заново
git add --renormalize .

# 5. Закоммитьте
git commit -m "Fix all Git issues"
```

### Или создайте новый коммит без истории:
```bash
# Удалите .git (ОСТОРОЖНО!)
rm -rf .git

# Инициализируйте заново
git init
git add .
git commit -m "Initial commit with fixes"
```

---

## ?? Контакты для поддержки

Если ничего не помогло, проверьте:
1. Права доступа к папке в Windows
2. Антивирус не блокирует Git
3. Версия Git актуальна: `git --version`
