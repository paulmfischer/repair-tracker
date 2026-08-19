export function getElementRect(element) {
    const rect = element.getBoundingClientRect();
    return { width: rect.width, height: rect.height };
}

// MudDialog applies its own focus-trap logic on open, which can steal focus back
// after Blazor's ElementReference.FocusAsync() runs. Deferring to the next macrotask
// lets that settle first so our container ends up with focus.
export function focusElement(element) {
    setTimeout(() => element.focus(), 0);
}
