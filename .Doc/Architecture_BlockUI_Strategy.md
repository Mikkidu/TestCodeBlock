# Архитектура BlockUI: Composition vs Наследование vs Гибридный подход

**Дата:** 2026-01-16
**Контекст:** Планирование архитектуры для будущих блоков (IfBlockUI, IfElseBlockUI, SwitchBlockUI и т.д.)

---

## Исходные данные

### Будущие типы блоков:
1. **SimpleBlockUI** - обычные блоки (Move, Turn, Wait)
2. **LoopBlockUI** - цикл (внутренние блоки, повторения)
3. **IfBlockUI** - условие (проверка 1 раз, 2 пути: true/false)
4. **IfElseBlockUI** - условие с else (два списка блоков)
5. **SwitchBlockUI** - множественный выбор

---

## Анализ: Что общего у всех?

### ✅ Вы правы - очень много общего!

**Базовая функциональность:**
1. ✓ Всегда есть **External Input** (вход из цепи)
2. ✓ Всегда есть **External Output** (выход в цепь)
3. ✓ **Выравнивание** к подсоединённому выходу (если есть)
4. ✓ **Передача управления** - получить от предыдущего, передать следующему
5. ✓ **Список коннекторов** - разные типы (Input/Output/Internal/Param/Variable)
6. ✓ **Доступ к команде** (`ICommand command`)
7. ✓ **Статус** (`inProgramArea`)
8. ✓ **Подсветка коннекторов** (визуальный feedback)
9. ✓ **Перетаскивание** (drag-drop)
10. ✓ **Возврат** в ProgramArea или Palette
11. ✓ **GetNextBlock()** - навигация по цепи
12. ✓ **Отключение входа** при перетаскивании

**Что разное:**
1. ⚠️ Наличие **внутренних блоков** (LoopBlockUI, IfBlockUI, SwitchBlockUI)
2. ⚠️ Подстройка **размера** (LoopBlockUI)
3. ⚠️ **Логика выполнения** - но это на самом деле команда, не блок!

---

## Вариант 1: Чистое наследование

```csharp
public abstract class BlockUIBase : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected ICommand command;
    protected Dictionary<string, BlockConnector> connectors;

    // Общие методы для всех блоков
    protected abstract void InitializeConnectors();
    public abstract void RecalculateSize();
    public ICommand Command => command;
    public BlockConnector GetConnector(string name) => connectors[name];
    // ... остальное общее
}

public class SimpleBlockUI : BlockUIBase
{
    protected override void InitializeConnectors()
    {
        connectors["external_input"] = ...;
        connectors["external_output"] = ...;
    }

    public override void RecalculateSize() { /* не меняется */ }
}

public class LoopBlockUI : BlockUIBase
{
    protected override void InitializeConnectors()
    {
        connectors["external_input"] = ...;
        connectors["external_output"] = ...;
        connectors["internal_input"] = ...;
        connectors["internal_output"] = ...;
    }

    public override void RecalculateSize() { /* пересчёт высоты */ }
}
```

### Плюсы наследования:
✅ **Унификация** - все блоки работают как BlockUIBase
✅ **Полиморфизм** - SnapManager работает с BlockUIBase
✅ **Единая логика** - Awake, OnBeginDrag и т.д. в базе
✅ **Нет GetComponent()** - всё на одном компоненте
✅ **Map коннекторов** - `GetConnector("external_input")` везде
✅ **Расширяемо** - легко добавлять новые типы

### Минусы наследования:
❌ **Все на одном компоненте** - может быть тяжело
❌ **Virtual methods overhead** - небольшой, но есть
❌ **Сложнее отладка** - нужно понимать иерархию
❌ **Переопределения** - каждый подкласс переопределяет методы

---

## Вариант 2: Чистое Composition (текущий)

