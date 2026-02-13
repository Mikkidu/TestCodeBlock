# Задача #25 Шаг 1: Публичные API методы + Stop при Reset

## Цель
Добавить публичные API методы для управления программой из внешнего кода (play-united) и исправить отсутствие остановки программы при Reset.

## Контекст
**Текущая ситуация:**
- Все методы управления программой приватные (OnRunButtonClicked, OnStopButtonClicked, etc.)
- Внешний код (MiniGameManager в play-united) не может управлять программой
- OnResetButtonClicked() дублирует логику Stop вместо переиспользования OnStopButtonClicked()
- OnClearButtonClicked() уже правильно вызывает OnStopButtonClicked()

**Файл:** `Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs`

## Ключевые шаги

### 1. Добавить публичные API методы (5 методов)

**Расположение:** После метода `CardinalDirectionToRotation()` (строка 318), перед `OnResetButtonClicked()`

**Код для добавления:**
```csharp
// =========================
// PUBLIC API для внешнего управления
// =========================

/// <summary>
/// Starts program execution from external code. Equivalent to clicking Run button.
/// </summary>
public void StartProgram()
{
    OnRunButtonClicked();
}

/// <summary>
/// Stops program execution from external code. Equivalent to clicking Stop button.
/// </summary>
public void StopProgram()
{
    OnStopButtonClicked();
}

/// <summary>
/// Clears all blocks from program area. Equivalent to clicking Clear button.
/// Automatically stops running program if any.
/// </summary>
public void ClearProgram()
{
    OnClearButtonClicked();
}

/// <summary>
/// Returns true if program is currently running.
/// </summary>
public bool IsProgramRunning => isProgramRunning;

/// <summary>
/// Returns number of blocks currently in program area.
/// </summary>
public int GetBlocksCount()
{
    return programArea?.GetBlocks().Count ?? 0;
}
```

**Зачем:**
- `StartProgram()` - запуск программы из play-united UI
- `StopProgram()` - остановка программы из play-united UI
- `ClearProgram()` - очистка программы из play-united UI
- `IsProgramRunning` - проверка состояния для блокировки UI
- `GetBlocksCount()` - статистика использованных блоков

### 2. Рефакторинг OnResetButtonClicked() для переиспользования Stop логики

**Текущий код (строки 320-349):**
```csharp
private void OnResetButtonClicked()
{
    // Дублирует логику из OnStopButtonClicked (строки 186-197)
    if (programArea != null)
    {
        List<BlockUIBase> blocks = programArea.GetBlocks();
        foreach (var block in blocks)
        {
            if (block.Command is Commands.LoopCommand loopCmd)
            {
                loopCmd.RequestStop();
            }
        }
    }

    if (commandExecutor != null)
    {
        commandExecutor.Stop();
    }

    if (robotController != null)
    {
        robotController.Reset();
    }

    robotPositionTracker?.ResetPosition();

    isProgramRunning = false;
    currentExecutingCommand = null;
    UpdateStatusDisplay("Reset completed");
}
```

**Новый код:**
```csharp
private void OnResetButtonClicked()
{
    // Stop program if running (reuses OnStopButtonClicked logic)
    if (isProgramRunning)
    {
        OnStopButtonClicked();
    }

    // Reset robot to start position
    if (robotController != null)
    {
        robotController.Reset();
    }

    // Reset position tracker
    robotPositionTracker?.ResetPosition();

    // Update UI
    UpdateStatusDisplay("Reset completed");
}
```

**Изменения:**
- Удалить дублирование Stop логики (строки 322-337, 346-347)
- Добавить вызов `OnStopButtonClicked()` если программа выполняется
- Оставить только Reset логику (robotController, robotPositionTracker)
- Упростить код с 30 строк до 16 строк

**Зачем:**
- DRY принцип - не дублировать код остановки
- Консистентность - Reset всегда останавливает программу
- Проще поддерживать - Stop логика в одном месте

### 3. Проверить OnClearButtonClicked() - уже корректен

**Текущий код (строки 351-364):**
```csharp
private void OnClearButtonClicked()
{
    if (isProgramRunning)
    {
        OnStopButtonClicked(); // ✅ Уже правильно вызывает Stop
    }

    if (programArea != null)
    {
        programArea.ClearProgram();
    }

    UpdateStatusDisplay("Program cleared");
}
```

**Действие:** НЕ ИЗМЕНЯТЬ - уже корректно реализовано.

## Блокирующие факторы
- Нет

## Критерии приёмки

### Публичные методы:
- [ ] `StartProgram()` запускает программу (вызов из play-united работает)
- [ ] `StopProgram()` останавливает программу (вызов из play-united работает)
- [ ] `ClearProgram()` очищает ProgramArea (вызов из play-united работает)
- [ ] `IsProgramRunning` возвращает `true` во время выполнения, `false` после
- [ ] `GetBlocksCount()` возвращает корректное количество блоков (0, 1, 5, etc.)

### Stop при Reset:
- [ ] Нажать Run → программа выполняется
- [ ] Нажать Reset во время выполнения → программа останавливается (Loop команды, commandExecutor)
- [ ] Робот возвращается на start point
- [ ] UI показывает "Reset completed"

### Regression:
- [ ] Кнопки Run/Stop/Clear/Reset из UI продолжают работать
- [ ] OnClearButtonClicked() НЕ изменён (уже корректен)
- [ ] Компиляция проходит без ошибок

## Заметки

**Архитектура:**
```
Внешний код (play-united)
    ↓
Public API методы (StartProgram, StopProgram, etc.)
    ↓
Приватные обработчики (OnRunButtonClicked, OnStopButtonClicked, etc.)
    ↓
Исполнение (commandExecutor, robotController)
```

**Совместимость:**
- Существующий UI (кнопки) продолжает работать через приватные методы
- Новый внешний код использует публичные методы
- Никаких breaking changes

**Время выполнения:**
- Добавить API методы: 10 минут
- Рефакторинг OnResetButtonClicked: 10 минут
- Тестирование: 20 минут
- **Итого:** ~40 минут
