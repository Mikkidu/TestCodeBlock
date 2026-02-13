# Task #20: GridPositionTracker - Отслеживание положения робота

**Status:** Pending
**Priority:** 🔴 CRITICAL
**Estimated Time:** 2 часа
**Depends On:** #18 (LevelRuntimeManager), #19 (Robot Grid Integration)
**Created:** 2026-01-21

---

## SMART Критерии

- **S (Specific):** Создать компонент GridPositionTracker, который отслеживает на какой grid-клетке находится робот после каждого движения
- **M (Measurable):** Event OnGridPositionChanged срабатывает после движения, текущая grid-позиция корректна (визуально и по координатам)
- **A (Achievable):** Один компонент + интеграция в GameManager
- **R (Relevant):** Необходимо для детекции финиша (#21) и будущих ловушек/преград
- **T (Time-bound):** 2 часа чистой работы

---

## Цель
Отслеживать положение робота на сетке в реальном времени, генерировать события при изменении позиции, валидировать точность позиционирования.

---

## Контекст

**Текущая ситуация:**
- Робот движется через RobotController (Vector3 world space)
- LevelRuntimeManager преобразует Grid ↔ World
- Нет системы отслеживания текущей grid-позиции робота
- Нужно знать на какой клетке робот, чтобы детектить финиш/ловушки

**Что нужно:**
- Компонент на роботе отслеживает текущую grid-позицию
- После каждого движения проверяется новая позиция
- Events: OnGridPositionChanged, OnMovedToImpassableTerrain (для будущего)
- Валидация точности (робот точно на клетке)

---

## Ключевые шаги

### **Шаг 1: Создать GridPositionTracker.cs**
**Файл:** `Runtime/Robot/GridPositionTracker.cs`

**Измеримый результат:**
- Файл создан, компилируется без ошибок
- Класс наследует MonoBehaviour
- Namespace: CodeBlocks.Robot

**Код:**
```csharp
using UnityEngine;
using System;
using CodeBlocks.Managers;

namespace CodeBlocks.Robot
{
    /// <summary>
    /// Tracks robot's position on the level grid.
    /// Fires events when robot moves to a new cell.
    /// </summary>
    [RequireComponent(typeof(RobotController))]
    public class GridPositionTracker : MonoBehaviour
    {
        private LevelRuntimeManager levelManager;
        private LevelGridData currentLevel;
        private RobotController robotController;

        private Vector2Int currentGridPosition;
        private Vector2Int lastGridPosition;
        private bool isInitialized = false;

        // Public properties
        public Vector2Int CurrentGridPosition => currentGridPosition;
        public Vector2Int LastGridPosition => lastGridPosition;
        public bool IsInitialized => isInitialized;

        // Events
        public event Action<Vector2Int, Vector2Int> OnGridPositionChanged; // (newPos, oldPos)
        public event Action<Vector2Int> OnMovedToImpassableTerrain;

        // Methods - TODO: implement
        private void Awake() { }
        public void Initialize(LevelRuntimeManager manager, LevelGridData level) { }
        private void LateUpdate() { }
        private void UpdateGridPosition() { }
        public void ResetPosition() { }
        public bool IsOnGrid() { return false; }
        public float GetDistanceFromGrid() { return 0f; }
    }
}
```

### **Шаг 2: Реализовать Awake() и Initialize()**
**Файл:** `GridPositionTracker.cs`

**Измеримый результат:**
- Awake() находит RobotController
- Initialize() сохраняет ссылки и вычисляет начальную позицию
- isInitialized флаг корректно устанавливается

**Код:**
```csharp
private void Awake()
{
    robotController = GetComponent<RobotController>();
    if (robotController == null)
    {
        Debug.LogError("GridPositionTracker: RobotController component not found!");
    }
}

public void Initialize(LevelRuntimeManager manager, LevelGridData level)
{
    if (manager == null)
    {
        Debug.LogError("GridPositionTracker: LevelRuntimeManager is null!");
        return;
    }

    if (level == null)
    {
        Debug.LogError("GridPositionTracker: LevelGridData is null!");
        return;
    }

    levelManager = manager;
    currentLevel = level;
    isInitialized = true;

    // Calculate initial position
    UpdateGridPosition();

    Debug.Log($"GridPositionTracker: Initialized at grid position {currentGridPosition}");
}
```

### **Шаг 3: Реализовать UpdateGridPosition()**
**Файл:** `GridPositionTracker.cs`

**Измеримый результат:**
- Метод преобразует текущую world-позицию робота в grid-позицию
- Если позиция изменилась → генерируется event OnGridPositionChanged
- Проверяется проходимость terrain (OnMovedToImpassableTerrain)

**Код:**
```csharp
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

        // Check terrain passability (for future trap/pit detection)
        if (currentLevel != null && !currentLevel.IsPassable(currentGridPosition.x, currentGridPosition.y))
        {
            OnMovedToImpassableTerrain?.Invoke(currentGridPosition);
            Debug.LogWarning($"GridPositionTracker: Robot moved to impassable terrain at {currentGridPosition}!");
        }
    }
}
```

### **Шаг 4: Реализовать LateUpdate()**
**Файл:** `GridPositionTracker.cs`

**Измеримый результат:**
- LateUpdate() вызывает UpdateGridPosition() только когда робот НЕ выполняет движение
- Проверка выполняется каждый кадр после завершения анимации

**Код:**
```csharp
private void LateUpdate()
{
    if (!isInitialized) return;

    // Only update position when robot is not executing movement
    // This prevents multiple triggers during lerp animation
    if (robotController != null && !robotController.IsExecuting)
    {
        UpdateGridPosition();
    }
}
```

### **Шаг 5: Реализовать IsOnGrid() и GetDistanceFromGrid()**
**Файл:** `GridPositionTracker.cs`

**Измеримый результат:**
- IsOnGrid() возвращает true если робот точно на клетке (tolerance 0.1f)
- GetDistanceFromGrid() возвращает расстояние до центра текущей клетки
- Методы используются для валидации точности позиционирования

**Код:**
```csharp
/// <summary>
/// Checks if robot is precisely positioned on the grid (within tolerance).
/// </summary>
/// <returns>True if robot is within 0.1 units of cell center</returns>
public bool IsOnGrid()
{
    if (!isInitialized || levelManager == null) return false;

    float distance = GetDistanceFromGrid();
    return distance < 0.1f; // 10cm tolerance
}

/// <summary>
/// Calculates distance from robot to the center of current grid cell.
/// </summary>
/// <returns>Distance in world units</returns>
public float GetDistanceFromGrid()
{
    if (!isInitialized || levelManager == null) return float.MaxValue;

    // Get expected world position (center of cell)
    Vector3 expectedWorldPos = levelManager.GetWorldPosition(currentGridPosition);
    expectedWorldPos.x += levelManager.CellSize * 0.5f;
    expectedWorldPos.z += levelManager.CellSize * 0.5f;
    expectedWorldPos.y = transform.position.y; // Ignore height

    // Calculate distance
    return Vector3.Distance(transform.position, expectedWorldPos);
}
```

### **Шаг 6: Реализовать ResetPosition()**
**Файл:** `GridPositionTracker.cs`

**Измеримый результат:**
- ResetPosition() пересчитывает текущую позицию немедленно
- Используется после teleport (Reset button)

**Код:**
```csharp
/// <summary>
/// Forces immediate position update. Call after teleporting robot.
/// </summary>
public void ResetPosition()
{
    if (!isInitialized) return;

    lastGridPosition = currentGridPosition;
    UpdateGridPosition();

    Debug.Log($"GridPositionTracker: Position reset to {currentGridPosition}");
}
```

### **Шаг 7: Добавить Debug Gizmos**
**Файл:** `GridPositionTracker.cs`

**Измеримый результат:**
- В Scene View видна текущая grid-клетка (зелёный квадрат)
- Видна стрелка направления движения (от lastGridPosition к currentGridPosition)
- Видно расстояние до центра клетки (красная линия если > 0.1f)

**Код:**
```csharp
private void OnDrawGizmos()
{
    if (!isInitialized || levelManager == null) return;

    // Draw current grid cell
    Gizmos.color = new Color(0, 1, 0, 0.3f); // Green transparent
    Vector3 cellCenter = levelManager.GetWorldPosition(currentGridPosition);
    cellCenter.x += levelManager.CellSize * 0.5f;
    cellCenter.z += levelManager.CellSize * 0.5f;
    cellCenter.y = 0.01f; // Slightly above ground
    Gizmos.DrawCube(cellCenter, new Vector3(levelManager.CellSize * 0.9f, 0.02f, levelManager.CellSize * 0.9f));

    // Draw arrow from last to current position (if moved)
    if (lastGridPosition != currentGridPosition)
    {
        Gizmos.color = Color.blue;
        Vector3 lastCellCenter = levelManager.GetWorldPosition(lastGridPosition);
        lastCellCenter.x += levelManager.CellSize * 0.5f;
        lastCellCenter.z += levelManager.CellSize * 0.5f;
        lastCellCenter.y = 0.5f;

        Vector3 currCellCenter = cellCenter;
        currCellCenter.y = 0.5f;

        Gizmos.DrawLine(lastCellCenter, currCellCenter);
        // Draw arrowhead
        Vector3 direction = (currCellCenter - lastCellCenter).normalized;
        Vector3 right = Vector3.Cross(direction, Vector3.up) * 0.2f;
        Gizmos.DrawLine(currCellCenter, currCellCenter - direction * 0.3f + right);
        Gizmos.DrawLine(currCellCenter, currCellCenter - direction * 0.3f - right);
    }

    // Draw distance to grid center (if not on grid)
    if (!IsOnGrid())
    {
        Gizmos.color = Color.red;
        Vector3 robotPos = transform.position;
        robotPos.y = 0.5f;
        cellCenter.y = 0.5f;
        Gizmos.DrawLine(robotPos, cellCenter);
    }

    // Draw position labels in Editor
    #if UNITY_EDITOR
    UnityEditor.Handles.Label(cellCenter + Vector3.up * 0.5f, $"Grid: {currentGridPosition}\nDist: {GetDistanceFromGrid():F3}");
    #endif
}
```

### **Шаг 8: Интегрировать в GameManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- GameManager автоматически добавляет GridPositionTracker к роботу
- Initialize() вызывается после LoadLevel()
- События подписаны в Awake(), отписаны в OnDestroy()

**Код:**
```csharp
private GridPositionTracker robotPositionTracker;

private void Awake()
{
    // ... existing code ...

    // Setup GridPositionTracker
    if (robotController != null)
    {
        robotPositionTracker = robotController.GetComponent<GridPositionTracker>();
        if (robotPositionTracker == null)
        {
            robotPositionTracker = robotController.gameObject.AddComponent<GridPositionTracker>();
            Debug.Log("GameManager: Added GridPositionTracker to robot");
        }

        // Subscribe to events
        robotPositionTracker.OnGridPositionChanged += OnRobotGridPositionChanged;
        robotPositionTracker.OnMovedToImpassableTerrain += OnRobotMovedToImpassable;
    }
}

private void OnDestroy()
{
    // ... existing code ...

    // Unsubscribe from GridPositionTracker events
    if (robotPositionTracker != null)
    {
        robotPositionTracker.OnGridPositionChanged -= OnRobotGridPositionChanged;
        robotPositionTracker.OnMovedToImpassableTerrain -= OnRobotMovedToImpassable;
    }
}

public void LoadLevel(LevelGridData level)
{
    // ... existing code ...

    // Initialize position tracker
    if (robotPositionTracker != null)
    {
        robotPositionTracker.Initialize(levelRuntimeManager, level);
    }

    Debug.Log($"GameManager: Level '{level.levelName}' loaded successfully!");
}

// Event handlers
private void OnRobotGridPositionChanged(Vector2Int newPos, Vector2Int oldPos)
{
    Debug.Log($"GameManager: Robot moved from {oldPos} to {newPos}");

    // Validate positioning accuracy
    if (robotPositionTracker != null && !robotPositionTracker.IsOnGrid())
    {
        Debug.LogWarning($"GameManager: Robot is not precisely on grid! Distance: {robotPositionTracker.GetDistanceFromGrid():F3}");
    }
}

private void OnRobotMovedToImpassable(Vector2Int gridPos)
{
    Debug.LogWarning($"GameManager: ⚠️ Robot moved to impassable terrain at {gridPos}");
    // TODO: Handle game over, restart level, etc. (future task)
}
```

### **Шаг 9: Обновить OnResetButtonClicked() - ResetPosition**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- После Reset вызывается tracker.ResetPosition()
- Grid-позиция обновляется немедленно после телепорта

**Код:**
```csharp
private void OnResetButtonClicked()
{
    // ... existing code (stop, reset robot) ...

    // Update grid position tracker
    if (robotPositionTracker != null)
    {
        robotPositionTracker.ResetPosition();
    }

    isProgramRunning = false;
    currentExecutingCommand = null;
    UpdateStatusDisplay("Сброс завершен");

    Debug.Log("GameManager: Reset complete - robot returned to start point");
}
```

### **Шаг 10: Протестировать отслеживание позиции**
**Измеримый результат:**
- Открыть SampleScene с загруженным уровнем (tutorial_01)
- Создать программу: MoveForward × 3
- Нажать Run
- Проверить Console: события OnGridPositionChanged логируются
- Проверить Scene View: зелёный квадрат перемещается вместе с роботом

**Шаги тестирования:**
1. Запустить Play режим (уровень tutorial_01 загружен)
2. **Проверка #1:** Console: "GridPositionTracker: Initialized at grid position (X, Y)"
3. Создать программу: MoveForward × 3
4. Нажать Run
5. **Проверка #2:** После каждого движения Console: "Robot moved from (X1, Y1) to (X2, Y2)"
6. **Проверка #3:** В Scene View зелёный квадрат перемещается вместе с роботом
7. **Проверка #4:** Синяя стрелка показывает направление последнего движения
8. **Проверка #5:** Нет красной линии (робот точно на клетке)
9. **Проверка #6:** Console: НЕТ warning "Robot is not precisely on grid"
10. Нажать Reset
11. **Проверка #7:** Console: "Position reset to (startX, startY)"
12. **Проверка #8:** Зелёный квадрат вернулся в стартовую клетку

---

## Acceptance Criteria

- [x] Файл `GridPositionTracker.cs` создан в `Runtime/Robot/`
- [x] Компиляция без ошибок
- [x] Метод `Initialize()` сохраняет ссылки и вычисляет начальную позицию
- [x] Метод `UpdateGridPosition()` преобразует World → Grid
- [x] Event `OnGridPositionChanged` срабатывает после движения
- [x] Event `OnMovedToImpassableTerrain` срабатывает при движении на Pit (опционально)
- [x] Метод `IsOnGrid()` корректно проверяет точность (tolerance 0.1f)
- [x] Метод `GetDistanceFromGrid()` возвращает расстояние до центра клетки
- [x] Метод `ResetPosition()` пересчитывает позицию после телепорта
- [x] Debug Gizmos показывают текущую клетку (зелёный квадрат)
- [x] Debug Gizmos показывают направление движения (синяя стрелка)
- [x] Debug Gizmos показывают неточность позиционирования (красная линия)
- [x] GameManager автоматически добавляет компонент к роботу
- [x] GameManager вызывает Initialize() после LoadLevel()
- [x] GameManager логирует все события позиционирования
- [x] После движения робот точно на клетке (IsOnGrid() == true)
- [x] После Reset grid-позиция корректно обновляется

---

## Blockers & Risks

**Blockers:**
- #18 (LevelRuntimeManager) - нужен для GetGridPosition()
- #19 (Robot Grid Integration) - нужен для инициализации

**Risks:**
1. **Множественные срабатывания события во время lerp:**
   - Решение: Проверять только когда IsExecuting == false (в LateUpdate)
2. **Неточность позиционирования:**
   - Решение: IsOnGrid() с tolerance 0.1f, Warning в Console
3. **Производительность LateUpdate:**
   - Решение: Минимальные вычисления, выход если !isInitialized

---

## Notes

### Timing диаграмма
```
Frame N: Robot starts MoveForward
  ↓
RobotController.IsExecuting = true
  ↓
Frames N+1...M: Lerp animation (position changes each frame)
  ↓
GridPositionTracker.LateUpdate() → SKIP (IsExecuting == true)
  ↓
Frame M: Animation complete
  ↓
RobotController.IsExecuting = false
  ↓
Frame M+1:
  ↓
GridPositionTracker.LateUpdate() → UpdateGridPosition()
  ↓
newGridPos != currentGridPosition → OnGridPositionChanged event
  ↓
GameManager.OnRobotGridPositionChanged() → Log, check IsOnGrid()
```

### Events параметры
```csharp
OnGridPositionChanged(Vector2Int newPos, Vector2Int oldPos)
  - newPos: новая grid-позиция
  - oldPos: предыдущая grid-позиция
  - Пример: (3, 4) → (3, 5) при MoveForward на North

OnMovedToImpassableTerrain(Vector2Int gridPos)
  - gridPos: позиция непроходимой клетки
  - Срабатывает когда IsPassable == false
  - Используется для будущей обработки ловушек/пропастей
```

---

## Следующие задачи
- **#21:** Finish Detection - Определение достижения финиша (использует OnGridPositionChanged)
