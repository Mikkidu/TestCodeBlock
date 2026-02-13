# Task #25 (REVISED): Унификация StartPoint/FinishPoint как обычных объектов

**Status:** TODO
**Priority:** 🔴 CRITICAL
**Estimated Time:** 4-5 hours
**Created:** 2026-01-28

## Motivation

**Текущая проблема:**
- StartPoint/FinishPoint обрабатываются как специальные случаи с отдельными полями и методами
- Дублирование кода: `InstantiateStartVisual()`, `InstantiateFinishVisual()` vs `InstantiateObject()`
- Баги дублирования маркеров (CreatePrimitive без SetParent)
- Несогласованность: обычные объекты (Wall, Door) vs специальные (Start, Finish)

**Целевое решение:**
- StartPoint и FinishPoint становятся обычными `GridObject` в массиве `objects[]`
- objectTypeId: `"StartPoint"`, `"FinishPoint"`
- Единый метод спавна: `InstantiateObject()` для всех типов объектов
- Сохранение функциональности: возврат робота на старт, событие победы при финише
- Gizmos для дебага сохраняются

## Current Architecture Analysis

### Affected Files
1. **LevelGridData.cs** (lines 18-19)
   - Отдельные поля: `StartPoint start`, `FinishPoint finish`
   - Нужно: унифицировать в `objects[]`

2. **GridObject.cs** (lines 10)
   - `Dictionary<string, string> parameters` - может хранить direction для StartPoint

3. **StartPoint.cs / FinishPoint.cs**
   - Классы нужны только для Inspector в Level Editor
   - В runtime будут GridObject с objectTypeId="StartPoint"/"FinishPoint"

4. **LevelRuntimeManager.cs** (lines 134-191)
   - Специальные методы: `InstantiateStartVisual()`, `InstantiateFinishVisual()`
   - Нужно: удалить, использовать `InstantiateObject()`

5. **GridPositionTracker.cs** (line 103)
   - `currentLevel.finish.position == currentGridPosition`
   - Нужно: использовать `GetFinishPoint()` метод

6. **GameManager.cs** (usage of start.position/direction)
   - Позиционирование робота: `currentLevel.start.position`
   - Нужно: использовать `GetStartPoint()` метод

## Target Architecture

### Data Model
```csharp
// GridObject с objectTypeId = "StartPoint"
{
    position = (2, 3),
    objectTypeId = "StartPoint",
    objectInstanceId = "start_001",
    parameters = {
        { "direction", "North" }  // CardinalDirection as string
    }
}

// GridObject с objectTypeId = "FinishPoint"
{
    position = (7, 8),
    objectTypeId = "FinishPoint",
    objectInstanceId = "finish_001",
    parameters = {}
}
```

### API Design
```csharp
// LevelGridData.cs - новые методы
public GridObject GetStartPoint()
{
    foreach (var obj in objects)
        if (obj.objectTypeId == "StartPoint")
            return obj;
    return null;
}

public GridObject GetFinishPoint()
{
    foreach (var obj in objects)
        if (obj.objectTypeId == "FinishPoint")
            return obj;
    return null;
}

public CardinalDirection GetStartDirection()
{
    var start = GetStartPoint();
    if (start?.parameters.TryGetValue("direction", out string dirStr) == true)
        return Enum.Parse<CardinalDirection>(dirStr);
    return CardinalDirection.North; // default
}
```

### Unified Instantiation
```csharp
// LevelRuntimeManager.cs - один метод для всех объектов
private void InstantiateObject(Vector2Int gridPos, string objectTypeId)
{
    GameObject prefab = Resources.Load<GameObject>($"LevelEditor/Objects/{objectTypeId}");
    // ... create instance

    // Special handling для StartPoint/FinishPoint (если нужно)
    if (objectTypeId == "StartPoint") {
        // Store reference for Gizmos
        startVisual = instance;
    } else if (objectTypeId == "FinishPoint") {
        finishVisual = instance;
    }

    objectInstances[gridPos] = instance;
}
```

## Implementation Plan

