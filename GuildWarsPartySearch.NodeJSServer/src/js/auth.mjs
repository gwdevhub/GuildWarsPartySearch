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

    const api_key = request.headers['x-api-key'] || 'no-api-key';
    const gwtoolbox_api_key = /gwtoolbox-(\S+)-(\d+)/.exec(api_key);
    if(gwtoolbox_api_key) {
        // TODO: Check supported toobox version, compare dll file size
        const gwtoolbox_version = gwtoolbox_api_key[1];
        const gwtoolbox_dll_size = gwtoolbox_api_key[2];
        return true;
    }
    if(config.bot_api_keys.length && !config.bot_api_keys.includes(api_key))
        return false;
    return true;
}