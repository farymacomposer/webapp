"use client";

import { Suspense, useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { exchangeTwitchCodeForJwt } from "@/lib/api/auth";
import { clearTwitchAuthArtifacts, getPkceVerifier, getTwitchAuthState, setAuthToken } from "@/lib/auth/storage";

function TwitchCallbackContent() {
  const router = useRouter();
  const params = useSearchParams();
  const [message, setMessage] = useState("Обрабатываем авторизацию...");

  useEffect(() => {
    const completeLogin = async () => {
      const code = params.get("code");
      const state = params.get("state");
      const error = params.get("error");

      if (error) {
        clearTwitchAuthArtifacts();
        setMessage(`Twitch вернул ошибку: ${error}`);
        return;
      }

      const expectedState = getTwitchAuthState();
      const codeVerifier = getPkceVerifier();

      if (!code || !state || !codeVerifier || !expectedState || state !== expectedState) {
        clearTwitchAuthArtifacts();
        setMessage("Некорректный callback Twitch OAuth. Попробуйте войти заново.");
        return;
      }

      try {
        const token = await exchangeTwitchCodeForJwt(code, codeVerifier);
        setAuthToken(token);
        setMessage("Успешный вход. Перенаправляем...");
        router.replace("/");
      } catch {
        clearTwitchAuthArtifacts();
        setMessage("Не удалось завершить вход через Twitch.");
      }
    };

    void completeLogin();
  }, [params, router]);

  return (
    <section className="card">
      <h1>Twitch OAuth</h1>
      <p className="muted">{message}</p>
    </section>
  );
}

export default function TwitchCallbackPage() {
  return (
    <Suspense
      fallback={
        <section className="card">
          <h1>Twitch OAuth</h1>
          <p className="muted">Обрабатываем авторизацию...</p>
        </section>
      }
    >
      <TwitchCallbackContent />
    </Suspense>
  );
}