### ⚠️ ВАЖНО: Backward Compatibility Strategy
Чтобы не сломать существующие уровни, используем **поэтапный подход с поддержкой старого формата**:

**Phase 1-3:** Добавить новые методы, сохранить старые поля (deprecated)
**Phase 4:** Migration существующих уровней
**Phase 5:** Удалить deprecated поля и методы

---

### Phase 1: Подготовка - Extension методы (1 час)

**Step 1.1: Добавить GetStartPoint/GetFinishPoint в LevelGridData**
- **File:** `LevelGridData.cs`
- **Location:** После метода `IsPassable()` (line 56)
- **Add methods:**
  ```csharp
  // =========================
  // NEW: Unified Start/Finish access
  // =========================

  /// <summary>
  /// Finds StartPoint in objects array. Falls back to legacy 'start' field.
  /// </summary>
  public GridObject GetStartPoint()
  {
      // NEW: Search in objects[]
      foreach (var obj in objects)
          if (obj.objectTypeId == "StartPoint")
              return obj;

      // FALLBACK: Legacy start field (for backward compatibility)
      if (start != null)
      {
          // Convert to GridObject on-the-fly
          var legacy = new GridObject
          {
              position = start.position,
              objectTypeId = "StartPoint",
              objectInstanceId = "start_legacy",
              parameters = new Dictionary<string, string>
              {
                  { "direction", start.direction.ToString() }
              }
          };
          return legacy;
      }

      return null;
  }

  /// <summary>
  /// Finds FinishPoint in objects array. Falls back to legacy 'finish' field.
  /// </summary>
  public GridObject GetFinishPoint()
  {
      // NEW: Search in objects[]
      foreach (var obj in objects)
          if (obj.objectTypeId == "FinishPoint")
              return obj;

      // FALLBACK: Legacy finish field
      if (finish != null)
      {
          var legacy = new GridObject
          {
              position = finish.position,
              objectTypeId = "FinishPoint",
              objectInstanceId = "finish_legacy",
              parameters = new Dictionary<string, string>()
          };
          return legacy;
      }

      return null;
  }

  /// <summary>
  /// Gets start direction from StartPoint parameters.
  /// </summary>
  public CardinalDirection GetStartDirection()
  {
      var startObj = GetStartPoint();
      if (startObj?.parameters != null &&
          startObj.parameters.TryGetValue("direction", out string dirStr))
      {
          if (System.Enum.TryParse<CardinalDirection>(dirStr, out var dir))
              return dir;
      }
      return CardinalDirection.North; // default
  }
  ```

**Step 1.2: Mark old fields as Obsolete**
- **File:** `LevelGridData.cs`
- **Lines:** 18-19
- **Change:**
  ```csharp
  [System.Obsolete("Use GetStartPoint() instead. Will be removed in future version.")]
  public StartPoint start;

  [System.Obsolete("Use GetFinishPoint() instead. Will be removed in future version.")]
  public FinishPoint finish;
  ```

**Acceptance Criteria Phase 1:**
- [ ] Код компилируется с warnings (obsolete fields)
- [ ] GetStartPoint() возвращает объект из objects[] или legacy fallback
- [ ] GetFinishPoint() возвращает объект из objects[] или legacy fallback
- [ ] Существующие уровни работают через fallback логику

---

### Phase 2: Обновить потребителей API (1.5 часа)

**Step 2.1: Обновить GridPositionTracker**
- **File:** `GridPositionTracker.cs`
- **Location:** Line 103 (метод `UpdateGridPosition()`)
- **Change:**
  ```csharp
  // OLD (line 103):
  if (currentLevel.finish != null && currentLevel.finish.position == currentGridPosition)

  // NEW:
  var finishObj = currentLevel.GetFinishPoint();
  if (finishObj != null && finishObj.position == currentGridPosition)
  {
      hasReachedFinish = true;
      OnReachedFinish?.Invoke();
      Debug.Log($"GridPositionTracker: 🎉 Reached finish at {currentGridPosition}!");
  }
  ```

