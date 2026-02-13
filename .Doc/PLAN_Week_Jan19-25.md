# План на неделю: 19-25 января 2026

**Проект:** TestCodeBlock - Robot Programming Game
**Фокус:** Завершение Phase 5 (#11) + Архитектурный рефактор (#11a)
**Статус:** 📋 Готово к планированию

---

## 📅 Недельный обзор

### Фаза 1: Финализация (19-18 января - 3 дня)
- **Задача:** #11 Phase 5 - Полное тестирование Loop Block
- **Статус:** ДОЛЖНО быть завершено перед стартом #11a
- **Объём:** 6-8 часов тестирования

### Фаза 2: Архитектурный рефактор (19-25 января - 6 дней)
- **Задача:** #11a - BlockUI гибридный подход
- **Статус:** Planning → Implementation
- **Объём:** 25 часов (5 часов в день)

---

## 📊 Цели недели

### 🎯 Первичная цель (Critical)
✅ **Завершить #11 Phase 5 тестирование**
- Все 7 сценариев Loop Block должны пройти
- Нет regression'ов от Phase 4
- Code frozen перед рефактором

### 🎯 Вторичная цель (High)
✅ **Начать #11a рефактор**
- Design & Planning (дни 1-2)
- Implementation Phase 1 (дни 3-4)
- Готовность к финализации в конце недели

---

## 📝 День за днём

### День 1: Понедельник 19 января
**Время:** 5-6 часов
**Блок 1 (2-3 часа) - FINISHING #11 PHASE 5**
- Запустить все 7 тестовых сценариев Loop Block
- Выявить и записать найденные баги
- Исправить найденные проблемы

**Блок 2 (2-3 часа) - STARTING #11A PLANNING**
- Прочитать Architecture_BlockUI_Strategy.md
- Дизайн BlockUIBase
- Список точных изменений в каждом файле

**Deliverables:**
- [ ] 7/7 Phase 5 тестов пройдены (или задокументированы баги)
- [ ] Дизайн документ BlockUIBase готов
- [ ] Список файлов для изменения

---

### День 2: Вторник 20 января
**Время:** 5-6 часов
**Блок 1 (2-3 часа) - FINAL #11 VALIDATION**
- Убедиться что все баги исправлены
- Финальный тест Loop блока
- Prepare code freeze для #11a

**Блок 2 (2-3 часа) - #11A DETAILED DESIGN**
- Дизайн структуры Map коннекторов
- Стандартизация имён коннекторов
- Дизайн миграции (как переходить от текущей архитектуры)

**Deliverables:**
- [ ] #11 Phase 5 DONE
- [ ] Issues.md обновлён - #11 marked as DONE
- [ ] Детальный дизайн #11a готов

---

### День 3: Среда 21 января
**Время:** 8 часов
**Блок 1 (3-4 часа) - CREATE BlockUIBase**
- Создать новый файл BlockUIBase.cs
- Перенести общую функциональность из BlockUI
- Написать abstract методы
- ~250 строк кода

**Блок 2 (3-4 часа) - REFACTOR BlockUI**
- BlockUI : BlockUIBase
- Удалить дублированный код
- Оставить только специфичное для простых блоков
- ~80 строк (было ~200)

**Deliverables:**
- [ ] BlockUIBase.cs компилируется
- [ ] BlockUI.cs компилируется
- [ ] Нет breaking changes в интерфейсе

---

### День 4: Четверг 22 января
**Время:** 8 часов
**Блок 1 (4 часа) - REFACTOR LoopBlockUI**
- LoopBlockUI : BlockUIBase
- Обновить InitializeConnectors() для Map
- Обновить getter методы
- Удалить дублированный код
- ~150 строк (было ~200)

**Блок 2 (3-4 часа) - QUICK INTEGRATION TEST**
- BlockFactory - может ли создавать блоки?
- SnapManager - есть ли breaking changes?
- Первичное тестирование интеграции

**Deliverables:**
- [ ] LoopBlockUI.cs компилируется
- [ ] BlockFactory может создавать Simple и Loop блоки
- [ ] Нет явных ошибок интеграции

---

### День 5: Пятница 23 января
**Время:** 7-8 часов
**Блок 1 (3 часа) - UPDATE DEPENDENCIES**
- BlockFactory.cs - окончательные изменения
- SnapManager.cs - унификация поиска коннекторов
- BlockConnector.cs - если нужны изменения типов
- ProgramArea.cs - если нужны изменения

**Блок 2 (4-5 часов) - COMPREHENSIVE TESTING**
- Все 7 Phase 5 тестов Loop Block
- Базовые блоки - создание, перетаскивание, выполнение
- Snap система - все типы snap'ов
- Интеграция - BlockFactory, SnapManager, ProgramArea

**Deliverables:**
- [ ] Все зависимости обновлены
- [ ] Компилируется без ошибок
- [ ] 7/7 Phase 5 тестов проходят
- [ ] Нет новых warnings

---

### День 6: Суббота 24 января
**Время:** 4-5 часов (опционально, если нужно)
**Блок 1 (2 часа) - CLEANUP**
- Удалить неиспользуемый код
- Удалить старые compatibility layers
- Код review

**Блок 2 (2-3 часа) - DOCUMENTATION**
- Обновить комментарии в коде
- Документировать Map коннекторов
- Создать guide для будущих If блоков

**Deliverables:**
- [ ] Код очищен
- [ ] Комментарии обновлены
- [ ] Документация готова

---

### День 7: Воскресенье 25 января
**Время:** 2-3 часа (опционально)
**Блок 1 - FINAL VERIFICATION**
- Последний полный тест
- Проверка что ничего не сломалось
- Подготовка к коммиту

**Deliverables:**
- [ ] Everything works
- [ ] Ready for commit & review

---

## 🎯 Key Milestones

| День | Дата | Миля | Статус |
|------|------|------|--------|
| 1 | 19 янв | #11 Phase 5 начало | 📋 |
| 2 | 20 янв | #11 Phase 5 завершение | 📋 |
| 3 | 21 янв | BlockUIBase + BlockUI готовы | 💻 |
| 4 | 22 янв | LoopBlockUI готов | 💻 |
| 5 | 23 янв | Зависимости обновлены, полное тестирование | 🧪 |
| 6 | 24 янв | Cleanup & Docs (опционально) | 📝 |
| 7 | 25 янв | Final verification & commit ready | ✅ |

---

## 📊 Временная затраты

### По дням:
- День 1: 5-6 часов
- День 2: 5-6 часов
- День 3: 8 часов
- День 4: 8 часов
- День 5: 7-8 часов
- День 6: 4-5 часов (опционально)
- День 7: 2-3 часа (опционально)

**Итого:** 39-45 часов на неделю (без opcionалов: 35-40 часов)

### По задачам:
- #11 Phase 5: 6-8 часов (дни 1-2)
- #11a Refactor: 25 часов (дни 3-7)
- Buffer: ~5-10 часов (на unexpected issues)

---

## ⚠️ Риски и mitigation

### Риск 1: #11 Phase 5 не завершится быстро
**Вероятность:** MEDIUM
**Impact:** MEDIUM
**Mitigation:** Начать Phase 5 как можно раньше (зависит от current status)

### Риск 2: SnapManager требует больше изменений
**Вероятность:** MEDIUM
**Impact:** HIGH
**Mitigation:**
- День 5 уже забронирован для интеграции
- Buffer дни 6-7 для доп. работы

### Риск 3: Regression в тестировании
**Вероятность:** LOW-MEDIUM
**Impact:** CRITICAL
**Mitigation:**
- Весь день 5 посвящен тестированию
- Git branch для rollback

### Риск 4: Сложнее чем ожидалось
**Вероятность:** MEDIUM
**Impact:** MEDIUM
**Mitigation:**
- План гибкий - дни 6-7 буферные
- Можно продлить в следующую неделю если нужно

---

## 💾 Git Strategy

### Branch naming:
- Feature branch: `feature/blockui-refactor` (создать перед днем 3)
- Keep master clean до завершения #11a

### Commits (примерный план):
1. `refactor: create BlockUIBase abstract class`
2. `refactor: migrate BlockUI to BlockUIBase`
3. `refactor: migrate LoopBlockUI to BlockUIBase`
4. `refactor: update dependencies (SnapManager, BlockFactory)`
5. `test: verify all Phase 5 scenarios pass`
6. `docs: update documentation for BlockUIBase`

### Merging:
- Merge в master только когда ВСЕ тесты проходят
- Code review перед merge

---

## 📚 Resources & References

### Документы которые будут полезны:
1. `.Doc/Architecture_BlockUI_Strategy.md` - детальный analysis
2. `.Doc/Tasks/11a_BlockUI_Refactor.md` - полный план рефактора
3. `.Doc/Tasks/11_LoopBlock.md` - текущая реализация Loop
4. `.Doc/Analysis_LoopBlockUI_Inheritance.md` - почему не pure наследование

### Перед стартом нужно:
- [ ] Прочитать Architecture_BlockUI_Strategy.md
- [ ] Прочитать Tasks/11a_BlockUI_Refactor.md
- [ ] Привести к чистоте все Phase 5 материалы
- [ ] Создать feature branch

---

## ✅ Success Criteria

Неделя считается успешной когда:
- ✅ #11 Phase 5 завершена (все 7 тестов pass)
- ✅ BlockUIBase создан и работает
- ✅ BlockUI переделан и работает
- ✅ LoopBlockUI переделан и работает
- ✅ Все зависимости обновлены
- ✅ Все 7 Phase 5 тестов всё ещё pass
- ✅ Нет новых warnings/errors
- ✅ Документация обновлена
- ✅ Код готов к commit

---

## 🚀 Что дальше (неделя 26 янв - 1 февраля)

После успешного завершения #11a:
1. **#11b** - Block Parameters (параметры блоков)
2. **#12** - IfBlockUI (условный блок)
3. **#13** - IfElseBlockUI (условный с else)

Все будут намного проще внедрять благодаря гибридной архитектуре!

---

## 📌 Подготовка

До 19 января (ЭТОТ ДЕНЬ):
- [ ] Завершить Phase 4 cleanup & documentation
- [ ] Прочитать Architecture_BlockUI_Strategy.md
- [ ] Готовить environment для Phase 5 тестирования
- [ ] Сделать backup проекта на случай проблем

**После этого:** Готовы начинать неделю полную рефактора! 🚀

