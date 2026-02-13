# Блок 4: Pit/Spike Logic - Реакции на ловушки

**Часть**: Task #25 (Collision System)
**Длительность**: ~2 часа (вместо 3)
**Зависимости**: Блок 1 ✓, Блок 2 ✓, Блок 3 ✓ (IReaction интерфейс, CellReactionProcessor)

---

## 📋 Описание

Реализовать реакции робота на две основные ловушки: падение в яму (Pit) и поломка при попадании на шип (Spike). Обе реакции останавливают программу.

**Текущее состояние:**
- ✅ IReaction интерфейс создан в Блоке 3
- ✅ BounceReaction реализована в Блоке 3
- ✅ CellReactionProcessor создан в Блоке 3
- ❌ Нет FallReaction для Pit
- ❌ Нет BreakReaction для Spike
- ❌ Нет анимаций падения и мигания в RobotController

---

## 🎯 Цели

1. Реализовать `FallReaction` для Pit ловушек (анимация падения)
2. Реализовать `BreakReaction` для Spike ловушек (анимация мигания)
3. Добавить методы в RobotController: `PlayFallAnimation()`, `PlayBreakAnimation()`
4. Интегрировать обе реакции в CellReactionProcessor
5. При Fall/Break: программа ОСТАНАВЛИВАЕТСЯ и показывается UI сообщение
6. Написать unit тесты для обеих реакций

---

## 🔧 Детальные шаги реализации

### Шаг 1: Добавить методы анимаций в RobotController
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/RobotController.cs`

**Добавить методы:**
```csharp
public class RobotController : MonoBehaviour, IRobotController
{
    // Параметры анимаций (можно сделать сериализуемыми)
    private const float FALL_ANIMATION_DURATION = 1.0f;
    private const float BREAK_ANIMATION_DURATION = 0.5f;
    private const float FALL_HEIGHT_OFFSET = -0.5f;
    private const int BREAK_FLASH_COUNT = 3;

    // ... существующие методы ...

    /// <summary>Воспроизвести анимацию падения в яму (Pit)</summary>
    public IPromise PlayFallAnimation()
    {
        Debug.Log("[RobotController] Playing fall animation");

        // Анимация падения: робот "падает" вниз
        // 1. Масштабирование вниз (робот становится меньше)
        // 2. Ротация (робот переворачивается)
        // 3. Сдвиг позиции вниз

        return AnimateFall(FALL_ANIMATION_DURATION, FALL_HEIGHT_OFFSET);
    }

    /// <summary>Воспроизвести анимацию поломки (Spike)</summary>
    public IPromise PlayBreakAnimation()
    {
        Debug.Log("[RobotController] Playing break animation");

        return AnimateBreak(BREAK_ANIMATION_DURATION, BREAK_FLASH_COUNT);
    }

    /// ========== ПРИВАТНЫЕ МЕТОДЫ ==========

    /// <summary>Анимировать падение</summary>
    private IPromise AnimateFall(float duration, float heightOffset)
    {
        var startPos = transform.position;
        var endPos = startPos + Vector3.down * Mathf.Abs(heightOffset);

        // Использовать Timers для синхронизации
        return Timers.Instance.Wait(duration)
            .Then(() =>
            {
                // После ожидания робот находится в "сломанном" состоянии
                transform.position = endPos;
                transform.localScale = Vector3.one * 0.7f;
                return Deferred.Resolved();
            });
    }

    /// <summary>Анимировать поломку (мигание)</summary>
    private IPromise AnimateBreak(float duration, int flashCount)
    {
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("[RobotController] SpriteRenderer not found for break animation");
            return Deferred.Resolved();
        }

        // Использовать Timers для синхронизации
        return Timers.Instance.Wait(duration)
            .Then(() =>
            {
                // После ожидания робот остаётся в "сломанном" состоянии (полупрозрачный)
                spriteRenderer.color = new Color(1, 1, 1, 0.5f);
                return Deferred.Resolved();
            });
    }
}
```

---

### Шаг 2: Создать FallReaction
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/Reactions/FallReaction.cs`

