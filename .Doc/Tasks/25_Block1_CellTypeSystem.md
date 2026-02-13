# Блок 1: Cell Type System - Система типов клеток и конфигурация реакций

**Часть**: Task #25 (Collision System)
**Длительность**: ~2 часа
**Зависимости**: Ничего, самостоятельный блок

---

## 📋 Описание

Создать расширенную систему типов клеток с конфигурируемыми реакциями. Текущая система поддерживает только "Ground" и "Pit", нужно добавить "Spike", "Water", "Ice" и создать гибкую конфигурацию для каждого типа.

**Текущее состояние:**
- ✅ `LevelGridData` имеет `terrain[]` с `TerrainCell.terrainType` (string)
- ✅ `TerrainCell.IsPassable` проверяет только Pit
- ❌ Нет конфигурации для разных типов реакций
- ❌ Нет привязки типов клеток к анимациям

---

## 🎯 Цели

1. Расширить enum типов клеток ("Floor", "Road", "Pit", "Spike", "Water", "Ice")
2. Создать `CellReactionType` enum (Move, Bounce, Fall, Break, Swim)
3. Создать `CellReaction` struct с параметрами анимации
4. Создать `CellReactionConfig` ScriptableObject для конфигурации
5. Добавить методы в `LevelGridData` для получения реакции по типу клетки
6. Создать примеры конфигов в Unity Editor

---

## 🔧 Детальные шаги реализации

### Шаг 1: Создать enum CellReactionType
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionType.cs`

```csharp
namespace CodeBlocks.Collision
{
    /// <summary>
    /// Тип реакции робота при попадании на клетку определённого типа.
    /// </summary>
    public enum CellReactionType
    {
        /// <summary>Нормальное движение по полу</summary>
        Move = 0,

        /// <summary>Откат назад (столкновение со стеной)</summary>
        Bounce = 1,

        /// <summary>Падение в яму (Pit)</summary>
        Fall = 2,

        /// <summary>Поломка при попадании на Spike</summary>
        Break = 3,

        /// <summary>Плавание в воде (замедление)</summary>
        Swim = 4,

        /// <summary>Скольжение по льду (ускорение)</summary>
        Slide = 5,

        /// <summary>Неизвестный тип (ошибка)</summary>
        None = -1
    }
}
```

**Логирование:**
- `Move` → робот проходит нормально, программа продолжается
- `Bounce` → откат назад + программа продолжается
- `Fall` → падение + программа ОСТАНАВЛИВАЕТСЯ
- `Break` → поломка + программа ОСТАНАВЛИВАЕТСЯ
- `Swim` → замедленное движение + программа продолжается
- `Slide` → ускоренное движение + программа продолжается

---

### Шаг 2: Создать struct CellReaction
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReaction.cs`

```csharp
using UnityEngine;

namespace CodeBlocks.Collision
{
    /// <summary>
    /// Конфигурация реакции для определённого типа клетки.
    /// Хранит параметры анимации, звуков и логики обработки.
    /// </summary>
    [System.Serializable]
    public struct CellReaction
    {
        /// <summary>Тип реакции (Move, Bounce, Fall, Break, Swim, Slide)</summary>
        public CellReactionType reactionType;

        /// <summary>Длительность анимации реакции в секундах</summary>
        [Range(0.1f, 5f)]
        public float animationDuration;

        /// <summary>Кривая анимации (для lerp движения/ротации)</summary>
        public AnimationCurve animationCurve;

        /// <summary>Название звукового эффекта (опционально, "bounce", "fall", "crack")</summary>
        public string sfxName;

        /// <summary>Сумма урона при попадании (для future health system)</summary>
        [Range(0f, 100f)]
        public float damageAmount;

        /// <summary>Останавливает ли реакция программу</summary>
        public bool stopsProgram;

        /// <summary>Дополнительные параметры (опционально)</summary>
        public float speedModifier;  // 1.0 = normal, 0.5 = half speed (Water), 1.5 = faster (Ice)

        /// <summary>Сдвиг высоты робота при анимации (для эффекта падения)</summary>
        public float heightOffset;

        // Конструктор с параметрами по умолчанию
        public CellReaction(CellReactionType type, float duration = 0.3f)
        {
            reactionType = type;
            animationDuration = duration;
            animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            sfxName = "";
            damageAmount = 0f;
            stopsProgram = false;
            speedModifier = 1.0f;
            heightOffset = 0f;
        }
    }
}
```

**Использование конструктора:**
```csharp
var moveReaction = new CellReaction(CellReactionType.Move, 0.3f);
var fallReaction = new CellReaction(CellReactionType.Fall, 1.0f);
```

---

