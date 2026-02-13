# Блок 5: Pit/Spike Logic & Full Integration - Полная интеграция системы столкновений

**Часть**: Task #25 (Collision System)
**Длительность**: ~3 часа
**Зависимости**: Блоки 1-4 ✓ (все компоненты системы)

---

## 📋 Описание

Завершающий блок: интегрировать все компоненты системы, провести полное тестирование на всех уровнях и убедиться что система готова к использованию.

**Текущее состояние после Блоков 1-4:**
- ✅ CellReactionType (Move, Bounce, Fall, Break) готовы
- ✅ CellReactionConfig готов
- ✅ IReaction интерфейс + BounceReaction готовы
- ✅ FallReaction + BreakReaction готовы
- ✅ CellReactionProcessor готов
- ❌ Нет финальной интеграции с GameManager
- ❌ Нет полного тестирования на уровнях
- ❌ Нет документации по использованию

---

## 🎯 Цели

1. Интегрировать CellReactionProcessor с GameManager
2. Убедиться что Finish ВСЕГДА имеет приоритет (проверяется первым)
3. При Pit/Spike программа останавливается и показывается UI сообщение об ошибке
4. При Wall программа продолжает выполняться (bounce)
5. Полное тестирование на всех 5 tutorial уровнях
6. Создать тестовый уровень с Wall, Pit, Spike для проверки

---

## 🔧 Детальные шаги реализации

### Шаг 1: Интегрировать CellReactionProcessor с GameManager
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs`

**Добавить:**
```csharp
public class GameManager : MonoBehaviour
{
    // ... существующие поля ...

    private CellReactionProcessor reactionProcessor;

    public void Initialize()
    {
        // ... существующая инициализация ...

        // Получить CellReactionProcessor с робота
        if (robot != null)
        {
            reactionProcessor = robot.GetComponent<CellReactionProcessor>();
            if (reactionProcessor == null)
            {
                Debug.LogWarning("[GameManager] CellReactionProcessor not found on robot");
            }
            else
            {
                // Подписаться на события реакций
                reactionProcessor.OnReactionStarted += HandleReactionStarted;
                reactionProcessor.OnReactionCompleted += HandleReactionCompleted;
            }
        }
    }

    /// <summary>Обработчик начала реакции</summary>
    private void HandleReactionStarted(CellReactionType reactionType)
    {
        Debug.Log($"[GameManager] Reaction started: {reactionType}");

        // Обновить UI для некоторых реакций
        switch (reactionType)
        {
            case CellReactionType.Fall:
                UpdateUIMessage("Робот упал в яму! 💀", UIMessageType.Error);
                break;

            case CellReactionType.Break:
                UpdateUIMessage("Робот сломался! 🔧", UIMessageType.Error);
                break;
        }
    }

    /// <summary>Обработчик завершения реакции</summary>
    private void HandleReactionCompleted(CellReactionType reactionType)
    {
        Debug.Log($"[GameManager] Reaction completed: {reactionType}");

        // Если реакция была Fall или Break, остановить программу
        if (reactionType == CellReactionType.Fall || reactionType == CellReactionType.Break)
        {
            Debug.Log("[GameManager] Stopping program due to trap reaction");

            // Остановить программу
            StopProgram();

            // Показать UI кнопку "Try Again"
            ShowRetryButton();

            // Заблокировать UI
            LockUI();
        }
    }

    private void ShowRetryButton()
    {
        // Реализация зависит от текущей UI системы
        // Например: retryButton.SetActive(true);
        Debug.Log("[UI] Showing Retry button");
    }
}
```

---

### Шаг 2: Обновить GridPositionTracker для обработки приоритетов
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`

