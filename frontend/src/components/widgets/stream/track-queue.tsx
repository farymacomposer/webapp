import React from "react";
import styles from "./track-queue.module.css";

interface Track {
  title: string;
  cover: string;
  nickname: string;
  url: string;
}

interface TrackQueueProps {
  tracks?: Track[];
  currentIndex?: number;
}

export const TrackQueue: React.FC<TrackQueueProps> = ({
  tracks,
  currentIndex = 0,
}) => {
  const mockTracks: Track[] = tracks || [
    {
      title: "Ado – Ussewa",
      cover: "/covers/ussewa.jpg",
      nickname: "NeoGhost",
      url: "https://www.youtube.com/watch?v=Qp3b-RXtz4w",
    },
    {
      title: "YOASOBI – Idol",
      cover: "/covers/idol.jpg",
      nickname: "itsKuma",
      url: "https://open.spotify.com/track/1",
    },
    {
      title: "Eve – Dramaturgy",
      cover: "/covers/dramaturgy.jpg",
      nickname: "Lunaria",
      url: "https://soundcloud.com/eve_dramaturgy",
    },
    {
      title: "Kenshi Yonezu – Lemon",
      cover: "/covers/lemon.jpg",
      nickname: "azurite",
      url: "https://vk.com/music/track/123",
    },
    {
      title: "YOASOBI – Racing into the Night",
      cover: "/covers/yoru-ni-kakeru.jpg",
      nickname: "reiya",
      url: "https://music.apple.com/us/album/yoru-ni-kakeru/123456",
    },
    {
      title: "",
      cover: "",
      nickname: "",
      url: "",
    },
    {
      title: "",
      cover: "",
      nickname: "",
      url: "",
    },
    {
      title: "",
      cover: "",
      nickname: "",
      url: "",
    },
  ];

  const getPlatformIcon = (url: string): string => {
    if (url.includes("youtube"))
      return "https://img.icons8.com/color/48/youtube-play.png";
    if (url.includes("spotify"))
      return "https://img.icons8.com/color/48/spotify.png";
    if (url.includes("soundcloud"))
      return "https://img.icons8.com/color/48/soundcloud.png";
    if (url.includes("vk")) return "https://img.icons8.com/color/48/vk-com.png";
    if (url.includes("apple"))
      return "https://img.icons8.com/color/48/apple-music.png";
    return "https://img.icons8.com/fluency/48/link.png";
  };

  return (
    <div className={styles.queue}>
      {mockTracks.map((track, i) => {
        const isActive = i === currentIndex;
        return (
          <div
            key={i}
            className={`${styles.trackCard} ${
              isActive ? styles.activeTrack : ""
            }`}
          >
            {isActive && (
              <div className={styles.topLabel}>Разбираемый сейчас трек</div>
            )}

            <div className={styles.coverWrapper}>
              <img
                src={track.cover}
                alt={track.title}
                className={styles.cover}
              />
              {track.url && (
                <a
                  href={track.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  className={styles.platformIcon}
                >
                  <img
                    src={getPlatformIcon(track.url)}
                    alt="platform icon"
                    className={styles.platformImage}
                  />
                </a>
              )}
            </div>
            <div className={styles.info}>
              <div className={styles.title}>{track.title}</div>
              <div className={styles.nickname}>от {track.nickname}</div>
            </div>
          </div>
        );
      })}
    </div>
  );
};
