import "@/styles/global.css";

export const metadata = {
  title: "Faryma Composer",
  description: "Frontend for Twitch OAuth login",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <main className="container">{children}</main>
      </body>
    </html>
  );
}
