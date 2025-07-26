import { SidebarFilters } from "@/components/shared";
import TrackList from "@/components/shared/track-list";

export default function CatalogPage() {
  const tracks = [
    {
      id: 1,
      title: "Ussewa",
      artist: "Ado",
      cover: "/images/ussewa.jpg",
      rating: "Гениально",
      likes: 6,
      comments: 13,
    },
    {
      id: 2,
      title: "Bohemian Rhapsody",
      artist: "Queen",
      cover: "/images/bohemian.jpg",
      rating: "Отлично",
      likes: 54,
      comments: 23,
    },
    // ... добавь свои треки
  ];
  return (
    <div className=" text-white min-h-screen">
      <div className="flex">
        <aside className="w-72 p-4 border-r border-gray-700">
          <SidebarFilters />
        </aside>
        <main className="flex-1 p-6 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          <TrackList tracks={tracks} />
        </main>
      </div>
    </div>
  );
}
