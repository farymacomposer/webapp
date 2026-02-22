export type TwitchLoginResponse = {
  token: string;
  refreshToken: string;
};

type TwitchLoginStateResponse = {
  state: string;
};

export async function getTwitchLoginState(): Promise<string> {
  const response = await fetch("/api/Auth/TwitchLoginState", {
    method: "GET",
    headers: {
      Accept: "application/json",
    },
  });

  if (!response.ok) {
    throw new Error(`State request failed with status ${response.status}`);
  }

  const payload = (await response.json()) as TwitchLoginStateResponse;
  if (!payload.state) {
    throw new Error("State response is invalid");
  }

  return payload.state;
}

export async function exchangeTwitchCodeForJwt(code: string, codeVerifier: string, state: string): Promise<TwitchLoginResponse> {
  const response = await fetch("/api/Auth/TwitchLogin", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      code,
      codeVerifier,
      state,
    }),
  });

  if (!response.ok) {
    throw new Error(`Auth request failed with status ${response.status}`);
  }

  return (await response.json()) as TwitchLoginResponse;
}

export async function refreshAccessToken(refreshToken: string): Promise<TwitchLoginResponse> {
  const response = await fetch("/api/Auth/Refresh", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      refreshToken,
    }),
  });

  if (!response.ok) {
    throw new Error(`Refresh request failed with status ${response.status}`);
  }

  return (await response.json()) as TwitchLoginResponse;
}

export async function logoutSession(refreshToken: string, accessToken: string): Promise<void> {
  await fetch("/api/Auth/Logout", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${accessToken}`,
    },
    body: JSON.stringify({
      refreshToken,
    }),
  });
}

export async function logoutAllSessions(accessToken: string): Promise<void> {
  await fetch("/api/Auth/LogoutAll", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  });
}
