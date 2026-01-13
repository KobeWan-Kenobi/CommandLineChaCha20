
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
     * Encrypt with step-by-step details for visualization
     * @param {string} plaintext - Text to encrypt
     * @param {Uint8Array} key - 32-byte encryption key
     * @param {Uint8Array} nonce - 8-byte nonce
     * @param {number} counter - Starting counter (default 0)
     * @returns {Object} Encryption result with steps array
     */
    createEncryptionWithStepsArray(plaintext) {
        const key = this.generateKey();
        const nonce = this.generateNonce();
        let counter = 0;

        const steps = [];

        
        const encoder = new TextEncoder();
        const message = encoder.encode(plaintext);
        //Prerequisites 
        steps.push({
            stepName: "1. Prerequisites",
            description: [
                "<br>",
                "The following demonstration assumes that the user has a grasp of",
                "binary and decimal numbers and basic binary operations, especially",
                "binary addition, exclusive or (xor), and bit rotations",
                "<br>"
            ],
            stateData: "\t",
        });

        // Step 2: Show bytes
        
        steps.push({
            stepName: "2. Convert to Bytes",
            description: [
                "<br>",
                "First, the message is converted into bytes using ASCII.",
                "1 byte is represented by 2 hex digits:",
                "e.g. : \'Yo!\' = 59 6f 21",
                "This demonstration uses binary and hex representations:",
                "e.g. 12 is 0b00001100 in binary and 0x0c in hexadecimal",
                "The following is your message in hexadecimal:",
                "<br>"
            ],
            stateData: Array.from(message).map(b => b.toString(16).padStart(2, '0')).join(' '),
        });

        // Step 3: Generate Nonce and Ciphertext
        steps.push({
            stepName: "3. Generate encryption key and nonce",
            description: [
                "We use Mozilla's crypto.getRandomValues(), a secure number generator, to get our key and nonce.",
                "The key is the heart of this whole operation.",
                "A nonce is a randomly generated PUBLIC number used to prevent",
                "the same plaintext from producing the same ciphertext, adding a layer of randomness",
                "Thus, nonces also prevent attackers from seeing patterns in the ciphertext.",
                "That way, even if attackers gain a portion of the key, they don't know which data to apply the key to.",
                "Finally, nonces allow key reuse, since they are public they don't need to be managed as strictly.",
                "Keys are expensive to store and manage as databases fill with users."
            ],
            stateData: {
                // TODO: Make this into a formal function - it converts first 64 bytes of keyStreamBytes into "aa bb cc dd" format.
                nonce: Array.from(nonce).map(b => b.toString(16).padStart(2, '0')).join(' '), 
                key: Array.from(key).map(b => b.toString(16).padStart(2, '0')).join(' ')
            },
        });


        // Step 4: Initialize state matrix
        const keyStreamMatrix = this.initializeKeyStreamMatrix(key, nonce, counter);
        steps.push({
            stepName: "4. Initialize keystream matrix",
            description: [
                "<br>",
                "Chacha20 generates a 4x4 matrix. Each box in the grid is 4 bytes",
                "Then, chacha20 fills the box with a constant, a key, a nonce, and a counter in the following format:",
                "<br>",
                "Const\tConst\tConst\tConst",
                "Key\tKey\tKey\tKey",
                "Key\tKey\tKey\tKey",
                "Nonce\tNonce\tNonce\tCount",
                "<br>",
            ],
            stateData: Array.from(message).map(b => b.toString(16).padStart(2, '0')).join(' '),
        });

        // Step 5: Show quarter round operations
        steps.push({
            stepName: "5. Shuffling keystream matrix",
            description: [
                "<br>",
                "Now that the matrix is generated, we get to the heart of the algorithm.",
                "We shuffle the matrix using a special function: QuarterRound(a,b,c,d)",
                "+= is an increment operator. e.g. given x = 0b0101, x+= 0b0001 sets x = 0b0110",
                "^= is an xor and assign operator. For example in binary: x = 0b0110, x^= 0b1010 sets x = 0b1100",
                "RotateLeft(binaryNum, digits) rotates a binary number left in a set amount of digits:",
                "e.g. RotateLeft(x = 0110, 1) => 1100",
                "Using the above operations, QuarterRound(a,b,c,d) shuffles 4 blocks in this specific way:",
                "<br>",
                "a+=b; d^=a; d=RotateLeft(d,16);",
                "c+=d; b^=c; b=RotateLeft(b,12);",
                "a+=b; d^=a; d=RotateLeft(d,8);",
                "c+=d; b^=c; b=RotateLeft(b,7);",
                "<br>",
                "We shuffle the matrix both vertically and diagonally with QuarterRound:",
                "<br>",
                //TODO: Color code the shuffling scheme.
                "a\tb\tc\td",
                "e\tf\tg\th",
                "i\tj\tk\tl",
                "m\tn\to\tp",
                "<br>",
                "Each round consists of 4 QuarterRounds applied to vertical and diagonal blocks.",
                "The algorithm alternates between diagonal and vertical rounds.",
                "Vertical rounds QuarterRound(a,e,i,m)...(d,h,l,p)",
                "Diagonal rounds QR(a,f,k,p)...(c,h,i,o)...(d,e,j,o)... ",
                "After shuffling the the keystream matrix 20 times, we add the original matrix to itself",
                "This way, potential attackers cannot simply undo the rounds to discover the initial state"

                /*
                a b c d
                e f g h
                i j k l
                m n o p
                */
            ],
            stateData: "",
        });

        // Step 6: XOR operation
        // 
        const keyStreamBytes = this.shuffleKeyStreamMatrix(keyStreamMatrix);
        const output = new Uint8Array(message.length);
        for (let i = 0; i < message.length; i++) {
            output[i] = message[i] ^ keyStreamBytes[i];
        }

        steps.push({
            stepName: "6. XOR with Keystream",
            description: [
                "Then we perform XORs with each keystream bit and each plaintext bit",
                "When we reach the final keystream bit, we increment the counter by 1",
                "and reshuffle the matrix.",
                "In other words, every 64 bytes, we generate a new keystream matrix",
                "with the incremented counter.",
                "In the unlikely event that the counter overflows (approx 4 billion * 64 bytes = 256 GB),", 
                "it simply starts from 0 again.",
            ],         
            stateData: `${message[0].toString(16).padStart(2, '0')} ⊕ ${keyStreamBytes[0].toString(16).padStart(2, '0')} = ${output[0].toString(16).padStart(2, '0')} (example for first byte)`,
        });

        // Step 7: Final ciphertext
        steps.push({
            stepName: "7. Final Ciphertext",
            description: ["Here is what your final ciphertext looks like:"],
            stateData: this.bytesToBase64(output),
            stateType: "base64"
        });

        return steps;
    },
    /**
     * 
     *  Uncaught TypeError: Cannot read properties of undefined (reading 'forEach')
     *  at loopLines (site.js:215:11)
     *  at Object.showEncryptionWithSteps (chacha.js:230:13)
     *  at learnCommander (site.js:175:31)
     *  at enterKey (site.js:58:14)
     */
    showEncryptionWithSteps(plaintext) {
        let steps = this.createEncryptionWithStepsArray(plaintext)
        let delay = 80; // starts delay at 80 ms will be incremented upon every addLine to prevent text from being displayed randomly and simultaneously 
        for (let i = 0; i < steps.length; i++) {
            let stepTitle = steps[i].stepName;
            let stateData = steps[i].stateData.toString();
            let stepDescription = steps[i].description;

            addLine(stepTitle, "white margin", delay+=10);
            for (let j = 0; j < stepDescription.length; j++) {
                addLine(stepDescription[j], "color2 margin", delay += 10);
            }

            addLine(stateData, "color2 margin", delay += 10);
        }
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


