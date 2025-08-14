import { Configuration } from "../../generated-api";

const basePath =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:8080";

export const apiConfig = new Configuration({
  basePath,
  // добавим авторизацию если потребуется
  accessToken: () => {
    if (typeof window !== "undefined") {
      return localStorage.getItem("access_token") || "";
    }
    return "";
  },
});