**Step 2.2: Обновить GameManager**
- **File:** `GameManager.cs` (нужно найти где используется start.position/direction)
- **Search pattern:** `currentLevel.start`
- **Replace with:** `currentLevel.GetStartPoint()` и `currentLevel.GetStartDirection()`

**Step 2.3: Compile and test with legacy levels**
- [ ] Загрузить существующий уровень (с legacy start/finish полями)
- [ ] Проверить: робот позиционируется правильно
- [ ] Проверить: финиш детектится корректно
- [ ] Console: нет ошибок, только warnings об obsolete

---

### Phase 3: Унифицировать LevelRuntimeManager (1.5 часа)

**Step 3.1: Обновить LoadLevel - использовать GetStartPoint/GetFinishPoint**
- **File:** `LevelRuntimeManager.cs`
- **Location:** Lines 79-89 (LoadLevel method)
- **Change:**
  ```csharp
  // OLD (lines 79-89):
  if (currentLevel.start != null)
  {
      InstantiateStartVisual(currentLevel.start.position, currentLevel.start.direction);
  }

  if (currentLevel.finish != null)
  {
      InstantiateFinishVisual(currentLevel.finish.position);
  }

  // NEW:
  var startObj = currentLevel.GetStartPoint();
  if (startObj != null)
  {
      InstantiateObject(startObj.position, startObj.objectTypeId);

      // Handle direction for StartPoint
      if (startObj.parameters.TryGetValue("direction", out string dirStr))
      {
          if (System.Enum.TryParse<CardinalDirection>(dirStr, out var dir))
          {
              // Rotate visual based on direction
              if (objectInstances.TryGetValue(startObj.position, out GameObject startVisual))
              {
                  float angle = dir switch
                  {
                      CardinalDirection.North => 0f,
                      CardinalDirection.East => 90f,
                      CardinalDirection.South => 180f,
                      CardinalDirection.West => 270f,
                      _ => 0f
                  };
                  startVisual.transform.rotation = Quaternion.Euler(0, angle, 0);
              }
          }
      }
  }

  var finishObj = currentLevel.GetFinishPoint();
  if (finishObj != null)
  {
      InstantiateObject(finishObj.position, finishObj.objectTypeId);
  }
  ```

**Step 3.2: Обновить InstantiateObject для StartPoint/FinishPoint**
- **File:** `LevelRuntimeManager.cs`
- **Location:** Line 114 (метод `InstantiateObject`)
- **Add special handling:**
  ```csharp
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
      worldPos.x += cellSize * 0.5f;
      worldPos.z += cellSize * 0.5f;

      // Special Y positioning for markers
      if (objectTypeId == "StartPoint")
          worldPos.y = 0.1f; // Slightly above ground
      else if (objectTypeId == "FinishPoint")
          worldPos.y = 0.25f; // Half height above ground

      instance.transform.position = worldPos;

      objectInstances[gridPos] = instance;

      // Store references for Gizmos (backward compatibility)
      if (objectTypeId == "StartPoint")
          startVisual = instance;
      else if (objectTypeId == "FinishPoint")
          finishVisual = instance;
  }
  ```

**Step 3.3: Обновить ClearLevel**
- **File:** `LevelRuntimeManager.cs`
- **Location:** Line 193 (метод `ClearLevel`)
- **Change:**
  ```csharp
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

      // References cleared automatically when levelContainer destroyed
      startVisual = null;
      finishVisual = null;

      currentLevel = null;
  }
  ```

**Step 3.4: Mark old methods as Obsolete**
- **File:** `LevelRuntimeManager.cs`
- **Mark:** `InstantiateStartVisual()`, `InstantiateFinishVisual()`
- **Add attribute:**
  ```csharp
  [System.Obsolete("No longer used. StartPoint instantiated via InstantiateObject()")]
  private void InstantiateStartVisual(...) { ... }
  ```

