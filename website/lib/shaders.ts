export const VERT = `
attribute vec2 p;
void main() { gl_Position = vec4(p, 0.0, 1.0); }
`;

// Accent-tinted flowing aurora. uAccent is the live accent color (0..1 rgb).
export const FRAG = `
precision highp float;
uniform vec2 iResolution;
uniform float iTime;
uniform vec3 uAccent;

float hash(vec2 p){ return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float noise(vec2 p){
  vec2 i = floor(p), f = fract(p);
  vec2 u = f*f*(3.0-2.0*f);
  return mix(mix(hash(i+vec2(0,0)), hash(i+vec2(1,0)), u.x),
             mix(hash(i+vec2(0,1)), hash(i+vec2(1,1)), u.x), u.y);
}
float fbm(vec2 p){
  float v = 0.0, a = 0.5;
  for(int i=0;i<5;i++){ v += a*noise(p); p *= 2.0; a *= 0.5; }
  return v;
}

void main(){
  vec2 uv = gl_FragCoord.xy / iResolution.xy;
  vec2 p = (gl_FragCoord.xy * 2.0 - iResolution.xy) / iResolution.y;
  float t = iTime * 0.05;

  // Two drifting aurora bands.
  float f1 = fbm(p * 1.3 + vec2(t, t * 0.4));
  float f2 = fbm(p * 2.0 - vec2(t * 0.6, t));
  float band1 = smoothstep(0.55, 0.0, abs(p.y + 0.6 + (f1 - 0.5) * 1.4));
  float band2 = smoothstep(0.5, 0.0, abs(p.y - 0.2 + (f2 - 0.5) * 1.6));

  vec3 base = vec3(0.02, 0.024, 0.035);
  vec3 accent = uAccent;
  vec3 accent2 = mix(uAccent, vec3(0.6, 0.4, 1.0), 0.35);

  vec3 col = base;
  col += accent * band1 * 0.85;
  col += accent2 * band2 * 0.55;

  // Soft haze toward the top, vignette toward edges.
  col += accent * 0.05 * (1.0 - uv.y);
  col *= 1.0 - 0.35 * length(p) * 0.4;

  gl_FragColor = vec4(col, 1.0);
}
`;
