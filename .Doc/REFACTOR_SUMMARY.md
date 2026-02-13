# Архитектурный рефактор BlockUI - Полная сводка

**Дата планирования:** 2026-01-16
**Статус:** ✅ Полный план готов
**Дата старта:** 2026-01-19 (воскресенье) или 2026-01-20 (понедельник)

---

## 🎯 Суть рефактора

### Текущая архитектура (Composition - работает, но не масштабируется)
```
GameObject LoopBlock
├── BlockUI (базовый функционал, drag-drop)
│   ├── inputPoints: List<BlockConnector>
│   ├── outputPoints: List<BlockConnector>
│   └── Общие методы для всех блоков
└── LoopBlockUI (sibling, Loop специфичное)
    ├── ExternalInput, ExternalOutput
    ├── InternalInput, InternalOutput
    └── RecalculateHeight()
```

**Проблемы:**
- ❌ GetComponent() вызовы везде
- ❌ Несогласованные интерфейсы (inputPoints vs ExternalInput)
- ❌ SnapManager знает о Loop специфике
- ❌ Сложно добавлять If, IfElse, Switch

---

### Целевая архитектура (Гибридный подход - масштабируется)
```
BlockUIBase : MonoBehaviour (abstract)
├── Dictionary<string, BlockConnector> connectors
├── GetConnector(name): BlockConnector
├── GetAllConnectors(): IEnumerable
├── Общие методы для ВСЕ блоков (OnDrag, AlignToInput, etc.)
├── abstract InitializeConnectors()
└── virtual RecalculateSize()

    SimpleBlockUI : BlockUIBase
    ├── InitializeConnectors() ✓
    └── RecalculateSize() (empty)

    LoopBlockUI : BlockUIBase
    ├── InitializeConnectors() ✓
    ├── RecalculateSize() ✓ (пересчёт размера)
    └── GetFirstInnerBlock()

    IfBlockUI : BlockUIBase (future)
    └── Два выхода (true/false)

    IfElseBlockUI : BlockUIBase (future)
    └── Две ветви с входом/выходом
```

**Преимущества:**
- ✅ Единый интерфейс через BlockUIBase
- ✅ Map коннекторов - гибкость и расширяемость
- ✅ SnapManager работает унифицированно
- ✅ Легко добавлять новые типы блоков
- ✅ Готовность к параметрам, переменным

---

## 📋 Что будет изменено

### Создание (1 новый файл)
```
✨ BlockUIBase.cs (~250 строк)
   └─ Abstract базовый класс для ВСЕ типов блоков
   └─ Dictionary<string, BlockConnector> connectors
   └─ Все общие методы (Awake, OnDrag, AlignToInput, etc.)
   └─ Abstract InitializeConnectors()
   └─ Virtual RecalculateSize()
```

### Переделка (2 основных файла)
```
📝 BlockUI.cs (~200 → ~80 строк)
   ├─ : BlockUIBase (вместо MonoBehaviour)
   ├─ Удалить все методы в BlockUIBase
   └─ Оставить только SimpleBlockUI специфичное

📝 LoopBlockUI.cs (~200 → ~150 строк)
   ├─ : BlockUIBase (вместо MonoBehaviour)
   ├─ InitializeConnectors() для Map коннекторов
   ├─ Helper методы (GetExternalInput, GetInternalOutput, etc.)
   └─ GetFirstInnerBlock() обновленный
```

### Обновления (5 файлов зависимостей)
```
⚙️ BlockFactory.cs
   └─ CreateBlock() может возвращать BlockUIBase

⚙️ SnapManager.cs
   └─ FindNearestSnap() работает с BlockUIBase
   └─ GetAllConnectors() вместо специальных проверок Loop

⚙️ BlockConnector.cs
   └─ parentBlock может быть BlockUIBase?
   └─ (может потребоваться тип-апдейт)

⚙️ ProgramArea.cs
   └─ Какие изменения требуются?

⚙️ GameManager.cs
   └─ Какие изменения требуются?
```

