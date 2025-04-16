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
    let found = 0;
    for(let i=0;i<str.length;i++) {
        // Get the Unicode code point for the character
        const codePoint = str.codePointAt(i);

        // Skip basic ASCII characters (0-127) and common extended Latin characters
        if(codePoint <= 0x7F || (codePoint >= 0xA0 && codePoint <= 0x24F)) continue;

        // Adjust for surrogate pairs
        if (codePoint > 0xFFFF) {
            i++; // Skip the second part of the surrogate pair
        }

        found++;
        if(found > 5)
            return true;
    }
    // True on match
    let msg_norm_manual = removeSpaces(removeUnderscores(removePunctuation(removeDiacritics(str))));
    for(let i in quarantine_regexes) {
        if(quarantine_regexes[i].test(msg_norm_manual))
            return true;
    }
    return false;
}