```csharp
using UnityEngine;
using CodeBlocks.Core;
using CodeBlocks.Promises;
using CodeBlocks.LevelEditor;

namespace CodeBlocks.Collision.Reactions
{
    /// <summary>
    /// Реакция на попадание в яму (Pit):
    /// - Визуальная анимация падения
    /// - Программа ОСТАНАВЛИВАЕТСЯ
    /// - UI показывает сообщение об ошибке
    /// </summary>
    public class FallReaction : IReaction
    {
        public string Name => "Fall (Pit)";
        public CellReactionType ReactionType => CellReactionType.Fall;
        public bool StopsProgram => true;  // Pit ОСТАНАВЛИВАЕТ программу

        public IPromise Execute(
            IRobotController robot,
            GridPositionTracker tracker,
            CellReaction config,
            ExecutionContext context
        )
        {
            Debug.Log("[FallReaction] Robot fell into a pit!");

            if (robot == null)
                return Deferred.Resolved();

            var robotController = robot as RobotController;
            if (robotController == null)
                return Deferred.Resolved();

            // Выполнить анимацию падения
            return robotController.PlayFallAnimation()
                .Then(() =>
                {
                    Debug.Log("[FallReaction] Fall animation completed");
                    return Deferred.Resolved();
                })
                .Fail((exception) =>
                {
                    Debug.LogError($"[FallReaction] Error: {exception}");
                    return Deferred.Resolved();  // Не прерывать программу
                });
        }
    }
}
```

---

### Шаг 3: Создать BreakReaction
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/Reactions/BreakReaction.cs`

```csharp
using UnityEngine;
using CodeBlocks.Core;
using CodeBlocks.Promises;

namespace CodeBlocks.Collision.Reactions
{
    /// <summary>
    /// Реакция на попадание на шип (Spike):
    /// - Визуальная анимация поломки (мигание)
    /// - Звуковой эффект (crack)
    /// - Программа ОСТАНАВЛИВАЕТСЯ
    /// - UI показывает сообщение об ошибке
    /// </summary>
    public class BreakReaction : IReaction
    {
        public string Name => "Break (Spike)";
        public CellReactionType ReactionType => CellReactionType.Break;
        public bool StopsProgram => true;  // Spike ОСТАНАВЛИВАЕТ программу

        public IPromise Execute(
            IRobotController robot,
            GridPositionTracker tracker,
            CellReaction config,
            ExecutionContext context
        )
        {
            Debug.Log("[BreakReaction] Robot hit a spike and broke!");

            if (robot == null)
                return Deferred.Resolved();

            var robotController = robot as RobotController;
            if (robotController == null)
                return Deferred.Resolved();

            // Выполнить анимацию поломки
            return robotController.PlayBreakAnimation()
                .Then(() =>
                {
                    Debug.Log("[BreakReaction] Break animation completed");
                    return Deferred.Resolved();
                })
                .Fail((exception) =>
                {
                    Debug.LogError($"[BreakReaction] Error: {exception}");
                    return Deferred.Resolved();
                });
        }
    }
}
```

---

### Шаг 4: Обновить CellReactionProcessor для новых реакций
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionProcessor.cs`

**Обновить InitializeReactions():**
```csharp
private void InitializeReactions()
{
    reactions = new Dictionary<CellReactionType, IReaction>
    {
        { CellReactionType.Bounce, new BounceReaction() },
        { CellReactionType.Fall, new FallReaction() },
        { CellReactionType.Break, new BreakReaction() },
    };

    Debug.Log($"[CellReactionProcessor] Initialized {reactions.Count} reaction types");
}
```

---

### Шаг 5: Написать тесты
**Файл**: `Packages/com.codeblocks.robotprogramming/Tests/Editor/PitSpikeReactionTests.cs`

