import type { Metadata } from "next";
import { Manrope } from "next/font/google";
import "./globals.css";
import { Header } from "@/components/shared";

const manrope = Manrope({
  variable: "--font-manrope",
  subsets: ["cyrillic"],
  weight: ["400", "500", "600", "700", "800"],
});

export const metadata: Metadata = {
  title: "Faryma App",
  description: "Faryma Composer",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html className="min-h-screen " lang="en">
      <body
        className={`${manrope.variable} ${manrope.variable} antialiased bg-background`}
      >
        <Header className="sticky top-0 bg-header s" />
        {children}
      </body>
    </html>
  );
}
