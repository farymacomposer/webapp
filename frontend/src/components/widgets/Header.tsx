"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import styles from "@/styles/header/Header.module.css";

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
                    {routes.map(route => {
                        const active = route.exact ? pathname === route.href : pathname.startsWith(route.href);
                        return (
                            <Link
                                key={route.href}
                                href={route.href}
                                className={`${styles.navLink} ${active ? styles.active : ""}`}
                            >
                                {route.label}
                            </Link>
                        );
                    })}
                </nav>
            </div>
        </header>
    );
}
