# #23 BUG: Позиционирование сброшенных блоков в локальных координатах

## Goal
Исправить критический баг позиционирования блоков при перетаскивании. Сброшенные блоки (наследники BlockUIBase) должны позиционироваться в своих родительских контейнерах в **локальных координатах** правильно, независимо от того, какой размер у родителя (свернут он на весь канвас или частично).

**Текущая проблема:** Родитель должен быть расширен на весь канвас, иначе положение сброса и позиционирования не совпадают.

**Желаемое поведение:** Блоки позиционируются правильно в любом иерархическом контексте, включая вложенные Loop контейнеры.

## Context
- После перетаскивания блок переходит из rootCanvas в ProgramArea или Loop контейнер через `SetParent(parent, true)`
- `true` параметр сохраняет мировую позицию, но локальные координаты могут быть неправильными
- Методы выравнивания блоков (`AlignToInputConnection()`, `ApplySnap()`, `ApplySnapToInput()`) используют мировые координаты (`rect.position`)
- Это работает только когда родитель совпадает с rootCanvas, но ломается для вложенных контейнеров

## Architecture Overview

```
Пример сценария с ошибкой:
[Canvas (0, 0)]
├─ [ProgramArea (500, 200)]        ← родитель
│  ├─ [Block A (100, 50)]
│  └─ [Block B (100, 150)]         ← при вставке координаты ломаются

Проблема:
- Block B рассчитывает offset в мировых координатах
- Применяет offset к мировой позиции (rect.position)
- SetParent меняет иерархию, но код не конвертирует в локальные координаты
- Результат: Block B оказывается в неправильной позиции внутри ProgramArea
```

## Key Steps

### Шаг 1: Исправить AlignToInputConnection()
**Файл:** `BlockUIBase.cs:199-226`

Заменить прямое манипулирование `rect.position` на вызов нового метода `SetWorldPosition()`.

Текущий код:
```csharp
Vector2 outputPos = connectedOutput.GetWorldPosition();
Vector2 myInputPos = myInput.GetWorldPosition();
Vector2 offset = outputPos - myInputPos;

RectTransform rect = GetComponent<RectTransform>();
if (rect != null)
{
    rect.position = new Vector3(
        rect.position.x + offset.x,
        rect.position.y + offset.y,
        rect.position.z
    );
}
```

Новый код:
```csharp
Vector2 outputPos = connectedOutput.GetWorldPosition();
Vector2 myInputPos = myInput.GetWorldPosition();
Vector2 offset = outputPos - myInputPos;

RectTransform rect = GetComponent<RectTransform>();
if (rect != null && offset.magnitude > 0.1f)
{
    // Вычислить новую мировую позицию
    Vector3 currentWorldPos = rect.position;
    Vector3 newWorldPos = currentWorldPos + new Vector3(offset.x, offset.y, 0);

    // Позиционировать в локальных координатах родителя
    SetWorldPosition(newWorldPos);

    Debug.Log($"[ALIGN] {gameObject.name} aligned to {connectedOutput.parentBlock?.gameObject.name} by offset ({offset.x:F1}, {offset.y:F1})");
}
```

Это полностью делегирует конвертацию координат методу `SetWorldPosition()`, который сам разберется с рекурсиями и иерархией.

### Шаг 2: Исправить ApplySnap()
**Файл:** `SnapManager.cs:217-307`

Заменить прямое манипулирование `blockRect.position` на вызов `draggingBlock.SetWorldPosition()`.

Текущий код (строка 341-354):
```csharp
Vector2 targetPosition = targetOutput.GetWorldPosition();
Vector2 currentInputWorldPos = inputPoint.GetWorldPosition();
Vector2 offset = targetPosition - currentInputWorldPos;

RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
if (blockRect != null && offset.magnitude > 0.1f)
{
    blockRect.position = new Vector3(
        blockRect.position.x + offset.x,
        blockRect.position.y + offset.y,
        blockRect.position.z
    );
}
```

