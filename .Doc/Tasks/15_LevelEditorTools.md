# Task #15: Level Editor Tools - сохранение и загрузка

**Status**: Ready for Implementation
**Priority**: 🟠 HIGH
**Blockers**: #13 (нужны структуры)
**Timeline**: 14-15 января (1-2 дня)

---

## Описание

Реализовать инструменты для сохранения, загрузки и валидации уровней.

### Компоненты
1. **LevelJsonExporter** - экспорт LevelGridData в JSON
2. **LevelJsonImporter** - импорт JSON в LevelGridData
3. **CodeBlocksLevelManager** - загрузка уровня при запуске игры
4. **Validation** - проверка целостности и ошибок

---

## Реализация

### 1. LevelJsonExporter.cs

```csharp
public static class LevelJsonExporter
{
    [MenuItem("Tools/CodeBlocks/Export Level to JSON")]
    public static void ExportLevel()
    {
        // 1. Получить текущую LevelGridData из Selection или Scene
        // 2. Вызвать SaveFileDialog
        // 3. Сериализовать в JSON (обработать Dictionary!)
        // 4. Сохранить в файл
        // 5. Сообщение об успехе в Console
    }

    public static string LevelToJson(LevelGridData level)
    {
        // Использовать JsonUtility или Newtonsoft.Json
        // ВАЖНО: Dictionary<string, string> требует кастомной сериализации
        return jsonString;
    }
}
```

### 2. LevelJsonImporter.cs

```csharp
public static class LevelJsonImporter
{
    [MenuItem("Tools/CodeBlocks/Import Level from JSON")]
    public static void ImportLevel()
    {
        // 1. OpenFileDialog
        // 2. Прочитать JSON файл
        // 3. Десериализовать в LevelGridData
        // 4. Создать ScriptableObject Asset в Assets/Resources/
        // 5. Сообщение об успехе
    }

    public static LevelGridData JsonToLevel(string jsonContent)
    {
        // Десериализация с обработкой Dictionary
        return levelData;
    }
}
```

### 3. CodeBlocksLevelManager.cs

```csharp
public class CodeBlocksLevelManager : MonoBehaviour
{
    public static CodeBlocksLevelManager Instance { get; private set; }

    private LevelGridData currentLevel;

    public void LoadLevel(string levelId)
    {
        // 1. Загрузить LevelGridData из Resources по levelId
        // 2. Установить currentLevel
        // 3. Инициализировать сетку в GameManager
        // 4. Сообщение об успехе
    }

    public LevelGridData GetCurrentLevel() => currentLevel;

    public TerrainCell GetTerrain(int x, int y) => currentLevel.GetTerrainAt(x, y);
    public GridObject GetObject(int x, int y) => currentLevel.GetObjectAt(x, y);
    public bool IsPassable(int x, int y) => currentLevel.IsPassable(x, y);
}
```

### 4. LevelValidator.cs

```csharp
public static class LevelValidator
{
    public static ValidationResult ValidateLevel(LevelGridData level)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        // Проверки:
        // ❌ Start не установлен
        if (level.start == null) errors.Add("Start point not set!");

        // ❌ Finish не установлен
        if (level.finish == null) errors.Add("Finish point not set!");

        // ❌ Objects на Pit
        foreach (var obj in level.objects)
        {
            if (!level.IsPassable(obj.position.x, obj.position.y))
                errors.Add($"Object at {obj.position} placed on Pit!");
        }

        // ⚠️ Уровень пустой
        if (level.terrain.Length == 0)
            warnings.Add("Level has no terrain!");

        return new ValidationResult { Errors = errors, Warnings = warnings };
    }
}

public class ValidationResult
{
    public List<string> Errors { get; set; }
    public List<string> Warnings { get; set; }
    public bool IsValid => Errors.Count == 0;
}
```

---

## JSON Format Пример

```json
{
  "levelId": "level_001",
  "levelName": "First Steps",
  "difficulty": 1,
  "gridWidth": 5,
  "gridHeight": 5,

  "terrain": [
    {"position": {"x": 0, "y": 0}, "terrainType": "Ground"},
    {"position": {"x": 0, "y": 1}, "terrainType": "Ground"},
    {"position": {"x": 1, "y": 1}, "terrainType": "Ground"}
  ],

  "objects": [
    {"position": {"x": 2, "y": 2}, "objectTypeId": "Wall", "objectInstanceId": "wall_1", "parameters": {}}
  ],

  "start": {
    "position": {"x": 0, "y": 0},
    "direction": "East"
  },

  "finish": {
    "position": {"x": 4, "y": 4}
  },

  "visualLayerId": 1
}
```

---

## Шаги реализации

### День 1 (14 янв)

1. **LevelJsonExporter**
   - Обработка Dictionary (кастомная сериализация)
   - SaveFileDialog
   - Тестирование экспорта

2. **LevelJsonImporter**
   - OpenFileDialog
   - Десериализация JSON
   - Создание ScriptableObject Asset

### День 2 (15 янв)

3. **CodeBlocksLevelManager**
   - Интеграция с GameManager
   - Методы доступа

4. **LevelValidator**
   - Все проверки
   - Error/Warning сообщения

5. **Тестирование**
   - Экспорт уровня
   - Импорт уровня
   - Загрузка при запуске
   - Валидация ошибок

---

## Критерии готовности

- ✅ Export уровня в JSON работает
- ✅ Import из JSON работает
- ✅ ScriptableObject создаётся корректно
- ✅ LevelManager загружает уровень при старте
- ✅ Валидация проверяет все ошибки
- ✅ Error/Warning сообщения в Console
- ✅ Меню Tools доступны из Editor

---

## Заметки

- Dictionary<string, string> требует кастомной сериализации для JSON
- Можно использовать `[System.Serializable]` wrapper класс для Dictionary
- ScriptableObject нужно сохранять в Assets/Resources/CodeBlocks/Levels/
- Валидация должна быть достаточно строгой чтобы ловить ошибки ГД
