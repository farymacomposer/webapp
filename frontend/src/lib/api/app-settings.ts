import { createApiConfiguration } from "@/lib/api/client";

export async function getProtectedAppSettings(): Promise<{
  data: unknown;
  authorizationHeaderPreview: string;
}> {
  const api = createApiConfiguration();

  if (!api.authorizationHeader) {
    throw new Error("JWT не найден. Выполните вход через Twitch.");
  }

  const response = await fetch(`${api.baseUrl}/api/AppSettings/GetAppSettings`, {
    method: "GET",
    headers: {
      Authorization: api.authorizationHeader,
    },
  });

  if (!response.ok) {
    throw new Error(`Protected API request failed with status ${response.status}`);
  }

  const data = (await response.json()) as unknown;
  const tokenPreview = api.authorizationHeader.length > 28
    ? `${api.authorizationHeader.slice(0, 20)}...`
    : api.authorizationHeader;

  return {
    data,
    authorizationHeaderPreview: tokenPreview,
  };
}
