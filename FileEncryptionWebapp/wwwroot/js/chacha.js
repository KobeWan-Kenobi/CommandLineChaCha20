
// It was a PITA to rewrite all this in JS :(
const ChaChaService = {
    MATRIX_SIZE: 16,
    ROUNDS: 10, // 10 Double rounds == 20 rounds, hence chacha20

    // Constant bits: "expand 32-byte k"
    CONSTANT_BITS: new Uint32Array([
        0x61707865, 0x3320646e, 0x79622d32, 0x6b206574
    ]),

    /** 
     * Encrypt plaintext using ChaCha20
     * @param {string} plaintext - Text to encrypt
     * @param {Uint8Array} key - 32-byte encryption key
     * @param {Uint8Array} nonce - 8-byte nonce
     * @param {number} counter - Starting counter (default 0)
     * @returns {Uint8Array} Encrypted bytes
     */
    encrypt(plaintext, key, nonce, counter = 0) {
        
        // Convert plaintext string to bytes
        const encoder = new TextEncoder();
        const message = encoder.encode(plaintext);

        return this.processData(message, key, nonce, counter);
    },

    /**
     * Decrypt ciphertext using ChaCha20
     * @param {Uint8Array} ciphertext - Encrypted bytes
     * @param {Uint8Array} key - 32-byte encryption key
     * @param {Uint8Array} nonce - 8-byte nonce
     * @param {number} counter - Starting counter (default 0)
     * @returns {string} Decrypted plaintext
     */
    decrypt(ciphertext, key, nonce, counter = 0) {
        if (key.length !== 32) {
            throw new Error('Key must be 32 bytes');
        }
        if (nonce.length !== 8) {
            throw new Error('Nonce must be 8 bytes');
        }

        const decryptedBytes = this.processData(ciphertext, key, nonce, counter);

        // Convert bytes back to string
        const decoder = new TextDecoder();
        return decoder.decode(decryptedBytes);
    },

    /**
     * Process data (both encryption and decryption use this)
     * @param {Uint8Array} input - Input bytes
     * @param {Uint8Array} key - 32-byte key
     * @param {Uint8Array} nonce - 8-byte nonce
     * @param {number} counter - Counter value
     * @returns {Uint8Array} Processed bytes
     */
    processData(input, key, nonce, counter) {
        const output = new Uint8Array(input.length);
        const keyStreamMatrix = this.initializeKeyStreamMatrix(key, nonce, counter);

        const totalNumberOfMatrices = Math.ceil(input.length / 64);

        for (let matrixCount = 0; matrixCount < totalNumberOfMatrices; matrixCount++) {
            const keyStreamBytes = this.shuffleKeyStreamMatrix(keyStreamMatrix);
            const matrixStart = matrixCount * 64;
            const maxBytesOrRemainder = Math.min(64, input.length - matrixStart);

            for (let i = 0; i < maxBytesOrRemainder; i++) {
                output[matrixStart + i] = input[matrixStart + i] ^ keyStreamBytes[i];
            }

            keyStreamMatrix[12]++;
        }

        return output;
    },

    
    initializeKeyStreamMatrix(key, nonce, counter) {
        const keyStreamMatrix = new Uint32Array(this.MATRIX_SIZE);

        // First 4 blocks are constants
        for (let i = 0; i < 4; i++) {
            keyStreamMatrix[i] = this.CONSTANT_BITS[i];
        }

        // Next 8 blocks contain the key
        for (let i = 0; i < 8; i++) {
            keyStreamMatrix[4 + i] = this.bytesToUint32(key, i * 4);
        }

        // Block 12 is the counter
        keyStreamMatrix[12] = counter;

        // Last 3 blocks contain the nonce (split into 2 uint32s for 8 bytes)
        keyStreamMatrix[13] = this.bytesToUint32(nonce, 0);
        keyStreamMatrix[14] = this.bytesToUint32(nonce, 4);
        keyStreamMatrix[15] = 0; // Padding to maintain 12-byte nonce space

        return keyStreamMatrix;
    },

    
    shuffleKeyStreamMatrix(originalKeyStreamMatrix) {
        // Clone the matrix
        const clonedKeyStreamMatrix = new Uint32Array(originalKeyStreamMatrix);

        // Perform 20 rounds (10 double rounds)
        for (let i = 0; i < this.ROUNDS; i += 2) {
            // Column rounds
            this.quarterRound(clonedKeyStreamMatrix, 0, 4, 8, 12);
            this.quarterRound(clonedKeyStreamMatrix, 1, 5, 9, 13);
            this.quarterRound(clonedKeyStreamMatrix, 2, 6, 10, 14);
            this.quarterRound(clonedKeyStreamMatrix, 3, 7, 11, 15);

            // Diagonal rounds
            this.quarterRound(clonedKeyStreamMatrix, 0, 5, 10, 15);
            this.quarterRound(clonedKeyStreamMatrix, 1, 6, 11, 12);
            this.quarterRound(clonedKeyStreamMatrix, 2, 7, 8, 13);
            this.quarterRound(clonedKeyStreamMatrix, 3, 4, 9, 14);
        }

        // Add original state to working state
        for (let i = 0; i < this.MATRIX_SIZE; i++) {
            clonedKeyStreamMatrix[i] = (clonedKeyStreamMatrix[i] + originalKeyStreamMatrix[i]) >>> 0;
        }

        return this.keyStreamMatrixToBytes(clonedKeyStreamMatrix);
    },

    
    keyStreamMatrixToBytes(keyStreamMatrix) {
        const bytes = new Uint8Array(64);

        for (let i = 0; i < this.MATRIX_SIZE; i++) {
            bytes[i * 4 + 0] = (keyStreamMatrix[i] >>> 0) & 0xff;
            bytes[i * 4 + 1] = (keyStreamMatrix[i] >>> 8) & 0xff;
            bytes[i * 4 + 2] = (keyStreamMatrix[i] >>> 16) & 0xff;
            bytes[i * 4 + 3] = (keyStreamMatrix[i] >>> 24) & 0xff;
        }

        return bytes;
    },

    
    quarterRound(matrix, aIdx, bIdx, cIdx, dIdx) {
        let a = matrix[aIdx];
        let b = matrix[bIdx];
        let c = matrix[cIdx];
        let d = matrix[dIdx];

        a = (a + b) >>> 0; d ^= a; d = this.rotateLeft(d, 16);
        c = (c + d) >>> 0; b ^= c; b = this.rotateLeft(b, 12);
        a = (a + b) >>> 0; d ^= a; d = this.rotateLeft(d, 8);
        c = (c + d) >>> 0; b ^= c; b = this.rotateLeft(b, 7);

        matrix[aIdx] = a;
        matrix[bIdx] = b;
        matrix[cIdx] = c;
        matrix[dIdx] = d;
    },

    
    rotateLeft(value, bits) {
        return ((value << bits) | (value >>> (32 - bits))) >>> 0;
    },

    
    bytesToUint32(bytes, offset) {
        return (bytes[offset] |
            (bytes[offset + 1] << 8) |
            (bytes[offset + 2] << 16) |
            (bytes[offset + 3] << 24)) >>> 0;
    },

    
    bytesToBase64(bytes) {
        const binString = Array.from(bytes, (byte) =>
            String.fromCodePoint(byte)
        ).join("");
        return btoa(binString);
    },

    
    base64ToBytes(base64) {
        const binString = atob(base64);
        return Uint8Array.from(binString, (char) => char.codePointAt(0));
    },

    
    generateKey() {
        const key = new Uint8Array(32);
        crypto.getRandomValues(key);
        return key;
    },

    
    generateNonce() {
        const nonce = new Uint8Array(8);
        crypto.getRandomValues(nonce);
        return nonce;
    },

    
    formatMatrix(matrix) {
        let result = '';
        for (let i = 0; i < 4; i++) {
            const row = [];
            for (let j = 0; j < 4; j++) {
                row.push(matrix[i * 4 + j].toString(16).padStart(8, '0'));
            }
            result += row.join(' ') + '\n';
        }
        return result;
    }
};

// Example 1: Basic encryption/decryption
function basicExample() {
    const plaintext = "Hello, World!";
    const key = ChaChaService.generateKey();
    const nonce = ChaChaService.generateNonce();

    // Encrypt
    const encrypted = ChaChaService.encrypt(plaintext, key, nonce);
    console.log("Encrypted:", ChaChaService.bytesToBase64(encrypted));

    // Decrypt
    const decrypted = ChaChaService.decrypt(encrypted, key, nonce);
    console.log("Decrypted:", decrypted); // "Hello, World!"
}

// Displaying encrypt with steps
function encryptionWithSteps() {
    const plaintext = "Secret";
    const key = ChaChaService.generateKey();
    const nonce = ChaChaService.generateNonce();

    const result = ChaChaService.encryptWithSteps(plaintext, key, nonce);

    console.log("Algorithm:", result.algorithm);
    console.log("Original:", result.originalText);
    console.log("Encrypted:", result.encryptedBase64);

    // Display each step
    result.steps.forEach(step => {
        console.log(`\nStep ${step.stepNumber}: ${step.stepName}`);
        console.log(`Description: ${step.description}`);
        console.log(`Data: ${step.stateData}`);
    });

    return result;
}


