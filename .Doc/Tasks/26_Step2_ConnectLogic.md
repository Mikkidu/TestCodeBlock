# Шаг 1.2: Подключение блока к цепи (Connect Logic)

**Этап**: Группа 1 - Базовое управление цепью
**Статус**: 📋 **ПЛАНИРОВАНИЕ**
**Метрика готовности**: Два блока можно подключить вход-к-выходу, цепь отслеживается корректно
**Время**: 3-4 часа
**Зависит от**: Шаг 1.1 (InputPoint готова)

---

## 💡 АРХИТЕКТУРА РЕШЕНИЯ

**Система подключения блоков**:
- **BlockConnector** (уже есть) - einzelный вход или выход блока
- **BlockUI** - имеет inputConnector и outputConnector
- **ProgramAreaManager** - управляет цепью (знает первый блок, может обойти всю цепь)
- **Связь**: output одного блока → input следующего блока

**Отслеживание цепи**:
- Цепь начинается от InputPoint (или первого блока, если он подключен к InputPoint)
- Каждый блок знает свой output и куда он подключен (следующий блок)
- Можем обойти цепь от начала до конца через output.connectedBlock

---

## 📝 ПОДРОБНЫЙ ПЛАН

### Подшаг 1.2.1: Усовершенствовать BlockConnector класс

**Файл**: `Assets/Scripts/Windows/CodeBlocks/BlockConnector.cs` (модифицировать существующий)

```csharp
public class BlockConnector : MonoBehaviour
{
    // Тип коннектора
    [SerializeField] private ConnectorType connectorType; // Input или Output
    public ConnectorType ConnectorType => connectorType;

    // Связь: куда подключен этот коннектор
    [SerializeField] private BlockConnector connectedTo;
    public BlockConnector ConnectedTo => connectedTo;

    // Обратная ссылка: какой блок содержит этот коннектор
    private BlockUI parentBlock;
    public BlockUI ParentBlock => parentBlock;

    private void OnValidate()
    {
        // Определить тип коннектора по имени или тегу
        if (connectorType == ConnectorType.None)
        {
            if (name.Contains("Input"))
                connectorType = ConnectorType.Input;
            else if (name.Contains("Output"))
                connectorType = ConnectorType.Output;
        }

        // Найти родительский блок
        if (parentBlock == null)
            parentBlock = GetComponentInParent<BlockUI>();
    }

    /// <summary>
    /// Подключить этот коннектор к другому коннектору
    /// (только для output → input)
    /// </summary>
    public bool ConnectTo(BlockConnector targetConnector)
    {
        // Валидация
        if (targetConnector == null)
        {
            Debug.LogWarning("[BlockConnector] Попытка подключиться к null!");
            return false;
        }

        if (connectorType != ConnectorType.Output || targetConnector.ConnectorType != ConnectorType.Input)
        {
            Debug.LogWarning("[BlockConnector] Можно подключать только Output → Input!");
            return false;
        }

        // Если уже что-то подключено, сначала отключить
        if (connectedTo != null)
            Disconnect();

        // Подключить
        connectedTo = targetConnector;
        Debug.Log($"[BlockConnector] Подключено: {parentBlock.name}.output → {targetConnector.ParentBlock.name}.input");
        return true;
    }

    /// <summary>
    /// Отключить этот коннектор от подключенного
    /// </summary>
    public void Disconnect()
    {
        if (connectedTo != null)
        {
            Debug.Log($"[BlockConnector] Отключено: {parentBlock.name}.output → {connectedTo.ParentBlock.name}.input");
            connectedTo = null;
        }
    }

    /// <summary>
    /// Получить блок, который подключен к этому коннектору (для output)
    /// </summary>
    public BlockUI GetConnectedBlock()
    {
        return connectedTo?.ParentBlock;
    }
}

public enum ConnectorType
{
    None,
    Input,
    Output
}
```

**Чек-лист**:
- [ ] Добавлены поля connectedTo, parentBlock
- [ ] Методы ConnectTo(), Disconnect(), GetConnectedBlock()
- [ ] ConnectorType enum определяет Input/Output
- [ ] OnValidate() автоматически определяет тип и родителя

---

### Подшаг 1.2.2: Добавить методы доступа в BlockUI

**Файл**: `Assets/Scripts/Windows/CodeBlocks/BlockUI.cs` (модифицировать)

