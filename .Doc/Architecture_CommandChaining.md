# Архитектура цепочки выполнения команд

## Проблема текущего подхода (Stage 6 v1)

### Подход "по Y-позиции" (текущий план):
```csharp
// Сортируем блоки сверху вниз и связываем
var sortedBlocks = blocksInProgram
    .OrderByDescending(b => b.GetComponent<RectTransform>().anchoredPosition.y)
    .ToList();

for (int i = 0; i < sortedBlocks.Count - 1; i++)
{
    sortedBlocks[i].Command.Next = sortedBlocks[i + 1].Command;
}
```

### ❌ Проблемы этого подхода:

1. **Не поддерживает условные ветвления**
   ```
   Блок: IF condition
   - Если true  → идем к блоку A
   - Если false → идем к блоку B

   Но `Command.Next` - одна ссылка!
   Y-позиция не решает, какую ветку выбрать
   ```

2. **Не работает с параллельными потоками** (если будут)
   ```
   Блок A может иметь 2+ выходов
   Каждый выход → разные следующие блоки
   ```

3. **Нарушает логику архитектуры**
   ```
   Визуальный порядок ≠ Логический порядок выполнения
   Если пользователь передвинет блок - изменится исполнение!
   ```

4. **Усложняет манипуляцию группами**
   ```
   Хочу выбрать блок A и все к нему подключенные
   По Y-позиции я не знаю, какие блоки "за ним" по логике
   ```

5. **Не масштабируется**
   ```
   Параметры между блоками?
   Переиспользование подпрограмм?
   Условные переходы на основе параметров?

   Y-позиция ничего не скажет!
   ```

---

## Правильный подход: Соединения блоков

### Архитектура "по соединениям":
```
BlockConnector (Input/Output точка)
    ├── Может быть подключена к другому BlockConnector
    └── Хранит ссылку на подключение

BlockUI (блок)
    ├── inputPoints: List<BlockConnector>
    ├── outputPoints: List<BlockConnector>
    │   └── каждый Output может быть связан с Input другого блока
    └── При выполнении: смотрим на outputPoints[0].connectedInput.parentBlock

ICommand
    ├── Больше НЕ нужно Command.Next
    ├── Или Next = динамическое из BlockUI
    └── Выполнение = идем по соединениям
```

### ✅ Пример визуально:
```
┌──────────┐      соединение      ┌──────────┐
│ Block A  │ ──────(snap)────→ │ Block B  │
│          │  (Output→Input)    │          │
│ Output ●─────────────────────● Input     │
└──────────┘                    └──────────┘

При выполнении A:
A.outputPoints[0].connectedInput.parentBlock = Block B
```

---

## Вариант 1: Минимальный (Рекомендуется)

### Описание:
Добавить поле в BlockConnector: ссылка на подключенный BlockConnector другого блока.
При snap создавать соединение.
При выполнении переходить по соединению.

### Структура:
```csharp
public class BlockConnector
{
    public enum PointType { Input, Output }

    public PointType pointType;
    public RectTransform visualElement;
    public BlockUI parentBlock;  // НОВОЕ

    // НОВОЕ: соединение с другим блоком
    public BlockConnector connectedTo;  // Output → связан с Input другого
}

// В BlockUI:
public BlockUI GetNextBlock()
{
    if (outputPoints.Count > 0 && outputPoints[0].connectedTo != null)
    {
        return outputPoints[0].connectedTo.parentBlock;
    }
    return null;
}
```

### Как работает snap:
```csharp
public void ApplySnap(BlockUI draggingBlock,
                      BlockConnector inputPoint,
                      BlockConnector targetOutput)
{
    // Выравнять позицию (как сейчас)
    // + создать соединение

    targetOutput.connectedTo = inputPoint;  // Output → Input
    inputPoint.connectedTo = targetOutput;  // Input → Output (для связи)
}
```

### Как работает выполнение:
```csharp
// В CommandExecutor или при выполнении
BlockUI currentBlock = startBlock;
while (currentBlock != null)
{
    ICommand cmd = currentBlock.Command;
    yield return ExecuteCommand(cmd);

    // Переходим по соединению, не по Y-позиции
    currentBlock = currentBlock.GetNextBlock();
}
```

### ✅ Плюсы:
- Простая реализация (добавить 1-2 поля)
- Максимально совместима с текущей архитектурой
- Сразу готово для условных ветвлений (добавим outputPoints[1])
- Логика выполнения = логика соединений
- Готово для группировки блоков (идем по соединениям)