### Шаг 3: Создать ScriptableObject CellReactionConfig
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionConfig.cs`

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace CodeBlocks.Collision
{
    /// <summary>
    /// Конфигурация реакций для всех типов клеток в уровне.
    /// ScriptableObject, который можно редактировать в Unity Inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "CellReactionConfig", menuName = "CodeBlocks/Collision/Cell Reaction Config")]
    public class CellReactionConfig : ScriptableObject
    {
        [System.Serializable]
        public struct ReactionMapping
        {
            [Tooltip("Тип terrain клетки (Ground, Pit, Spike, Water, Ice)")]
            public string terrainType;

            [Tooltip("Тип reaction для этой terrain клетки")]
            public CellReaction reaction;
        }

        [SerializeField]
        [Tooltip("Маппинг terrain типов на реакции")]
        public ReactionMapping[] reactionMappings = new ReactionMapping[0];

        /// <summary>Кэш для быстрого доступа по типу</summary>
        private Dictionary<string, CellReaction> reactionCache;

        /// <summary>Инициализировать кэш (вызывается при загрузке конфига)</summary>
        public void Initialize()
        {
            reactionCache = new Dictionary<string, CellReaction>();

            foreach (var mapping in reactionMappings)
            {
                if (!string.IsNullOrEmpty(mapping.terrainType))
                {
                    reactionCache[mapping.terrainType] = mapping.reaction;
                }
            }
        }

        /// <summary>Получить реакцию для типа terrain</summary>
        public CellReaction GetReactionForTerrain(string terrainType)
        {
            // Инициализировать кэш если ещё не инициализирован
            if (reactionCache == null || reactionCache.Count == 0)
            {
                Initialize();
            }

            if (reactionCache.TryGetValue(terrainType, out var reaction))
            {
                return reaction;
            }

            // Если не найдено, вернуть default Move reaction
            Debug.LogWarning($"[CellReactionConfig] No reaction mapping for terrain type '{terrainType}'. Using default Move.");
            return new CellReaction(CellReactionType.Move);
        }

        /// <summary>Получить всё маппинги (для Editor)</summary>
        public ReactionMapping[] GetAllMappings() => reactionMappings;

        /// <summary>Установить маппинг (для Editor)</summary>
        public void SetMappings(ReactionMapping[] mappings)
        {
            reactionMappings = mappings;
            Initialize();
        }
    }
}
```

**Использование:**
```csharp
CellReactionConfig config = Resources.Load<CellReactionConfig>("Configs/DefaultCellReactions");
config.Initialize();

CellReaction groundReaction = config.GetReactionForTerrain("Ground");    // Move
CellReaction pitReaction = config.GetReactionForTerrain("Pit");          // Fall
```

---

### Шаг 4: Расширить LevelGridData
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/LevelEditor/LevelGridData.cs`

**Добавить поле:**
```csharp
public class LevelGridData : ScriptableObject
{
    // ... существующие поля ...

    [SerializeField]
    [Tooltip("Конфигурация реакций для этого уровня")]
    public CellReactionConfig cellReactionConfig;

    /// <summary>Получить реакцию для клетки по grid позиции</summary>
    public CellReaction GetCellReaction(int x, int y)
    {
        // Проверить границы
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.LogWarning($"[LevelGridData] Position ({x}, {y}) is out of grid bounds!");
            return new CellReaction(CellReactionType.None);
        }

        // Сначала проверить terrain
        var terrain = GetTerrainAt(x, y);
        if (terrain != null && !string.IsNullOrEmpty(terrain.terrainType))
        {
            if (cellReactionConfig != null)
            {
                return cellReactionConfig.GetReactionForTerrain(terrain.terrainType);
            }
        }

        // Если нет конфига, вернуть default
        return new CellReaction(CellReactionType.Move);
    }

    /// <summary>Расширить IsPassable для разных типов ловушек</summary>
    public bool IsPassable(int x, int y)
    {
        // Проверить границы
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
            return false;

        var terrain = GetTerrainAt(x, y);
        if (terrain == null)
            return true;  // Empty cell is passable

        // Определить проходимость по типу
        return terrain.terrainType switch
        {
            "Pit" => false,         // Яма непроходима (робот падает)
            "Spike" => false,       // Шип непроходим (робот ломается)
            "Water" => true,        // Вода проходима (но с замедлением)
            "Ice" => true,          // Лёд проходим (но с ускорением)
            _ => true               // По умолчанию проходимо
        };
    }
}
```

---

### Шаг 5: Создать примеры конфигов в Unity
**Место**: `Assets/CodeBlocks/Resources/Configs/`

**Создать файл: `DefaultCellReactions.asset`**

Через Unity Editor:
1. Right-click в Assets/CodeBlocks/Resources/Configs
2. Create → CodeBlocks → Collision → Cell Reaction Config
3. Заполнить маппинги:

| Terrain Type | Reaction Type | Duration | Speed Mod | Damage | Stops Program |
|---|---|---|---|---|---|
| Ground | Move | 0.3 | 1.0 | 0 | ❌ |
| Road | Move | 0.3 | 1.0 | 0 | ❌ |
| Pit | Fall | 1.0 | 1.0 | 10 | ✅ |
| Spike | Break | 0.5 | 1.0 | 20 | ✅ |
| Water | Swim | 0.5 | 0.5 | 0 | ❌ |
| Ice | Slide | 0.3 | 1.5 | 0 | ❌ |

**Кривые анимации:**
- Move: Linear (0,0) → (1,1)
- Bounce: EaseOut (отскок)
- Fall: EaseIn (ускорение вниз)
- Break: Sudden at 0.7

---

### Шаг 6: Интегрировать конфиг в LevelRuntimeManager
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManager.cs`

