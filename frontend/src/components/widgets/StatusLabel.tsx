"use client";

import styles from "@/styles/header/StatusLabel.module.css";

type Props = {
    prefix?: string;        // "стрим идёт,"
    highlight?: string;     // "разносим!"
    variant?: "live" | "upcoming" | "offline";
};

export default function StatusLabel({
    prefix = "стрим идёт,",
    highlight = "разносим!",
    variant = "live",
}: Props) {
    return (
        <div className={`${styles.label} ${styles[variant]}`} role="status" aria-live="polite">
            <span className={styles.text}>{prefix}
                <span className={styles.highlight}>{highlight}</span>
            </span>
        </div>
    );
}
