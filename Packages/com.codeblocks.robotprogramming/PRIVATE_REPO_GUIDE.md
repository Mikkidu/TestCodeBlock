# Использование приватного Git репозитория с UPM пакетом

## ✅ Да, репозиторий может оставаться ПРИВАТНЫМ!

UPM полностью поддерживает приватные репозитории на GitHub, GitLab, Gitea, Bitbucket и других сервисах.

---

## Как это работает

### GitHub Private Repository

**Вариант 1: SSH ключ (рекомендуется)**

```
Требования:
- SSH ключ добавлен в GitHub аккаунт
- Локально настроен SSH для git
- Пакет загружается через SSH URL
```

Git URL для пакета:
```
git@github.com:YOUR_USERNAME/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.0
```

**Вариант 2: Personal Access Token (GitHub)**

```
1. Создай token: Settings → Developer settings → Personal access tokens
   - Выбери scope: repo (для приватных repo)
   - Скопируй token

2. Используй в git URL:
   https://YOUR_TOKEN@github.com/YOUR_USERNAME/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming

3. Или добавь в manifest.json через Credential:
   (см. ниже)
```

**Вариант 3: .netrc файл (скрытый)**

```bash
# Windows: C:\Users\YOUR_USER\.netrc
# Unix: ~/.netrc

machine github.com
login YOUR_USERNAME
password YOUR_TOKEN
```

После этого обычный HTTPS URL работает автоматически.

---

## Выдача доступа другим людям

### GitHub

1. **Пригласить в Organization** (рекомендуется)
   - Settings → Members → Invite someone
   - Выбрать уровень доступа (Maintainer, Developer, Reader)

2. **Пригласить в Collaborators** (для одного репо)
   - Repository → Settings → Collaborators
   - Добавить нужного человека

3. **Team + Repository**
   - Organizations → Teams → Create new team
   - Добавить людей в team
   - Дать team доступ к репо

### GitLab

1. Settings → Members → Invite members
2. Выбрать роль (Developer, Maintainer, etc.)
3. Человек получит инвайт по почте

### Bitbucket

1. Repository settings → Users and groups
2. Add user/group с выбранными правами

---

## Безопасность: Кто может скачивать пакет?

| Способ | Кто имеет доступ | Безопасность |
|--------|-----------------|-------------|
| **SSH ключ** | Люди с добавленным ключом в GitHub | ✅ Высокая (по умолчанию) |
| **Personal Token** | Люди с token'ом | ⚠️ Средняя (token может быть скомпрометирован) |
| **OAuth** | Авторизованные пользователи | ✅ Высокая |
| **Organization Team** | Все члены team с доступом к репо | ✅ Высокая |

---

## Полная инструкция для команды

### Шаг 1: Создание SSH ключа (если ещё нет)

```bash
# На каждом компьютере (один раз)
ssh-keygen -t ed25519 -C "your_email@example.com"
# Сохрани ключ, выбрав путь и пароль
```

### Шаг 2: Добавить SSH ключ в GitHub

```
GitHub Settings → SSH and GPG keys → New SSH key
Вставь содержимое ~/.ssh/id_ed25519.pub
```

### Шаг 3: Пригласить в репозиторий

```
Repository → Settings → Collaborators → Add people
Выбери нужных людей и роль
```

### Шаг 4: Каждый человек может установить пакет

```
В Package Manager (их проект):
Add package from git URL

https://github.com/YOUR_USERNAME/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming#v1.0.0
```

---

## Проверка доступа

```bash
# Проверь работает ли SSH
ssh -T git@github.com

# Если ошибка - нужно настроить SSH ключ
# Если успех - пакет будет скачиваться автоматически
```

---

## Альтернатива: Публичный репозиторий + Лицензия

Если не хочешь управлять приватностью:

```
1. Сделай репозиторий публичным
2. Добавь LICENSE файл (MIT, Apache 2.0, etc.)
3. Люди смогут использовать без приглашений
4. Контролируй использование через лицензию
```

---

## Практический пример для вашей команды

```bash
# 1. У тебя есть приватный TestCodeBlock репо

# 2. Пригласить коллегу в GitHub
# GitHub → Repo Settings → Collaborators → Add

# 3. У коллеги есть SSH ключ в GitHub

# 4. Коллега открывает Package Manager в Unity и добавляет:
https://github.com/YOUR_USERNAME/TestCodeBlock.git?path=Packages/com.codeblocks.robotprogramming

# 5. Unity скачивает пакет автоматически через SSH (если ключ настроен)

# Готово! Пакет работает как если бы был локальный.
```

---

## Когда обновляется пакет?

```bash
# В твоем проекте (TestCodeBlock):
git tag v1.1.0
git push origin main --tags

# В проекте коллеги (автоматически):
Package Manager → мигает индикатор обновления
Или: Window → Package Manager → проверить обновления
```

---

## Резюме

✅ **Приватный репозиторий — ОК**
- SSH ключ — лучший вариант
- Personal Token — если SSH не подходит
- Лицензия контролирует использование

✅ **Выдача доступа — просто**
- Один клик в GitHub Settings
- Человек получит инвайт

✅ **Никакой верификации не нужно**
- UPM работает как с приватными так и публичными репо
- Нет специальных регистраций для пакетов
- Кто может читать репо — может установить пакет
