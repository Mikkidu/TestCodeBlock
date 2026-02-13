# Блок 3: Wall Collision & Bounce Reaction - Система столкновения стен

**Часть**: Task #25 (Collision System)
**Длительность**: ~3 часа
**Зависимости**: Блок 1 ✓ (Cell Types), Блок 2 ✓ (Finish Logic)

---

## 📋 Описание

Реализовать систему отката робота при столкновении со стеной. Робот пытается двигаться на Wall → откатывается назад с визуальной bounce анимацией. Программа продолжает выполняться нормально.

**Текущее состояние:**
- ✅ LevelGridData имеет objects[] с Wall объектами
- ✅ LevelGridData.IsPassable() проверяет Wall как непроходимый
- ❌ Нет системы обработки столкновений при попытке движения на Wall
- ❌ Нет bounce анимации для отката
- ❌ Нет интеграции с GridPositionTracker

---

## 🎯 Цели

1. Создать интерфейс `IReaction` для любых реакций
2. Реализовать `BounceReaction` для Wall столкновений
3. Создать `CellReactionProcessor` компонент для обработки реакций
4. Интегрировать с `GridPositionTracker.OnGridPositionChanged`
5. При попытке движения на Wall: откатить робота с анимацией
6. Программа продолжает выполняться, следующая команда выполняется нормально

---

## 🔧 Детальные шаги реализации

### Шаг 1: Создать интерфейс IReaction
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/IReaction.cs`

```csharp
using CodeBlocks.Core;
using CodeBlocks.Promises;

namespace CodeBlocks.Collision
{
    /// <summary>
    /// Интерфейс для любого типа реакции робота на клетку поля.
    /// Все реакции возвращают IPromise для асинхронного выполнения анимаций.
    /// </summary>
    public interface IReaction
    {
        /// <summary>Отображаемое имя реакции</summary>
        string Name { get; }

        /// <summary>Тип реакции (Move, Bounce, Fall, Break, Swim, Slide)</summary>
        CellReactionType ReactionType { get; }

        /// <summary>
        /// Выполнить реакцию.
        /// </summary>
        /// <param name="robot">Контроллер робота для движения/ротации</param>
        /// <param name="tracker">GridPositionTracker для получения позиций</param>
        /// <param name="config">Конфигурация реакции (длительность, curve, etc.)</param>
        /// <param name="context">ExecutionContext для отслеживания отмены</param>
        /// <returns>IPromise, который резолвится когда реакция завершена</returns>
        IPromise Execute(
            IRobotController robot,
            GridPositionTracker tracker,
            CellReaction config,
            ExecutionContext context
        );

        /// <summary>Должна ли эта реакция останавливать программу</summary>
        bool StopsProgram { get; }
    }
}
```

---

### Шаг 2: Создать BounceReaction для Wall столкновений
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/Reactions/BounceReaction.cs`

