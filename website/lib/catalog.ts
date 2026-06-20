import { site } from "./site";

export type Wallpaper = {
  id: string;
  title: string;
  type: "url" | "html" | "video" | "image" | "shader";
  source: string;
  thumbnail?: string;
  tags?: string[];
  author?: string;
  audio?: boolean;
};

/** A few entries used if the live catalog can't be reached at build time. */
const FALLBACK: Wallpaper[] = [
  { id: "earth", title: "Earth — Live Wind Map", type: "url", source: "https://earth.nullschool.net", tags: ["weather"], author: "nullschool" },
  { id: "m1", title: "Mountain Vista", type: "image", source: "https://picsum.photos/id/1018/1280/720", thumbnail: "https://picsum.photos/id/1018/600/375", tags: ["nature"] },
  { id: "m2", title: "Forest Path", type: "image", source: "https://picsum.photos/id/1015/1280/720", thumbnail: "https://picsum.photos/id/1015/600/375", tags: ["nature"] },
  { id: "city", title: "City Lights", type: "image", source: "https://picsum.photos/id/1019/1280/720", thumbnail: "https://picsum.photos/id/1019/600/375", tags: ["urban"] },
];

/** Thumbnail to render in the gallery for any wallpaper type. */
export function thumbFor(w: Wallpaper): string {
  if (w.thumbnail) return w.thumbnail;
  if (w.type === "url") return `https://image.thum.io/get/width/600/${w.source}`;
  return w.source;
}

export async function getCatalog(): Promise<Wallpaper[]> {
  try {
    const res = await fetch(site.catalogRaw, { next: { revalidate: 3600 } });
    if (!res.ok) return FALLBACK;
    const data = await res.json();
    const list: Wallpaper[] = data.wallpapers ?? [];
    return list.length ? list : FALLBACK;
  } catch {
    return FALLBACK;
  }
}

/** Group wallpapers by their primary tag for the gallery carousels. */
export function groupByTag(items: Wallpaper[]): Record<string, Wallpaper[]> {
  const groups: Record<string, Wallpaper[]> = {};
  for (const w of items) {
    const tag = (w.tags && w.tags[0]) || "Other";
    const key = tag.charAt(0).toUpperCase() + tag.slice(1);
    (groups[key] ||= []).push(w);
  }
  return groups;
}
