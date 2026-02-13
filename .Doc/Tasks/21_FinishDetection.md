# Task #21: Finish Detection - Определение достижения финиша

**Status:** Pending
**Priority:** 🔴 CRITICAL
**Estimated Time:** 1 час
**Depends On:** #20 (GridPositionTracker)
**Created:** 2026-01-21

---

## SMART Критерии

- **S (Specific):** При достижении робота finish point показывать UI сообщение "Уровень пройден!" и логировать событие
- **M (Measurable):** UI отображает победное сообщение, Console логирует "Robot reached finish!", программа останавливается
- **A (Achievable):** Обработка event в GameManager + обновление UI
- **R (Relevant):** Завершает игровой цикл уровня
- **T (Time-bound):** 1 час чистой работы

---

## Цель
Детектировать момент достижения роботом финиша и показывать пользователю победное сообщение.

---

## Контекст

**Текущая ситуация:**
- GridPositionTracker (#20) генерирует event OnGridPositionChanged
- LevelGridData содержит finish.position (Vector2Int)
- Нет проверки достижения финиша
- Нет UI feedback при победе

**Что нужно:**
- Добавить event OnReachedFinish в GridPositionTracker
- Проверять текущую позицию == finish.position
- Обрабатывать событие в GameManager
- Показывать UI сообщение "Уровень пройден! 🎉"
- Логировать в Console

---

## Ключевые шаги

### **Шаг 1: Добавить event OnReachedFinish в GridPositionTracker**
**Файл:** `Runtime/Robot/GridPositionTracker.cs`

**Измеримый результат:**
- Event OnReachedFinish добавлен
- Проверка в UpdateGridPosition()
- Event срабатывает ОДИН РАЗ (флаг hasReachedFinish)

**Код:**
```csharp
// Add to existing events section
public event Action OnReachedFinish;

// Add private field
private bool hasReachedFinish = false;

// Update UpdateGridPosition() method
private void UpdateGridPosition()
{
    if (!isInitialized || levelManager == null) return;

    // Get current grid position from world position
    Vector2Int newGridPos = levelManager.GetGridPosition(transform.position);

    // Check if position changed
    if (newGridPos != currentGridPosition)
    {
        lastGridPosition = currentGridPosition;
        currentGridPosition = newGridPos;

        // Fire position changed event
        OnGridPositionChanged?.Invoke(currentGridPosition, lastGridPosition);

        Debug.Log($"GridPositionTracker: Moved from {lastGridPosition} to {currentGridPosition}");

        // Check finish (NEW)
        if (currentLevel != null && !hasReachedFinish)
        {
            if (currentLevel.finish != null && currentLevel.finish.position == currentGridPosition)
            {
                hasReachedFinish = true;
                OnReachedFinish?.Invoke();
                Debug.Log($"GridPositionTracker: 🎉 Reached finish at {currentGridPosition}!");
            }
        }

        // Check terrain passability
        if (currentLevel != null && !currentLevel.IsPassable(currentGridPosition.x, currentGridPosition.y))
        {
            OnMovedToImpassableTerrain?.Invoke(currentGridPosition);
            Debug.LogWarning($"GridPositionTracker: Robot moved to impassable terrain at {currentGridPosition}!");
        }
    }
}

// Add to ResetPosition() method
public void ResetPosition()
{
    if (!isInitialized) return;

    lastGridPosition = currentGridPosition;
    hasReachedFinish = false; // Reset finish flag
    UpdateGridPosition();

    Debug.Log($"GridPositionTracker: Position reset to {currentGridPosition}");
}

// Add to Initialize() method
public void Initialize(LevelRuntimeManager manager, LevelGridData level)
{
    // ... existing code ...

    hasReachedFinish = false; // Reset finish flag on level load

    // Calculate initial position
    UpdateGridPosition();

    Debug.Log($"GridPositionTracker: Initialized at grid position {currentGridPosition}");
}
```

### **Шаг 2: Подписаться на OnReachedFinish в GameManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Awake() подписывается на OnReachedFinish
- OnDestroy() отписывается
- Обработчик OnRobotReachedFinish() вызывается при победе

**Код (уже должно быть из #20):**
```csharp
private void Awake()
{
    // ... existing code ...

    // Subscribe to events
    if (robotPositionTracker != null)
    {
        robotPositionTracker.OnGridPositionChanged += OnRobotGridPositionChanged;
        robotPositionTracker.OnMovedToImpassableTerrain += OnRobotMovedToImpassable;
        robotPositionTracker.OnReachedFinish += OnRobotReachedFinish; // NEW
    }
}

private void OnDestroy()
{
    // ... existing code ...

    // Unsubscribe
    if (robotPositionTracker != null)
    {
        robotPositionTracker.OnGridPositionChanged -= OnRobotGridPositionChanged;
        robotPositionTracker.OnMovedToImpassableTerrain -= OnRobotMovedToImpassable;
        robotPositionTracker.OnReachedFinish -= OnRobotReachedFinish; // NEW
    }
}
```

### **Шаг 3: Реализовать OnRobotReachedFinish() в GameManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Программа останавливается (Stop executor)
- UI показывает "Уровень пройден! 🎉"
- Console логирует "Robot reached finish!"
- Подсветка блоков очищается

**Код:**
```csharp
private void OnRobotReachedFinish()
{
    Debug.Log("🎉 GameManager: Robot reached finish!");

    // Stop program execution
    if (commandExecutor != null)
    {
        commandExecutor.Stop();
    }

    // Stop all loop commands
    if (programArea != null)
    {
        List<BlockUIBase> blocks = programArea.GetBlocks();
        foreach (var block in blocks)
        {
            if (block.Command is Commands.LoopCommand loopCmd)
            {
                loopCmd.RequestStop();
            }
        }
    }

    isProgramRunning = false;
    currentExecutingCommand = null;

    // Update UI
    UpdateStatusDisplay("Уровень пройден! 🎉");

    // Clear block highlight
    ClearBlockHighlight();

    // TODO: Show win screen, play victory sound, unlock next level, etc.
}
```

### **Шаг 4: Добавить визуальный эффект (опционально)**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Finish визуал анимируется (пульсация/вращение)
- Робот подсвечивается зелёным цветом
- Звук победы (если есть AudioSource)

**Код (опционально, для будущего улучшения):**
```csharp
private void OnRobotReachedFinish()
{
    // ... existing code ...

    // Visual effects (optional)
    PlayVictoryEffects();
}

private void PlayVictoryEffects()
{
    // Highlight robot in green
    if (robotController != null)
    {
        Renderer robotRenderer = robotController.GetComponent<Renderer>();
        if (robotRenderer != null)
        {
            robotRenderer.material.color = Color.green;
        }
    }

    // Play victory sound (if AudioSource exists)
    AudioSource audioSource = GetComponent<AudioSource>();
    if (audioSource != null && audioSource.clip != null)
    {
        audioSource.Play();
    }

    // TODO: Animate finish visual (rotation, scale pulse)
    // TODO: Show particle effect
}
```

### **Шаг 5: Протестировать finish detection**
**Измеримый результат:**
- Открыть SampleScene с tutorial_01
- Создать программу до финиша (3× MoveForward)
- Нажать Run
- Проверить: UI "Уровень пройден!", Console "Robot reached finish!"

**Шаги тестирования:**
1. Открыть tutorial_01 (Start: (0,0) North, Finish: (0,3))
2. Создать программу: MoveForward × 3
3. Нажать Run
4. **Проверка #1:** Робот движется к финишу
5. **Проверка #2:** При достижении финиша Console: "🎉 Reached finish at (0, 3)"
6. **Проверка #3:** Console: "🎉 GameManager: Robot reached finish!"
7. **Проверка #4:** UI statusText: "Уровень пройден! 🎉"
8. **Проверка #5:** Программа останавливается (не продолжает выполнение)
9. **Проверка #6:** Подсветка блоков очищается
10. Нажать Reset
11. **Проверка #7:** Робот вернулся на старт
12. **Проверка #8:** UI statusText: "Сброс завершен"
13. Повторить программу
14. **Проверка #9:** Finish детектируется повторно (hasReachedFinish сброшен)

### **Шаг 6: Протестировать с разными уровнями**
**Измеримый результат:**
- Протестировать tutorial_02, tutorial_03
- Проверить корректность finish для разных позиций
- Проверить что finish НЕ срабатывает при проходе мимо

**Шаги тестирования:**
1. Загрузить tutorial_02 (L-образный путь)
2. Создать программу: MoveForward × 2, TurnRight, MoveForward × 2
3. Нажать Run
4. **Проверка #1:** Finish детектируется только в конечной точке
5. **Проверка #2:** НЕ детектируется при прохождении рядом с finish

### **Шаг 7: Добавить тесты для edge cases**
**Измеримый результат:**
- Проверить: робот стартует НА финише (finish == start)
- Проверить: робот проходит финиш несколько раз (должен сработать 1 раз)
- Проверить: уровень без finish point (не крашит)

**Шаги тестирования:**
1. **Edge Case #1:** Start == Finish
   - Создать тестовый уровень с start.position == finish.position
   - Запустить Play режим
   - Проверить: finish НЕ срабатывает сразу (только после движения)

2. **Edge Case #2:** Робот проходит финиш несколько раз
   - Создать программу: MoveForward × 3 (к финишу), TurnRight × 2, MoveForward × 3 (от финиша), TurnRight × 2, MoveForward × 3 (к финишу снова)
   - Нажать Run
   - Проверить: finish срабатывает ОДИН РАЗ (hasReachedFinish флаг работает)

3. **Edge Case #3:** Уровень без finish
   - Создать LevelGridData без finish point (finish == null)
   - Запустить Play режим
   - Проверить: нет NullReferenceException, программа выполняется нормально

---

## Acceptance Criteria

- [x] Event `OnReachedFinish` добавлен в GridPositionTracker
- [x] Проверка `currentGridPosition == finish.position` в UpdateGridPosition()
- [x] Флаг `hasReachedFinish` предотвращает множественные срабатывания
- [x] Флаг сбрасывается в ResetPosition() и Initialize()
- [x] GameManager подписан на OnReachedFinish
- [x] Обработчик OnRobotReachedFinish() останавливает программу
- [x] UI statusText отображает "Уровень пройден! 🎉"
- [x] Console логирует "🎉 Robot reached finish!"
- [x] Подсветка блоков очищается при победе
- [x] После Reset finish может сработать повторно
- [x] Finish детектируется только при движении НА finish, не при старте на finish
- [x] Finish срабатывает ОДИН РАЗ за программу (флаг работает)
- [x] Уровень без finish не вызывает ошибок
- [x] Finish детектируется корректно для всех tutorial уровней

---

## Blockers & Risks

**Blockers:**
- #20 (GridPositionTracker) - нужен для event OnGridPositionChanged

**Risks:**
1. **Множественные срабатывания:**
   - Решение: Флаг hasReachedFinish
2. **Срабатывание при старте на finish:**
   - Решение: Проверка только при изменении позиции (в UpdateGridPosition, не в Initialize)
3. **NullReferenceException если finish == null:**
   - Решение: Проверка `currentLevel.finish != null`

---

## Notes

### Порядок событий при достижении финиша
```
Robot executes MoveForward
  ↓
Animation completes (IsExecuting = false)
  ↓
GridPositionTracker.LateUpdate() → UpdateGridPosition()
  ↓
newGridPos == finish.position → OnReachedFinish event
  ↓
GameManager.OnRobotReachedFinish()
  ↓
1. Stop CommandExecutor
2. Stop all Loops
3. Update UI: "Уровень пройден! 🎉"
4. Clear block highlight
5. Console log: "🎉 Robot reached finish!"
```

### Флаг hasReachedFinish
```
Initialize() → hasReachedFinish = false
  ↓
UpdateGridPosition() → check finish → hasReachedFinish = true
  ↓
UpdateGridPosition() → skip check (hasReachedFinish == true)
  ↓
ResetPosition() → hasReachedFinish = false (можно повторить)
```

### Future Enhancements
1. **Animated victory screen:** Full-screen UI panel with "Level Complete", stars, time, score
2. **Victory sound/music:** Play audio clip on finish
3. **Particle effect:** Confetti/fireworks at finish point
4. **Auto-load next level:** Button "Next Level" → load tutorial_02
5. **Level progression system:** Track completed levels, unlock new levels
6. **Statistics:** Count moves, time, blocks used → compare with optimal solution

---

## Связанные задачи
- **#18:** LevelRuntimeManager (DONE) - загрузка уровня
- **#19:** Robot Grid Integration (DONE) - позиционирование робота
- **#20:** GridPositionTracker (DONE) - отслеживание позиции
- **Future:** Level Progression System - автоматическая загрузка следующего уровня
- **Future:** Victory Screen UI - красивый UI при победе
- **Future:** Trap/Pit Detection - проверка IsPassable перед движением
