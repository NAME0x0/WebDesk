import { Carousel } from "./carousel";
import { WallpaperCard } from "./wallpaper-card";
import type { Wallpaper } from "@/lib/catalog";

export function GalleryTeaser({ items }: { items: Wallpaper[] }) {
  return (
    <Carousel>
      {items.map((w) => (
        <div key={w.id} className="flex-[0_0_72%] sm:flex-[0_0_40%] lg:flex-[0_0_25%]">
          <WallpaperCard w={w} />
        </div>
      ))}
    </Carousel>
  );
}
