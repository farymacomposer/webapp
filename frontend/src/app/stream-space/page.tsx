"use client";

import React, { useMemo, useState, useEffect } from "react";
import { StreamVideo } from "@/components/widgets/stream/stream-video";
import { StreamChat } from "@/components/widgets/stream/stream-chat";
import { Queue, QueueItem } from "@/components/widgets/stream/track-queue";
import styles from "./stream.module.css";
import { ArrowRight } from "lucide-react";
import Link from "next/link";

export default function StreamPage() {
  const channel = "farymacomposer";

  const initial: QueueItem[] = [
    {
      id: "1",
      label: "NOW",
      labelColor: "#ffffff",
      title: "YOASOBI 『アイドル』 Official Music Video",
      price: "3500₽",
      textColor: "#000000",
      coverUrl:
        "https://asset.watch.impress.co.jp/img/gmw/docs/1492/924/main_l.jpg",
    },
    {
      id: "2",
      label: "NEW",
      labelColor: "#ff8c1a",
      price: "3500₽",
      title: "HOLLOW KNIGHT SILKSONG - Official Soundtrack",
    },
    {
      id: "3",
      label: "W2",
      labelColor: "#E10741",
      price: "3500₽",
      title: "Queen - Bohemian Rhapsody",
      coverUrl:
        "https://sfae.blob.core.windows.net/media/ecommercesite/media/sfae/sfae.artwork/342_1.jpg",
    },
    {
      id: "4",
      label: "NEW",
      labelColor: "#ff8c1a",
      price: "3500₽",
      title: "Lady Gaga - Bad Romance (Official Music Video)",
    },
    {
      id: "5",
      label: "W3",
      labelColor: "#C9006E",
      title:
        "HOLLOW KNIGHT SILKSONG - Official Soundtrack HOLLOW KNIGHT SILKSONG...",
      price: "3500₽",
    },
  ];

  const [queueItems, setQueueItems] = useState<QueueItem[]>(initial);
  const [isQueueOpen, setIsQueueOpen] = useState(false);

  const displayItems = useMemo(() => {
    return queueItems.map((item, index) =>
      index === 0 ? { ...item, label: "NOW", labelColor: "#FFFFFF" } : item
    );
  }, [queueItems]);

  return (
    <main className={styles.page}>
      <div
        className={[
          styles.layout,
          isQueueOpen ? styles.queueOpen : "",
        ]
          .filter(Boolean)
          .join(" ")}
      >
        <section className={styles.mainColumn}>
          <div className={styles.videoSection}>
            <StreamVideo channel={channel} />
          </div>

          <div className={styles.queueSection}>
            <Queue items={displayItems} onOpenChange={setIsQueueOpen} />
          </div>
        </section>

        <aside className={styles.chatContainer}>
          <div className={styles.chatHeader}>
            <div className={styles.chatNav}>
              <Link className={styles.chatNavItem} href="/stream">
                <span className={styles.brand}>стрим-space</span>
              </Link>

              <Link className={styles.chatNavItem} href="/tracks">
                база треков
              </Link>

              <Link className={styles.chatNavItem} href="/faq">
                FAQ
              </Link>
            </div>

            <button className={styles.primaryCta} type="button">
              закинуть трек на разнос
            </button>
          </div>

          <StreamChat channel={channel} />
        </aside>
      </div>
    </main>
  );
}
