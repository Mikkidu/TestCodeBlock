# Task #9: Магнитный снап блоков к выходам

## Обзор
Реализация визуальной системы входов/выходов блоков с магнитным снапом при перетаскивании. Блоки будут автоматически "примагничиваться" к ближайшему выходу другого блока.

## Контекст
- Текущая система просто стакует блоки вертикально
- Нужна новая система где блоки визуально связаны через входы/выходы
- В будущем появятся блоки с несколькими выходами (условные ветвления)
- Архитектура должна поддерживать передачу параметров между блоками

## Требования
1. **Визуальные точки входа/выхода** - маленькие кружки обозначающие места подключения
2. **Магнитный снап** - при drag блок примагничивается к ближайшему выходу
3. **Расстояние snap** - настраиваемое расстояние активации магнита (~50px)
4. **Позиционирование** - при примагничивании блок выравнивается по выходу другого блока
5. **Отпускание** - блок остается в примагниченной позиции
6. **Разрыв связи** - можно перетащить блок в пустое место чтобы разорвать связь
7. **Заготовка для параметров** - архитектура должна поддерживать передачу параметров

---

## Архитектура решения

### BlockConnector - точка подключения

```csharp
public class BlockConnector
{
    public enum PointType { Input, Output }

    // Основные
    public PointType pointType;
    public RectTransform visualElement;  // маленький UI элемент (кружок)

    // Будущее - передача параметров
    public enum ParameterType { None, Number, String, Boolean }
    public ParameterType parameterType = ParameterType.None;
    public string parameterName = "";

    // Получить мировую позицию точки
    public Vector2 GetWorldPosition()
    {
        return visualElement.position;
    }
}
```

### BlockUI - обновленная структура

```csharp
public class BlockUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Новые поля
    public List<BlockConnector> inputPoints = new List<BlockConnector>();
    public List<BlockConnector> outputPoints = new List<BlockConnector>();

    // Текущее состояние при drag
    private BlockConnector nearestTargetOutput;
    private bool isSnapReady;

    // ... остальное как было
}
```

### SnapManager - логика поиска и применения снапа

```csharp
public class SnapManager : MonoBehaviour
{
    [SerializeField] private float snapDistance = 50f;

    public struct SnapInfo
    {
        public BlockUI targetBlock;
        public BlockConnector targetOutput;
        public bool canSnap;
        public float distance;
    }

    // Поиск ближайшего выхода
    public SnapInfo FindNearestOutput(BlockUI draggingBlock, BlockConnector inputPoint)
    {
        // Найти все блоки в программе
        // Для каждого найти ближайший выход
        // Вернуть информацию о снапе
    }

    // Применить позицию снапа
    public void ApplySnap(BlockUI draggingBlock, BlockConnector input, BlockConnector targetOutput)
    {
        // Выравнять input block по targetOutput
    }
}
```

### Визуализация

```
Каждый блок выглядит так:

┌─────────────────────┐
│                     │
●  (Input point)      │  ← маленький кружок вверху
│   Move Forward      │
│                     │
│                   ● ← маленький кружок внизу
│              (Output point)
└─────────────────────┘

При drag близко к выходу:
- Input точка подсвечивается зеленым
- Output точка подсвечивается зеленым
- Блок готов к снапу
```

---

## План реализации (7 проверяемых этапов)

### Этап 1: Инфраструктура (30 мин)
**Цель:** Создать классы для точек подключения

**Действия:**
1. Создать `Assets/Scripts/RobotProgramming/UI/BlockConnector.cs`
2. Добавить поля в `BlockUI.cs`:
   - `List<BlockConnector> inputPoints`
   - `List<BlockConnector> outputPoints`
3. Добавить метод инициализации (пока пустой):
   ```csharp
   public void InitializeConnectors()
   {
       // Будет реализовано на этапе 2
   }
   ```

**Файлы для изменения:**
- ✓ Создать: `BlockConnector.cs`
- ✓ Изменить: `BlockUI.cs`

**Проверка:**
- ✓ BlockConnector создается без ошибок
- ✓ BlockUI компилируется
- ✓ Поля visible в Inspector

---

### Этап 2: Визуальные точки (30 мин)
**Цель:** Подготовить возможность назначения входов/выходов в Inspector

**Действия:**
1. Добавить SerializeField в `BlockUI.cs`:
   ```csharp
   [SerializeField] private RectTransform inputPointVisual;
   [SerializeField] private List<RectTransform> outputPointsVisuals = new List<RectTransform>();
   ```

2. Реализовать `BlockUI.InitializeConnectors()`:
   - Инициализировать BlockConnector объекты из назначенных визуальных элементов
   - Добавить warning если точки не назначены
   - Поддержка нескольких выходов (для будущих блоков If/Else)

3. Обновить `BlockFactory.CreateBlock()`:
   - Вызывать `InitializeConnectors()` после `SetCommand()`

**Файлы для изменения:**
- ✓ Изменить: `BlockUI.cs` (SerializeField + InitializeConnectors)
- ✓ Изменить: `BlockFactory.cs` (вызов InitializeConnectors)

