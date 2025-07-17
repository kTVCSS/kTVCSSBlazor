export function getWindowSize() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}

export function registerResizeCallback(dotNetObject, methodName) {
    function onResize() {
        dotNetObject.invokeMethodAsync(methodName, window.innerWidth, window.innerHeight);
    }

    window.addEventListener('resize', onResize);
    onResize();

    return {
        dispose: () => window.removeEventListener('resize', onResize)
    };
}