**Step 3.5: Обновить OnDrawGizmos для StartPoint/FinishPoint**
- **File:** `LevelRuntimeManager.cs`
- **Location:** Lines 259-283 (OnDrawGizmos method)
- **Change:**
  ```csharp
  // Draw start point (green sphere + arrow pointing in direction)
  var startObj = currentLevel.GetStartPoint();
  if (startObj != null)
  {
      Gizmos.color = Color.green;
      Vector3 startPos = GetWorldPosition(startObj.position) +
                         new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
      Gizmos.DrawWireSphere(startPos, 0.3f);

      // Draw direction arrow
      CardinalDirection dir = currentLevel.GetStartDirection();
      Vector3 direction = dir switch
      {
          CardinalDirection.North => Vector3.forward,
          CardinalDirection.East => Vector3.right,
          CardinalDirection.South => Vector3.back,
          CardinalDirection.West => Vector3.left,
          _ => Vector3.forward
      };
      Gizmos.DrawLine(startPos, startPos + direction * 0.5f);
  }

  // Draw finish point (yellow sphere)
  var finishObj = currentLevel.GetFinishPoint();
  if (finishObj != null)
  {
      Gizmos.color = Color.yellow;
      Vector3 finishPos = GetWorldPosition(finishObj.position) +
                          new Vector3(cellSize * 0.5f, 0, cellSize * 0.5f);
      Gizmos.DrawWireSphere(finishPos, 0.3f);
  }
  ```

**Acceptance Criteria Phase 3:**
- [ ] StartPoint/FinishPoint создаются через InstantiateObject()
- [ ] Rotation StartPoint работает корректно (direction parameter)
- [ ] ClearLevel удаляет маркеры (через levelContainer.Destroy)
- [ ] Gizmos рисуются правильно (зелёный старт, жёлтый финиш)
- [ ] Существующие уровни (legacy format) работают

---

### Phase 4: Migration существующих уровней (30 min)

**Step 4.1: Создать Migration утилиту**
- **File:** `Editor/LevelMigrationTool.cs` (новый)
- **Menu:** Tools → CodeBlocks → Migrate Levels (Start/Finish)
- **Logic:**
  ```csharp
  [MenuItem("Tools/CodeBlocks/Migrate Levels (Start/Finish)")]
  public static void MigrateLevels()
  {
      var allLevels = Resources.LoadAll<LevelGridData>("RobotLevels");
      int migratedCount = 0;

      foreach (var level in allLevels)
      {
          bool changed = false;
          var objectsList = new List<GridObject>(level.objects);

          // Migrate StartPoint
          if (level.start != null && level.GetStartPoint()?.objectInstanceId != "start_legacy")
          {
              var startObj = new GridObject
              {
                  position = level.start.position,
                  objectTypeId = "StartPoint",
                  objectInstanceId = $"start_{level.levelId}",
                  parameters = new Dictionary<string, string>
                  {
                      { "direction", level.start.direction.ToString() }
                  }
              };
              objectsList.Add(startObj);
              level.start = null; // Clear legacy field
              changed = true;
          }

          // Migrate FinishPoint
          if (level.finish != null && level.GetFinishPoint()?.objectInstanceId != "finish_legacy")
          {
              var finishObj = new GridObject
              {
                  position = level.finish.position,
                  objectTypeId = "FinishPoint",
                  objectInstanceId = $"finish_{level.levelId}"
              };
              objectsList.Add(finishObj);
              level.finish = null; // Clear legacy field
              changed = true;
          }

          if (changed)
          {
              level.objects = objectsList.ToArray();
              EditorUtility.SetDirty(level);
              migratedCount++;
          }
      }

      AssetDatabase.SaveAssets();
      Debug.Log($"Migrated {migratedCount} levels to unified Start/Finish format.");
  }
  ```

**Step 4.2: Run migration на всех уровнях**
- [ ] Tools → CodeBlocks → Migrate Levels (Start/Finish)
- [ ] Проверить Assets/Resources/RobotLevels/ - все 5 уровней
- [ ] Проверить в Inspector: objects[] содержит StartPoint и FinishPoint
- [ ] Проверить: legacy поля start/finish = null

**Step 4.3: Test migrated levels**
- [ ] Загрузить все 5 уровней по очереди
- [ ] Проверить: робот на старте, финиш детектится
- [ ] Проверить: визуалы правильно отображаются

---

### Phase 5: Cleanup - Удалить deprecated код (30 min)

