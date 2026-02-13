# #11 Блок цикла (Loop Block)

## Goal
Создать блок цикла с 4 коннекторами и специальной логикой выполнения:
- **Бесконечное выполнение** (пока не нажат Stop или условие не станет false)
- **Динамический размер** - автоматически подстраивается под количество вложенных блоков
- **Логика через возврат** - Loop определяет откуда пришёл вызов (извне или из итерации)

## Визуальная архитектура

```
┌──●────────────────────────────┐  ← внешний INPUT (20 от левого края)
│          HEADER (300x50)      │
│            ЦИКЛ (∞)           │
├──────┬──●─────────────────────┘  ← внутренний OUTPUT (70 от левого края)
│      │     ↓
│ LEFT │  [блок 1]
│ SLICE│     ↓
│ (50) │  [блок 2]
│      │     ↓
│      │  [блок N]
│      │     ↓
├──────┴──○───────────────┐        ← внутренний INPUT (70 от левого края)
│        FOOTER (250x25)  │
└──○──────────────────────┘        ← внешний OUTPUT (20 от левого края)

Размеры:
- Стандартный блок: 200x50
- Header: 300x50
- Footer: 250x25
- Left Slice: ширина 50, sliced по вертикали
- Отступ left slice: верх 50, низ 25
- Минимальный gap между внутренними коннекторами: 25
```

## Логика выполнения

```
Вход в Loop (внешний INPUT)
    ↓
┌─> Проверка условия (true? не пустой?)
│       ├─ ДА → внутренний OUTPUT → первый блок внутри
│       │              ↓
│       │        [выполнение блоков по цепочке]
│       │              ↓
│       │        последний блок
│       │              ↓
│       └──────── внутренний INPUT (возврат в Loop)
│                      ↓
│                снова проверка условия ←──┘
│
└─ НЕТ/пустой → внешний OUTPUT → следующий блок после Loop
```

**Ключевая идея:** Loop сам определяет откуда пришёл вызов:
- Если через **внешний INPUT** → начало цикла, iteration = 0
- Если через **внутренний INPUT** → конец итерации, iteration++, проверка условия

## Context
- Есть система snap с физическими соединениями через BlockConnector
- Блоки связываются через `connectedTo` поле
- Выполнение идёт по цепочке через `GetNextBlock()`
- Promise-based execution для асинхронных команд

## Implementation Plan

### Phase 1: Базовая инфраструктура ✓ DONE
- [✓] Добавить `CommandType.Loop` в enum
- [✓] Создать `LoopCommand.cs` (базовая версия)
- [✓] Создать `LoopBlockUI.cs` (базовая версия)
- [✓] Интеграция в `BlockFactory.cs`
- [✓] Интеграция в `GameManager.cs` (Stop button)
- [✓] Добавить Loop в `BlockPalette.cs`

### Phase 2: Префаб в Unity ✓ DONE
- [✓] Создать LoopBlockUI.prefab с базовой структурой
- [✓] Настроить визуальные элементы (header, footer, left slice)

### Phase 4: Новая архитектура с 4 коннекторами (✓ DONE - 2026-01-16)

#### Step 8.1: BlockConnector - добавить ConnectorRole ✓
**Файл:** `Assets/Scripts/RobotProgramming/UI/BlockConnector.cs`

```csharp
// Добавить enum
public enum ConnectorRole
{
    External,        // обычные блоки (вход/выход)
    InternalOutput,  // Loop: верхний внутренний (передаёт управление внутрь)
    InternalInput    // Loop: нижний внутренний (принимает возврат из итерации)
}

// Добавить поля
public ConnectorRole role = ConnectorRole.External;
public LoopBlockUI ownerLoop;  // для внутренних коннекторов - ссылка на Loop
```

#### Step 8.2: LoopBlockUI - полная переработка ✓
**Файл:** `Assets/Scripts/RobotProgramming/UI/LoopBlockUI.cs`

```csharp
public class LoopBlockUI : MonoBehaviour
{
    // 4 коннектора
    [Header("External Connectors")]
    [SerializeField] private RectTransform externalInputPoint;   // внешний вход
    [SerializeField] private RectTransform externalOutputPoint;  // внешний выход

    [Header("Internal Connectors")]
    [SerializeField] private RectTransform internalOutputPoint;  // верхний внутренний
    [SerializeField] private RectTransform internalInputPoint;   // нижний внутренний

    [Header("Visual Elements")]
    [SerializeField] private RectTransform header;      // 300x50
    [SerializeField] private RectTransform footer;      // 250x25
    [SerializeField] private RectTransform leftSlice;   // 50 ширина, sliced
    [SerializeField] private RectTransform container;   // основной контейнер

    // Размеры
    private const float HEADER_HEIGHT = 50f;
    private const float FOOTER_HEIGHT = 25f;
    private const float MIN_INNER_GAP = 25f;
    private const float LEFT_SLICE_WIDTH = 50f;

    // BlockConnector объекты
    public BlockConnector ExternalInput { get; private set; }
    public BlockConnector ExternalOutput { get; private set; }
    public BlockConnector InternalOutput { get; private set; }
    public BlockConnector InternalInput { get; private set; }

    // Методы
    public void RecalculateHeight();           // пересчёт размера контейнера
    public BlockUI GetFirstInnerBlock();       // первый блок внутри цикла
    public BlockUI GetBlockAfterLoop();        // блок подключённый к внешнему output
    public float GetInnerContentHeight();      // высота содержимого
}
```

