let onlineHandler = null;
let offlineHandler = null;

// navigator.onLine-driven browser events are only a supplementary signal here (real request
// outcomes, probed and reported from C#, are the source of truth) - see ConnectivityService.

// Single-subscriber module (MainLayout is the one top-level layout for the whole app),
// so a module-level handler pair is enough - no need for a registry keyed by caller.
export function registerConnectivityCallback(dotNetRef) {
    onlineHandler = () => dotNetRef.invokeMethodAsync('OnConnectivityChanged', true);
    offlineHandler = () => dotNetRef.invokeMethodAsync('OnConnectivityChanged', false);
    window.addEventListener('online', onlineHandler);
    window.addEventListener('offline', offlineHandler);
}

export function unregisterConnectivityCallback() {
    if (onlineHandler) {
        window.removeEventListener('online', onlineHandler);
        onlineHandler = null;
    }
    if (offlineHandler) {
        window.removeEventListener('offline', offlineHandler);
        offlineHandler = null;
    }
}
