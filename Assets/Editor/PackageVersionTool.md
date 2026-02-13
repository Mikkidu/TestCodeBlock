# Package Version Tool

**Локация:** `Assets/Editor/PackageVersionTool.cs`
**Меню:** `Tools → CodeBlocks → Package Version Tool`

## Назначение

Автоматизированный инструмент для подготовки нового релиза UPM пакета `com.codeblocks.robotprogramming`.

⚠️ **Важно:** Этот скрипт находится в `Assets/Editor/` и **НЕ входит в UPM пакет**. Он нужен только разработчикам для создания новых версий пакета.

## Функции

### 1. Обновление версий
- Автоматически обновляет версию в `package.json`
- Опционально обновляет версию в `Samples~/package.json`

### 2. Обновление CHANGELOG.md
- Добавляет новую запись в формате [Keep a Changelog](https://keepachangelog.com/en/1.0.0/)
- Автоматически подставляет текущую дату
- Вставляет описание изменений в правильное место (перед предыдущими версиями)

### 3. Обновление README.md
- Обновляет git URL для установки пакета (HTTPS версия)
- Обновляет git URL для SSH версия
- Заменяет версионный тег на новый

### 4. Генерация Git команд и TODO чеклист
- Выводит в консоль **3 сообщения** (вместо 28!):
  1. **TODO чеклист** для проверки изменений
  2. **Git команды** для релиза (единым блоком)
  3. **Success сообщение**

## Использование

### Шаг 1: Открыть инструмент
1. Unity Editor → `Tools → CodeBlocks → Package Version Tool`
2. Откроется окно с формой

### Шаг 2: Заполнить форму

**Поля:**
- **Current Version** (readonly) — текущая версия из `package.json`
- **New Version** (editable) — новая версия в формате `X.Y.Z`
- **Update Samples Version** (checkbox) — обновлять ли версию в `Samples~/package.json`
- **Changelog Description** (textarea) — описание изменений в markdown формате

**Формат описания изменений:**
```markdown
### Added
- Новая функция X
- Компонент Y

### Fixed
- Исправлен баг Z (#номер задачи)

### Changed
- Изменена архитектура W
```

### Шаг 3: Подтвердить
1. Нажать кнопку **"Confirm and Update Files"**
2. Инструмент автоматически обновит все файлы
3. В консоли появятся 3 сообщения ↓

### Шаг 4: Проверить TODO чеклист и выполнить git команды

**Консоль выведет 3 сообщения:**

**Сообщение 1 — TODO Checklist:**
```
═══════════════════════════════════════════════════════════
📦 PACKAGE VERSION X.Y.Z UPDATED
═══════════════════════════════════════════════════════════

✅ TODO: Проверьте изменения перед коммитом:
  [ ] CHANGELOG.md — добавлена новая версия X.Y.Z
  [ ] package.json — версия обновлена на X.Y.Z
  [ ] README.md — git URLs обновлены на vX.Y.Z
  [ ] Все изменения добавлены в git (используйте git status для проверки)

Выполните команды ниже для релиза ↓
```

**Сообщение 2 — Git Commands (копировать целиком):**
```
═══════════════════════════════════════════════════════════
📝 GIT COMMANDS FOR RELEASE vX.Y.Z
═══════════════════════════════════════════════════════════

# 1. Проверить изменения
cd "D:/Projects/TestCodeBlock"
git status
git diff

# 2. Добавить ВСЕ изменённые файлы
git add .

# 3. Создать коммит
git commit -m "Release vX.Y.Z"

# 4. Создать и запушить тег
git tag vX.Y.Z
git push origin vX.Y.Z

# 5. Запушить изменения в мастер
git push origin master

═══════════════════════════════════════════════════════════
```

**Сообщение 3 — Success:**
```
✅ Version X.Y.Z is ready to be released! Скопируйте команды выше.
```

## Валидация

Кнопка "Confirm" активна только если:
- ✅ Новая версия отличается от текущей
- ✅ Поле "New Version" не пустое
- ✅ Поле "Changelog Description" не пустое

## Изменяемые файлы

1. **package.json** — обновляется `"version": "X.Y.Z"`
2. **Samples~/package.json** — обновляется `"version": "X.Y.Z"` (если галочка проставлена)
3. **CHANGELOG.md** — добавляется новая запись в начало
4. **README.md** — обновляются git URL с новой версией тега

## Пример работы

### Входные данные
- Current Version: `1.0.5`
- New Version: `1.0.6`
- Update Samples: ✅ Checked
- Changelog Description:
  ```markdown
  ### Added
  - InitLevel() API for multiple level loading (#24)
  - Lazy initialization pattern

  ### Fixed
  - Memory leak when switching levels
  ```

### Результат

**package.json:**
```json
{
  "version": "1.0.6",
  ...
}
```

**CHANGELOG.md:**
```markdown
## [1.0.6] - 2026-01-26

### Added
- InitLevel() API for multiple level loading (#24)
- Lazy initialization pattern

### Fixed
- Memory leak when switching levels

## [1.0.5] - 2026-01-23
...
```

**README.md:**
```markdown
https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.6
```

**Console output (3 сообщения):**

**Сообщение 1 — TODO Checklist:**
```
═══════════════════════════════════════════════════════════
📦 PACKAGE VERSION 1.0.6 UPDATED
═══════════════════════════════════════════════════════════

✅ TODO: Проверьте изменения перед коммитом:
  [ ] CHANGELOG.md — добавлена новая версия 1.0.6
  [ ] package.json — версия обновлена на 1.0.6
  [ ] Samples~/package.json — версия обновлена на 1.0.6
  [ ] README.md — git URLs обновлены на v1.0.6
  [ ] Все изменения добавлены в git (используйте git status для проверки)

Выполните команды ниже для релиза ↓
```

**Сообщение 2 — Git Commands:**
```
═══════════════════════════════════════════════════════════
📝 GIT COMMANDS FOR RELEASE v1.0.6
═══════════════════════════════════════════════════════════

# 1. Проверить изменения
cd "D:/Projects/TestCodeBlock"
git status
git diff

# 2. Добавить ВСЕ изменённые файлы
git add .

# 3. Создать коммит
git commit -m "Release v1.0.6"

# 4. Создать и запушить тег
git tag v1.0.6
git push origin v1.0.6

# 5. Запушить изменения в мастер
git push origin master

═══════════════════════════════════════════════════════════
```

**Сообщение 3 — Success:**
```
✅ Version 1.0.6 is ready to be released! Скопируйте команды выше.
```

## Важные замечания

⚠️ **Инструмент НЕ выполняет git команды автоматически!**
- Команды только логируются в консоль (3 сообщения)
- Пользователь должен вручную выполнить их в терминале
- Это сделано для безопасности и контроля

⚠️ **TODO чеклист:**
- Проверьте все изменения перед коммитом: `git status`, `git diff`
- Убедитесь что обновлены: CHANGELOG, package.json, README
- Используйте `git add .` чтобы добавить **ВСЕ** изменения (не только перечисленные файлы)

⚠️ **Формат версии:**
- Используйте [Semantic Versioning](https://semver.org/): `MAJOR.MINOR.PATCH`
- Примеры: `1.0.6`, `2.1.0`, `1.0.0-beta.1`

⚠️ **Локация файла:**
- Находится в `Assets/Editor/` — **НЕ входит в UPM пакет**
- Нужен только для разработки пакета, не для пользователей

## Техническая информация

**Файл:** `Assets/Editor/PackageVersionTool.cs`
**Namespace:** `CodeBlocks.Editor.Tools`
**Assembly:** `Assembly-CSharp-Editor` (Assets/Editor)

**Зависимости:**
- UnityEngine
- UnityEditor
- System.IO
- System.Text.RegularExpressions
