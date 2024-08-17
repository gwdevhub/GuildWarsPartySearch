
// True on match
import {pad, removeDiacritics, removePunctuation, removeSpaces, removeUnderscores} from "../src/string_functions.mjs";
import {is_quarantine_hit} from "../src/spam_filter.mjs";

let str = process.argv[2];
//var msg_norm_auto = message.m.normalize("NFD").replace(/[\u0300-\u036f]/g, "")
let msg_norm_manual = removeSpaces(removeUnderscores(removePunctuation(removeDiacritics(str))));
console.log("message normalised:\n"+msg_norm_manual+"\n");

let arr = msg_norm_manual.split('').map(function(el) {
    return pad(el.charCodeAt(0).toString(16),4);
})
console.log(arr);
if(is_quarantine_hit(str)) {
    console.log("Quarantine Hit!");
} else {
    console.log("Quarantine pass");
}