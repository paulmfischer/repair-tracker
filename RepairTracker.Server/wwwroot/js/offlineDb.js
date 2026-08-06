const DB_NAME = 'repair-tracker-offline';
const DB_VERSION = 1;
const STORE_NAMES = ['items', 'settings', 'outbox'];

let dbPromise = null;

function openDb() {
    dbPromise ??= new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);
        request.onupgradeneeded = () => {
            const db = request.result;
            for (const name of STORE_NAMES) {
                if (!db.objectStoreNames.contains(name)) {
                    // Blazor's JS interop serializes C# objects with camelCase property names,
                    // so the key path here is 'id', not the C# model's 'Id'.
                    db.createObjectStore(name, { keyPath: 'id' });
                }
            }
        };
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
    return dbPromise;
}

async function withStore(storeName, mode, fn) {
    const db = await openDb();
    return new Promise((resolve, reject) => {
        const tx = db.transaction(storeName, mode);
        const store = tx.objectStore(storeName);
        const request = fn(store);
        tx.onerror = () => reject(tx.error);
        request.onsuccess = () => resolve(request.result);
        request.onerror = () => reject(request.error);
    });
}

export async function getAll(storeName) {
    return withStore(storeName, 'readonly', (store) => store.getAll());
}

export async function get(storeName, key) {
    const result = await withStore(storeName, 'readonly', (store) => store.get(key));
    return result ?? null;
}

export async function put(storeName, value) {
    await withStore(storeName, 'readwrite', (store) => store.put(value));
}

export async function remove(storeName, key) {
    await withStore(storeName, 'readwrite', (store) => store.delete(key));
}

export async function clear(storeName) {
    await withStore(storeName, 'readwrite', (store) => store.clear());
}
