"use client";

import React, {
  ReactNode,
  CSSProperties,
  useEffect,
  useRef,
  useState,
} from "react";
import styles from "./track-queue.module.css";
import { SiSpotify, SiYoutube } from "@icons-pack/react-simple-icons";
import { BellRing, MessageCircle } from "lucide-react";
import { useClickAway } from "react-use";

export type QueueItem = {
  id: string;
  label: string;
  labelColor: string;
  textColor?: string;
  title: string;
  price: string;
  coverUrl?: string;

  previewArtist?: string;
  previewActions?: ReactNode;
};

type QueueProps = {
  items: QueueItem[];
  onAdd?: () => void;
  onOpenChange?: (isOpen: boolean) => void;
  className?: string;
};

export const Queue: React.FC<QueueProps> = ({
  items,
  onAdd,
  onOpenChange,
  className,
}) => {
  const [activeId, setActiveId] = useState<string | null>(null);
  const rootRef = useRef<HTMLDivElement | null>(null);
  const listRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    onOpenChange?.(Boolean(activeId));
  }, [activeId, onOpenChange]);

  useClickAway(rootRef, () => {
    if (activeId) {
      setActiveId(null);
    }
  });

  return (
    <div
      ref={rootRef}
      className={[styles.root, className ?? ""].filter(Boolean).join(" ")}
      onClickCapture={(event) => {
        if (!activeId) {
          return;
        }

        const target = event.target as HTMLElement | null;
        if (!target?.closest('[data-queue-card="true"]')) {
          setActiveId(null);
        }
      }}
    >
      {activeId && (
        <button
          type="button"
          className={styles.backdrop}
          aria-label="Закрыть карточку"
          onClick={() => setActiveId(null)}
        />
      )}

      <button
        type="button"
        className={styles.addCard}
        onClick={onAdd}
        aria-label="Добавить трек"
      >
        +
      </button>

      <div
        ref={listRef}
        className={styles.list}
        role="listbox"
        aria-label="Очередь треков"
      >
        {items.map((item, index) => {
          const isFirst = index === 0;
          const isActive = item.id === activeId;

          const accentStyle: CSSProperties = {
            ["--accent-color" as any]: item.labelColor,
            ["--text-color" as any]: item.textColor,
          };

          return (
            <React.Fragment key={item.id}>
              {!isFirst && (
                <div className={styles.separator} aria-hidden="true">
                  <span className={styles.chevron} />
                </div>
              )}

              <div
                className={[
                  styles.card,
                  isActive ? styles.cardActive : "",
                ].join(" ")}
                data-queue-card="true"
                style={accentStyle}
                role="option"
                aria-selected={isActive}
                tabIndex={0}
                onClick={() =>
                  setActiveId((prev) => (prev === item.id ? null : item.id))
                }
              >
                <div className={styles.headerRow}>
                  <div className={styles.tags}>
                    <span className={styles.tag}>{item.label}</span>
                  </div>
                  <div className={styles.body}>
                    <span className={styles.title}>{item.title}</span>
                  </div>
                </div>

                {isActive && (
                  <div
                    className={styles.preview}
                    onClick={(e) => e.stopPropagation()}
                  >
                    {/* Top bar */}
                    <div className={styles.previewTop}>
                      <div className={styles.previewActions}>
                        {item.previewActions ?? (
                          <>
                            <button
                              className={`${styles.iconBtn} ${styles.spotify}`}
                            >
                              <SiSpotify />
                            </button>

                            <button
                              className={`${styles.iconBtn} ${styles.youtube}`}
                            >
                              <SiYoutube />
                            </button>

                            <button
                              className={`${styles.iconBtn} ${styles.comment}`}
                            >
                              <MessageCircle />
                            </button>

                            <button
                              className={`${styles.iconBtn} ${styles.notify}`}
                            >
                              <BellRing
                                color="#FF9100FF"
                                fill="#FF9100FF"
                              />
                            </button>
                          </>
                        )}
                      </div>

                      {item.price && (
                        <span className={styles.previewPrice}>
                          {item.price}
                        </span>
                      )}
                    </div>

                    {/* Big square cover */}
                    <div className={styles.previewCover}>
                      {item.coverUrl && (
                        <img
                          src={item.coverUrl}
                          alt={item.title}
                          className={styles.previewCoverImg}
                        />
                      )}
                    </div>
                  </div>
                )}
              </div>
            </React.Fragment>
          );
        })}
      </div>
    </div>
  );
};
