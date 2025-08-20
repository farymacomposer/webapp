// types/signalr.ts

import { FarymaComposerApiFeaturesCommonDtoReviewOrderDto } from "../../generated-api";

export interface OrderDto {
  id: number;
  createdAt: string;
  trackUrl: string | null;
  mainNickname: string;
  totalAmount: number;
  type: number;
  status: number;
}

export interface OrderQueuePositionDto {
  queueIndex: number;
  activityStatus: number;
  categoryType: number;
  categoryDebtNumber: number;
}

export interface NewOrderAddedEvent {
  syncVersion: number;
  order: FarymaComposerApiFeaturesCommonDtoReviewOrderDto;
  currentPosition: OrderQueuePositionDto;
}

export interface OrderRemovedEvent {
  syncVersion: number;
  order: OrderDto;
  previousPosition: OrderQueuePositionDto;
}

export interface OrderPositionChangedEvent {
  syncVersion: number;
  order: FarymaComposerApiFeaturesCommonDtoReviewOrderDto;
  previousPosition: OrderQueuePositionDto;
  currentPosition: OrderQueuePositionDto;
}

export type OrderWithPosition = OrderDto & {
  currentPosition: OrderQueuePositionDto;
};
