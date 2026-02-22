const AUTH_TOKEN_KEY = "faryma.auth.jwt";
const REFRESH_TOKEN_KEY = "faryma.auth.refresh";
const TWITCH_PKCE_VERIFIER_KEY = "faryma.auth.twitch.pkce.verifier";
const TWITCH_STATE_KEY = "faryma.auth.twitch.state";

export function clearTwitchAuthArtifacts(): void {
  if (typeof window === "undefined") {
    return;
  }

  sessionStorage.removeItem(TWITCH_PKCE_VERIFIER_KEY);
  sessionStorage.removeItem(TWITCH_STATE_KEY);
}

export function getAuthToken(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return localStorage.getItem(AUTH_TOKEN_KEY);
}

export function setAuthToken(token: string): void {
  if (typeof window === "undefined") {
    return;
  }

  localStorage.setItem(AUTH_TOKEN_KEY, token);
  clearTwitchAuthArtifacts();
}

export function getRefreshToken(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function setAuthSession(token: string, refreshToken: string): void {
  if (typeof window === "undefined") {
    return;
  }

  localStorage.setItem(AUTH_TOKEN_KEY, token);
  localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  clearTwitchAuthArtifacts();
}

export function clearAuthToken(): void {
  if (typeof window === "undefined") {
    return;
  }

  localStorage.removeItem(AUTH_TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function setPkceVerifier(verifier: string): void {
  if (typeof window === "undefined") {
    return;
  }

  sessionStorage.setItem(TWITCH_PKCE_VERIFIER_KEY, verifier);
}

export function getPkceVerifier(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return sessionStorage.getItem(TWITCH_PKCE_VERIFIER_KEY);
}

export function setTwitchAuthState(state: string): void {
  if (typeof window === "undefined") {
    return;
  }

  sessionStorage.setItem(TWITCH_STATE_KEY, state);
}

export function getTwitchAuthState(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  return sessionStorage.getItem(TWITCH_STATE_KEY);
}
