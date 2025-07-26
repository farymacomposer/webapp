
import TrackCard from "./track-card";

export default function TrackList({ tracks }: { tracks: Track[] }) {
  return (
    <>
      {tracks.map((track: Track) => (
        <TrackCard key={track.id} track={track} />
      ))}
    </>
  );
}
