import { type Wallpaper, thumbFor } from "@/lib/catalog";

export function WallpaperCard({ w }: { w: Wallpaper }) {
  return (
    <div className="accent-ring overflow-hidden rounded-xl border border-[var(--color-line)] bg-[var(--color-panel)] transition-transform hover:-translate-y-1">
      <div className="relative aspect-[16/10] bg-[var(--color-panel-2)]">
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={thumbFor(w)} alt={w.title} loading="lazy" className="h-full w-full object-cover" />
        <span className="absolute left-2 top-2 rounded bg-black/60 px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide text-white backdrop-blur">
          {w.type}
        </span>
        {w.audio && (
          <span className="accent-text absolute right-2 top-2 rounded bg-black/60 px-2 py-0.5 text-[10px] font-semibold backdrop-blur">
            audio
          </span>
        )}
      </div>
      <div className="p-3">
        <div className="truncate text-sm font-semibold">{w.title}</div>
        {w.author && <div className="truncate text-xs text-[var(--color-muted)]">{w.author}</div>}
      </div>
    </div>
  );
}
