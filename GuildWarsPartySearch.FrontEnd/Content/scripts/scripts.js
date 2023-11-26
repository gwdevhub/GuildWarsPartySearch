window.onload = function () {
    connect();
};

let socket;
let isConnected = false;
function connect() {
    socket = new WebSocket('wss://guildwarspartysearch.northeurope.azurecontainer.io/party-search/live-feed');

    socket.onopen = function (event) {
        console.log('Connected to WebSocket server.');
        document.getElementById('status').textContent = 'Connected';
        isConnected = true;
        startPing();
    };

    socket.onmessage = function (event) {
        if (event.data === 'pong') {
            console.log('Pong received from server.');
        } else {
            console.log('Message from server:', event.data);
            document.getElementById('response').textContent = 'Response: ' + event.data;
        }
    };

    socket.onclose = function (event) {
        console.log('Disconnected from WebSocket server.');
        document.getElementById('status').textContent = 'Disconnected';
        isConnected = false;
        stopPing();
        connect();
    };

    socket.onerror = function (error) {
        console.log('WebSocket error:', error);
        document.getElementById('status').textContent = 'Error';
        stopPing();
        connect();
    };
}

function startPing() {
    pingInterval = setInterval(() => {
        if (socket.readyState === WebSocket.OPEN) {
            socket.send('ping');
            console.log('Ping sent to server.');
        }
    }, 5000); // Ping every 30 seconds
}

function stopPing() {
    clearInterval(pingInterval);
}

function disconnect() {
    if (socket) {
        socket.close();
    }
}