**Добавить:**
```csharp
public class LevelRuntimeManager : MonoBehaviour
{
    private CellReactionConfig cellReactionConfig;

    public void LoadLevel(LevelGridData levelData)
    {
        // ... существующий код ...

        // Загрузить конфиг реакций
        if (levelData.cellReactionConfig != null)
        {
            cellReactionConfig = levelData.cellReactionConfig;
            cellReactionConfig.Initialize();
        }
        else
        {
            // Загрузить дефолтный конфиг
            cellReactionConfig = Resources.Load<CellReactionConfig>("Configs/DefaultCellReactions");
            if (cellReactionConfig != null)
            {
                cellReactionConfig.Initialize();
            }
        }
    }

    public CellReactionConfig GetCellReactionConfig() => cellReactionConfig;
}
```

---

### Шаг 7: Добавить методы в GridPositionTracker
**Файл**: `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`

**Добавить методы для получения реакции:**
```csharp
public class GridPositionTracker : MonoBehaviour
{
    // ... существующие методы ...

    /// <summary>Получить реакцию для текущей позиции робота</summary>
    public CellReaction GetCurrentCellReaction()
    {
        if (levelRuntimeManager == null)
            return new CellReaction(CellReactionType.None);

        var level = levelRuntimeManager.CurrentLevel;
        if (level == null)
            return new CellReaction(CellReactionType.None);

        return level.GetCellReaction(CurrentGridPosition.x, CurrentGridPosition.y);
    }

    /// <summary>Получить реакцию для позиции</summary>
    public CellReaction GetCellReactionAt(Vector2Int gridPos)
    {
        if (levelRuntimeManager == null)
            return new CellReaction(CellReactionType.None);

        var level = levelRuntimeManager.CurrentLevel;
        if (level == null)
            return new CellReaction(CellReactionType.None);

        return level.GetCellReaction(gridPos.x, gridPos.y);
    }
}
```

---

## ✅ Acceptance Criteria (Блок 1)

- [ ] `CellReactionType.cs` создан с 6 типами реакций (Move, Bounce, Fall, Break, Swim, Slide)
- [ ] `CellReaction.cs` struct с полями: type, duration, curve, sfx, damage, stopsProgram, speedModifier, heightOffset
- [ ] `CellReactionConfig.cs` ScriptableObject с реакциями по типам
- [ ] `LevelGridData.GetCellReaction()` возвращает реакцию для позиции
- [ ] `LevelGridData.IsPassable()` проверяет Pit, Spike, Water, Ice
- [ ] Конфиг `DefaultCellReactions.asset` создан в Resources/Configs
- [ ] Все 6 типов terrain маппированы на соответствующие реакции
- [ ] `LevelRuntimeManager` загружает конфиг при `LoadLevel()`
- [ ] `GridPositionTracker.GetCurrentCellReaction()` работает
- [ ] Код скомпилирован, 0 errors, console чистая
- [ ] All tests pass in Editor

---

## 🔍 Debug & Testing

### Проверка в Unity Editor:

1. **Создать тестовый конфиг:**
   - Create → CodeBlocks → Collision → Cell Reaction Config
   - Заполнить маппинги
   - Проверить в Inspector что отображаются все поля

2. **Проверить GetCellReaction():**
   ```csharp
   // В GameManagerAPITest.cs
   [Test]
   public void TestCellReactionConfig()
   {
       var config = Resources.Load<CellReactionConfig>("Configs/DefaultCellReactions");
       var moveReaction = config.GetReactionForTerrain("Ground");
       Assert.AreEqual(CellReactionType.Move, moveReaction.reactionType);

       var fallReaction = config.GetReactionForTerrain("Pit");
       Assert.AreEqual(CellReactionType.Fall, fallReaction.reactionType);
       Assert.IsTrue(fallReaction.stopsProgram);
   }
   ```

3. **Консоль:**
   - Должны быть логи вида: "[CellReactionConfig] Initialized with X mappings"
   - Нет LogErrors про missing конфиги

---

## 🎓 Lessons & Notes

1. **Модульность**: CellReactionConfig может быть переиспользован в нескольких уровнях
2. **Гибкость**: Легко добавить новый тип реакции без изменения кода (просто добавить enum значение)
3. **Performance**: Кэш в CellReactionConfig избегает повторных Dictionary lookups
4. **Safety**: GetCellReaction() никогда не возвращает null, всегда возвращает CellReaction (даже если None)

---

## 🔗 Переход к следующему блоку

После завершения Блока 1, переходим к **Блоку 2: Finish Logic Improvements** → `.Doc/Tasks/25_Block2_FinishLogicImprovement.md`

Блок 1 обеспечивает инфраструктуру, Блоки 2-5 её используют для реализации конкретных механик.
