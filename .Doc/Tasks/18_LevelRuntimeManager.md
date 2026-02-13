# Task #18: LevelRuntimeManager - Загрузка уровней в Play режиме

**Status:** Pending
**Priority:** 🔴 CRITICAL
**Estimated Time:** 2-3 часа
**Created:** 2026-01-21

---

## SMART Критерии

- **S (Specific):** Создать компонент LevelRuntimeManager для инстанцирования уровней из LevelGridData в Play режиме
- **M (Measurable):** Уровень отображается в сцене, все префабы (terrain, objects, start/finish визуалы) корректно позиционированы
- **A (Achievable):** Переиспользует логику из LevelVisualizationManager, только для Runtime вместо Editor
- **R (Relevant):** Базовая интеграция Level Editor с игрой
- **T (Time-bound):** 2-3 часа чистой работы

---

## Цель
Создать систему загрузки уровней в Play режиме, которая инстанцирует префабы terrain и objects из LevelGridData и предоставляет API для преобразования координат Grid ↔ World.

---

## Контекст

**Текущая ситуация:**
- LevelVisualizationManager работает только в Editor режиме
- Нужна Runtime версия для Play режима
- Координатная система: Grid (x, y) → World (x, 0, z)
- cellSize = 1.0f (совпадает с robot moveDistance)

**Файлы для изучения:**
- `Runtime/LevelEditor/LevelVisualizationManager.cs` (референс)
- `Runtime/LevelEditor/LevelGridData.cs` (data model)
- `Runtime/Robot/RobotConfig.cs` (moveDistance = 1f)

---

## Ключевые шаги

### **Шаг 1: Создать LevelRuntimeManager.cs**
**Файл:** `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManager.cs`

**Измеримый результат:** Файл создан, компилируется без ошибок

**Код:**
```csharp
using UnityEngine;
using System.Collections.Generic;

namespace CodeBlocks.Managers
{
    public class LevelRuntimeManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 levelOrigin = Vector3.zero;

        [Header("Visuals")]
        [SerializeField] private GameObject backgroundPrefab; // Optional

        private LevelGridData currentLevel;
        private GameObject levelContainer;
        private GameObject backgroundInstance;
        private Dictionary<Vector2Int, GameObject> terrainInstances = new Dictionary<Vector2Int, GameObject>();
        private Dictionary<Vector2Int, GameObject> objectInstances = new Dictionary<Vector2Int, GameObject>();
        private GameObject startVisual;
        private GameObject finishVisual;

        public LevelGridData CurrentLevel => currentLevel;
        public float CellSize => cellSize;
        public Vector3 LevelOrigin => levelOrigin;

        // Public API - TODO: implement methods
        public void LoadLevel(LevelGridData levelData) { }
        public void ClearLevel() { }
        public Vector3 GetWorldPosition(Vector2Int gridPos) { return Vector3.zero; }
        public Vector2Int GetGridPosition(Vector3 worldPos) { return Vector2Int.zero; }
    }
}
```

### **Шаг 2: Реализовать координатное преобразование**
**Файл:** `LevelRuntimeManager.cs`

**Измеримый результат:**
- GetWorldPosition корректно преобразует Grid → World
- GetGridPosition корректно преобразует World → Grid
- Unit test: `GetGridPosition(GetWorldPosition(pos)) == pos`

**Код:**
```csharp
public Vector3 GetWorldPosition(Vector2Int gridPos)
{
    return levelOrigin + new Vector3(
        gridPos.x * cellSize,
        0,
        gridPos.y * cellSize
    );
}

public Vector2Int GetGridPosition(Vector3 worldPos)
{
    Vector3 localPos = worldPos - levelOrigin;
    return new Vector2Int(
        Mathf.FloorToInt(localPos.x / cellSize),
        Mathf.FloorToInt(localPos.z / cellSize)
    );
}
```

### **Шаг 3: Реализовать LoadLevel() - Инициализация**
**Файл:** `LevelRuntimeManager.cs`

**Измеримый результат:**
- ClearLevel() удаляет предыдущий уровень
- Создаётся контейнер "LevelRuntime"
- currentLevel сохраняется

