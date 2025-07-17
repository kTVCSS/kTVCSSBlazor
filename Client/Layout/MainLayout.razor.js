window.showNotification = (title, message, icon) => {
    let audio = new Audio('/sounds/new-msg-v1.mp3')
    audio.volume = 0.5
    audio.play();
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, {
            body: message,
            icon: icon || '/images/chat-icon.png',
            badge: '/images/chat-badge.png'
        });
    }
};

window.getPermission = () => {
    return Notification.permission === 'default'
}

window.requestNotificationPermission = () => {
    if ('Notification' in window && Notification.permission === 'default') {
        Notification.requestPermission();
    }
};

function isMobileDevice() {
    const userAgent = navigator.userAgent || navigator.vendor || window.opera;
    const isMobile = /android|iphone|kindle|ipad/i.test(userAgent);
    if (isMobile) {
        return true;
    }
    const isTouchDevice = 'ontouchstart' in window || navigator.maxTouchPoints;
    if (screen.width <= 768) {
        return true;
    }
    if (isTouchDevice) {
        return true;
    }
    return false;
}

window.toBoosty = () => {
    window.open('https://boosty.to/ktvcss_vip', '_blank')
}

window.goBack = function () {
    history.back();
}

window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.focusElement = (element) => {
    if (element) {
        element.focus();
    }
};

window.setAnimateHook = async () => {
    while (document.getElementById('msg-cntr') == null) {
        await new Promise(resolve => setTimeout(resolve, 100));
    }

    document.getElementById('msg-cntr').addEventListener('animationend', function () {
        let element = document.getElementById('msg-cntr');
        element.scrollTop = element.scrollHeight;
    });
}

window.isElementInViewport = (element) => {
    if (!element) return false;

    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
};

window.resizeInterop = {
    initResizeListener: function (dotNetObjectReference) {
        function onResize() {
            dotNetObjectReference.invokeMethodAsync('OnResize', window.innerWidth, window.innerHeight);
        }

        window.addEventListener('resize', onResize);

        onResize();
    },

    removeResizeListener: function () {
        window.removeEventListener('resize', this.onResize);
    }
};

window.downloadFileFromStream = async (fileName, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
}

window.connectConsole = function (dotNetObjRef, wsUrl) {
    const ws = new WebSocket(wsUrl);
    ws.onmessage = function (evt) {
        dotNetObjRef.invokeMethodAsync('ReceiveConsoleMessage', evt.data);
    };
    ws.onclose = function () {
        dotNetObjRef.invokeMethodAsync('WebSocketClosed');
    };
}

window.scrollToBottom = (element) => {
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
}

window.saveFileFromStream = async (filename, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer]);
    const url = URL.createObjectURL(blob);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = filename ?? 'download';
    document.body.appendChild(anchorElement);
    anchorElement.click();
    document.body.removeChild(anchorElement);
    URL.revokeObjectURL(url);
};