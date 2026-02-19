import { generateOAuthState, generatePkcePair } from "@/lib/auth/pkce";
import { setPkceVerifier, setTwitchAuthState } from "@/lib/auth/storage";

const TWITCH_AUTHORIZE_URL = "https://id.twitch.tv/oauth2/authorize";
const TWITCH_SCOPE = "user:read:email";

function getRequiredEnv(name: string): string {
  const value = process.env[name];
  if (!value) {
    throw new Error(`Missing required env variable: ${name}`);
  }

  return value;
}

export function getTwitchRedirectUri(): string {
  return getRequiredEnv("NEXT_PUBLIC_TWITCH_REDIRECT_URI");
}

export async function startTwitchLogin(): Promise<void> {
  const clientId = getRequiredEnv("NEXT_PUBLIC_TWITCH_CLIENT_ID");
  const redirectUri = getTwitchRedirectUri();
  const { verifier, challenge } = await generatePkcePair();
  const state = generateOAuthState();

  setPkceVerifier(verifier);
  setTwitchAuthState(state);

  const url = new URL(TWITCH_AUTHORIZE_URL);
  url.searchParams.set("client_id", clientId);
  url.searchParams.set("redirect_uri", redirectUri);
  url.searchParams.set("response_type", "code");
  url.searchParams.set("scope", TWITCH_SCOPE);
  url.searchParams.set("state", state);
  url.searchParams.set("code_challenge", challenge);
  url.searchParams.set("code_challenge_method", "S256");

  window.location.assign(url.toString());
}