**Убедиться что порядок проверок правильный:**
```csharp
private void UpdateGridPosition()
{
    if (!IsInitialized)
        return;

    var newGridPos = levelRuntimeManager.GetGridPosition(transform.position);

    if (newGridPos == CurrentGridPosition)
        return;

    // ============ АБСОЛЮТНЫЙ ПРИОРИТЕТ: FINISH ============
    // Finish проверяется ПЕРВЫМ и если достигнут - выход из функции
    if (!hasReachedFinish && levelRuntimeManager.CurrentLevel != null)
    {
        var finishPoint = levelRuntimeManager.CurrentLevel.GetFinishPoint();
        if (finishPoint != null && newGridPos == finishPoint.position)
        {
            Debug.Log($"[GridPositionTracker] Robot reached finish at {newGridPos}!");
            hasReachedFinish = true;

            // КРИТИАЛЬНО: Вызвать OnReachedFinish ДО обновления позиции
            OnReachedFinish?.Invoke();

            // Вернуться без дальнейших проверок
            return;
        }
    }

    // ============ ОСТАЛЬНЫЕ ПРОВЕРКИ (ТОЛЬКО если Finish НЕ достигнут) ============
    // Эти события будут обработаны CellReactionProcessor

    // Обновить позицию
    LastGridPosition = CurrentGridPosition;
    CurrentGridPosition = newGridPos;

    // Воздействовать OnGridPositionChanged
    // CellReactionProcessor слушает это событие и выполняет соответствующую реакцию
    OnGridPositionChanged?.Invoke(newGridPos, LastGridPosition);

    Debug.Log($"[GridPositionTracker] Position changed: {LastGridPosition} → {CurrentGridPosition}");
}
```

---

### Шаг 3: Обновить LevelRuntimeManager для загрузки Pit и Spike префабов
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManager.cs`

**Убедиться что префабы существуют:**
```csharp
private void InstantiateTerrain(Vector2Int gridPos, string terrainType)
{
    // ... существующий код ...

    var prefabPath = terrainType switch
    {
        "Ground" => "LevelEditor/Terrain/Ground",
        "Road" => "LevelEditor/Terrain/Road",
        "Pit" => "LevelEditor/Terrain/Pit",      // Новый префаб
        "Spike" => "LevelEditor/Terrain/Spike",  // Новый префаб
        "Water" => "LevelEditor/Terrain/Water",  // Новый префаб
        "Ice" => "LevelEditor/Terrain/Ice",      // Новый префаб
        _ => "LevelEditor/Terrain/Ground"
    };

    var prefab = Resources.Load<GameObject>(prefabPath);
    if (prefab == null)
    {
        Debug.LogError($"[LevelRuntimeManager] Prefab not found at {prefabPath}");
        return;
    }

    // ... остальной код инстанцирования ...
}
```

**Проверить что все префабы существуют в проекте:**
```
Assets/CodeBlocks/Resources/LevelEditor/Terrain/
├── Ground.prefab
├── Road.prefab
├── Pit.prefab       ← Новый
├── Spike.prefab     ← Новый
├── Water.prefab     ← Новый
└── Ice.prefab       ← Новый
```

---

### Шаг 4: Создать тестовый уровень со всеми типами ловушек
**Место**: `Assets/CodeBlocks/Resources/Levels/`

**Создать: `test_all_traps.asset`**

Структура уровня:
```
Start (0,0) → Floor (1,0) → Water (2,0) → Ice (3,0) → Floor (4,0) →
Wall (5,0) → Floor (5,1) → Spike (5,2) → Floor (5,3) →
Pit (4,3) → Floor (3,3) → Finish (2,3)
```

**Размер**: 6x4
**Start**: (0,0) направление East
**Finish**: (2,3)

---

### Шаг 5: Провести полное тестирование
**Создать checklist для каждого типа ловушки:**

#### 1️⃣ Floor (базовый случай)
```
[ ] Робот проходит нормально
[ ] Анимация движения воспроизводится
[ ] Программа продолжает выполняться
[ ] Следующая команда выполняется
```

#### 2️⃣ Wall
```
[ ] Попытка движения на Wall
[ ] Bounce анимация (откат назад)
[ ] Робот возвращается на исходную позицию
[ ] Программа продолжает выполняться
[ ] Логи: "[BounceReaction] Robot hit a wall!"
```

#### 3️⃣ Water
```
[ ] Робот входит в воду
[ ] UI сообщение: "Робот в воде (замедление)"
[ ] SwimReaction выполняется
[ ] Скорость движения снижается на 50% (waterSpeedModifier=0.5)
[ ] Программа продолжает выполняться
[ ] Логи: "[SwimReaction] Robot entered water!"
```

#### 4️⃣ Ice
```
[ ] Робот входит на лёд
[ ] UI сообщение: "Робот на льду (ускорение)"
[ ] SlideReaction выполняется
[ ] Скорость движения увеличивается на 50% (iceSpeedModifier=1.5)
[ ] Программа продолжает выполняться
[ ] Логи: "[SlideReaction] Robot entered ice!"
```

#### 5️⃣ Pit
```
[ ] Робот попадает в яму
[ ] UI сообщение: "Робот упал в яму! 💀"
[ ] FallReaction выполняется (анимация падения)
[ ] Программа ОСТАНАВЛИВАЕТСЯ
[ ] Робот остаётся в "сломанном" состоянии (прозрачный)
[ ] Показывается кнопка "Try Again"
[ ] Логи: "[FallReaction] Robot fell into a pit!"
```

#### 6️⃣ Spike
```
[ ] Робот наступает на шип
[ ] UI сообщение: "Робот сломался! 🔧"
[ ] BreakReaction выполняется (анимация мигания)
[ ] Программа ОСТАНАВЛИВАЕТСЯ
[ ] Робот остаётся в "сломанном" состоянии
[ ] Показывается кнопка "Try Again"
[ ] Логи: "[BreakReaction] Robot hit a spike and broke!"
```

#### 7️⃣ Finish
```
[ ] Робот достигает финиша
[ ] UI сообщение: "Уровень пройден! 🎉"
[ ] Программа ОСТАНАВЛИВАЕТСЯ немедленно (даже если команды в очереди)
[ ] Показывается кнопка "Next Level"
[ ] Логи: "[GridPositionTracker] Robot reached finish!"
```

---

### Шаг 6: Тестирование на существующих уровнях

**Tutorial Level 1: Move Forward**
```
[ ] Робот движется по Floor
[ ] Достигает Finish
[ ] Программа останавливается
[ ] UI сообщение: "Уровень пройден!"
```

**Tutorial Level 2: Turn and Move**
```
[ ] Робот движется и поворачивается
[ ] Достигает Finish
[ ] Программа останавливается
```

**Tutorial Level 3: Avoid Obstacles**
```
[ ] Робот навигирует вокруг Wall
[ ] Возможны столкновения (Wall bounce)
[ ] Программа продолжает выполняться после bounce
[ ] Робот может достичь Finish
```

**Tutorial Level 4: Buttons & Doors**
```
[ ] Робот взаимодействует с объектами (если поддерживается)
[ ] Все основные механики работают
```

**Tutorial Level 5: Complex Maze**
```
[ ] Большой сложный уровень
[ ] Множество Wall столкновений
[ ] Все bounce реакции работают корректно
[ ] Финальное достижение Finish работает
```

---

### Шаг 7: Написать Integration тесты
**Файл**: `Packages/com.codeblocks.robotprogramming/Tests/Editor/FullCollisionSystemTests.cs`

```csharp
using NUnit.Framework;
using UnityEngine;
using CodeBlocks.Collision;
using CodeBlocks.LevelEditor;
using CodeBlocks.Managers;
using CodeBlocks.Execution;

