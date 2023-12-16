window.onload = function () {
    connect();
};

let locationMap = new Map();
let socket;
let isConnected = false;
function connect() {
    socket = new WebSocket('wss://guildwarspartysearch.azurewebsites.net/party-search/live-feed');

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
            let obj = JSON.parse(event.data);
            console.log(obj);
            obj.Searches.forEach(searchEntry => {
                let combinedKey = `${searchEntry.Campaign};${searchEntry.Continent};${searchEntry.Region};${searchEntry.Map};${searchEntry.District}`;
                locationMap.set(combinedKey, searchEntry);
            });

            updateEntriesDiv();
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

function updateEntriesDiv() {
    let entriesDiv = document.getElementById('entries');

    let htmlContent = '';

    locationMap.forEach((searchEntry, combinedKey) => {
        // Adding the combined key as a header
        htmlContent += `<h3>${searchEntry.Campaign} - ${searchEntry.Continent} - ${searchEntry.Region} - ${searchEntry.Map} - ${searchEntry.District}</h3 >`;

        // Adding each party search entry as a row
        searchEntry.PartySearchEntries.forEach(entry => {
            htmlContent += `<div>Character Name: ${entry.CharName}, Party Size: ${entry.PartySize}, Max Party Size: ${entry.PartyMaxSize}, NPCs: ${entry.Npcs}</div>`;
        });
    });

    // Updating the div with the constructed HTML
    entriesDiv.innerHTML = htmlContent;
}