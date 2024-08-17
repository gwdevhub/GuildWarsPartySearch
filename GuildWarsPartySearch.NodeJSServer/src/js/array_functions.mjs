/**
 *
 * @param arr {Array}
 * @param cb {Function}
 * @returns {Array}
 */
export const unique = (arr,cb) => {
    let out = {};
    arr.forEach((item) => {
        const res = cb(item);
        if(out[res])
            return;
        out[res] = item;
    })
    return Object.values(out);
}

export const groupBy = (arr,cb) => {
    let out = {};
    arr.forEach((item) => {
        const res = cb(item);
        out[res] = out[res] || [];
        out[res].push(item);
    })
    return out;
}

/**
 *
 * @param obj Object
 * @param check function
 */
export const find_if = (obj,check) => {
    if(!(obj && check))
        return;
    return Object.keys(obj).filter((key) => {
        const entry = obj[key];
        return check(entry);
    }).map((key) => {
        return obj[key];
    });
}
/**
 *
 * @param obj
 * @param check
 */
export const delete_if = (obj,check) => {
    if(!(obj && check))
        return;
    Object.keys(obj).forEach((key) => {
        if(check(obj[key]))
            delete obj[key];
    });
}
