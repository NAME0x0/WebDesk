import { site } from "./site";

export type ReleaseInfo = {
  version: string;
  downloadUrl: string;
  assetSize: string;
  stars: number;
};

const FALLBACK: ReleaseInfo = {
  version: "v2.0.0",
  downloadUrl: site.releasesUrl,
  assetSize: "~164 MB",
  stars: 0,
};

function fmtBytes(n: number): string {
  if (!n) return "";
  const mb = n / (1024 * 1024);
  return `${Math.round(mb)} MB`;
}

/** Latest release + star count, fetched at build time (revalidated hourly). */
export async function getReleaseInfo(): Promise<ReleaseInfo> {
  try {
    const opts = {
      headers: { Accept: "application/vnd.github+json" },
      next: { revalidate: 3600 },
    };

    const [relRes, repoRes] = await Promise.all([
      fetch(`https://api.github.com/repos/${site.repo}/releases/latest`, opts),
      fetch(`https://api.github.com/repos/${site.repo}`, opts),
    ]);

    if (!relRes.ok) return FALLBACK;
    const rel = await relRes.json();
    const repo = repoRes.ok ? await repoRes.json() : { stargazers_count: 0 };

    const exe =
      (rel.assets ?? []).find((a: { name: string }) => a.name.endsWith(".exe")) ??
      (rel.assets ?? [])[0];

    return {
      version: rel.tag_name ?? FALLBACK.version,
      downloadUrl: exe?.browser_download_url ?? site.releasesUrl,
      assetSize: exe?.size ? fmtBytes(exe.size) : FALLBACK.assetSize,
      stars: repo.stargazers_count ?? 0,
    };
  } catch {
    return FALLBACK;
  }
}
