export type TwitchLoginResponse = {
  token: string;
};

export async function exchangeTwitchCodeForJwt(code: string, codeVerifier: string): Promise<string> {
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;

  if (!baseUrl) {
    throw new Error("Missing NEXT_PUBLIC_API_BASE_URL");
  }

  const response = await fetch(`${baseUrl}/api/Auth/TwitchLogin`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      code,
      codeVerifier,
    }),
  });

  if (!response.ok) {
    throw new Error(`Auth request failed with status ${response.status}`);
  }

  const payload = (await response.json()) as TwitchLoginResponse;
  return payload.token;
}
