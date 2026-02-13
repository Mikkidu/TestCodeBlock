# Блок 2: Finish Logic Improvements - Улучшение логики финиша

**Часть**: Task #25 (Collision System)
**Длительность**: ~2 часа
**Зависимости**: Блок 1 ✓ (Cell Type System)
**Статус**: Depends on #21 ✓, #24 ✓, #27 ✓

---

## 📋 Описание

Модифицировать систему обработки финиша так чтобы программа ВСЕГДА останавливалась при достижении финиша, даже если в очереди есть команды. Использовать существующую инфраструктуру `ExecutionContext.IsCancelled` и `CommandExecutor.Stop()` из #27.

**Текущее состояние:**
- ✅ GridPositionTracker.OnReachedFinish срабатывает при совпадении позиции с финишем
- ✅ GameManager.OnRobotReachedFinish() показывает UI message
- ✅ CommandExecutor.Stop() устанавливает context.Cancel() (#27)
- ❌ Нет гарантии что программа остановится ДО выполнения следующей команды
- ❌ Нет разделения между "программа завершена" (Finish) и "программа остановлена вручную" (Reset/Clear)

---

## 🎯 Цели

1. Гарантировать что Finish ВСЕГДА останавливает программу немедленно
2. Разделить две ситуации: levelCompleted (Finish) vs programStopped (User action)
3. Обновить GameManager чтобы показывать правильный UI для каждого случая
4. Убедиться что CommandExecutor проверяет context.IsCancelled ПЕРЕД выполнением каждой команды
5. Написать integration тесты

---

## 🔧 Детальные шаги реализации

### Шаг 1: Проверить текущую реализацию CommandExecutor
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Execution/CommandExecutor.cs`

**Проверить:**
1. Метод `Stop()` вызывает `context.Cancel()`
2. Метод `ExecuteBlockChain()` проверяет `context.IsCancelled` после каждой команды
3. При `IsCancelled == true` цепь выполнения прерывается

**Текущий код (должен быть):**
```csharp
private IPromise ExecuteBlockChain(BlockUIBase currentBlock, ExecutionContext context)
{
    if (currentBlock == null)
        return Deferred.Resolved();

    // Проверка ПЕРЕД выполнением команды
    if (context.IsCancelled)
    {
        Debug.Log("[CommandExecutor] Execution cancelled, stopping block chain");
        isRunning = false;
        OnProgramStopped?.Invoke();
        return Deferred.Resolved();
    }

    // ... выполнить команду ...

    return command.Execute(robot, context)
        .Then(() =>
        {
            // Проверка ПОСЛЕ выполнения команды
            if (context.IsCancelled)
            {
                isRunning = false;
                OnProgramStopped?.Invoke();
                return Deferred.Resolved();
            }

            // ... выполнить следующую команду ...
        });
}
```

**Если не так:** → Исправить логику

---

### Шаг 2: Обновить GridPositionTracker для приоритета Finish
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`

**Модификация:**
```csharp
private void UpdateGridPosition()
{
    if (!IsInitialized)
        return;

    // Получить текущую grid позицию
    var newGridPos = levelRuntimeManager.GetGridPosition(transform.position);

    // Проверить если позиция совпадает с последней (нет движения)
    if (newGridPos == CurrentGridPosition)
        return;

    // ============ КРИТИЧЕСКАЯ ПРОВЕРКА: FINISH ИМЕЕТ ПРИОРИТЕТ ============
    // Проверить СНАЧАЛА если достигли финиша
    if (!hasReachedFinish && levelRuntimeManager.CurrentLevel != null)
    {
        var finishPoint = levelRuntimeManager.CurrentLevel.GetFinishPoint();
        if (finishPoint != null && newGridPos == finishPoint.position)
        {
            Debug.Log($"[GridPositionTracker] Robot reached finish at {newGridPos}!");
            hasReachedFinish = true;
            LastGridPosition = CurrentGridPosition;
            CurrentGridPosition = newGridPos;

            // Срабатывает OnReachedFinish ПЕРЕД любыми другими проверками
            OnReachedFinish?.Invoke();
            return;  // Выход из функции, остальные проверки не выполняются
        }
    }

    // ============ ДРУГИЕ ПРОВЕРКИ (Wall, Pit, Spike) ============
    // Эти проверки выполняются ТОЛЬКО если финиш НЕ был достигнут

    OnGridPositionChanged?.Invoke(newGridPos, CurrentGridPosition);
    LastGridPosition = CurrentGridPosition;
    CurrentGridPosition = newGridPos;
}
```

**Логирование для debug:**
```csharp
Debug.Log($"[GridPositionTracker] Position changed: {LastGridPosition} → {CurrentGridPosition}");
Debug.Log($"[GridPositionTracker] Current cell reaction: {GetCurrentCellReaction().reactionType}");
```

---

### Шаг 3: Расширить GameManager для разделения событий
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs`

**Добавить флаги и методы:**
```csharp
public class GameManager : MonoBehaviour
{
    // ... существующие поля ...

    private bool levelCompleted = false;      // Достигли финиша
    private bool programStopped = false;      // Остановили вручную (Reset/Clear)

    private GridPositionTracker gridPositionTracker;
    private CommandExecutor commandExecutor;

    public void Initialize()
    {
        // ... существующая инициализация ...

        // Подписаться на события завершения/остановки
        if (gridPositionTracker != null)
        {
            gridPositionTracker.OnReachedFinish -= OnRobotReachedFinish;
            gridPositionTracker.OnReachedFinish += OnRobotReachedFinish;
        }

        if (commandExecutor != null)
        {
            commandExecutor.OnProgramStopped -= OnProgramStoppedByUser;
            commandExecutor.OnProgramStopped += OnProgramStoppedByUser;
        }
    }

    /// <summary>Робот достиг финиша (GridPositionTracker.OnReachedFinish)</summary>
    private void OnRobotReachedFinish()
    {
        if (levelCompleted)  // Защита от повторного срабатывания
            return;

        levelCompleted = true;
        Debug.Log("[GameManager] Level completed! Robot reached finish!");

        // Немедленно остановить программу
        StopProgram();

        // Показать UI сообщение
        UpdateUIMessage("Уровень пройден! 🎉", UIMessageType.Success);

        // Запретить дальнейшие действия
        LockUI();

        // Опционально: переключиться на next level после задержки
        // Timers.Instance.Wait(2f).Done(() => LoadNextLevel());
    }

    /// <summary>Пользователь остановил программу (Reset/Clear кнопки)</summary>
    private void OnProgramStoppedByUser()
    {
        if (programStopped || levelCompleted)  // Уже остановлена или завершена
            return;

        programStopped = true;
        Debug.Log("[GameManager] Program stopped by user");

        // Обновить UI
        UpdateUIMessage("Программа остановлена", UIMessageType.Info);

        // Разрешить редактирование
        UnlockUI();
    }

    /// <summary>Остановить текущую программу</summary>
    public void StopProgram()
    {
        if (commandExecutor != null && commandExecutor.IsRunning)
        {
            commandExecutor.Stop();  // Это вызовет OnProgramStoppedByUser()
        }
    }

    /// <summary>Очистить программу и сбросить уровень</summary>
    public void OnResetButtonClicked()
    {
        Debug.Log("[GameManager] Reset button clicked");

        // Сбросить флаги
        levelCompleted = false;
        programStopped = false;

        // Остановить текущую программу
        StopProgram();

        // Очистить ProgramArea
        ClearProgram();

        // Сбросить позицию робота
        if (robot != null)
            robot.Reset();

        // Сбросить GridPositionTracker
        if (gridPositionTracker != null)
            gridPositionTracker.ResetPosition();

        // Обновить UI
        UpdateUIMessage("Уровень сброшен", UIMessageType.Info);
        UnlockUI();
    }

    /// <summary>Вспомогательный метод для показа UI сообщений</summary>
    private void UpdateUIMessage(string message, UIMessageType type)
    {
        // Реализация зависит от текущей UI системы
        Debug.Log($"[UI] {type}: {message}");

        // Например: messagePanel.text = message;
        //           messagePanel.color = type == UIMessageType.Success ? green : yellow;
    }

    private enum UIMessageType { Info, Success, Warning, Error }

    private void LockUI()
    {
        // Заблокировать BlockPalette, ProgramArea drag
        if (blockPalette != null)
            blockPalette.SetInteractable(false);

        if (programArea != null)
            programArea.SetInteractable(false);
    }

    private void UnlockUI()
    {
        // Разблокировать BlockPalette, ProgramArea drag
        if (blockPalette != null)
            blockPalette.SetInteractable(true);

        if (programArea != null)
            programArea.SetInteractable(true);
    }
}
```

---

### Шаг 4: Проверить OrderOfExecution в GridPositionTracker
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`

**Убедиться что порядок проверок:**
```
1. ✅ Finish (приоритет 1) → STOP программу сразу
2. ❌ Wall (приоритет 2) → Bounce, продолжить
3. ❌ Pit (приоритет 3) → Fall, остановить программу
4. ❌ Spike (приоритет 4) → Break, остановить программу
5. ⚠️ Water/Ice (приоритет 5) → Специальное движение
6. ✅ Floor (приоритет 6) → Нормальное движение
```

**Диаграмма:**
```
OnGridPositionChanged()
  ↓
[Получить новую позицию]
  ↓
[Проверка 1: FINISH?] ← ЕСЛИ ДА → OnReachedFinish() → STOP и ВЫХОД
  ↓ (НЕТ)
[Проверка 2: WALL?] ← ЕСЛИ ДА → BounceReaction (Block 3) → Continue
  ↓ (НЕТ)
[Проверка 3: PIT/SPIKE?] ← ЕСЛИ ДА → FallReaction/BreakReaction (Block 4) → STOP
  ↓ (НЕТ)
[Проверка 4: WATER/ICE?] ← ЕСЛИ ДА → SpeedModification (Block 4)
  ↓ (НЕТ)
[Floor] ← Normal movement
  ↓
Emit OnGridPositionChanged()
```

---

### Шаг 5: Добавить методы-утилиты в GridPositionTracker
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`

```csharp
/// <summary>Проверить если текущая позиция - это финиш</summary>
public bool IsOnFinish()
{
    if (levelRuntimeManager == null || levelRuntimeManager.CurrentLevel == null)
        return false;

    var finishPoint = levelRuntimeManager.CurrentLevel.GetFinishPoint();
    if (finishPoint == null)
        return false;

    return CurrentGridPosition == finishPoint.position;
}

/// <summary>Получить расстояние до финиша</summary>
public float GetDistanceToFinish()
{
    if (levelRuntimeManager == null || levelRuntimeManager.CurrentLevel == null)
        return float.MaxValue;

    var finishPoint = levelRuntimeManager.CurrentLevel.GetFinishPoint();
    if (finishPoint == null)
        return float.MaxValue;

    return Vector2Int.Distance(CurrentGridPosition, finishPoint.position);
}

/// <summary>Проверить если позиция - это Wall</summary>
public bool IsOnWall(Vector2Int gridPos)
{
    if (levelRuntimeManager == null || levelRuntimeManager.CurrentLevel == null)
        return false;

    var level = levelRuntimeManager.CurrentLevel;
    var obj = level.GetObjectAt(gridPos.x, gridPos.y);

    return obj != null && obj.objectTypeId == "Wall";
}

/// <summary>Проверить если позиция - это Pit</summary>
public bool IsOnPit(Vector2Int gridPos)
{
    if (levelRuntimeManager == null || levelRuntimeManager.CurrentLevel == null)
        return false;

    var level = levelRuntimeManager.CurrentLevel;
    var terrain = level.GetTerrainAt(gridPos.x, gridPos.y);

    return terrain != null && terrain.terrainType == "Pit";
}

/// <summary>Проверить если позиция - это Spike</summary>
public bool IsOnSpike(Vector2Int gridPos)
{
    if (levelRuntimeManager == null || levelRuntimeManager.CurrentLevel == null)
        return false;

    var level = levelRuntimeManager.CurrentLevel;
    var terrain = level.GetTerrainAt(gridPos.x, gridPos.y);

    return terrain != null && terrain.terrainType == "Spike";
}
```

---

### Шаг 6: Написать Integration Тесты
**Файл**: `Packages/com.codeblocks.robotprogramming/Tests/Editor/GameManagerFinishTests.cs`

```csharp
using NUnit.Framework;
using UnityEngine;
using CodeBlocks.LevelEditor;
using CodeBlocks.Managers;
using CodeBlocks.Execution;

namespace CodeBlocks.Tests
{
    [TestFixture]
    public class GameManagerFinishTests
    {
        private GameManager gameManager;
        private GridPositionTracker gridPositionTracker;
        private CommandExecutor commandExecutor;
        private LevelRuntimeManager levelRuntimeManager;

        [SetUp]
        public void SetUp()
        {
            // Загрузить тестовый уровень
            var testLevel = Resources.Load<LevelGridData>("Levels/tutorial_01_move_forward");
            Assert.IsNotNull(testLevel, "Test level not found");

            // Создать GameManager
            gameManager = Object.Instantiate(new GameObject()).AddComponent<GameManager>();
            gridPositionTracker = gameManager.gameObject.AddComponent<GridPositionTracker>();
            commandExecutor = gameManager.gameObject.AddComponent<CommandExecutor>();
            levelRuntimeManager = gameManager.gameObject.AddComponent<LevelRuntimeManager>();

            // Инициализировать
            levelRuntimeManager.LoadLevel(testLevel);
            gameManager.Initialize();
        }

        [Test]
        public void TestFinishDetection()
        {
            var finishPoint = levelRuntimeManager.CurrentLevel.GetFinishPoint();
            Assert.IsNotNull(finishPoint, "Finish point not found in test level");

            // Переместить робота на финиш
            var finishWorldPos = levelRuntimeManager.GetWorldPosition(finishPoint.position);
            var robot = gameManager.GetComponent<RobotController>();
            robot.SetStartPosition(finishWorldPos, Quaternion.identity);

            // Обновить позицию
            gridPositionTracker.Initialize(levelRuntimeManager, levelRuntimeManager.CurrentLevel);

            // Проверить что финиш был достигнут
            Assert.IsTrue(gridPositionTracker.IsOnFinish(), "Robot should be on finish");
        }

        [Test]
        public void TestProgramStopsOnFinish()
        {
            // Запустить программу
            var startBlock = CreateTestProgram();
            commandExecutor.ExecuteProgramFromBlock(startBlock, robot);

            // Переместить робота на финиш (имитация)
            gridPositionTracker.OnReachedFinish?.Invoke();

            // Проверить что программа остановлена
            Assert.IsFalse(commandExecutor.IsRunning, "Program should stop when reaching finish");
        }

        [Test]
        public void TestLevelCompletedVsProgramStopped()
        {
            // Сценарий 1: Достигли финиша → levelCompleted = true
            gridPositionTracker.OnReachedFinish?.Invoke();
            Assert.IsTrue(gameManager.IsLevelCompleted(), "Level should be marked as completed");

            // Сценарий 2: Пользователь нажал Reset → programStopped = true, но не levelCompleted
            gameManager.OnResetButtonClicked();
            Assert.IsFalse(gameManager.IsLevelCompleted(), "Level should be reset");
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(gameManager.gameObject);
        }
    }
}
```

---

### Шаг 7: Обновить ProgramArea для UI блокировки
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/UI/ProgramArea.cs`

**Добавить методы:**
```csharp
public class ProgramArea : MonoBehaviour, IDropHandler
{
    // ... существующие методы ...

    private bool isInteractable = true;

    public void SetInteractable(bool value)
    {
        isInteractable = value;

        // Блокировать drag операции
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.blocksRaycasts = value;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!isInteractable)
            return;

        // ... существующая логика ...
    }
}
```

---

## ✅ Acceptance Criteria (Блок 2)

- [ ] GridPositionTracker проверяет Finish с приоритетом 1 (ПЕРЕД другими проверками)
- [ ] При достижении Finish вызывается GridPositionTracker.OnReachedFinish()
- [ ] GameManager.OnRobotReachedFinish() сразу вызывает StopProgram()
- [ ] CommandExecutor.Stop() устанавливает context.IsCancelled = true
- [ ] CommandExecutor проверяет IsCancelled ПЕРЕД выполнением каждой команды
- [ ] Если IsCancelled = true, выполнение цепи прерывается немедленно
- [ ] Даже если в очереди есть команды, они НЕ выполняются после Finish
- [ ] Различие между levelCompleted (Finish) и programStopped (User action) работает
- [ ] OnResetButtonClicked() корректно сбрасывает оба флага
- [ ] ProgramArea блокируется при выполнении программы
- [ ] BlockPalette блокируется при выполнении программы
- [ ] UI сообщения правильно показываются для разных сценариев
- [ ] Все integration тесты проходят
- [ ] Код скомпилирован, 0 errors, console чистая

---

## 🔍 Debug & Testing в Unity

1. **Создать уровень с кратким путём до финиша**
   - Разместить Start и Finish близко (2-3 клетки)

2. **Создать программу из 5+ команд**
   - Первые 3 команды приводят робота на финиш
   - Последние 2 команды НЕ должны выполниться

3. **Запустить программу**
   - Прочитать логи:
     ```
     [GridPositionTracker] Robot reached finish at (5, 5)!
     [GameManager] Level completed! Robot reached finish!
     [CommandExecutor] Execution cancelled, stopping block chain
     ```

4. **Проверить UI:**
   - Должна появиться надпись "Уровень пройден! 🎉"
   - BlockPalette и ProgramArea должны быть заблокированы

---

## 🔗 Переход к следующему блоку

После завершения Блока 2, переходим к **Блоку 3: Wall Collision** → `.Doc/Tasks/25_Block3_WallCollision.md`

Блок 2 гарантирует что Finish работает правильно. Блоки 3-5 добавляют остальные типы ловушек и реакции.