```csharp
using UnityEngine;
using CodeBlocks.Core;
using CodeBlocks.Promises;
using CodeBlocks.LevelEditor;
using CodeBlocks.Managers;

namespace CodeBlocks.Collision.Reactions
{
    /// <summary>
    /// Реакция на столкновение со стеной: робот откатывается назад с bounce анимацией.
    /// Программа НЕ прерывается, следующая команда выполняется нормально.
    /// </summary>
    public class BounceReaction : IReaction
    {
        public string Name => "Bounce (Wall Collision)";
        public CellReactionType ReactionType => CellReactionType.Bounce;
        public bool StopsProgram => false;  // Wall collision НЕ останавливает программу

        public IPromise Execute(
            IRobotController robot,
            GridPositionTracker tracker,
            CellReaction config,
            ExecutionContext context
        )
        {
            Debug.Log("[BounceReaction] Robot hit a wall! Bouncing back...");

            // Логирование для debug
            Debug.Log($"[BounceReaction] Current position: {tracker.CurrentGridPosition}");
            Debug.Log($"[BounceReaction] Animation duration: {config.animationDuration}s");

            // Создать Deferred для синхронизации
            var deferred = new Deferred();

            // Выполнить bounce анимацию
            ExecuteBounce(robot, tracker, config)
                .Done(() =>
                {
                    Debug.Log("[BounceReaction] Bounce animation completed");
                    deferred.Resolve();
                })
                .Fail((exception) =>
                {
                    Debug.LogError($"[BounceReaction] Error during bounce: {exception}");
                    deferred.Reject(exception);
                });

            return deferred;
        }

        /// <summary>Выполнить bounce анимацию</summary>
        private IPromise ExecuteBounce(
            IRobotController robot,
            GridPositionTracker tracker,
            CellReaction config
        )
        {
            // Этапы bounce анимации:
            // 1. (0.0-0.3) Попытка движения вперёд (визуальный feedback)
            // 2. (0.3-1.0) Откат назад на исходную позицию

            var bounceTime = config.animationDuration;
            var curve = config.animationCurve;

            // Получить текущую позицию (до столкновения)
            var startPos = tracker.LastGridPosition;  // Позиция ДО попытки шага
            var wallPos = tracker.CurrentGridPosition; // Позиция стены (куда пытались войти)

            Debug.Log($"[BounceReaction] Bounce: {startPos} ← {wallPos}");

            // ========== ВАРИАНТ 1: ПРОСТОЙ BOUNCE (визуальная анимация) ==========
            // Откатить робота на исходную позицию через visual animation без реального движения

            return ExecuteVisualBounce(robot, tracker, startPos, wallPos, bounceTime, curve);
        }

        /// <summary>Визуальная bounce анимация (без реального движения)</summary>
        private IPromise ExecuteVisualBounce(
            IRobotController robot,
            GridPositionTracker tracker,
            Vector2Int startPos,
            Vector2Int wallPos,
            float bounceTime,
            AnimationCurve curve
        )
        {
            // Получить мировые позиции
            var levelManager = tracker.GetComponent<LevelRuntimeManager>() ??
                Object.FindObjectOfType<LevelRuntimeManager>();

            if (levelManager == null)
            {
                Debug.LogError("[BounceReaction] LevelRuntimeManager not found");
                return Deferred.Resolved();
            }

            var startWorldPos = levelManager.GetWorldPosition(startPos);
            var wallWorldPos = levelManager.GetWorldPosition(wallPos);
            var robotWorldPos = robot.Position;

            Debug.Log($"[BounceReaction] Start world: {startWorldPos}, Wall world: {wallWorldPos}");

            // Фаза 1: Продвижение вперёд (0.15 сек, 50% от времени)
            float phase1Time = bounceTime * 0.3f;

            // Фаза 2: Откат назад (0.45 сек, 50% от времени)
            float phase2Time = bounceTime * 0.7f;

            var deferred = new Deferred();

            // Фаза 1: MoveForward к стене
            robot.MoveForward(0.5f)  // Половинное движение для визуального эффекта
                .Then(() =>
                {
                    // Фаза 2: MoveBackward на исходную позицию
                    return robot.MoveBackward(0.5f);
                })
                .Done(() =>
                {
                    Debug.Log("[BounceReaction] Bounce animation completed");
                    deferred.Resolve();
                })
                .Fail((exception) =>
                {
                    Debug.LogError($"[BounceReaction] Animation failed: {exception}");
                    deferred.Reject(exception);
                });

            return deferred;
        }

        /// <summary>Обработка bounce логики (АЛЬТЕРНАТИВА - для будущего)</summary>
        /// <remarks>
        /// Эта альтернатива может быть использована если нужна более сложная физика bounce.
        /// Пока используем простую визуальную анимацию.
        /// </remarks>
        private IPromise ExecutePhysicalBounce(
            IRobotController robot,
            GridPositionTracker tracker,
            CellReaction config
        )
        {
            // Алтернативный подход: использовать Rigidbody.velocity для реалистичного bounce
            // TODO: Реализовать если потребуется физическая интеграция

            return Deferred.Resolved();
        }
    }
}
```

---

### Шаг 3: Создать CellReactionProcessor
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionProcessor.cs`

```csharp
using UnityEngine;
using CodeBlocks.Collision.Reactions;
using CodeBlocks.Core;
using CodeBlocks.LevelEditor;
using CodeBlocks.Promises;
using System.Collections.Generic;

