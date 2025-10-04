import "@/styles/global.css";
import Header from "@/components/header/Header";

export const metadata = {
  title: "Composer",
  description: "Composer & Streamer site",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <Header />
        <main className="container">{children}</main>
      </body>
    </html>
  );
}
