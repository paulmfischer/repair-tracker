// The service worker caches wiki images/attachments indefinitely (cache-first, revalidated
// in the background - see service-worker.js). Deleting a file only removes it from disk, so
// without this the service worker keeps serving the cached copy of a file that's gone,
// leaving stale images visible in the article. Iterates every cache (rather than hardcoding
// the service worker's CACHE_NAME) so it doesn't need to stay in sync with that constant.
export async function evictFromCache(url) {
    if (!('caches' in window)) {
        return;
    }

    const keys = await caches.keys();
    await Promise.all(keys.map(async (key) => {
        const cache = await caches.open(key);
        await cache.delete(url);
    }));
}
