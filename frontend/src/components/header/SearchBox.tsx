"use client";

import { useState } from "react";
import styles from "./SearchBox.module.css";

export default function SearchBox() {
    const [query, setQuery] = useState("");

    const handleRandom = () => {
        // TODO: здесь дергай API/рандом из каталога треков
        console.log("random track");
    };

    return (
        <div className={styles.wrap}>
            <input
                type="text"
                className={styles.input}
                placeholder="Поиск трека…"
                aria-label="Поиск трека"
                value={query}
                onChange={(e) => setQuery(e.target.value)}
            />
            <button
                type="button"
                className={styles.random}
                onClick={handleRandom}
            >
                случайно!
            </button>
        </div>
    );
}