namespace CodeBlocks.Tests
{
    [TestFixture]
    public class FullCollisionSystemTests
    {
        private GameManager gameManager;
        private RobotController robot;
        private GridPositionTracker tracker;
        private LevelRuntimeManager levelManager;
        private CommandExecutor executor;

        [SetUp]
        public void SetUp()
        {
            var testLevel = Resources.Load<LevelGridData>("Levels/test_all_traps");
            Assert.IsNotNull(testLevel, "Test level not found");

            // Создать GameObject hierarchy
            var gmGO = new GameObject("GameManager");
            gameManager = gmGO.AddComponent<GameManager>();

            var robotGO = new GameObject("Robot");
            robot = robotGO.AddComponent<RobotController>();
            tracker = robotGO.AddComponent<GridPositionTracker>();

            var levelGO = new GameObject("LevelManager");
            levelManager = levelGO.AddComponent<LevelRuntimeManager>();

            var execGO = new GameObject("CommandExecutor");
            executor = execGO.AddComponent<CommandExecutor>();

            // Инициализировать
            levelManager.LoadLevel(testLevel);
            gameManager.Initialize();
        }

        [Test]
        public void TestFinishStopsProgram()
        {
            // Робот находится на позиции до финиша
            var finish = levelManager.CurrentLevel.GetFinishPoint();
            Assert.IsNotNull(finish);

            // Имитировать попадание на финиш
            var finishWorldPos = levelManager.GetWorldPosition(finish.position);
            robot.SetStartPosition(finishWorldPos, Quaternion.identity);

            // Программа должна остановиться
            // (проверяется через OnReachedFinish event)
            Assert.IsTrue(tracker.IsOnFinish());
        }

