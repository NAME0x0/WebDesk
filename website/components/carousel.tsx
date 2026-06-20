"use client";

import useEmblaCarousel from "embla-carousel-react";
import Autoplay from "embla-carousel-autoplay";
import { useCallback, type ReactNode } from "react";

export function Carousel({
  children,
  autoplay = true,
}: {
  children: ReactNode;
  autoplay?: boolean;
}) {
  const [ref, api] = useEmblaCarousel(
    { loop: true, align: "start", dragFree: true },
    autoplay ? [Autoplay({ delay: 3800, stopOnInteraction: false })] : []
  );
  const prev = useCallback(() => api?.scrollPrev(), [api]);
  const next = useCallback(() => api?.scrollNext(), [api]);

  return (
    <div className="group relative">
      <div className="overflow-hidden" ref={ref}>
        <div className="flex gap-4 px-1 py-1">{children}</div>
      </div>
      <button
        aria-label="Previous"
        onClick={prev}
        className="glass absolute left-1 top-1/2 hidden h-10 w-10 -translate-y-1/2 items-center justify-center rounded-full text-xl opacity-0 transition-opacity group-hover:opacity-100 md:flex"
      >
        ‹
      </button>
      <button
        aria-label="Next"
        onClick={next}
        className="glass absolute right-1 top-1/2 hidden h-10 w-10 -translate-y-1/2 items-center justify-center rounded-full text-xl opacity-0 transition-opacity group-hover:opacity-100 md:flex"
      >
        ›
      </button>
    </div>
  );
}
