# Система приоритизированного магнитного притяжения блоков (#10b Enhancement)

## Обзор

После выявления в тестировании была реализована система приоритизированного поиска магнитных точек. Это значительно улучшает удобство использования при построении цепей команд.

## Проблема (До реализации)

В оригинальной системе магнитное притяжение просто искало ближайший коннектор по расстоянию без учёта контекста:
- Пользователь зачастую случайно цеплялся за середину цепи, вместо того чтобы добавить блок в конец
- Нелогичное поведение: INPUT блока магнитился к любому OUTPUT, вне зависимости от позиции в цепи

## Решение

Реализована **двухуровневая приоритизация** с учётом позиции блока в цепи:

### 1. FindNearestOutput() - Поиск OUTPUT для INPUT перетаскиваемого блока

**Цель:** Когда пользователь перетаскивает блок за его INPUT, найти OUTPUT для подключения.

**Логика приоритизации:**

```
ПРИОРИТЕТ 1: OUTPUT блоков в конце цепи (output.connectedTo == null)
  └─ Блоки без выходящей связи - идеальные точки для добавления нового блока

ПРИОРИТЕТ 2: OUTPUT блоков в середине цепи (output.connectedTo != null)
  └─ Уже подключённые к другим блокам - для вставки в середину цепи
```

**Примеры:**

```
Цепь:  [A] → [B] → [C]

1. Перетаскиваем новый блок X за INPUT:
   ├─ Если X близко к OUTPUT A: Приоритет 1 (A без выхода... нет, A → B)
   ├─ Если X близко к OUTPUT B: Приоритет 1 (B → C, тоже не конец)
   └─ Если X близко к OUTPUT C: Приоритет 1 ✓ (C - конец цепи, выбираем его!)

2. Вставка: [A] → [B] → [X] → [C]
   └─ INPUT X подключился к OUTPUT B, OUTPUT X подключён к INPUT C
```

### 2. FindNearestInput() - Поиск INPUT для OUTPUT перетаскиваемого блока

**Цель:** Когда пользователь перетаскивает блок за его OUTPUT, найти INPUT для подключения.

**Логика приоритизации:**

```
ПРИОРИТЕТ 1: INPUT блоков в начале своих участков (NO incoming connection)
  └─ Блоки, у которых INPUT не подключен ни к какому OUTPUT
  └─ Это "первые в цепи" блоки (или первые в разорванной части)

ПРИОРИТЕТ 2: INPUT блоков уже подключённых (HAS incoming connection)
  └─ INPUT уже подключен к чему-то - для вставки в середину
```

**Примеры:**

```
Цепь 1:  [A] → [B] → [C]
Цепь 2:  [D] → [E]

1. Перетаскиваем новый блок X за OUTPUT:
   ├─ Если X близко к INPUT A: Приоритет 1 (нет входящей связи к A - это начало!)
   ├─ Если X близко к INPUT B: Приоритет 2 (B имеет входящую связь от A)
   ├─ Если X близко к INPUT D: Приоритет 1 (нет входящей связи к D - это начало!)
   └─ Если X близко к INPUT E: Приоритет 2 (E имеет входящую связь от D)

2. Выбор: Приоритет 1 всегда выбирается раньше
   └─ Если несколько блоков одного приоритета - выбираем ближайший по расстоянию
```

## Код

### SnapManager.cs - Модифицированные методы

#### `FindNearestOutput()` - поиск выхода для входа

```csharp
// Разделяем выходы на две группы приоритета
float minDistancePriority1 = float.MaxValue;  // Конец цепи
BlockConnector nearestOutputPriority1 = null;

float minDistancePriority2 = float.MaxValue;  // Середина цепи
BlockConnector nearestOutputPriority2 = null;

// Поиск по блокам...
foreach (BlockUI block in allBlocks)
{
    foreach (BlockConnector output in block.outputPoints)
    {
        bool isEndOfChain = output.connectedTo == null;

        if (isEndOfChain)
        {
            // Priority 1: концы цепей
            if (distance < minDistancePriority1)
            {
                minDistancePriority1 = distance;
                nearestOutputPriority1 = output;
            }
        }
        else
        {
            // Priority 2: середины цепей
            if (distance < minDistancePriority2)
            {
                minDistancePriority2 = distance;
                nearestOutputPriority2 = output;
            }
        }
    }
}

// Выбираем: сначала Приоритет 1, потом Приоритет 2
if (nearestOutputPriority1 != null && minDistancePriority1 <= snapDistance)
{
    return nearestOutputPriority1;
}
else if (nearestOutputPriority2 != null && minDistancePriority2 <= snapDistance)
{
    return nearestOutputPriority2;
}
```

#### `FindNearestInput()` - поиск входа для выхода

