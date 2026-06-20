"use client";

import { motion, useMotionValue, useSpring, useTransform } from "framer-motion";
import { ShaderCanvas } from "./shader-canvas";
import { DeviceMockup } from "./device-mockup";
import { AccentPicker } from "./accent";
import { site } from "@/lib/site";

const EASE = [0.16, 1, 0.3, 1] as const;

export function Hero({ version, downloadUrl }: { version: string; downloadUrl: string }) {
  const mx = useMotionValue(0);
  const my = useMotionValue(0);
  const rotateX = useSpring(useTransform(my, [-0.5, 0.5], [6, -6]), { stiffness: 120, damping: 18 });
  const rotateY = useSpring(useTransform(mx, [-0.5, 0.5], [-8, 8]), { stiffness: 120, damping: 18 });

  const onMove = (e: React.MouseEvent) => {
    const r = e.currentTarget.getBoundingClientRect();
    mx.set((e.clientX - r.left) / r.width - 0.5);
    my.set((e.clientY - r.top) / r.height - 0.5);
  };

  return (
    <section className="relative overflow-hidden px-6 pb-24 pt-32" onMouseMove={onMove}>
      <ShaderCanvas className="absolute inset-0 -z-20 h-full w-full opacity-80" />
      <div className="hero-grid absolute inset-0 -z-10" />
      <div className="absolute inset-x-0 bottom-0 -z-10 h-72 bg-gradient-to-b from-transparent to-[var(--color-bg)]" />

      <div className="mx-auto max-w-6xl">
        <motion.div
          initial={{ opacity: 0, y: 22 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.8, ease: EASE }}
          className="mx-auto max-w-3xl text-center"
        >
          <span className="glass inline-flex items-center gap-2 rounded-full px-3 py-1 text-xs text-[var(--color-muted)]">
            <span className="accent-bg h-1.5 w-1.5 rounded-full" /> Free &amp; open source · {version}
          </span>

          <h1 className="mt-6 font-display text-5xl font-extrabold leading-[1.04] sm:text-7xl">
            Your desktop,
            <br />
            <span className="accent-text">alive.</span>
          </h1>

          <p className="mx-auto mt-6 max-w-xl text-lg text-[var(--color-muted)]">
            Turn any website, video, image, or WebGL shader into your live Windows wallpaper.
            Browse the web for one, or pick from a built-in catalog.
          </p>

          <div className="mt-8 flex flex-wrap items-center justify-center gap-3">
            <a
              href={downloadUrl}
              className="accent-bg accent-glow rounded-xl px-6 py-3 font-semibold text-black transition-transform hover:scale-[1.04]"
            >
              ↓ Download for Windows
            </a>
            <a
              href={site.repoUrl}
              className="glass rounded-xl px-6 py-3 font-semibold transition-colors hover:border-[var(--accent)]"
            >
              ★ Star on GitHub
            </a>
          </div>

          <div className="mt-7 flex justify-center">
            <AccentPicker />
          </div>
        </motion.div>

        <motion.div
          style={{ perspective: 1200 }}
          initial={{ opacity: 0, y: 44 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.9, delay: 0.15, ease: EASE }}
          className="mt-16"
        >
          <motion.div style={{ rotateX, rotateY }} className="animate-float mx-auto max-w-4xl">
            <DeviceMockup />
          </motion.div>
        </motion.div>
      </div>
    </section>
  );
}
