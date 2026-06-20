"use client";

import { useEffect, useState } from "react";
import { Logo } from "./logo";
import { AccentPicker } from "./accent";
import { site } from "@/lib/site";

export function Nav() {
  const [scrolled, setScrolled] = useState(false);
  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 16);
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <header
      className={`fixed inset-x-0 top-0 z-50 transition-all duration-300 ${
        scrolled ? "py-2" : "py-4"
      }`}
    >
      <div
        className={`mx-auto flex max-w-6xl items-center justify-between rounded-2xl px-4 py-2.5 transition-all duration-300 ${
          scrolled ? "glass" : "bg-transparent"
        }`}
      >
        <a href="/" className="flex items-center gap-2.5">
          <Logo className="h-7 w-7" />
          <span className="font-display text-lg font-semibold tracking-tight">WebDesk</span>
        </a>

        <nav className="hidden items-center gap-7 text-sm text-[var(--color-muted)] md:flex">
          <a className="transition-colors hover:text-[var(--color-text)]" href="/#features">Features</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href="/#compare">Compare</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href="/gallery">Gallery</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href={site.repoUrl}>GitHub</a>
        </nav>

        <div className="flex items-center gap-3">
          <div className="hidden sm:block">
            <AccentPicker compact />
          </div>
          <a
            href="/#download"
            className="accent-bg rounded-lg px-3.5 py-2 text-sm font-semibold text-black transition-transform hover:scale-[1.03]"
          >
            Download
          </a>
        </div>
      </div>
    </header>
  );
}
