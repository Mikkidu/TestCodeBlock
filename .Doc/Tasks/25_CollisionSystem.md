# Задача #25: Collision System & Cell Reactions - Система реакции робота на типы клеток

**Status**: 🟡 **IN PROGRESS** (обновлено 2026-02-11)
**Priority**: 🔴 CRITICAL (gameplay foundation)
**Depends On**: #21 ✓ (Finish Detection), #19 ✓ (Robot Grid Integration), #20 ✓ (GridPositionTracker), #24 ✓ (InitLevel), #26 ✓ (InputPoint), #27 ✓ (CancellationToken)

---

## Актуальный прогресс (2026-02-11)

- Выполнено:
  - Door/Button pipeline стабилизирован: закрытая дверь блокирует, открытая пропускает, повторное переключение работает.
  - Устранена задержка реакции двери после нажатия кнопки.
  - Добавлен исход движения `StopProgramAtTarget` для сценариев `FinishPoint` и `NoTerrain`.
  - Runtime уровень изолирован от source asset и корректно очищается (`Destroy(currentLevel)`).
  - `GridPositionTracker` инициализируется runtime-уровнем, а не source asset.
- Проверка:
  - `dotnet build Assembly-CSharp.csproj` проходит без ошибок.
- Ближайший следующий шаг:
  - добавить полноценную реакцию на `Pit` в общем reaction pipeline (с анимацией робота), затем `Finish`/`Spike` в том же стиле.

---

## 📋 Обзор задачи

**Мотивация**: Реализовать реакции робота при переходе на разные типы клеток поля (стены, ямы, шипы). Система должна быть модульной и легко расширяемой.

