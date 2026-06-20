"use client";

import { useEffect, useState } from "react";
import { AnimatePresence, motion } from "framer-motion";

const shots = [
  { src: "/shots/discover.png", label: "Discover" },
  { src: "/shots/wallhaven.png", label: "Wallhaven" },
  { src: "/shots/browser.png", label: "In-app browser" },
  { src: "/shots/library.png", label: "Playlists" },
  { src: "/shots/settings.png", label: "Settings" },
];

export function DeviceMockup() {
  const [i, setI] = useState(0);
  useEffect(() => {
    const reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    if (reduced) return;
    const t = setInterval(() => setI((v) => (v + 1) % shots.length), 3400);
    return () => clearInterval(t);
  }, []);

  return (
    <div className="accent-glow relative w-full overflow-hidden rounded-xl border border-[var(--color-line)] bg-[var(--color-panel)] shadow-2xl">
      <div className="flex items-center gap-1.5 border-b border-[var(--color-line)] bg-[var(--color-panel-2)] px-3 py-2">
        <span className="h-2.5 w-2.5 rounded-full bg-[#ff5f57]" />
        <span className="h-2.5 w-2.5 rounded-full bg-[#febc2e]" />
        <span className="h-2.5 w-2.5 rounded-full bg-[#28c840]" />
        <span className="ml-2 text-xs text-[var(--color-muted)]">WebDesk — {shots[i].label}</span>
      </div>
      <div className="relative aspect-[1900/1085]">
        <AnimatePresence>
          <motion.img
            key={shots[i].src}
            src={shots[i].src}
            alt={shots[i].label}
            className="absolute inset-0 h-full w-full object-cover"
            initial={{ opacity: 0, scale: 1.03 }}
            animate={{ opacity: 1, scale: 1 }}
            exit={{ opacity: 0 }}
            transition={{ duration: 0.8, ease: "easeInOut" }}
          />
        </AnimatePresence>
      </div>
    </div>
  );
}