---

## 📅 График реализации (6 дней)

### День 1-2: Planning & Design (19-20 янв)
- [ ] Дизайн BlockUIBase
- [ ] Список точных изменений
- [ ] Стратегия миграции
- **Время:** 5-6 часов/день

### День 3-4: Implementation Phase 1 (21-22 янв)
- [ ] Создать BlockUIBase
- [ ] Переделать BlockUI
- [ ] Переделать LoopBlockUI
- **Время:** 8 часов/день

### День 5: Integration & Testing (23 янв)
- [ ] Обновить зависимости
- [ ] Comprehensive testing
- **Время:** 7-8 часов

### День 6-7: Cleanup & Docs (24-25 янв)
- [ ] Code cleanup
- [ ] Документация
- [ ] Final verification
- **Время:** 4-5 часов/день (опционально)

**Итого:** ~25 часов эффективной работы

---

## ✅ Что получим в результате

1. **Унифицированный интерфейс**
   - Все блоки наследуются от BlockUIBase
   - Map коннекторов вместо разных структур
   - GetConnector(name) везде

2. **Лучшая интеграция**
   - SnapManager работает унифицированно
   - BlockFactory проще создавать блоки
   - Нет специальных проверок для Loop

3. **Готовность к расширению**
   - If/IfElse/Switch добавятся легко
   - Параметры как дополнительные коннекторы
   - Переменные как дополнительные коннекторы

4. **Чистая архитектура**
   - Single Responsibility - каждый класс делает одно
   - Полиморфизм работает правильно
   - DRY - нет дублирования кода

---

## 🔍 Map коннекторов - ключевая идея

### Как это работает:

```csharp
// Инициализация в подклассе
protected override void InitializeConnectors()
{
    connectors["external_input"] = new BlockConnector(...);
    connectors["external_output"] = new BlockConnector(...);
    connectors["internal_input"] = new BlockConnector(...);  // Loop only
    connectors["internal_output"] = new BlockConnector(...); // Loop only
}

// Использование везде
public BlockConnector GetExternalInput()
    => GetConnector("external_input");

public BlockConnector GetInternalOutput()
    => GetConnector("internal_output");

// В SnapManager работает унифицированно
foreach (var connector in block.GetAllConnectors())
{
    // Работает для всех типов коннекторов!
    ProcessSnap(connector);
}
```

### Будущие расширения:

```csharp
// If блок
connectors["true_output"] = ...;
connectors["false_output"] = ...;

// IfElse блок
connectors["if_input"] = ...;
connectors["if_output"] = ...;
connectors["else_input"] = ...;
connectors["else_output"] = ...;

// Параметры
connectors["param_repeat_count"] = ...;  // числовой параметр
connectors["param_condition"] = ...;      // логический параметр

// Переменные (будущее)
connectors["var_iteration"] = ...;
connectors["var_result"] = ...;
```

---

## ⚡ Быстрая справка - что менялось

### BlockUIBase (НОВЫЙ)
```csharp
public abstract class BlockUIBase : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Общее для ВСЕ
    protected Dictionary<string, BlockConnector> connectors;

    protected virtual void Awake() { }
    public virtual void OnBeginDrag(PointerEventData eventData) { }
    public virtual void OnDrag(PointerEventData eventData) { }
    public virtual void OnEndDrag(PointerEventData eventData) { }
    public virtual void AlignToInputConnection() { }
    protected virtual void UpdateSnapVisuals(SnapInfo info) { }

    // Специфичное для подклассов
    protected abstract void InitializeConnectors();
    public virtual void RecalculateSize() { }
}
```

### BlockUI (ПЕРЕДЕЛАН)
```csharp
public class BlockUI : BlockUIBase
{
    protected override void InitializeConnectors()
    {
        connectors["external_input"] = new BlockConnector(...);
        connectors["external_output"] = new BlockConnector(...);
    }

    // Всё остальное от BlockUIBase
}
```