namespace CodeBlocks.Collision
{
    /// <summary>
    /// Монобиха для обработки реакций клеток.
    /// Слушает GridPositionTracker и выполняет соответствующие реакции.
    /// </summary>
    public class CellReactionProcessor : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Конфигурация реакций (можно оставить null, будет использоваться дефолтная)")]
        private CellReactionConfig reactionConfig;

        private Dictionary<CellReactionType, IReaction> reactions;
        private GridPositionTracker tracker;
        private RobotController robot;
        private LevelRuntimeManager levelManager;

        /// <summary>Event срабатывает когда реакция начинается</summary>
        public event System.Action<CellReactionType> OnReactionStarted;

        /// <summary>Event срабатывает когда реакция завершается</summary>
        public event System.Action<CellReactionType> OnReactionCompleted;

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDisable()
        {
            Cleanup();
        }

        /// <summary>Инициализировать процессор</summary>
        public void Initialize()
        {
            tracker = GetComponent<GridPositionTracker>();
            if (tracker == null)
            {
                Debug.LogError("[CellReactionProcessor] GridPositionTracker not found on same GameObject");
                return;
            }

            robot = GetComponent<RobotController>();
            if (robot == null)
            {
                robot = Object.FindObjectOfType<RobotController>();
            }

            levelManager = Object.FindObjectOfType<LevelRuntimeManager>();

            // Инициализировать реакции
            InitializeReactions();

            // Подписаться на события
            tracker.OnGridPositionChanged += HandleGridPositionChanged;

            Debug.Log("[CellReactionProcessor] Initialized");
        }

        /// <summary>Очистить подписки</summary>
        private void Cleanup()
        {
            if (tracker != null)
            {
                tracker.OnGridPositionChanged -= HandleGridPositionChanged;
            }
        }

        /// <summary>Инициализировать встроенные реакции</summary>
        private void InitializeReactions()
        {
            reactions = new Dictionary<CellReactionType, IReaction>
            {
                { CellReactionType.Bounce, new BounceReaction() },
                // { CellReactionType.Fall, new FallReaction() },      // Блок 4
                // { CellReactionType.Break, new BreakReaction() },    // Блок 4
                // { CellReactionType.Swim, new SwimReaction() },      // Блок 4
                // { CellReactionType.Slide, new SlideReaction() },    // Блок 4
            };

            Debug.Log($"[CellReactionProcessor] Initialized {reactions.Count} reaction types");
        }

        /// <summary>Обработчик события OnGridPositionChanged из GridPositionTracker</summary>
        private void HandleGridPositionChanged(Vector2Int newPos, Vector2Int oldPos)
        {
            ProcessCellReaction(newPos, oldPos);
        }

        /// <summary>Обработать реакцию для клетки</summary>
        private void ProcessCellReaction(Vector2Int newPos, Vector2Int oldPos)
        {
            if (robot == null || tracker == null || levelManager == null)
                return;

            // Получить текущий уровень
            var level = levelManager.CurrentLevel;
            if (level == null)
                return;

            // Получить конфигурацию реакции для этой клетки
            var cellReaction = level.GetCellReaction(newPos.x, newPos.y);

            // Проверить если реакция настроена
            if (cellReaction.reactionType == CellReactionType.None)
                return;

            Debug.Log($"[CellReactionProcessor] Processing reaction: {cellReaction.reactionType} at {newPos}");

            // Получить реализацию реакции
            if (!reactions.TryGetValue(cellReaction.reactionType, out var reaction))
            {
                Debug.LogWarning($"[CellReactionProcessor] No reaction handler for {cellReaction.reactionType}");
                return;
            }

            // Выполнить реакцию
            ExecuteReaction(reaction, cellReaction);
        }

        /// <summary>Выполнить реакцию</summary>
        private void ExecuteReaction(IReaction reaction, CellReaction config)
        {
            OnReactionStarted?.Invoke(reaction.ReactionType);

            Debug.Log($"[CellReactionProcessor] Executing reaction: {reaction.Name}");

            var context = new ExecutionContext();  // TODO: Use actual context from CommandExecutor

            reaction.Execute(robot, tracker, config, context)
                .Done(() =>
                {
                    Debug.Log($"[CellReactionProcessor] Reaction completed: {reaction.Name}");
                    OnReactionCompleted?.Invoke(reaction.ReactionType);

                    // Если реакция останавливает программу, это обрабатывается отдельно
                    // (смотри GridPositionTracker для Finish логики)
                })
                .Fail((exception) =>
                {
                    Debug.LogError($"[CellReactionProcessor] Reaction failed: {exception}");
                });
        }

