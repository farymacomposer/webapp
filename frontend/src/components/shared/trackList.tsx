import { Track } from "@/types";
import { TrackCard } from "./trackCard";

export default function TrackList({ tracks }: { tracks: Track[] }) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      {tracks.map((track) => (
        <TrackCard key={track.id} track={track} />
      ))}
    </div>
  );
}
