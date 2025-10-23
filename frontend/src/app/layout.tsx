import "@/styles/global.css";
import Header from "@/components/widgets/Header";

export const metadata = {
  title: "Composer",
  description: "Composer & Streamer site",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body className="bg-black text-white min-h-screen flex flex-col">
        <Header />
        <main className="flex-1 w-full">{children}</main>
      </body>
    </html>
  );
}
