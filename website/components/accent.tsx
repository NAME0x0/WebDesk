"use client";

import { createContext, useContext, useEffect, useState, type ReactNode } from "react";
import { accentSwatches } from "@/lib/site";

type Ctx = { accent: string; setAccent: (c: string) => void };
const AccentCtx = createContext<Ctx>({ accent: "#38bdf8", setAccent: () => {} });
export const useAccent = () => useContext(AccentCtx);

export function AccentProvider({ children }: { children: ReactNode }) {
  const [accent, setAccentState] = useState("#38bdf8");

  useEffect(() => {
    const saved = localStorage.getItem("webdesk-accent");
    if (saved) {
      setAccentState(saved);
      document.documentElement.style.setProperty("--accent", saved);
    }
  }, []);

  const setAccent = (c: string) => {
    setAccentState(c);
    document.documentElement.style.setProperty("--accent", c);
    localStorage.setItem("webdesk-accent", c);
  };

  return <AccentCtx.Provider value={{ accent, setAccent }}>{children}</AccentCtx.Provider>;
}

export function AccentPicker({ compact = false }: { compact?: boolean }) {
  const { accent, setAccent } = useAccent();
  return (
    <div className="flex items-center gap-1.5">
      {!compact && <span className="mr-1 text-xs text-[var(--color-muted)]">Glow</span>}
      {accentSwatches.map((s) => (
        <button
          key={s.value}
          aria-label={`Set accent ${s.name}`}
          onClick={() => setAccent(s.value)}
          className="h-5 w-5 rounded-full border transition-transform hover:scale-110"
          style={{
            background: s.value,
            borderColor: accent === s.value ? "#fff" : "transparent",
            boxShadow: accent === s.value ? `0 0 12px ${s.value}` : "none",
          }}
        />
      ))}
      <label
        className="relative h-5 w-5 cursor-pointer overflow-hidden rounded-full border border-[var(--color-line)]"
        title="Custom color"
        style={{ background: "conic-gradient(red, yellow, lime, cyan, blue, magenta, red)" }}
      >
        <input
          type="color"
          value={accent}
          onChange={(e) => setAccent(e.target.value)}
          className="absolute inset-0 cursor-pointer opacity-0"
        />
      </label>
    </div>
  );
}