```csharp
using NUnit.Framework;
using UnityEngine;
using CodeBlocks.Collision;
using CodeBlocks.Collision.Reactions;
using CodeBlocks.Core;
using CodeBlocks.Execution;
using CodeBlocks.LevelEditor;
using CodeBlocks.Managers;

namespace CodeBlocks.Tests
{
    [TestFixture]
    public class PitSpikeReactionTests
    {
        private RobotController robot;
        private GridPositionTracker tracker;
        private LevelRuntimeManager levelManager;
        private ExecutionContext context;

        [SetUp]
        public void SetUp()
        {
            var testLevel = Resources.Load<LevelGridData>("Levels/tutorial_01_move_forward");
            Assert.IsNotNull(testLevel);

            var robotGO = new GameObject("TestRobot");
            robot = robotGO.AddComponent<RobotController>();
            tracker = robotGO.AddComponent<GridPositionTracker>();

            var levelGO = new GameObject("LevelManager");
            levelManager = levelGO.AddComponent<LevelRuntimeManager>();
            levelManager.LoadLevel(testLevel);

            context = new ExecutionContext();
        }

        [Test]
        public void TestFallReactionExists()
        {
            var reaction = new FallReaction();
            Assert.AreEqual(CellReactionType.Fall, reaction.ReactionType);
            Assert.IsTrue(reaction.StopsProgram, "Fall reaction should stop program");
        }

        [Test]
        public void TestBreakReactionExists()
        {
            var reaction = new BreakReaction();
            Assert.AreEqual(CellReactionType.Break, reaction.ReactionType);
            Assert.IsTrue(reaction.StopsProgram, "Break reaction should stop program");
        }

        [Test]
        public void TestFallAnimationExecutes()
        {
            var promise = robot.PlayFallAnimation();
            Assert.IsNotNull(promise);
        }

        [Test]
        public void TestBreakAnimationExecutes()
        {
            var promise = robot.PlayBreakAnimation();
            Assert.IsNotNull(promise);
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

## ✅ Acceptance Criteria (Блок 4)

- [ ] `FallReaction` реализована (падение в яму, STOP программу)
- [ ] `BreakReaction` реализована (поломка на шипе, STOP программу)
- [ ] Обе реакции наследуют IReaction с `StopsProgram = true`
- [ ] RobotController имеет методы: `PlayFallAnimation()`, `PlayBreakAnimation()`
- [ ] Методы возвращают IPromise
- [ ] CellReactionProcessor инициализирует обе реакции (Bounce, Fall, Break)
- [ ] Обе реакции интегрированы в CellReactionProcessor
- [ ] При Fall/Break программа ОСТАНАВЛИВАЕТСЯ
- [ ] Анимации воспроизводятся: Fall (1.0 сек), Break (0.5 сек)
- [ ] Тесты PitSpikeReactionTests проходят
- [ ] Код скомпилирован, 0 errors, console чистая

---

## 🔍 Debug & Testing в Unity

1. **Создать тестовый уровень с разными ловушками:**
   - Ground → Water → Ice → Spike → Pit → Finish

2. **Создать программу:**
   - 6x MoveForward (проверить каждый тип клетки)

3. **Запустить и наблюдать:**
   ```
   [GridPositionTracker] Position: (0,0) → (1,0) [Ground]
   [GridPositionTracker] Position: (1,0) → (2,0) [Water]
   [SwimReaction] Robot entered water!
   [GridPositionTracker] Position: (2,0) → (3,0) [Ice]
   [SlideReaction] Robot entered ice!
   [GridPositionTracker] Position: (3,0) → (4,0) [Spike]
   [BreakReaction] Robot hit a spike and broke!
   [CellReactionProcessor] Reaction completed: Break (Spike)
   [GameManager] Program stopped due to break reaction
   ```

---

## 🔗 Переход к следующему блоку

После завершения Блока 4, переходим к **Блоку 5: Integration & Testing** → `.Doc/Tasks/25_Block5_Integration.md`

Блок 4 завершает реализацию всех трёх типов реакций (Bounce, Fall, Break). Блок 5 интегрирует систему и проводит полное тестирование на всех уровнях.
