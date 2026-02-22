import { authorizedFetch, createApiConfiguration } from "@/lib/api/client";
import { getAuthToken } from "@/lib/auth/storage";

export async function getProtectedAppSettings(): Promise<{
  data: unknown;
  authorizationHeaderPreview: string;
}> {
  const api = createApiConfiguration();

  if (!api.authorizationHeader) {
    throw new Error("JWT не найден. Выполните вход через Twitch.");
  }

  const response = await authorizedFetch("/api/AppSettings/GetAppSettings", {
    method: "GET",
  });

  if (!response.ok) {
    throw new Error(`Protected API request failed with status ${response.status}`);
  }

  const data = (await response.json()) as unknown;
  const latestToken = getAuthToken();
  const latestAuthorizationHeader = latestToken ? `Bearer ${latestToken}` : api.authorizationHeader;
  const tokenPreview = latestAuthorizationHeader.length > 28
    ? `${latestAuthorizationHeader.slice(0, 20)}...`
    : latestAuthorizationHeader;

  return {
    data,
    authorizationHeaderPreview: tokenPreview,
  };
}