### ❌ Минусы:
- Command.Next больше не используется (можно оставить для совместимости)
- Нужно обновить ApplySnap
- Нужно обновить разрыв соединения (если реализовать)

---

## Вариант 2: Явные соединения (Connection объекты)

### Описание:
Создать класс Connection который хранит связь между Output и Input.
Более OOP подход.

### Структура:
```csharp
public class BlockConnection
{
    public BlockConnector sourceOutput;   // Output блока A
    public BlockConnector targetInput;    // Input блока B
    public bool isActive = true;

    public BlockUI GetTargetBlock()
    {
        return targetInput.parentBlock;
    }
}

// В BlockUI:
public List<BlockConnection> outgoingConnections = new List<BlockConnection>();
public BlockConnection incomingConnection;

public BlockUI GetNextBlock()
{
    if (outgoingConnections.Count > 0 && outgoingConnections[0].isActive)
    {
        return outgoingConnections[0].GetTargetBlock();
    }
    return null;
}
```

### ✅ Плюсы:
- Явная семантика (Connection = видимое соединение)
- Легко реализовать если/затем более сложные сценарии
- Можно отключать соединения (isActive)
- Готово для валидации типов параметров
- Легче отладить (видно все соединения)

### ❌ Минусы:
- Больше кода
- Дополнительный слой абстракции
- Нужно синхронизировать Connection и BlockConnector
- Более сложно для простых случаев

---

## Вариант 3: Расширенная архитектура (Future-proof)

### Описание:
Connection как полноценный объект с типами, параметрами, условиями.

### Структура:
```csharp
public class BlockConnection
{
    public BlockConnector sourceOutput;
    public BlockConnector targetInput;

    // Параметры соединения
    public enum ConditionType { Always, OnTrue, OnFalse, OnValue }
    public ConditionType condition = ConditionType.Always;
    public string expectedValue; // для фильтрации по значению

    // Передача параметров
    public Dictionary<string, string> parameterMapping;
    // output.paramName → input.paramName

    public bool CanExecute(ICommand sourceCommand)
    {
        if (condition == ConditionType.Always) return true;
        if (condition == ConditionType.OnTrue) return sourceCommand.LastResult;
        // ... и т.д.
    }
}
```

### ✅ Плюсы:
- Поддерживает все будущие сценарии
- Передача параметров встроена
- Условные переходы
- Параллельные ветки

### ❌ Минусы:
- Много кода для MVP
- Усложняет Stage 6
- Не нужно прямо сейчас
- Может быть рефакторено позже

---

## Сравнительная таблица

| Аспект | Вариант 1 | Вариант 2 | Вариант 3 |
|--------|-----------|-----------|-----------|
| **Сложность реализации** | ⭐ Простая | ⭐⭐ Средняя | ⭐⭐⭐ Сложная |
| **Время на Stage 6** | 30 мин | 1-2 часа | 3+ часа |
| **Готовность к If/Else** | ✓ Легко | ✓ Легко | ✓ Готово |
| **Параллельные ветки** | ✓ Легко | ✓ Легко | ✓ Встроено |
| **Параметры между блоками** | ❌ Нет | Δ Можно добавить | ✓ Готово |
| **Группировка блоков** | ✓ Работает | ✓ Лучше | ✓ Идеально |
| **Code bloat** | Минимум | Среднее | Много |
| **Масштабируемость** | Хорошо | Отлично | Идеально |
| **Рекомендуется сейчас?** | ✅ ДА | Может быть | Нет, позже |

---

## Рекомендуемый путь

### Phase 1 (Stage 6 - СЕЙЧАС): Вариант 1
- Добавить `connectedTo` в BlockConnector
- Обновить ApplySnap для создания соединения
- Обновить логику выполнения (GetNextBlock)
- Результат: соединение-ориентированное выполнение ✓

### Phase 2 (Stage 7+): Вариант 2
- Создать BlockConnection класс
- Мигрировать с Вариант 1
- Добавить support для множественных выходов
- Добавить If/Else поддержку

### Phase 3 (Future): Вариант 3
- Полная система с параметрами
- Условные переходы
- Параллельные потоки

---

## Конкретные изменения для Вариант 1

