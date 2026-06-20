import { getReleaseInfo } from "@/lib/github";
import { getCatalog } from "@/lib/catalog";
import { Hero } from "@/components/hero";
import { WallpaperTypes } from "@/components/wallpaper-types";
import { FeatureStory, type Feature } from "@/components/feature-story";
import { Comparison } from "@/components/comparison";
import { GalleryTeaser } from "@/components/gallery-teaser";
import { Download } from "@/components/download";
import { OpenSource } from "@/components/open-source";
import { Footer } from "@/components/footer";
import { Reveal } from "@/components/reveal";

const features: Feature[] = [
  {
    kicker: "In-app browser",
    title: "Browse the web. Click. It's your wallpaper.",
    body: "A real browser lives inside WebDesk. Open Moewalls, Unsplash, X, anywhere — then set the video, the image, or the whole live page as your wallpaper. No downloads, no scraping.",
    img: "/shots/browser.png",
  },
  {
    kicker: "Discover + Wallhaven",
    title: "A built-in catalog, plus the web's wallpaper sites.",
    body: "Browse a community catalog that updates without a new release, and search Wallhaven right inside the app. Star what you like and save it to your library.",
    img: "/shots/wallhaven.png",
  },
  {
    kicker: "Shaders + audio",
    title: "Wallpapers that move to your music.",
    body: "Run GPU WebGL shaders as your wallpaper, with uniforms driven live by your system audio. Bass, mids, and treble feed the visuals in real time.",
    img: "/shots/discover.png",
  },
  {
    kicker: "Playlists + per-monitor",
    title: "Every screen its own scene. Rotate on a timer.",
    body: "Give each monitor a different wallpaper, or build a playlist that shuffles on an interval. Global hotkeys skip, pause, and mute without leaving your game.",
    img: "/shots/library.png",
  },
];

function Section({ id, children }: { id?: string; children: React.ReactNode }) {
  return (
    <section id={id} className="relative z-10 mx-auto max-w-6xl px-6 py-20 sm:py-28">
      {children}
    </section>
  );
}

function Heading({ kicker, title }: { kicker: string; title: string }) {
  return (
    <Reveal className="mb-12 max-w-2xl">
      <div className="accent-text text-sm font-semibold uppercase tracking-[0.14em]">{kicker}</div>
      <h2 className="mt-3 font-display text-3xl font-bold sm:text-4xl">{title}</h2>
    </Reveal>
  );
}

export default async function Home() {
  const [release, catalog] = await Promise.all([getReleaseInfo(), getCatalog()]);

  return (
    <main>
      <Hero version={release.version} downloadUrl={release.downloadUrl} />

      <Section id="features">
        <Heading kicker="Anything the web can render" title="Five kinds of wallpaper, one app." />
        <WallpaperTypes />
      </Section>

      <Section>
        <div className="flex flex-col gap-24">
          {features.map((f, i) => (
            <FeatureStory key={f.title} feature={f} reverse={i % 2 === 1} />
          ))}
        </div>
      </Section>

      <Section id="compare">
        <Heading kicker="The honest comparison" title="Everything you'd pay for. Plus things you can't buy. Free." />
        <Comparison />
      </Section>

      <Section>
        <div className="mb-12 flex items-end justify-between gap-6">
          <div className="max-w-2xl">
            <div className="accent-text text-sm font-semibold uppercase tracking-[0.14em]">Gallery</div>
            <h2 className="mt-3 font-display text-3xl font-bold sm:text-4xl">A taste of what&apos;s inside.</h2>
          </div>
          <a href="/gallery" className="accent-text hidden shrink-0 text-sm font-semibold hover:underline sm:block">
            Explore the gallery →
          </a>
        </div>
        <Reveal>
          <GalleryTeaser items={catalog.slice(0, 10)} />
        </Reveal>
      </Section>

      <Section>
        <OpenSource stars={release.stars} />
      </Section>

      <Section id="download">
        <Download version={release.version} size={release.assetSize} downloadUrl={release.downloadUrl} />
      </Section>

      <Footer />
    </main>
  );
}
