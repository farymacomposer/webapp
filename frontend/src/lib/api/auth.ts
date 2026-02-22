export type TwitchLoginResponse = {
  token: string;
};

type TwitchLoginStateResponse = {
  state: string;
};

function getApiBaseUrl(): string {
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

  if (!baseUrl) {
    throw new Error("Missing NEXT_PUBLIC_API_BASE_URL");
  }

  return baseUrl;
}

export async function getTwitchLoginState(): Promise<string> {
  const baseUrl = getApiBaseUrl();
  const response = await fetch(`${baseUrl}/api/Auth/TwitchLoginState`, {
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

export async function exchangeTwitchCodeForJwt(code: string, codeVerifier: string, state: string): Promise<string> {
  const baseUrl = getApiBaseUrl();
  const response = await fetch(`${baseUrl}/api/Auth/TwitchLogin`, {
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

  const payload = (await response.json()) as TwitchLoginResponse;
  return payload.token;
}
