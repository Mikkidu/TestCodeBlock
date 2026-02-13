# Решение: Безопасное вытаскивание блоков из Loop (KISS + SMART)

**Дата:** 2026-01-22
**Статус:** Proposal
**Приоритет:** 🟠 HIGH
**Сложность:** НИЗКАЯ (локализованное изменение в BlockDragHandler)

---

## Проблема

При перетаскивании блока из цепи внутри Loop:
1. `OnBeginDrag` вызывает `DisconnectAllConnections()` → разрываются ВСЕ связи
2. Включая внутренние соединения Loop (InternalOutput ↔ первый блок, последний блок ↔ InternalInput)
3. Если блок вернулся в Loop → соединения восстановятся через `ConnectInnerConnectors()`
4. Но это неправильно! Нужно восстановить ИМЕННО эти соединения, а не создавать новые

**Пример:**
```
Loop [InternalOutput → [Block1] → [Block2] ← InternalInput]

При вытаскивании Block1:
  ✗ ТЕКУЩЕЕ: DisconnectAll() → InternalOutput.connectedTo = null, Block1.input.connectedTo = null
  ✓ НУЖНО: Переподключить InternalOutput к Block2.input
```

---

## Решение (KISS + SMART)

**Принцип:** Запомнить Loop-соединения перед разрывом → восстановить их при OnEndDrag.

### Структура решения

```
BlockDragHandler.OnBeginDrag()
  ↓
1. DetectLoopConnections() — запомнить Loop-связи ДО разрыва
  - Есть ли входящее соединение от InternalOutput?
  - Есть ли исходящее соединение к InternalInput?
  ↓
2. DisconnectAllConnections() — разорвать все связи
  ↓
3. Перетаскивание...
  ↓
BlockDragHandler.OnEndDrag()
  ↓
4. Если блок НЕ защелкнулся в новый snap:
   RestoreLoopConnections() — восстановить Loop-связи
  ↓
5. Если блок защелкнулся:
   Ничего не делать (новые snap-связи работают)
```

---

## Реализация

### Шаг 1: Добавить поля в BlockDragHandler

```csharp
// После строки 19, перед Awake():

// Loop connection tracking
private LoopBlockUI connectedLoopBlock = null;
private bool wasFirstBlockInLoop = false;    // InternalOutput → input
private bool wasLastBlockInLoop = false;     // output → InternalInput
```

### Шаг 2: Метод DetectLoopConnections()

```csharp
/// <summary>
/// Check if this block is connected to a Loop (first or last in chain).
/// Must be called BEFORE DisconnectAllConnections().
/// </summary>
private void DetectLoopConnections()
{
    connectedLoopBlock = null;
    wasFirstBlockInLoop = false;
    wasLastBlockInLoop = false;

    // Check INPUTS: Is InternalOutput connected to our input?
    foreach (var inputConnector in parentBlock.GetInputConnectors())
    {
        if (inputConnector.connectedTo?.role == BlockConnector.ConnectorRole.InternalOutput)
        {
            connectedLoopBlock = inputConnector.connectedTo.parentBlock as LoopBlockUI;
            wasFirstBlockInLoop = true;
            Debug.Log($"[DRAG] Block '{parentBlock.name}' is FIRST in Loop");
            return;
        }
    }

    // Check OUTPUTS: Is InternalInput connected to our output?
    foreach (var outputConnector in parentBlock.GetOutputConnectors())
    {
        if (outputConnector.connectedTo?.role == BlockConnector.ConnectorRole.InternalInput)
        {
            connectedLoopBlock = outputConnector.connectedTo.parentBlock as LoopBlockUI;
            wasLastBlockInLoop = true;
            Debug.Log($"[DRAG] Block '{parentBlock.name}' is LAST in Loop");
            return;
        }
    }
}
```

### Шаг 3: Модифицировать OnBeginDrag()

Найти строку 42-60 и добавить:

```csharp
public void OnBeginDrag(PointerEventData eventData)
{
    // NEW: Remember Loop connections BEFORE disconnecting
    DetectLoopConnections();

    if (canvasGroup != null)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    originalParent = transform.parent;
    originalSiblingIndex = transform.GetSiblingIndex();

    // Disconnect from any connected blocks when starting drag (Stage 6)
    parentBlock.DisconnectAllConnections();

    // Move to root canvas for dragging
    if (rootCanvas != null)
    {
        transform.SetParent(rootCanvas.transform, true);
    }
}
```

### Шаг 4: Метод RestoreLoopConnections()

```csharp
/// <summary>
/// Restore Loop connections if block wasn't snapped to a new target.
/// Called from OnEndDrag when block returns to original position or drops outside snap.
/// </summary>
private void RestoreLoopConnections()
{
    if (connectedLoopBlock == null)
        return;

    if (wasFirstBlockInLoop)
    {
        // Block was first: restore InternalOutput → input
        var internalOutput = connectedLoopBlock.GetConnector("internal_output");
        var blockInput = parentBlock.GetPrimaryInput();

        if (internalOutput != null && blockInput != null)
        {
            internalOutput.connectedTo = blockInput;
            blockInput.connectedTo = internalOutput;
            Debug.Log($"[RESTORE] Restored: Loop.InternalOutput → {parentBlock.name}.input");
        }
    }

    if (wasLastBlockInLoop)
    {
        // Block was last: restore output → InternalInput
        var blockOutput = parentBlock.GetPrimaryOutput();
        var internalInput = connectedLoopBlock.GetConnector("internal_input");

        if (blockOutput != null && internalInput != null)
        {
            blockOutput.connectedTo = internalInput;
            internalInput.connectedTo = blockOutput;
            Debug.Log($"[RESTORE] Restored: {parentBlock.name}.output → Loop.InternalInput");
        }
    }

    // Reset tracking variables
    connectedLoopBlock = null;
    wasFirstBlockInLoop = false;
    wasLastBlockInLoop = false;
}
```

