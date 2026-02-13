# BugFix #10b.1: Сдвиг блоков при вставке из ProgramArea не работает

**Дата обнаружения:** 2026-01-14
**Приоритет:** 🔴 CRITICAL
**Статус:** PENDING

## Проблема

При перетаскивании **существующего блока** из ProgramArea в середину цепи, сдвиг блоков не срабатывает. При этом вставка **новых блоков** (с палитры) работает правильно.

### Симптомы

```
Цепь: [A] → [B] → [C]

Действие 1: Новый блок X из палитры
├─ Берём X за INPUT
├─ Вставляем в середину между B и C
└─ Результат: [A] → [B] → [X] → [C] ✓ (работает - блоки выравнены)

Действие 2: Существующий блок D из ProgramArea
├─ Берём D за INPUT (D уже в программе)
├─ Вставляем в середину между B и C
└─ Результат: [A] → [B] → [D] [C] ✗ (НЕ работает - блоки наложены)
```

### Видимые эффекты

- ✓ Новый блок вставляется в центром: соединение правильное + визуальное выравнивание
- ✗ Существующий блок вставляется со сдвигом: соединение правильное, но нет выравнивания (наложение визуальное)

## Анализ кода

### OnEndDrag() логика в BlockUI.cs:

```csharp
public void OnEndDrag(PointerEventData eventData)
{
    // ...

    // Palette blocks should NOT apply snap in OnEndDrag
    // They are handled by ProgramArea.OnDrop() which creates a copy
    if (!inProgramArea)  // <-- Новые блоки с палитры
    {
        ReturnToOriginalPosition();
        return;
    }

    // Только для блоков уже в ProgramArea
    if (snapArea != null)
    {
        // Priority 1: Try OUTPUT → INPUT snap (insert at beginning)
        if (outputPoints.Count > 0)
        {
            snapInfo = snapManager.FindNearestInput(this, snapArea.GetBlocks());
            if (snapInfo.canSnap && snapInfo.targetConnector != null)
            {
                snapManager.ApplySnapToInput(this, outputPoints[0], snapInfo.targetConnector);
                return;  // <-- Выход после успеха
            }
        }

        // Priority 2: Try INPUT → OUTPUT snap (append at end)
        if (!snapInfo.canSnap && inputPoints.Count > 0)
        {
            snapInfo = snapManager.FindNearestOutput(this, snapArea.GetBlocks());
            if (snapInfo.canSnap && snapInfo.targetConnector != null)
            {
                snapManager.ApplySnap(this, inputPoints[0], snapInfo.targetConnector);
                return;  // <-- Выход после успеха
            }
        }

        // No snap possible, return block to original position
        ReturnToOriginalPosition();
    }
}
```

### Разница между новым и существующим блоком

1. **Новый блок (из палитры):**
   - `inProgramArea == false`
   - OnEndDrag вызывает ReturnToOriginalPosition()
   - Вставка происходит в ProgramArea.OnDrop() → создаётся копия
   - **ApplySnapToInput вызывается с новым объектом**

2. **Существующий блок (в ProgramArea):**
   - `inProgramArea == true`
   - OnEndDrag вызывает ApplySnapToInput напрямую
   - **ApplySnapToInput работает с тем же объектом**

### Возможная причина

Вероятно, разница в том как блоки перемещаются из палитры:

1. **ProgramArea.OnDrop()** при добавлении из палитры:
   - Создаёт новый блок (копию)
   - Может быть там вызывается дополнительное выравнивание?

2. **OnEndDrag()** при перемещении в ProgramArea:
   - Напрямую вызывает ApplySnapToInput без дополнительной обработки?

## Шаги воспроизведения

1. Откройте GameScene в Play режиме
2. Создайте цепь из 3 блоков: [A] → [B] → [C]
3. Берём блок B за INPUT
4. Вставляем между A и C
5. **Ожидание:** Блок B должен переместиться, C должен сдвинуться вниз
6. **Реальность:** B встаёт на место C (наложение), C не движется

## Ожидаемое поведение

При вставке **любого** блока (нового или существующего) в середину цепи:
- INPUT вставляемого блока выравнивается к OUTPUT источника
- OUTPUT вставляемого блока выравнивается к INPUT целевого
- Все последующие блоки сдвигаются вниз (через AlignToInputConnection cascade)
- Нет визуального наложения блоков

## Предполагаемые решения

### Гипотеза 1: Разница в вызове ApplySnapToInput

**Проверить:**
- Вызов ApplySnapToInput одинаков для новых и существующих?
- Может быть нужен дополнительный flagging или предварительное отключение?

### Гипотеза 2: Проблема с DisconnectInput()

**Проверить:**
- OnBeginDrag вызывает DisconnectInput() для существующих блоков
- Может быть это разрушает соединение раньше чем нужно?
- Для новых блоков DisconnectInput() вызывается в ProgramArea.OnDrop()?

### Гипотеза 3: Порядок операций

**Проверить:**
- При вставке существующего блока: сначала позиционируется? потом переконектируется?
- Может быть нужно поменять порядок операций в ApplySnapToInput?

## Тестовый код для проверки

```csharp
// В BlockUI.OnEndDrag перед ApplySnapToInput добавить логирование:

Debug.Log($"[DEBUG SNAP] inProgramArea: {inProgramArea}");
Debug.Log($"[DEBUG SNAP] Block: {gameObject.name}");
Debug.Log($"[DEBUG SNAP] Current position: {(transform as RectTransform).position}");

snapManager.ApplySnapToInput(this, outputPoints[0], snapInfo.targetConnector);

Debug.Log($"[DEBUG SNAP] After ApplySnapToInput position: {(transform as RectTransform).position}");
```

## Checklist для исследования

- [ ] Сравнить логи OnEndDrag для нового и существующего блока
- [ ] Проверить вызов ApplySnapToInput в обоих случаях
- [ ] Проверить вызов AlignToInputConnection для cascade
- [ ] Проверить позиции блоков до и после ApplySnap
- [ ] Проверить connectedTo связи после вставки

## Notes

- Код был изменён при добавлении приоритизации в FindNearestOutput/FindNearestInput
- Может быть это обнаружило существующий баг, а не создало новый?
- Нужно тщательное тестирование как новых, так и существующих блоков

## Статус исследования

🔴 **Требуется исследование** - нужен debug процесс для определения точной причины
