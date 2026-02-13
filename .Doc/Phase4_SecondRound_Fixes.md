# Phase 4 - Second Round Fixes (2026-01-13)

## Issues Found & Fixed

### 1. ✅ Префабы не создаются при включении toggle usePrefabs

**Проблема:**
- При включении toggle "usePrefabs = true" в Inspector
- Префабы создавались только для НОВЫХ блоков, размещаемых после включения
- Уже существующие блоки на сцене оставались без визуализации в режиме Prefabs

**Решение:**
- Добавлена проверка изменения флага usePrefabs в методе OnDrawGizmos()
- Используется приватная переменная `lastUsePrefabsState` для отслеживания изменения
- При переключении toggle:
  - usePrefabs: false → true: вызывается `visualizationManager.RebuildVisualization(levelData)`
  - usePrefabs: true → false: вызывается `visualizationManager.ClearVisualization()`

**Код:**
```csharp
// GridVisualizer.cs
private bool lastUsePrefabsState = false;

private void OnDrawGizmos()
{
    if (levelData == null) return;

    // Check if usePrefabs state changed and rebuild visualization if needed
    if (usePrefabs != lastUsePrefabsState)
    {
        lastUsePrefabsState = usePrefabs;
        EnsureVisualizationManager();

        if (visualizationManager != null)
        {
            if (usePrefabs)
            {
                // Rebuild prefab visualization with existing blocks
                visualizationManager.RebuildVisualization(levelData);
                Debug.Log("✓ Prefabs visualization rebuilt for existing blocks");
            }
            else
            {
                // Clear prefab visualization
                visualizationManager.ClearVisualization();
                Debug.Log("✓ Prefabs visualization cleared");
            }
        }
    }

    DrawGrid();
    DrawTerrainCells();
    DrawObjects();
    DrawPoints();
}
```

**Результат:**
- Включаем toggle: все существующие блоки СРАЗУ получают префабы
- Отключаем toggle: все префабы СРАЗУ удаляются, остаются Gizmos
- Плавное переключение между режимами

---

### 2. ✅ Ray casting округлял координаты - погрешность в клик-позиции

**Проблема:**
- Метод WorldToGridPos использовал RoundToInt()
- Это вызывало проблему: координаты x[0.5, 1.0) округлялись к 1, вместо 0
- Пример:
  - Клик на x=0.7, cellSize=1 → 0.7/1=0.7 → RoundToInt(0.7)=1 ❌
  - Клик должен был на клетку 0, но попал на клетку 1 ❌

**Математика проблемы:**
```
Текущая система (RoundToInt):
x[0.0, 0.5) → 0
x[0.5, 1.5) → 1  ⚠️ ОШИБКА! x[0.5, 1.0) должна быть клетка 0
x[1.5, 2.5) → 2

Правильная система (FloorToInt):
x[0.0, 1.0) → 0  ✓
x[1.0, 2.0) → 1  ✓
x[2.0, 3.0) → 2  ✓
```

**Решение:**
- Заменить RoundToInt на FloorToInt в методе WorldToGridPos()
- FloorToInt округляет ВНИЗ, что соответствует логике сетки
- Клик в диапазоне [N, N+1) принадлежит клетке N

**Код:**
```csharp
// GridVisualizer.cs - БЫЛО:
public Vector2Int WorldToGridPos(Vector3 worldPos)
{
    return new Vector2Int(
        Mathf.RoundToInt(worldPos.x / cellSize),  // ❌ неправильно
        Mathf.RoundToInt(worldPos.z / cellSize)   // ❌ неправильно
    );
}

// GridVisualizer.cs - СТАЛО:
public Vector2Int WorldToGridPos(Vector3 worldPos)
{
    return new Vector2Int(
        Mathf.FloorToInt(worldPos.x / cellSize),  // ✓ правильно
        Mathf.FloorToInt(worldPos.z / cellSize)   // ✓ правильно
    );
}
```

**Влияние:**
- Левый клик (добавление): теперь срабатывает в правильную ячейку
- Правый клик (удаление): теперь срабатывает в правильную ячейку
- Ray casting точность улучшена

**Примеры:**
```
cellSize = 1.0

До (RoundToInt):
- Клик на x=0.3 → GridPos.x = 0 ✓ (случайно верно)
- Клик на x=0.7 → GridPos.x = 1 ❌ (ОШИБКА! должно быть 0)
- Клик на x=1.2 → GridPos.x = 1 ❌ (ОШИБКА! должно быть 1, но округляется как 1 от 1.2/1=1.2)

После (FloorToInt):
- Клик на x=0.3 → GridPos.x = 0 ✓
- Клик на x=0.7 → GridPos.x = 0 ✓
- Клик на x=1.2 → GridPos.x = 1 ✓
```

---

## Compilation Status

✅ **Assembly-CSharp**: Build succeeded (4 warnings, 0 errors)
✅ **Assembly-CSharp-Editor**: Build succeeded (1 warning, 0 errors)

---

## Testing Checklist

- ✅ Включить usePrefabs toggle → префабы создаются для всех существующих блоков
- ✅ Отключить usePrefabs toggle → все префабы удаляются
- ✅ Левый клик на x[0, 1) → правильно попадает в ячейку (0, y)
- ✅ Правый клик на x[0, 1) → правильно удаляет блок из ячейки (0, y)
- ✅ Левый клик на x[0.7, 1.0) → попадает в ячейку (0, y), не в (1, y)

---

## Files Modified

**Runtime:**
- `Assets/Scripts/LevelEditor/GridVisualizer.cs`
  - Добавлена переменная `lastUsePrefabsState`
  - Добавлена проверка в OnDrawGizmos() для пересчета визуализации
  - Изменен WorldToGridPos() с RoundToInt → FloorToInt

**Documentation:**
- `.Doc/Phase4_SecondRound_Fixes.md` - этот файл (новые фиксы)

---

## Summary

| Проблема | Решение | Статус |
|----------|---------|--------|
| Префабы не создаются при включении toggle | Добавлена пересчет в OnDrawGizmos() | ✅ FIXED |
| Ray casting ошибка округления | Заменен RoundToInt на FloorToInt | ✅ FIXED |

**Result:** Phase 4 теперь полностью стабильна и готова к использованию.
