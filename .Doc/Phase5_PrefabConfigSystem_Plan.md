# Phase 5 (Optional) - Prefab Configuration System

## Overview

Текущая система использует жесткокодированные пути к префабам:
```csharp
Resources.Load<GameObject>($"CodeBlocks/Terrain/{terrainType}");
Resources.Load<GameObject>($"CodeBlocks/Objects/{objectTypeId}");
```

Это работает, но не гибко. **Phase 5** добавит конфиг-систему для маппинга типов блоков к их префабам.

## Problem

- Префабы жестко привязаны к именам типов (Ground → Assets/Resources/CodeBlocks/Terrain/Ground.prefab)
- Нельзя быстро менять визуальные представления без изменения кода
- Нельзя использовать разные префабы для одного типа в разных уровнях
- Сложно добавлять новые типы блоков

## Solution

Создать **BlockPrefabConfig.cs** - ScriptableObject конфиг, который:

1. Маппит `BlockTypeId` (строка) → `Префаб GameObject`
2. Используется как источник правды для всех префабов
3. Может быть отредактирован в Inspector без изменения кода
4. Кэшируется в памяти для быстрого доступа

## Architecture

### Files to Create

**1. BlockPrefabMapping.cs** (Serializable класс для одной строки маппинга)
```csharp
[System.Serializable]
public class BlockPrefabMapping
{
    public string blockTypeId;        // "Ground", "Wall", "Door", etc.
    public GameObject prefabAsset;    // Префаб из Assets/Resources
    public BlockCategory category;    // Terrain, Object, etc.
}

public enum BlockCategory
{
    Terrain,
    Object,
    Decorator
}
```

**2. BlockPrefabConfig.cs** (ScriptableObject конфиг)
```csharp
[CreateAssetMenu(fileName = "BlockPrefabConfig", menuName = "CodeBlocks/Block Prefab Config")]
public class BlockPrefabConfig : ScriptableObject
{
    public List<BlockPrefabMapping> mappings = new();

    // Кэш для быстрого поиска
    private Dictionary<string, GameObject> cache;

    public GameObject GetPrefab(string blockTypeId)
    {
        // Инициализируем кэш если нужно
        if (cache == null)
            RebuildCache();

        return cache.TryGetValue(blockTypeId, out var prefab) ? prefab : null;
    }

    public void RebuildCache()
    {
        cache = new Dictionary<string, GameObject>();
        foreach (var mapping in mappings)
        {
            if (!string.IsNullOrEmpty(mapping.blockTypeId))
                cache[mapping.blockTypeId] = mapping.prefabAsset;
        }
    }

    // Для редактирования в Editor
    public void AddMapping(string blockTypeId, GameObject prefab, BlockCategory category)
    {
        // Удаляем старую маппинг если существует
        mappings.RemoveAll(m => m.blockTypeId == blockTypeId);

        mappings.Add(new BlockPrefabMapping
        {
            blockTypeId = blockTypeId,
            prefabAsset = prefab,
            category = category
        });

        RebuildCache();
    }
}
```

**3. BlockPrefabManager.cs** (Синглтон для глобального доступа)
```csharp
public class BlockPrefabManager : MonoBehaviour
{
    private static BlockPrefabManager instance;
    private BlockPrefabConfig config;

    public static BlockPrefabManager Instance
    {
        get
        {
            if (instance == null)
                instance = CreateDefaultManager();
            return instance;
        }
    }

    public GameObject GetPrefab(string blockTypeId)
    {
        if (config == null)
            LoadConfig();

        return config?.GetPrefab(blockTypeId);
    }

    private void LoadConfig()
    {
        config = Resources.Load<BlockPrefabConfig>("CodeBlocks/BlockPrefabConfig");
    }

    private static BlockPrefabManager CreateDefaultManager()
    {
        var go = new GameObject("BlockPrefabManager");
        return go.AddComponent<BlockPrefabManager>();
    }
}
```

## Integration Points

### Update LevelVisualizationManager

**Before:**
```csharp
private GameObject GetTerrainPrefab(string terrainType)
{
    return Resources.Load<GameObject>($"CodeBlocks/Terrain/{terrainType}");
}
```

**After:**
```csharp
private GameObject GetTerrainPrefab(string terrainType)
{
    return BlockPrefabManager.Instance.GetPrefab(terrainType);
}
```

### Update PrefabGenerator

Добавить опцию для автоматического создания и регистрации маппингов в конфиге.

## Implementation Steps

### Day 1: Core System
1. Создать BlockPrefabMapping.cs и BlockPrefabConfig.cs
2. Создать BlockPrefabManager.cs синглтон
3. Скомпилировать и протестировать базовый кэш

### Day 2: Integration
4. Обновить LevelVisualizationManager для использования BlockPrefabManager
5. Обновить PrefabGenerator для создания конфига
6. Создать дефолтный BlockPrefabConfig в Resources

### Day 3: Polish & Testing
7. Добавить Editor tool для визуализации маппингов
8. Тестировать с разными префабами
9. Документировать использование

## Advantages

✅ **Гибкость**: Менять префабы можно в Inspector без кода
✅ **Масштабируемость**: Легко добавлять новые типы блоков
✅ **Переиспользуемость**: Один конфиг для всех уровней или разные конфиги для разных уровней
✅ **Отладка**: Видеть все маппинги в Inspector
✅ **Производительность**: Кэширование для быстрого доступа

## Example Usage

```
1. Create → CodeBlocks → Block Prefab Config
2. В Inspector добавить маппинги:
   - blockTypeId: "Ground" → prefabAsset: Ground.prefab
   - blockTypeId: "Wall" → prefabAsset: Wall.prefab
   - и т.д.
3. Все префабы будут автоматически загружаться по ID
```

## Alternative Approaches

**Option A**: Использовать Dictionary в JSON (текущий подход)
- Pros: Просто, человеко-читаемо
- Cons: Нельзя ссылаться на assets напрямую

**Option B**: AssetReference (AddressableAssets)
- Pros: Мощно и гибко
- Cons: Требует AddressableAssets package

**Option C**: Выбранный подход - ScriptableObject List
- Pros: Встроенная сериализация, визуализация в Inspector, кэширование
- Cons: Требует ручного добавления маппингов (но можно автоматизировать)

## Timeline

- **не срочно** - это enhancement, текущая система работает
- Приоритет: LOW (после #15-16)
- Предпоследний шаг перед интеграцией в play-united

## Notes

- Можно реализовать после #15-16, когда основная функциональность будет готова
- BlockPrefabConfig может быть отредактирован в Editor или программно
- Синглтон гарантирует что конфиг загружается один раз
