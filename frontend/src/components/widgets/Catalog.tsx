'use client'
import React, { useState, useRef, useEffect, useMemo } from 'react';
import TrackCard from "@/components/shared/TrackCard";
import catalog from '@/styles/catalog/Catalog.module.css'
import header from '@/styles/catalog/Header.module.css'

export interface Track {
    id: number;
    title: string;
    artist: string;
    source: string;
    status: 'distributed' | 'on-air' | 'delivered' | 'upcoming' | 'frozen';
    wave: string;
    price?: string;
    time?: string;
    hasVideo?: boolean;
    isFavorite?: boolean;
    isNotified?: boolean;
}

const OrderQueue: React.FC<{hideWindow: (arg: boolean) => void}> = ( {hideWindow}: {hideWindow: (arg: boolean) => void} ) => {
    const [window, setWindow] = useState(true);

    const [activeTab, setActiveTab] = useState<'order' | 'waves'>('waves');
    const [searchQuery, setSearchQuery] = useState('');
    const [expandedSections, setExpandedSections] = useState<Record<string, boolean>>({});
    const [activeHeaderTab, setActiveHeaderTab] = useState<string>('');

    const sectionRefs = useRef<Record<string, HTMLDivElement | null>>({});
    const containerRef = useRef<HTMLDivElement>(null);

    const tracks: Track[] = [
        { id: 1, title: 'Queen - Bohemian Rhapsody', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/1a1a1a/ffffff?text=Q', status: 'distributed', wave: 'new', time: 'вне очереди' },
        { id: 2, title: 'әt ək ON táɪtn', artist: 'TakaoYamataki', source: 'https://via.placeholder.com/80x80/4a90e2/ffffff?text=AT', status: 'on-air', wave: 'new', price: '2600₽', time: '~30 минут', isFavorite: true },
        { id: 3, title: 'Imagine Dragons', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/1a1a1a/ffffff?text=ID', status: 'on-air', wave: 'wave10', price: '1900₽', time: '~10 минут' },
        { id: 4, title: 'Genshin Theme', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/f4a460/ffffff?text=ST', status: 'on-air', wave: 'wave10', price: '1666₽', time: '~10 минут', isFavorite: true, isNotified: true },
        { id: 5, title: 'Bad Romance', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/9b59b6/ffffff?text=BR', status: 'delivered', wave: 'wave10', price: '1200₽', time: '~10 минут', isFavorite: true, hasVideo: true },
        { id: 6, title: 'Track 6', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/9b59b6/ffffff?text=6', status: 'delivered', wave: 'wave9', price: '1111₽', time: '~10 минут', hasVideo: true },
        { id: 7, title: 'Track 7', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/9b59b6/ffffff?text=7', status: 'delivered', wave: 'wave9', price: '1111₽', time: '~10 минут', hasVideo: true },
        { id: 8, title: 'Track 8', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/9b59b6/ffffff?text=8', status: 'delivered', wave: 'wave8', price: '1111₽', time: '~10 минут', hasVideo: true },
        { id: 9, title: 'Track 9', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/9b59b6/ffffff?text=9', status: 'delivered', wave: 'wave8', price: '1111₽', time: '~10 минут', hasVideo: true },
        { id: 10, title: 'Track 10', artist: 'Static Stream', source: 'https://via.placeholder.com/80x80/9b59b6/ffffff?text=10', status: 'upcoming', wave: 'wave7', price: '1100₽', time: '~10 минут', isFavorite: true, hasVideo: true },
        { id: 14, title: 'Frozen Song', artist: 'Ice Queen', source: 'https://via.placeholder.com/80x80/404040/ffffff?text=F', status: 'frozen', wave: 'wave5', price: '800₽', time: 'заморожено', hasVideo: false },
        { id: 15, title: 'Cold as Ice', artist: 'Winter Soul', source: 'https://via.placeholder.com/80x80/404040/ffffff?text=C', status: 'frozen', wave: 'wave4', price: '900₽', time: 'заморожено', isFavorite: true }
    ];

    const statusOrder = ['distributed', 'on-air', 'delivered', 'upcoming', 'frozen'];
    const waveOrder = ['new', 'wave10', 'wave9', 'wave8', 'wave7', 'wave6', 'wave5', 'wave4', 'wave3', 'wave2'];

    const getGroupLabel = (key: string, type: 'status' | 'wave') => {
        if (type === 'status') {
            const labels: Record<string, string> = {
                distributed: 'NOW',
                'on-air': 'NEXT UP',
                delivered: 'FINISHED',
                upcoming: 'FUTURE',
                frozen: 'FROZEN'
            };
            return labels[key] || key;
        } else {
            if (key === 'new') return 'NEW';
            return key.toUpperCase(); // WAVE10, WAVE9 etc.
        }
    };

    const currentGroupKeys = activeTab === 'order' ? statusOrder : waveOrder;
    const currentGroupType = activeTab === 'order' ? 'status' : 'wave';

    // Группировка треков
    const groupedTracks = useMemo(() => {
        return tracks.reduce((acc, track) => {
            const key = activeTab === 'order' ? track.status : track.wave;
            if (!acc[key]) {
                acc[key] = [];
            }
            acc[key].push(track);
            return acc;
        }, {} as Record<string, Track[]>);
    }, [tracks, activeTab]);

    const getColorClass = (key: string) => {
        if (activeTab === 'order') {
            return key.replace('-', '_'); // distributed, on_air, etc.
        } else {
            if (key === 'new') return 'new';
            if (['wave10', 'wave9', 'wave8'].includes(key)) return 'wave_blue';
            if (['wave7', 'wave6', 'wave5', 'wave4'].includes(key)) return 'wave_purple';
            return 'wave_pink';
        }
    };

    const toggleSection = (sectionKey: string) => {
        setExpandedSections(prev => ({
            ...prev,
            [sectionKey]: !prev[sectionKey]
        }));
    };

    const scrollToSection = (key: string) => {
        setActiveHeaderTab(key);
        const element = sectionRefs.current[key];
        if (element && containerRef.current) {
            const container = containerRef.current;
            const elementTop = element.offsetTop;
            const containerTop = container.offsetTop;
            // Учитываем высоту заголовка
            container.scrollTo({
                top: elementTop - containerTop - 0,
                behavior: 'smooth'
            });
        }
    };

    useEffect(() => {
        const container = containerRef.current;
        if (!container) return;

        const handleScroll = () => {
            const scrollPosition = container.scrollTop + 120; // Оффсет для триггера

            for (const key of currentGroupKeys) {
                const element = sectionRefs.current[key];
                if (element) {
                    const top = element.offsetTop - container.offsetTop;
                    const bottom = top + element.offsetHeight;

                    if (scrollPosition >= top && scrollPosition < bottom) {
                        setActiveHeaderTab(key);
                        break;
                    }
                }
            }
        };

        container.addEventListener('scroll', handleScroll);
        return () => container.removeEventListener('scroll', handleScroll);
    }, [currentGroupKeys]);

    const getGlobalTrackIndex = (trackId: number) => {
        return tracks.findIndex(t => t.id === trackId) + 1;
    };

    return window && (
        <div className={catalog.modalOverlay} onClick={(event) => event.target === event.currentTarget && hideWindow(false)}>
            <div className={catalog.modalContainer}>
                {/* Хедер */}
                <div className={header.modalHeader}>
                    <div className={header.headerTop}>
                        <span className={header.trackCount}>{tracks.length} треков</span>
                        <div className={header.statusTabs}>
                            {currentGroupKeys.map(key => {
                                const trackCount = groupedTracks[key]?.length || 0;
                                if (trackCount === 0) return null;
                                return (
                                    <button
                                        key={key}
                                        className={`${header.statusTab} ${header[getColorClass(key)]} ${
                                            activeHeaderTab === key ? header.active : header.inactive
                                        }`}
                                        onClick={() => scrollToSection(key)}
                                    >
                                        {getGroupLabel(key, currentGroupType)}
                                    </button>
                                );
                            })}
                        </div>
                    </div>
                </div>

                {/* Контролы */}
                <div className={header.controls}>
                    <div className={header.viewTabs}>
                        <button
                            className={`${header.viewTab} ${activeTab === 'order' ? header.active : ''}`}
                            onClick={() => setActiveTab('order')}
                        >
                            по порядку
                        </button>
                        <button
                            className={`${header.viewTab} ${activeTab === 'waves' ? header.active : ''}`}
                            onClick={() => setActiveTab('waves')}
                        >
                            по волнам
                        </button>
                    </div>
                    <input
                        type="text"
                        className={header.searchInput}
                        placeholder="Поиск по треку или нику"
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>

                {/* Список треков */}
                <div className={catalog.tracksContainer} ref={containerRef}>
                    {currentGroupKeys.map(key => {
                        const groupTracks = groupedTracks[key];
                        if (!groupTracks || groupTracks.length === 0) return null;

                        const maxVisible = 3;
                        const isExpanded = expandedSections[key];
                        const visibleTracks = isExpanded ? groupTracks : groupTracks.slice(0, maxVisible);
                        const hiddenCount = groupTracks.length - maxVisible;

                        return (
                            <div
                                key={key}
                                className={catalog.statusSection}
                                ref={el => sectionRefs.current[key] = el}
                            >
                                <div className={catalog.sectionHeader}>
                                    <button className={`${catalog.sectionTitle} ${catalog[getColorClass(key)]}`}>
                                        {getGroupLabel(key, currentGroupType)}
                                    </button>

                                    {hiddenCount > 0 && !isExpanded && (
                                        <button className={catalog.moreCount} onClick={() => toggleSection(key)}>
                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                                                <path d="M7 10l5 5 5-5z"/>
                                            </svg>
                                            ещё {hiddenCount}
                                        </button>
                                    )}
                                    {isExpanded && hiddenCount > 0 && (
                                        <button className={catalog.moreCount} onClick={() => toggleSection(key)}>
                                            <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor">
                                                <path d="M7 14l5-5 5 5z"/>
                                            </svg>
                                            свернуть
                                        </button>
                                    )}
                                </div>

                                <div className={catalog.tracksList}>
                                    {visibleTracks.map(track => (
                                        <TrackCard
                                            key={track.id}
                                            track={track}
                                            trackNumber={getGlobalTrackIndex(track.id)}
                                        />
                                    ))}
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>
        </div>
    )
}

export default OrderQueue;