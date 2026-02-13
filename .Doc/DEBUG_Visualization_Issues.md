# Отладка визуализации магнитных линий

**Дата:** 2026-01-14
**Проблема:** Линия не видна в Game View, хотя snap работает правильно

## Обновлённый визуализатор (v2)

Переделал `SnapLineVisualizer.cs` для лучшей отладки:

### Что добавлено:

1. **Debug.DrawLine с крестиками**
   - Рисует крест (+) в позиции koннектора перетаскиваемого блока
   - Рисует крест (+) в позиции целевого коннектора
   - Рисует линию между ними

2. **Гизмо визуализация (Scene View)**
   - Рисует кружки вокруг каждого коннектора
   - Помогает видеть в Scene View где происходит снап

3. **Подробное логирование**
   - `[VISUALIZER] SNAP ACTIVE` - когда snap включен
   - `[VISUALIZER] INPUT→OUTPUT: from X to Y` - показывает координаты
   - `[VISUALIZER] SNAP CLEARED` - когда snap выключен

## Инструкция для отладки

### Step 1: Откройте Console и Game View рядом

```
Window → General → Console (или Ctrl+Shift+C)
Window → General → Game (или Play и смотрите вниз)
```

Расположите Console и Game View так чтобы видеть оба окна:
- Слева: Game View (полноэкранный)
- Снизу: Console (логи)

### Step 2: Включите Gizmos в Game View

В Game View найдите кнопку **Gizmos** (верхний правый угол):
```
┌─────────────────────────────────────┐
│ Game View [Gizmos ▼]                │  ← Нажмите Gizmos
├─────────────────────────────────────┤
│                                     │
│  [Блоки программы]                  │
│                                     │
└─────────────────────────────────────┘
```

Убедитесь что Gizmos **включены** (галочка должна быть)

### Step 3: Нажмите Play

1. Play в сцене
2. Создайте цепь: [Move] → [TurnLeft]
3. Возьмите новый блок [TurnLeft] за INPUT
4. Поднесите к OUTPUT последнего блока

### Step 4: Смотрите на Console и Game View

**В Console должны появиться логи:**
```
[SNAP READY INPUT→OUTPUT] PaletteBlock_TurnLeft → Block_MoveBackward_6 | Distance: XX.XXpx
[VISUALIZER] SNAP ACTIVE: PaletteBlock_TurnLeft → Block_MoveBackward_6
[VISUALIZER] INPUT→OUTPUT: from (XXX, YYY) to (AAA, BBB)
```

**В Game View должны появиться:**
- ✓ Крестики (+) в позициях коннекторов
- ✓ Линия между крестиками

### Step 5: Если ничего не видно

#### Вариант A: Проверить что SnapLineVisualizer вообще вызывается

1. Откройте `BlockUI.cs` строка 78
2. Добавьте лог перед вызовом:
```csharp
Debug.Log($"[DEBUG] UpdateSnapVisuals called, programArea: {programArea}, snap: {snapInfo.canSnap}");
```

3. Проверьте консоль - должны быть логи при перетаскивании

#### Вариант B: Проверить координаты коннекторов

1. Откройте `BlockConnector.cs` (найти файл)
2. Найти метод `GetWorldPosition()`
3. Проверить что он возвращает правильные координаты

Добавьте временный лог:
```csharp
public Vector3 GetWorldPosition()
{
    Vector3 pos = visualElement.transform.position;
    Debug.Log($"[CONNECTOR] GetWorldPosition: {pos}");
    return pos;
}
```

#### Вариант C: Debug.DrawLine может быть невидим из-за масштаба

Попробуйте увеличить крестики в `DrawDebugSpheres()`:
```csharp
// Измените 5f на 20f для больших крестиков
Debug.DrawLine(snapFromPos - Vector3.right * 20f, snapFromPos + Vector3.right * 20f, snapLineColor, 0f);
Debug.DrawLine(snapFromPos - Vector3.up * 20f, snapFromPos + Vector3.up * 20f, snapLineColor, 0f);
```

