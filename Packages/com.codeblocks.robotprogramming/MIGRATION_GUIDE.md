# Инструкция по миграции в UPM пакет

## Структура пакета

```
Packages/com.codeblocks.robotprogramming/
├── package.json                    ✅ создан
├── CHANGELOG.md                    ✅ создан
├── Runtime/
│   ├── CodeBlocks.Runtime.asmdef   ✅ создан
│   ├── Commands/                   ← переносим сюда
│   ├── Core/                       ← переносим сюда
│   ├── Data/                       ← переносим сюда
│   ├── Execution/                  ← переносим сюда
│   ├── Managers/                   ← переносим сюда
│   ├── Robot/                      ← переносим сюда
│   ├── UI/                         ← переносим сюда
│   ├── Promises/                   ← переносим сюда
│   └── LevelEditor/                ← Runtime часть
├── Editor/
│   ├── CodeBlocks.Editor.asmdef    ✅ создан
│   └── LevelEditor/                ← Editor скрипты
└── Resources/
    ├── Prefabs/
    │   ├── UI/                     ← UI префабы
    │   └── CodeBlocks/             ← Level Editor префабы
    ├── Levels/                     ← уровни
    └── Configs/                    ← конфиги
```

---

## Шаг 1: Перенос Runtime скриптов

### Из `Assets/Scripts/RobotProgramming/` в `Packages/.../Runtime/`

| Источник | Назначение |
|----------|------------|
| `Assets/Scripts/RobotProgramming/Commands/*.cs` | `Runtime/Commands/` |
| `Assets/Scripts/RobotProgramming/Core/*.cs` | `Runtime/Core/` |
| `Assets/Scripts/RobotProgramming/Data/*.cs` | `Runtime/Data/` |
| `Assets/Scripts/RobotProgramming/Execution/*.cs` | `Runtime/Execution/` |
| `Assets/Scripts/RobotProgramming/Managers/*.cs` | `Runtime/Managers/` |
| `Assets/Scripts/RobotProgramming/Robot/*.cs` | `Runtime/Robot/` |
| `Assets/Scripts/RobotProgramming/UI/*.cs` | `Runtime/UI/` |

### Из `Assets/Scripts/Promises/` в `Packages/.../Runtime/Promises/`

| Источник | Назначение |
|----------|------------|
| `Assets/Scripts/Promises/*.cs` | `Runtime/Promises/` |

### Из `Assets/Scripts/LevelEditor/` (только Runtime)

| Источник | Назначение |
|----------|------------|
| `CardinalDirection.cs` | `Runtime/LevelEditor/` |
| `FinishPoint.cs` | `Runtime/LevelEditor/` |
| `GridObject.cs` | `Runtime/LevelEditor/` |
| `GridVisualizer.cs` | `Runtime/LevelEditor/` |
| `LevelEditorPaletteConfig.cs` | `Runtime/LevelEditor/` |
| `LevelGridData.cs` | `Runtime/LevelEditor/` |
| `LevelJsonData.cs` | `Runtime/LevelEditor/` |
| `LevelVisualizationManager.cs` | `Runtime/LevelEditor/` |
| `ObjectBlockVisual.cs` | `Runtime/LevelEditor/` |
| `StartPoint.cs` | `Runtime/LevelEditor/` |
| `TerrainBlockVisual.cs` | `Runtime/LevelEditor/` |
| `TerrainCell.cs` | `Runtime/LevelEditor/` |

---

## Шаг 2: Перенос Editor скриптов

### Из `Assets/Scripts/LevelEditor/Editor/` в `Packages/.../Editor/LevelEditor/`

| Источник | Назначение |
|----------|------------|
| `CodeBlocksLevelEditorWindow.cs` | `Editor/LevelEditor/` |
| `LevelJsonSerializer.cs` | `Editor/LevelEditor/` |
| `PrefabGenerator.cs` | `Editor/LevelEditor/` |
| `TutorialLevelGenerator.cs` | `Editor/LevelEditor/` |

---

## Шаг 3: Перенос ресурсов

### UI Префабы

| Источник | Назначение |
|----------|------------|
| `Assets/PrefabsUI/BlockUI.prefab` | `Resources/Prefabs/UI/` |
| `Assets/PrefabsUI/LoopBlockUI.prefab` | `Resources/Prefabs/UI/` |
| `Assets/PrefabsUI/BlockPalette.prefab` | `Resources/Prefabs/UI/` |
| `Assets/PrefabsUI/ProgramArea.prefab` | `Resources/Prefabs/UI/` |
| `Assets/PrefabsUI/Controls.prefab` | `Resources/Prefabs/UI/` |

### Level Editor Префабы

| Источник | Назначение |
|----------|------------|
| `Assets/Resources/CodeBlocks/Terrain/*.prefab` | `Resources/Prefabs/CodeBlocks/Terrain/` |
| `Assets/Resources/CodeBlocks/Objects/*.prefab` | `Resources/Prefabs/CodeBlocks/Objects/` |

### Уровни и конфиги

| Источник | Назначение |
|----------|------------|
| `Assets/Resources/RobotLevels/*.asset` | `Resources/Levels/` |
| `Assets/Resources/Configs/*.asset` | `Resources/Configs/` |

---

## Шаг 4: Обновление namespace (опционально)

Если хочешь единый namespace, измени во всех файлах:
- `namespace RobotProgramming.*` → `namespace CodeBlocks.*`

---

## Шаг 5: Исправление путей Resources.Load

После переноса нужно обновить пути загрузки ресурсов:

```csharp
// Было:
Resources.Load<LevelGridData>("RobotLevels/tutorial_01");

// Стало (из пакета):
Resources.Load<LevelGridData>("Levels/tutorial_01");
```

---

## Шаг 6: Тестирование

1. Открой Unity
2. Проверь что пакет появился в `Packages/` в Project window
3. Убедись что компиляция прошла без ошибок
4. Протестируй основные функции

---

## Шаг 7: Подготовка Git репозитория

```bash
# Создай .gitignore в корне пакета (опционально)
echo "*.meta" > .gitignore  # НЕ ДЕЛАЙ ЭТОГО! .meta нужны для Unity

# Добавь в git
git add Packages/com.codeblocks.robotprogramming/
git commit -m "feat: convert to UPM package structure"
git tag v1.0.0
git push origin main --tags
```

---

## Использование в другом проекте

### Вариант A: Git URL (рекомендуется)

В другом проекте:
1. `Window → Package Manager`
2. `+ → Add package from git URL`
3. Введи: `https://github.com/YOUR_USERNAME/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming`

Для конкретной версии:
```
https://github.com/YOUR_USERNAME/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.0
```

### Вариант B: Локальный путь (для разработки)

В `Packages/manifest.json` другого проекта добавь:
```json
{
  "dependencies": {
    "com.codeblocks.robotprogramming": "file:../../TestCodeBlock/Packages/com.codeblocks.robotprogramming"
  }
}
```

---

## Важные замечания

1. **Не удаляй .meta файлы!** Они хранят GUID ссылок
2. **Переноси файлы через Unity** (drag-drop в Project window) - это сохранит ссылки
3. После переноса **удали старые папки** из Assets/Scripts/
4. **Prefabs могут потерять ссылки** на скрипты - нужно будет переназначить
