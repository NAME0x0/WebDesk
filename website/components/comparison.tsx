import { Reveal } from "./reveal";

const rows: { label: string; webdesk: string | boolean; we: string | boolean; lively: string | boolean }[] = [
  { label: "Price", webdesk: "Free", we: "~$4", lively: "Free" },
  { label: "Open source", webdesk: true, we: false, lively: true },
  { label: "Website / URL wallpapers", webdesk: true, we: "Limited", lively: true },
  { label: "In-app browser + capture", webdesk: true, we: false, lively: false },
  { label: "WebGL shaders", webdesk: true, we: true, lively: true },
  { label: "Audio-reactive (any wallpaper)", webdesk: true, we: "Scenes only", lively: "Limited" },
  { label: "Per-monitor wallpapers", webdesk: true, we: true, lively: true },
  { label: "No Steam / no account", webdesk: true, we: false, lively: true },
  { label: "Single portable .exe", webdesk: true, we: false, lively: false },
];

function Cell({ v, accent }: { v: string | boolean; accent?: boolean }) {
  if (v === true)
    return <span className={accent ? "accent-text font-semibold" : "text-emerald-400"}>✓</span>;
  if (v === false) return <span className="text-[var(--color-muted)]">—</span>;
  return <span className={accent ? "accent-text font-medium" : "text-[var(--color-text)]"}>{v}</span>;
}

export function Comparison() {
  return (
    <Reveal>
      <div className="overflow-hidden rounded-2xl border border-[var(--color-line)] glass">
        <table className="w-full text-left text-sm sm:text-base">
          <thead>
            <tr className="border-b border-[var(--color-line)]">
              <th className="px-5 py-4 font-medium text-[var(--color-muted)]"></th>
              <th className="px-5 py-4 font-display font-bold accent-text">WebDesk</th>
              <th className="px-5 py-4 font-medium text-[var(--color-muted)]">Wallpaper Engine</th>
              <th className="px-5 py-4 font-medium text-[var(--color-muted)]">Lively</th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r, i) => (
              <tr key={r.label} className={i % 2 ? "bg-white/[0.015]" : ""}>
                <td className="px-5 py-3.5 text-[var(--color-muted)]">{r.label}</td>
                <td className="px-5 py-3.5"><Cell v={r.webdesk} accent /></td>
                <td className="px-5 py-3.5"><Cell v={r.we} /></td>
                <td className="px-5 py-3.5"><Cell v={r.lively} /></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </Reveal>
  );
}
