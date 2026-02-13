# Переименование Namespace: RobotProgramming → CodeBlocks

## Зачем это нужно?

**Текущие namespace:**
```csharp
namespace RobotProgramming.Commands
namespace RobotProgramming.Core
// ...
```

**Проблемы:**
- ❌ Название слишком специфичное ("Robot")
- ❌ Не соответствует названию пакета (`com.codeblocks.robotprogramming`)
- ❌ Будущие изменения = breaking change

**После переименования:**
```csharp
namespace CodeBlocks.Commands
namespace CodeBlocks.Core
// ...
```

**Преимущества:**
- ✅ Соответствует названию пакета
- ✅ Более универсальное (не только робот)
- ✅ Стандарт UPM пакетов
- ✅ Избегает конфликтов
- ✅ Делаем один раз — забываем навсегда

---

## Список замен

### 1. RobotProgramming → CodeBlocks

| Старый namespace | Новый namespace |
|------------------|-----------------|
| `RobotProgramming.Commands` | `CodeBlocks.Commands` |
| `RobotProgramming.Core` | `CodeBlocks.Core` |
| `RobotProgramming.Data` | `CodeBlocks.Data` |
| `RobotProgramming.Execution` | `CodeBlocks.Execution` |
| `RobotProgramming.Managers` | `CodeBlocks.Managers` |
| `RobotProgramming.Robot` | `CodeBlocks.Robot` |
| `RobotProgramming.UI` | `CodeBlocks.UI` |

### 2. LevelEditor → CodeBlocks.LevelEditor

| Старый namespace | Новый namespace |
|------------------|-----------------|
| `LevelEditor` | `CodeBlocks.LevelEditor` |

### 3. Promises — БЕЗ изменений!

**⚠️ НЕ трогай namespace Promises!**

Promises остаётся:
```csharp
namespace Promises  // НЕ МЕНЯТЬ!
```

Почему:
- Promises — общая библиотека
- Может использоваться в других местах
- Изменение сломает другие проекты

---

## Способ 1: Автоматическая замена (Visual Studio / Rider)

### Шаг 1: Открыть Find & Replace

**Visual Studio:**
```
Ctrl+Shift+H
```

**Rider:**
```
Ctrl+Shift+R
```

### Шаг 2: Замены (выполнять по очереди!)

#### Замена 1: namespace RobotProgramming

```
Find:    namespace RobotProgramming
Replace: namespace CodeBlocks

Scope: Entire Solution
Files: *.cs
```

**⚠️ Preview before Replace All!**

#### Замена 2: using RobotProgramming

```
Find:    using RobotProgramming
Replace: using CodeBlocks

Scope: Entire Solution
Files: *.cs
```

#### Замена 3: namespace LevelEditor

```
Find:    namespace LevelEditor
Replace: namespace CodeBlocks.LevelEditor

Scope: Entire Solution
Files: *.cs
```

**⚠️ Проверь что не задело "using UnityEditor"!**

#### Замена 4: using LevelEditor

```
Find:    using LevelEditor;
Replace: using CodeBlocks.LevelEditor;

Scope: Entire Solution
Files: *.cs
```

---

## Способ 2: Ручная замена (VS Code или любой редактор)

### Шаг 1: Открыть Search & Replace

```
Ctrl+Shift+F (Search)
Ctrl+Shift+H (Replace)
```

### Шаг 2: Замены

#### Замена 1:
```
Find:    namespace RobotProgramming
Replace: namespace CodeBlocks
```

**Проверь preview → Replace All**

#### Замена 2:
```
Find:    using RobotProgramming
Replace: using CodeBlocks
```

#### Замена 3:
```
Find:    namespace LevelEditor
Replace: namespace CodeBlocks.LevelEditor
```

**⚠️ Проверь Preview — не должно задеть "UnityEditor"!**

#### Замена 4:
```
Find:    using LevelEditor;
Replace: using CodeBlocks.LevelEditor;
```

---

## Способ 3: PowerShell скрипт (Windows)

Скопируй и запусти:

