import {
  Configuration,
  ReviewOrderApi,
  OrderQueueApi,
  ComposerStreamApi,
  AppSettingsApi,
  AuthApi,
} from "../../generated-api";

const basePath =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:8080/api";

const config = new Configuration({
  basePath,
});

export const Api = {
  reviewOrder: new ReviewOrderApi(config),
  orderQueue: new OrderQueueApi(config),
  composerStream: new ComposerStreamApi(config),
  appSettings: new AppSettingsApi(config),
  auth: new AuthApi(config),
};