```csharp
public class BlockUI : MonoBehaviour
{
    public List<BlockConnector> inputPoints;
    public List<BlockConnector> outputPoints;
    // Базовая функциональность
}

public class LoopBlockUI : MonoBehaviour
{
    private BlockUI blockUI; // GetComponent<BlockUI>()
    public BlockConnector ExternalInput { get; private set; }
    public BlockConnector ExternalOutput { get; private set; }
    public BlockConnector InternalInput { get; private set; }
    public BlockConnector InternalOutput { get; private set; }
    // Loop специфичная функциональность
}
```

### Плюсы Composition:
✅ **Простая структура** - каждый компонент делает одно
✅ **Изоляция** - LoopBlockUI не трогает BlockUI
✅ **Гибкость** - легко менять компоненты
✅ **Отладка** - ясная иерархия

### Минусы Composition:
❌ **GetComponent() вызовы** - много одинаковых вызовов
❌ **Несогласованность** - BlockUI.inputPoints vs LoopBlockUI.ExternalInput
❌ **Дублирование** - нужно инициализировать оба компонента
❌ **SnapManager complexity** - нужно проверять оба типа компонентов
❌ **Будущие проблемы** - при добавлении IfBlockUI нужно обновлять везде

---

## Вариант 3: Гибридный подход (РЕКОМЕНДУЕМЫЙ)

### Идея: Базовый класс + вспомогательные компоненты

