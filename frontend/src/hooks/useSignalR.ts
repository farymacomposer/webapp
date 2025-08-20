"use client";

import { useEffect, useState } from "react";
import { signalRService } from "@/services/signalRService";
import {
  NewOrderAddedEvent,
  OrderPositionChangedEvent,
  OrderRemovedEvent,
} from "@/types/signalR";
import { Api } from "@/services/client";
import {
  FarymaComposerApiFeaturesOrderQueueFeatureGetOrderQueueGetOrderQueueResponse,
  FarymaComposerApiFeaturesCommonDtoReviewOrderDto,
  FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto,
} from "../../generated-api";

export function useOrdersSignalR() {
  // 👉 теперь храним именно ReviewOrderDto[]
  const [orders, setOrders] = useState<
    FarymaComposerApiFeaturesCommonDtoReviewOrderDto[]
  >([]);
  const [connected, setConnected] = useState(false);
  const [syncVersion, setSyncVersion] = useState(0);

  // Загружаем полный список очереди через HTTP
  async function reloadQueue() {
    try {
      const response = await Api.orderQueue.apiOrderQueueGetOrderQueueGet();
      const data =
        response.data as FarymaComposerApiFeaturesOrderQueueFeatureGetOrderQueueGetOrderQueueResponse;

      // Забираем только order из OrderPositionDto
      const merged = [
        ...data.activeOrders.map((p) => p.order!).filter(Boolean),
        ...(data.inProgressOrder ? [data.inProgressOrder.order!] : []),
        ...data.completedOrders.map((p) => p.order!).filter(Boolean),
        ...data.scheduledOrders.map((p) => p.order!).filter(Boolean),
        ...data.frozenOrders.map((p) => p.order!).filter(Boolean),
      ];

      setOrders(merged);
      setSyncVersion(data.syncVersion ?? 0);
    } catch (err) {
      console.error("❌ Ошибка загрузки очереди:", err);
    }
  }

  // Проверка версий синхронизации
  function checkSyncVersion(newSyncVersion: number): boolean {
    if (newSyncVersion === syncVersion + 1) {
      setSyncVersion(newSyncVersion);
      return true;
    }
    if (newSyncVersion > syncVersion + 1) {
      console.warn(
        `⚠️ Потеряно событие: ожидался ${
          syncVersion + 1
        }, но пришёл ${newSyncVersion}. Перезагружаем очередь...`
      );
      reloadQueue();
      return false;
    }
    return false;
  }

  useEffect(() => {
    let mounted = true;

    const init = async () => {
      try {
        await signalRService.start();
        if (!mounted) return;
        setConnected(true);

        // Новый заказ
        signalRService.on<NewOrderAddedEvent>(
          "newOrderAddedEvent",
          ({ syncVersion: sv, order }) => {
            if (!checkSyncVersion(sv)) return;

            setOrders((prev) => {
              const exists = prev.find((o) => o.id === order.id);
              if (exists) {
                return prev.map((o) => (o.id === order.id ? order : o));
              }
              return [...prev, order];
            });
          }
        );

        // Удаление заказа
        signalRService.on<OrderRemovedEvent>(
          "orderRemovedEvent",
          ({ syncVersion: sv, order }) => {
            if (!checkSyncVersion(sv)) return;
            setOrders((prev) => prev.filter((o) => o.id !== order.id));
          }
        );

        // Изменение позиции одного заказа (нам важен только order)
        signalRService.on<OrderPositionChangedEvent>(
          "orderPositionChangedEvent",
          ({ syncVersion: sv, order }) => {
            if (!checkSyncVersion(sv)) return;

            setOrders((prev) =>
              prev.map((o) => (o.id === order.id ? order : o))
            );
          }
        );

        // Изменение позиций нескольких заказов (опять же берём order)
        signalRService.on<OrderPositionsChangedEvent>(
          "orderPositionsChangedEvent",
          ({ syncVersion: sv, orderPositions }) => {
            if (!checkSyncVersion(sv)) return;

            setOrders((prev) =>
              prev.map((o) => {
                const update = orderPositions.find(
                  (
                    u: FarymaComposerApiFeaturesOrderQueueFeatureDtoOrderPositionDto
                  ) => u.order?.id === o.id
                );
                return update?.order ?? o;
              })
            );
          }
        );

        // Инициализация состояния очереди
        reloadQueue();
      } catch (err) {
        console.error("❌ Ошибка инициализации SignalR:", err);
      }
    };

    init();

    return () => {
      mounted = false;
      signalRService.stop();
    };
  }, []);

  return { orders, connected, syncVersion, reloadQueue };
}
