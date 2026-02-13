# Задача #7: UI - ProgramArea и BlockPalette

## Цель
Реализовать рабочую область программирования и палитру доступных блоков с логикой snap-to-block.

## Контекст
Зависит от: Задача #5, #6
Это завершение основного UI для визуального программирования.

## Ключевые шаги
1. Создать `UI/ProgramArea.cs`:
   - Реализовать IDropHandler
   - List<BlockUI> programBlocks
   - ProgramSequence programSequence
   - OnDrop обработка: добавление блока в программу
   - FindSnapTarget - поиск блока для привязки
   - SnapBlocks - связывание двух блоков через command.Next
   - GetProgramSequence() для запуска
   - Clear() для сброса

2. Создать `UI/BlockPalette.cs`:
   - BlockFactory blockFactory (SerializeField)
   - Transform paletteContent
   - CreatePaletteBlocks() - создание всех типов блоков

3. Настроить UI Layout в сцене:
   - Левая панель: BlockPalette с доступными блоками
   - Центральная область: ProgramArea для собранной программы
   - Scroll View для обоих

4. Тестировать сборку программы из 5-6 блоков:
   - Drag block из palette
   - Drop в program area
   - Проверить snap-to-block привязку
   - Проверить command.Next связь

## Критерии завершения
- [ ] ProgramArea создана и реализует IDropHandler
- [ ] snap-to-block расстояние настроено (50 pixels)
- [ ] Блоки привязываются друг к другу правильно
- [ ] command.Next устанавливается при snap
- [ ] BlockPalette показывает все типы команд
- [ ] Можно собрать программу из 6+ блоков
- [ ] Блоки визуально выравниваются при snap
- [ ] GetProgramSequence возвращает правильную последовательность