**Код:**
```csharp
public void LoadLevel(LevelGridData levelData)
{
    if (levelData == null)
    {
        Debug.LogError("LevelRuntimeManager: Cannot load null level data!");
        return;
    }

    ClearLevel();

    currentLevel = levelData;

    // Create container
    levelContainer = new GameObject("LevelRuntime");
    levelContainer.transform.SetParent(transform);
    levelContainer.transform.position = levelOrigin;

    // Calculate level center for proper origin
    float centerX = (currentLevel.gridWidth - 1) * cellSize * 0.5f;
    float centerZ = (currentLevel.gridHeight - 1) * cellSize * 0.5f;
    levelOrigin = new Vector3(-centerX, 0, -centerZ); // Center level at (0,0,0)

    // Load components in next steps...
}

public void ClearLevel()
{
    if (levelContainer != null)
    {
        Destroy(levelContainer);
        levelContainer = null;
    }

    if (backgroundInstance != null)
    {
        Destroy(backgroundInstance);
        backgroundInstance = null;
    }

    terrainInstances.Clear();
    objectInstances.Clear();
    startVisual = null;
    finishVisual = null;
    currentLevel = null;
}
```

### **Шаг 4: Реализовать LoadLevel() - Background (опционально)**
**Файл:** `LevelRuntimeManager.cs`

**Измеримый результат:**
- Если backgroundPrefab назначен → инстанцируется фон
- Фон масштабируется под размер уровня + отступы
- Фон позиционирован в центре уровня

**Код:**
```csharp
public void LoadLevel(LevelGridData levelData)
{
    // ... existing initialization code ...

    // Create background (optional)
    if (backgroundPrefab != null)
    {
        backgroundInstance = Instantiate(backgroundPrefab, transform);
        backgroundInstance.name = "LevelBackground";

        float width = currentLevel.gridWidth * cellSize;
        float height = currentLevel.gridHeight * cellSize;
        backgroundInstance.transform.localScale = new Vector3(width + 4, 1, height + 4);
        backgroundInstance.transform.position = new Vector3(0, -0.1f, 0); // Slightly below level
    }

    // Load terrain and objects in next step...
}
```

### **Шаг 5: Реализовать LoadLevel() - Terrain и Objects**
**Файл:** `LevelRuntimeManager.cs`

**Измеримый результат:**
- Все terrain cells из levelData инстанцируются
- Все objects из levelData инстанцируются
- Позиционирование корректное (центр клетки)
- Префабы загружаются из Resources

**Код:**
```csharp
public void LoadLevel(LevelGridData levelData)
{
    // ... existing code ...

    // Load terrain
    foreach (var cell in currentLevel.terrain)
    {
        InstantiateTerrain(cell.position, cell.terrainType);
    }

    // Load objects
    foreach (var obj in currentLevel.objects)
    {
        InstantiateObject(obj.position, obj.objectTypeId);
    }

    // Load start/finish visuals in next step...
}

private void InstantiateTerrain(Vector2Int gridPos, string terrainType)
{
    GameObject prefab = Resources.Load<GameObject>($"LevelEditor/Terrain/{terrainType}");
    if (prefab == null)
    {
        Debug.LogWarning($"LevelRuntimeManager: Terrain prefab not found: {terrainType}");
        return;
    }

    GameObject instance = Instantiate(prefab, levelContainer.transform);
    instance.name = $"{terrainType}_{gridPos.x}_{gridPos.y}";

    Vector3 worldPos = GetWorldPosition(gridPos);
    worldPos.x += cellSize * 0.5f; // Center of cell
    worldPos.z += cellSize * 0.5f;
    instance.transform.position = worldPos;

    terrainInstances[gridPos] = instance;
}

private void InstantiateObject(Vector2Int gridPos, string objectTypeId)
{
    GameObject prefab = Resources.Load<GameObject>($"LevelEditor/Objects/{objectTypeId}");
    if (prefab == null)
    {
        Debug.LogWarning($"LevelRuntimeManager: Object prefab not found: {objectTypeId}");
        return;
    }

    GameObject instance = Instantiate(prefab, levelContainer.transform);
    instance.name = $"{objectTypeId}_{gridPos.x}_{gridPos.y}";

    Vector3 worldPos = GetWorldPosition(gridPos);
    worldPos.x += cellSize * 0.5f; // Center of cell
    worldPos.z += cellSize * 0.5f;
    instance.transform.position = worldPos;

    objectInstances[gridPos] = instance;
}
```

### **Шаг 6: Реализовать LoadLevel() - Start/Finish визуалы (опционально)**
**Файл:** `LevelRuntimeManager.cs`

**Измеримый результат:**
- В start point появляется визуальный маркер (стрелка/флаг)
- В finish point появляется визуальный маркер (флаг/портал)
- Правильное направление для start marker