```csharp
// ===== BASE CLASS (Общая функциональность) =====
public abstract class BlockUIBase : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] protected Image blockImage;
    [SerializeField] protected TextMeshProUGUI blockLabel;

    protected ICommand command;
    protected CanvasGroup canvasGroup;
    protected Canvas rootCanvas;
    protected ProgramArea programArea;

    // Коннекторы - унифицированные через Map
    protected Dictionary<string, BlockConnector> connectors = new();

    // === Общие методы для ВСЕ блоков ===

    public void SetCommand(ICommand cmd)
    {
        command = cmd;
        if (blockLabel != null) blockLabel.text = cmd.GetDisplayName();
        if (blockImage != null) blockImage.color = cmd.GetBlockColor();
    }

    public ICommand Command => command;
    public BlockConnector GetConnector(string name) => connectors[name];
    public BlockConnector GetExternalInput() => GetConnector("external_input");
    public BlockConnector GetExternalOutput() => GetConnector("external_output");

    public bool HasConnector(string name) => connectors.ContainsKey(name);
    public IEnumerable<BlockConnector> GetAllConnectors() => connectors.Values;

    // Перетаскивание - одинаково для всех
    public void OnBeginDrag(PointerEventData eventData) { /* ... */ }
    public void OnDrag(PointerEventData eventData) { /* ... */ }
    public void OnEndDrag(PointerEventData eventData) { /* ... */ }

    // Выравнивание - общее для всех
    public void AlignToInputConnection() { /* ... */ }

    // Подсветка - есть специфичность но интерфейс общий
    protected virtual void UpdateSnapVisuals(SnapManager.SnapInfo snapInfo) { }

    // === Виртуальные методы для подклассов ===

    protected abstract void InitializeConnectors();
    public virtual void RecalculateSize() { } // Override only if needed

    protected virtual void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
        programArea = GetComponentInParent<ProgramArea>();

        InitializeConnectors();
    }
}

// ===== ПРОСТОЙ БЛОК =====
public class SimpleBlockUI : BlockUIBase
{
    protected override void InitializeConnectors()
    {
        var rectInput = GetComponent<RectTransform>(); // или найти в Inspector
        var rectOutput = GetComponent<RectTransform>();

        connectors["external_input"] = new BlockConnector(
            BlockConnector.PointType.Input, rectInput);
        connectors["external_output"] = new BlockConnector(
            BlockConnector.PointType.Output, rectOutput);
    }

    // RecalculateSize не переопределяем - размер фиксирован
}

// ===== LOOP БЛОК =====
public class LoopBlockUI : BlockUIBase
{
    [SerializeField] private RectTransform internalOutputPoint;
    [SerializeField] private RectTransform internalInputPoint;
    [SerializeField] private RectTransform container;

    private LoopCommand loopCommand;

    protected override void InitializeConnectors()
    {
        // External - как всегда
        connectors["external_input"] = new BlockConnector(
            BlockConnector.PointType.Input, GetComponent<RectTransform>());
        connectors["external_output"] = new BlockConnector(
            BlockConnector.PointType.Output, GetComponent<RectTransform>());

        // Internal - специфичные для Loop
        connectors["internal_output"] = new BlockConnector(
            BlockConnector.PointType.Output, internalOutputPoint);
        connectors["internal_input"] = new BlockConnector(
            BlockConnector.PointType.Input, internalInputPoint);

        // Связи
        connectors["internal_output"].connectedTo = connectors["internal_input"];
    }

    public override void RecalculateSize()
    {
        // Пересчёт высоты для Loop
        BlockUI firstBlock = GetFirstInnerBlock();
        // ...
    }

    public BlockConnector GetInternalOutput() => GetConnector("internal_output");
    public BlockConnector GetInternalInput() => GetConnector("internal_input");

    public BlockUI GetFirstInnerBlock()
    {
        var internal_output = GetConnector("internal_output");
        return internal_output?.connectedTo?.parentBlock;
    }
}

// ===== IF БЛОК (Будущее) =====
public class IfBlockUI : BlockUIBase
{
    [SerializeField] private RectTransform trueOutputPoint;
    [SerializeField] private RectTransform falseOutputPoint;

    protected override void InitializeConnectors()
    {
        connectors["external_input"] = new BlockConnector(
            BlockConnector.PointType.Input, GetComponent<RectTransform>());

        // Два выхода вместо одного!
        connectors["true_output"] = new BlockConnector(
            BlockConnector.PointType.Output, trueOutputPoint);
        connectors["false_output"] = new BlockConnector(
            BlockConnector.PointType.Output, falseOutputPoint);
    }

    public BlockUI GetTrueBlock() => GetConnector("true_output")?.connectedTo?.parentBlock;
    public BlockUI GetFalseBlock() => GetConnector("false_output")?.connectedTo?.parentBlock;
}

// ===== IF-ELSE БЛОК (Будущее) =====
public class IfElseBlockUI : BlockUIBase
{
    protected override void InitializeConnectors()
    {
        connectors["external_input"] = /* ... */;
        connectors["if_branch_input"] = /* ... */;   // Вход в true блоки
        connectors["if_branch_output"] = /* ... */;  // Выход из true блоков
        connectors["else_branch_input"] = /* ... */; // Вход в false блоки
        connectors["else_branch_output"] = /* ... */;// Выход из false блоков
    }
}
```

### Как это использовать:

```csharp
// ===== SnapManager =====
public class SnapManager
{
    public SnapInfo FindNearestSnap(BlockUIBase draggingBlock)
    {
        // Работает с любым типом блока!
        var allConnectors = draggingBlock.GetAllConnectors();

        // Вычислить snap для каждого коннектора
        foreach (var connector in allConnectors)
        {
            // Снап работает единообразно
        }
    }
}

// ===== BlockFactory =====
public class BlockFactory
{
    public BlockUIBase CreateBlock(CommandType commandType)
    {
        BlockUIBase blockUI = commandType switch
        {
            CommandType.MoveForward => new SimpleBlockUI(), // Но как создать?
            CommandType.Loop => gameObject.AddComponent<LoopBlockUI>(),
            CommandType.If => gameObject.AddComponent<IfBlockUI>(),
            // ...
        };

        blockUI.SetCommand(CreateCommand(commandType));
        return blockUI;
    }
}

// ===== Использование в коде =====
void OnBlockPlaced(BlockUIBase block)
{
    // Работает с любым типом блока!
    var externalInput = block.GetExternalInput();
    var allConnectors = block.GetAllConnectors();

    block.RecalculateSize(); // Переопределено у LoopBlockUI, у SimpleBlockUI - пусто
}
```

