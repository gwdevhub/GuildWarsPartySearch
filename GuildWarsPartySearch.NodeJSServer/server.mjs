
import {is_uuid, json_parse, to_number} from "./src/js/string_functions.mjs";
import express from "express";
import bodyParser from "body-parser";
import {is_quarantine_hit} from "./src/js/spam_filter.mjs";
import {PartySearch} from "./src/js/PartySearch.class.mjs";
import {start_websocket_server} from "./src/js/websocket_server.mjs";
import {assert} from "./src/js/assert.mjs";
import * as http from "http";
import { is_whitelisted} from "./src/js/auth.mjs";
import { WebSocket } from 'ws';
import morgan from "morgan";
import {
    get_ip_from_request,
    get_data_from_request,
    is_websocket,
    send_header,
    send_json,
    build_express_cache_options
} from "./src/js/networking.mjs";
import * as path from "path";
import {district_from_region, region_from_district} from "./src/js/gw_constants.mjs";
import {unique,groupBy, find_if, delete_if} from "./src/js/array_functions.mjs";

import { dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import LZString from "./src/js/lz-string.min.mjs";

import "./src/js/date_functions.js";

import config from "./config.json" with { type: "json" };

console.logDefault = console.log;
console.log = (...args) => {
    console.logDefault(`[${(new Date()).format('H:i:s')}]`,...args);
}
console.errorDefault = console.error;
console.error = (...args) => {
    console.errorDefault(`[${(new Date()).format('H:i:s')}]`,...args);
}


const __dirname = dirname(fileURLToPath(import.meta.url));

let parties_by_client = {};
let all_parties = [];
let last_party_change_timestamp_by_map = {};
let last_party_change = 0;
let last_available_maps_broadcasted = [];
let bot_clients = {};

const minimum_supported_bot_version = 1;

/**
 *
 * @param request
 * @return {*|string}
 */
function get_client_id(request) {
    if(!is_websocket(request))
        return '';
    if(!request.headers)
        return '';
    if(request.headers['x-account-uuid'])
        return `${request.headers['x-account-uuid']}`;
    if(!request.headers['user-agent'])
        return '';
    // LEGACY: Bots passing uuid as part of the user agent string
    const matches = /([a-zA-Z0-9]+)-[0-9]+-[0-9]+/.exec(`${request.headers['user-agent']}`);
    if(matches)
        return matches[1];
    return '';
}

/**
 * Find matching bot client for websocket
 * @param request
 * @return {boolean|*|null}
 */
function get_bot_client(request) {
    if(!is_websocket(request))
        return false;
    const found = find_if(bot_clients,(entry) => {
        return entry === request;
    });
    return found.length ? found[0] : null;
}
/**
 *
 * @param client_id {string}
 * @param request {http.IncomingMessage|WebSocket}
 * @returns bot_client
 */
function add_bot_client(request) {
    const client_id = get_client_id(request);
    assert(client_id, "Missing client_id, ensure x-account-uuid header is present");
    assert(to_number(request.headers['x-bot-version'] || 0) >= minimum_supported_bot_version, "Out of date bot version, ensure x-bot-version header is present and is latest bot version")
    assert(is_whitelisted(request), "Bot failed to pass whitelisting checks; ensure valid API key is given and IP is allowed");
    if(bot_clients[client_id] && bot_clients[client_id] !== request) {
        bot_clients[client_id].close();
    }
    request.is_bot_client = 1;
    request.client_id = client_id;
    bot_clients[client_id] = request;
    console.log(`Bot client added: ${JSON.stringify(request.headers,null,1)}`)
}

/**
 * Broadcast party searches to all connected clients that are subscribed to this map id
 * @param map_id {number}
 * @param request_or_websocket {WebSocket|Request|null}
 */
function send_map_parties(map_id, request_or_websocket = null) {
    const parties_by_map = all_parties.filter((party) => {
        return map_id === 0 || party.map_id === map_id;
    });
    const ret = JSON.stringify({
        "type":"map_parties",
        "map_id":map_id,
        "parties":unique(parties_by_map,(party) => {
                    return `${party.party_id}-${party.district_region}`;
                }).map((party) => {
                    let json = party.toJSON();
                    delete json.map_id; // No need to send map id twice
                    return json;
                })
    });

    if(request_or_websocket === null) {
        send_to_websockets(get_user_websockets().filter((ws) => {
            return ws.map_id === map_id;
        }),ret);
        return;
    }
    request_or_websocket.map_id = map_id;
    send_json(request_or_websocket,ret);
}

/**
 * Delete a party search from the system
 * @param party {PartySearch}
 */
function remove_party(party) {
    const now = new Date();
    let idx = all_parties.indexOf(party);
    if(idx !== -1) {
        all_parties.splice(idx,1);
    }
    if(parties_by_client[party.client_id]) {
        idx = parties_by_client[party.client_id].indexOf(party);
        if(idx !== -1) {
            parties_by_client[party.client_id].splice(idx,1);
        }
    }

    last_party_change_timestamp_by_map[party.map_id] = now;
    last_party_change = now;
}
/**
 * Add new party search info to the system
 * @param party {PartySearch}
 */
function add_party(party) {
    const now = new Date();
    if(!parties_by_client[party.client_id])
        parties_by_client[party.client_id] = [];
    parties_by_client[party.client_id].push(party);
    if(!all_parties.includes(party)) {
        all_parties.push(party);
    }
    last_party_change_timestamp_by_map[party.map_id] = now;
    last_party_change = now;
}

/**
 *
 * @param client_id {string}
 */
function on_bot_disconnected(request) {
    console.log("on_bot_disconnected",request.headers);
    delete_if(bot_clients,(entry) => {
        return entry === request;
    });
    const client_id = request.client_id;
    if(!client_id)
        return; // Valid error; could be in response to a bad connection
    let maps_affected = {};
    all_parties.filter((party) => { return party.client_id === client_id; })
        .forEach((party) => {
            maps_affected[party.map_id] = 1;
            remove_party(party);
        })
    Object.keys(maps_affected).forEach((map_id) => {
        send_map_parties(to_number(map_id));
    });
    send_available_maps();
}

/**
 * Handler for receiving party search data from a bot websocket
 * @param ws {http.IncomingMessage|WebSocket}
 * @param data {Object}
 */
function on_recv_parties(ws, data) {
    const client_id = get_client_id(ws);
    const bot_client = get_bot_client(ws);
    assert(bot_client, "on_recv_parties: not a bot client");
    const map_id = to_number(data.map_id);
    assert(map_id, "on_recv_parties: invalid map_id");
    if(!data.hasOwnProperty('district_region')) {
        assert(data.hasOwnProperty('district'));
        // Extract district region from district
        data.district_region = region_from_district(data.district);
    }
    const district_region = to_number(data.district_region);
    bot_client.map_id = map_id;
    bot_client.district_region = district_region;
    // NB: don't give a shit about language
    bot_client.district = district_from_region(bot_client.district_region, 0);
    // Cast parties to PartySearch objects
    const parties = data.parties.filter((party_json) => {
        return !is_quarantine_hit(party_json.message || '');
    }).map((party_json) => {
        party_json.client_id = client_id;
        party_json.district_region = data.district_region;
        party_json.map_id = map_id;
        return new PartySearch(party_json);
    });

    let maps_affected = {};
    maps_affected[map_id] = 1;
    // Remove any current parties for this client
    all_parties.filter((party) => { return party.client_id === client_id; })
        .forEach((party) => {
            maps_affected[party.map_id] = 1;
            remove_party(party);
        })
    // Cycle and add the parties to the system
    parties.forEach((party) => {
        add_party(party);
    });
    // Broadcast the update to all connected clients
    //send_map_parties(map_id);
    // Broadcast the summary list to all connected clients
    send_available_maps();
    Object.keys(maps_affected).forEach((map_id) => {
        send_map_parties(map_id);
    })
}

/**
 * Send a summary list of map_ids that have available parties to a http request or endpoint
 * @param ws {WebSocket|Request}
 * @param force
 */
function send_available_maps(ws = null, force = false, exclude_ws = null) {

    const unique_parties = unique(all_parties,(party) => {
        return `${party.map_id}-${party.district_region}-${party.party_id}`;
    })
    const parties_by_district = groupBy(unique_parties,(party) => {
        return `${party.map_id}-${party.district}`;
    })
    let unique_bots = Object.values(bot_clients).filter((bot_client) => {
        // Exclude current websocket from the list
        return bot_client.map_id && (!ws && bot_client.client_id !== ws.client_id);
    })
    const available_maps = unique(unique_bots,(bot_client) => {
            return `${bot_client.map_id}-${bot_client.district}`;
        }).map((bot_client) => {
            const key = `${bot_client.map_id}-${bot_client.district}`;
            return {
                map_id:bot_client.map_id,
                district:bot_client.district,
                party_count:(parties_by_district[key] || []).length
            };
        });
    const res = JSON.stringify({
        "type":"available_maps",
        "available_maps":available_maps
    });

    if(ws === null) {

        if(!force && last_available_maps_broadcasted === res)
            return; // Don't broadcast unless the available maps array has changed
        const websockets = Array.from(wss.clients).filter((ws) => {
            return ws !== exclude_ws;
        });
        send_to_websockets(websockets, res);
        last_available_maps_broadcasted = res;
        return;
    }
    send_json(ws,res);
}

/**
 * LEGACY: Send array of currently covered maps by the system
 * @param request_or_websocket {WebSocket|Request|null}
 */
function send_map_activity(request_or_websocket = null) {
    const maps_by_client = Object.keys(bot_clients).map((key) => {
        const bot = bot_clients[key];
        return {
            mapId:bot.map_id,
            district:district_from_region(bot.district_region),
            lastUpdate:(new Date(bot.last_message)).toJSON(),
            party_count:(parties_by_client[key] || []).length
        };
    });

    const ret = JSON.stringify(maps_by_client);

    if(request_or_websocket === null) {
        send_to_websockets(get_user_websockets(), ret);
        return;
    }
    send_json(request_or_websocket,ret);
}

/**
 * LEGACY: Send all party searches to legacy clients
 * @param request_or_websocket
 */
function send_searches(request_or_websocket = null) {
    const parties_by_map_and_district = {};
    all_parties.forEach((party) => {
        if(parties_by_map_and_district[`${party.map_id}-${party.district}`])
            return;
        parties_by_map_and_district[`${party.map_id}-${party.district}`] = {
            map_id:party.map_id,
            district:party.district,
            parties:all_parties.filter((checkParty) => {
                return checkParty.map_id === party.map_id && checkParty.district === party.district;
            })
        };
    });
    const ret = JSON.stringify(Object.values(parties_by_map_and_district));

    if(request_or_websocket === null) {
        send_to_websockets(get_user_websockets(), ret);
        return;
    }
    send_json(request_or_websocket,ret);
}

/**
 * Send full list of current party searches in response to a http request or websocket
 * @param request_or_websocket {WebSocket|Request}
 */
function send_all_parties(request_or_websocket = null) {
    const ret = JSON.stringify({
        "type":"all_parties",
        "parties":all_parties
    });

    if(request_or_websocket === null) {
        send_to_websockets(get_user_websockets(), ret);
        return;
    }
    send_json(request_or_websocket,ret);
}

/**
 * Handler for incoming message from http request or websocket
 * @param request {Request|WebSocket}
 * @param data {Object}
 * @returns {Promise<void>}
 */
async function on_request_message(request, data) {
    assert(data,"no data");
    if(data.compression && is_websocket(request)) {
        request.compression = data.compression;
        send_header(request, 200,`Compression for websocket ${get_ip_from_request(request)} set to ${data.compression}`);
        return;
    }
    switch(data.type || '') {
        case "map_id":
            const map_id = to_number(data.map_id);
            request.map_id = map_id;
            send_map_parties(map_id, request);
            break;
        case "client_parties":
            on_recv_parties(request, data);
            break;
        case "available_maps":
            send_available_maps(request);
            break;
        case "map_parties":
            send_map_parties(to_number(data.map_id),request);
            break;
        case "all_parties":
            send_all_parties(request);
            break;
        default:
            send_header(request, 400,"Invalid or missing type parameter");
            break;
    }
}

/**
 * Handler for incoming http request
 * @param request {Request}
 * @param response {Response}
 */
async function on_http_message(request,response) {
    console.log(`Http message ${request.method} ${request.url}`);
    try {
        let data = await get_data_from_request(request);
        if(!data) {
            response.sendStatus(500);
            return;
        }
        console.log("Http data type %s",data.type || 'unknown');
        await on_request_message(request, data);
        send_header(request, 200);
    } catch(e) {
        console.error(e);
        send_header(request, 500,e.message);
    }
}

/**
 * Handler for incoming websocket messag
 * @param data {string}
 * @param ws {WebSocket}
 * @returns {Promise<void>}
 */
async function on_websocket_message(data, ws) {
    try {
        data = json_parse(data);
        await on_request_message(ws, data);
    } catch(e) {
        console.error(e);
        send_header(ws,500,e.message);
    }
}

const app = express();
app.use(bodyParser.text({type: '*/*'}));
app.use(morgan("tiny"));
let two_week_cache = build_express_cache_options(14*24*60*60*1000);
app.set('etag', 'strong');
app.set('x-powered-by', false);
app.use(function (req, res, next) {
    res.removeHeader("date");
    res.set('X-Clacks-Overhead',"GNU Terry Pratchett"); // For Terry <3
    next();
});
app.use('/', express.static(path.join(__dirname,'dist'),two_week_cache));
app.use('/api', on_http_message);
app.use('/status/map-activity',(request,response) => {
    try {
        send_map_activity(request);
    } catch(e) {
        console.error(e);
        send_header(request, 500,e.message);
    }
});

const http_server = http.createServer();
http_server.on('request', app);
http_server.listen(80, function() {
    console.log("http/ws server listening on port 80");
});

const wss = start_websocket_server(http_server);
wss.on('connection', function connection(ws, request) {
    ws.headers = request.headers;
    ws.ip = get_ip_from_request(request);
    console.log(`[websocket]`,ws.ip,"connected");
    if(get_client_id(ws)) {
        try {
            add_bot_client(ws);
        } catch(e) {
            console.error(`[websocket]`,ws.ip,e.message);
            ws.ignore = 1;
            send_header(ws, 403,e.message);
            setTimeout(() => ws.close(),5000);
            return;
        }
    }
    ws.originalSend = ws.send;
    ws.send = (data) => {
        if(ws.ignore) return;
        console.log(`[websocket]`,ws.ip,`-->`,data);
        ws.originalSend(data);
    }
    ws.map_id = 0;
    send_available_maps(ws, true);
    if(!ws.is_bot_client) {
        send_all_parties(ws);
    }
    ws.on('message',(data) => {
        if(ws.ignore)
            return;
        ws.last_message = new Date();
        data = data.toString();

        if(ws.compression === 'lz') {
            try {
                const decompressed = LZString.decompressFromUTF16(data.toString());
                if(decompressed && decompressed !== "@@@\u0000")
                    data = decompressed;
                console.log("Decompressed");
            } catch(e) {
                console.error(e.message);
                send_header(ws, 500, e.message);
                return;
            }
        }

        console.log(`[websocket]`,ws.ip,`<--`,data);
        on_websocket_message(data,ws).catch((e) => {
            console.error(e);
        })
    });
    ws.on('close',() => {
        console.log(`[websocket]`,ws.ip,"closed");
        if(ws.client_id)
            on_bot_disconnected(ws);
    })
});

function get_bot_websockets() {
    return Array.from(wss.clients).filter((ws) => {
        return ws.is_bot_client;
    });
}
function get_user_websockets() {
    return Array.from(wss.clients).filter((ws) => {
        return !ws.is_bot_client;
    });
}
function send_to_websockets(websockets, obj) {
    const compressed = LZString.compressToUTF16(obj);
    websockets.forEach((ws) => {
        if(ws.compression === 'lz')
            ws.send(compressed);
        else
            ws.send(obj);
    })
}
