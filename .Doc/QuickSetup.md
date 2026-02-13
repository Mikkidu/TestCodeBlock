# ⚡ Быстрая настройка сцены

## Шаг 1: Создать Robot prefab

```
1. В Hierarchy создайте пустой GameObject → переименуйте в "Robot"
2. Добавьте компоненты:
   - Transform: Position (0, 0, 0)
   - Mesh Filter: выберите "Cube"
   - Mesh Renderer: назначьте материал Robot/RobotMaterial.mat
   - Rigidbody: отключите Gravity, Use Gravity
   - RobotController (скрипт)

3. Создайте child объект "DirectionIndicator":
   - Mesh Filter: "Cube"
   - Scale: (0.1, 0.1, 0.5)
   - Position: (0, 0, 0.5) - выступает вперёд
   - Material: Robot/DirectionMarker.mat

4. Отберите Robot и сохраните как prefab в Prefabs/Robot/
```

## Шаг 2: Создать RobotConfig.asset

```
1. ПКМ в Prefabs → Assets/ScriptableObjects/Configs
2. Create → Robot Programming → Robot Config
3. Настройте параметры:
   - Move Distance: 1.0 (шаг в 1 юнит)
   - Move Speed: 2.0 (скорость движения)
   - Turn Angle: 90 (поворот на 90 градусов)
   - Turn Speed: 180 (угловая скорость)
   - Movement Curve: EaseInOut
   - Rotation Curve: Linear
```

## Шаг 3: Создать Canvas и UI

```
1. GameObject → UI → Canvas (обновлённый)
   - Canvas Scaler: Ref Resolution (1920x1080)

2. Добавьте Event System (если не создался автоматически)

3. В Canvas создайте структуру:
   - Panel "Palette" (слева)
     ├─ Text "Available Commands"
     └─ ScrollView → Content (для блоков)

   - Panel "ProgramArea" (в центре)
     └─ Content (вертикальная сетка для программы)

   - Panel "Controls" (снизу/справа)
     ├─ Button "Run"
     ├─ Button "Stop"
     ├─ Button "Reset"
     └─ Button "Clear"

   - Text "Status" (информация о статусе)
   - Slider "Progress" (прогресс выполнения)
```

## Шаг 4: Создать BlockUI prefab

```
1. GameObject → UI → Panel → переименуйте в "BlockUI"

2. Компоненты:
   - Image: Color (выбранный по типу команды)
   - Text: "Вперёд" (DisplayName команды)
   - CanvasGroup: для opacity при drag

3. RectTransform:
   - Width: 200, Height: 50
   - Alignment: Center
   - Pivot: Center Center

4. Добавьте скрипт BlockUI.cs

5. Сохраните как prefab в Prefabs/UI/BlockUI.prefab
```

## Шаг 5: Создать BlockPalette

```
1. Скопируйте Palette Panel из Canvas

2. Добавьте компоненты:
   - BlockPalette (скрипт)
   - BlockFactory (скрипт)
   - Назначьте BlockUI prefab в BlockFactory.blockPrefab

3. В BlockPalette инспекторе:
   - BlockFactory: ссылка на GameObject с BlockFactory
   - RobotConfig: ссылка на RobotConfig.asset
   - Palette Content: ссылка на ScrollView Content

4. Сохраните как prefab в Prefabs/UI/BlockPalette.prefab
```

## Шаг 6: Создать ProgramArea

```
1. Скопируйте ProgramArea Panel из Canvas

2. Добавьте компоненты:
   - ProgramArea (скрипт)
   - Layout Group: Vertical Layout Group
   - Canvas Scaler

3. В ProgramArea инспекторе:
   - Palette Content: ссылка на содержимое палитры
   - Canvas: ссылка на основной Canvas

4. Сохраните как prefab в Prefabs/UI/ProgramArea.prefab
```

## Шаг 7: Создать главную сцену