        /// <summary>Получить реакцию по типу (для debug)</summary>
        public IReaction GetReaction(CellReactionType type)
        {
            if (reactions.TryGetValue(type, out var reaction))
                return reaction;

            return null;
        }
    }
}
```

---

### Шаг 4: Интегрировать CellReactionProcessor с Robot
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/RobotController.cs`

**При инициализации робота:**
```csharp
public class RobotController : MonoBehaviour, IRobotController
{
    private CellReactionProcessor reactionProcessor;

    private void Start()
    {
        // ... существующая инициализация ...

        // Инициализировать CellReactionProcessor если его нет
        reactionProcessor = GetComponent<CellReactionProcessor>();
        if (reactionProcessor == null)
        {
            reactionProcessor = gameObject.AddComponent<CellReactionProcessor>();
        }

        reactionProcessor.Initialize();
    }
}
```

---

### Шаг 5: Обновить LevelGridData для Wall проверки
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/LevelEditor/LevelGridData.cs`

**Обновить IsPassable():**
```csharp
public bool IsPassable(int x, int y)
{
    // Проверить границы
    if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        return false;

    // Проверить objects сначала (Wall)
    var obj = GetObjectAt(x, y);
    if (obj != null && obj.objectTypeId == "Wall")
        return false;  // Wall непроходим

    // Затем проверить terrain
    var terrain = GetTerrainAt(x, y);
    if (terrain == null)
        return true;  // Empty cell is passable

    // Определить проходимость по типу terrain
    return terrain.terrainType switch
    {
        "Pit" => false,         // Яма непроходима
        "Spike" => false,       // Шип непроходим
        "Water" => true,        // Вода проходима (но с замедлением)
        "Ice" => true,          // Лёд проходим (но с ускорением)
        _ => true               // По умолчанию проходимо
    };
}
```

---

### Шаг 6: Дополнить GridPositionTracker для интеграции
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`

**Добавить:**
```csharp
private CellReactionProcessor reactionProcessor;

public void Initialize(LevelRuntimeManager manager, LevelGridData level)
{
    // ... существующая инициализация ...

    // Получить CellReactionProcessor с робота
    reactionProcessor = GetComponent<CellReactionProcessor>();
    if (reactionProcessor == null)
    {
        Debug.LogWarning("[GridPositionTracker] CellReactionProcessor not found");
    }
}

private void UpdateGridPosition()
{
    if (!IsInitialized)
        return;

    var newGridPos = levelRuntimeManager.GetGridPosition(transform.position);

    if (newGridPos == CurrentGridPosition)
        return;

    // === ПОРЯДОК ПРОВЕРОК ===
    // 1. FINISH (приоритет максимум)
    if (!hasReachedFinish && levelRuntimeManager.CurrentLevel != null)
    {
        var finishPoint = levelRuntimeManager.CurrentLevel.GetFinishPoint();
        if (finishPoint != null && newGridPos == finishPoint.position)
        {
            hasReachedFinish = true;
            OnReachedFinish?.Invoke();
            return;  // STOP - больше ничего не проверяем
        }
    }

    // 2. WALL COLLISION (отскок)
    // Эта проверка будет обработана CellReactionProcessor через OnGridPositionChanged

    // 3. PIT/SPIKE (падение, поломка) - Блок 4
    // Эта проверка будет обработана CellReactionProcessor через OnGridPositionChanged

    // === НОРМАЛЬНОЕ ОБНОВЛЕНИЕ ПОЗИЦИИ ===
    OnGridPositionChanged?.Invoke(newGridPos, CurrentGridPosition);
    LastGridPosition = CurrentGridPosition;
    CurrentGridPosition = newGridPos;
}
```

---

### Шаг 7: Тесты для BounceReaction
**Файл**: `Packages/com.codeblocks.robotprogramming/Tests/Editor/BounceReactionTests.cs`