**Код:**
```csharp
public void LoadLevel(LevelGridData levelData)
{
    // ... existing code ...

    // Load start visual
    if (currentLevel.start != null)
    {
        InstantiateStartVisual(currentLevel.start.position, currentLevel.start.direction);
    }

    // Load finish visual
    if (currentLevel.finish != null)
    {
        InstantiateFinishVisual(currentLevel.finish.position);
    }

    Debug.Log($"LevelRuntimeManager: Level '{currentLevel.levelName}' loaded successfully!");
}

private void InstantiateStartVisual(Vector2Int gridPos, CardinalDirection direction)
{
    // Try to load custom start marker prefab
    GameObject prefab = Resources.Load<GameObject>("LevelEditor/Markers/StartPoint");
    if (prefab == null)
    {
        // Fallback: Create simple arrow
        startVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        startVisual.transform.localScale = new Vector3(0.3f, 0.1f, 0.3f);
        startVisual.GetComponent<Renderer>().material.color = Color.green;
    }
    else
    {
        startVisual = Instantiate(prefab, levelContainer.transform);
    }

    startVisual.name = "StartVisual";
    Vector3 worldPos = GetWorldPosition(gridPos);
    worldPos.x += cellSize * 0.5f;
    worldPos.z += cellSize * 0.5f;
    worldPos.y = 0.1f; // Slightly above ground
    startVisual.transform.position = worldPos;

    // Rotate based on direction
    float angle = direction switch
    {
        CardinalDirection.North => 0f,
        CardinalDirection.East => 90f,
        CardinalDirection.South => 180f,
        CardinalDirection.West => 270f,
        _ => 0f
    };
    startVisual.transform.rotation = Quaternion.Euler(0, angle, 0);
}

private void InstantiateFinishVisual(Vector2Int gridPos)
{
    // Try to load custom finish marker prefab
    GameObject prefab = Resources.Load<GameObject>("LevelEditor/Markers/FinishPoint");
    if (prefab == null)
    {
        // Fallback: Create simple flag
        finishVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finishVisual.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        finishVisual.GetComponent<Renderer>().material.color = Color.yellow;
    }
    else
    {
        finishVisual = Instantiate(prefab, levelContainer.transform);
    }

    finishVisual.name = "FinishVisual";
    Vector3 worldPos = GetWorldPosition(gridPos);
    worldPos.x += cellSize * 0.5f;
    worldPos.z += cellSize * 0.5f;
    worldPos.y = 0.25f; // Half height above ground
    finishVisual.transform.position = worldPos;
}
```

### **Шаг 7: Добавить Debug Gizmos**
**Файл:** `LevelRuntimeManager.cs`

**Измеримый результат:**
- В Scene View видны границы уровня (wireframe cube)
- Видны линии сетки
- Видны start (зелёная) и finish (жёлтая) точки

**Код:**
```csharp
private void OnDrawGizmos()
{
    if (currentLevel == null) return;

    // Draw grid bounds
    Gizmos.color = Color.cyan;
    float width = currentLevel.gridWidth * cellSize;
    float height = currentLevel.gridHeight * cellSize;
    Vector3 center = levelOrigin + new Vector3(width * 0.5f, 0, height * 0.5f);
    Gizmos.DrawWireCube(center, new Vector3(width, 0.1f, height));

    // Draw grid lines
    Gizmos.color = new Color(1, 1, 1, 0.2f);
    for (int x = 0; x <= currentLevel.gridWidth; x++)
    {
        Vector3 start = GetWorldPosition(new Vector2Int(x, 0));
        Vector3 end = GetWorldPosition(new Vector2Int(x, currentLevel.gridHeight));
        Gizmos.DrawLine(start, end);
    }
    for (int y = 0; y <= currentLevel.gridHeight; y++)
    {
        Vector3 start = GetWorldPosition(new Vector2Int(0, y));
        Vector3 end = GetWorldPosition(new Vector2Int(currentLevel.gridWidth, y));
        Gizmos.DrawLine(start, end);
    }

    // Draw start point
    if (currentLevel.start != null)
    {
        Gizmos.color = Color.green;
        Vector3 startPos = GetWorldPosition(currentLevel.start.position);
        startPos.x += cellSize * 0.5f;
        startPos.z += cellSize * 0.5f;
        Gizmos.DrawWireSphere(startPos, 0.3f);
    }

    // Draw finish point
    if (currentLevel.finish != null)
    {
        Gizmos.color = Color.yellow;
        Vector3 finishPos = GetWorldPosition(currentLevel.finish.position);
        finishPos.x += cellSize * 0.5f;
        finishPos.z += cellSize * 0.5f;
        Gizmos.DrawWireSphere(finishPos, 0.3f);
    }
}
```

