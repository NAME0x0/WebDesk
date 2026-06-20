/** @type {import('next').NextConfig} */
const nextConfig = {
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: 'picsum.photos' },
      { protocol: 'https', hostname: 'fastly.picsum.photos' },
      { protocol: 'https', hostname: 'w.wallhaven.cc' },
      { protocol: 'https', hostname: 'th.wallhaven.cc' },
      { protocol: 'https', hostname: 'image.thum.io' },
    ],
  },
};

export default nextConfig;
