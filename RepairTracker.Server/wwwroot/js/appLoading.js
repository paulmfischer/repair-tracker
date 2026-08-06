window.hideAppLoadingScreen = function () {
    document.getElementById('app-loading')?.remove();
};

window.getGitCommitMetaTag = function () {
    return document.querySelector('meta[name="git-commit"]')?.getAttribute('content') ?? '';
};

if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => {
        navigator.serviceWorker.register('/service-worker.js').catch(() => {
            // Offline support just won't kick in this session; the app still works online.
        });
    });
}