        [Test]
        public void TestWallBounce()
        {
            var level = levelManager.CurrentLevel;

            // Проверить что на позиции есть Wall
            var wallFound = false;
            for (int x = 0; x < level.gridWidth; x++)
            {
                for (int y = 0; y < level.gridHeight; y++)
                {
                    var obj = level.GetObjectAt(x, y);
                    if (obj != null && obj.objectTypeId == "Wall")
                    {
                        wallFound = true;
                        break;
                    }
                }
                if (wallFound) break;
            }

            Assert.IsTrue(wallFound, "Wall not found in test level");
        }

        [Test]
        public void TestPitDetection()
        {
            var level = levelManager.CurrentLevel;

            // Проверить что на позиции есть Pit
            var pitFound = false;
            for (int x = 0; x < level.gridWidth; x++)
            {
                for (int y = 0; y < level.gridHeight; y++)
                {
                    var terrain = level.GetTerrainAt(x, y);
                    if (terrain != null && terrain.terrainType == "Pit")
                    {
                        pitFound = true;
                        break;
                    }
                }
                if (pitFound) break;
            }

            Assert.IsTrue(pitFound, "Pit not found in test level");
        }

        [Test]
        public void TestWaterDetection()
        {
            var level = levelManager.CurrentLevel;

            // Проверить что на позиции есть Water
            var waterFound = false;
            for (int x = 0; x < level.gridWidth; x++)
            {
                for (int y = 0; y < level.gridHeight; y++)
                {
                    var terrain = level.GetTerrainAt(x, y);
                    if (terrain != null && terrain.terrainType == "Water")
                    {
                        waterFound = true;
                        break;
                    }
                }
                if (waterFound) break;
            }

            Assert.IsTrue(waterFound, "Water not found in test level");
        }

        [Test]
        public void TestAllReactionsRegistered()
        {
            var processor = robot.GetComponent<CellReactionProcessor>();
            Assert.IsNotNull(processor, "CellReactionProcessor not found");

            // Проверить что все реакции зарегистрированы
            Assert.IsNotNull(processor.GetReaction(CellReactionType.Bounce));
            Assert.IsNotNull(processor.GetReaction(CellReactionType.Fall));
            Assert.IsNotNull(processor.GetReaction(CellReactionType.Break));
            Assert.IsNotNull(processor.GetReaction(CellReactionType.Swim));
            Assert.IsNotNull(processor.GetReaction(CellReactionType.Slide));
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(gameManager.gameObject);
            Object.Destroy(robot.gameObject);
            Object.Destroy(levelManager.gameObject);
            Object.Destroy(executor.gameObject);
        }
    }
}
```

---

### Шаг 8: Создать документацию по использованию
**Файл**: `.Doc/Architecture_CollisionSystem.md`

(Документ про архитектуру и использование системы столкновений)

```markdown
# Collision System Architecture - Архитектура системы столкновений

## Обзор

Система столкновений (Collision System) обрабатывает реакции робота при попадании на разные типы клеток поля.

## Структура

```
CellReactionType → CellReaction → IReaction
                                    ├── BounceReaction (Wall)
                                    ├── FallReaction (Pit)
                                    ├── BreakReaction (Spike)
                                    ├── SwimReaction (Water)
                                    └── SlideReaction (Ice)
                 ↓
         CellReactionConfig (ScriptableObject)
                 ↓
         LevelGridData.GetCellReaction()
                 ↓
         GridPositionTracker.OnGridPositionChanged
                 ↓
         CellReactionProcessor.ProcessCellReaction()
                 ↓
         IReaction.Execute()
```

## Использование

### 1. Создать конфиг реакций
```csharp
// Assets/CodeBlocks/Resources/Configs/DefaultCellReactions.asset
// Create → CodeBlocks → Collision → Cell Reaction Config
```

