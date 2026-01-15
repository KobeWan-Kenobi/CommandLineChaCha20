const modes = {
    LEARN: 'learn',
    HOME: 'home',
    ENCRYPT: 'encrypt',
    /*LEARNING: 'learning'*/
}

let currentMode = modes.HOME;

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
        case "register":
            if (parts.length === 3) {
                registerUser(parts[1], parts[2]);
            } else {
                addLine("<span class='error'>Usage: register [username] [password]</span>", "error", 100);
            }
            break;
        case "login":
            if (parts.length === 3) {
                loginUser(parts[1], parts[2]);
            } else {
                addLine("<span class='error'>Usage: login [username] [password]</span>", "error", 100);
            }
            break;
        case "logout":
            sessionStorage.clear();
            addLine("<span class='inherit'>Logged out successfully</span>", "color2", 100);
            break;
        default:
            addLine("<span class=\"inherit\">Command not found. For a list of commands, type <span class=\"command\">'help'</span>.</span>", "error", 100);
            break;
    }
}

function learnCommander(input) {
    const parts = input.split(' ');
    const command = parts[0].toLowerCase();

    switch (command) {
        case "demo":
            if (parts.length < 2) {
                addLine("<span class='error'>ERROR usage: demo [message] </span>", "error", 100);
            }
            else {
                let message = parts.slice(1).join(' ');
                ChaChaService.showEncryptionWithSteps(message);
            }
            break;
        case "exit":
            currentMode = modes.HOME;
            syncLiner();
            addLine("Exiting LEARN mode...", "color2 margin", 80);
            break;
        default:
            break;
    }
}