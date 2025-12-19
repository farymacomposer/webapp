"use client";

import React, { ReactNode, CSSProperties } from "react";
import styles from "./track-queue.module.css";

export type QueueItem = {
  id: string;
  label: string;
  labelColor: string;
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
  onAdd?: () => void;
  className?: string;
};

export const Queue: React.FC<QueueProps> = ({
  items,
  activeId,
  onSelect,
  onAdd,
  className,
}) => {
  return (
    <div
      className={[styles.root, className ?? ""].filter(Boolean).join(" ")}
    >
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
                  isFirst ? styles.cardFirst : "",
                  isActive ? styles.cardActive : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
                style={accentStyle}
                role="option"
                aria-selected={isActive}
                tabIndex={0}
                onClick={() => onSelect?.(item.id)}
              >
                <div className={styles.cardInner}>
                  <div className={styles.headerRow}>
                    <div className={styles.tags}>
                      <span className={styles.tag}>{item.label}</span>
                    </div>

                    <div className={styles.body}>
                      <span className={styles.title}>{item.title}</span>
                    </div>
                  </div>
                </div>

                {isActive && (
                  <div
                    className={styles.preview}
                    onClick={(e) => e.stopPropagation()}
                  >
                    <div className={styles.previewHeader}>
                      <div className={styles.previewActions}>
                        {item.previewActions ?? (
                          <>
                            <span className={styles.actionDot} />
                            <span className={styles.actionDot} />
                            <span className={styles.actionDot} />
                            <span className={styles.actionDot} />
                          </>
                        )}
                      </div>
                      {item.previewPrice && (
                        <span className={styles.previewPrice}>
                          {item.previewPrice}
                        </span>
                      )}
                    </div>

                    <div className={styles.previewBody}>
                      <div className={styles.previewCover}>
                        {item.previewCoverUrl && (
                          <img
                            src={item.previewCoverUrl}
                            alt={item.title}
                            className={styles.previewCoverImg}
                          />
                        )}
                      </div>
                      <div className={styles.previewMeta}>
                        <span className={styles.previewTitle}>
                          {item.title}
                        </span>
                        {item.previewArtist && (
                          <span className={styles.previewArtist}>
                            {item.previewArtist}
                          </span>
                        )}
                      </div>
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
