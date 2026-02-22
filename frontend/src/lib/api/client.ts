import { refreshAccessToken } from "@/lib/api/auth";
import { clearAuthToken, getAuthToken, getRefreshToken, setAuthSession } from "@/lib/auth/storage";

export type ApiConfiguration = {
  authorizationHeader: string | null;
};

export type AuthorizedRequestInit = Omit<RequestInit, "headers"> & {
  headers?: HeadersInit;
};

export function createApiConfiguration(): ApiConfiguration {
  const token = getAuthToken();

  return {
    authorizationHeader: token ? `Bearer ${token}` : null,
  };
}

function buildHeaders(initHeaders: HeadersInit | undefined, authorizationHeader: string): Headers {
  const headers = new Headers(initHeaders);
  headers.set("Authorization", authorizationHeader);

  return headers;
}

export async function authorizedFetch(input: string, init: AuthorizedRequestInit = {}): Promise<Response> {
  const api = createApiConfiguration();
  if (!api.authorizationHeader) {
    throw new Error("JWT не найден. Выполните вход через Twitch.");
  }

  let response = await fetch(input, {
    ...init,
    headers: buildHeaders(init.headers, api.authorizationHeader),
  });

  if (response.status !== 401) {
    return response;
  }

  const refreshToken = getRefreshToken();
  if (!refreshToken) {
    clearAuthToken();
    return response;
  }

  try {
    const refreshedTokens = await refreshAccessToken(refreshToken);
    setAuthSession(refreshedTokens.token, refreshedTokens.refreshToken);
  } catch {
    clearAuthToken();
    return response;
  }

  const nextAccessToken = getAuthToken();
  if (!nextAccessToken) {
    clearAuthToken();
    return response;
  }

  response = await fetch(input, {
    ...init,
    headers: buildHeaders(init.headers, `Bearer ${nextAccessToken}`),
  });

  return response;
}