```powershell
# Перейти в папку проекта
cd "D:\Projects\TestCodeBlock"

# Замена 1: namespace RobotProgramming → CodeBlocks
Get-ChildItem -Path . -Filter *.cs -Recurse | ForEach-Object {
    (Get-Content $_.FullName) -replace 'namespace RobotProgramming', 'namespace CodeBlocks' | Set-Content $_.FullName
}

# Замена 2: using RobotProgramming → CodeBlocks
Get-ChildItem -Path . -Filter *.cs -Recurse | ForEach-Object {
    (Get-Content $_.FullName) -replace 'using RobotProgramming', 'using CodeBlocks' | Set-Content $_.FullName
}

# Замена 3: namespace LevelEditor → CodeBlocks.LevelEditor (осторожно!)
Get-ChildItem -Path . -Filter *.cs -Recurse | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    # Только если это НЕ UnityEditor
    if ($content -match 'namespace LevelEditor\b' -and $content -notmatch 'using UnityEditor') {
        $content -replace 'namespace LevelEditor\b', 'namespace CodeBlocks.LevelEditor' | Set-Content $_.FullName
    }
}

# Замена 4: using LevelEditor; → using CodeBlocks.LevelEditor;
Get-ChildItem -Path . -Filter *.cs -Recurse | ForEach-Object {
    (Get-Content $_.FullName) -replace 'using LevelEditor;', 'using CodeBlocks.LevelEditor;' | Set-Content $_.FullName
}

Write-Host "Namespace rename complete!" -ForegroundColor Green
```

---

## После замены: Проверка

### 1. Компиляция

Открой Unity → дождись компиляции

**Ожидается:**
- ✅ 0 ошибок компиляции
- ⚠️ Возможны warnings (нормально)

### 2. Поиск остатков старых namespace

```
Ctrl+Shift+F

Find: namespace RobotProgramming
      → Должно быть 0 результатов

Find: using RobotProgramming
      → Должно быть 0 результатов

Find: namespace LevelEditor
      → Должны остаться только "using UnityEditor"
```

### 3. Проверка using директив

Открой несколько файлов и проверь:

```csharp
// Было
using RobotProgramming.Core;
using RobotProgramming.Commands;

// Стало
using CodeBlocks.Core;
using CodeBlocks.Commands;
```

---

## Если что-то пошло не так

### Git Reset

```bash
# Отменить все изменения
git checkout .

# Попробовать заново
```

### Ручное исправление

Если что-то сломалось:
1. Найди файл с ошибкой (Unity Console)
2. Открой его
3. Исправь namespace/using вручную

---

## Финальный Checklist

- [ ] Выполнены все замены (4 шт)
- [ ] Unity компилирует без ошибок
- [ ] Promises namespace НЕ тронут
- [ ] Git diff проверен (нет лишних изменений)
- [ ] Тесты запускаются (если есть)
- [ ] Коммит сделан:
  ```bash
  git add .
  git commit -m "refactor: rename namespace RobotProgramming → CodeBlocks"
  ```

---

## Оценка времени

| Способ | Время |
|--------|-------|
| Автоматическая замена (IDE) | 5-10 минут |
| VS Code Find & Replace | 10-15 минут |
| PowerShell скрипт | 2-5 минут |
| Ручное | 1-2 часа (не рекомендуется) |

---

## Что дальше?

После переименования namespace:
1. ✅ Продолжить миграцию в UPM пакет
2. ✅ Коммитить изменения
3. ✅ Создать тег v1.0.0

---

## Полезные команды

### Поиск всех namespace

```bash
# PowerShell
Select-String -Path "*.cs" -Pattern "^namespace " -Recurse | Select-Object -Unique Line

# Bash/Git Bash
grep -rh "^namespace " --include="*.cs" . | sort -u
```

### Поиск всех using

```bash
# PowerShell
Select-String -Path "*.cs" -Pattern "^using " -Recurse | Select-Object -Unique Line

# Bash/Git Bash
grep -rh "^using " --include="*.cs" . | sort -u
```