```csharp
using NUnit.Framework;
using UnityEngine;
using CodeBlocks.Collision;
using CodeBlocks.Collision.Reactions;
using CodeBlocks.LevelEditor;
using CodeBlocks.Managers;
using CodeBlocks.Execution;

namespace CodeBlocks.Tests
{
    [TestFixture]
    public class BounceReactionTests
    {
        private RobotController robot;
        private GridPositionTracker tracker;
        private LevelRuntimeManager levelManager;
        private CellReactionProcessor processor;

        [SetUp]
        public void SetUp()
        {
            // Загрузить тестовый уровень
            var testLevel = Resources.Load<LevelGridData>("Levels/tutorial_01_move_forward");
            Assert.IsNotNull(testLevel, "Test level not found");

            // Создать robot с компонентами
            var robotGO = new GameObject("TestRobot");
            robot = robotGO.AddComponent<RobotController>();
            tracker = robotGO.AddComponent<GridPositionTracker>();
            processor = robotGO.AddComponent<CellReactionProcessor>();

            // Создать level manager
            var levelGO = new GameObject("LevelManager");
            levelManager = levelGO.AddComponent<LevelRuntimeManager>();

            // Инициализировать
            levelManager.LoadLevel(testLevel);
            tracker.Initialize(levelManager, testLevel);
            processor.Initialize();
        }

        [Test]
        public void TestBounceReactionExists()
        {
            var bounceReaction = new BounceReaction();
            Assert.AreEqual("Bounce (Wall Collision)", bounceReaction.Name);
            Assert.AreEqual(CellReactionType.Bounce, bounceReaction.ReactionType);
            Assert.IsFalse(bounceReaction.StopsProgram);
        }

        [Test]
        public void TestBounceExecutes()
        {
            var bounceReaction = new BounceReaction();
            var config = new CellReaction(CellReactionType.Bounce, 0.5f);
            var context = new ExecutionContext();

            var promise = bounceReaction.Execute(robot, tracker, config, context);

            Assert.IsNotNull(promise);
            // Promise должна резолвиться асинхронно
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(robot.gameObject);
            Object.Destroy(levelManager.gameObject);
        }
    }
}
```

---

## ✅ Acceptance Criteria (Блок 3)

- [ ] `IReaction` интерфейс создан с методом Execute()
- [ ] `BounceReaction` реализует IReaction
- [ ] BounceReaction.Name = "Bounce (Wall Collision)"
- [ ] BounceReaction.StopsProgram = false
- [ ] BounceReaction выполняет визуальную bounce анимацию
- [ ] `CellReactionProcessor` создан и слушает OnGridPositionChanged
- [ ] При столкновении со стеной срабатывает BounceReaction
- [ ] Робот откатывается назад на исходную позицию
- [ ] Анимация отката длится config.animationDuration
- [ ] Программа продолжает выполняться после bounce
- [ ] Следующая команда выполняется нормально
- [ ] Все реакции выполняются асинхронно через IPromise
- [ ] Тесты проходят
- [ ] Код скомпилирован, 0 errors

---

## 🔍 Debug & Testing в Unity

1. **Создать тестовый уровень с Wall:**
   - Разместить Start, несколько Floor клеток, Wall, затем Finish

2. **Создать программу:**
   - 3x MoveForward (робот должен попытаться войти на Wall)
   - 2x TurnRight

3. **Запустить и наблюдать:**
   ```
   [GridPositionTracker] Position changed: (0,0) → (1,0)
   [GridPositionTracker] Position changed: (1,0) → (2,0)
   [GridPositionTracker] Position changed: (2,0) → (2,0)  ← Wall detected, no movement
   [CellReactionProcessor] Processing reaction: Bounce at (2,0)
   [BounceReaction] Robot hit a wall! Bouncing back...
   [BounceReaction] Bounce animation completed
   [GridPositionTracker] Robot returned to (1,0)
   ```

---

## 🔗 Переход к следующему блоку

После завершения Блока 3, переходим к **Блоку 4: Animation Mapping** → `.Doc/Tasks/25_Block4_AnimationMapping.md`

Блок 3 создаёт инфраструктуру для wall collision. Блок 4 расширяет систему для других типов реакций (Fall, Break, Swim, Slide).
