import http from "http";
import {WebSocket} from "ws";
import {json_parse} from "./string_functions.mjs";

/**
 *
 * @param obj {Object}
 * @returns {boolean}
 */
export function is_http(obj) {
    return obj instanceof http.IncomingMessage;
}
/**
 *
 * @param obj {Object}
 * @returns {boolean}
 */
export function is_websocket(obj) {
    return obj instanceof WebSocket;
}

/**
 *
 * @param request {Request|WebSocket}
 * @param obj {Object|string}
 */
export function send_json(request,obj) {
    const json_str = typeof obj === 'string' ? obj : JSON.stringify(obj);
    if(is_websocket(request)) {
        request.send(json_str);
        return;
    }
    if(is_http(request)) {
        request.res.setHeader('Content-Type', 'application/json');
        request.res.end(json_str);
        return;
    }
    throw new Error("Invalid request object in send_json");
}

/**
 *
 * @param request {Request|WebSocket}
 * @param statusCode
 * @param statusMessage
 */
export function send_header(request,statusCode,statusMessage = '') {
    if(is_websocket(request)) {
        request.send(JSON.stringify({
            "code":statusCode,
            "message":statusMessage
        }));
        return;
    }
    if(is_http(request)) {
        if(request.res.headersSent)
            return;
        request.res.status(statusCode).send(statusMessage);
        return;
    }
    throw new Error("Invalid request object in send_header");
}


/**
 *
 * @param request {Request}
 * @returns {Promise<Object>}
 */
export async function get_data_from_request(request) {
    let data = null;
    switch(request.method.toLowerCase()) {
        case 'post':
            try {
                data = request.body;
                if(typeof data === 'string')
                    data = json_parse(data);
            } catch (e) {
                console.error(e);
            }
            break;
        case 'get':
            data = {};
            Object.keys(request.query).forEach((key) => {
                data[key] = request.query[key];
            });
            break;
    }
    return data;
}

/**
 *
 * @param request {Request|WebSocket}
 * @returns {string}
 */
export function get_ip_from_request(request) {
    let ip = '';
    if(!request)
        return ip;
    if(request.headers)
        ip = request.headers['cf-connecting-ip'] || request.headers['x-forwarded-for'] || ip;
    if(!ip && request._socket)
        ip = request._socket.remoteAddress;
    if(!ip && request.socket)
        ip = request.socket.remoteAddress;
    if(ip === '::1')
        ip = '127.0.0.1';
    return ip.split(':').pop();
}

/**
 * @param {number} cache_time_ms
 * @return {{setHeaders: setHeaders, etag: string}}
 */
export function build_express_cache_options(cache_time_ms) {
    let headers = {
        'Cache-Control': 'max-age='+(cache_time_ms/1000)+', immutable',
        'Expires':new Date(Date.now()+cache_time_ms).toUTCString()
    }
    return {
        etag:'strong',
        setHeaders:function(res) {
            res.set(headers);
        }
    }
}