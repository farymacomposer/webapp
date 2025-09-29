"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import styles from "./Header.module.css";

import StatusLabel from "./StatusLabel";
import SearchBox from "./SearchBox";

//Задаём пути
const routes = [
    { href: "/", label: "Главная", exact: true },
    { href: "/stream-space", label: "Стрим-Space" },
    { href: "/catalog", label: "База Треков" },
    { href: "/help", label: "Помощь" },
];

export default function Header() {
    const pathname = usePathname();
    return (
        <header className={styles.header}>
            <div className={styles.inner}>
                <nav className={styles.nav}>
                    <StatusLabel />
                    <SearchBox />
                    {/* Генерируем ссылки для страниц */}
                    {routes.map(r => {
                        const active = r.exact ? pathname === r.href : pathname.startsWith(r.href);
                        return (
                            <Link
                                key={r.href}
                                href={r.href}
                                className={`${styles.navLink} ${active ? styles.active : ""}`}
                            >
                                {r.label}
                            </Link>
                        );
                    })}
                </nav>
            </div>
        </header>
    );
}
