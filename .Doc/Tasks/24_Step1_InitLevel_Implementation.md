# Задача #24 - Шаг 1: Реализация InitLevel() для множественной загрузки уровней

**Дата:** 2026-01-26
**Статус:** ✅ ВЫПОЛНЕН
**Приоритет:** 🔴 CRITICAL (блокирует интеграцию в play-united)

---

## Цель

Переделать `GameManager.LoadLevel()` чтобы можно было вызывать несколько раз подряд с разными уровнями. Текущая реализация работает только один раз в `Start()`.

## Контекст

### Архитектурное решение для интеграции
В play-united проекте нужен публичный API для загрузки уровней:
```
play-united:
  CodeBlocksGameWindow.OnShowing() → gameManager.InitLevel(level)
```

### Разделение ответственности
- **`Init()`** - приватный, однократный (инициализация компонентов, подписка на события)
- **`InitLevel(level)`** - публичный, многократный (загрузка конкретного уровня)

### Ключевое требование
**ВСЕГДА очищать ProgramArea** при загрузке нового уровня. Рестарт уровня в play-united будет через отдельный механизм (`OnResetButtonClicked()`), без вызова `InitLevel()`.

---

## Реализация

### 1. Добавлен флаг `isInitialized`
```csharp
private bool isInitialized = false;
```

**Файл:** `GameManager.cs:33`

### 2. Переделан метод `Init()`
```csharp
private void Init()
{
    // Prevent multiple initialization
    if (isInitialized)
    {
        return;
    }

    // ... существующая логика инициализации ...

    isInitialized = true;
    Debug.Log("GameManager: Initialized successfully");
}
```

**Изменения:**
- Добавлена проверка `isInitialized` в начале
- Удалён `UpdateStatusDisplay()` в конце (перенесён в `InitLevel`)
- Добавлен debug лог для отладки

**Файл:** `GameManager.cs:43-113`

### 3. Создан публичный метод `InitLevel(LevelGridData level)`
```csharp
/// <summary>
/// Initialize and load a level. Can be called multiple times to switch levels.
/// This method performs lazy initialization on first call, then loads the specified level.
/// Always clears the program area and stops any running program.
/// </summary>
public void InitLevel(LevelGridData level)
{
    // 1. Lazy initialization (only happens once)
    if (!isInitialized)
    {
        Init();
    }

    // 2. Stop running program if any
    if (isProgramRunning)
    {
        OnStopButtonClicked();
    }

    // 3. ALWAYS clear program when loading new level
    if (programArea != null)
    {
        programArea.ClearProgram();
    }

    // 4. Load the level
    if (level != null)
    {
        LoadLevel(level);
        UpdateStatusDisplay("Уровень загружен");
    }
    else
    {
        Debug.LogWarning("GameManager: Cannot initialize with null level!");
        UpdateStatusDisplay("Ошибка загрузки уровня");
    }
}
```

**Файл:** `GameManager.cs:217-256`

### 4. Обновлён `Start()` метод
```csharp
private void Start()
{
    if (currentLevel != null)
    {
        InitLevel(currentLevel); // ← Было: LoadLevel(currentLevel)
    }
    else
    {
        Debug.LogWarning("GameManager: No level assigned!");
    }
}
```

**Файл:** `GameManager.cs:204-214`

### 5. Обновлён `LevelRuntimeManagerTest.cs`
```csharp
[SerializeField] private GameManager gameManager; // ← Было: LevelRuntimeManager levelManager

private void Start()
{
    if (gameManager != null && testLevel != null)
    {
        gameManager.InitLevel(testLevel); // ← Было: levelManager.LoadLevel(testLevel)
        TestCoordinateConversion();
    }
}

private void TestCoordinateConversion()
{
    // Find LevelRuntimeManager for coordinate testing
    LevelRuntimeManager levelManager = FindFirstObjectByType<LevelRuntimeManager>();
    // ... rest of the test
}
```