### Плюсы гибридного подхода:
✅ **Унификация** - все блоки это BlockUIBase
✅ **Map коннекторов** - `GetConnector("name")` везде
✅ **Полиморфизм** - SnapManager работает с BlockUIBase
✅ **Гибкость** - каждый тип может иметь свои коннекторы
✅ **Расширяемость** - легко добавлять If, IfElse, Switch
✅ **Чистота** - общее в базе, специфичное в подклассах
✅ **Будущая готовность** - параметры, переменные как дополнительные коннекторы

### Минусы гибридного подхода:
⚠️ **Virtual methods** - небольшой overhead
⚠️ **Все на одном компоненте** - может быть тяжело для сложных блоков
⚠️ **Рефактор сейчас** - нужно переделывать текущий код

---

## Сравнение стратегий

| Критерий | Наследование | Composition | Гибрид |
|----------|--------------|-------------|--------|
| **Унификация** | ✅ Отличная | ❌ Плохая | ✅ Отличная |
| **Map коннекторов** | ✅ Да | ❌ Нет | ✅ Да |
| **Полиморфизм** | ✅ Да | ⚠️ Сложный | ✅ Да |
| **Простота** | ⚠️ Средняя | ✅ Простая | ⚠️ Средняя |
| **Расширяемость** | ✅ Отличная | ❌ Плохая | ✅ Отличная |
| **Текущий код** | ❌ Рефактор | ✓ No changes | ⚠️ Рефактор |
| **Будущие If/Switch** | ✅ Легко | ❌ Сложно | ✅ Легко |

---

## Мой анализ ваших аргументов

### Вы правы в:
1. ✅ **Много общего** - действительно 80% функциональности повторяется
2. ✅ **Нужна унификация** - SnapManager, BlockFactory должны работать с базовым типом
3. ✅ **Map коннекторов** - отличная идея для расширяемости
4. ✅ **Будущие типы** - If, IfElse, Switch имеют похожую структуру
5. ✅ **Агрегация** - BlockUI должен управлять и координировать

### Я ошибался в:
1. ❌ Рекомендовал Composition без план на будущее
2. ❌ Не учитывал масштабирование (If, IfElse, Switch)
3. ❌ Не видел проблем с дублированием логики
4. ❌ Недооценил ценность унификации через базовый класс

---

## Финальная рекомендация

### 🎯 **Гибридный подход (BlockUIBase + подклассы)**

**Почему:**
1. ✅ Решает все ваши аргументы про унификацию
2. ✅ Map коннекторов - можно расширять на будущее
3. ✅ Легко добавлять If, IfElse, Switch
4. ✅ SnapManager работает с BlockUIBase везде
5. ✅ Не теряем гибкость Composition
6. ✅ Масштабируется хорошо

**Шаги реализации:**
1. Создать `BlockUIBase : MonoBehaviour`
2. Переместить общую логику из `BlockUI` в `BlockUIBase`
3. Сделать `BlockUI : BlockUIBase` для простых блоков
4. Переделать `LoopBlockUI : BlockUIBase`
5. Использовать `BlockUIBase` в SnapManager, BlockFactory и т.д.
6. Добавить Map коннекторов вместо inputPoints/outputPoints

**Объём работы:** ~200-300 строк (не критично)

**Дальнейшее расширение:**
- If/IfElse добавятся легко как новые подклассы
- Параметры/переменные - новые записи в Map коннекторов
- Всё работает через единый интерфейс BlockUIBase

---

## Заключение

**Вы поставили отличный вопрос и были правы в анализе!**

Текущий Composition подход работает, но не масштабируется хорошо на будущее. Гибридный подход (наследование + Map) решает всё и готовит архитектуру на 2-3 года вперёд.