**Логика расчёта высоты:**
```
totalHeight = HEADER_HEIGHT + innerContentHeight + FOOTER_HEIGHT
innerContentHeight = max(MIN_INNER_GAP, расстояние от internalOutput.y до lastBlock.output.y)
```

#### Step 8.3: LoopCommand - новая логика выполнения
**Файл:** `Assets/Scripts/RobotProgramming/Commands/LoopCommand.cs`

```csharp
public class LoopCommand : CommandBase
{
    private LoopBlockUI loopBlockUI;
    private int currentIteration = 0;
    private bool shouldStop = false;

    public void SetLoopBlockUI(LoopBlockUI ui) => loopBlockUI = ui;

    // Основной метод - вызывается из внешнего входа
    public override IPromise Execute(IRobotController robot, ExecutionContext context)
    {
        Debug.Log("[LOOP] Entering from EXTERNAL input");
        currentIteration = 0;
        shouldStop = false;
        return ExecuteIteration(robot, context);
    }

    // Вызывается когда управление вернулось через внутренний input
    public IPromise ExecuteFromInternalInput(IRobotController robot, ExecutionContext context)
    {
        Debug.Log($"[LOOP] Returned from iteration {currentIteration}");
        currentIteration++;
        return ExecuteIteration(robot, context);
    }

    private IPromise ExecuteIteration(IRobotController robot, ExecutionContext context)
    {
        // Проверка условия остановки
        if (shouldStop || !robot.IsExecuting)
        {
            Debug.Log($"[LOOP] Stopped after {currentIteration} iterations");
            return ContinueAfterLoop(robot, context);
        }

        // Проверка условия цикла (пока всегда true)
        if (!CheckCondition())
        {
            Debug.Log($"[LOOP] Condition false, exiting after {currentIteration} iterations");
            return ContinueAfterLoop(robot, context);
        }

        // Получить первый блок внутри
        BlockUI firstInner = loopBlockUI.GetFirstInnerBlock();

        if (firstInner == null)
        {
            Debug.Log("[LOOP] Empty loop, continuing to next block");
            return ContinueAfterLoop(robot, context);
        }

        Debug.Log($"[LOOP] Starting iteration {currentIteration + 1}");
        context.SetVariable("loop_iteration", currentIteration);

        // Передать управление первому блоку внутри
        // Когда цепочка дойдёт до внутреннего input - вызовется ExecuteFromInternalInput
        return firstInner.Command.Execute(robot, context);
    }

    private IPromise ContinueAfterLoop(IRobotController robot, ExecutionContext context)
    {
        BlockUI nextBlock = loopBlockUI.GetBlockAfterLoop();
        if (nextBlock != null && nextBlock.Command != null)
        {
            return nextBlock.Command.Execute(robot, context);
        }
        return Deferred.Resolved();
    }

    private bool CheckCondition()
    {
        // Пока бесконечный цикл - всегда true
        // В будущем здесь будет проверка условия (iteration < maxCount, etc.)
        return true;
    }

    public void RequestStop() => shouldStop = true;
}
```

#### Step 8.4: Snap логика - поддержка внутренних коннекторов
**Файлы:** `SnapManager.cs`, `BlockUI.cs`

Изменения:
1. `FindNearestSnap()` должен также искать среди `InternalOutput` коннекторов Loop блоков
2. При подключении к `InternalOutput`:
   - Блок становится первым внутри Loop
   - При отпускании блока - вызвать `loopBlockUI.RecalculateHeight()`
3. При подключении выхода блока к `InternalInput`:
   - Это последний блок в цепочке внутри Loop
   - Вызвать `loopBlockUI.RecalculateHeight()`
4. Добавить логику распознавания что блок находится "внутри" Loop

#### Step 8.5: Обновить Loop prefab в Unity
- Добавить 4 RectTransform для коннекторов с правильными позициями:
  - externalInputPoint: (20, контейнер.top)
  - externalOutputPoint: (20, контейнер.bottom)
  - internalOutputPoint: (70, header.bottom)
  - internalInputPoint: (70, footer.top)
- Настроить header (300x50), footer (250x25), leftSlice (50 ширина, Image Sliced)
- Присвоить ссылки в Inspector

**Статус:** ✓ DONE (2026-01-16)
- ✓ All steps 8.1-8.5 completed
- ✓ Loop работает правильно
- ✓ Code cleanup завершён

### Phase 5: Полное тестирование всех сценариев (NEXT)