**Step 5.1: Удалить obsolete поля из LevelGridData**
- **File:** `LevelGridData.cs`
- **Remove:** Lines 18-19 (`public StartPoint start; public FinishPoint finish;`)

**Step 5.2: Удалить obsolete методы из LevelRuntimeManager**
- **File:** `LevelRuntimeManager.cs`
- **Remove:**
  - `InstantiateStartVisual()` (lines 134-167)
  - `InstantiateFinishVisual()` (lines 169-191)

**Step 5.3: Update CHANGELOG**
- **File:** `Packages/com.codeblocks.robotprogramming/CHANGELOG.md`
- **Add entry:**
  ```markdown
  ## [1.1.0] - 2026-01-28

  ### Changed (BREAKING)
  - **StartPoint/FinishPoint унифицированы** как обычные GridObject в objects[]
  - objectTypeId: "StartPoint", "FinishPoint"
  - Direction хранится в parameters["direction"] как string
  - Migration tool: Tools → CodeBlocks → Migrate Levels (Start/Finish)

  ### Removed
  - LevelGridData.start (use GetStartPoint())
  - LevelGridData.finish (use GetFinishPoint())
  - LevelRuntimeManager.InstantiateStartVisual()
  - LevelRuntimeManager.InstantiateFinishVisual()

  ### Fixed
  - Start/Finish markers no longer duplicate on level reload
  - Background positioning fixed (SetParent to levelContainer)
  ```

**Step 5.4: Git commit and tag**
- [ ] Commit: "Refactor: Unify StartPoint/FinishPoint as GridObject (v1.1.0)"
- [ ] Tag: `v1.1.0`
- [ ] Push: `git push --tags`

**Acceptance Criteria Phase 5:**
- [ ] Код компилируется без warnings
- [ ] Все тесты проходят
- [ ] Миграция завершена, legacy поля удалены
- [ ] CHANGELOG обновлён

---

## Level Editor Integration (Future - не в этой задаче)

После завершения Phase 1-5, отдельная задача:
- Обновить GridVisualizer для размещения StartPoint/FinishPoint как objects
- UI для выбора direction при размещении StartPoint
- Validation: только 1 StartPoint, только 1 FinishPoint на уровень

---

## Testing Strategy

### Unit Tests (optional, but recommended)
```csharp
// LevelGridDataTests.cs
[Test]
public void GetStartPoint_ReturnsFromObjects()
{
    var level = CreateTestLevel();
    level.objects = new[] {
        new GridObject {
            objectTypeId = "StartPoint",
            position = new Vector2Int(2, 3),
            parameters = new Dictionary<string, string> { {"direction", "North"} }
        }
    };

    var start = level.GetStartPoint();
    Assert.NotNull(start);
    Assert.AreEqual("StartPoint", start.objectTypeId);
    Assert.AreEqual(new Vector2Int(2, 3), start.position);
}

[Test]
public void GetStartPoint_FallsBackToLegacy()
{
    var level = CreateTestLevel();
    level.start = new StartPoint {
        position = new Vector2Int(5, 6),
        direction = CardinalDirection.East
    };

    var start = level.GetStartPoint();
    Assert.NotNull(start);
    Assert.AreEqual(new Vector2Int(5, 6), start.position);
    Assert.AreEqual("East", start.parameters["direction"]);
}
```

