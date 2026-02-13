# Task #9: Магнитный снап блоков к выходам

## Обзор
Реализация визуальной системы входов/выходов блоков с магнитным снапом при перетаскивании. Блоки будут автоматически "примагничиваться" к ближайшему выходу другого блока.

## Контекст
- Текущая система просто стакует блоки вертикально
- Нужна новая система где блоки визуально связаны через входы/выходы
- Выполнение программы должно идти по физическим соединениям блоков, не по Y-позиции
- В будущем появятся блоки с несколькими выходами (условные ветвления)
- **Параметры (типы данных) вынесены в отдельную задачу #10**

## Требования (Задача #9 - 6 этапов)
1. **Визуальные точки входа/выхода** - маленькие кружки обозначающие места подключения
2. **Магнитный снап** - при drag блок примагничивается к ближайшему выходу
3. **Расстояние snap** - настраиваемое расстояние активации магнита (~50px)
4. **Позиционирование** - при примагничивании блок выравнивается по выходу другого блока
5. **Отпускание** - блок остается в примагниченной позиции
6. **Разрыв связи** - можно перетащить блок в пустое место чтобы разорвать связь
7. **Выполнение по соединениям** - программа выполняется по физическим связям между блоками

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

## План реализации (6 проверяемых этапов)

**ВАЖНО:** Этап 7 (Инфраструктура для параметров) вынесен в отдельную задачу #10

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
   - Struct SnapInfo для информации о снапе
   - Метод FindNearestOutput(draggingBlock, allBlocks)
   - Расчет расстояния между точками

2. Исправить координаты в `BlockConnector.cs`:
   ```csharp
   public Vector2 GetWorldPosition()
   {
       Vector3[] corners = new Vector3[4];
       visualElement.GetWorldCorners(corners);  // ✅ Правильные мировые coords
       return (Vector2)((corners[0] + corners[2]) / 2f);
   }
   ```

3. Интегрировать в `ProgramArea.cs`:
   - Создание SnapManager в Awake
   - Метод GetSnapManager() для доступа
   - Позиционирование нового блока при drop через RectTransformUtility

4. Оптимизировать `BlockUI.cs`:
   - Кэширование `ProgramArea programArea` в Awake
   - Использование кэша в OnDrag вместо GetComponentInParent каждый frame
   - Возврат блока в палитру после drop

**Файлы для изменения:**
- ✓ Создать: `SnapManager.cs`
- ✓ Изменить: `BlockConnector.cs` (GetWorldPosition)
- ✓ Изменить: `ProgramArea.cs` (позиционирование, snap интеграция)
- ✓ Изменить: `BlockUI.cs` (кэширование ProgramArea, оптимизация)

**Проверка:**
- ✓ При перемещении блока логируется ближайший выход ([SNAP READY])
- ✓ Расстояние считается правильно в screen space
- ✓ Новый блок позиционируется в точку drop
- ✓ Блок из палитры возвращается в исходную позицию
- ✓ Нет лагов при перемещении (оптимизированы поиски)

**Критические моменты (обнаруженные баги):**
- ❌ GetWorldPosition() должен использовать GetWorldCorners(), не position
- ❌ OnDrop должен использовать ScreenPointToLocalPointInRectangle для позиции
- ❌ GetComponentInParent() в OnDrag каждый frame - нужно кэшировать
- ❌ Блок должен возвращаться в палитру после drop (ReturnToOriginalPosition)

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

### Этап 5: Применение снапа (45 мин) ✓ ВЫПОЛНЕНО
**Цель:** При drop блок остается в примагниченной позиции

**Статус:** Завершено 24.12.2025

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
- ✓ Изменить: `SnapManager.cs` (ApplySnap) - ВЫПОЛНЕНО
- ✓ Изменить: `BlockUI.cs` (OnEndDrag) - ВЫПОЛНЕНО

**Проверка:**
- ✓ При drop близко к выходу блок примагничивается - ВЫПОЛНЕНО
- ✓ Вход выравнивается с выходом другого блока - ВЫПОЛНЕНО
- ✓ При повторном drag блок снова ищет снап - ВЫПОЛНЕНО

**Детали реализации:**
- SnapManager.ApplySnap() вычисляет world position offset между input и output
- OnEndDrag проверяет canSnap и применяет snap или возвращает в палитру
- Логирование: [SNAP APPLIED] для отладки
- Компиляция: ✓ успешно (0 ошибок)

---

