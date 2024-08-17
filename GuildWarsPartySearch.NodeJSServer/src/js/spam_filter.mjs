import * as fs from "fs";
import path from 'path';
import { fileURLToPath } from 'url';
import {removeDiacritics, removePunctuation, removeSpaces, removeUnderscores} from "./string_functions.mjs";

const __filename = fileURLToPath(import.meta.url); // get the resolved path to the file
const __dirname = path.dirname(__filename); // get the name of the directory

let quarantine_regexes = [];
try {
    const regex_strings = (fs.readFileSync(__dirname+'/chat_filter_regexes.txt')+'').split('\n');
    for(let i=0;i<regex_strings.length;i++) {
        regex_strings[i] = regex_strings[i].trim();
        if(!regex_strings[i].length)
            continue;
        quarantine_regexes.push(new RegExp(regex_strings[i],'i'));
    }
    console.log("Chat filter regexes: ",quarantine_regexes);

} catch(e) {
    console.error("Failed to parse "+__dirname+'/chat_filter_regexes.txt');
    console.error(e);
}

/**
 *
 * @param str {string}
 * @returns {boolean}
 */
export function is_quarantine_hit(str) {
    if(!str)
        return false;
    // True on match
    let msg_norm_manual = removeSpaces(removeUnderscores(removePunctuation(removeDiacritics(str))));
    for(let i in quarantine_regexes) {
        if(quarantine_regexes[i].test(msg_norm_manual))
            return true;
    }
    return false;
}