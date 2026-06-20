import { Reveal } from "./reveal";
import { site } from "@/lib/site";

export function OpenSource({ stars }: { stars: number }) {
  const items = [
    { k: "MIT", v: "Licensed" },
    { k: stars > 0 ? `${stars}` : "★", v: "GitHub stars" },
    { k: "C# / .NET 8", v: "WebView2" },
  ];
  return (
    <Reveal>
      <div className="grid items-center gap-10 md:grid-cols-2">
        <div>
          <div className="accent-text text-sm font-semibold uppercase tracking-[0.14em]">Open source</div>
          <h3 className="mt-3 font-display text-3xl font-bold sm:text-4xl">Built in the open. Yours to shape.</h3>
          <p className="mt-4 text-lg text-[var(--color-muted)]">
            Every line is on GitHub. Add a wallpaper to the Discover catalog with a single pull request, file an
            issue, or fork it. The whole thing is a few thousand lines of C# and a WebView2.
          </p>
          <div className="mt-7 flex flex-wrap gap-3">
            <a href={site.repoUrl} className="accent-bg rounded-xl px-5 py-2.5 font-semibold text-black transition-transform hover:scale-[1.03]">
              ★ Star on GitHub
            </a>
            <a href={`${site.repoUrl}/blob/main/catalog/catalog.json`} className="glass rounded-xl px-5 py-2.5 font-semibold transition-colors hover:border-[var(--accent)]">
              Contribute a wallpaper
            </a>
          </div>
        </div>
        <div className="grid grid-cols-3 gap-4">
          {items.map((it) => (
            <div key={it.v} className="rounded-2xl border border-[var(--color-line)] glass p-5 text-center">
              <div className="font-display text-2xl font-bold accent-text">{it.k}</div>
              <div className="mt-1 text-xs text-[var(--color-muted)]">{it.v}</div>
            </div>
          ))}
        </div>
      </div>
    </Reveal>
  );
}
