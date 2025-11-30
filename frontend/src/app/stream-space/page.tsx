"use client";

import React, { useState } from "react";
import { StreamVideo } from "@/components/widgets/stream/stream-video";
import { StreamChat } from "@/components/widgets/stream/stream-chat";
import { TrackQueue } from "@/components/widgets/stream/track-queue";
import styles from "./stream.module.css";

export default function StreamPage() {
  const channel = "farymacomposer";
  const [showQueue, setShowQueue] = useState(false);

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
            <TrackQueue />
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
