import { Reveal } from "./reveal";

export type Feature = {
  kicker: string;
  title: string;
  body: string;
  img: string;
};

export function FeatureStory({ feature, reverse }: { feature: Feature; reverse?: boolean }) {
  return (
    <Reveal>
      <div className="grid items-center gap-10 md:grid-cols-2">
        <div className={reverse ? "md:order-2" : ""}>
          <div className="accent-text text-sm font-semibold uppercase tracking-[0.14em]">{feature.kicker}</div>
          <h3 className="mt-3 font-display text-3xl font-bold leading-tight sm:text-4xl">{feature.title}</h3>
          <p className="mt-4 text-lg leading-relaxed text-[var(--color-muted)]">{feature.body}</p>
        </div>
        <div className={reverse ? "md:order-1" : ""}>
          <div className="accent-glow overflow-hidden rounded-xl border border-[var(--color-line)] bg-[var(--color-panel)]">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img src={feature.img} alt={feature.title} loading="lazy" className="w-full" />
          </div>
        </div>
      </div>
    </Reveal>
  );
}
