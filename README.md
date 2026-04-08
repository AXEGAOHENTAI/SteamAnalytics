# SteamAnalytics (Starter Template)

Минималистичный шаблон Windows-приложения на C# (WPF + MVVM) для аналитики Steam.

## Что уже реализовано

- WPF UI с вкладками **Игры / Друзья / Инвентарь**.
- Проверка профиля по Steam ID/URL.
- Реальные HTTP-вызовы Steam API:
  - `ISteamUser/GetPlayerSummaries`
  - `IPlayerService/GetOwnedGames`
  - `ISteamUser/GetPlayerBans`
  - `ISteamUser/GetFriendList`
  - `ISteamUser/ResolveVanityURL`
- Загрузка публичного инвентаря через endpoint Steam Community inventory.
- Кнопка входа через Steam OpenID (формирование URL и запуск браузера).

## Важно перед запуском

Приложению нужен ключ Web API в переменной окружения:

```powershell
setx STEAM_API_KEY "ваш_steam_api_key"
```

После `setx` перезапустите Visual Studio/терминал.

## Как запустить (Windows)

1. Установить .NET 8 SDK и Visual Studio 2022 (Desktop development with .NET).
2. Открыть `SteamAnalytics.sln`.
3. Запустить проект `SteamAnalytics.App`.
4. Ввести один из форматов:
   - `7656119...` (steamid64)
   - `https://steamcommunity.com/profiles/...`
   - `https://steamcommunity.com/id/...`

## Ограничения

- Если профиль приватный, часть данных (игры/друзья/инвентарь) может быть недоступна.
- Инвентарь загружается для `appId=730, contextId=2` (CS2) как пример.
- Для production-уровня нужно добавить retry, логирование и кэш.