Новый код:
```csharp
Vector2 targetPosition = targetOutput.GetWorldPosition();
Vector2 currentInputWorldPos = inputPoint.GetWorldPosition();
Vector2 offset = targetPosition - currentInputWorldPos;

RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
if (blockRect != null && offset.magnitude > 0.1f)
{
    // Вычислить новую мировую позицию
    Vector3 newWorldPos = blockRect.position + new Vector3(offset.x, offset.y, 0);

    // Позиционировать через BlockUIBase метод (правильная конвертация координат)
    draggingBlock.SetWorldPosition(newWorldPos);

    Debug.Log($"  → Shift {draggingBlock.gameObject.name} by ({offset.x:F1}, {offset.y:F1})");
}
```

**Дополнительно:** После установки позиции убедиться что SetParent использует правильный параметр (строка 303):
```csharp
// После ApplySnap вернуть в ProgramArea
if (draggingBlock.inProgramArea && programArea != null)
{
    // SetWorldPosition уже установил anchoredPosition, можно использовать false
    draggingBlock.transform.SetParent(programArea.transform, false);
}
```

### Шаг 3: Исправить ApplySnapToInput()
**Файл:** `SnapManager.cs:310-369`

Заменить прямое манипулирование `blockRect.position` на вызов `draggingBlock.SetWorldPosition()`.

Текущий код (строка 341-354):
```csharp
Vector2 targetPosition = targetInput.GetWorldPosition();
Vector2 currentOutputWorldPos = outputPoint.GetWorldPosition();
Vector2 offset = targetPosition - currentOutputWorldPos;

RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
if (blockRect != null && offset.magnitude > 0.1f)
{
    blockRect.position = new Vector3(
        blockRect.position.x + offset.x,
        blockRect.position.y + offset.y,
        blockRect.position.z
    );
}
```

Новый код:
```csharp
Vector2 targetPosition = targetInput.GetWorldPosition();
Vector2 currentOutputWorldPos = outputPoint.GetWorldPosition();
Vector2 offset = targetPosition - currentOutputWorldPos;

RectTransform blockRect = draggingBlock.GetComponent<RectTransform>();
if (blockRect != null && offset.magnitude > 0.1f)
{
    // Вычислить новую мировую позицию
    Vector3 newWorldPos = blockRect.position + new Vector3(offset.x, offset.y, 0);

    // Позиционировать через BlockUIBase метод (правильная конвертация координат)
    draggingBlock.SetWorldPosition(newWorldPos);

    Debug.Log($"  → Shift {draggingBlock.gameObject.name} by ({offset.x:F1}, {offset.y:F1})");
}
```

**Дополнительно:** После установки позиции убедиться что SetParent использует правильный параметр (строка 363-366):
```csharp
// После ApplySnapToInput вернуть в ProgramArea
if (draggingBlock.inProgramArea && programArea != null)
{
    // SetWorldPosition уже установил anchoredPosition, можно использовать false
    draggingBlock.transform.SetParent(programArea.transform, false);
}
```

### Шаг 4: Создать публичный метод позиционирования в BlockUIBase
**Файл:** `BlockUIBase.cs`

Создать публичный метод, который инкапсулирует всю логику конвертации координат. Метод не требует параметров — сам получает доступ к нужным компонентам через `this`, кеширует их при необходимости.

```csharp
/// <summary>
/// Позиционировать блок в мировой позиции, конвертируя в локальные координаты родителя.
/// Используется для выравнивания при snap-е и drag-е.
/// </summary>
public void SetWorldPosition(Vector3 worldPosition)
{
    RectTransform rect = GetComponent<RectTransform>();
    if (rect == null) return;

    // Получить родителя и Canvas
    RectTransform parentRect = rect.parent as RectTransform;
    if (parentRect == null || rootCanvas == null || rootCanvas.worldCamera == null)
    {
        // Fallback: если нет родителя или canvas, используем прямое позиционирование
        rect.position = worldPosition;
        return;
    }

    // Конвертировать мировую позицию в локальные координаты родителя
    Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(rootCanvas.worldCamera, worldPosition);

    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        parentRect,
        screenPos,
        rootCanvas.worldCamera,
        out Vector2 localPos);

    rect.anchoredPosition = localPos;
}
```

Затем использовать этот метод в AlignToInputConnection(), ApplySnap(), ApplySnapToInput() вместо прямого манипулирования rect.position/anchoredPosition.

### Шаг 5: Проверить SetParent вызовы
**Файлы:** `BlockDragHandler.cs`, `SnapManager.cs`, `ProgramArea.cs`

