export interface Order {
  id: number;
  title: string;
  user: string;
  img: string;
  category: string;
  price: number;
  youtubeLink: string | null;
  spotifyLink: string | null;
  comment: string | null;
}

export interface CategoryWithOrders {
  id: number;
  name: string;
  orders: Order[];
}