```csharp
public class BlockUI : MonoBehaviour
{
    // ... существующий код ...

    private BlockConnector inputConnector;
    private BlockConnector outputConnector;

    private void Start()
    {
        // Найти коннекторы
        BlockConnector[] connectors = GetComponentsInChildren<BlockConnector>();
        foreach (var connector in connectors)
        {
            if (connector.ConnectorType == ConnectorType.Input)
                inputConnector = connector;
            else if (connector.ConnectorType == ConnectorType.Output)
                outputConnector = connector;
        }

        if (inputConnector == null)
            Debug.LogWarning($"[{name}] InputConnector не найдена!");
        if (outputConnector == null)
            Debug.LogWarning($"[{name}] OutputConnector не найдена!");
    }

    /// <summary>
    /// Получить входной коннектор блока
    /// </summary>
    public BlockConnector GetInputConnector()
    {
        return inputConnector;
    }

    /// <summary>
    /// Получить выходной коннектор блока
    /// </summary>
    public BlockConnector GetOutputConnector()
    {
        return outputConnector;
    }

    /// <summary>
    /// Получить следующий блок в цепи (подключен к выходу)
    /// </summary>
    public BlockUI GetNextBlock()
    {
        return outputConnector?.GetConnectedBlock();
    }

    /// <summary>
    /// Получить предыдущий блок в цепи (подключен к входу)
    /// </summary>
    public BlockUI GetPreviousBlock()
    {
        return inputConnector?.ConnectedTo?.ParentBlock;
    }
}
```

**Чек-лист**:
- [ ] Методы GetInputConnector(), GetOutputConnector() работают
- [ ] GetNextBlock() возвращает следующий блок через output
- [ ] GetPreviousBlock() возвращает предыдущий блок через input
- [ ] Все компилируется без ошибок

---

### Подшаг 1.2.3: Реализовать GetLastBlockInChain в ProgramAreaManager

**Файл**: `Assets/Scripts/Windows/CodeBlocks/ProgramAreaManager.cs` (добавить методы)

```csharp
public class ProgramAreaManager : MonoBehaviour
{
    // ... существующий код (из шага 1.1) ...

    // Первый блок в цепи (подключен к InputPoint)
    private BlockUI firstBlock;

    /// <summary>
    /// Установить первый блок в цепи (подключен к InputPoint)
    /// </summary>
    public void SetFirstBlock(BlockUI block)
    {
        firstBlock = block;
    }

    /// <summary>
    /// Получить первый блок в цепи
    /// </summary>
    public BlockUI GetFirstBlock()
    {
        return firstBlock;
    }

    /// <summary>
    /// Получить последний блок в цепи
    /// Если цепь пуста - вернуть null
    /// </summary>
    public BlockUI GetLastBlockInChain()
    {
        // Если нет блоков - последний это InputPoint (null)
        if (firstBlock == null)
            return null;

        // Обойти цепь от первого до последнего
        BlockUI currentBlock = firstBlock;
        BlockUI lastBlock = currentBlock;

        while (currentBlock != null)
        {
            lastBlock = currentBlock;
            currentBlock = currentBlock.GetNextBlock();
        }

        return lastBlock;
    }

    /// <summary>
    /// Получить весь список блоков в цепи (от начала до конца)
    /// </summary>
    public List<BlockUI> GetBlocksInChain()
    {
        var blocks = new List<BlockUI>();
        BlockUI current = firstBlock;

        while (current != null)
        {
            blocks.Add(current);
            current = current.GetNextBlock();
        }

        return blocks;
    }

    /// <summary>
    /// Получить позицию в цепи для блока (0 = первый, 1 = второй и т.д.)
    /// Если блок не в цепи - вернуть -1
    /// </summary>
    public int GetBlockPositionInChain(BlockUI block)
    {
        int position = 0;
        BlockUI current = firstBlock;

        while (current != null)
        {
            if (current == block)
                return position;

            current = current.GetNextBlock();
            position++;
        }

        return -1; // Блок не найден в цепи
    }
}
```

**Чек-лист**:
- [ ] Метод GetLastBlockInChain() корректно обходит цепь
- [ ] Возвращает null если цепь пуста
- [ ] GetBlocksInChain() возвращает все блоки в порядке
- [ ] GetBlockPositionInChain() находит позицию блока или -1

---

### Подшаг 1.2.4: Реализовать ConnectBlocks в ProgramAreaManager

