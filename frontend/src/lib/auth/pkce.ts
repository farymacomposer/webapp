function toBase64Url(bytes: ArrayBuffer): string {
  const base64 = btoa(String.fromCharCode(...new Uint8Array(bytes)));
  return base64.replace(/\+/g, "-").replace(/\//g, "_").replace(/=+$/g, "");
}

function randomString(length: number): string {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
  const bytes = new Uint8Array(length);
  crypto.getRandomValues(bytes);

  return Array.from(bytes, (byte) => chars[byte % chars.length]).join("");
}

export async function generatePkcePair(): Promise<{ verifier: string; challenge: string }> {
  const verifier = randomString(96);
  const data = new TextEncoder().encode(verifier);
  const digest = await crypto.subtle.digest("SHA-256", data);

  return {
    verifier,
    challenge: toBase64Url(digest),
  };
}

export function generateOAuthState(): string {
  return randomString(48);
}
