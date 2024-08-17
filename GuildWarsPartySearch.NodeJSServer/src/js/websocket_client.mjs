
// Create WebSocket connection.
import {assert} from './assert.mjs'
import {PartySearch} from "./PartySearch.class.mjs";

let ws = null;

/**
 * Current websocket if available
 * @return {WebSocket|null}
 */
export function get_websocket_client() {
    return ws;
}

/**
 *
 * @param {WebSocket} websocket
 * @return {boolean}
 */
export function is_websocket_ready(websocket) {
    return websocket && websocket.readyState === WebSocket.OPEN;
}

/**
 *
 * @param {string} _url
 * @return {Promise<WebSocket>}
 */
export function start_websocket_client(_url = null) {
    if(ws) ws.close();
    ws = null;
    const url = _url || new URL(window.location.href);
    let websocket_url='';
    if(url.protocol === 'https:') {
        websocket_url = `wss://${url.host}${url.pathname}`;
    } else {
        websocket_url = `ws://${url.host}${url.pathname}`;
    }
    return new Promise((resolve,reject) => {
        let websocket = new WebSocket(websocket_url);
        let resolved = false;
        // Connection opened
        websocket.addEventListener("open", (event) => {
            console.log("Websocket connected successfully",event);
            ws = websocket;
            resolve(ws);
            resolved = true;
        });

        websocket.addEventListener("close",(event) => {
            console.log("Websocket close",event);
            ws = null;
        })
        websocket.addEventListener("error",(event) => {
            console.log("Websocket error",event);
            if(ws) ws.close();
            ws = null;
        })
        setTimeout(() => {
            if(!resolved) {
                if(ws) ws.close();
                ws = null;
                reject(new Error("Timeout"));
            }
        },5000);
    })
}