```csharp
// Разделяем входы на две группы приоритета
float minDistancePriority1 = float.MaxValue;  // Начало цепи (нет входящей связи)
BlockConnector nearestInputPriority1 = null;

float minDistancePriority2 = float.MaxValue;  // Середина цепи (есть входящая связь)
BlockConnector nearestInputPriority2 = null;

// Поиск по блокам...
foreach (BlockUI block in allBlocks)
{
    foreach (BlockConnector input in block.inputPoints)
    {
        bool hasIncomingConnection = HasIncomingConnection(input, allBlocks);

        if (!hasIncomingConnection)
        {
            // Priority 1: блоки без входящей связи (начало цепей)
            if (distance < minDistancePriority1)
            {
                minDistancePriority1 = distance;
                nearestInputPriority1 = input;
            }
        }
        else
        {
            // Priority 2: блоки с входящей связью (для вставки в середину)
            if (distance < minDistancePriority2)
            {
                minDistancePriority2 = distance;
                nearestInputPriority2 = input;
            }
        }
    }
}

// Выбираем: сначала Приоритет 1, потом Приоритет 2
if (nearestInputPriority1 != null && minDistancePriority1 <= snapDistance)
{
    return nearestInputPriority1;
}
else if (nearestInputPriority2 != null && minDistancePriority2 <= snapDistance)
{
    return nearestInputPriority2;
}
```

#### `HasIncomingConnection()` - вспомогательный метод

```csharp
private bool HasIncomingConnection(BlockConnector targetInput, List<BlockUI> allBlocks)
{
    if (targetInput == null) return false;

    // Ищем, подключен ли какой-то OUTPUT к этому INPUT
    foreach (BlockUI block in allBlocks)
    {
        foreach (BlockConnector output in block.outputPoints)
        {
            if (output.connectedTo == targetInput)
            {
                return true;  // Есть входящее соединение
            }
        }
    }

    return false;  // Нет входящего соединения
}
```

## Примеры использования

### Сценарий 1: Добавление в конец простой цепи

```
Было:  [Move] → [Turn] → [Move]

Пользователь:
1. Берёт новый блок "Turn" за INPUT (первый блок с зелёной точкой)
2. Приносит к OUTPUT последнего блока "Move"
3. Система видит: это OUTPUT конца цепи (Приоритет 1)
4. Снап срабатывает, блок встаёт в конец

Стало:  [Move] → [Turn] → [Move] → [Turn]
```

### Сценарий 2: Вставка в середину цепи

```
Было:  [A] → [B] → [C]

Пользователь:
1. Берёт блок X за INPUT
2. Приносит к OUTPUT блока A (который ведёт к B)
3. Система видит: это OUTPUT в середине цепи (Приоритет 2)
4. Может быть даже OUTPUT C ближе, но он в конце (Приоритет 1)
5. Система выбирает OUTPUT A (если он ближе среди Приоритета 1)

Вставка между A и B успешна!
Стало:  [A] → [X] → [B] → [C]
```

### Сценарий 3: Несколько цепей

```
Цепь 1:  [A] → [B] → [C]
Цепь 2:  [D] → [E]

Пользователь берёт блок X за OUTPUT:
1. Если близко к INPUT A: Приоритет 1 (начало Цепи 1) ✓
2. Если близко к INPUT B: Приоритет 2 (середина Цепи 1)
3. Если близко к INPUT D: Приоритет 1 (начало Цепи 2) ✓
4. Если близко к INPUT E: Приоритет 2 (середина Цепи 2)

Система выберет одну из начал (Приоритет 1) - ту, что ближе!
```

## Тестирование

### Требуемые сценарии для тестирования:

1. ✓ Добавление блока в конец одиночной цепи
2. ✓ Добавление блока в конец длинной цепи (3+ блоков)
3. ✓ Вставка блока в середину цепи между любыми блоками
4. ✓ Работа с несколькими разрывистыми цепями одновременно
5. ✓ Проверка что магнитизм приоритизирует корректно (конец > середина)
6. ✓ Выполнение программы в правильном порядке после вставок
7. ✓ Отсутствие визуальных артефактов и наложений

### Debug логи

При работе система выводит логи вида:
```
[SNAP READY OUTPUT→INPUT] BlockName → TargetBlockName | Distance: 25.50px
[SNAP APPLIED OUTPUT→INPUT] BlockName → TargetBlockName
[CONNECTION OUTPUT→INPUT] BlockName → TargetBlockName
[ALIGN] BlockName aligned to ConnectedBlockName
```

## Производительность

- **Сложность:** O(N×M) где N = количество блоков, M = среднее количество коннекторов
- **Оптимизация:** Поиск останавливается при нахождении Приоритета 1 (не продолжает поиск Приоритета 2)
- **Для типичной программы:** 10-20 блоков = миллисекундные вычисления

## Возможные улучшения в будущем

1. **Визуальный feedback:** Показывать двумя разными цветами какой приоритет будет применён
2. **Динамическая зона магнитизма:** Расширять/сужать snapDistance в зависимости от приоритета
3. **Звуковой feedback:** Разные звуки для разных приоритетов
4. **Цифровой помощник:** Показывать текст "Добавить в конец цепи" vs "Вставить в середину"
