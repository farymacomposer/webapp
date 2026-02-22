"use client";

import { useMemo, useState } from "react";
import { logoutAllSessions, logoutSession } from "@/lib/api/auth";
import { startTwitchLogin } from "@/lib/auth/twitch";
import { clearAuthToken, getAuthToken, getRefreshToken } from "@/lib/auth/storage";
import { getProtectedAppSettings } from "@/lib/api/app-settings";

export default function HomePage() {
  const token = useMemo(() => getAuthToken(), []);
  const [error, setError] = useState<string | null>(null);
  const [apiResult, setApiResult] = useState<string | null>(null);

  return (
    <section className="card">
      <h1>Faryma Composer</h1>
      <p className="muted">Вход пользователей выполняется через Twitch OAuth.</p>
      {error ? <p className="error">{error}</p> : null}

      {token ? (
        <>
          <p className="ok">JWT получен и сохранен.</p>
          {apiResult ? <pre className="result">{apiResult}</pre> : null}
          <button
            type="button"
            onClick={async () => {
              const accessToken = getAuthToken();
              const refreshToken = getRefreshToken();

              try {
                if (accessToken && refreshToken) {
                  await logoutSession(refreshToken, accessToken);
                }
              } finally {
                clearAuthToken();
                window.location.reload();
              }
            }}
          >
            Выйти
          </button>
          <button
            type="button"
            onClick={async () => {
              const accessToken = getAuthToken();
              if (!accessToken) {
                clearAuthToken();
                window.location.reload();
                return;
              }

              try {
                await logoutAllSessions(accessToken);
              } finally {
                clearAuthToken();
                window.location.reload();
              }
            }}
          >
            Выйти со всех устройств
          </button>
          <button
            type="button"
            onClick={async () => {
              try {
                setError(null);
                const result = await getProtectedAppSettings();
                setApiResult(
                  [
                    `Authorization: ${result.authorizationHeaderPreview}`,
                    "Response:",
                    JSON.stringify(result.data, null, 2),
                  ].join("\n"),
                );
              } catch (requestError) {
                const message = requestError instanceof Error ? requestError.message : "Ошибка запроса";
                setError(message);
              }
            }}
          >
            Проверить защищенный API
          </button>
        </>
      ) : (
        <button
          type="button"
          className="button-link"
          onClick={async () => {
            try {
              setError(null);
              setApiResult(null);
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