### **Шаг 8: Протестировать в пустой сцене**
**Измеримый результат:**
- Создать тестовую сцену "LevelTest"
- Добавить GameObject с LevelRuntimeManager
- Через Inspector вызвать LoadLevel (tutorial_01)
- Проверить: уровень отобразился, координаты корректны

**Шаги тестирования:**
1. Создать тестовый скрипт `LevelRuntimeManagerTest.cs`:
```csharp
using UnityEngine;
using CodeBlocks.Managers;

public class LevelRuntimeManagerTest : MonoBehaviour
{
    [SerializeField] private LevelRuntimeManager levelManager;
    [SerializeField] private LevelGridData testLevel;

    private void Start()
    {
        if (levelManager != null && testLevel != null)
        {
            levelManager.LoadLevel(testLevel);
            TestCoordinateConversion();
        }
    }

    private void TestCoordinateConversion()
    {
        // Test Grid → World → Grid
        Vector2Int originalGrid = new Vector2Int(3, 4);
        Vector3 worldPos = levelManager.GetWorldPosition(originalGrid);
        Vector2Int convertedGrid = levelManager.GetGridPosition(worldPos);

        Debug.Log($"Test: Grid {originalGrid} → World {worldPos} → Grid {convertedGrid}");
        Debug.Assert(originalGrid == convertedGrid, "Coordinate conversion failed!");

        // Test World → Grid → World
        Vector3 originalWorld = new Vector3(2.5f, 0, 3.5f);
        Vector2Int gridPos = levelManager.GetGridPosition(originalWorld);
        Vector3 convertedWorld = levelManager.GetWorldPosition(gridPos);

        Debug.Log($"Test: World {originalWorld} → Grid {gridPos} → World {convertedWorld}");
        Debug.Log($"Expected grid: (2, 3), Actual: {gridPos}");
    }
}
```

2. Запустить Play режим
3. Проверить Console: нет ошибок, координаты корректны
4. Проверить Scene View: уровень отображается, Gizmos видны

---

## Acceptance Criteria

- [x] Файл `LevelRuntimeManager.cs` создан в `Runtime/Managers/`
- [x] Компиляция без ошибок
- [x] Метод `GetWorldPosition()` корректно преобразует Grid → World
- [x] Метод `GetGridPosition()` корректно преобразует World → Grid
- [x] Метод `LoadLevel()` инстанцирует все terrain префабы
- [x] Метод `LoadLevel()` инстанцирует все object префабы
- [x] Метод `ClearLevel()` удаляет все инстансы
- [x] Префабы позиционированы в центре клеток
- [x] Background префаб инстанцируется и масштабируется (если назначен)
- [x] Start/Finish визуалы отображаются (опционально)
- [x] Debug Gizmos показывают границы и сетку уровня
- [x] Тестовая сцена загружает tutorial_01 без ошибок
- [x] Координатное преобразование проходит unit test (Grid → World → Grid == Grid)

---

## Blockers & Risks

**Blockers:** None

**Risks:**
1. **Префабы не найдены в Resources:**
   - Решение: Warning в Console, fallback на примитивы (Cube/Cylinder)
2. **Координатное смещение:**
   - Решение: Debug Gizmos для визуальной проверки
3. **Центрирование уровня:**
   - Решение: Вычислять levelOrigin динамически в LoadLevel()

---

## Notes

### Координатная система
```
Grid → World:
  (x, y) → (x * cellSize, 0, y * cellSize) + levelOrigin

World → Grid:
  (x, y, z) → (Floor((x - originX) / cellSize), Floor((z - originZ) / cellSize))
```

### Resources структура
```
Assets/CodeBlocks/Resources/
├── LevelEditor/
│   ├── Terrain/
│   │   ├── Ground.prefab
│   │   ├── Road.prefab
│   │   └── Pit.prefab
│   ├── Objects/
│   │   ├── Wall.prefab
│   │   └── Button.prefab
│   └── Markers/
│       ├── StartPoint.prefab (optional)
│       └── FinishPoint.prefab (optional)
```

---

## Следующие задачи
- **#19:** Robot Grid Integration - Позиционирование робота на уровне
- **#20:** GridPositionTracker - Отслеживание положения робота
- **#21:** Finish Detection - Определение достижения финиша
