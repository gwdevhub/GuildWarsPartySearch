import { WebSocketServer } from 'ws';
import {assert} from './assert.mjs'

let connected_sockets = [];

function check_connections(wss) {
    if(wss) {
        wss.clients.forEach((ws) => {
            if (ws.isAlive === false) {
                console.log("Terminating timed out ws");
                ws.terminate();
                return;
            }
            ws.isAlive = false;
            ws.ping();
        });
    }
    setTimeout(() => {
        check_connections(wss);
    },30000);
}

export function start_websocket_server(http_server) {
    let wss = 0;
    assert(!wss);
    if(typeof http_server == 'number') {
        wss = new WebSocketServer({port:http_server});
        console.log("Webserver started, port %d",http_server);
    } else {
        wss = new WebSocketServer({server:http_server});
    }


    check_connections(wss);

    wss.on('close', function close() {
        wss = null;
    });
    wss.on('connection', function connection(ws) {
        ws.last_message = new Date();
        ws.isAlive = true;
        ws.on('error', console.error);
        ws.on('pong', () => {
            ws.last_message = new Date();
            ws.isAlive = true;
        });
        ws.on('message', function message(data) {
            ws.last_message = new Date();
            ws.isAlive = true;
        });
        ws.on('close',() => {
            //wss on close
            // console.log(`Websocket client closed`);
        });
    });
    return wss;
}

