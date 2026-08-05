window.hideAppLoadingScreen = function () {
    document.getElementById('app-loading')?.remove();
};

window.getGitCommitMetaTag = function () {
    return document.querySelector('meta[name="git-commit"]')?.getAttribute('content') ?? '';
};
