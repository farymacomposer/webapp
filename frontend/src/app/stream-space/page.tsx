"use client";

import React, { useMemo, useState, useEffect } from "react";
import { StreamVideo } from "@/components/widgets/stream/stream-video";
import { StreamChat } from "@/components/widgets/stream/stream-chat";
import { Queue, QueueItem } from "@/components/widgets/stream/track-queue";
import styles from "./stream.module.css";

export default function StreamPage() {
  const channel = "farymacomposer";

  const initial: QueueItem[] = [
    {
      id: "1",
      label: "NOW",
      labelColor: "#ff2d6f",
      title: "YOASOBI 『アイドル』 Official Music Video",
      price: "100",
      coverUrl:
        "https://sfae.blob.core.windows.net/media/ecommercesite/media/sfae/sfae.artwork/342_1.jpg",
    },
    {
      id: "2",
      label: "NEW",
      labelColor: "#ff8c1a",
      price: "100",
      title: "HOLLOW KNIGHT SILKSONG - Official Soundtrack",
    },
    {
      id: "3",
      label: "W2",
      labelColor: "#7e3ff2",
      price: "100",
      title: "Queen - Bohemian Rhapsody",
    },
    {
      id: "4",
      label: "NEW",
      labelColor: "#ff8c1a",
      price: "100",
      title: "Lady Gaga - Bad Romance (Official Music Video)",
    },
    {
      id: "5",
      label: "W3",
      labelColor: "#7e3ff2",
      title:
        "HOLLOW KNIGHT SILKSONG - Official Soundtrack HOLLOW KNIGHT SILKSONG...",
      price: "100",
    },
  ];

  const [queueItems, setQueueItems] = useState<QueueItem[]>(initial);

  const displayItems = useMemo(() => {
    return queueItems.map((item, index) =>
      index === 0 ? { ...item, label: "NOW", labelColor: "#ff2d6f" } : item
    );
  }, [queueItems]);

  return (
    <main className={styles.page}>
      <div className={styles.layout}>
        <section className={styles.mainColumn}>
          <div className={styles.videoSection}>
            <StreamVideo channel={channel} />
          </div>

          <div className={styles.queueSection}>
            <Queue items={displayItems} />
          </div>
        </section>

        <aside className={styles.chatContainer}>
          <StreamChat channel={channel} />
        </aside>
      </div>
    </main>
  );
}