### Этап 6: Выполнение по соединениям (60 мин) - ФИНАЛЬНЫЙ ЭТАП
**Цель:** Связать выполнение команд с физическими соединениями между блоками через BlockConnector

**Контекст:**
Исходная архитектура предусматривала связанный список через соединения, а не Y-позицию.
При snap блоки физически соединяются (Output ↔ Input).
При выполнении программы нужно идти по этим соединениям.

**Это финальный этап задачи #9.** После его завершения система магнитного снапа полностью работает.

**Действия:**

#### 1. Расширить BlockConnector (BlockConnector.cs)
Добавить два поля для навигации и соединения:
```csharp
public class BlockConnector
{
    public enum PointType { Input, Output }

    public PointType pointType;
    public RectTransform visualElement;

    // НОВОЕ: ссылка на владельца (для навигации)
    public BlockUI parentBlock;

    // НОВОЕ: соединение с другим BlockConnector
    public BlockConnector connectedTo;

    public BlockConnector(PointType type, RectTransform visual)
    {
        pointType = type;
        visualElement = visual;
    }

    // Получить мировую позицию точки
    public Vector2 GetWorldPosition()
    {
        Vector3[] corners = new Vector3[4];
        visualElement.GetWorldCorners(corners);
        return (Vector2)((corners[0] + corners[2]) / 2f);
    }
}
```

#### 2. Обновить инициализацию (BlockUI.cs)
В методе `InitializeConnectors()` установить parentBlock:
```csharp
public void InitializeConnectors()
{
    inputPoints.Clear();
    outputPoints.Clear();

    // Инициализировать входную точку
    if (inputPointVisual != null)
    {
        Image inputImage = inputPointVisual.GetComponent<Image>();
        if (inputImage != null)
        {
            inputImage.color = new Color(0f, 1f, 0f, 1f); // Green
        }

        BlockConnector inputConnector = new BlockConnector(
            BlockConnector.PointType.Input, inputPointVisual);
        inputConnector.parentBlock = this;  // ← УСТАНОВИТЬ ВЛАДЕЛЬЦА
        inputPoints.Add(inputConnector);
    }

    // Инициализировать выходные точки
    foreach (RectTransform outputVisual in outputPointsVisuals)
    {
        if (outputVisual != null)
        {
            Image outputImage = outputVisual.GetComponent<Image>();
            if (outputImage != null)
            {
                outputImage.color = new Color(1f, 0f, 0f, 1f); // Red
            }

            BlockConnector outputConnector = new BlockConnector(
                BlockConnector.PointType.Output, outputVisual);
            outputConnector.parentBlock = this;  // ← УСТАНОВИТЬ ВЛАДЕЛЬЦА
            outputPoints.Add(outputConnector);
        }
    }
}
```

#### 3. Обновить ApplySnap (SnapManager.cs)
Создавать соединение при snap:
```csharp
public void ApplySnap(BlockUI draggingBlock,
                      BlockConnector inputPoint,
                      BlockConnector targetOutput)
{
    if (draggingBlock == null || inputPoint == null || targetOutput == null)
    {
        return;
    }

    // Выравнивание позиции (как было)
    Vector2 targetPosition = targetOutput.GetWorldPosition();
    Vector2 currentInputWorldPos = inputPoint.GetWorldPosition();
    Vector2 offset = targetPosition - currentInputWorldPos;

    RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
    if (blockRect != null)
    {
        blockRect.position = new Vector3(
            blockRect.position.x + offset.x,
            blockRect.position.y + offset.y,
            blockRect.position.z
        );
    }

    // НОВОЕ: создать соединение между блоками
    targetOutput.connectedTo = inputPoint;
    Debug.Log($"[CONNECTION] {targetOutput.parentBlock.gameObject.name} " +
              $"→ {inputPoint.parentBlock.gameObject.name}");
}
```

#### 4. Добавить навигацию (BlockUI.cs)
Новый метод для получения следующего блока по соединению:
```csharp
/// <summary>
/// Получить следующий блок, подключенный к выходу этого блока.
/// Выполнение идет по физическому соединению, не по Y-позиции.
/// </summary>
public BlockUI GetNextBlock()
{
    // Получить первый выход
    if (outputPoints.Count > 0 && outputPoints[0] != null)
    {
        BlockConnector output = outputPoints[0];

        // Если есть соединение с другим блоком
        if (output.connectedTo != null)
        {
            return output.connectedTo.parentBlock;
        }
    }

    return null;  // Это конец цепочки
}

/// <summary>
/// Разорвать соединение на выходе (опционально для будущего).
/// </summary>
public void DisconnectOutput(int outputIndex = 0)
{
    if (outputIndex >= 0 && outputIndex < outputPoints.Count)
    {
        outputPoints[outputIndex].connectedTo = null;
        Debug.Log($"[DISCONNECT] {gameObject.name} output {outputIndex}");
    }
}
```

