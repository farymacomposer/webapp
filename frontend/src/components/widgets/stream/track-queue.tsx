"use client";

import React, { ReactNode, CSSProperties, useState } from "react";
import styles from "./track-queue.module.css";
import { SiReact, SiSpotify, SiYoutube } from "@icons-pack/react-simple-icons";
import { BellRing, MessageCircle } from "lucide-react";

export type QueueItem = {
  id: string;
  label: string;
  labelColor: string;
  title: string;
  price: string;
  coverUrl?: string;

  previewArtist?: string;
  previewActions?: ReactNode;
};

type QueueProps = {
  items: QueueItem[];
  onAdd?: () => void;
  className?: string;
};

export const Queue: React.FC<QueueProps> = ({ items, onAdd, className }) => {
  const [activeId, setActiveId] = useState<string | null>(null);

  return (
    <div className={[styles.root, className ?? ""].filter(Boolean).join(" ")}>
      {activeId && <div className={styles.backdrop} aria-hidden="true" />}

      <button
        type="button"
        className={styles.addCard}
        onClick={onAdd}
        aria-label="Добавить трек"
      >
        +
      </button>

      <div className={styles.list} role="listbox" aria-label="Очередь треков">
        {items.map((item, index) => {
          const isFirst = index === 0;
          const isActive = item.id === activeId;

          const accentStyle: CSSProperties = {
            ["--accent-color" as any]: item.labelColor,
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
                            <SiSpotify color="#1DB954" size={24} />
                            <SiYoutube color="#FF0000" size={24} />
                            <MessageCircle
                              color="#4C4C4C"
                              size={24}
                              fill="#4C4C4C"
                            />
                            <BellRing
                              color="#FF9100FF"
                              size={24}
                              fill="#FF9100FF"
                            />
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
