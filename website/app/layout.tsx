import type { Metadata } from "next";
import { Inter, Sora } from "next/font/google";
import "./globals.css";
import { AccentProvider } from "@/components/accent";
import { Nav } from "@/components/nav";
import { SmoothScroll } from "@/components/smooth-scroll";
import { site } from "@/lib/site";

const inter = Inter({ subsets: ["latin"], variable: "--font-inter", display: "swap" });
const sora = Sora({
  subsets: ["latin"],
  variable: "--font-sora",
  weight: ["400", "600", "700", "800"],
  display: "swap",
});

export const metadata: Metadata = {
  metadataBase: new URL(site.url),
  title: { default: "WebDesk — Your desktop, alive.", template: "%s · WebDesk" },
  description: site.description,
  openGraph: {
    title: "WebDesk — Your desktop, alive.",
    description: site.description,
    url: site.url,
    siteName: "WebDesk",
    type: "website",
  },
  twitter: { card: "summary_large_image", title: "WebDesk", description: site.description },
  icons: { icon: "/favicon.svg" },
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en" className={`${inter.variable} ${sora.variable}`}>
      <body className="grain">
        <AccentProvider>
          <SmoothScroll />
          <Nav />
          {children}
        </AccentProvider>
      </body>
    </html>
  );
}
