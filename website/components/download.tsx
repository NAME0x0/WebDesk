import { Reveal } from "./reveal";
import { site } from "@/lib/site";

export function Download({
  version,
  size,
  downloadUrl,
}: {
  version: string;
  size: string;
  downloadUrl: string;
}) {
  return (
    <Reveal>
      <div className="accent-glow relative overflow-hidden rounded-3xl border border-[var(--color-line)] glass px-6 py-16 text-center sm:px-12">
        <div
          className="pointer-events-none absolute inset-0 opacity-30"
          style={{ background: "radial-gradient(ellipse 50% 60% at 50% 0%, var(--accent), transparent 70%)" }}
        />
        <h2 className="font-display text-4xl font-extrabold sm:text-5xl">Get WebDesk free</h2>
        <p className="mx-auto mt-4 max-w-md text-[var(--color-muted)]">
          One portable <code className="rounded bg-[var(--color-panel-2)] px-1.5 py-0.5 text-sm">.exe</code>. No installer,
          no account. The Edge WebView2 runtime ships with Windows 11.
        </p>
        <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
          <a
            href={downloadUrl}
            className="accent-bg rounded-xl px-7 py-3.5 text-lg font-semibold text-black transition-transform hover:scale-[1.04]"
          >
            ↓ Download {version}
          </a>
          <a href={site.repoUrl} className="glass rounded-xl px-6 py-3.5 font-semibold transition-colors hover:border-[var(--accent)]">
            View source
          </a>
        </div>
        <p className="mt-6 text-sm text-[var(--color-muted)]">
          Windows 10 / 11 · {size} · SHA-256 verified ·{" "}
          <a className="accent-text hover:underline" href={site.releasesUrl}>
            all releases
          </a>
        </p>
      </div>
    </Reveal>
  );
}
