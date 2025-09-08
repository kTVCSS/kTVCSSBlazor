const CACHE_NAME = 'blazor-keep-alive-v1';

self.addEventListener('install', event => {
    console.log('Service Worker установлен');
    self.skipWaiting();
});

self.addEventListener('activate', event => {
    console.log('Service Worker активирован');
    event.waitUntil(self.clients.claim());
});

self.addEventListener('sync', event => {
    if (event.tag === 'keep-alive-sync') {
        event.waitUntil(keepAlive());
    }
});

async function keepAlive() {
    try {
        const response = await fetch('/api/ping', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });
        console.log('Keep-alive ping отправлен:', new Date());
    } catch (error) {
        console.log('Keep-alive ping не удался:', error);
    }
}

self.addEventListener('message', event => {
    if (event.data && event.data.type === 'MANUAL_PING') {
        keepAlive();
    }
});