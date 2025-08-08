import { Track } from "@/lib/models";
import Image from "next/image";

interface TrackCardProps {
  track: Track;
}

export const TrackCard: React.FC<TrackCardProps> = ({ track }) => {
  return (
    <div className="bg-header h-[380] rounded-lg p-3 flex flex-col">
      <Image
        src={track.cover}
        alt={track.title}
        width={300}
        height={300}
        className="rounded"
      />
      <h3 className="mt-2 text-lg font-semibold">{track.title}</h3>
      <p className="text-sm text-gray-400">{track.artist}</p>
      <div className="mt-1 text-xs text-gray-500">Оценка: {track.rating}</div>
      <div className="mt-1 text-xs text-gray-500">
        ❤️ {track.likes} 💬 {track.comments}
      </div>
    </div>
  );
};
