# Task #13: Level Editor инфраструктура - структуры данных

**Status**: Ready for Implementation
**Priority**: 🔴 CRITICAL
**Timeline**: 12-13 января (1-2 дня)
**Blockers**: None

---

## Описание

Создать структуры данных для редактора уровней с двухслойной архитектурой:
- **Слой 1**: Terrain (Ground, Road, Pit) - определяет проходимость
- **Слой 2**: Objects (Wall, Button, Door и т.д.) - препятствия на terrain
- **Слой 3**: Points (Start, Finish) - начало и конец уровня

---

## Структуры для реализации

### 1. LevelGridData.cs (главный контейнер)

```csharp
[System.Serializable]
public class LevelGridData : ScriptableObject
{
    // Основные параметры
    public string levelId = "level_001";
    public string levelName = "First Steps";
    public int difficulty = 1;
    public string hintText = "";

    // Размер сетки
    public int gridWidth = 8;
    public int gridHeight = 8;

    // Три слоя
    public TerrainCell[] terrain;        // СЛОЙ 1
    public GridObject[] objects;         // СЛОЙ 2
    public StartPoint start;             // СЛОЙ 3
    public FinishPoint finish;           // СЛОЙ 3

    // Визуальный стиль
    public int visualLayerId = 1;

    // Вспомогательные методы
    public TerrainCell GetTerrainAt(int x, int y) { ... }
    public GridObject GetObjectAt(int x, int y) { ... }
    public bool IsPassable(int x, int y) { ... }  // Проверка Ground/Road
}
```

### 2. TerrainCell.cs (основной слой)

```csharp
[System.Serializable]
public class TerrainCell
{
    public Vector2Int position;        // (0,0), (1,2) и т.д.
    public string terrainType;         // "Ground", "Road", "Pit"

    // Дополнительно (опционально)
    // public Sprite visualSprite;     // Спрайт для отображения

    // Свойство для проверки проходимости
    public bool IsPassable => terrainType != "Pit";
}
```

### 3. GridObject.cs (слой препятствий)

```csharp
[System.Serializable]
public class GridObject
{
    public Vector2Int position;
    public string objectTypeId;        // "Wall", "Button", "Door" и т.д.
    public string objectInstanceId;    // Уникальный ID для уничтожимых/связей

    // Параметры для триггеров
    public Dictionary<string, string> parameters;
    // Примеры:
    // {"color": "Red"}              - для Button/Door
    // {"triggerId": "button_red_1"} - для связей

    // Ограничение: объект может быть только на проходимом terrain!
}
```

### 4. StartPoint.cs и FinishPoint.cs

```csharp
[System.Serializable]
public class StartPoint
{
    public Vector2Int position;
    public CardinalDirection direction;  // North/East/South/West
}

[System.Serializable]
public class FinishPoint
{
    public Vector2Int position;
}

public enum CardinalDirection
{
    North = 0,
    East = 1,
    South = 2,
    West = 3
}
```

---

## Шаги реализации

### День 1 (12 янв)

1. **Создать папку и файлы**
   ```
   Assets/Scripts/LevelEditor/
   ├─ LevelGridData.cs
   ├─ TerrainCell.cs
   ├─ GridObject.cs
   ├─ StartPoint.cs
   └─ FinishPoint.cs
   ```

2. **Реализовать LevelGridData**
   - Структура данных
   - Методы доступа: GetTerrainAt(), GetObjectAt()
   - Валидация: IsPassable()

3. **Реализовать TerrainCell**
   - Поля position, terrainType
   - Свойство IsPassable

4. **Реализовать GridObject**
   - Поля position, objectTypeId, objectInstanceId
   - Dictionary<string, string> параметры
   - Примечание про ограничение (только на проходимом terrain)

### День 2 (13 янв)

5. **Реализовать StartPoint и FinishPoint**
   - CardinalDirection enum

6. **Тестирование структур**
   - Создать test сцену для проверки сериализации
   - Убедиться что всё сохраняется в JSON
   - Проверить Serialization warnings

7. **Документация**
   - Добавить комментарии в код
   - Примеры использования

---

## Критерии готовности

- ✅ Все структуры созданы и компилируются
- ✅ LevelGridData сохраняется как ScriptableObject
- ✅ JSON сериализация работает корректно
- ✅ Нет Serialization warnings в Console
- ✅ Методы доступа (GetTerrainAt, IsPassable) работают
- ✅ Dictionary для параметров ObjectGrid работает
- ✅ Примечания и ограничения задокументированы

---

## Зависимости для следующих задач

- #14 (UI) нужны все эти структуры для отображения
- #15 (Tools) нужны эти структуры для JSON экспорта/импорта
- #16 (Examples) нужны эти структуры для создания уровней

---

## Заметки

- Убедиться что все классы Serializable ([System.Serializable])
- Dictionary требует специальной обработки для JSON - будет в #15
- Можно добавить Sprite визуализацию позже (не критично для #13)