**Как использовать:**
- На каждом block-prefab создать маленькие кружки вручную (Panel, Image, 12x12px)
- В Inspector перетащить элементы в поля `inputPointVisual` и `outputPointsVisuals`

**Проверка:**
- ✓ BlockUI компилируется
- ✓ Поля видны в Inspector
- ✓ InitializeConnectors() вызывается при создании блока
- ✓ Warning если точки не назначены

---

### Этап 3: SnapManager и поиск (45 мин)
**Цель:** Логика поиска ближайшего выхода

**Действия:**
1. Создать `Assets/Scripts/RobotProgramming/UI/SnapManager.cs`
2. Реализовать `FindNearestOutput()`:
   ```csharp
   public SnapInfo FindNearestOutput(BlockUI draggingBlock,
                                     List<BlockUI> allBlocks,
                                     float snapDistance)
   {
       float minDistance = float.MaxValue;
       BlockConnector nearestOutput = null;
       BlockUI targetBlock = null;

       // Для каждого блока в программе (кроме перетаскиваемого)
       // Найти его выходы
       // Вычислить расстояние до Input точки перетаскиваемого
       // Запомнить ближайший

       return new SnapInfo
       {
           targetBlock = targetBlock,
           targetOutput = nearestOutput,
           canSnap = minDistance <= snapDistance,
           distance = minDistance
       };
   }
   ```
3. Интегрировать в `ProgramArea`:
   ```csharp
   private SnapManager snapManager;

   private void Awake()
   {
       snapManager = gameObject.AddComponent<SnapManager>();
   }
   ```

**Файлы для изменения:**
- ✓ Создать: `SnapManager.cs`
- ✓ Изменить: `ProgramArea.cs` (добавить snapManager)

**Проверка:**
- ✓ При перемещении блока логируется (Debug.Log) ближайший выход
- ✓ Расстояние считается правильно
- ✓ Работает с несколькими блоками в программе

---

### Этап 4: Визуальный feedback (30 мин)
**Цель:** Показать когда блок готов к снапу

**Действия:**
1. Обновить `BlockUI.OnDrag()`:
   ```csharp
   public void OnDrag(PointerEventData eventData)
   {
       // ... существующий код перемещения ...

       // Новое
       SnapManager snapManager = GetComponentInParent<ProgramArea>().GetSnapManager();
       SnapManager.SnapInfo snapInfo = snapManager.FindNearestOutput(
           this,
           GetComponentInParent<ProgramArea>().GetBlocks(),
           snapDistance: 50f
       );

       UpdateSnapVisuals(snapInfo);
   }

   private void UpdateSnapVisuals(SnapManager.SnapInfo snapInfo)
   {
       if (snapInfo.canSnap)
       {
           // Подсветить Input точку зеленым
           inputPoints[0].visualElement.GetComponent<Image>().color = Color.green;
           // Подсветить Output точку зеленым
           snapInfo.targetOutput.visualElement.GetComponent<Image>().color = Color.green;
       }
       else
       {
           // Вернуть нормальный цвет
           ResetConnectorColors();
       }
   }
   ```

**Файлы для изменения:**
- ✓ Изменить: `BlockUI.cs` (OnDrag + UpdateSnapVisuals)

**Проверка:**
- ✓ При близости к выходу точки становятся зеленым
- ✓ При удалении от выхода цвет восстанавливается
- ✓ Гладко работает без lag-ов

---

### Этап 5: Применение снапа (45 мин)
**Цель:** При drop блок остается в примагниченной позиции

**Действия:**
1. Реализовать `SnapManager.ApplySnap()`:
   ```csharp
   public void ApplySnap(BlockUI draggingBlock,
                         BlockConnector inputPoint,
                         BlockConnector targetOutput)
   {
       // Получить позицию targetOutput
       Vector2 outputPos = targetOutput.GetWorldPosition();

       // Вычислить смещение Input от центра блока
       Vector2 inputOffset = inputPoint.visualElement.anchoredPosition;

       // Выравнять Input с Output
       RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
       blockRect.position = outputPos - (Vector2)inputOffset;
   }
   ```

2. Обновить `BlockUI.OnEndDrag()`:
   ```csharp
   public void OnEndDrag(PointerEventData eventData)
   {
       // ... существующий код восстановления цветов ...

       // Новое
       SnapManager snapManager = GetComponentInParent<ProgramArea>().GetSnapManager();
       SnapManager.SnapInfo snapInfo = snapManager.FindNearestOutput(...);

       if (snapInfo.canSnap)
       {
           // Сохранить информацию о снапе
           nearestTargetOutput = snapInfo.targetOutput;
           snapManager.ApplySnap(this, inputPoints[0], snapInfo.targetOutput);
       }
       else
       {
           ReturnToOriginalPosition();
       }
   }
   ```

**Файлы для изменения:**
- ✓ Изменить: `SnapManager.cs` (ApplySnap)
- ✓ Изменить: `BlockUI.cs` (OnEndDrag)