```
1. Scene → New Scene → Save as "GameScene.unity"

2. Иерархия объектов:
   ├─ Robot (инстанцируйте из Prefabs/Robot/Robot.prefab)
   ├─ Canvas
   │  ├─ Palette (инстанцируйте из Prefabs/UI/BlockPalette.prefab)
   │  ├─ ProgramArea (инстанцируйте из Prefabs/UI/ProgramArea.prefab)
   │  └─ Controls (кнопки, текст статуса)
   ├─ GameManager (пустой GameObject с компонентом GameManager.cs)
   ├─ CommandExecutor (пустой GameObject с компонентом CommandExecutor.cs)
   └─ Timers (уже должен быть в сцене или создайте пустой GameObject)

3. В GameManager инспекторе назначьте:
   - Robot Controller: ссылка на Robot
   - Command Executor: ссылка на CommandExecutor
   - Block Palette: ссылка на Palette
   - Program Area: ссылка на ProgramArea
   - Run Button: Button Run
   - Stop Button: Button Stop
   - Reset Button: Button Reset
   - Clear Button: Button Clear
   - Status Text: Text для статуса
   - Progress Text: Text для прогресса
```

## Шаг 8: Настройка тегов

```
1. Edit → Project Settings → Tags and Layers

2. Добавьте теги:
   - "DropZone" (для ProgramArea)
   - "Block" (для UI блоков)
   - "Robot" (для робота)

3. В инспекторе назначьте соответствующие теги
```

## Шаг 9: Тестирование

```
1. Обеспечьте наличие в сцене:
   ✅ Timers MonoBehaviour
   ✅ Robot с RobotController
   ✅ Canvas с Event System
   ✅ GameManager
   ✅ CommandExecutor
   ✅ BlockPalette с BlockFactory
   ✅ ProgramArea

2. Нажмите Play

3. Проверьте:
   □ Блоки отображаются в палитре
   □ Можно перетащить блок в ProgramArea
   □ Кнопка Run запускает программу
   □ Робот движется согласно программе
   □ Кнопки Stop/Reset работают
```

---

## Отладочные подсказки

### Если блоки не отображаются:
- Проверьте, что BlockFactory.blockPrefab назначен
- Проверьте, что BlockPalette.PopulatePalette() вызывается в Awake
- Проверьте Console на ошибки

### Если робот не движется:
- Проверьте, что RobotConfig.asset создан и назначен
- Проверьте, что Timers.Instance существует в сцене
- Проверьте, что RobotController имеет ссылку на RobotConfig

### Если блоки не привязываются:
- Проверьте наличие Canvas и Event System
- Проверьте, что BlockUI имеет RectTransform
- Проверьте теги ("DropZone")

### Если программа не выполняется:
- Проверьте, что CommandExecutor добавлен в сцену
- Проверьте, что есть хотя бы один блок в ProgramArea
- Проверьте Console на ошибки ICommand.Execute()

---

## Файлы для быстрого старта

```
✅ Обязательно создать:
├─ Assets/Scenes/GameScene.unity
├─ Assets/Prefabs/Robot/Robot.prefab
├─ Assets/Prefabs/UI/BlockUI.prefab
├─ Assets/Prefabs/UI/Canvas.prefab
├─ Assets/Prefabs/UI/BlockPalette.prefab
├─ Assets/Prefabs/UI/ProgramArea.prefab
└─ Assets/ScriptableObjects/Configs/RobotConfig.asset

📁 Уже существуют:
├─ Assets/Scripts/RobotProgramming/Core/*.cs
├─ Assets/Scripts/RobotProgramming/Commands/*.cs
├─ Assets/Scripts/RobotProgramming/Robot/*.cs
├─ Assets/Scripts/RobotProgramming/Execution/*.cs
├─ Assets/Scripts/RobotProgramming/UI/*.cs
└─ Assets/Scripts/RobotProgramming/Managers/*.cs
```

---

## Команды для консоли (если нужно создать вручную)

```csharp
// Создать Robot в коде
var robot = Instantiate(robotPrefab);
robot.GetComponent<RobotController>().MoveForward(1f);

// Создать программу в коде
var seq = new ProgramSequence();
seq.AddCommand(new MoveForwardCommand(1));
seq.AddCommand(new TurnRightCommand(2));
executor.ExecuteProgram(seq.StartCommand, robotController);

// Загрузить конфиг
var config = Resources.Load<RobotConfig>("Configs/RobotConfig");
```

---

Время настройки: **15-30 минут** для полного запуска! 🚀
