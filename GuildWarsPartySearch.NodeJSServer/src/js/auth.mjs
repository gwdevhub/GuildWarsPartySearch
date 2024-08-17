import {get_ip_from_request, is_websocket} from "./networking.mjs";

import config from "../../config.json" with { type: "json" };

const whitelisted_ips  = [];
const blacklisted_ips = [];
const api_keys = [];

/**
 *
 * @param request {Request|WebSocket}
 * @returns {boolean}
 */
export function is_whitelisted(request) {
    if(!is_websocket(request))
        return false;
    const ip_address = get_ip_from_request(request) || 'no-ip';
    if(config.whitelisted_ips.length && !config.whitelisted_ips.includes(ip_address))
        return false;
    if(config.blacklisted_ips.includes(ip_address))
        return false;
    if(config.bot_api_keys.length && !config.bot_api_keys.includes(request.headers['x-api-key'] || 'no-api-key'))
        return false;
    return true;
}