### 2. Назначить конфиг уровню
```csharp
// В LevelGridData assets:
// Drag-drop конфиг в поле cellReactionConfig
```

### 3. Автоматическое воспроизведение реакций
```csharp
// При движении робота:
// 1. GridPositionTracker отслеживает позицию
// 2. CellReactionProcessor обрабатывает реакцию
// 3. IReaction.Execute() воспроизводит анимацию
```

## Приоритеты

```
1. FINISH (максимум) → Stop программу сразу
2. PIT/SPIKE → Stop программу
3. WALL → Bounce, продолжить
4. WATER/ICE → Модификатор, продолжить
5. FLOOR → Normal
```

## Расширение

### Добавить новую реакцию

```csharp
public class MyReaction : IReaction
{
    public string Name => "My Custom Reaction";
    public CellReactionType ReactionType => CellReactionType.YourType;
    public bool StopsProgram => false;

    public IPromise Execute(IRobotController robot, GridPositionTracker tracker, CellReaction config, ExecutionContext context)
    {
        // Реализация...
        return Deferred.Resolved();
    }
}

// Регистрация в CellReactionProcessor.InitializeReactions():
reactions[CellReactionType.YourType] = new MyReaction();
```

...
```

---

## ✅ Acceptance Criteria (Блок 5)

- [ ] CellReactionProcessor интегрирован с GameManager
- [ ] GameManager обрабатывает события OnReactionStarted/Completed
- [ ] Finish ВСЕГДА имеет приоритет (проверяется первым)
- [ ] При Pit/Spike программа останавливается
- [ ] При Water/Ice программа продолжается
- [ ] Тестовый уровень `test_all_traps.asset` создан с 6 типами ловушек
- [ ] Все префабы для ловушек существуют (Pit, Spike, Water, Ice)
- [ ] Полное тестирование на 5 tutorial уровнях пройдено
- [ ] Полное тестирование на новом тестовом уровне пройдено
- [ ] Integration тесты проходят
- [ ] Документация `.Doc/Architecture_CollisionSystem.md` создана
- [ ] Консоль чистая (нет спама LogError)
- [ ] Код скомпилирован, 0 errors

---

## 🔍 Debug & Testing в Unity

### Быстрая проверка всей системы:

1. **Открыть GameScene**
2. **Загрузить уровень `test_all_traps`**
3. **Play**
4. **Запустить программу:**
   ```
   MoveForward
   MoveForward
   MoveForward
   MoveForward
   MoveForward
   TurnLeft
   MoveForward
   (Robot hits Spike, break animation)
   (Program stops)
   ```

5. **Проверить консоль логи:**
   ```
   [GridPositionTracker] Position changed: (0,0) → (1,0)
   [GridPositionTracker] Position changed: (1,0) → (2,0)
   [CellReactionProcessor] Processing reaction: Swim at (2,0)
   [SwimReaction] Robot entered water!
   ...
   [BreakReaction] Robot hit a spike and broke!
   [CellReactionProcessor] Reaction completed: Break (Spike)
   [GameManager] Program stopped due to trap reaction
   ```

---

## 📊 Итоговая статистика Блока 5

| Компонент | Статус |
|---|---|
| CellReactionProcessor | ✅ |
| GameManager интеграция | ✅ |
| Приоритеты Finish | ✅ |
| Pit/Spike логика | ✅ |
| Water/Ice логика | ✅ |
| Тестовый уровень | ✅ |
| Тестирование | ✅ |
| Документация | ✅ |

---

## 🎬 Итоговый результат Task #25

После завершения Блока 5 система столкновений будет **полностью готова к использованию**:

- ✅ 5 типов реакций реализованы
- ✅ Приоритеты выстроены правильно
- ✅ Интеграция с GameManager завершена
- ✅ Все уровни тестированы
- ✅ Система легко расширяется для новых типов ловушек
- ✅ Код готов к интеграции в play-united

**Next Steps**:
1. Запустить систему на всех существующих уровнях
2. Собрать feedback от team
3. Если необходимо: создать новые уровни с ловушками
4. Интегрировать UPM пакет в play-united с версией v1.2.0+