**Файл:** `LevelRuntimeManagerTest.cs:4-24`

---

## Логика работы

### Первый вызов `InitLevel(level1)`
1. `isInitialized == false` → вызвать `Init()`
2. `Init()` инициализирует компоненты, подписывается на события, устанавливает `isInitialized = true`
3. Остановить программу (если запущена) → NO-OP
4. Очистить ProgramArea
5. Загрузить `level1` через `LoadLevel()`

### Второй вызов `InitLevel(level2)`
1. `isInitialized == true` → **пропустить Init()**
2. Остановить программу (если запущена) → вызов `OnStopButtonClicked()`
3. Очистить ProgramArea → **программа удалена**
4. Загрузить `level2` через `LoadLevel()`

### `LoadLevel()` внутри
```csharp
public void LoadLevel(LevelGridData level)
{
    // LevelRuntimeManager.LoadLevel() вызывает ClearLevel() внутри
    levelRuntimeManager.LoadLevel(level); // ← Очищает старый уровень автоматически

    // Позиционирование робота
    PositionRobotAtStart(level);

    // Переинициализация GridPositionTracker
    robotPositionTracker.Initialize(levelRuntimeManager, level); // ← Сбрасывает hasReachedFinish
}
```

---

## Преимущества архитектуры

✅ **Ленивая инициализация** - Init() вызывается автоматически при первом InitLevel()
✅ **Idempotent** - можно вызывать InitLevel() сколько угодно раз
✅ **Безопасность** - всегда останавливает программу перед загрузкой
✅ **Чистота** - всегда очищает ProgramArea перед загрузкой
✅ **Memory-safe** - LevelRuntimeManager.ClearLevel() удаляет старые GameObjects

---

## Использование в play-united

### Загрузка нового уровня
```csharp
// CodeBlocksGameWindow.cs
void LoadNextLevel()
{
    currentLevelIndex++;
    LevelGridData nextLevel = levelSequence[currentLevelIndex];
    gameManager.InitLevel(nextLevel); // ← Очищает программу, загружает новый уровень
}
```

### Рестарт текущего уровня
```csharp
// CodeBlocksGameWindow.cs
void RestartCurrentLevel()
{
    gameManager.OnResetButtonClicked(); // ← Робот на старт, программа НЕ очищается
}
```

---

## Тестирование

### Тест-кейсы
- [ ] **Test 1:** Первый вызов `InitLevel(level1)` - инициализация + загрузка
- [ ] **Test 2:** Второй вызов `InitLevel(level2)` - очистка + загрузка (без повторной инициализации)
- [ ] **Test 3:** Вызов с `null` - логирование предупреждения
- [ ] **Test 4:** Остановка программы при загрузке нового уровня
- [ ] **Test 5:** ProgramArea очищается при каждом InitLevel
- [ ] **Test 6:** Memory leak test - многократные вызовы InitLevel не создают утечек

### Как проверить в Unity
1. Открыть Unity Editor
2. Проверить компиляцию (Console должен быть чист)
3. Play Mode → проверить первый уровень загружается
4. Создать тестовую кнопку для вызова `InitLevel(otherLevel)`
5. Нажать кнопку → проверить что старый уровень удалён, новый загружен, программа очищена

---

## Следующие шаги

- [ ] **Шаг 2:** Протестировать в Unity Editor
- [ ] **Шаг 3:** Добавить debug UI для переключения уровней в TestCodeBlock проекте
- [ ] **Шаг 4:** Создать Memory Profiler тест для проверки утечек
- [ ] **Шаг 5:** Обновить документацию API

---

## Изменённые файлы

- `Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManagerTest.cs`

---

## Заметки

- `LevelRuntimeManager.ClearLevel()` уже правильно реализован (удаляет все GameObjects)
- `GridPositionTracker.Initialize()` уже сбрасывает `hasReachedFinish` флаг
- Не требуется дополнительная очистка событий (OnDestroy всё отпишет)
