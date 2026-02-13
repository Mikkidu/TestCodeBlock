# UPM Samples Setup - Инструкция по переносу ассетов в Samples~

## 📋 Обзор

Переносим `Assets/CodeBlocks/` (префабы, ресурсы, сцены) в `Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/` для опционального импорта через Package Manager.

## 🎯 Финальная структура

```
Packages/com.codeblocks.robotprogramming/
├── Runtime/                              (код - UPM)
├── Editor/                               (редактор - UPM)
├── Samples~/
│   ├── Assets/
│   │   └── CodeBlocks/
│   │       ├── Materials/
│   │       ├── Prefabs/
│   │       │   ├── LevelEditor/
│   │       │   │   ├── Objects/
│   │       │   │   └── Terrain/
│   │       │   ├── Robot/
│   │       │   └── UI/
│   │       ├── Resources/
│   │       │   ├── Configs/
│   │       │   ├── LevelEditor/
│   │       │   │   ├── Objects/
│   │       │   │   └── Terrain/
│   │       │   └── Levels/
│   │       │       └── Jsons/
│   │       ├── Scene/
│   │       └── Sprites/
│   │           └── UI/
│   └── package.json                    (Sample manifest)
├── package.json                         (основной пакет)
├── CHANGELOG.md
├── README.md
└── ...
```

---

## 🚀 Пошаговая инструкция

### Шаг 1: Создать Samples~ структуру

В терминале/PowerShell выполни:

```powershell
# Переходим в папку пакета
cd Packages/com.codeblocks.robotprogramming

# Создаём структуру Samples~
mkdir Samples~/Assets/CodeBlocks -Force
```

### Шаг 2: Перенести ассеты в Samples~

**Вариант A: Через File Explorer (Windows)**

1. Открыть `Assets/CodeBlocks/`
2. Выделить ВСЕ подпапки (Materials, Prefabs, Resources, Scene, Sprites)
3. Копировать (Ctrl+C)
4. Перейти в `Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/`
5. Вставить (Ctrl+V)
6. ✅ Проверить что все папки скопировались

**Вариант B: Через PowerShell (более надежно)**

```powershell
# Скопировать все подпапки (без самой папки CodeBlocks)
Copy-Item -Path "Assets/CodeBlocks/*" `
          -Destination "Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/" `
          -Recurse -Force

# Проверить результат
Get-ChildItem "Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/"
```

**Вариант C: Через bash (если используешь Git Bash)**

```bash
cp -r Assets/CodeBlocks/* Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/

# Проверить
ls -la Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/
```

### Шаг 3: Создать Samples~/package.json

Создать файл `Packages/com.codeblocks.robotprogramming/Samples~/package.json`:

```json
{
  "name": "com.codeblocks.robotprogramming.samples",
  "version": "1.0.1",
  "displayName": "CodeBlocks Robot Programming - Samples",
  "description": "Sample assets, prefabs, and demo levels for CodeBlocks Robot Programming package.",
  "unity": "6000.0",
  "unityRelease": "0f1",
  "author": {
    "name": "Mikki Ducher",
    "url": "https://github.com/mikkiducher"
  }
}
```

**Важно:**
- `name` должен быть уникальным (обычно добавляют `.samples`)
- `version` совпадает с основным пакетом
- `unity` и `unityRelease` те же что в основном package.json

### Шаг 4: Обновить README.md

Добавить в раздел Installation:

```markdown
### Option 3: With Sample Assets (Recommended)

1. **Add package via UPM** (как обычно)
2. **Import Samples** (опционально):
   - Open Package Manager → CodeBlocks Robot Programming
   - Click "Samples" tab
   - Click "Import" next to "Sample Assets"
   - Assets будут скопированы в `Assets/CodeBlocks/`

**What's included in samples:**
- UI Prefabs (BlockUI, LoopBlockUI, ProgramArea, BlockPalette)
- Robot Prefabs
- LevelEditor Prefabs (Terrain, Objects)
- Demo Levels (5 tutorial levels as JSON)
- Materials and Sprites
- Resource configurations (RobotConfig, LevelEditor configs)
```

### Шаг 5: Обновить CHANGELOG.md

Добавить запись для v1.0.2:

```markdown
## [1.0.2] - 2026-01-21

### Added
- **Samples~ folder** with all assets (prefabs, resources, demo levels)
- Optional import of sample assets through Package Manager
- Sample assets include:
  - UI Prefabs (BlockUI, LoopBlockUI, ProgramArea, BlockPalette)
  - Robot Prefabs
  - LevelEditor Prefabs (Terrain, Objects)
  - 5 Tutorial Levels with JSON configs
  - Materials, Sprites, Resource configurations

### Changed
- Package structure: Code in `Runtime/Editor/`, Assets in `Samples~/Assets/`
- Users can now import sample assets directly from Package Manager

## [1.0.1] - 2026-01-21
...
```

### Шаг 6: Обновить основной package.json

Только визуально проверить (НЕ менять версию, это сделаем в конце):

