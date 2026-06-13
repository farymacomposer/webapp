export interface Order {
  id: number;
  title: string;
  user: string;
  img: string;
  waveShortName: string;
  price: number;
  youtubeLink: string | null;
  spotifyLink: string | null;
  comment: string | null;
}