**Файл**: `Assets/Scripts/Windows/CodeBlocks/ProgramAreaManager.cs` (добавить)

```csharp
public class ProgramAreaManager : MonoBehaviour
{
    // ... существующий код ...

    /// <summary>
    /// Подключить два блока: prevBlock.output → newBlock.input
    /// Если prevBlock = null → подключить к InputPoint (первый блок)
    /// </summary>
    public bool ConnectBlocks(BlockUI prevBlock, BlockUI newBlock)
    {
        if (newBlock == null)
        {
            Debug.LogWarning("[ProgramAreaManager] Новый блок не может быть null!");
            return false;
        }

        if (prevBlock == null)
        {
            // Подключить к InputPoint (новый блок становится первым)
            firstBlock = newBlock;
            Debug.Log($"[ProgramAreaManager] {newBlock.name} подключена к InputPoint (первый блок)");
            return true;
        }

        // Подключить prevBlock.output → newBlock.input
        BlockConnector outputConnector = prevBlock.GetOutputConnector();
        BlockConnector inputConnector = newBlock.GetInputConnector();

        if (outputConnector == null || inputConnector == null)
        {
            Debug.LogError("[ProgramAreaManager] Не удалось найти коннекторы!");
            return false;
        }

        bool success = outputConnector.ConnectTo(inputConnector);

        if (success)
        {
            Debug.Log($"[ProgramAreaManager] Подключено: {prevBlock.name} → {newBlock.name}");
        }

        return success;
    }

    /// <summary>
    /// Добавить блок в конец цепи
    /// </summary>
    public bool AppendBlockToChain(BlockUI newBlock)
    {
        BlockUI lastBlock = GetLastBlockInChain();

        // Если цепь пуста - новый блок становится первым
        if (lastBlock == null)
            return ConnectBlocks(null, newBlock);

        // Добавить в конец
        return ConnectBlocks(lastBlock, newBlock);
    }

    /// <summary>
    /// Добавить блок в начало цепи
    /// </summary>
    public bool PrependBlockToChain(BlockUI newBlock)
    {
        if (firstBlock == null)
        {
            // Цепь пуста
            firstBlock = newBlock;
            return true;
        }

        // Новый блок подключается к InputPoint
        // Старый первый блок подключается к новому
        ConnectBlocks(newBlock, firstBlock);
        firstBlock = newBlock;
        return true;
    }
}
```

**Чек-лист**:
- [ ] ConnectBlocks() подключает два блока корректно
- [ ] Обрабатывает null (подключение к InputPoint)
- [ ] AppendBlockToChain() добавляет в конец
- [ ] PrependBlockToChain() добавляет в начало
- [ ] Все методы логируют операции

---

### Подшаг 1.2.5: Тестирование подключения

**Файл**: `Assets/Scripts/Tests/BlockConnectionTests.cs` (создать)

