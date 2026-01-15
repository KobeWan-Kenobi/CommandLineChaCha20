/**
 * Encrypt with step-by-step details for visualization
 * @param {string} plaintext - Text to encrypt
 * @param {Uint8Array} key - 32-byte encryption key
 * @param {Uint8Array} nonce - 8-byte nonce
 * @param {number} counter - Starting counter (default 0)
 * @returns {Object} Encryption result with steps array
 */
const ChaChaVisualizer = {
    createEncryptionWithStepsArray(plaintext) {
        const key = ChaChaService.generateKey();
        const nonce = ChaChaService.generateNonce();
        let counter = 0;

        const steps = [];


        const encoder = new TextEncoder();
        const message = encoder.encode(plaintext);
        //Prerequisites 
        steps.push({
            stepName: "1. Prerequisites",
            description: [
                "<br>",
                "• The following demonstration assumes that the user has a grasp of",
                "  binary and decimal numbers and basic binary operations, especially",
                "  binary addition, exclusive or (xor), and bit rotations",
                "<br>"
            ],
            stateData: "\t",
        });

        // Step 2: Show bytes

        steps.push({
            stepName: "2. Convert to Bytes",
            description: [
                "<br>",
                "• First, the message is converted into bytes using ASCII.",
                "• 1 byte is represented by 2 hex digits:",
                "• e.g. : \'Yo!\' = 59 6f 21",
                "• This demonstration uses binary and hex representations:",
                "• e.g. 12 is 0b00001100 in binary and 0x0c in hexadecimal",
                "• The following is your message in hexadecimal:",
                "<br>"
            ],
            stateData: `${Array.from(message).map(b => b.toString(16).padStart(2, '0')).join(' ')} <br> <br>`,
        });

        // Step 3: Generate Nonce and Ciphertext
        steps.push({
            stepName: "3. Generate encryption key and nonce",
            description: [
                "<br>",
                "• We use Mozilla's crypto.getRandomValues(), a secure number generator, to get our key and nonce.",
                "• The key is the heart of this whole operation.",
                "• A nonce is a PUBLIC randomly generated number used to prevent",
                "  the same plaintext from producing the same ciphertext, adding a layer of randomness",
                "• Thus, nonces also prevent attackers from seeing patterns in the ciphertext.",
                "• That way, even if attackers gain a portion of the key, they don't know which data to apply the key to.",
                "• Finally, nonces allow key reuse, since they are public they don't need to be managed as strictly.",
                "• Keys are expensive to store and manage as databases fill with users.",
                "<br>"
            ],
            stateData: `Nonce: ${Array.from(nonce).map(b => b.toString(16).padStart(2, '0')).join(' ')} <br>Key: ${Array.from(key).map(b => b.toString(16).padStart(2, '0')).join(' ')}<br>`
        });
        // `$Nonce: {Array.from(nonce).map(b => b.toString(16).padStart(2, '0')).join(' ')} Key: {Array.from(key).map(b => b.toString(16).padStart(2, '0')).join(' ')}`

        // Step 4: Initialize state matrix
        const keyStreamMatrix = ChaChaService.initializeKeyStreamMatrix(key, nonce, counter);
        steps.push({
            stepName: "4. Initialize keystream matrix",
            description: [
                "<br>",
                "• Chacha20 generates a 4x4 matrix. Each box in the grid is 4 bytes",
                "• Then, chacha20 fills the box with a constant, a key, a nonce, and a counter in the following format:",
                "<br>",
                "Const\tConst\tConst\tConst",
                "Key\tKey\tKey\tKey",
                "Key\tKey\tKey\tKey",
                "Nonce\tNonce\tNonce\tCount",
                "<br>",
            ],
            stateData: "\t"
        });

        // Step 5: Show quarter round operations
        steps.push({
            stepName: "5. Shuffling keystream matrix",
            description: [
                "<br>",
                "• Now that the matrix is generated, we get to the heart of the algorithm.",
                "• We shuffle the matrix using a special function: QuarterRound(a,b,c,d)",
                "• += is an increment operator. e.g. given x = 0b0101, x+= 0b0001 sets x = 0b0110",
                "• ^= is an xor and assign operator. For example in binary: x = 0b0110, x^= 0b1010 sets x = 0b1100",
                "• RotateLeft(binaryNum, digits) rotates a binary number left in a set amount of digits:",
                "• e.g. RotateLeft(x = 0110, 1) => 1100",
                "• Using the above operations, QuarterRound(a,b,c,d) shuffles 4 blocks in this specific way:",
                "<br>",
                "a+=b; d^=a; d=RotateLeft(d,16);",
                "c+=d; b^=c; b=RotateLeft(b,12);",
                "a+=b; d^=a; d=RotateLeft(d,8);",
                "c+=d; b^=c; b=RotateLeft(b,7);",
                "<br>",
                "• We shuffle the matrix both vertically and diagonally with QuarterRound:",
                "<br>",
                //TODO: Color code the shuffling scheme.
                "a\tb\tc\td",
                "e\tf\tg\th",
                "i\tj\tk\tl",
                "m\tn\to\tp",
                "<br>",
                "• Each round consists of 4 QuarterRounds applied to vertical and diagonal blocks.",
                "• The algorithm alternates between diagonal and vertical rounds.",
                "• Vertical rounds QuarterRound(a,e,i,m)...(d,h,l,p)",
                "• Diagonal rounds QR(a,f,k,p)...(c,h,i,o)...(d,e,j,o)... ",
                "• After shuffling the the keystream matrix 20 times, we add the original matrix to itself",
                "• This way, potential attackers cannot simply undo the rounds to discover the initial state",
                "<br>"
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
        const keyStreamBytes = ChaChaService.shuffleKeyStreamMatrix(keyStreamMatrix);
        const output = new Uint8Array(message.length);
        for (let i = 0; i < message.length; i++) {
            output[i] = message[i] ^ keyStreamBytes[i];
        }

        steps.push({
            stepName: "6. XOR with Keystream",
            description: [
                "<br>",
                "• Then we perform XORs with each keystream bit and each plaintext bit",
                "• When we reach the final keystream bit, we increment the counter by 1",
                "  and reshuffle the matrix.",
                "• In other words, every 64 bytes, we generate a new keystream matrix",
                "  with the incremented counter.",
                "• In the unlikely event that the counter overflows (approx 4 billion * 64 bytes = 256 GB),",
                "  it simply starts from 0 again.",
                "<br>"
            ],
            stateData: `${message[0].toString(16).padStart(2, '0')} ⊕ ${keyStreamBytes[0].toString(16).padStart(2, '0')} = ${output[0].toString(16).padStart(2, '0')} (example for first byte) <br> <br>`,
        });

        // Step 7: Final ciphertext
        steps.push({
            stepName: "7. Final Ciphertext",
            description: [
                "<br>",
                "Here is what your final ciphertext looks like:",
                "<br>"
            ],
            stateData: ChaChaService.bytesToBase64(output),
            stateType: "base64"
        });

        return steps;
    },
    showEncryptionWithSteps(plaintext) {
        let steps = this.createEncryptionWithStepsArray(plaintext)
        let delay = 80; // starts delay at 80 ms will be incremented upon every addLine to prevent text from being displayed randomly and simultaneously 
        for (let i = 0; i < steps.length; i++) {
            let stepTitle = steps[i].stepName;
            let stateData = steps[i].stateData.toString();
            let stepDescription = steps[i].description;

            addLine(stepTitle, "white margin", delay += 10);
            for (let j = 0; j < stepDescription.length; j++) {
                addLine(stepDescription[j], "color2 margin", delay += 10);
            }
            addLine(stateData, "color2 margin", delay += 10);

        }
    }
}