После внедрения `SetWorldPosition()` логика SetParent упрощается:
- `SetParent(rootCanvas, true)` во время drag - сохраняет мировую позицию ✓ (правильно, блок "плывет" за мышкой)
- `SetParent(parent, false)` после snap - используется ЕСЛИ anchoredPosition уже установлена через SetWorldPosition()

Проверить и исправить строки:
- BlockDragHandler.cs:106 - `SetParent(rootCanvas.transform, true)` ✓ (оставить как есть)
- BlockDragHandler.cs:204 - `SetParent(originalParent, false)` ✓ (оставить как есть, возврат на место)
- SnapManager.cs:303 - **ИСПРАВИТЬ:** `SetParent(programArea.transform, true)` → `false`
  - Потому что `SetWorldPosition()` уже установил anchoredPosition правильную
  - `false` параметр сохранит эту локальную позицию
- SnapManager.cs:365 - **ИСПРАВИТЬ:** `SetParent(programArea.transform, true)` → `false`
  - Аналогично ApplySnap
- ProgramArea.cs:139 - `SetParent(transform, false)` ✓ (оставить как есть)

### Шаг 6: Тестирование в разных сценариях
1. **Простой ProgramArea:** Бросить блок в ProgramArea, проверить позицию
2. **Вложенный Loop:** Бросить блок в контейнер Loop, проверить локальные координаты
3. **Вставка в цепь:** Перетащить блок в середину цепи, проверить выравнивание
4. **Множественные вставки:** A→B→C, вставить X между A и B, потом Y между X и B, проверить визуальное совпадение

### Шаг 7: Debug и валидация
- Добавить временные логи в AlignToInputConnection():
  ```csharp
  Debug.Log($"[ALIGN DEBUG] block={gameObject.name} offset=({offset.x:F1},{offset.y:F1}) worldPos=({newWorldPos.x:F1},{newWorldPos.y:F1}) localPos=({localPos.x:F1},{localPos.y:F1})");
  ```
- Проверить в Inspector что `anchoredPosition` совпадает с визуальным положением
- Сравнить мировую позицию до и после SetParent

## Acceptance Criteria
- [ ] AlignToInputConnection() правильно конвертирует координаты через RectTransformUtility
- [ ] ApplySnap() применяет позиционирование через локальные координаты
- [ ] ApplySnapToInput() применяет позиционирование через локальные координаты
- [ ] Блоки позиционируются правильно в ProgramArea, независимо от его размера
- [ ] Блоки позиционируются правильно в Loop контейнерах (вложенные блоки)
- [ ] Вставка в цепь работает корректно (визуально блоки совпадают)
- [ ] Цепь выполняется в правильном порядке после позиционирования
- [ ] SetParent использует правильные параметры (true для сохранения мировых, false когда локальные уже установлены)
- [ ] Debug логи показывают конвертацию координат

## Blockers & Risks

### Риски при использовании SetWorldPosition():
1. **Поле rootCanvas может быть null** если BlockUIBase не инициализирована правильно
   - Решение: В SetWorldPosition() проверить `rootCanvas != null && rootCanvas.worldCamera != null`
   - Fallback: использовать прямое rect.position если условия не выполнены

2. **Родитель может не иметь RectTransform** (теоретически, если кто-то игрется с иерархией)
   - Решение: В SetWorldPosition() проверить `parentRect != null`, иначе fallback

3. **Canvas.worldCamera может быть null** если Canvas находится в UI world space
   - Решение: Проверить и использовать fallback в SetWorldPosition()

4. **Масштаб и ротация вложенных контейнеров** - RectTransformUtility должен это учитывать, но нужно тестировать
   - Решение: Полное тестирование (Шаг 6-7)

5. **Цепочка вызовов AlignToInputConnection** - если установка позиции неправильная, ошибка распространится на все следующие блоки
   - Решение: Добавить debug логирование в SetWorldPosition() для контроля

### Преимущества нового подхода:
- SetWorldPosition() — единая точка контроля для всей логики конвертации
- Если найденная проблема в конвертации, нужно исправить только одно место
- Проще тестировать: можно написать unit тест для SetWorldPosition()
- Гибко: можно добавить кеширование или оптимизации в одном методе

## Notes

