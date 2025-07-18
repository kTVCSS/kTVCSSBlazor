window.getWindowSize = function() {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
}

window.registerResizeCallback = function (dotNetObject, methodName) {
    function onResize() {
        try {
            dotNetObject.invokeMethodAsync(methodName, window.innerWidth, window.innerHeight);
        }
        catch (ex) {
            alert(ex);
        }
    }

    window.addEventListener('resize', onResize);
    onResize();

    return {
        dispose: () => window.removeEventListener('resize', onResize)
    };
};