#### 5. Обновить логику выполнения
Найти где выполняются команды (CommandExecutor или GameManager) и обновить цикл:

**ДО (не используется):**
```csharp
// Старый подход по Y-позиции (удалить)
var sortedBlocks = blocksInProgram
    .OrderByDescending(b => b.GetComponent<RectTransform>().anchoredPosition.y)
    .ToList();

for (int i = 0; i < sortedBlocks.Count - 1; i++)
{
    sortedBlocks[i].Command.Next = sortedBlocks[i + 1].Command;
}
```

**ПОСЛЕ (по соединениям):**
```csharp
// Новый подход - идем по физическим соединениям
public IEnumerator ExecuteProgram(BlockUI startBlock)
{
    BlockUI currentBlock = startBlock;

    while (currentBlock != null)
    {
        ICommand command = currentBlock.Command;

        // Выполнить команду
        Debug.Log($"[EXECUTE] {currentBlock.gameObject.name}: {command.GetDisplayName()}");
        yield return ExecuteCommand(command);

        // Перейти к следующему блоку по соединению
        currentBlock = currentBlock.GetNextBlock();
    }

    Debug.Log("[PROGRAM COMPLETE]");
}
```

**Файлы для изменения:**
- ✓ Изменить: `BlockConnector.cs` (добавить parentBlock + connectedTo)
- ✓ Изменить: `BlockUI.cs` (InitializeConnectors + GetNextBlock + DisconnectOutput)
- ✓ Изменить: `SnapManager.cs` (ApplySnap создает соединение)
- ✓ Изменить: `CommandExecutor.cs` или `GameManager.cs` (логика выполнения)

**Проверка:**
- ✓ При snap создается соединение (логи [CONNECTION])
- ✓ Разрыв соединения при перемещении (нужно реализовать)
- ✓ При Run программа выполняется по соединениям, не по Y-позиции
- ✓ Команды выполняются в правильном порядке (по физической связи)
- ✓ Robot выполняет программу правильно
- ✓ GetNextBlock() возвращает правильный следующий блок
- ✓ LastBlock.GetNextBlock() возвращает null (конец программы)

---

## Итоговая архитектура после выполнения всех этапов

### Иерархия соединений (связанный список):
```
Block A (Start)
└── outputPoints[0]
    └── connectedTo → Block B input
        └── Block B
            └── outputPoints[0]
                └── connectedTo → Block C input
                    └── Block C
                        └── outputPoints[0]
                            └── connectedTo → null (конец)

Выполнение: A → B → C (по физическим соединениям)
```

### Структура компонентов:

**BlockConnector (точка подключения)**
```
BlockConnector
├── pointType: Input / Output
├── visualElement: RectTransform (кружок на экране)
├── parentBlock: BlockUI (владелец точки) ← STAGE 6
├── connectedTo: BlockConnector (физическое соединение) ← STAGE 6
└── GetWorldPosition(): Vector2 (координаты)
```

**BlockUI (блок программы)**
```
BlockUI
├── inputPoints: List<BlockConnector> (входные точки)
├── outputPoints: List<BlockConnector> (выходные точки)
├── Command: ICommand (команда)
├── inProgramArea: bool
├── previousHighlightedOutput: BlockConnector (для подсветки)
├── InitializeConnectors() (создание точек с parentBlock)
├── GetNextBlock(): BlockUI (получить блок по соединению) ← STAGE 6
├── DisconnectOutput(int) (разрыв соединения) ← STAGE 6
├── UpdateSnapVisuals(SnapInfo) (подсветка при drag)
├── ResetConnectorColors() (восстановление цветов)
├── OnDrag(PointerEventData) (поиск snap)
├── OnEndDrag(PointerEventData) (применение snap/возврат)
└── ReturnToOriginalPosition() (возврат в палитру)
```

