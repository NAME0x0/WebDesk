import { site } from "./site";

export type ReleaseInfo = {
  version: string;
  downloadUrl: string;
  assetSize: string;
  stars: number;
};

// If the API can't be reached, the download link still points at the latest
// release (GitHub's redirect), and the label just reads "latest".
const FALLBACK: ReleaseInfo = {
  version: "latest",
  downloadUrl: site.downloadLatest,
  assetSize: "",
  stars: 0,
};

function fmtBytes(n: number): string {
  if (!n) return "";
  return `${Math.round(n / (1024 * 1024))} MB`;
}

/**
 * Latest release version + size + star count. The download link is always the
 * "/releases/latest/download" redirect, so it stays current even if this fetch
 * is rate-limited or the page is cached. Revalidated every 10 minutes (ISR).
 */
export async function getReleaseInfo(): Promise<ReleaseInfo> {
  try {
    const headers: Record<string, string> = { Accept: "application/vnd.github+json" };
    // Optional: set GITHUB_TOKEN in Vercel to lift the unauthenticated rate limit.
    const token = process.env.GITHUB_TOKEN;
    if (token) headers.Authorization = `Bearer ${token}`;

    const opts = { headers, next: { revalidate: 600 } };

    const [relRes, repoRes] = await Promise.all([
      fetch(`https://api.github.com/repos/${site.repo}/releases/latest`, opts),
      fetch(`https://api.github.com/repos/${site.repo}`, opts),
    ]);

    if (!relRes.ok) return FALLBACK;
    const rel = await relRes.json();
    const repo = repoRes.ok ? await repoRes.json() : { stargazers_count: 0 };

    const exe = (rel.assets ?? []).find((a: { name: string }) => a.name.endsWith(".exe"));

    return {
      version: rel.tag_name ?? "latest",
      downloadUrl: site.downloadLatest,
      assetSize: exe?.size ? fmtBytes(exe.size) : "",
      stars: repo.stargazers_count ?? 0,
    };
  } catch {
    return FALLBACK;
  }
}
