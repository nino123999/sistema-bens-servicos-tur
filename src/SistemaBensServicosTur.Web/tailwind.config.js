/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Components/**/*.razor',
    './wwwroot/**/*.{html,js}',
  ],
  theme: {
    extend: {
      colors: {
        'primary':  '#4B7DAD',
        'on-dark':  '#FFFFFF',
        'main':     '#F0F4F8',
        'surface':  '#F7F8FA',
        'muted':    '#6B7280',
        'secondary':'#9CA3AF',
        'soft':     '#E2E8F0',
        'danger':   '#DC2626',
      },
    },
  },
  plugins: [],
}
