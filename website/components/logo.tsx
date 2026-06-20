export function Logo({ className = "", accent = false }: { className?: string; accent?: boolean }) {
  return (
    <svg
      viewBox="0 0 256 256"
      fill="none"
      stroke={accent ? "var(--accent)" : "currentColor"}
      strokeWidth={9}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={className}
      aria-hidden
    >
      <rect x="38" y="48" width="180" height="128" rx="14" />
      <line x1="116" y1="176" x2="116" y2="196" />
      <line x1="140" y1="176" x2="140" y2="196" />
      <line x1="92" y1="200" x2="164" y2="200" />
      <circle cx="128" cy="112" r="46" />
      <ellipse cx="128" cy="112" rx="18" ry="46" />
      <line x1="82" y1="112" x2="174" y2="112" />
      <line x1="87" y1="92" x2="169" y2="92" />
      <line x1="87" y1="132" x2="169" y2="132" />
    </svg>
  );
}
