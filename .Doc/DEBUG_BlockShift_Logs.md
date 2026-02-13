# Отладка сдвига блоков через логирование

**Статус:** ✓ Логирование переписано для отладки сдвига
**Дата:** 2026-01-14

---

## Что изменилось в логировании

### Удалено (лишние логи)
```
[SNAP PRIORITY] Using/Switching logs
[SNAP CHOICE] логи
[SNAP APPLIED] логи
[SNAP READY] логи
[RETURN TO PROGRAM] логи
[CONNECTION INPUT→OUTPUT] / [CONNECTION OUTPUT→INPUT] полные логи
```

### Добавлено (для отладки сдвига)
```
[SHIFT BlockName] offset: (X, Y)       ← сдвиг конкретного блока
[CONNECT] A.output → B.input           ← создание соединения
[INSERT MIDDLE] A → C → B              ← вставка в середину
[INSERT START] C → A                   ← вставка в начало
```

---

## Как использовать для отладки

### Сценарий 1: Вставка в СЕРЕДИНУ

**Ожидаемые логи:**
```
[INSERT MIDDLE] Move_Forward → Turn_Right → Turn_Left
[SHIFT Turn_Right] offset: (0.0, -50.5)
[SHIFT Turn_Left] offset: (0.0, -50.5)
[CONNECT] Turn_Right.output → Turn_Left.input
```

**Что это означает:**
- Turn_Right вставляется между Move_Forward и Turn_Left
- Turn_Right сдвигается на (0, -50.5) чтобы его INPUT совпал с выходом Move_Forward
- Turn_Left сдвигается на (0, -50.5) чтобы его INPUT совпал с выходом Turn_Right
- После этого создается соединение

**Если логов нет:**
- [INSERT MIDDLE] должен быть → проверь что `previousOutput != null`
- [SHIFT] должны быть → проверь что `offsetForC` и `offsetForB` считаются правильно

### Сценарий 2: Вставка в НАЧАЛО

**Ожидаемые логи:**
```
[INSERT START] Turn_Right → Move_Forward
[SHIFT Turn_Right] offset: (0.0, -25.3)
[CONNECT] Turn_Right.output → Move_Forward.input
```

**Что это означает:**
- Turn_Right вставляется в начало перед Move_Forward
- Turn_Right сдвигается на (0, -25.3) чтобы его OUTPUT совпал с INPUT Move_Forward
- После этого создается соединение

### Сценарий 3: Простой INPUT→OUTPUT снап

**Ожидаемые логи:**
```
[SHIFT Turn_Right] offset: (0.0, -40.2)
[CONNECT] Move_Forward.output → Turn_Right.input
```

**Что это означает:**
- Turn_Right сдвигается на (0, -40.2) чтобы его INPUT совпал с OUTPUT Move_Forward
- Создается соединение

---

## Отладка: Если сдвиг не работает

### Проверка 1: Вообще ли вызывается сдвиг?

```
Смотри в Console:
- Есть ли логи [SHIFT ...]?
- Если нет → снап не применяется, проверь ApplySnap/ApplySnapToInput
- Если да → переходи на проверку 2
```

### Проверка 2: Значение смещения правильно?

```
Логи типа:
[SHIFT Turn_Right] offset: (0.0, -50.5)

Если offset очень большой (например -300):
  → Может быть что GetWorldPosition() возвращает неправильное значение

Если offset (0, 0):
  → Позиции одинаковые, смещение не требуется (может быть ошибка в позициях)
```

### Проверка 3: Сдвиг в правильном направлении?

```
Если блоки наслаиваются:
  → offset.y может быть с неправильным знаком
  → Проверь вычисление: offsetForB = cOutputPos - bInputPos
  → Знак должен сдвинуть блок ВНИЗ если c выше чем b

Если блоки сдвигаются в стороны неправильно:
  → offset.x может быть неправильный
```

---

## Все логи при нормальном потоке

### При вставке нового блока из палитры в конец цепи

```
Console должен показать:

[INSERT MIDDLE] Move_Forward → Turn_Right → Turn_Left

[SHIFT Turn_Right] offset: (0.0, -51.2)
[SHIFT Turn_Left] offset: (0.0, -51.2)

[CONNECT] Turn_Right.output → Turn_Left.input
```

### При перетаскивании существующего блока в середину

```
[INSERT MIDDLE] Move_Forward → Move_Backward → Turn_Left

[SHIFT Move_Backward] offset: (0.0, -45.0)
[SHIFT Turn_Left] offset: (0.0, -45.0)

[CONNECT] Move_Backward.output → Turn_Left.input
```

### При перетаскивании в начало

```
[INSERT START] Turn_Right → Move_Forward

[SHIFT Turn_Right] offset: (0.0, -30.0)

[CONNECT] Turn_Right.output → Move_Forward.input
```

---

## Фильтрирование логов

В Console можно фильтровать по:
- `[SHIFT` - все логи сдвига
- `[INSERT` - все логи вставки (начало или середина)
- `[CONNECT` - все логи соединений

Используй это для отладки конкретной проблемы.

---

## Типичные проблемы и их логи

| Проблема | Логи которые есть | Логи которых нет |
|----------|------------------|-----------------|
| Сдвиг не происходит | [INSERT MIDDLE/START] | [SHIFT ...] |
| Сдвиг неправильный | [SHIFT] с неправильным offset | (логи есть но позиции неправильные) |
| Соединение не создается | [SHIFT ...] | [CONNECT] |
| Cascade не работает | все логи | (но блоки не выравниваются дальше) |

---

## Как читать логи для отладки сдвига

### Пример проблемного случая

```
Console показывает:
[INSERT MIDDLE] Move_Forward → Turn_Right → Turn_Left
[SHIFT Turn_Right] offset: (0.0, 0.0)      ← ПРОБЛЕМА! offset (0, 0)
[SHIFT Turn_Left] offset: (0.0, 0.0)
[CONNECT] Turn_Right.output → Turn_Left.input
```

**Анализ:**
- offset (0, 0) означает что позиции INPUT и OUTPUT совпали
- Либо GetWorldPosition() возвращает одну и ту же позицию для обоих коннекторов
- Либо коннекторы не инициализированы

**Решение:**
- Проверь что BlockConnector.GetWorldPosition() работает правильно
- Убедись что INPUT и OUTPUT имеют разные позиции

---

## Быстрая проверка сдвига

1. Откройте Console (Ctrl+Shift+C)
2. Перетащите блок в цепь
3. Смотрите логи:
   - `[SHIFT ...]` должны быть
   - Значения offset НЕ должны быть (0, 0)
   - `[CONNECT]` должен быть после SHIFTs

Если логов нет или они неправильные - есть баг в FindConnectedOutput или вычислениях смещения.

---

**Готово к отладке!** 🔍