| # | Сценарий | Ожидание |
|---|----------|----------|
| 1 | Пустой Loop | Минимальная высота (50+25+25=100) |
| 2 | 1 блок внутри | Высота = 50 + 50 + 25 = 125 |
| 3 | 3 блока внутри | Высота увеличивается соответственно |
| 4 | Удалить блок | Высота уменьшается |
| 5 | Выполнение | Итерации работают (логи видны) |
| 6 | Stop | Прерывание работает |
| 7 | Loop в цепи | A → Loop → B выполняется правильно |

## Acceptance Criteria
- [ ] Loop имеет 4 коннектора (2 внешних + 2 внутренних)
- [ ] Блоки можно подключать к внутреннему OUTPUT (верхнему)
- [ ] Последний блок автоматически подключается к внутреннему INPUT (нижнему)
- [ ] Размер Loop динамически пересчитывается
- [ ] Выполнение: итерации работают через возврат управления
- [ ] Stop button прерывает цикл
- [ ] Debug логи: [LOOP] Starting iteration N, [LOOP] Returned from iteration N

## Known Limitations & Future Improvements

### 🔴 CRITICAL (Нужна доделка перед Phase 5 тестированием)
1. **Пересчёт при вставке в начало/середину** - Loop размер не обновляется при вставке блока в начало или середину цепи внутри Loop
2. **Размер вложенного Loop** - Когда Loop внутри Loop, outer Loop не пересчитывает размер при изменении inner Loop

### 🟠 HIGH (Желательно для удобства)
3. **Stop при reset** - При Reset robot state программа должна остановиться, а не продолжать Loop
4. **Lock UI во время выполнения** - Запретить перетаскивание блоков и использование палитры во время выполнения программы
5. **Размер при удалении** - Пересчёт высоты Loop при удалении блока из его цепи

## Blockers & Risks (Resolved)
- ✓ Определение "последнего блока" - решено через прямой доступ `connectedTo` и `OnAlignmentComplete` событие
- ✓ Циклические ссылки - защита через исключение dragging block из поиска snap
- ✓ Вложенные циклы - архитектура поддерживает, тестирование отложено на Phase 5.2

## Architecture Decisions & Trade-offs

### ✓ Event-driven approach
**Выбор:** Использование `OnAlignmentComplete` события вместо polling/poling
**Причины:**
- Более эффективно (O(1) вместо O(n))
- Не требует постоянных вычислений
- Более реактивно

### ✓ Direct connector references
**Выбор:** Использование `connectedTo` поля для прямого доступа вместо поиска по цепи
**Причины:**
- O(1) вместо O(n) сложности
- BlockConnector всегда знает к кому подключён
- Упрощает логику

### ✓ World-space coordinates for height calculation
**Выбор:** Использование `position.y` и `lossyScale` вместо local coordinates
**Причины:**
- Элементы в разных parent иерархиях
- World space обходит эту проблему
- Более надёжно при скейлировании

## Notes
- Реализован бесконечный цикл (условие всегда true в CheckCondition())
- Параметр repeatCount будет добавлен в задаче #12 (Block Parameters)
- Вложенные циклы (Loop в Loop) поддерживаются архитектурой, но требуют полного тестирования
- Left Slice был удалён из кода (не использовался) - оставлен в префабе для будущих улучшений

## Files Modified/Created

### Created (Phase 1):
- `Assets/Scripts/RobotProgramming/Commands/LoopCommand.cs`
- `Assets/Scripts/RobotProgramming/UI/LoopBlockUI.cs`
- `Assets/PrefabsUI/LoopBlockUI.prefab`

### Modified (Phase 1):
- `Assets/Scripts/RobotProgramming/Data/CommandType.cs` - добавлен Loop
- `Assets/Scripts/RobotProgramming/UI/BlockFactory.cs` - поддержка Loop
- `Assets/Scripts/RobotProgramming/Managers/GameManager.cs` - Stop для Loop
- `Assets/Scripts/RobotProgramming/UI/BlockPalette.cs` - Loop в палитре

### Modified (Phase 4):
- ✓ `Assets/Scripts/RobotProgramming/UI/BlockConnector.cs` - ConnectorRole enum + ownerLoop поле
- ✓ `Assets/Scripts/RobotProgramming/UI/LoopBlockUI.cs` - полная переработка с 4 коннекторами
- ✓ `Assets/Scripts/RobotProgramming/Commands/LoopCommand.cs` - новая логика выполнения
- ✓ `Assets/Scripts/RobotProgramming/UI/SnapManager.cs` - поддержка внутренних коннекторов Loop
- ✓ `Assets/Scripts/RobotProgramming/UI/BlockUI.cs` - добавлен OnAlignmentComplete event
- ✓ `Assets/PrefabsUI/LoopBlockUI.prefab` - обновлён с 4 коннекторами

### Code Cleanup (Phase 4):
- ✓ BlockUI.cs - удалён закомментированный код поиска (lines 526-547)
- ✓ LoopBlockUI.cs - удалены неиспользуемые методы GetInnerContentHeight(), GetTotalHeight()
- ✓ LoopBlockUI.cs - удалены неиспользуемые leftSlice и LEFT_SLICE_WIDTH
- ✓ LoopCommand.cs - очищено закомментированное условие (line 55)
- 📄 Анализ документирован в Analysis_11_CodeCleanup.md
