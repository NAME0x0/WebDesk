import type { Metadata } from "next";
import { getCatalog, groupByTag } from "@/lib/catalog";
import { Carousel } from "@/components/carousel";
import { WallpaperCard } from "@/components/wallpaper-card";
import { Footer } from "@/components/footer";
import { Reveal } from "@/components/reveal";

export const metadata: Metadata = {
  title: "Gallery",
  description: "Browse the live WebDesk wallpaper catalog — websites, images, video, and audio-reactive shaders.",
};

export default async function Gallery() {
  const catalog = await getCatalog();
  const groups = groupByTag(catalog);

  return (
    <main className="relative">
      <section className="relative z-10 mx-auto max-w-6xl px-6 pb-8 pt-36">
        <Reveal>
          <h1 className="font-display text-5xl font-extrabold sm:text-6xl">Gallery</h1>
          <p className="mt-4 max-w-xl text-lg text-[var(--color-muted)]">
            Live from the WebDesk catalog. Set any of these from inside the app, or add your own with a pull request.
          </p>
        </Reveal>
      </section>

      <div className="mx-auto flex max-w-6xl flex-col gap-16 px-6 pb-24">
        {Object.entries(groups).map(([tag, items]) => (
          <Reveal key={tag}>
            <h2 className="mb-5 font-display text-2xl font-bold">{tag}</h2>
            <Carousel autoplay={false}>
              {items.map((w) => (
                <div key={w.id} className="flex-[0_0_72%] sm:flex-[0_0_40%] lg:flex-[0_0_25%]">
                  <WallpaperCard w={w} />
                </div>
              ))}
            </Carousel>
          </Reveal>
        ))}
      </div>

      <Footer />
    </main>
  );
}
