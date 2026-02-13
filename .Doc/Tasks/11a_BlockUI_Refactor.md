# #11a Архитектурный рефактор BlockUI - Гибридный подход

**Статус:** Planning (начало на 2026-01-19)
**Приоритет:** 🟠 HIGH (подготовка к #12 - Block Parameters и будущим If/IfElse)
**Сложность:** Medium (200-300 строк, 5-7 дней)
**Зависимости:** #11 Phase 5 (тестирование) - должно быть DONE перед стартом

---

## 🎯 Цель

Переработать архитектуру BlockUI с Composition подхода на гибридный:
- **Базовый класс** `BlockUIBase` для всех типов блоков
- **Map коннекторов** вместо List для гибкости и унификации
- **Полиморфизм** через наследование
- **Подготовка** к будущим If, IfElse, Switch блокам

**Результат:** Единообразный интерфейс для SnapManager, BlockFactory и т.д.

---

## 📋 Предусловия

- [x] #11 Phase 4 - Code Cleanup: DONE
- [ ] #11 Phase 5 - Тестирование: ДОЛЖНО быть DONE перед стартом
- [ ] Backup проекта (на случай проблем)
- [ ] Git branch: `feature/blockui-refactor`

---

## 📐 Архитектурные изменения

### Текущая структура (будет удалена):
```
BlockUI : MonoBehaviour
├── inputPoints: List<BlockConnector>
├── outputPoints: List<BlockConnector>
└── Специфичные методы

LoopBlockUI : MonoBehaviour (sibling)
├── ExternalInput: BlockConnector
├── ExternalOutput: BlockConnector
├── InternalInput: BlockConnector
├── InternalOutput: BlockConnector
└── Свои методы
```

### Новая структура (target):
```
BlockUIBase : MonoBehaviour (abstract)
├── connectors: Dictionary<string, BlockConnector>
├── GetConnector(name): BlockConnector
├── GetAllConnectors(): IEnumerable<BlockConnector>
├── SetCommand(cmd): void
├── AlignToInputConnection(): void
├── OnBeginDrag/OnDrag/OnEndDrag(): virtual
└── InitializeConnectors(): abstract
└── RecalculateSize(): virtual

    SimpleBlockUI : BlockUIBase
    ├── InitializeConnectors() ✓
    └── RecalculateSize() (empty)

    LoopBlockUI : BlockUIBase
    ├── InitializeConnectors() ✓
    ├── RecalculateSize() ✓
    └── GetFirstInnerBlock(): BlockUI
    └── GetInternalOutput(): BlockConnector

    IfBlockUI : BlockUIBase (future)
    ├── InitializeConnectors() ✓
    ├── GetTrueBlock(): BlockUI
    └── GetFalseBlock(): BlockUI

    IfElseBlockUI : BlockUIBase (future)
    └── ...
```

---

## 🗓️ Сроки и этапы

### День 1-2 (19-20 января) - PLANNING & DESIGN
**Объём:** 4-6 часов

- [ ] **Дизайн BlockUIBase**
  - Что идёт в базовый класс?
  - Какие методы virtual?
  - Структура Map коннекторов
  - Имена коннекторов (стандартизация)

- [ ] **Дизайн подклассов**
  - SimpleBlockUI - что переопределяет?
  - LoopBlockUI - как мигрировать?
  - IfBlockUI прототип (скелет кода)

- [ ] **Стратегия миграции**
  - Какой порядок переделывать?
  - Как обновлять зависимости?
  - Fallback план если что-то сломается

**Deliverables:**
- Подробный дизайн документ
- Список точных изменений в каждом файле
- Тестовый план

---

### День 3-4 (21-22 января) - IMPLEMENTATION PHASE 1

**Объём:** 8-10 часов

#### Шаг 1: Создать BlockUIBase (3-4 часа)
```
Файл: Assets/Scripts/RobotProgramming/UI/BlockUIBase.cs (новый)

Что добавить:
- abstract class BlockUIBase : MonoBehaviour
- Dictionary<string, BlockConnector> connectors
- GetConnector(name), GetAllConnectors()
- SetCommand() - общая функциональность
- AlignToInputConnection() - оптимизированная
- OnBeginDrag/OnDrag/OnEndDrag - перемещено из BlockUI
- UpdateSnapVisuals() - виртуальный метод
- abstract InitializeConnectors()
- virtual RecalculateSize()
- Все остальное общее из текущего BlockUI

Объём: ~250 строк
```

**Проверки:**
- [ ] Компилируется без ошибок
- [ ] Все методы перенесены корректно
- [ ] No breaking changes в интерфейсе

---

#### Шаг 2: Переделать BlockUI (2-3 часа)
```
Файл: Assets/Scripts/RobotProgramming/UI/BlockUI.cs

Что менять:
- BlockUI : BlockUIBase (вместо MonoBehaviour)
- Удалить все общие методы (перенесены в BlockUIBase)
- Оставить простую реализацию InitializeConnectors()
  * inputPoints и outputPoints можно удалить (теперь в Map)
  * Или оставить compatibility layer для других компонентов
- RecalculateSize() - пусто (или удалить)
- Проверить что inputPointVisual, outputPointsVisuals в Inspector

Риск: ⚠️ СРЕДНИЙ - Много других компонентов зависят от BlockUI
Решение: Compatibility layer - свойства inputPoints/outputPoints как wrapper'ы
```

**Проверки:**
- [ ] Компилируется
- [ ] GetComponent<BlockUI>() всё ещё работает
- [ ] inputPoints и outputPoints работают (если нужны другим компонентам)
- [ ] InitializeConnectors() вызывается из базовой Awake()

---

#### Шаг 3: Переделать LoopBlockUI (2-3 часа)
```
Файл: Assets/Scripts/RobotProgramming/UI/LoopBlockUI.cs

Что менять:
- LoopBlockUI : BlockUIBase (вместо MonoBehaviour)
- Удалить Awake/OnDestroy (заменить на base.Awake())
- InitializeConnectors() переработать для Map:
  * connectors["external_input"] = ExternalInput
  * connectors["external_output"] = ExternalOutput
  * connectors["internal_input"] = InternalInput
  * connectors["internal_output"] = InternalOutput

- Добавить helper методы:
  * GetExternalInput() => GetConnector("external_input")
  * GetExternalOutput() => GetConnector("external_output")
  * GetInternalInput() => GetConnector("internal_input")
  * GetInternalOutput() => GetConnector("internal_output")

- Обновить GetFirstInnerBlock():
  * var io = GetConnector("internal_output")
  * return io?.connectedTo?.parentBlock

- RecalculateSize() переделать если нужно

Объём: ~100 строк (много удаляется, мало добавляется)
```

**Проверки:**
- [ ] Компилируется
- [ ] GetComponent<LoopBlockUI>() работает
- [ ] InitializeConnectors() создаёт все 4 коннектора
- [ ] LoopCommand всё ещё может вызвать SetLoopBlockUI()

---

### День 5 (23 января) - INTEGRATION & TESTING

**Объём:** 6-8 часов

#### Шаг 4: Обновить зависимости (2-3 часа)

```
Файлы для обновления:

1. BlockFactory.cs
   - Может ли CreateBlock() возвращать BlockUIBase вместо BlockUI?
   - Обновить создание loop блока
   - Тестировать создание блоков

2. SnapManager.cs
   - Переделать поиск коннекторов:
     * Было: ищем в block.outputPoints
     * Будет: ищем в block.GetAllConnectors()
   - Удалить специальные проверки для Loop
   - Работает унифицированно для всех типов

3. ProgramArea.cs
   - Какие изменения нужны?
   - Может ли работать с BlockUIBase?

4. BlockConnector.cs
   - Может ли parentBlock быть BlockUIBase вместо BlockUI?
   - Обновить типы если нужно

5. BlockUI.cs (компоненты которые ищут BlockUI)
   - Найти все GetComponent<BlockUI>()
   - Какие нужны обновления?

Риск: ⚠️ ВЫСОКИЙ - много зависимостей!
```

**Проверки:**
- [ ] Компилируется
- [ ] Все GetComponent<BlockUI>() работают (или обновлены на BlockUIBase)
- [ ] BlockFactory создаёт блоки корректно
- [ ] SnapManager работает с новой архитектурой

---

#### Шаг 5: Тестирование (3-4 часа)

```
Test Plan:

Базовые блоки:
- [ ] Создать простой блок из палитры
- [ ] Перетащить его в ProgramArea
- [ ] Выравнивание работает
- [ ] Выполнение программы работает

Loop блок:
- [ ] Создать Loop блок
- [ ] Добавить блоки внутри
- [ ] RecalculateSize() работает
- [ ] Выполнение программы (итерации)
- [ ] Stop button работает

Snap система:
- [ ] Магнитный snap работает
- [ ] Подсветка коннекторов работает
- [ ] Вставка в начало работает
- [ ] Вставка в середину работает

Интеграция:
- [ ] Никаких новых warnings в Console
- [ ] Нет regression'ов от Phase 4
- [ ] На prefabs нет ошибок
```

---

### День 6-7 (24-25 января) - CLEANUP & DOCUMENTATION

**Объём:** 4-6 часов

#### Шаг 6: Cleanup (2 часа)
- [ ] Удалить неиспользуемый код из BlockUI
- [ ] Удалить старые compatibility layers если не нужны
- [ ] Проверить что нет dead code

#### Шаг 7: Документация (2-3 часа)
- [ ] Обновить комментарии в BlockUIBase
- [ ] Документировать Map коннекторов
- [ ] Создать guide для будущих If/IfElse блоков
- [ ] Обновить Issues.md и Tasks документацию

#### Шаг 8: Final Testing (1 час)
- [ ] Полный тест всех 7 сценариев Loop
- [ ] Полный тест всех базовых блоков
- [ ] Проверка что ничего не сломалось

---

## 📝 Файлы для изменения

### Создание (новые файлы):
```
✨ Assets/Scripts/RobotProgramming/UI/BlockUIBase.cs
  └─ Abstract базовый класс для всех блоков
  └─ ~250 строк кода
```

### Переделка (основные):
```
📝 Assets/Scripts/RobotProgramming/UI/BlockUI.cs
  └─ : BlockUIBase (вместо MonoBehaviour)
  └─ Удалить общие методы (в BlockUIBase)
  └─ ~200 строк (было), ~80 строк (станет)

📝 Assets/Scripts/RobotProgramming/UI/LoopBlockUI.cs
  └─ : BlockUIBase (вместо MonoBehaviour)
  └─ Обновить InitializeConnectors() для Map
  └─ Обновить методы для new API
  └─ ~200 строк (было), ~150 строк (станет)
```

### Обновления (зависимости):
```
⚙️ Assets/Scripts/RobotProgramming/UI/BlockFactory.cs
  └─ CreateBlock() может возвращать BlockUIBase
  └─ Обновить создание Loop блока

⚙️ Assets/Scripts/RobotProgramming/UI/SnapManager.cs
  └─ Переделать поиск коннекторов
  └─ Работает унифицированно для всех типов
  └─ Может избавиться от специальных проверок Loop

⚙️ Assets/Scripts/RobotProgramming/UI/BlockConnector.cs
  └─ Может parentBlock быть BlockUIBase?

⚙️ Assets/Scripts/RobotProgramming/UI/ProgramArea.cs
  └─ Какие изменения нужны?

⚙️ Assets/Scripts/RobotProgramming/Managers/GameManager.cs
  └─ Какие изменения нужны?
```

### Документация:
```
📄 .Doc/Tasks/11a_BlockUI_Refactor.md (этот файл)
  └─ Обновить с результатами рефактора

📄 .Doc/Architecture_BlockUI_Strategy.md
  └─ Уже есть detailed analysis

📄 .Doc/Issues.md
  └─ Добавить #11a как subtask
```

---

## ⚠️ Риски и mitigation

### Риск 1: Много зависимостей в SnapManager
**Вероятность:** HIGH
**Воздействие:** MEDIUM
**Mitigation:**
- Подробно изучить SnapManager перед стартом
- Может потребоваться рефактор поиска коннекторов
- Обновить FindNearestSnap() и FindNearestInput/Output()

### Риск 2: Backward compatibility
**Вероятность:** MEDIUM
**Воздействие:** HIGH
**Mitigation:**
- Compatibility layer для inputPoints/outputPoints если нужны другим компонентам
- Добавить wrapper свойства

### Риск 3: Breaking changes в BlockFactory
**Вероятность:** MEDIUM
**Воздействие:** HIGH
**Mitigation:**
- Тщательное тестирование CreateBlock()
- Проверить что prefabs всё ещё работают

### Риск 4: Regression в Phase 5 тестах
**Вероятность:** MEDIUM
**Воздействие:** CRITICAL
**Mitigation:**
- Перед рефактором - Phase 5 должна быть DONE
- Запустить все 7 тестов после рефактора
- Git branch для rollback если нужно

---

## 📊 Временная оценка

| Этап | День | Часы | Статус |
|------|------|------|--------|
| Planning & Design | 19-20 янв | 5 | 📋 |
| Implementation Phase 1 | 21-22 янв | 8 | 💻 |
| Integration & Testing | 23 янв | 7 | 🧪 |
| Cleanup & Documentation | 24-25 янв | 5 | 📝 |
| **TOTAL** | **6 дней** | **25 часов** | **~1 неделя** |

**Буфер:** 1-2 дня на unexpected issues

---

## ✅ Definition of Done

Рефактор считается завершённым когда:

1. ✅ BlockUIBase создан и работает
2. ✅ BlockUI переделан на наследование
3. ✅ LoopBlockUI переделан на наследование
4. ✅ Все зависимости обновлены
5. ✅ Все 7 Phase 5 тестов проходят
6. ✅ Нет новых warnings в Console
7. ✅ Код очищен и задокументирован
8. ✅ Git commit с описанием

**Acceptance Criteria:**
- [ ] `GetConnector(name)` работает везде
- [ ] SnapManager работает унифицированно
- [ ] BlockFactory создаёт все типы блоков
- [ ] Нет regression'ов
- [ ] Архитектура готова к If/IfElse блокам

---

## 🚀 Next Steps After Refactor

После завершения #11a можно:

1. **#11b** - Параметры блоков (Block Parameters)
   - Добавить parameter коннекторы
   - Реализовать UI для параметров

2. **#12** - IfBlockUI
   - Наследовать от BlockUIBase
   - Реализовать true/false ветви

3. **#13** - IfElseBlockUI
   - Наследовать от BlockUIBase

4. **#14** - SwitchBlockUI
   - Наследовать от BlockUIBase

**Все будут в масштабируемой архитектуре!** ✅

---

## 📌 Checklist для старта

До 19 января:
- [ ] Завершить #11 Phase 5 (все 7 тестов должны пройти)
- [ ] Создать Git branch `feature/blockui-refactor`
- [ ] Backup проекта (на случай проблем)
- [ ] Прочитать Architecture_BlockUI_Strategy.md
- [ ] Подготовить environment

---

## Примечания

- Это не срочно, но стратегически важно для масштабирования
- Лучше делать сейчас пока код ещё относительно простой
- После этого добавлять If/IfElse/Switch будет намного легче
- План гибкий - можно переносить дни если нужно

