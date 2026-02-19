"use client";

import { useMemo, useState } from "react";
import { startTwitchLogin } from "@/lib/auth/twitch";
import { clearAuthToken, getAuthToken } from "@/lib/auth/storage";

export default function HomePage() {
  const token = useMemo(() => getAuthToken(), []);
  const [error, setError] = useState<string | null>(null);

  return (
    <section className="card">
      <h1>Faryma Composer</h1>
      <p className="muted">Вход пользователей выполняется через Twitch OAuth.</p>
      {error ? <p className="error">{error}</p> : null}

      {token ? (
        <>
          <p className="ok">JWT получен и сохранен.</p>
          <button
            type="button"
            onClick={() => {
              clearAuthToken();
              window.location.reload();
            }}
          >
            Выйти
          </button>
        </>
      ) : (
        <button
          type="button"
          className="button-link"
          onClick={async () => {
            try {
              setError(null);
              await startTwitchLogin();
            } catch (loginError) {
              const message = loginError instanceof Error ? loginError.message : "Не удалось начать авторизацию";
              setError(message);
            }
          }}
        >
          Войти через Twitch
        </button>
      )}
    </section>
  );
}