// Bump this whenever the caching strategy changes, to drop stale caches on activate.
const CACHE_NAME = 'repair-tracker-v1';

// There's no static index.html to precache (this is a server-rendered Blazor Web App
// host page, not a standalone WASM app) - so instead of precaching a fixed asset list,
// assets are cached lazily as they're actually requested, and the most recent successful
// navigation response is kept under this fixed key as the offline fallback shell.
const SHELL_CACHE_KEY = new Request('/offline-shell');

const CACHEABLE_PATH_PREFIXES = ['/_framework/', '/_content/', '/js/', '/icons/'];
const CACHEABLE_PATHS = ['/app.css', '/favicon.png', '/manifest.json'];

self.addEventListener('install', () => {
    self.skipWaiting();
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (event) => {
    const { request } = event;
    if (request.method !== 'GET') {
        return;
    }

    const url = new URL(request.url);
    if (url.origin !== self.location.origin || url.pathname.startsWith('/api/')) {
        return;
    }

    if (request.mode === 'navigate') {
        event.respondWith(handleNavigate(request));
        return;
    }

    const isCacheable = CACHEABLE_PATH_PREFIXES.some((prefix) => url.pathname.startsWith(prefix))
        || CACHEABLE_PATHS.includes(url.pathname);
    if (isCacheable) {
        event.respondWith(cacheFirst(request));
    }
});

async function handleNavigate(request) {
    const cache = await caches.open(CACHE_NAME);
    try {
        const response = await fetch(request);
        if (response && response.ok) {
            cache.put(SHELL_CACHE_KEY, response.clone());
        }
        return response;
    } catch {
        const cachedShell = await cache.match(SHELL_CACHE_KEY);
        if (cachedShell) {
            return cachedShell;
        }
        throw new Error('Offline and no cached shell available yet.');
    }
}

async function cacheFirst(request) {
    const cache = await caches.open(CACHE_NAME);
    const cached = await cache.match(request);
    if (cached) {
        // Revalidate in the background so the cache stays fresh after the next deploy.
        fetch(request).then((response) => {
            if (response && response.ok) {
                cache.put(request, response.clone());
            }
        }).catch(() => {});
        return cached;
    }

    const response = await fetch(request);
    if (response && response.ok) {
        cache.put(request, response.clone());
    }
    return response;
}