**SnapManager (поиск и применение snap)**
```
SnapManager
├── snapDistance: float = 50px
├── SnapInfo struct
│   ├── targetBlock: BlockUI
│   ├── targetOutput: BlockConnector
│   ├── canSnap: bool
│   └── distance: float
├── FindNearestOutput(block, allBlocks): SnapInfo
│   └── Поиск ближайшего Output в пределах 50px
├── ApplySnap(draggingBlock, input, output) ← STAGE 5-6
│   ├── Выравнять позицию блока
│   └── Создать соединение: output.connectedTo = input ← STAGE 6
├── GetSnapDistance(): float
└── SetSnapDistance(float)
```

**ProgramArea (рабочая область)**
```
ProgramArea
├── blocksInProgram: List<BlockUI>
├── programSequence: ProgramSequence
├── snapManager: SnapManager
├── OnDrop(PointerEventData) (обработка drop палитры)
│   ├── Создать копию блока через BlockFactory
│   ├── Позиционировать по месту drop
│   └── AddBlockToProgram(newBlock)
├── AddBlockToProgram(blockUI)
│   ├── Добавить в programSequence
│   └── Обновить визуальное расположение
├── GetBlocks(): List<BlockUI>
└── GetSnapManager(): SnapManager
```

### Поток данных при выполнении программы (STAGE 6):
```
1. GameManager вызывает: ExecuteProgram(startBlock)

2. CommandExecutor выполняет:
   BlockUI currentBlock = startBlock
   while (currentBlock != null)
   {
       yield return ExecuteCommand(currentBlock.Command)
       currentBlock = currentBlock.GetNextBlock()  // ← По соединению!
   }

3. GetNextBlock() возвращает:
   outputPoints[0].connectedTo.parentBlock

4. Если соединения нет (null):
   Программа завершена
```

### Цветовая схема (текущая реализация):
```
Input point (зелёный):  ●
Output point (красный): ●
Ready to snap (жёлтый): ●
```

