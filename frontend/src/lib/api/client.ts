import { getAuthToken } from "@/lib/auth/storage";

export type ApiConfiguration = {
  baseUrl: string;
  authorizationHeader: string | null;
};

function getRequiredApiBaseUrl(): string {
  const baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (!baseUrl) {
    throw new Error("Missing NEXT_PUBLIC_API_BASE_URL");
  }

  return baseUrl;
}

export function createApiConfiguration(): ApiConfiguration {
  const token = getAuthToken();

  return {
    baseUrl: getRequiredApiBaseUrl(),
    authorizationHeader: token ? `Bearer ${token}` : null,
  };
}
