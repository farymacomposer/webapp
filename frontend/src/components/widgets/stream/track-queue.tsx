"use client";

import React, { ReactNode, CSSProperties } from "react";
import styles from "./track-queue.module.css";

export type QueueItem = {
  id: string;
  label: string;      // NOW / NEW / W2 / W3
  labelColor: string; // пока используем только для превью и первой полоски
  title: string;

  previewCoverUrl?: string;
  previewArtist?: string;
  previewPrice?: string;
  previewActions?: ReactNode;
};

type QueueProps = {
  items: QueueItem[];
  activeId?: string;
  onSelect?: (id: string) => void;
  className?: string;
};

export const Queue: React.FC<QueueProps> = ({
  items,
  activeId,
  onSelect,
  className,
}) => {
  return (
    <div className={`${styles.root} ${className ?? ""}`}>
      <div className={styles.list}>
        {items.map((item, index) => {
          const isFirst = index === 0;
          const isActive = item.id === activeId;

          const accentStyle: CSSProperties = {
            // пригодится для первой полоски и превью
            ["--accent-color" as any]: item.labelColor,
          };

          return (
            <button
              key={item.id}
              type="button"
              className={[
                styles.card,
                isFirst ? styles.cardFirst : "",
                isActive ? styles.cardActive : "",
              ]
                .filter(Boolean)
                .join(" ")}
              onClick={() => onSelect?.(item.id)}
              style={accentStyle}
            >
              {/* таб NOW / NEW / W2 / W3 — фон берём из CSS (чёрный) */}
              <div className={styles.tab}>
                <div className={styles.colorTop}></div>
                <div className={styles.colorLeft}></div>
                <div className={styles.tabInner}>{item.label}</div>
              </div>

              <div className={styles.pointer} />

              <div className={styles.body}>
                <span className={styles.title}>{item.title}</span>
              </div>

              {(item.previewCoverUrl || item.previewActions) && (
                <div className={styles.preview}>
                  <div className={styles.previewHeader}>
                    <div className={styles.previewActions}>
                      {item.previewActions}
                    </div>

                    {(item.label || item.previewPrice) && (
                      <div className={styles.previewMeta}>
                        {item.label && (
                          <span
                            className={styles.previewTag}
                            style={{ backgroundColor: item.labelColor }}
                          >
                            {item.label}
                          </span>
                        )}
                        {item.previewPrice && (
                          <span className={styles.previewPrice}>
                            {item.previewPrice}
                          </span>
                        )}
                      </div>
                    )}
                  </div>

                  {item.previewCoverUrl && (
                    <div className={styles.previewImageWrapper}>
                      <img
                        src={item.previewCoverUrl}
                        alt={item.title}
                        className={styles.previewImage}
                      />
                    </div>
                  )}

                  <div className={styles.previewInfo}>
                    <div className={styles.previewTitle}>{item.title}</div>
                    {item.previewArtist && (
                      <div className={styles.previewArtist}>
                        от {item.previewArtist}
                      </div>
                    )}
                  </div>
                </div>
              )}
            </button>
          );
        })}
      </div>
    </div>
  );
};
