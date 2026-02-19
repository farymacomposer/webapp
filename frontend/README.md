## Frontend (Twitch OAuth)

Минимальный фронтенд для входа через Twitch OAuth и получения JWT от backend API.

### Env-переменные

Создай `.env.local` на основе `.env.example`:

```bash
NEXT_PUBLIC_API_BASE_URL=http://localhost:8080
NEXT_PUBLIC_TWITCH_CLIENT_ID=your_twitch_client_id
NEXT_PUBLIC_TWITCH_REDIRECT_URI=http://localhost:3000/auth/twitch/callback
```

### Запуск

```bash
npm install
npm run dev
```

### Что реализовано

- PKCE (`code_verifier` + `code_challenge`) на клиенте.
- Redirect на Twitch authorize endpoint.
- Callback-страница `/auth/twitch/callback`.
- Обмен `code` на JWT через `POST /api/Auth/TwitchLogin`.
- Сохранение JWT в `localStorage`.
