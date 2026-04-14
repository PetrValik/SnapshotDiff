/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.razor',
    './**/*.html',
    './**/*.cs',
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        'bg-main': 'var(--bg-main)',
        'bg-card': 'var(--bg-card)',
        'bg-topbar': 'var(--bg-topbar)',
        'border-clr': 'var(--border)',
        'txt': 'var(--text)',
        'txt-secondary': 'var(--text-secondary)',
        'txt-muted': 'var(--text-muted)',
        'accent': 'var(--accent)',
        'accent-hover': 'var(--accent-hover)',
        'added': 'var(--color-added)',
        'modified': 'var(--color-modified)',
        'deleted': 'var(--color-deleted)',
      },
    },
  },
  plugins: [],
}
