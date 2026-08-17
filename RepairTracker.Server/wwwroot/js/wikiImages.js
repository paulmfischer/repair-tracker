// Event delegation for images embedded in rendered wiki markdown (raw HTML from Markdig,
// so Blazor can't attach @onclick to individual <img> tags directly).
export function attachImageClickHandler(container, dotNetRef) {
    if (container.__wikiImageHandler) {
        container.removeEventListener('click', container.__wikiImageHandler);
    }

    const handler = (e) => {
        const target = e.target;
        if (target instanceof HTMLImageElement) {
            const src = target.getAttribute('src') || '';
            if (src.startsWith('/')) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('OnImageClicked', src.slice(1));
            }
        }
    };

    container.__wikiImageHandler = handler;
    container.addEventListener('click', handler);
}