**Исходное задание (#25 backlog)**:
- ✅ **Floor** (пол) — обычное движение (уже работает)
- **Wall** (стена) — блокада, откат назад
- **Spike** (шип) — ловушка, повреждение робота
- **Pit** (яма) — ловушка, падение в яму
- ✅ **Finish** (финиш) — остановка программы (уже работает в #21)

---

## 🎯 Критические требования

### 1. **Finish имеет приоритет** ✅ (частично реализовано)
- Если робот достигает финиша → программа ОСТАНАВЛИВАЕТСЯ НЕМЕДЛЕННО
- Даже если в очереди есть команды, они НЕ выполняются
- Нужно убедиться что Finish проверяется ПЕРВЫМ в GridPositionTracker

### 2. **Wall collision (стена)** ❌ (нужно реализовать)
- Робот пытается войти на стену → откатывается назад
- Анимация: move forward (0.3s) → bounce back (0.3s)
- Программа НЕ прерывается, следующая команда выполняется нормально

### 3. **Pit (яма)** ❌ (нужно реализовать)
- Робот падает в яму
- Анимация падения (1.0s)
- Программа ОСТАНАВЛИВАЕТСЯ

### 4. **Spike (шип)** ❌ (нужно реализовать)
- Робот ломается при попадании на шип
- Анимация поломки: мигание (0.5s)
- Программа ОСТАНАВЛИВАЕТСЯ

---

## 🏗️ Архитектурный подход

### Текущее состояние кодовой базы:

**✅ Уже есть:**
- `LevelGridData.terrain[]` → `TerrainCell` с полем `terrainType` ("Ground", "Road", "Pit")
- `LevelGridData.objects[]` → `GridObject` с полем `objectTypeId` ("Wall", ...)
- `GridPositionTracker` → отслеживает позицию, вызывает `OnGridPositionChanged`, `OnReachedFinish`
- `RobotController.MoveForward/Backward()` → возвращают `IPromise`
- `CommandExecutor` → использует `ExecutionContext.IsCancelled` флаг для остановки (#27)
- `LevelRuntimeManager` → конвертирует world ↔ grid координаты

**❌ Нужно добавить:**
1. **CellReactionConfig** - конфигурация реакций (Bounce, Fall, Break)
2. **Cell Reaction System** - обработчик реакций
3. **Wall Collision** - логика отката при столкновении
4. **Pit/Spike Logic** - обработка ловушек с анимациями

### Модульная структура:

```
Packages/com.codeblocks.robotprogramming/Runtime/
├── Collision/                          (новая папка)
│   ├── CellReactionConfig.cs          # Конфигурация реакций (Bounce, Fall, Break)
│   ├── CellReactionType.cs            # Enum: Move, Bounce, Fall, Break
│   ├── CellReactionProcessor.cs       # Обработчик реакций (монобиха)
│   ├── Reactions/                     (подпапка с реакциями)
│   │   ├── IReaction.cs               # Интерфейс для любой реакции
│   │   ├── BounceReaction.cs          # Откат назад (Wall)
│   │   ├── FallReaction.cs            # Падение (Pit)
│   │   └── BreakReaction.cs           # Поломка (Spike)
│   └── CollisionDebugger.cs           # Debug визуализация
└── Robot/
    └── GridPositionTracker.cs         # Интеграция CellReactionProcessor
```

---

## 📝 Разбор по блокам задач

### **БЛОК 1: Cell Type System** (2 часа)
**Цель**: Система типов клеток и конфигурация реакций (только 4 типа)

**Файлы для создания:**
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionType.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionConfig.cs`

**Детальные шаги:**
1. Создать enum `CellReactionType` (Move, Bounce, Fall, Break)
2. Создать struct `CellReaction` с полями: type, animationDuration, animationCurve
3. Создать ScriptableObject `CellReactionConfig` с реакциями по типам
4. Добавить метод `LevelGridData.GetCellReaction(gridPos)` → `CellReaction`
5. Тестирование: создать конфиг в Unity Editor

**План для docs**: 📄 `.Doc/Tasks/25_Block1_CellTypeSystem.md`

---

### **БЛОК 2: Finish Logic Improvements** (2 часа)
**Цель**: Гарантировать что Finish ВСЕГДА имеет приоритет

**Файлы для изменения:**
- `Packages/com.codeblocks.robotprogramming/Runtime/Robot/GridPositionTracker.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs`

**Детальные шаги:**
1. В GridPositionTracker: проверка Finish ПЕРВОЙ (перед остальными реакциями)
2. При достижении Finish: `commandExecutor.Stop()` (остановить программу)
3. В GameManager: флаг `levelCompleted` для разделения Finish vs Reset
4. При Finish: UI message "Уровень пройден! 🎉" и блокировка UI
5. Убедиться что программа ОСТАНАВЛИВАЕТСЯ без выполнения остальных команд

**План для docs**: 📄 `.Doc/Tasks/25_Block2_FinishLogicImprovement.md`

---

### **БЛОК 3: Wall Collision** (3 часа)
**Цель**: Откат робота при столкновении со стеной

**Файлы для создания:**
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/CellReactionProcessor.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/IReaction.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/Reactions/BounceReaction.cs`

**Детальные шаги:**
1. Создать интерфейс `IReaction` с методом `Execute(robot, tracker, config, context) → IPromise`
2. Создать `BounceReaction`: откат на 1 шаг + bounce анимация
3. Создать `CellReactionProcessor` компонент:
   - Слушает `GridPositionTracker.OnGridPositionChanged`
   - Если Wall → выполняет BounceReaction
4. Логика отката: MoveForward(0.5) → MoveBackward(0.5) с анимацией
5. Программа продолжает выполняться после bounce

**План для docs**: 📄 `.Doc/Tasks/25_Block3_WallCollision.md`

---

### **БЛОК 4: Pit/Spike Logic** (2 часа)
**Цель**: Реакции на ловушки (падение в яму, поломка на шипе)

**Файлы для создания:**
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/Reactions/FallReaction.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Collision/Reactions/BreakReaction.cs`

**Детальные шаги:**
1. Создать `FallReaction`: анимация падения (масштабирование + сдвиг Y)
2. Создать `BreakReaction`: анимация мигания робота
3. Добавить методы в `RobotController`: `PlayFallAnimation()`, `PlayBreakAnimation()`
4. Интегрировать в `CellReactionProcessor`
5. При Fall/Break: программа ОСТАНАВЛИВАЕТСЯ, UI показывает сообщение

**План для docs**: 📄 `.Doc/Tasks/25_Block4_PitSpikeLogic.md`

---

### **БЛОК 5: Integration & Testing** (2 часа)
**Цель**: Полная интеграция и тестирование всей системы

**Файлы для изменения:**
- `Packages/com.codeblocks.robotprogramming/Runtime/Managers/GameManager.cs`
- `Packages/com.codeblocks.robotprogramming/Runtime/Managers/LevelRuntimeManager.cs`

**Детальные шаги:**
1. Интегрировать `CellReactionProcessor` с `GameManager`
2. Убедиться в правильности приоритетов: Finish > Pit/Spike > Wall
3. Создать тестовый уровень с Wall, Pit, Spike
4. Создать префабы для Pit и Spike (если не существуют)
5. Полное тестирование на всех 5 tutorial уровнях

**План для docs**: 📄 `.Doc/Tasks/25_Block5_Integration.md`

---

## 🧪 План тестирования

### Чек-лист для каждого типа клетки:

- [ ] **Floor**: Робот проходит нормально (текущее поведение)
- [ ] **Wall**:
  - [ ] Попытка движения вперёд → bounce back анимация
  - [ ] Робот возвращается на исходную позицию
  - [ ] Программа продолжает выполняться
  - [ ] Следующая команда выполняется
- [ ] **Pit**:
  - [ ] Робот падает при движении на Pit
  - [ ] Анимация падения (1.0 сек)
  - [ ] Программа останавливается
  - [ ] UI показывает сообщение об ошибке
  - [ ] Reset возвращает робота на старт
- [ ] **Spike**:
  - [ ] Робот ломается при движении на Spike
  - [ ] Анимация поломки: мигание (0.5 сек)
  - [ ] Программа останавливается
  - [ ] UI показывает сообщение об ошибке
- [ ] **Finish**:
  - [ ] Робот достигает финиша → программа ВСЕГДА останавливается
  - [ ] Даже если в очереди есть команды, они НЕ выполняются ✅
  - [ ] UI показывает "Level Completed! 🎉" ✅

### Интеграция-тесты:

- [ ] Уровень tutorial_03_avoid_obstacles работает (Wall navigation)
- [ ] Уровень tutorial_05_complex_maze работает (многоэтапное решение)
- [ ] Создать новый тестовый уровень с Pit и Spike для проверки

---

## 📌 Ключевые архитектурные решения

### 1. **Модульность через IReaction**
```csharp
// Любая новая реакция = новый класс :IReaction
public interface IReaction
{
    string Name { get; }
    CellReactionType ReactionType { get; }
    IPromise Execute(RobotController robot, GridPositionTracker tracker, ExecutionContext context);
}

// Новая реакция: просто реализуй интерфейс
public class CustomReaction : IReaction { ... }
```

### 2. **Конфигурируемость через ScriptableObject**
```csharp
// Каждый уровень может иметь свою конфигурацию реакций
[CreateAssetMenu(...)]
public class CellReactionConfig : ScriptableObject
{
    public ReactionData[] reactionsByTerrainType;  // Pit → FallReaction, Spike → BreakReaction
}
```

### 3. **Promise-based асинхронность**
- Все реакции возвращают `IPromise` для синхронизации
- `CellReactionProcessor` ждёт завершения реакции перед тем как вернуться в `GridPositionTracker`
- Позволяет анимировать отката, падения, поломки без блокировки потока

### 4. **Приоритеты обработки**
```
1. Finish Point → STOP программу сразу
2. Wall → Bounce back (продолжить программу)
3. Pit/Spike → Stop программу
4. Water/Ice → Special handling (опционально)
5. Floor → Normal movement
```

---

## 📊 Дополнительные метрики

**Estimated Lines of Code**: ~800 LOC (вместо 1200)
**New Files**: 6 файлов (вместо 8)
**Modified Files**: 4 файла
**Test Coverage**: ~70% (unit + integration tests)
**Общее время разработки**: ~11 часов (вместо 13)

---

## 🔗 Связь с другими задачами

```
#25 (Collision System) зависит от:
├─ #19 ✓ (Robot Grid Integration) - grid conversions
├─ #20 ✓ (GridPositionTracker) - position tracking
├─ #21 ✓ (Finish Detection) - finish logic (расширяем)
├─ #24 ✓ (InitLevel) - level loading (используем)
└─ #26 ✓ (InputPoint API) - program structure

#25 будет использоваться в:
├─ play-united (game integration)
├─ Tutorial levels (новый туториал с ловушками)
├─ Future: Level Designer tools
└─ Future: Difficulty levels (больше Spike/Pit при higher difficulty)
```

---

## 📚 Документация

Для каждого блока будет создан отдельный файл:

1. 📄 `.Doc/Tasks/25_Block1_CellTypeSystem.md` - Система типов клеток
2. 📄 `.Doc/Tasks/25_Block2_FinishLogicImprovement.md` - Улучшение Finish логики
3. 📄 `.Doc/Tasks/25_Block3_WallCollision.md` - Wall Collision с откатом
4. 📄 `.Doc/Tasks/25_Block4_AnimationMapping.md` - Система анимаций
5. 📄 `.Doc/Tasks/25_Block5_Integration.md` - Интеграция всех компонентов

**Итоговый документ:** `.Doc/Architecture_CollisionSystem.md` - архитектурный обзор всей системы

---

## ✅ Acceptance Criteria

- [ ] Система типов клеток расширена (Pit, Spike, Water)
- [ ] Wall столкновения работают с откатом
- [ ] Pit вызывает падение + stop программы
- [ ] Spike вызывает поломку + stop программы
- [ ] Finish ВСЕГДА останавливает программу (даже с queued командами)
- [ ] Анимации воспроизводятся для каждого типа реакции
- [ ] Все реакции модульны и легко расширяются
- [ ] Интегрировано с GridPositionTracker
- [ ] Протестировано в Editor и в play-united
- [ ] Код скомпилирован, 0 errors, console чистая

---

## 🎬 Начало разработки

- Старт: 9 фев 2026
- Блок 1-2: Первая неделя (Cell Types + Finish Logic)
- Блок 3-4: Вторая неделя (Collision + Animations)
- Блок 5: Финал (Integration + Testing)

**Next**: Перейти к детальному планированию Блока 1 в `.Doc/Tasks/25_Block1_CellTypeSystem.md`

---

## Текущий статус (2026-02-10)

**Что сделано:**
- Введён DecisionService до шага и маппинг реакций через конфиг.
- Реакции для объектов: стена, кнопка, дверь. Кнопка переключает дверь.
- Визуальное состояние двери/кнопки: с анимацией через `Animator` или через enable/disable без анимации.
- Фиксы object ids и корректная активация ячейки при `ReturnToOrigin`.
- Базовая анимация робота `Idle/Move` подключена через `Animator` bool-параметр (`moveBoolParameter`, по умолчанию `IsMoving`).
- Визуал кнопки обновляется по world-позиции робота, нажатие/отжатие происходит на середине перехода между клетками.
- Runtime-состояние дверей изолировано от source-ассета уровня через runtime-копию `LevelGridData`.

**Что проверено:**
- Ручной тест в Unity: дверь блокирует, открытая дверь пропускает, кнопка переключает состояние.
- `dotnet build Assembly-CSharp.csproj` — без ошибок.

**Что запланировано:**
- Анимации поворота (left/right 90) с fallback на кодовый поворот.
- Подключение реакций Pit/Spike через анимации робота.
- Маппинг реакций через конфиг (строка id → анимация/поведение).
