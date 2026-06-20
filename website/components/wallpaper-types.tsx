"use client";

import { Carousel } from "./carousel";

const types = [
  { label: "Websites", blurb: "Any live URL, running as your wallpaper.", glyph: "🌐", grad: "from-sky-500/25 to-blue-700/5" },
  { label: "Video", blurb: "Loop .mp4 / .webm behind your icons.", glyph: "▶", grad: "from-rose-500/25 to-purple-700/5" },
  { label: "Images", blurb: "Stills in crisp full resolution.", glyph: "🖼", grad: "from-emerald-500/25 to-teal-700/5" },
  { label: "WebGL shaders", blurb: "GPU shaders that react to your audio.", glyph: "◈", grad: "from-fuchsia-500/25 to-indigo-700/5" },
  { label: "Local HTML", blurb: "Bring your own interactive scenes.", glyph: "</>", grad: "from-amber-500/25 to-orange-700/5" },
];

export function WallpaperTypes() {
  return (
    <Carousel>
      {types.map((t) => (
        <div key={t.label} className="flex-[0_0_80%] sm:flex-[0_0_44%] lg:flex-[0_0_29%]">
          <div
            className={`accent-ring flex h-56 flex-col justify-between rounded-2xl border border-[var(--color-line)] bg-gradient-to-br ${t.grad} p-6 transition-colors`}
          >
            <div className="text-4xl">{t.glyph}</div>
            <div>
              <div className="font-display text-xl font-semibold">{t.label}</div>
              <p className="mt-1 text-sm text-[var(--color-muted)]">{t.blurb}</p>
            </div>
          </div>
        </div>
      ))}
    </Carousel>
  );
}
