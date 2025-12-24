# Задача #1: Базовая инфраструктура

## Цель
Создать структуру папок и базовые классы данных для системы команд. Проект должен компилироваться без ошибок.

## Контекст
Это базовая задача, от которой зависят все остальные. Нужно создать основу для всей архитектуры.

## Ключевые шаги
1. Создать папку структуру `Assets/Scripts/RobotProgramming/` с подпапками:
   - Core/
   - Commands/
   - Robot/
   - Execution/
   - UI/
   - Data/
   - Managers/

2. Создать `Data/CommandType.cs` - enum со всеми типами команд:
   ```csharp
   public enum CommandType
   {
       MoveForward,
       MoveBackward,
       TurnLeft,
       TurnRight,
       Wait
   }
   ```

3. Создать `Data/BlockData.cs` - сериализуемые данные блока:
   ```csharp
   [System.Serializable]
   public class BlockData
   {
       public int id;
       public CommandType commandType;
       public float[] parameters;
       public int nextBlockId;
   }
   ```

4. Создать `Data/ProgramData.cs` - сериализуемая программа:
   ```csharp
   [System.Serializable]
   public class ProgramData
   {
       public List<BlockData> blocks;
       public int startBlockId;
       public string programName;
       public System.DateTime createdDate;
   }
   ```

5. Проверить, что проект компилируется без ошибок

## Критерии завершения
- [ ] Папки созданы в правильной структуре
- [ ] CommandType.cs создан и содержит все типы команд
- [ ] BlockData.cs создан с полями id, commandType, parameters, nextBlockId
- [ ] ProgramData.cs создан со списком блоков и startBlockId
- [ ] Проект компилируется (Console чистая)
- [ ] Файлы находятся в папке RobotProgramming namespace

## Пример структуры
```
Assets/Scripts/RobotProgramming/
├── Core/
├── Commands/
├── Robot/
├── Execution/
├── UI/
├── Data/
│   ├── CommandType.cs
│   ├── BlockData.cs
│   └── ProgramData.cs
└── Managers/
```

## Заметки
- Используйте namespace `RobotProgramming.Data`
- Все enum/data классы должны быть сериализуемы для будущего save/load
- Не создавайте .meta файлы вручную - Unity создаст автоматически
