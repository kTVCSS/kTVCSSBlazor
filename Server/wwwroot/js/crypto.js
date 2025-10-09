window.cryptoHelper = {
    stringToArrayBuffer: function(str) {
        const encoder = new TextEncoder();
        return encoder.encode(str);
    },

    arrayBufferToString: function(buffer) {
        const decoder = new TextDecoder();
        return decoder.decode(buffer);
    },

    arrayBufferToBase64: function(buffer) {
        let binary = '';
        const bytes = new Uint8Array(buffer);
        for (let i = 0; i < bytes.byteLength; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary);
    },

    base64ToArrayBuffer: function(base64) {
        const binary = window.atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }
        return bytes.buffer;
    },

    generateKey: async function(keyString) {
        const keyMaterial = await crypto.subtle.importKey(
            'raw',
            this.stringToArrayBuffer(keyString),
            { name: 'PBKDF2' },
            false,
            ['deriveBits', 'deriveKey']
        );

        return await crypto.subtle.deriveKey(
            {
                name: 'PBKDF2',
                salt: this.stringToArrayBuffer('salt'),
                iterations: 100000,
                hash: 'SHA-256'
            },
            keyMaterial,
            { name: 'AES-GCM', length: 256 },
            false,
            ['encrypt', 'decrypt']
        );
    },

    encrypt: async function(plaintext, keyString) {
        try {
            const key = await this.generateKey(keyString);
            const iv = crypto.getRandomValues(new Uint8Array(12)); // 96-bit IV для GCM
            const encodedText = this.stringToArrayBuffer(plaintext);

            const encrypted = await crypto.subtle.encrypt(
                { name: 'AES-GCM', iv: iv },
                key,
                encodedText
            );

            const result = new Uint8Array(iv.length + encrypted.byteLength);
            result.set(iv, 0);
            result.set(new Uint8Array(encrypted), iv.length);

            return this.arrayBufferToBase64(result.buffer);
        } catch (error) {
            console.error('Encryption error:', error);
            throw error;
        }
    },

    decrypt: async function(ciphertext, keyString) {
        try {
            const key = await this.generateKey(keyString);
            const data = this.base64ToArrayBuffer(ciphertext);
            
            const iv = data.slice(0, 12);
            const encrypted = data.slice(12);

            const decrypted = await crypto.subtle.decrypt(
                { name: 'AES-GCM', iv: iv },
                key,
                encrypted
            );

            return this.arrayBufferToString(decrypted);
        } catch (error) {
            console.error('Decryption error:', error);
            throw error;
        }
    }
};