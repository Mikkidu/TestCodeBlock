# Инструкция по миграции (Гибридный подход)

## Концепция

**Код в Packages/** (UPM пакет) — **Ассеты в Assets/** (видимые для команды)

```
Packages/com.codeblocks.robotprogramming/
└── Только скрипты (.cs файлы)

Assets/CodeBlocks/
└── Префабы, материалы, уровни, конфиги
    (Легко найти и модифицировать)
```

---

## Зачем гибридный подход?

✅ **Команде проще** — все ассеты видны в привычном месте Assets/
✅ **Обновления проще** — скрипты обновляются автоматически через UPM
✅ **Кастомизация** — можно менять префабы и ресурсы без изменения пакета
✅ **Гибкость** — разные проекты могут иметь разные ассеты, но общий код

---

## Структура после миграции

```
TestCodeBlock/
├── Packages/
│   └── com.codeblocks.robotprogramming/    ← UPM ПАКЕТ (только код)
│       ├── package.json
│       ├── Runtime/
│       │   ├── Commands/
│       │   ├── Core/
│       │   ├── Data/
│       │   ├── Execution/
│       │   ├── Managers/
│       │   ├── Robot/
│       │   ├── UI/
│       │   ├── Promises/
│       │   └── LevelEditor/
│       └── Editor/
│           └── LevelEditor/
└── Assets/
    └── CodeBlocks/                         ← АССЕТЫ (префабы, уровни)
        ├── Prefabs/
        │   ├── UI/
        │   │   ├── BlockUI.prefab
        │   │   ├── LoopBlockUI.prefab
        │   │   ├── BlockPalette.prefab
        │   │   ├── ProgramArea.prefab
        │   │   └── Controls.prefab
        │   └── LevelEditor/
        │       ├── Terrain/
        │       │   ├── Ground.prefab
        │       │   ├── Road.prefab
        │       │   └── Pit.prefab
        │       └── Objects/
        │           ├── Wall.prefab
        │           ├── Button.prefab
        │           └── Door.prefab
        ├── Resources/
        │   ├── Levels/                     ← Уровни (.asset файлы)
        │   └── Configs/                    ← Конфиги
        └── Scenes/                         ← Сцены
```

---

## Шаг 1: Создать папку Assets/CodeBlocks

В Unity:
1. `Assets → Create → Folder → "CodeBlocks"`
2. Внутри создать:
   - `Prefabs/UI/`
   - `Prefabs/LevelEditor/Terrain/`
   - `Prefabs/LevelEditor/Objects/`
   - `Resources/Levels/`
   - `Resources/Configs/`

---

## Шаг 2: Перенос Runtime скриптов → Packages/

### ⚠️ Переноси через Unity (drag-drop), НЕ через Проводник!

| Источник | Назначение |
|----------|------------|
| `Assets/Scripts/RobotProgramming/Commands/*.cs` | `Packages/.../Runtime/Commands/` |
| `Assets/Scripts/RobotProgramming/Core/*.cs` | `Packages/.../Runtime/Core/` |
| `Assets/Scripts/RobotProgramming/Data/*.cs` | `Packages/.../Runtime/Data/` |
| `Assets/Scripts/RobotProgramming/Execution/*.cs` | `Packages/.../Runtime/Execution/` |
| `Assets/Scripts/RobotProgramming/Managers/*.cs` | `Packages/.../Runtime/Managers/` |
| `Assets/Scripts/RobotProgramming/Robot/*.cs` | `Packages/.../Runtime/Robot/` |
| `Assets/Scripts/RobotProgramming/UI/*.cs` | `Packages/.../Runtime/UI/` |

### ⚠️ ВАЖНО: Promises — НЕ переносить!

**`Assets/Scripts/Promises/` остаётся на месте!**

Почему:
- Promises — общая библиотека (используется в других местах)
- Уже есть в основном проекте
- Чтобы избежать конфликтов дубликатов

Что проверить:
- ✅ `Assets/Scripts/Promises/Promises.asmdef` существует
- ✅ CodeBlocks.Runtime.asmdef имеет ссылку на "Promises"
- ✅ Promises остаётся в Assets/Scripts/Promises/

### Level Editor Runtime скрипты

| Источник | Назначение |
|----------|------------|
| `Assets/Scripts/LevelEditor/CardinalDirection.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/FinishPoint.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/GridObject.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/GridVisualizer.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/LevelEditorPaletteConfig.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/LevelGridData.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/LevelJsonData.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/LevelVisualizationManager.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/ObjectBlockVisual.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/StartPoint.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/TerrainBlockVisual.cs` | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/TerrainCell.cs` | `Packages/.../Runtime/LevelEditor/` |

---

## Шаг 3: Перенос Editor скриптов → Packages/

| Источник | Назначение |
|----------|------------|
| `Assets/Scripts/LevelEditor/Editor/*.cs` | `Packages/.../Editor/LevelEditor/` |

---

## Шаг 4: Перенос ассетов → Assets/CodeBlocks/

### UI Префабы

| Источник | Назначение |
|----------|------------|
| `Assets/PrefabsUI/BlockUI.prefab` | `Assets/CodeBlocks/Prefabs/UI/` |
| `Assets/PrefabsUI/LoopBlockUI.prefab` | `Assets/CodeBlocks/Prefabs/UI/` |
| `Assets/PrefabsUI/BlockPalette.prefab` | `Assets/CodeBlocks/Prefabs/UI/` |
| `Assets/PrefabsUI/ProgramArea.prefab` | `Assets/CodeBlocks/Prefabs/UI/` |
| `Assets/PrefabsUI/Controls.prefab` | `Assets/CodeBlocks/Prefabs/UI/` |

### Level Editor Префабы

| Источник | Назначение |
|----------|------------|
| `Assets/Resources/CodeBlocks/Terrain/*.prefab` | `Assets/CodeBlocks/Prefabs/LevelEditor/Terrain/` |
| `Assets/Resources/CodeBlocks/Objects/*.prefab` | `Assets/CodeBlocks/Prefabs/LevelEditor/Objects/` |

### Уровни и конфиги

| Источник | Назначение |
|----------|------------|
| `Assets/Resources/RobotLevels/*.asset` | `Assets/CodeBlocks/Resources/Levels/` |
| `Assets/Resources/Configs/*.asset` | `Assets/CodeBlocks/Resources/Configs/` |

---

## Шаг 5: Обновление путей Resources.Load

### ❌ Старые пути

```csharp
Resources.Load<LevelGridData>("RobotLevels/tutorial_01");
Resources.Load<GameObject>("CodeBlocks/Terrain/Ground");
```

### ✅ Новые пути

```csharp
Resources.Load<LevelGridData>("Levels/tutorial_01");
Resources.Load<GameObject>("LevelEditor/Terrain/Ground");
```

### Файлы где нужно поменять пути:

Поиск через Unity или Grep:
```bash
grep -r "Resources.Load" Assets/
grep -r "Resources.Load" Packages/
```

Скорее всего нужно обновить:
- `LevelVisualizationManager.cs`
- `TutorialLevelGenerator.cs`
- `GameManager.cs` (если загружает уровни)

---

## Шаг 6: Удаление старых папок

После переноса **БЕЗОПАСНО** удали:
- `Assets/Scripts/RobotProgramming/` (код перенесён в Packages)
- `Assets/Scripts/Promises/` (код перенесён в Packages)
- `Assets/Scripts/LevelEditor/` (код перенесён в Packages)
- `Assets/PrefabsUI/` (префабы перенесены в Assets/CodeBlocks)
- `Assets/Resources/CodeBlocks/` (префабы перенесены)
- `Assets/Resources/RobotLevels/` (уровни перенесены)

**Оставь только:**
- `Assets/CodeBlocks/` (новая структура)

---

## Шаг 7: Тестирование

1. Компиляция без ошибок
2. Открой сцену
3. Протестируй UI (drag-drop блоков)
4. Открой Level Editor
5. Загрузи уровень
6. Проверь что всё работает

---

## Шаг 8: Git коммит

```bash
git add Packages/com.codeblocks.robotprogramming/
git add Assets/CodeBlocks/
git commit -m "feat: convert to UPM package (hybrid structure)"
git tag v1.0.0
git push origin master --tags
```

---

## Использование в другом проекте

### Вариант A: Git URL

```
Package Manager → Add package from git URL

HTTPS:
https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.2

SSH:
git@github.com:mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.2
```

**Что получит другой проект:**
- ✅ Скрипты автоматически (через UPM)
- ❌ Ассеты НЕТ (нужно скопировать вручную или через UnityPackage)

### Вариант B: Git URL + Export Assets

1. **Установить пакет** через Git URL (скрипты)
2. **Экспортировать ассеты** отдельно:
   ```
   Assets/CodeBlocks → Export Package → CodeBlocks_Assets.unitypackage
   ```
3. **В другом проекте:** Import Package

### Вариант C: Полный экспорт (для простоты)

Если команда не готова к разделению:
```
Assets → Export Package
- Выбрать Assets/CodeBlocks/
- Выбрать Packages/com.codeblocks.robotprogramming/
- Экспорт
```

---

## Обновления в будущем

### Скрипты обновляются автоматически

```bash
# В TestCodeBlock
git commit -m "fix: bug in snap"
git tag v1.0.1
git push origin master --tags

# В другом проекте
Package Manager → Check for updates → Update
```

### Ассеты обновляются вручную

```
Export Assets/CodeBlocks/ → unitypackage
Import в другом проекте
```

---

## Преимущества гибридного подхода

✅ **Код в пакете** — автоматические обновления
✅ **Ассеты в Assets/** — легко найти и изменить
✅ **Раздельные релизы** — можно обновить код, не трогая ассеты
✅ **Кастомизация** — каждый проект может иметь свои префабы

---

## Итого

```
Packages/       → Код (обновляется через git)
Assets/CodeBlocks/ → Ассеты (видимы команде, легко найти)
```

Лучшее из обоих миров! 🎉
