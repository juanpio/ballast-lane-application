/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{js,ts,jsx,tsx}',
    '../Views/**/*.cshtml',
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ["'Inter Variable'", 'system-ui', "'Segoe UI'", 'Roboto', 'sans-serif'],
        heading: ["'Inter Variable'", 'system-ui', "'Segoe UI'", 'Roboto', 'sans-serif'],
        mono: ["'JetBrains Mono Variable'", 'ui-monospace', 'Consolas', 'monospace'],
      },
      fontSize: {
        // 10px anchor — smaller than Tailwind's default xs (12px)
        '2xs': ['0.625rem', { lineHeight: '1.4' }],
      },
    },
  },
  plugins: [],
}