### Шаг 5: Модифицировать OnEndDrag()

Найти строку 92-150 и добавить логику перед `ReturnToOriginalPosition()`:

```csharp
public void OnEndDrag(PointerEventData eventData)
{
    if (canvasGroup != null)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
    }

    // Reset connector colors to normal
    parentBlock.ResetAllConnectorColors();

    // Palette blocks should NOT apply snap in OnEndDrag
    if (!parentBlock.inProgramArea)
    {
        // NEW: If it was from a Loop, restore its connections
        RestoreLoopConnections();
        ReturnToOriginalPosition();
        return;
    }

    // Check if we can apply snap
    if (programArea == null && rootCanvas != null)
    {
        programArea = rootCanvas.GetComponentInChildren<ProgramArea>();
    }

    if (parentBlock.Command.Type == CommandType.Loop)
    {
        ((LoopBlockUI)parentBlock).ConnectInnerConnectors();
    }

    if (programArea != null)
    {
        SnapManager snapManager = programArea.GetSnapManager();
        if (snapManager != null)
        {
            SnapManager.SnapInfo snapInfo = snapManager.FindNearestSnap(parentBlock, programArea.GetBlocks());

            if (snapInfo.canSnap && snapInfo.targetConnector != null)
            {
                // Block snapped successfully → don't restore Loop connections
                connectedLoopBlock = null;
                wasFirstBlockInLoop = false;
                wasLastBlockInLoop = false;

                // Apply snap based on type
                if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.InputToOutput)
                {
                    snapManager.ApplySnap(parentBlock, snapInfo.targetConnector, programArea);
                }
                else if (snapInfo.snapType == SnapManager.SnapInfo.SnapType.OutputToInput)
                {
                    snapManager.ApplySnapToInput(parentBlock, snapInfo.targetConnector, programArea);
                }
            }
            else
            {
                // No snap possible → restore Loop connections if was in Loop
                RestoreLoopConnections();
                ReturnToOriginalPosition();
            }
        }
        else
        {
            RestoreLoopConnections();
            ReturnToOriginalPosition();
        }
    }
    else
    {
        RestoreLoopConnections();
        ReturnToOriginalPosition();
    }
}
```

---

## Тестовый сценарий

### Setup:
```
Loop
├─ Block_A (MoveForward)
├─ Block_B (TurnRight)
└─ Block_C (MoveBackward)
```

### Тест 1: Вытащить Block_A (первый)
1. Начало перетаскивания
   - `DetectLoopConnections()` → wasFirstBlockInLoop = true
   - `DisconnectAllConnections()` → разрыв всех связей
2. Перетащить Block_A вверх (за пределы Loop)
3. Отпустить
   - No snap found
   - `RestoreLoopConnections()` → InternalOutput подключится к Block_B.input
4. ✓ Результат: Loop теперь содержит [Block_B → Block_C]

### Тест 2: Вытащить Block_C (последний)
1. Перетащить Block_C вниз (за пределы Loop)
2. Отпустить
   - No snap found
   - `RestoreLoopConnections()` → Block_B.output подключится к InternalInput
3. ✓ Результат: Loop теперь содержит [Block_A → Block_B]

### Тест 3: Вытащить и переподключить
1. Перетащить Block_B (первый)
2. Отпустить на MoveForward (ниже Loop)
   - Snap found → ApplySnap()
   - Block_A.output → Block_B.input (физическое соединение)
   - Block_A.output → InternalInput (внутреннее соединение)
3. ✓ Результат: Loop содержит только Block_C, Block_B снаружи

---

## Acceptance Criteria

- [ ] Метод `DetectLoopConnections()` добавлен в BlockDragHandler
- [ ] Метод `RestoreLoopConnections()` добавлен в BlockDragHandler
- [ ] `OnBeginDrag()` вызывает `DetectLoopConnections()` перед `DisconnectAllConnections()`
- [ ] `OnEndDrag()` вызывает `RestoreLoopConnections()` когда блок не защелкнулся
- [ ] Тест 1: Вытаскивание первого блока работает
- [ ] Тест 2: Вытаскивание последнего блока работает
- [ ] Тест 3: Вытаскивание и переподключение работает
- [ ] Внутренние соединения Loop сохраняются при вытаскивании
- [ ] Нет NullReferenceException
- [ ] Console логирует все этапы (DETECT, DISCONNECT, RESTORE)

---

## Риски & Смягчение

| Риск | Смягчение |
|------|-----------|
| Неправильное восстановление соединений | Валидация: check null перед подключением |
| Баги Loop size при восстановлении | `RecalculateSize()` вызовется при следующем движении |
| Множественное восстановление | Обнуляем переменные после восстановления |

---

## Дополнительно (Future)

Эта система также подготавливает нас к:
- Вытаскиванию блоков из СЕРЕДИНЫ Loop (более сложный случай)
- Перетаскиванию ЦЕПИ блоков вместо одного
- Drag & Drop для If/IfElse блоков (аналогичная логика)

