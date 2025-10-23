import React from "react";
import { StreamVideo } from "@/components/widgets/stream/stream-video";
import { StreamChat } from "@/components/widgets/stream/stream-chat";
import styles from "./stream.module.css";

export default function StreamPage() {
  const channel = "farymacomposer";

  return (
    <main className={styles.page}>
      <div className={styles.videoContainer}>
        <StreamVideo channel={channel} />
      </div>
      <aside className={styles.chatContainer}>
        <StreamChat channel={channel} />
      </aside>
    </main>
  );
}