## Проверка step-by-step

### Тест 1: Линия в Scene View

1. Play в сцене
2. **Переключитесь на Scene View** (не Game View!)
3. Перетащите блок и смотрите на Scene View
4. **Должны быть видны:**
   - ✓ Кружки вокруг коннекторов (Gizmos)
   - ✓ Линия между ними

### Тест 2: Логи в Console

1. Play в сцене
2. Откройте Console (Ctrl+Shift+C)
3. Перетащите блок
4. **Должны быть видны в Console:**
```
[SNAP READY INPUT→OUTPUT] ...
[VISUALIZER] SNAP ACTIVE: ...
[VISUALIZER] INPUT→OUTPUT: from ... to ...
```

Если логов нет → проблема в интеграции с BlockUI

### Тест 3: Крестики в Game View

1. Убедитесь что Gizmos включены
2. Play в сцене
3. Перетащите блок
4. **Должны быть видны:**
   - ✓ Кресты в позициях коннекторов
   - ✓ Цветная линия между ними

## Координатные системы и Z-позиция

### Важно: Z-позиция!

Debug.DrawLine работает в мировых координатах (World Space), но UI элементы в Canvas Space. Может быть проблема с Z-координатой:

```csharp
// Убедитесь что Z соответствует Canvas Z
Debug.DrawLine(
    new Vector3(snapFromPos.x, snapFromPos.y, 0f),  // Явно Z=0
    new Vector3(snapToPos.x, snapToPos.y, 0f),
    snapLineColor,
    0f
);
```

## Файлы для проверки

1. **BlockUI.cs** строка 76-90
   - Вызов `lineVisualizer.SetSnapInfo()`
   - Может быть `lineVisualizer` == null?

2. **SnapLineVisualizer.cs**
   - Проверить что Update() вызывается
   - Проверить что hasActiveSnap = true

3. **BlockConnector.cs**
   - Метод `GetWorldPosition()`
   - Возвращает ли правильные координаты?

## Логирование для добавления

Добавьте эти логи в **BlockUI.cs** в методе `UpdateSnapVisuals`:

```csharp
Debug.Log($"[DEBUG VISUALS] snap.canSnap={snapInfo.canSnap}, programArea={programArea != null}");

if (programArea != null)
{
    SnapLineVisualizer lineVisualizer = programArea.GetComponent<SnapLineVisualizer>();
    Debug.Log($"[DEBUG VISUALS] lineVisualizer={lineVisualizer != null}");

    if (lineVisualizer != null && snapInfo.canSnap)
    {
        Debug.Log($"[DEBUG VISUALS] Calling SetSnapInfo()");
        lineVisualizer.SetSnapInfo(this, snapInfo);
    }
}
```

## Ожидаемый вывод Console при успехе

```
[SNAP READY INPUT→OUTPUT] PaletteBlock_TurnLeft → Block_MoveBackward_6 | Distance: 23.49px
[DEBUG VISUALS] snap.canSnap=True, programArea=True
[DEBUG VISUALS] lineVisualizer=True
[DEBUG VISUALS] Calling SetSnapInfo()
[VISUALIZER] SNAP ACTIVE: PaletteBlock_TurnLeft → Block_MoveBackward_6
[VISUALIZER] INPUT→OUTPUT: from (453, 234) to (623, 402)
[SNAP READY INPUT→OUTPUT] PaletteBlock_TurnLeft → Block_MoveBackward_6 | Distance: 24.47px
...
```

## Что дальше?

Попробуйте эти шаги:
1. Проверьте логи в Console
2. Переключитесь на Scene View
3. Смотрите видны ли Gizmos (кружки и линия)
4. Если видны в Scene View но не в Game View → проблема с масштабом или Z-позицией

---

**Нужна помощь?** Добавьте эти логи и поделитесь выводом Console при перетаскивании блока.
