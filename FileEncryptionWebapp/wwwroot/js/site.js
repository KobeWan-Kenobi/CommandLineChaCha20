// Write your JavaScript code.
//site.js
/* TODOs:
 * 1. Login mode: move the commented-out login/register logic to its own commander
 * 2. Finish chacha.js 'encryptWithSteps' method
 * 3. Re-scaffold backend with the 'Users' table
 * 4. Write the back-end for the Entries, Users and EncryptionKeys tables
 * 5. Update 'help' function to include new commands
 */
﻿var before = document.getElementById("before");
﻿var liner = document.getElementById("liner");
﻿var command = document.getElementById("typer");
﻿var textarea = document.getElementById("texter");
﻿var terminal = document.getElementById("terminal");
﻿
﻿var git = 0;
﻿var pw = false;
﻿let pwd = false;
﻿var commands = [];

const modes = {
    LEARN: 'learn',
    HOME: 'home',
    ENCRYPT: 'encrypt',
    LOGIN: 'login'
}


﻿
﻿console.log("site.js loaded");
﻿console.log("help variable:", help);
﻿setTimeout(function () {
﻿    loopLines(banner, "", 80);
﻿    textarea.focus();
﻿}, 100);
﻿
﻿window.addEventListener("keyup", enterKey);
﻿
﻿//init
﻿textarea.value = "";
﻿command.innerHTML = textarea.value;


﻿function enterKey(e) {
﻿    if (e.keyCode == 181) {
﻿        document.location.reload(true);
﻿    }﻿
﻿    if (e.keyCode == 13) { // enter key
﻿        commands.push(command.innerHTML);
         git = commands.length;
         if (currentMode === modes.HOME) {
             addLine("KobeCodesVisitor:~$ " + command.innerHTML, "no-animation", 0);
             homeCommander(command.innerHTML.toLowerCase());
         }
         else if (currentMode === modes.LEARN){
             addLine("KobeCodesVisitor[LEARN]:~$ " + command.innerHTML, "no-animation", 0);
             learnCommander(command.innerHTML.toLowerCase());
         }
﻿           
﻿        command.innerHTML = "";
﻿        textarea.value = "";
﻿    }
﻿    if (e.keyCode == 38 && git != 0) {
﻿        git -= 1;
﻿        textarea.value = commands[git];
﻿        command.innerHTML = textarea.value;
﻿    }
﻿    if (e.keyCode == 40 && git != commands.length) {
﻿        git += 1;
﻿        if (commands[git] === undefined) {
﻿            textarea.value = "";
﻿        } else {
﻿            textarea.value = commands[git];
﻿        }
﻿        command.innerHTML = textarea.value;
﻿    }
﻿    
}
// Updates the "KobeCodes...$" style based on the current state
function syncLiner() {
    const liner = document.getElementById("liner");

    liner.classList.remove("learn-mode", "password");

    if (currentMode === modes.LEARN) {
        liner.classList.add("learn-mode");
    } else if (currentMode === modes.LOGIN) {
        liner.classList.add("password");
    }
}
// TODO: Make more robust states machine

function homeCommander(cmd) {
    const parts = cmd.split(' ');
    const command = parts[0].toLowerCase();
    
    switch (command) {
        case "help":
            loopLines(help, "color2 margin", 80);
            break;
        case "whoami":
            loopLines(whoami, "color2 margin", 80);
            break;
        case "projects":
            loopLines(projects, "color2 margin", 80);
            break;
        case "history":
            addLine("<br>", "", 0);
            loopLines(commands, "color2", 80);
            addLine("<br>", "command", 80 * commands.length + 50);
            break;
        case "learn":
            loopLines(learn, "color2 margin", 80);
            currentMode = modes.LEARN;
            syncLiner();
            break;
        case "email":
            addLine('I said don\'t email me!', "color2", 80);
            break;
        case "clear":
            setTimeout(function () {
                var outputs = terminal.getElementsByTagName('p');
                while (outputs[0]) {
                    outputs[0].remove();
                }
            }, 1);
            break;
        case "newMessage":
            if (parts.length >= 3) {
                const password = parts[0]; // last element is password
                const message = parts.slice(1, -1).join(' ');
                ChaChaService.saveEncryptedEntry();
                addLine("Message sent to server.", "color2", 80);
            }
            else {
                addLine("<span class='error'>Usage: write [message] [password]</span>", "error", 100);
            }
            break;
        //case "register":
        //    if (parts.length === 3) {
        //        registerUser(parts[1], parts[2]);
        //    } else {
        //        addLine("<span class='error'>Usage: register [username] [password]</span>", "error", 100);
        //    }
        //    break;
        //case "login":
        //    if (parts.length === 3) {
        //        loginUser(parts[1], parts[2]);
        //    } else {
        //        addLine("<span class='error'>Usage: login [username] [password]</span>", "error", 100);
        //    }
        //    break;
        //case "logout":
        //    sessionStorage.clear();
        //    addLine("<span class='inherit'>Logged out successfully</span>", "color2", 100);
        //    break;
        default:
            addLine("<span class=\"inherit\">Command not found. For a list of commands, type <span class=\"command\">'help'</span>.</span>", "error", 100);
            break;
    }
}