```csharp
using UnityEngine;
using PU.Windows;

public class BlockConnectionTests : MonoBehaviour
{
    public void TestConnectTwoBlocks()
    {
        // 1. Получить ProgramAreaManager
        var window = GetComponent<CodeBlocksWindow>();
        var manager = window.GetProgramAreaManager();

        // 2. Создать два тестовых блока (или получить из палитры)
        BlockUI block1 = CreateTestBlock("Block1");
        BlockUI block2 = CreateTestBlock("Block2");

        // 3. Подключить
        bool success = manager.ConnectBlocks(block1, block2);

        Debug.Assert(success, "Подключение не удалось!");
        Debug.Assert(block1.GetNextBlock() == block2, "Next блок неправильный!");
        Debug.Assert(block2.GetPreviousBlock() == block1, "Previous блок неправильный!");

        Debug.Log("✅ TestConnectTwoBlocks PASSED");
    }

    public void TestAppendToChain()
    {
        var window = GetComponent<CodeBlocksWindow>();
        var manager = window.GetProgramAreaManager();

        BlockUI block1 = CreateTestBlock("Block1");
        BlockUI block2 = CreateTestBlock("Block2");
        BlockUI block3 = CreateTestBlock("Block3");

        // Добавить в пустую цепь
        manager.AppendBlockToChain(block1);
        Debug.Assert(manager.GetFirstBlock() == block1, "Первый блок неправильный!");

        // Добавить второй
        manager.AppendBlockToChain(block2);
        Debug.Assert(block1.GetNextBlock() == block2, "Цепь нарушена!");

        // Добавить третий
        manager.AppendBlockToChain(block3);
        Debug.Assert(manager.GetLastBlockInChain() == block3, "Последний блок неправильный!");

        Debug.Log("✅ TestAppendToChain PASSED");
    }

    public void TestGetBlocksInChain()
    {
        var window = GetComponent<CodeBlocksWindow>();
        var manager = window.GetProgramAreaManager();

        BlockUI[] blocks = new BlockUI[3];
        for (int i = 0; i < 3; i++)
        {
            blocks[i] = CreateTestBlock($"Block{i}");
            manager.AppendBlockToChain(blocks[i]);
        }

        var chainList = manager.GetBlocksInChain();
        Debug.Assert(chainList.Count == 3, "Неправильное количество блоков!");
        Debug.Assert(chainList[0] == blocks[0], "Порядок блоков неправильный!");
        Debug.Assert(chainList[2] == blocks[2], "Последний блок неправильный!");

        Debug.Log("✅ TestGetBlocksInChain PASSED");
    }

    private BlockUI CreateTestBlock(string name)
    {
        // Упрощенное создание тестового блока
        GameObject go = new GameObject(name);
        BlockUI block = go.AddComponent<BlockUI>();
        go.AddComponent<BlockConnector>().ConnectorType = ConnectorType.Input;
        go.AddComponent<BlockConnector>().ConnectorType = ConnectorType.Output;
        return block;
    }

    public void RunAllTests()
    {
        TestConnectTwoBlocks();
        TestAppendToChain();
        TestGetBlocksInChain();
        Debug.Log("✅ ВСЕ ТЕСТЫ ПРОЙДЕНЫ!");
    }
}
```

**Как запустить тесты**:
1. Добавить скрипт BlockConnectionTests на любой GameObject в сцене
2. Вызвать `RunAllTests()` из инспектора или консоли
3. Проверить логи в консоли

**Чек-лист**:
- [ ] TestConnectTwoBlocks() проходит ✅
- [ ] TestAppendToChain() проходит ✅
- [ ] TestGetBlocksInChain() проходит ✅
- [ ] Все логи выводятся корректно

---

## 🧪 ИНТЕГРАЦИОННОЕ ТЕСТИРОВАНИЕ

### Тест 1: Подключение через инспектор
1. Создать сцену с двумя BlockUI
2. Вручную подключить их коннекторы через инспектор
3. Запустить игру, проверить что GetNextBlock() возвращает правильный блок

**Результат**: ✅ / ❌

### Тест 2: Добавление в цепь через код
1. Запустить TestAppendToChain()
2. Проверить что первый блок знает про второй, второй про третий

**Результат**: ✅ / ❌

### Тест 3: Обход цепи
1. Создать цепь из 5 блоков
2. GetBlocksInChain() должен вернуть ровно 5 блоков в правильном порядке

**Результат**: ✅ / ❌

---

## 📦 АРТЕФАКТЫ ЭТАПА

**Модифицированные файлы**:
- `Assets/Scripts/Windows/CodeBlocks/BlockConnector.cs` ← добавлены связи и методы
- `Assets/Scripts/Windows/CodeBlocks/BlockUI.cs` ← добавлены методы доступа
- `Assets/Scripts/Windows/CodeBlocks/ProgramAreaManager.cs` ← добавлена логика цепи

**Новые файлы**:
- `Assets/Scripts/Tests/BlockConnectionTests.cs` ← набор тестов

**Новый enum**:
- `ConnectorType` (Input, Output)

---

## 🚀 СЛЕДУЮЩИЙ ШАГ (1.3)

После завершения этого шага:
- Можно подключать два блока и узнавать порядок в цепи
- ProgramAreaManager знает первый и последний блок в цепи
- Блоки знают своих соседей (next/previous)
- Готово для шага 1.3 (отключение блока и сращивание цепи)

---

## 📝 ЗАМЕТКИ

- ConnectBlocks() это основной метод, все операции через него
- GetLastBlockInChain() вернет null если цепь пуста (нет блоков)
- GetBlockPositionInChain() полезна для отладки и UI (показать номер блока)
- BlockConnector.ConnectedTo это односторонняя связь (output → input)
- При подключении нового блока к уже подключенному выходу - старое подключение автоматически разрывается

---

**Версия**: 1.0
**Дата**: 29 янв 2026
