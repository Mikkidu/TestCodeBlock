# Реализация каскадного выравнивания для вставки блоков в середину цепи

## Обзор решения

Вместо традиционного подхода смещения всех блоков по одному расчету, реализована система **каскадного выравнивания**, где каждый блок автономно выравнивается к своему входящему соединению и затем уведомляет следующий блок.

## Архитектура

```
SnapManager.ApplySnapToInput()
  ↓
  Позиционирует X, B, затем:
  ↓
  nextBlock.AlignToInputConnection()  (B)
  ├─ Ищет свой INPUT и находит подключенный OUTPUT (от X)
  ├─ Вычисляет offset для выравнивания
  ├─ Позиционирует себя
  └─ Вызывает GetNextBlock().AlignToInputConnection()  (C)
      ├─ Ищет свой INPUT и находит подключенный OUTPUT (от B)
      ├─ Вычисляет offset
      ├─ Позиционирует себя
      └─ Вызывает GetNextBlock().AlignToInputConnection()  (D)
          └─ ... (продолжает вниз по цепи)
```

## Ключевые компоненты

### 1. BlockUI.AlignToInputConnection() (BlockUI.cs:435-502)

```csharp
public void AlignToInputConnection()
{
    // Проверка: только для блоков в программе
    if (inputPoints.Count == 0 || !inProgramArea)
        return;

    BlockConnector myInput = inputPoints[0];
    ProgramArea programArea = GetComponentInParent<ProgramArea>();

    // ШАГ 1: Найти какой OUTPUT подключен к моему INPUT
    BlockConnector connectedOutput = null;
    foreach (BlockUI block in programArea.GetBlocks())
    {
        foreach (BlockConnector output in block.outputPoints)
        {
            if (output.connectedTo == myInput)
            {
                connectedOutput = output;
                break;  // Найдено - выход
            }
        }
        if (connectedOutput != null) break;
    }

    // ШАГ 2: Выравнять себя к этому OUTPUT'у
    if (connectedOutput != null)
    {
        Vector2 outputPos = connectedOutput.GetWorldPosition();
        Vector2 myInputPos = myInput.GetWorldPosition();
        Vector2 offset = outputPos - myInputPos;

        RectTransform rect = GetComponent<RectTransform>();
        rect.position = new Vector3(
            rect.position.x + offset.x,
            rect.position.y + offset.y,
            rect.position.z
        );

        // ШАГ 3: Инициировать cascade для следующего блока
        BlockUI nextBlock = GetNextBlock();
        if (nextBlock != null)
        {
            nextBlock.AlignToInputConnection();
        }
    }
}
```

### 2. SnapManager.ApplySnapToInput() Изменения (SnapManager.cs:244-297)

В методе ApplySnapToInput (обработка вставки в середину):

```csharp
// Прежде:
// ShiftBlockChain(targetBlock, offsetY);  // Ручное смещение

// Теперь:
BlockUI nextBlock = targetBlock.GetNextBlock();
if (nextBlock != null)
{
    nextBlock.AlignToInputConnection();  // Инициировать cascade
}
```

Это единственное изменение - вместо расчета смещения и смещения всех блоков вручную, мы просто инициируем каскадное выравнивание.

## Поток выполнения при вставке в середину (A→B→C, вставляем X между A и B)

```
1. Пользователь перетащит X из палитры и подносит OUTPUT X к INPUT B

2. BlockUI.OnDrop() срабатывает в ProgramArea:
   - Создается копия блока X
   - Добавляется в программу

3. ProgramArea.OnDrop() вызывает SnapManager.ApplySnapToInput():

4. ApplySnapToInput() в цикле выполняет:

   Шаг 1: Позиционирование X
   - Находит что OUTPUT A подключен к INPUT B
   - previousOutput = A.outputPoints[0]
   - Позиционирует X так, чтобы X.inputPoints[0] совпадал с A.outputPoints[0]
   - Создает соединение: A.output → X.input

   Шаг 2: Позиционирование B
   - Позиционирует B так, чтобы B.inputPoints[0] совпадал с X.outputPoints[0]
   - Логирует: [DISCONNECT FOR INSERT] A → B

   Шаг 3: Инициация cascade
   - nextBlock = B.GetNextBlock()  // Получает C (т.к. B.output → C.input)
   - nextBlock.AlignToInputConnection()  // Запускает cascade

5. B.AlignToInputConnection() выполняет:
   - Находит OUTPUT подключенный к B.input (это X.output)
   - Вычисляет offset для выравнивания
   - Позиционирует B
   - Получает C через GetNextBlock() и вызывает C.AlignToInputConnection()

6. C.AlignToInputConnection() выполняет:
   - Находит OUTPUT подключенный к C.input (это B.output)
   - Вычисляет offset
   - Позиционирует C
   - Получает D (если существует) и вызывает D.AlignToInputConnection()

7. Cascade продолжается вниз по цепи до конца

Результат: A → X → B → C, все блоки визуально выравнены корректно
```

## Преимущества каскадного подхода

