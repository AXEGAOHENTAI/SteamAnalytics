# SteamAnalytics (Starter Template)

Минималистичный стартовый шаблон Windows-приложения на C# (WPF + MVVM) для аналитики Steam.

## Что внутри

- WPF UI (минималистичный макет)
- MVVM-структура
- Резолвер Steam ID/URL в `steamid64`
- Заготовка OpenID-авторизации через Steam (через браузер)
- Мок-сервис проверки профиля и банов (для быстрого старта UI)

## Структура

```text
src/
  SteamAnalytics.App/
    Models/
    Services/
    Utils/
    ViewModels/
    Views/
```

## Как запустить (на Windows)

1. Установить **.NET 8 SDK** и Visual Studio 2022 (Desktop development with .NET).
2. Открыть `SteamAnalytics.sln`.
3. Запустить проект `SteamAnalytics.App`.

## Следующие шаги

1. Заменить мок-данные в `SteamProfileService` на реальные вызовы Steam Web API.
2. Добавить callback endpoint для OpenID (например, через локальный HTTP listener).
3. Добавить SQLite-кэш и обработку rate-limit.
