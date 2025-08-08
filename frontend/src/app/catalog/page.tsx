import { SidebarFilters } from "@/components/shared";
import TrackList from "@/components/shared/track-list";

export default function CatalogPage() {
  const tracks = [
    {
      id: 1,
      title: "Ussewa",
      artist: "Ado",
      cover:
        "https://lastfm.freetls.fastly.net/i/u/ar0/38ee277844d2211f8cc7db9791639592.jpg",
      rating: "Гениально",
      likes: 6,
      comments: 13,
    },
    {
      id: 2,
      title: "Bohemian Rhapsody",
      artist: "Queen",
      cover:
        "https://avatars.dzeninfra.ru/get-zen_doc/9505890/pub_641f6c668e063c1a40faae0a_641f6d09d4b1f54fcf543e6f/scale_1200",
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
