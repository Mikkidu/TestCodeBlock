# Анализ багов Этапа 3: Snap Detection

## Проблемы которые были выявлены

### 1. Неправильное вычисление мировых координат

**Было:**
```csharp
public Vector2 GetWorldPosition()
{
    return visualElement.position;  // ❌ Просто берет position
}
```

**Проблема:**
- `RectTransform.position` - это позиция в мировом пространстве (3D)
- Но для UI элементов нужны координаты в Screen Space (2D экрана)
- Точка входа может быть в разных RectTransform иерархиях
- Координаты не приводятся к единой системе

**Исправлено:**
```csharp
public Vector2 GetWorldPosition()
{
    Vector3[] corners = new Vector3[4];
    visualElement.GetWorldCorners(corners);  // ✅ Правильные мировые координаты
    return (Vector2)((corners[0] + corners[2]) / 2f);  // Центр элемента
}
```

---

### 2. Неправильное позиционирование нового блока при drop

**Было:**
```csharp
BlockUI newBlock = blockFactory.CreateBlock(commandType, transform);
// Блок создается с parent = ProgramArea
// Но координаты не устанавливаются → встает в центр (0, 0)
```

**Проблема:**
- Новый блок создается с position (0, 0) в родителе
- Не учитывается где пользователь отпустил мышь
- Блок появляется в центре вместо точки drop

**Исправлено:**
```csharp
BlockUI newBlock = blockFactory.CreateBlock(commandType, transform);

RectTransform newBlockRect = newBlock.GetComponent<RectTransform>();
RectTransform programAreaRect = GetComponent<RectTransform>();

// Конвертируем screen position в local position relative to ProgramArea
RectTransformUtility.ScreenPointToLocalPointInRectangle(
    programAreaRect,
    eventData.position,        // ✅ Позиция drop в screen coords
    eventData.pressEventCamera,
    out Vector2 localPoint);

newBlockRect.anchoredPosition = localPoint;  // ✅ Устанавливаем правильную позицию
```

---

### 3. Частые поиски ProgramArea в OnDrag

**Было:**
```csharp
public void OnDrag(PointerEventData eventData)
{
    // ... движение блока ...

    ProgramArea programArea = GetComponentInParent<ProgramArea>();  // ❌ Каждый frame!
    if (programArea != null)
    {
        // поиск ближайшего выхода
    }
}
```

**Проблема:**
- `GetComponentInParent<>()` - дорогая операция (обходит иерархию)
- Вызывается КАЖДЫЙ FRAME при перемещении
- Неэффективно для производительности

**Исправлено:**
```csharp
private ProgramArea programArea;  // ✅ Кэш

private void Awake()
{
    programArea ??= GetComponentInParent<ProgramArea>();  // Один раз при инициализации
}

public void OnDrag(PointerEventData eventData)
{
    // ... движение блока ...

    if (programArea != null)  // ✅ Используем кэшированное значение
    {
        // поиск ближайшего выхода
    }
}
```

---

### 4. Отсутствие возврата блока в палитру

**Было:**
```csharp
if (newBlock != null)
{
    AddBlockToProgram(newBlock);
    // droppedBlock.ReturnToOriginalPosition();  // ❌ Закомментировано
}
```

**Проблема:**
- Блок из палитры не возвращается в исходную позицию
- Блок остается в rootCanvas где был начат drag
- Пользователь не может снова использовать этот блок из палитры

**Исправлено:**
```csharp
if (newBlock != null)
{
    AddBlockToProgram(newBlock);
    droppedBlock.ReturnToOriginalPosition();  // ✅ Возвращаем в палитру
}
```

---

## Итоги: Правильный подход

### Координаты в Unity UI
```
Screen Space:      (0, 0) - верхний левый угол экрана
                   (Screen.width, Screen.height) - нижний правый

RectTransform coords:
- position: мировые координаты (3D world space)
- anchoredPosition: локальные координаты в parent
- GetWorldCorners(): массив углов в мировом пространстве

Конвертация:
Screen → Local: RectTransformUtility.ScreenPointToLocalPointInRectangle()
Local → World: visualElement.GetWorldCorners()
```

### Best Practices при работе с Drag-Drop
1. **Кэшировать поиски** - не вызывать GetComponent* каждый frame
2. **Использовать ScreenPointToLocalPointInRectangle** при drop
3. **Всегда приводить координаты к одной системе** перед сравнением расстояний
4. **Возвращать исходное состояние** (для переиспользуемых элементов)

---

## Тесты которые следует провести

- ✓ Блок из палитры позиционируется в точку drop
- ✓ Блок из палитры возвращается в палитру после drop
- ✓ При drag показывается расстояние до ближайшего выхода
- ✓ Нет лагов при перемещении (оптимизированы поиски)