**Будущее (Задача #10 - типы параметров):**
```
Number (синий):   ●
String (жёлтый):  ●
Vector (оранжевый): ●
Boolean (розовый): ●
```

### Архитектурные преимущества задачи #9:
✅ **Связанный список** - логика выполнения = физические соединения
✅ **Множественные выходы** - готово для If/Else (outputPoints[1], [2]...)
✅ **Группировка** - можем управлять блоками начиная от выбранного по соединениям
✅ **Расширяемость** - архитектура готова для параметров (Задача #10) без рефакторинга
✅ **Без рефакторинга** - Y-позиция не используется, логика не зависит от визуального порядка
```

---

## Приемочные критерии

### После Этап 1-5:
- ✓ Каждый блок имеет видимые Input (зелёный) и Output (красный) точки
- ✓ При drag ближайший выход подсвечивается жёлтым
- ✓ Блок примагничивается на расстоянии ~50px
- ✓ При OnEndDrag блок встает в примагниченную позицию (snap применяется)
- ✓ Блоки визуально выравниваются при snap

### После Этап 6 (Выполнение по соединениям) - ФИНАЛЬНЫЕ КРИТЕРИИ:
- [ ] **Соединения создаются при snap** (Output.connectedTo = Input)
- [ ] **GetNextBlock() возвращает правильный следующий блок**
- [ ] **Выполнение идет по соединениям, не по Y-позиции**
- [ ] **Программа выполняется в правильном порядке** (по физической цепочке)
- [ ] **Последний блок.GetNextBlock() = null** (конец программы)
- [ ] Логи [CONNECTION] при snap показывают создание соединения
- [ ] DisconnectOutput() разрывает соединение (опционально)

### Общие критерии (итоговая приемка задачи #9):
- [ ] Robot выполняет программу в правильном порядке
- [ ] Нет регрессии существующего функционала (Этапы 1-5)
- [ ] Каждый этап разработки проверен и работает
- [ ] Архитектура поддерживает If/Else (outputPoints[1], [2]...) для будущих задач
- [ ] Архитектура поддерживает параллельные потоки (множественные выходы)
- [ ] Архитектура поддерживает группировку блоков (навигация по соединениям)

**Примечание:** Типы параметров и валидация данных будут реализованы в отдельной задаче #10

---

## Возможные сложности и решения

### Проблема: parentBlock не установлен при создании соединения
**Решение:** Убедиться что InitializeConnectors() вызывается с установкой parentBlock:
```csharp
inputConnector.parentBlock = this;
outputConnector.parentBlock = this;
```

### Проблема: GetNextBlock() возвращает null когда блок не соединен
**Решение:** Это нормальное поведение - означает конец программы. Проверить логику выполнения:
```csharp
while (currentBlock != null)  // Цикл корректно заканчивается
{
    // выполнить блок
    currentBlock = currentBlock.GetNextBlock();
}
```

### Проблема: Разрыв соединения при перемещении блока
**Решение:** Реализовать логику разрыва в OnBeginDrag или добавить явный вызов DisconnectOutput():
```csharp
public void DisconnectOutput(int outputIndex = 0)
{
    if (outputIndex < outputPoints.Count)
        outputPoints[outputIndex].connectedTo = null;
}
```

### Проблема: Циклические соединения (A → B → A)
**Решение:** Добавить валидацию в SnapManager.ApplySnap():
```csharp
// Проверить нет ли цикла
if (IsPartOfChain(draggingBlock, targetOutput.parentBlock))
{
    Debug.LogWarning("[CYCLE] Cannot create circular connection");
    return;
}
```

### Проблема: Выполнение идет по старой Y-позиции логике
**Решение:** Удалить/закомментировать старый код и использовать только GetNextBlock():
```csharp
// УДАЛИТЬ:
// var sorted = blocksInProgram.OrderByDescending(...).ToList();
// for (...) sorted[i].Command.Next = sorted[i+1].Command;

// ИСПОЛЬЗОВАТЬ:
currentBlock = currentBlock.GetNextBlock();
```

### Проблема: connectedTo указывает на Input, но нужна ссылка на сам блок
**Решение:** Всегда использовать `connectedTo.parentBlock` для получения BlockUI:
```csharp
public BlockUI GetNextBlock()
{
    if (outputPoints[0].connectedTo != null)
        return outputPoints[0].connectedTo.parentBlock;  // ← parentBlock!
    return null;
}
```

### Проблема: Multiple outputs (If/Else) - какой выход использовать при выполнении?
**Решение:** Пока используем outputPoints[0]. При If/Else добавить логику выбора:
```csharp
public BlockUI GetNextBlock(bool condition = true)
{
    int outputIndex = condition ? 0 : 1;  // True → [0], False → [1]
    if (outputIndex < outputPoints.Count && outputPoints[outputIndex].connectedTo != null)
        return outputPoints[outputIndex].connectedTo.parentBlock;
    return null;
}
```

### Проблема: RectTransform координаты при вычислении snap
**Решение:** Использовать GetWorldCorners() как в GetWorldPosition():
```csharp
Vector3[] corners = new Vector3[4];
visualElement.GetWorldCorners(corners);
Vector2 worldPos = (Vector2)((corners[0] + corners[2]) / 2f);
```

### Проблема: Canvas scaling при расстояниях
**Решение:** GetWorldPosition() уже работает в world координатах, расстояния вычисляются корректно

### Проблема: Блок примагничивается к самому себе
**Решение:** В FindNearestOutput исключить перетаскиваемый блок (уже реализовано):

---

## Заметки

### Архитектура (Вариант B - В процессе реализации):
- ✓ Выполнение через соединения (connectedTo), не Y-позицию
- ✓ Связанный список: Output.connectedTo → Input.parentBlock
- [ ] GetNextBlock() для навигации по цепочке (Этап 6)
- ✓ Готово для If/Else (outputPoints[1], [2]...) без рефакторинга
- ✓ Готово для группировки (навигация по соединениям)

### Будущие задачи (вне рамок #9):
- **Задача #10:** Типы параметров (Number, String, Vector, Boolean)
- **Задача #10:** Валидация типов при соединении (CanConnectTo)
- **Задача #10:** Передача данных между блоками (parameterValue)
- Визуальное редактирование параметров в соединениях
- Разные цвета для разных типов параметров

### Визуальные улучшения (Будущее):
- Маленькие кружки (BlockConnector visual) можно сделать интерактивны (hover)
- Можно рисовать Bezier кривые между соединенными точками
- Разные цвета для разных выходов If/Else (True/False)
- snapDistance можно сделать настраиваемым в Inspector (50-100px рекомендуется)

### Оптимизации (Будущее):
- Кэширование сортировки блоков (если потребуется)
- Pooling BlockConnector объектов если много соединений

### Отладка (текущая задача #9):
- Логи [CONNECTION] при создании соединения
- Логи [DISCONNECT] при разрыве соединения
- Логи [EXECUTE] при выполнении каждого блока
- Логи [CYCLE] при попытке создать циклическое соединение (опционально)