### LoopBlockUI (ПЕРЕДЕЛАН)
```csharp
public class LoopBlockUI : BlockUIBase
{
    protected override void InitializeConnectors()
    {
        connectors["external_input"] = new BlockConnector(...);
        connectors["external_output"] = new BlockConnector(...);
        connectors["internal_input"] = new BlockConnector(...);
        connectors["internal_output"] = new BlockConnector(...);
    }

    public override void RecalculateSize()
    {
        // Пересчёт высоты Loop
    }

    public BlockUI GetFirstInnerBlock()
    {
        var io = GetConnector("internal_output");
        return io?.connectedTo?.parentBlock;
    }
}
```

---

## 🧪 Тестовый план

Перед commit'ом нужно убедиться что:

1. **Базовые блоки работают**
   - [ ] Создать простой блок из палитры
   - [ ] Перетащить в ProgramArea
   - [ ] Выравнивание работает
   - [ ] Выполнение программы работает

2. **Loop блок работает**
   - [ ] Создать Loop блок
   - [ ] Добавить блоки внутри (1, 3, много)
   - [ ] Размер подстраивается
   - [ ] Выполнение (итерации)
   - [ ] Stop button работает

3. **Snap система работает**
   - [ ] Магнитный snap работает
   - [ ] Подсветка коннекторов
   - [ ] Вставка в начало
   - [ ] Вставка в середину
   - [ ] Удаление блоков

4. **Интеграция работает**
   - [ ] BlockFactory создаёт блоки
   - [ ] SnapManager работает унифицированно
   - [ ] Нет новых warnings
   - [ ] Нет regression'ов от Phase 4

---

## 📚 Документация

После завершения:

1. **BlockUIBase guide** - как создавать новые типы блоков
2. **Map коннекторов** - стандартные имена для коннекторов
3. **If/IfElse/Switch примеры** - как реализовать в будущем
4. **Issues.md update** - отметить #11a как DONE

---

## 🎬 Как начать

### Подготовка (сегодня):
1. [ ] Завершить Phase 4 cleanup
2. [ ] Прочитать Architecture_BlockUI_Strategy.md
3. [ ] Прочитать Tasks/11a_BlockUI_Refactor.md
4. [ ] Backup проекта

### День 19 (старт):
1. [ ] Завершить #11 Phase 5 (если ещё не сделано)
2. [ ] Создать Git branch: `feature/blockui-refactor`
3. [ ] Начать Day 1: Planning & Design

### Дни 3-5 (основная работа):
1. Следовать плану в Tasks/11a_BlockUI_Refactor.md
2. Коммитить после каждого этапа
3. Тестировать после каждого изменения

---

## 🏆 Финальный результат

**Архитектура готова к:**
- ✅ If/IfElse/Switch блокам (задачи #12-14)
- ✅ Block Parameters (задача #11b)
- ✅ Переменным и параметрам (future)
- ✅ Масштабированию на 2-3 года вперёд

**Код:**
- ✅ Чище и читабельнее
- ✅ Без дублирования
- ✅ Полиморфный и расширяемый
- ✅ Готов к production

---

## 📞 Вопросы перед стартом

Перед 19 января подумайте над:
1. Есть ли другие компоненты которые зависят от BlockUI.inputPoints?
2. Нужен ли backward compatibility слой?
3. Есть ли prefabs которые нужно обновлять?
4. Нужны ли какие-то специальные проверки для future If блоков?

---

**ПЛАН ГОТОВ К РЕАЛИЗАЦИИ! 🚀**

Документы:
- `.Doc/Tasks/11a_BlockUI_Refactor.md` - детальный технический план
- `.Doc/Architecture_BlockUI_Strategy.md` - архитектурный анализ
- `.Doc/PLAN_Week_Jan19-25.md` - день за днём план
- `.Doc/REFACTOR_SUMMARY.md` - этот файл

Начинаем 19 января! 🎯

