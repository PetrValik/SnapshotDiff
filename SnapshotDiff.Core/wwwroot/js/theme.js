export function applyTheme(theme) {
    const html = document.documentElement;
    html.classList.remove('light', 'dark');
    if (theme === 'light') html.classList.add('light');
    // dark = default, no class needed
}

export function prefersLight() {
    return window.matchMedia('(prefers-color-scheme: light)').matches;
}

export function watchSystemTheme(dotnetRef) {
    const mq = window.matchMedia('(prefers-color-scheme: light)');
    mq.addEventListener('change', e => {
        dotnetRef.invokeMethodAsync('OnSystemThemeChanged', e.matches ? 'light' : 'dark');
    });
}
