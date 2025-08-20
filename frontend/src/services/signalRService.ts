// services/signalRService.ts
import {
  HubConnectionBuilder,
  HubConnection,
  LogLevel,
  HttpTransportType,
} from "@microsoft/signalr";

class SignalRService {
  private connection: HubConnection | null = null;

  constructor(private hubUrl: string) {}

  public async start() {
    if (!this.connection) {
      this.connection = new HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          skipNegotiation: true,
          transport: HttpTransportType.WebSockets,
        })
        .withAutomaticReconnect()
        .configureLogging(LogLevel.Information)
        .build();
    }

    if (this.connection.state === "Disconnected") {
      try {
        await this.connection.start();
        console.log("✅ SignalR Connected");
      } catch (err) {
        console.error("❌ SignalR Error:", err);
        setTimeout(() => this.start(), 5000);
      }
    }
  }

  public on<T>(eventName: string, callback: (data: T) => void) {
    this.connection?.on(eventName, callback);
  }

  public off(eventName: string) {
    this.connection?.off(eventName);
  }

  public async send<T>(method: string, payload: T) {
    await this.connection?.invoke(method, payload);
  }

  public async stop() {
    await this.connection?.stop();
  }
}

const hubUrl = process.env.NEXT_PUBLIC_SIGNALR_URL;
if (!hubUrl) {
  throw new Error("❌ NEXT_PUBLIC_SIGNALR_URL is not set in .env.local");
}

export const signalRService = new SignalRService(hubUrl);
