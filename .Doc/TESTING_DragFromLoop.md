# Тестирование #22: Вытаскивание блоков из Loop

**Дата:** 2026-01-22
**Статус:** Ready for Testing
**Реализация:** BlockDragHandler.BypassBlockInLoop()

---

## Что было сделано

Добавлен метод `BypassBlockInLoop()` в BlockDragHandler который:
1. Проверяет только PrimaryInput и PrimaryOutput (External коннекторы)
2. Ищет соединение с внутренними коннекторами (InternalOutput / InternalInput)
3. При нахождении → переподключает соответствующий противоположный коннектор
4. Вызывается в `OnBeginDrag()` ДО `DisconnectAllConnections()`

**Результат:** Loop соединения схлопываются СРАЗУ, блок вырывается из цепи

---

## Тестовые сценарии

### Setup для всех тестов:
1. Открыть SampleScene с инициализированной GameManager
2. Создать Loop блок
3. Добавить 3 блока внутрь Loop:
   - Block_A (MoveForward)
   - Block_B (TurnRight)
   - Block_C (MoveBackward)

Текущее состояние:
```
Loop [InternalOutput → A → B → C ← InternalInput]
```

---

### Test 1: Вытащить ПЕРВЫЙ блок (Block_A)

**Шаги:**
1. Начать перетаскивание Block_A
2. Посмотреть в Console:
   - ✓ Должен быть лог: `[DRAG] Loop bypassed FIRST block: Block_A`
3. Перетащить Block_A за пределы Loop (вверх)
4. Отпустить блок в ProgramArea (вне Loop)

**Проверка:**
- [ ] Console: лог о bypass-е
- [ ] Loop теперь содержит: [Block_B → Block_C]
- [ ] InternalOutput подключён к Block_B.input
- [ ] Block_A лежит отдельно в ProgramArea
- [ ] Нет ошибок / NullReferenceException

**Ожидаемые логи:**
```
[DRAG] Loop bypassed FIRST block: Block_A
[SNAP] No snap found or invalid snap
```

---

### Test 2: Вытащить ПОСЛЕДНИЙ блок (Block_C)

**Шаги:**
1. Вернуть состояние: Loop [A → B → C]
2. Начать перетаскивание Block_C
3. Посмотреть в Console:
   - ✓ Должен быть лог: `[DRAG] Loop bypassed LAST block: Block_C`
4. Перетащить Block_C за пределы Loop (вниз)
5. Отпустить блок в ProgramArea

**Проверка:**
- [ ] Console: лог о bypass-е
- [ ] Loop теперь содержит: [Block_A → Block_B]
- [ ] Block_B.output подключён к InternalInput
- [ ] Block_C лежит отдельно в ProgramArea
- [ ] Нет ошибок

**Ожидаемые логи:**
```
[DRAG] Loop bypassed LAST block: Block_C
```

---

### Test 3: ОДИН блок в Loop (вытащить его)

**Шаги:**
1. Удалить Block_B и Block_C, оставить только Block_A
2. Loop состояние: [A] (один блок)
3. Начать перетаскивание Block_A

**Проверка:**
- [ ] Console: лог `[DRAG] Loop bypassed FIRST block: Block_A`
  - (Он одновременно и первый, и последний, но bypass считает только первый)
- [ ] InternalOutput.connectedTo = null
- [ ] InternalInput остался не связанным
- [ ] Loop внутри пустой (CorrectlyEmpty)

**Ожидаемое поведение:**
```
InternalOutput → null (был → Block_A)
Block_A.input → null (был ← InternalOutput)
InternalInput → null (остался null)
```

---

### Test 4: Вытащить СРЕДНИЙ блок (Block_B) - Edge Case

**Шаги:**
1. Setup: Loop [A → B → C]
2. Начать перетаскивание Block_B
3. Посмотреть в Console:
   - ✗ НЕ должно быть логов о bypass-е (он не первый, не последний)

**Проверка:**
- [ ] Console: НЕТ логов о bypass-е
- [ ] Block_B остаётся в ProgramArea (вне Loop)
- [ ] Loop содержит: [A → C]
- [ ] Соединения: A.output → C.input ✓

**Ожидаемый результат:**
Блок просто удаляется из цепи (через DisconnectAllConnections), затем при snap или возврате в позицию...

> **ПРИМЕЧАНИЕ:** Edge case для вытаскивания из середины - это более сложная задача (не в scope #22). Пока проверяем что НЕ ломаются первый/последний.

---

### Test 5: Вытащить и переподключить в другое место

**Шаги:**
1. Setup: Loop [A → B → C], рядом есть Block_D (MoveBackward) снаружи Loop
2. Вытащить Block_A из Loop
3. В процессе перетаскивания приблизиться к Block_D
4. Защелкнуть Block_A.input к Block_D.output

**Проверка:**
- [ ] Block_A защелкнулся: Block_D.output → Block_A.input
- [ ] Loop соединения схлопнулись: InternalOutput → Block_B
- [ ] Визуальная цепь: Block_D → Block_A (снаружи), Loop [B → C] (внутри)
- [ ] Нет ошибок

---

### Test 6: Отпустить блок БЕЗ snap (возврат на место)

**Шаги:**
1. Setup: Loop [A → B → C]
2. Вытащить Block_B и тащить его в пустое место (далеко от других блоков)
3. Отпустить

**Проверка:**
- [ ] Block_B вернулась на ОРИГИНАЛЬНОЕ место (в Loop)
- [ ] Соединения восстановлены: A.output → B.input, B.output → C.input ✓
- [ ] Loop состояние: [A → B → C] (как было)

> **Примечание:** Это НЕ проверяет наш новый код (так как B не first/last), но важно что мы ничего не сломали

---

## Checklist завершения

- [ ] Test 1: Вытащить первый блок ✓
- [ ] Test 2: Вытащить последний блок ✓
- [ ] Test 3: Один блок в Loop ✓
- [ ] Test 4: Вытащить средний блок (не должно быть bypass логов) ✓
- [ ] Test 5: Вытащить и переподключить в другое место ✓
- [ ] Test 6: Отпустить блок без snap (возврат на место) ✓
- [ ] Console: нет NullReferenceException
- [ ] Console: нет других ошибок
- [ ] Логирование работает (все [DRAG] логи видны)

---

## Debug режим

Если тест не проходит, добавьте в BypassBlockInLoop() дополнительные логи:

```csharp
Debug.Log($"[DRAG] primaryInput = {primaryInput?.parentBlock.name}");
Debug.Log($"[DRAG] primaryOutput = {primaryOutput?.parentBlock.name}");
Debug.Log($"[DRAG] primaryInput.connectedTo?.role = {primaryInput?.connectedTo?.role}");
Debug.Log($"[DRAG] primaryOutput.connectedTo?.role = {primaryOutput?.connectedTo?.role}");
```

---

## Известные ограничения (для будущего)

- ❌ Вытаскивание блока из СЕРЕДИНЫ цепи → не реализовано (сложнее)
- ❌ Перетаскивание нескольких блоков вместе → не реализовано
- ❌ Drag внутри вложенных Loop-ов → не тестировалось

