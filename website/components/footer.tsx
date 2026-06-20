import { Logo } from "./logo";
import { site } from "@/lib/site";

export function Footer() {
  return (
    <footer className="relative z-10 border-t border-[var(--color-line)] px-6 py-12">
      <div className="mx-auto flex max-w-6xl flex-col items-center justify-between gap-6 sm:flex-row">
        <div className="flex items-center gap-2.5">
          <Logo className="h-6 w-6" />
          <span className="font-display text-lg font-semibold">WebDesk</span>
          <span className="ml-2 text-sm text-[var(--color-muted)]">Free &amp; open source · MIT</span>
        </div>
        <nav className="flex flex-wrap items-center gap-x-6 gap-y-2 text-sm text-[var(--color-muted)]">
          <a className="transition-colors hover:text-[var(--color-text)]" href="/#features">Features</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href="/gallery">Gallery</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href={site.repoUrl}>GitHub</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href={site.issuesUrl}>Issues</a>
          <a className="transition-colors hover:text-[var(--color-text)]" href={site.releasesUrl}>Download</a>
        </nav>
      </div>
      <p className="mx-auto mt-8 max-w-6xl text-center text-xs text-[var(--color-muted)] sm:text-left">
        Not affiliated with Wallpaper Engine or Lively Wallpaper. Windows 10/11.
      </p>
    </footer>
  );
}
