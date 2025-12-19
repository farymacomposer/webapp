"use client";

import React, { useState } from "react";
import { StreamVideo } from "@/components/widgets/stream/stream-video";
import { StreamChat } from "@/components/widgets/stream/stream-chat";
import { Queue, QueueItem } from "@/components/widgets/stream/track-queue";
import styles from "./stream.module.css";

export default function StreamPage() {
  const channel = "farymacomposer";
  const [showQueue, setShowQueue] = useState(false);

  const initial: QueueItem[] = [
    {
      id: "1",
      label: "NOW",
      labelColor: "#ff2d6f",
      title: "YOASOBI 『アイドル』 Official Music Video",
    },
    {
      id: "2",
      label: "NEW",
      labelColor: "#ff8c1a",
      title: "HOLLOW KNIGHT SILKSONG - Official Soundtrack",
    },
    {
      id: "3",
      label: "W2",
      labelColor: "#9b59b6",
      title: "Queen - Bohemian Rhapsody",
    },
    {
      id: "4",
      label: "NEW",
      labelColor: "#ff8c1a",
      title: "Lady Gaga - Bad Romance (Official Music Video)",
    },
    {
      id: "5",
      label: "W3",
      labelColor: "#7e3ff2",
      title:
        "HOLLOW KNIGHT SILKSONG - Official Soundtrack HOLLOW KNIGHT SILKSONG...",
    },
  ];

  const [activeId, setActiveId] = useState<string | undefined>("1");

  return (
    <main className={styles.page}>
      <div className={styles.layout}>
        <div className={styles.videoSection}>
          <StreamVideo channel={channel} />
          <div
            className={`${styles.queueWrapper} ${
              showQueue ? styles.queueVisible : styles.queueHidden
            }`}
          >
            <Queue
              items={initial}
              activeId={activeId}
              onSelect={(id) => {
                setActiveId(id);
                // здесь потом легко дернуть SignalR:
                // hubConnection.invoke("SelectTrack", id);
              }}
            />
          </div>
          <button
            className={styles.toggleQueueBtn}
            onClick={() => setShowQueue(!showQueue)}
          >
            {showQueue ? "Скрыть очередь" : "Показать очередь"}
          </button>
        </div>
        <aside className={styles.chatContainer}>
          <StreamChat channel={channel} />
        </aside>
      </div>
    </main>
  );
}
