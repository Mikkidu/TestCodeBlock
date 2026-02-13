# Quick Start — UPM Package (Hybrid Approach)

## 📦 Что готово

### ✅ package.json
- Версия: 1.0.0
- Правильные ссылки на github.com/mikkiducher/TestCodeBlock
- Зависимости: TextMeshPro, UGUI

### ✅ Документация
- `README.md` — Quick Start и API reference
- `MIGRATION_GUIDE_HYBRID.md` — Пошаговая инструкция миграции
- `PRIVATE_REPO_GUIDE.md` — Как работать с приватным репо
- `CHANGELOG.md` — История версий

### ✅ Структура
```
Packages/com.codeblocks.robotprogramming/
├── Runtime/ (папки готовы)
├── Editor/ (папки готовы)
└── Resources/ (удалены, будут в Assets)
```

---

## 🎯 Гибридный подход

```
Packages/com.codeblocks.robotprogramming/
└── Только .cs файлы (скрипты)
    → Автообновления через UPM

Assets/CodeBlocks/
└── Префабы, уровни, конфиги
    → Видимы команде, легко найти
```

---

## 🚀 Следующие шаги (выполняй в Unity)

### 1. Создать Assets/CodeBlocks/

В Unity Project window:
```
Assets → Create → Folder → "CodeBlocks"

Внутри создать:
- Prefabs/UI/
- Prefabs/LevelEditor/Terrain/
- Prefabs/LevelEditor/Objects/
- Resources/Levels/
- Resources/Configs/
```

### 1.5. Переименовать namespace (РЕКОМЕНДУЕТСЯ)

**Зачем:** Соответствие названию пакета, избежать breaking changes в будущем

```
RobotProgramming.* → CodeBlocks.*
LevelEditor → CodeBlocks.LevelEditor
```

**⚠️ Promises НЕ трогать!** (остаётся `namespace Promises`)

**Инструкция:** `NAMESPACE_RENAME_GUIDE.md` (5-10 минут)

### 2. Перенести скрипты → Packages/

**⚠️ Через Unity drag-drop для сохранения .meta!**

| Откуда | Куда |
|--------|------|
| `Assets/Scripts/RobotProgramming/*` | `Packages/.../Runtime/...` |
| `Assets/Scripts/LevelEditor/*` (runtime) | `Packages/.../Runtime/LevelEditor/` |
| `Assets/Scripts/LevelEditor/Editor/*` | `Packages/.../Editor/LevelEditor/` |

**⚠️ ВАЖНО: `Assets/Scripts/Promises/` — НЕ переносить!**
- Promises остаётся в Assets/Scripts/Promises/
- Это внешняя зависимость (уже есть в основном проекте)
- CodeBlocks.Runtime.asmdef ссылается на "Promises" assembly

Подробная таблица: см. `MIGRATION_GUIDE_HYBRID.md`

### 3. Перенести ассеты → Assets/CodeBlocks/

| Откуда | Куда |
|--------|------|
| `Assets/PrefabsUI/*` | `Assets/CodeBlocks/Prefabs/UI/` |
| `Assets/Resources/CodeBlocks/*` | `Assets/CodeBlocks/Prefabs/LevelEditor/` |
| `Assets/Resources/RobotLevels/*` | `Assets/CodeBlocks/Resources/Levels/` |

### 4. Обновить Resources.Load

Поиск:
```
Find → Find in Files
Искать: "Resources.Load"
```

Изменить:
```csharp
// Было
Resources.Load<LevelGridData>("RobotLevels/tutorial_01")

// Стало
Resources.Load<LevelGridData>("Levels/tutorial_01")
```

### 5. Тестирование

- [ ] Компиляция без ошибок
- [ ] Открыть сцену
- [ ] Drag-drop блоков работает
- [ ] Level Editor открывается
- [ ] Запуск программы работает

### 6. Git коммит

```bash
cd "D:\Projects\TestCodeBlock"

git add Packages/com.codeblocks.robotprogramming/
git add Assets/CodeBlocks/

git commit -m "feat: convert to UPM package (hybrid structure)"
git tag v1.0.0
git push origin master --tags
```

---

## 📝 Использование в другом проекте

### Установка пакета

1. **Package Manager → Add package from git URL**

   **HTTPS:**
   ```
   https://github.com/mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.2
   ```

   **SSH:**
   ```
   git@github.com:mikkiducher/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.2
   ```

2. **Скопировать ассеты** (один из вариантов):

   **Вариант A: Export/Import**
   ```
   В TestCodeBlock:
   Assets/CodeBlocks → Export Package → CodeBlocks_Assets.unitypackage

   В другом проекте:
   Assets → Import Package → Custom Package → CodeBlocks_Assets.unitypackage
   ```

   **Вариант B: Ручное копирование**
   ```
   Скопировать папку Assets/CodeBlocks/ целиком
   ```

### Обновление

**Скрипты** (автоматически):
```
Package Manager → Check for updates
```

**Ассеты** (вручную):
```
Export/Import заново
```

---

## ✅ Checklist готовности

- [x] package.json с правильными ссылками
- [x] README.md создан
- [x] MIGRATION_GUIDE_HYBRID.md готов
- [x] Структура Packages/ создана
- [ ] Скрипты перенесены в Packages/
- [ ] Ассеты перенесены в Assets/CodeBlocks/
- [ ] Resources.Load пути обновлены
- [ ] Тестирование пройдено
- [ ] Git tag v1.0.0 создан
- [ ] Тестовая интеграция в другом проекте

---

## 🆘 Если что-то пошло не так

### Компиляция не проходит
→ Проверь что все .cs файлы перенесены
→ Проверь что assembly definitions на месте

### Prefabs сломались
→ Переносил через Unity? (не через Проводник!)
→ .meta файлы должны быть на месте

### Resources.Load не находит файлы
→ Проверь пути (должны быть относительно Resources/)
→ Проверь что файлы в Assets/CodeBlocks/Resources/

### Пакет не виден в другом проекте
→ Git tag создан? (`git tag -l`)
→ Git push с тегами? (`git push --tags`)
→ URL правильный с `#v1.0.0` в конце?

---

## 📚 Полная документация

- `README.md` — Quick Start, API
- `MIGRATION_GUIDE_HYBRID.md` — Детальная инструкция миграции
- `PRIVATE_REPO_GUIDE.md` — Приватные репо, SSH, токены
- `CHANGELOG.md` — История версий

---

## 🎉 Готово!

После миграции у вас будет:
- ✅ UPM пакет с автообновлениями
- ✅ Ассеты в Assets/ (видимы команде)
- ✅ Версионирование через git tags
- ✅ Простая интеграция в другие проекты