```json
{
  "name": "com.codeblocks.robotprogramming",
  "version": "1.0.1",  // ← оставить как есть (обновим позже)
  ...
}
```

### Шаг 7: Проверить структуру в Unity

1. **Открыть Unity Project**
2. **Обновить Package Manager:**
   - Window → TextMeshPro → Import TMP Essentials (может спросить)
   - Дождаться пересчёта пакетов
3. **Проверить что нет ошибок:**
   - Console должна быть чистой (или только обычныеWarningBox)
4. **Проверить Samples в Package Manager:**
   - Open Package Manager (Window → Package Manager)
   - Find "CodeBlocks Robot Programming"
   - Должна быть вкладка "Samples" с кнопкой "Import"

### Шаг 8: Удалить старую Assets/CodeBlocks/

⚠️ **ВАЖНО:** Только после проверки что всё скопировалось правильно!

```powershell
# Удалить старую папку
Remove-Item -Path "Assets/CodeBlocks" -Recurse -Force

# Или удалить вручную в File Explorer, потом удалить .meta файл
# Assets/CodeBlocks.meta
```

### Шаг 9: Git commit

```powershell
# Проверить статус
git status

# Добавить новые файлы (Samples~)
git add Packages/com.codeblocks.robotprogramming/Samples~/

# Удалить старые файлы (Assets/CodeBlocks)
git add -A

# Коммит
git commit -m "feat: move sample assets to UPM Samples~ folder

- Move Assets/CodeBlocks/ to Packages/.../Samples~/Assets/CodeBlocks/
- Create Samples~/package.json manifest
- Update README.md with sample import instructions
- Update CHANGELOG.md for v1.0.2 (samples added)

Users can now import sample assets optionally through Package Manager."
```

---

## ✅ Проверочный список

После переноса убедись что:

- [ ] `Packages/com.codeblocks.robotprogramming/Samples~/Assets/CodeBlocks/` существует
- [ ] В нём находятся все подпапки:
  - [ ] Materials/
  - [ ] Prefabs/ (с LevelEditor, Robot, UI)
  - [ ] Resources/ (с Configs, LevelEditor, Levels)
  - [ ] Scene/
  - [ ] Sprites/
- [ ] `Packages/com.codeblocks.robotprogramming/Samples~/package.json` создан
- [ ] README.md обновлён с инструкцией импорта Samples
- [ ] CHANGELOG.md содержит информацию о Samples~
- [ ] Unity Project открывается без ошибок
- [ ] Package Manager показывает вкладку "Samples" для CodeBlocks
- [ ] При клике "Import" ассеты копируются в Assets/CodeBlocks/
- [ ] Старая `Assets/CodeBlocks/` удалена
- [ ] Git статус чистый (после коммита)

---

## 🔧 Если что-то пошло не так

### Проблема: Package Manager не показывает Samples вкладку

**Решение:**
1. Проверить `Samples~/package.json` на синтаксис (валидный JSON)
2. Проверить что папка `Samples~/` находится на уровне `Runtime/`, `Editor/`, `package.json`
3. Перезагрузить Unity Editor: File → Reload Domain

### Проблема: При импорте ассеты копируются неправильно

**Решение:**
1. Удалить импортированные ассеты: Assets/CodeBlocks/
2. Проверить структуру в `Samples~/Assets/CodeBlocks/`
3. Попробовать импортировать снова

### Проблема: Ошибки при загрузке сцены

**Решение:**
1. Проверить что все .meta файлы перенеслись (скрытые файлы в File Explorer)
2. Пересчитать базу GUID: Assets → Reimport All
3. Очистить Library папку (закрыть Unity, удалить Library/, открыть заново)

---

## 📚 Дополнительная информация

### Как работают Samples~ в UPM

1. **Samples~** - специальная папка в UPM пакете (тильда в конце важна!)
2. Контент не импортируется автоматически (остаётся в пакете)
3. В Package Manager отображается вкладка "Samples"
4. Пользователь нажимает "Import" → контент копируется в `Assets/`
5. После импорта Samples~ все ещё находятся в пакете (можно импортировать снова)

### Версионирование

- `Samples~/package.json` версия совпадает с основным пакетом
- Оба пакета (основной и samples) версионируются вместе
- Git tag один: `v1.0.2` (для обоих пакетов)

### Resources.Load пути

После импорта Samples все пути остаются прежними:
- `Resources.Load("RobotConfig")` → ищет в `Assets/CodeBlocks/Resources/Configs/RobotConfig.asset`
- `Resources.Load("Levels/tutorial_01")` → ищет в `Assets/CodeBlocks/Resources/Levels/tutorial_01.asset`

---

## 🎉 Результат

После завершения:
- Код распространяется через UPM (автоматические обновления)
- Ассеты в Samples~ (опциональный импорт, видны в Package Manager)
- Один релиз (v1.0.2) покрывает обе части
- Пользователи могут выбрать: только код или код + samples
