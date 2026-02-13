# Task #19: Robot Grid Integration - Позиционирование робота на уровне

**Status:** Pending
**Priority:** 🔴 CRITICAL
**Estimated Time:** 1-2 часа
**Depends On:** #18 (LevelRuntimeManager)
**Created:** 2026-01-21

---

## SMART Критерии

- **S (Specific):** Связать робота с уровнем - автоматическая установка в start point при загрузке, Reset возвращает на старт
- **M (Measurable):** Робот стоит в правильной grid-клетке с правильным направлением (визуально и по координатам)
- **A (Achievable):** Расширение существующих классов GameManager и RobotController
- **R (Relevant):** Связывает Level Editor с Robot системой
- **T (Time-bound):** 1-2 часа чистой работы

---

## Цель
Интегрировать робота с загруженным уровнем: автоматически устанавливать робота в стартовую позицию при LoadLevel(), возвращать на старт при Reset/Stop.

---

## Контекст

**Текущая ситуация:**
- LevelRuntimeManager (#18) загружает уровень и предоставляет GetWorldPosition()
- RobotController имеет фиксированный startPosition (задан в сцене)
- GameManager не связан с LevelRuntimeManager
- При Reset робот возвращается в старую позицию, а не в start point уровня

**Что нужно:**
- GameManager загружает уровень при Start()
- Робот автоматически ставится в start point уровня
- Reset/Stop возвращают робота на start point уровня

---

## Ключевые шаги

### **Шаг 1: Расширить RobotController - SetStartPosition()**
**Файл:** `Runtime/Robot/RobotController.cs`

**Измеримый результат:**
- Метод `SetStartPosition(Vector3, Quaternion)` добавлен
- startPosition и startRotation обновляются динамически
- Reset() использует новые значения

**Код:**
```csharp
// Add new public method
public void SetStartPosition(Vector3 position, Quaternion rotation)
{
    startPosition = position;
    startRotation = rotation;

    Debug.Log($"RobotController: Start position updated to {position}, rotation {rotation.eulerAngles}");
}
```

**Изменения в Awake():**
```csharp
private void Awake()
{
    if (Timers.Instance == null)
    {
        Debug.LogError("Timers MonoBehaviour not found! Create a GameObject with Timers component.");
        return;
    }

    // Initialize with current transform as fallback
    if (startPosition == Vector3.zero)
    {
        startPosition = transform.position;
    }
    if (startRotation == Quaternion.identity)
    {
        startRotation = transform.rotation;
    }
}
```

### **Шаг 2: Расширить GameManager - Добавить LevelRuntimeManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Поле `levelRuntimeManager` добавлено в Inspector
- Поле `currentLevel` (LevelGridData) добавлено в Inspector
- Awake() находит компонент автоматически

**Код:**
```csharp
[Header("Level Settings")]
[SerializeField] private LevelGridData currentLevel;
[SerializeField] private LevelRuntimeManager levelRuntimeManager;

private void Awake()
{
    // Find LevelRuntimeManager if not assigned
    if (levelRuntimeManager == null)
    {
        levelRuntimeManager = FindObjectOfType<LevelRuntimeManager>();
    }

    // ... existing code ...
}
```

### **Шаг 3: Добавить Start() в GameManager - Автоматическая загрузка уровня**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- При запуске Play режима автоматически загружается currentLevel
- Если currentLevel == null → Warning в Console
- Робот устанавливается в start point

**Код:**
```csharp
private void Start()
{
    if (currentLevel != null)
    {
        LoadLevel(currentLevel);
    }
    else
    {
        Debug.LogWarning("GameManager: No level assigned! Please assign a LevelGridData to 'Current Level' field.");
    }
}
```

### **Шаг 4: Реализовать LoadLevel() в GameManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Метод LoadLevel(LevelGridData) загружает уровень через LevelRuntimeManager
- Вызывает PositionRobotAtStart()
- Debug.Log подтверждает успешную загрузку

**Код:**
```csharp
public void LoadLevel(LevelGridData level)
{
    if (level == null)
    {
        Debug.LogError("GameManager: Cannot load null level!");
        return;
    }

    if (levelRuntimeManager == null)
    {
        Debug.LogError("GameManager: LevelRuntimeManager not found!");
        return;
    }

    // Load level visuals
    levelRuntimeManager.LoadLevel(level);

    // Position robot at start
    PositionRobotAtStart(level);

    Debug.Log($"GameManager: Level '{level.levelName}' loaded successfully!");
}
```

### **Шаг 5: Реализовать PositionRobotAtStart() в GameManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Робот перемещается в start.position (grid coordinates)
- Робот поворачивается в start.direction
- Визуально робот стоит в правильной клетке

**Код:**
```csharp
private void PositionRobotAtStart(LevelGridData level)
{
    if (robotController == null)
    {
        Debug.LogWarning("GameManager: RobotController not found!");
        return;
    }

    if (level.start == null)
    {
        Debug.LogWarning($"GameManager: Level '{level.levelName}' has no start point!");
        return;
    }

    // Convert grid position to world position
    Vector3 worldPos = levelRuntimeManager.GetWorldPosition(level.start.position);

    // Center robot in the cell
    worldPos.x += levelRuntimeManager.CellSize * 0.5f;
    worldPos.z += levelRuntimeManager.CellSize * 0.5f;
    worldPos.y = robotController.transform.position.y; // Preserve height

    // Convert direction to rotation
    Quaternion worldRot = CardinalDirectionToRotation(level.start.direction);

    // Update robot's start position
    robotController.SetStartPosition(worldPos, worldRot);

    // Apply immediately (teleport robot)
    robotController.Reset();

    Debug.Log($"GameManager: Robot positioned at grid {level.start.position}, world {worldPos}, direction {level.start.direction}");
}

private Quaternion CardinalDirectionToRotation(CardinalDirection dir)
{
    float angle = dir switch
    {
        CardinalDirection.North => 0f,
        CardinalDirection.East => 90f,
        CardinalDirection.South => 180f,
        CardinalDirection.West => 270f,
        _ => 0f
    };
    return Quaternion.Euler(0, angle, 0);
}
```

### **Шаг 6: Обновить OnResetButtonClicked() в GameManager**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Reset останавливает выполнение программы
- Reset останавливает все Loop команды
- Reset возвращает робота в start point уровня

**Код:**
```csharp
private void OnResetButtonClicked()
{
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

    // Stop command executor
    if (commandExecutor != null)
    {
        commandExecutor.Stop();
    }

    // Reset robot to start position
    if (robotController != null)
    {
        robotController.Reset(); // Returns to start point set by PositionRobotAtStart()
    }

    isProgramRunning = false;
    currentExecutingCommand = null;
    UpdateStatusDisplay("Сброс завершен");

    Debug.Log("GameManager: Reset complete - robot returned to start point");
}
```

### **Шаг 7: Обновить OnStopButtonClicked() (опционально)**
**Файл:** `Runtime/Managers/GameManager.cs`

**Измеримый результат:**
- Stop останавливает выполнение программы
- Stop НЕ возвращает робота на старт (остаётся где остановился)

**Код (остаётся без изменений):**
```csharp
private void OnStopButtonClicked()
{
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

    if (commandExecutor != null)
    {
        commandExecutor.Stop();
    }

    isProgramRunning = false;
    UpdateStatusDisplay("Остановлено");

    Debug.Log("GameManager: Program stopped - robot stays at current position");
}
```

### **Шаг 8: Протестировать интеграцию**
**Измеримый результат:**
- Открыть SampleScene
- Назначить GameManager → Current Level → tutorial_01
- Назначить GameManager → Level Runtime Manager
- Запустить Play режим
- Проверить: уровень загрузился, робот стоит в стартовой клетке

**Шаги тестирования:**
1. Открыть SampleScene
2. Найти GameObject "GameManager"
3. Inspector → GameManager:
   - Current Level: перетащить `Assets/Resources/RobotLevels/tutorial_01`
   - Level Runtime Manager: перетащить GameObject с компонентом или оставить пустым (auto-find)
4. Запустить Play режим
5. **Проверка #1:** Уровень отображается в Scene/Game View
6. **Проверка #2:** Робот стоит в клетке start point (визуально)
7. **Проверка #3:** Робот направлен согласно start.direction
8. **Проверка #4:** Console: "Level loaded successfully", "Robot positioned at..."
9. Создать простую программу: MoveForward × 2
10. Нажать Run → робот движется
11. **Проверка #5:** Нажать Reset → робот вернулся в start point
12. **Проверка #6:** Направление робота восстановлено

---

## Acceptance Criteria

- [x] Метод `SetStartPosition()` добавлен в RobotController
- [x] GameManager имеет поля `currentLevel` и `levelRuntimeManager`
- [x] Метод `Start()` автоматически загружает currentLevel
- [x] Метод `LoadLevel()` загружает уровень и позиционирует робота
- [x] Метод `PositionRobotAtStart()` корректно преобразует Grid → World
- [x] Метод `CardinalDirectionToRotation()` корректно преобразует направление
- [x] Reset возвращает робота в start point уровня (не старую позицию)
- [x] Stop останавливает программу, робот остаётся на месте
- [x] При запуске Play режима робот стоит в правильной клетке
- [x] При запуске Play режима робот направлен правильно
- [x] После Reset робот возвращается в start point с правильным направлением
- [x] Console логирует все ключевые события (load, position, reset)

---

## Blockers & Risks

**Blockers:**
- #18 (LevelRuntimeManager) должен быть завершён

**Risks:**
1. **Робот не в центре клетки:**
   - Решение: `worldPos += cellSize * 0.5f` для центрирования
2. **Неправильное направление:**
   - Решение: Debug Gizmos для визуализации направления (стрелка)
3. **LevelRuntimeManager не найден:**
   - Решение: FindObjectOfType() в Awake(), Warning если null

---

## Notes

### Координатное преобразование (Start Point)
```
Grid: start.position = (3, 2)
      start.direction = East (90°)

World:
  worldPos = levelOrigin + (3 * cellSize, 0, 2 * cellSize)
  worldPos += (cellSize/2, 0, cellSize/2)  // Center of cell
  worldRot = Quaternion.Euler(0, 90, 0)
```

### Направления
```
CardinalDirection → Y rotation:
- North (0) → 0°
- East (1) → 90°
- South (2) → 180°
- West (3) → 270°
```

### UI Flow
```
Start Play режима:
  ↓
GameManager.Start()
  ↓
LoadLevel(currentLevel)
  ↓
LevelRuntimeManager.LoadLevel() (визуалы)
  ↓
PositionRobotAtStart() (робот)
  ↓
RobotController.SetStartPosition()
  ↓
RobotController.Reset() (применить немедленно)
```

### Button Flow
```
Reset Button:
  ↓
OnResetButtonClicked()
  ↓
Stop Loops + Executor
  ↓
RobotController.Reset()
  ↓
Transform = startPosition/startRotation (уже обновлены LoadLevel)
```

---

## Следующие задачи
- **#20:** GridPositionTracker - Отслеживание положения робота
- **#21:** Finish Detection - Определение достижения финиша