### Новый публичный метод SetWorldPosition()
Ключевой компонент решения:
```csharp
public void SetWorldPosition(Vector3 worldPosition)
{
    // Не требует параметров, получает доступ к нужным компонентам сам:
    // - rect.parent → parentRect
    // - GetComponent<RectTransform>() → rect
    // - rootCanvas → уже есть поле в BlockUIBase
    //
    // Преимущества:
    // 1. Инкапсуляция логики в одном месте
    // 2. Легко кешировать компоненты если понадобится оптимизация
    // 3. Сам разбирается с иерархией и масштабами
    // 4. Вызывается как простой: block.SetWorldPosition(newWorldPos)
}
```

### Координатные системы в Unity UI:
- **rect.position** - мировая позиция (world space)
- **rect.anchoredPosition** - локальная позиция в родителе (local space)
- `SetParent(parent, true)` - сохраняет мировую позицию, меняет локальную
- `SetParent(parent, false)` - сохраняет локальную позицию, меняет мировую

### RectTransformUtility методы:
```csharp
// Конвертировать мировую позицию в локальную
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    rect,                                    // какой RectTransform считать родителем
    RectTransformUtility.WorldToScreenPoint(...),  // сначала мировую в экранную
    canvas.worldCamera,                      // камера для перспективы
    out Vector2 localPoint);
```

### Практические примеры использования SetWorldPosition()

**Пример 1: В AlignToInputConnection()**
```csharp
// До: rect.position = new Vector3(rect.position.x + offset.x, ...);
// После:
Vector3 newWorldPos = GetComponent<RectTransform>().position + new Vector3(offset.x, offset.y, 0);
SetWorldPosition(newWorldPos);
```

**Пример 2: В ApplySnap() при вставке в цепь**
```csharp
// До: blockRect.position = new Vector3(...);
// После:
Vector3 newWorldPos = blockRect.position + new Vector3(offset.x, offset.y, 0);
draggingBlock.SetWorldPosition(newWorldPos);
```

**Пример 3: В ApplySnapToInput() при вставке в начало**
```csharp
// До: blockRect.position = new Vector3(...);
// После:
Vector3 newWorldPos = blockRect.position + new Vector3(offset.x, offset.y, 0);
draggingBlock.SetWorldPosition(newWorldPos);
```

Во всех случаях логика одна: вычислить новую мировую позицию → вызвать SetWorldPosition() → готово!

### После исправления:
- Система позиционирования будет работать корректно для любой иерархии
- Можно безопасно вложить ProgramArea в другие контейнеры
- Система готова к параметризации (цикл с параметром "количество итераций" в #12)

## Implementation Priority
1. **Первым:** Шаг 4 (SetWorldPosition) - создать публичный метод в BlockUIBase
   - Это основа, на которой строятся остальные исправления
   - Инкапсулирует всю логику конвертации координат
   - Используется из трех мест

2. **Критично:** Шаг 1 (AlignToInputConnection) - рефакторинг для использования SetWorldPosition
3. **Критично:** Шаг 2 (ApplySnap) - рефакторинг для использования SetWorldPosition
4. **Критично:** Шаг 3 (ApplySnapToInput) - рефакторинг для использования SetWorldPosition

5. **Валидация:** Шаг 5 (SetParent) - исправить параметры в SnapManager.cs
   - Изменить `true` на `false` в строках 303 и 365

6. **Тестирование:** Шаг 6-7 (тесты и валидация)
   - Комплексное тестирование всех сценариев
   - Debug логирование

## Related Tasks
- #10b (Снап в середину) - использует AlignToInputConnection()
- #11 (Блок цикла) - вложенные контейнеры требуют правильной работы локальных координат
- #22 (Drag & Drop из Loop) - использует все три метода позиционирования

## Verification Checklist
После исправления:
- [ ] Бросить блок в пустую ProgramArea → позиция правильная
- [ ] Бросить блок в ProgramArea между двумя блоками → выравнивается корректно
- [ ] Бросить блок в Loop контейнер → позиция правильная относительно Loop bounds
- [ ] Перетащить блок из Loop контейнера в главный ProgramArea → координаты конвертируются правильно
- [ ] Выполнить программу → порядок блоков правильный (коннекции не нарушены)
- [ ] Протестировать с несколькими вложенными Loop блоками → все координаты правильные