function learnCommander(input) {
    switch (input) {
        case "exit":
            currentMode = modes.HOME;
            syncLiner();
            addLine("Exiting LEARN mode...", "color2 margin", 80);
            break;
        default:
            break;
    }
}
﻿function newTab(link) {
﻿    setTimeout(function () {
﻿        window.open(link, "_blank");
﻿    }, 500);
﻿}
﻿
﻿function addLine(text, style, time) {
﻿    var t = "";
﻿    for (let i = 0; i < text.length; i++) {
﻿        if (text.charAt(i) == " " && text.charAt(i + 1) == " ") {
﻿            t += "&nbsp;&nbsp;";
﻿            i++;
﻿        } else {
﻿            t += text.charAt(i);
﻿        }
﻿    }
﻿    setTimeout(function () {
﻿        var next = document.createElement("p");
﻿        next.innerHTML = t;
﻿        next.className = style;
﻿
﻿        before.parentNode.insertBefore(next, before);
﻿
﻿        window.scrollTo(0, document.body.offsetHeight);
﻿    }, time);
﻿}
﻿
﻿function loopLines(name, style, time) {
﻿    name.forEach(function (item, index) {
﻿        addLine(item, style, index * time);
﻿    });
﻿}

// Test ChaCha20 encryption
function testEncryption() {
    const plaintext = "Hello, World!";
    const key = ChaChaService.generateKey();
    const nonce = ChaChaService.generateNonce();

    console.log("Original:", plaintext);

    const encrypted = ChaChaService.encrypt(plaintext, key, nonce);
    console.log("Encrypted (base64):", ChaChaService.bytesToBase64(encrypted));

    const decrypted = ChaChaService.decrypt(encrypted, key, nonce);
    console.log("Decrypted:", decrypted);

    console.log("Match:", plaintext === decrypted ? "SUCCESS" : "FAILED");
}

async function saveEncryptedEntry(plaintext) {
    const userId = sessionStorage.getItem('userId');

    if (!userId) {
        console.error("Must be logged in");
        return;
    }

    // Generate key and nonce
    const key = ChaChaService.generateKey();
    const nonce = ChaChaService.generateNonce();

    // Encrypt client-side
    const encrypted = ChaChaService.encrypt(plaintext, key, nonce);

    // Send encrypted data to server
    const response = await fetch('/api/entries', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            encryptedEntry: ChaChaService.bytesToBase64(encrypted),
            key: ChaChaService.bytesToBase64(key),  // In real app, derive from user password
            nonce: ChaChaService.bytesToBase64(nonce),
            userId: parseInt(userId)
        })
    });

    return await response.json();
}

// Example 4: Retrieve and decrypt entry
async function getEncryptedEntry(entryId) {
    const response = await fetch(`/api/entries/${entryId}`);
    const data = await response.json();

    // Get encrypted data and keys from server
    const encrypted = ChaChaService.base64ToBytes(data.encryptedEntry);
    const key = ChaChaService.base64ToBytes(data.key);
    const nonce = ChaChaService.base64ToBytes(data.nonce);

    // Decrypt client-side
    const decrypted = ChaChaService.decrypt(encrypted, key, nonce);

    console.log("Decrypted entry:", decrypted);
    return decrypted;
}

//function registerUser(username, password) {
//    fetch('/api/users/register', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//        },
//        body: JSON.stringify({
//            username: username,
//            password: password
//        })
//    })
//        .then(response => response.json())
//        .then(data => {
//            if (data.userId) {
//                addLine("<span class='inherit'>Registration successful!" + "</span>", "color2", 100);
//            } else {
//                addLine("<span class='error'>" + data.message + "</span>", "error", 100);
//            }
//        })
//        .catch(error => {
//            addLine("<span class='error'>Registration error: " + error + "</span>", "error", 100);
//        });
//}

//function loginUser(username, password) {
//    fetch('/api/users/login', {
//        method: 'POST',
//        headers: {
//            'Content-Type': 'application/json',
//        },
//        body: JSON.stringify({
//            username: username,
//            password: password
//        })
//    })
//        .then(response => response.json())
//        .then(data => {
//            if (data.userId) {
//                addLine("<span class='inherit'>Login successful! Welcome, " + data.username + "</span>", "color2", 100);
//                // Store user session
//                sessionStorage.setItem('userId', data.userId);
//                sessionStorage.setItem('username', data.username);
//            } else {
//                addLine("<span class='error'>" + data.message + "</span>", "error", 100);
//            }
//        })
//        .catch(error => {
//            addLine("<span class='error'>Login error: " + error + "</span>", "error", 100);
//        });
//}﻿