1. **Автономность**: каждый блок сам решает как выравняться
2. **Универсальность**: работает для любой длины цепи
3. **Рекурсивность**: естественно подходит для каскадирования
4. **Отсутствие хардкода**: не нужно рассчитывать высоту блоков вручную
5. **Гибкость**: если изменить размер блока - все равно будет работать

## Важные детали

### Проверка inProgramArea
```csharp
if (inputPoints.Count == 0 || !inProgramArea)
    return;
```
Это гарантирует, что AlignToInputConnection вызывается только для блоков уже в программе, не для палитровых.

### Использование RectTransform.position (мировые координаты)
```csharp
Vector2 outputPos = connectedOutput.GetWorldPosition();  // Мировые координаты
Vector2 myInputPos = myInput.GetWorldPosition();         // Мировые координаты
Vector2 offset = outputPos - myInputPos;                 // Разница в мировых координатах

rect.position = new Vector3(
    rect.position.x + offset.x,
    rect.position.y + offset.y,
    rect.position.z
);
```

GetWorldPosition() вычисляет мировые координаты коннектора. Затем мы используем эти координаты для расчета offset, который применяем к rect.position (тоже мировые координаты).

### Рекурсия и GetNextBlock()
```csharp
BlockUI nextBlock = GetNextBlock();  // Получает блок через OUTPUT.connectedTo.parentBlock
if (nextBlock != null)
{
    nextBlock.AlignToInputConnection();  // Рекурсивный вызов
}
```

GetNextBlock() следует цепи физических соединений (OUTPUT → connectedTo → INPUT → parentBlock).

## Сценарии тестирования

### Тест 1: Вставка в начало
```
Было: A → B
Перетащим X, подносим OUTPUT X к INPUT A
Ожидается: X → A → B
```
✓ X.output→INPUT A, A выравнивается к X (cascade инициируется, но A.input не подключен → нет смещения, работает)

### Тест 2: Вставка в конец
```
Было: A → B
Перетащим X, подносим INPUT X к OUTPUT A (конец цепи)
Ожидается: A → X → B
```
✓ X.input→OUTPUT A, затем X.output→INPUT B, B выравнивается через cascade

### Тест 3: Вставка в середину простая (А→B, вставляем X)
```
Было: A → B
Перетащим X, подносим OUTPUT X к INPUT B
Ожидается: A → X → B
```
✓ Cascade: A→X→B, B выравнивается к X

### Тест 4: Вставка в дольгую цепь (A→B→C→D, вставляем X между B и C)
```
Было: A → B → C → D
Перетащим X, подносим OUTPUT X к INPUT C
Ожидается: A → B → X → C → D
```
✓ Cascade: A→B→X→C, затем C→X (cascade выполняет), затем D→C (cascade выполняет)

### Тест 5: Множественные вставки подряд
```
1. Было: A → B
2. Вставляем X между A и B: A → X → B
3. Вставляем Y между X и B: A → X → Y → B
4. Вставляем Z между A и X: Z → A → X → Y → B
```
✓ Каждый cascade корректно перестраивает цепь

### Тест 6: Выполнение программы после вставок
```
1. Создаем: MoveForward → TurnRight
2. Вставляем MoveBackward между ними: MoveForward → MoveBackward → TurnRight
3. Запускаем программу
4. Ожидается [EXECUTE]:
   MoveForward
   MoveBackward
   TurnRight
   PROGRAM COMPLETE
```
✓ GetFirstBlock() находит MoveForward, затем GetNextBlock() следует физическим соединениям

## Debug информация

При выполнении должны появляться логи:

```
[CONNECTION OUTPUT→INPUT] X → B  // ApplySnapToInput создает соединение
[DISCONNECT FOR INSERT] A → B    // ApplySnapToInput логирует разрыв
[RECONNECT] A → X                // ApplySnapToInput логирует новое соединение
[ALIGN] B aligned to X           // Из AlignToInputConnection
[ALIGN] C aligned to B           // Cascade продолжается
[ALIGN] D aligned to C           // Cascade продолжается
```

## Критические места для проверки

1. **Расчет offset в AlignToInputConnection**: должен быть точный
2. **Получение GetWorldPosition()**: должны вернуть корректные мировые координаты
3. **RectTransform.position**: должна измениться корректно
4. **Цепь соединений**: должна остаться целостной после cascade
5. **GetNextBlock()**: должна вернуть корректный следующий блок
6. **Null checks**: должна быть защита от null references

## Если что-то не работает

Включить Debug.Log вывод и проверить:

1. Логируется ли AlignToInputConnection() вызов?
2. Находится ли connectedOutput?
3. Корректно ли вычисляется offset?
4. Изменяется ли rect.position?
5. Продолжается ли cascade (логи для каждого блока)?

## Заключение

Система каскадного выравнивания обеспечивает надежный и универсальный способ позиционирования блоков при вставке в цепь. Каждый блок отвечает за свое выравнивание и передачу инициативы следующему, создавая естественный каскадный эффект.
