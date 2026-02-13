# Task #14 Phase 4: Prefabs & Real Visualization

**Status**: Planning
**Depends On**: #14 Phase 1-3 (done)
**Timeline**: Optional enhancement

---

## Описание

Добавить реальную визуализацию уровня с GameObjects вместо только Gizmos. Это позволит:
- Видеть 3D модели terrain/объектов
- Проще понимать как выглядит готовый уровень
- Быстрее итерировать дизайн

---

## Архитектура

### Два режима визуализации:

1. **Gizmos-only mode** (текущий)
   - Быстро
   - Работает без создания объектов
   - Хорошо для быстрого редактирования

2. **Prefabs mode** (новый)
   - Создаёт реальные GameObjects
   - Красивая 3D визуализация
   - Можно добавлять эффекты/анимации

---

## Компоненты для реализации

### 1. TerrainBlockVisual.cs (prefab script)
```csharp
public class TerrainBlockVisual : MonoBehaviour
{
    public Vector2Int gridPosition;
    public string terrainType;

    public void SetTerrain(Vector2Int pos, string type)
    {
        gridPosition = pos;
        terrainType = type;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        // Менять материал по типу terrain
    }
}
```

### 2. ObjectBlockVisual.cs (prefab script)
```csharp
public class ObjectBlockVisual : MonoBehaviour
{
    public Vector2Int gridPosition;
    public string objectTypeId;
    public string objectInstanceId;

    public void SetObject(GridObject data)
    {
        gridPosition = data.position;
        objectTypeId = data.objectTypeId;
        objectInstanceId = data.objectInstanceId;
        UpdateVisuals();
    }
}
```

### 3. LevelVisualizationManager.cs (управление визуализацией)
```csharp
public class LevelVisualizationManager : MonoBehaviour
{
    public bool usePrefabs = true;

    private Dictionary<Vector2Int, GameObject> terrainVisuals;
    private Dictionary<Vector2Int, GameObject> objectVisuals;

    public void RebuildVisualization(LevelGridData levelData)
    {
        ClearVisualization();

        if (usePrefabs)
            RebuildWithPrefabs(levelData);
        else
            OnDrawGizmos(); // Gizmos-only
    }

    private void RebuildWithPrefabs(LevelGridData levelData)
    {
        foreach (var cell in levelData.terrain)
        {
            var obj = Instantiate(GetTerrainPrefab(cell.terrainType));
            obj.GetComponent<TerrainBlockVisual>().SetTerrain(cell.position, cell.terrainType);
            terrainVisuals[cell.position] = obj;
        }

        foreach (var obj in levelData.objects)
        {
            var visual = Instantiate(GetObjectPrefab(obj.objectTypeId));
            visual.GetComponent<ObjectBlockVisual>().SetObject(obj);
            objectVisuals[obj.position] = visual;
        }
    }

    public void PlaceTerrainVisual(Vector2Int pos, string terrainType)
    {
        if (usePrefabs)
        {
            var visual = Instantiate(GetTerrainPrefab(terrainType));
            visual.GetComponent<TerrainBlockVisual>().SetTerrain(pos, terrainType);
            terrainVisuals[pos] = visual;
        }
    }

    public void RemoveTerrainVisual(Vector2Int pos)
    {
        if (usePrefabs && terrainVisuals.TryGetValue(pos, out var obj))
        {
            DestroyImmediate(obj);
            terrainVisuals.Remove(pos);
        }
    }

    private GameObject GetTerrainPrefab(string terrainType)
    {
        return Resources.Load<GameObject>($"CodeBlocks/Terrain/{terrainType}");
    }

    private GameObject GetObjectPrefab(string objectTypeId)
    {
        return Resources.Load<GameObject>($"CodeBlocks/Objects/{objectTypeId}");
    }
}
```

### 4. GridVisualizer.cs (интеграция)
```csharp
// Добавить в PlaceTerrain:
public void PlaceTerrain(Vector2Int position, string terrainType)
{
    // Существующий код...

    // Добавить визуализацию
    if (visualizationManager != null)
        visualizationManager.PlaceTerrainVisual(position, terrainType);
}
```

---

## Структура Prefabs & Resources

```
Assets/Resources/CodeBlocks/
├── Terrain/
│   ├── Ground.prefab
│   ├── Road.prefab
│   └── Pit.prefab
├── Objects/
│   ├── Wall.prefab
│   ├── Button.prefab
│   ├── Door.prefab
│   └── DestructibleWall.prefab
└── Editor/
    ├── GridCell.prefab      (для редактора)
    └── VisualizerRoot.prefab
```

---

## Шаги реализации

### День 1: Архитектура
- [ ] Создать LevelVisualizationManager.cs
- [ ] Добавить toggle "usePrefabs" в GridVisualizer
- [ ] Интегрировать с PlaceTerrain/RemoveTerrain

### День 2: Prefabs
- [ ] Создать простые prefabs для Terrain (Ground, Road, Pit)
- [ ] Создать простые prefabs для Objects (Wall, Button, Door)
- [ ] Добавить TerrainBlockVisual.cs в prefabs
- [ ] Добавить ObjectBlockVisual.cs в prefabs

### День 3: Интеграция & Тестирование
- [ ] Подключить LevelVisualizationManager к GridVisualizer
- [ ] Тестировать создание/удаление визуальных объектов
- [ ] Оптимизировать производительность (pooling)
- [ ] Добавить поддержку обоих режимов

---

## Критерии готовности

- ✅ Toggle "usePrefabs" в Inspector
- ✅ Prefabs создаются при PlaceTerrain
- ✅ Prefabs удаляются при RemoveTerrain
- ✅ Визуализация синхронна с данными
- ✅ Можно быстро переключать между режимами
- ✅ Производительность нормальная (100+ блоков)

---

## Альтернативные подходы

**Вариант A**: Использовать одинаковые визуальные подходы
- Pros: простая реализация
- Cons: может быть скучно

**Вариант B**: Стилизованная 3D визуализация
- Pros: красиво
- Cons: сложнее создавать prefabs

**Вариант C**: Material-based coloring (текущий + цвета)
- Pros: быстро
- Cons: не очень красиво

---

## Заметки

- LevelVisualizationManager должна быть отдельным компонентом (можно сделать как ScriptableObject)
- Pooling будет полезен когда много создания/удаления
- Можно добавить гриды/сетку как отдельный визуальный слой