### Manual Testing Checklist
- [ ] Load legacy level (with start/finish fields) → works through fallback
- [ ] Load migrated level (StartPoint/FinishPoint in objects[]) → works directly
- [ ] Robot positioned at StartPoint correctly
- [ ] Robot detects FinishPoint and triggers victory
- [ ] StartPoint rotation matches direction parameter
- [ ] Gizmos draw correctly in Scene view
- [ ] No marker duplication after multiple InitLevel() calls
- [ ] Background positioned correctly (SetParent fix from original #25 BUG-2)

---

## Benefits of Unified Architecture

### Code Simplification
- ❌ **Before:** 2 special methods + 2 standard methods = 4 methods
- ✅ **After:** 1 unified method = 1 method

### Consistency
- ❌ **Before:** Wall/Door = objects[], Start/Finish = special fields
- ✅ **After:** All objects = objects[]

### Bug Prevention
- ❌ **Before:** CreatePrimitive без SetParent → дублирование маркеров
- ✅ **After:** Все через InstantiateObject() с правильным parent

### Future Extensibility
- Easy to add new object types (Trap, Key, Portal)
- Consistent pattern: objectTypeId + parameters
- Level Editor just adds to objects[], no special cases

---

## Rollback Plan

Если что-то сломается:
1. Revert commits до Phase 1
2. Существующие уровни работают (legacy fallback сохранён)
3. Migration tool можно запустить заново

---

## Related Tasks

### After this refactor
- [ ] #25 BUG-2: Background positioning (SetParent fix) - уже включено в Phase 3.1
- [ ] #25 FEATURE-1: Public API methods (StartProgram/StopProgram) - отдельная задача
- [ ] Level Editor update: Place StartPoint/FinishPoint as objects - отдельная задача

---

## Acceptance Criteria (Overall)

- [ ] StartPoint и FinishPoint в objects[] массиве (не отдельные поля)
- [ ] objectTypeId: "StartPoint", "FinishPoint"
- [ ] Direction хранится в parameters["direction"]
- [ ] Unified InstantiateObject() для всех типов объектов
- [ ] Gizmos рисуются правильно (зелёный старт с стрелкой, жёлтый финиш)
- [ ] Робот позиционируется на старте корректно
- [ ] Финиш детектится и вызывает событие победы
- [ ] Нет дублирования маркеров при перезагрузке уровня
- [ ] Все 5 туториальных уровней мигрированы и работают
- [ ] Код компилируется без ошибок/warnings
- [ ] CHANGELOG обновлён (v1.1.0)
- [ ] Migration tool работает корректно

---

## Time Estimate Breakdown

| Phase | Task | Time |
|-------|------|------|
| 1 | Extension methods + Obsolete marks | 1h |
| 2 | Update consumers (GridPositionTracker, GameManager) | 1.5h |
| 3 | Unify LevelRuntimeManager | 1.5h |
| 4 | Migration tool + Run migration | 30min |
| 5 | Cleanup + Git release | 30min |
| **TOTAL** | | **5h** |

---

## Notes

### Dictionary Serialization Caveat
Unity **не сериализует** `Dictionary<string, string>` по умолчанию в ScriptableObject!

**Solution Options:**
1. **Используем SerializableDictionary** (custom wrapper)
2. **Используем List<Parameter>** вместо Dictionary
3. **Используем JSON string** для parameters

**Recommended:** Используем существующий Dictionary в runtime, но для Inspector добавим serialization wrapper.

**File:** `GridObject.cs`
```csharp
[System.Serializable]
public class Parameter
{
    public string key;
    public string value;
}

[System.Serializable]
public class GridObject
{
    public Vector2Int position;
    public string objectTypeId;
    public string objectInstanceId;

    // Serializable format for Inspector
    [SerializeField] private List<Parameter> parametersList = new List<Parameter>();

    // Runtime accessor (lazy init from parametersList)
    private Dictionary<string, string> _parameters;
    public Dictionary<string, string> parameters
    {
        get
        {
            if (_parameters == null)
            {
                _parameters = new Dictionary<string, string>();
                foreach (var p in parametersList)
                    _parameters[p.key] = p.value;
            }
            return _parameters;
        }
    }

    // Helper to sync Dictionary → List (for Editor)
    public void SyncParameters()
    {
        parametersList.Clear();
        foreach (var kvp in parameters)
            parametersList.Add(new Parameter { key = kvp.Key, value = kvp.Value });
    }
}
```

Это нужно сделать в **Phase 1, Step 0** (перед всеми остальными шагами).

---

## Next Steps

1. Обновить GridObject.cs для сериализации Dictionary (Step 0)
2. Начать Phase 1: Extension methods
3. После Phase 3: проверить что background positioning fix (SetParent) включён
4. После Phase 5: создать новую задачу для #25 FEATURE-1 (Public API methods)