**Проверка:**
- ✓ При drop близко к выходу блок примагничивается
- ✓ Вход выравнивается с выходом другого блока
- ✓ При повторном drag блок снова ищет снап

---

### Этап 6: Обновление связей команд (30 мин)
**Цель:** Связать порядок выполнения команд с порядком блоков

**Действия:**
1. Обновить `ProgramArea.OnDrop()`:
   ```csharp
   public void OnDrop(PointerEventData eventData)
   {
       // ... существующий код создания копии ...

       // После AddBlockToProgram
       // Обновить связи на основе позиций блоков
       UpdateCommandChainByPosition();
   }

   private void UpdateCommandChainByPosition()
   {
       // Отсортировать блоки по Y позиции (сверху вниз)
       // Обновить ICommand.Next для каждого блока

       var sortedBlocks = blocksInProgram
           .OrderByDescending(b => b.GetComponent<RectTransform>().anchoredPosition.y)
           .ToList();

       for (int i = 0; i < sortedBlocks.Count - 1; i++)
       {
           sortedBlocks[i].Command.Next = sortedBlocks[i + 1].Command;
       }
   }
   ```

2. Вызвать UpdateCommandChainByPosition после каждого snap

**Файлы для изменения:**
- ✓ Изменить: `ProgramArea.cs`

**Проверка:**
- ✓ При Run команды выполняются в правильном порядке (по снапу)
- ✓ Можно изменить порядок перетаскиванием
- ✓ Robot выполняет программу правильно

---

### Этап 7: Инфраструктура для параметров (30 мин)
**Цель:** Подготовить архитектуру для передачи параметров

**Действия:**
1. Расширить `BlockConnector`:
   ```csharp
   public class BlockConnector
   {
       // ... существующее ...

       public enum ParameterType { None, Number, String, Boolean, Vector }
       public ParameterType parameterType = ParameterType.None;
       public string parameterName = "";

       // Для будущей валидации
       public bool CanConnectTo(BlockConnector other)
       {
           // Проверить совместимость типов параметров
           if (parameterType == ParameterType.None) return true;
           return parameterType == other.parameterType;
       }
   }
   ```

2. Добавить комментарии в `BlockFactory` для будущего:
   ```csharp
   // TODO: При создании блока передачи параметров
   // - Создавать Output точки с типом Number/String/etc
   // - Валидировать совместимость при снапе через CanConnectTo()
   ```

3. Обновить документацию (эта задача) с примером для параметров

**Файлы для изменения:**
- ✓ Изменить: `BlockConnector.cs`
- ✓ Изменить: `BlockFactory.cs` (добавить комментарии)
- ✓ Изменить: `.Doc/Tasks/9_SnapToOutputs.md`

**Проверка:**
- ✓ BlockConnector имеет поля для типов параметров
- ✓ Метод CanConnectTo() работает
- ✓ Нет регрессии функционала

---

## Итоговая архитектура после выполнения

```
BlockConnector (точка подключения)
    ├── Input/Output type
    ├── Visual element (маленький кружок)
    ├── Parameter info (для будущего)
    └── CanConnectTo() - валидация

BlockUI (блок)
    ├── inputPoints: List<BlockConnector>
    ├── outputPoints: List<BlockConnector>
    ├── InitializeConnectors() - создание точек
    ├── UpdateSnapVisuals() - подсвечивание
    └── OnDrag/OnEndDrag - интеграция со снапом

SnapManager (управление снапом)
    ├── FindNearestOutput() - поиск ближайшего
    ├── ApplySnap() - применение позиции
    └── snapDistance - настраиваемое расстояние

ProgramArea
    ├── GetSnapManager() - доступ к менеджеру
    └── UpdateCommandChainByPosition() - обновление связей
```

---

## Приемочные критерии

- ✓ Каждый блок имеет видимые Input и Output точки
- ✓ При drag ближайший выход подсвечивается
- ✓ Блок примагничивается на расстоянии ~50px
- ✓ При OnEndDrag блок встает в примагниченную позицию
- ✓ Команды выполняются в порядке снапа
- ✓ Архитектура готова для параметров (ParameterType в BlockConnector)
- ✓ Можно разорвать связь (drag в пустое место)
- ✓ Нет регрессии существующего функционала
- ✓ Каждый этап разработки проверен и работает

---

## Возможные сложности и решения

### Проблема: RectTransform координаты
**Решение:** Использовать `GetComponent<RectTransform>().position` для мировых координат

### Проблема: Canvas scaling
**Решение:** Учитывать `canvas.scaleFactor` при расчетах расстояний

### Проблема: Блок примагничивается к самому себе
**Решение:** В FindNearestOutput исключить перетаскиваемый блок

### Проблема: Выход находится не в центре блока
**Решение:** Вычислять смещение от центра как (outputPos - blockCenterPos)

---

## Заметки

- Маленькие кружки должны быть интерактивны (видны при hover)
- Для будущего: можно рисовать Bezier кривые между точками
- Для будущего: If/Else блоки будут иметь 2 Output (True/False) с разными цветами
- snapDistance можно сделать настраиваемым в Inspector (50-100px рекомендуется)