### 1. BlockConnector.cs:
```csharp
public class BlockConnector
{
    public enum PointType { Input, Output }

    public PointType pointType;
    public RectTransform visualElement;
    public BlockUI parentBlock;  // ← НОВОЕ

    public BlockConnector connectedTo;  // ← НОВОЕ: соединение

    public BlockConnector(PointType type, RectTransform visual)
    {
        pointType = type;
        visualElement = visual;
    }
}
```

### 2. SnapManager.ApplySnap():
```csharp
public void ApplySnap(BlockUI draggingBlock,
                      BlockConnector inputPoint,
                      BlockConnector targetOutput)
{
    // ... выравнивание позиции (как сейчас) ...

    // НОВОЕ: создать соединение
    targetOutput.connectedTo = inputPoint;
    Debug.Log($"Соединение создано: {targetOutput.parentBlock.name} → {draggingBlock.name}");
}
```

### 3. BlockUI.cs:
```csharp
public BlockUI GetNextBlock()
{
    if (outputPoints.Count > 0 && outputPoints[0].connectedTo != null)
    {
        BlockConnector connectedInput = outputPoints[0].connectedTo;
        return connectedInput.parentBlock;
    }
    return null;
}

// Опционально: разрыв соединения
public void DisconnectOutput(int outputIndex)
{
    if (outputIndex < outputPoints.Count && outputPoints[outputIndex].connectedTo != null)
    {
        outputPoints[outputIndex].connectedTo = null;
    }
}
```

### 4. BlockUI.InitializeConnectors():
```csharp
public void InitializeConnectors()
{
    inputPoints.Clear();
    outputPoints.Clear();

    if (inputPointVisual != null)
    {
        Image inputImage = inputPointVisual.GetComponent<Image>();
        if (inputImage != null)
        {
            inputImage.color = new Color(0f, 1f, 0f, 1f); // Green
        }

        BlockConnector inputConnector = new BlockConnector(
            BlockConnector.PointType.Input, inputPointVisual);
        inputConnector.parentBlock = this;  // ← НОВОЕ
        inputPoints.Add(inputConnector);
    }

    foreach (RectTransform outputVisual in outputPointsVisuals)
    {
        if (outputVisual != null)
        {
            Image outputImage = outputVisual.GetComponent<Image>();
            if (outputImage != null)
            {
                outputImage.color = new Color(1f, 0f, 0f, 1f); // Red
            }

            BlockConnector outputConnector = new BlockConnector(
                BlockConnector.PointType.Output, outputVisual);
            outputConnector.parentBlock = this;  // ← НОВОЕ
            outputPoints.Add(outputConnector);
        }
    }
}
```

### 5. CommandExecutor (или где выполняются команды):
```csharp
// ДО (по Y-позиции):
// for (int i = 0; i < sortedBlocks.Count - 1; i++)
// {
//     sortedBlocks[i].Command.Next = sortedBlocks[i + 1].Command;
// }

// ПОСЛЕ (по соединениям):
BlockUI currentBlock = startBlock;
while (currentBlock != null)
{
    ICommand cmd = currentBlock.Command;
    yield return ExecuteCommand(cmd);

    // Идем по физическому соединению
    currentBlock = currentBlock.GetNextBlock();
}
```

---

## Что УДАЛЯЕМ из Stage 6 v1

❌ `UpdateCommandChainByPosition()` - больше не нужна
❌ Y-сортировка блоков - больше не нужна
❌ `Command.Next` установка через цикл - не используется

✅ Вместо этого используем соединения BlockConnector

---

## Итоговый план Stage 6 (Вариант 1)

**Время:** 45-60 минут (вместо 30)

1. **Добавить parentBlock в BlockConnector** (5 мин)
2. **Добавить connectedTo в BlockConnector** (5 мин)
3. **Обновить InitializeConnectors** (10 мин)
4. **Обновить ApplySnap** (10 мин)
5. **Добавить GetNextBlock в BlockUI** (5 мин)
6. **Обновить CommandExecutor/выполнение** (15 мин)
7. **Тестирование и отладка** (10-15 мин)
8. **Обновить документацию** (5 мин)

---

## Выводы

✅ **Вариант 1 (соединения)** - правильный подход для Stage 6
- Простая реализация
- Соответствует исходной архитектуре (связанный список)
- Готовит почву для If/Else и параллельных веток
- Поддерживает группировку блоков

❌ **Вариант "Y-позиция"** - тупик, может привести к переработкам позже

✅ Рекомендуется реализовать именно Вариант 1 как Stage 6
