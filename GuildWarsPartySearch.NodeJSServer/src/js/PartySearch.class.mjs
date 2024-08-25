import {assert} from "./assert.mjs";
import {
    district_from_region,
    district_regions, getDistrictAbbreviation,
    getDistrictName, getMapInfo,
    getMapName,
    languages, map_info,
    party_search_types
} from "./gw_constants.mjs";
import {to_number} from "./string_functions.mjs";

export const party_json_keys = {
    'party_id':'i',
    'party_size':'ps',
    'sender':'s',
    'message':'ms',
    'hero_count':'hc',
    'search_type':'t',
    'map_id':'m',
    'district':'d',
    'hardmode':'hm',
    'district_number':'dn',
    'district_region':'dr',
    'district_language':'dl',
    'level':'1',
    'primary':'p',
    'secondary':'sc'
}

export class PartySearch {
    constructor(json) {
        this.client_id = json.client_id || '';
        this.message = json.message || json[party_json_keys['message']] || '';
        this.sender = json.sender || json[party_json_keys['sender']] || '';
        this.party_id = to_number(json.party_id || json[party_json_keys['i']] || 0);
        this.hardmode = to_number(json.hardmode || json[party_json_keys['hardmode']] || 0);
        this.party_size = to_number(json.party_size || json[party_json_keys['party_size']] ||  1);
        this.hero_count = to_number(json.hero_count || json[party_json_keys['hero_count']] || 0);
        this.level = to_number(json.level || json[party_json_keys['level']] || 20);
        this.search_type = to_number(json.search_type || json[party_json_keys['search_type']]);
        this.primary = to_number(json.primary || json[party_json_keys['primary']] || 0);
        this.secondary = to_number(json.secondary || json[party_json_keys['secondary']] || 0);
        this.district_number = to_number(json.district_number || json[party_json_keys['district_number']] || 1);
        this.district_region = to_number(json.district_region || json[party_json_keys['district_region']] || 0);
        this.district_language = to_number(json.district_language || json.language || json[party_json_keys['district_language']] || 0);
        this.map_id = to_number(json.map_id || json[party_json_keys['map_id']] || 0);

        this.validate();

        this.district = json.district || json[party_json_keys['district']] || district_from_region(this.district_region);
    }

    update(json) {
        Object.keys(party_json_keys).forEach((full_key) => {
            const abbr_key = party_json_keys[full_key];
            if(json.hasOwnProperty(abbr_key))
                json[full_key] = json[abbr_key];
        });
        if(json.hasOwnProperty('message'))
            this.message = json.message;
        if(json.hasOwnProperty('sender'))
            this.sender = json.sender;
        // NB: Map and district can't change in practive without the party being removed anyway
        ['hardmode','parry_size','hero_count','level','search_type','primary','secondary'].forEach((key) => {
            if(json.hasOwnProperty(key))
                this[key] = to_number(json[key]);
        });
        this.validate();
    }

    /**
     *
     * @param full
     * @return {{}}
     */
    toJSON(full = false) {
        const json_to_set = [
            'message',
            'sender',
            'hardmode',
            'party_size',
            'hero_count',
            'search_type',
            'district_region',
            'district_number',
            'map_id',
            'level',
            'primary',
            'secondary'
        ];
        let obj = {};
        json_to_set.forEach((key) => {
            obj[key] = this[key];
        });
        // Unset presumed defaults
        if(obj.district_region === 0)
            delete obj.district_region;
        if(obj.district_number === 1)
            delete obj.district_number;
        if(obj.party_size === 1)
            delete obj.party_size;
        if(obj.hero_count === 0)
            delete obj.hero_count;
        if(!obj.message)
            delete obj.message;
        if(!obj.hardmode)
            delete obj.hardmode;
        if(!obj.secondary)
            delete obj.secondary;
        if(obj.level === 20)
            delete obj.level;
        // Abbreviate to reduce footprint
        Object.keys(obj).forEach((key) => {
            obj[party_json_keys[key]] = obj[key];
            delete obj[key];
        });

        return obj;
    }
    validate() {
        //assert(typeof this.client_id === 'string' && this.client_id.length);
        assert(typeof this.message === 'string');
        assert(typeof this.sender === 'string' && this.sender.length);
        //assert(typeof this.party_id === 'number' && this.party_size > 0);
        assert(typeof this.party_size === 'number' && this.party_size > 0);
        assert(typeof this.hero_count === 'number' && this.hero_count < this.party_size);
        assert(typeof this.map_id === 'number' && this.map_id > 0 && map_info[this.map_id]);
        assert(typeof this.district_number === 'number');
        assert(typeof this.search_type === 'number' && Object.values(party_search_types).includes(this.search_type));
        assert(typeof this.district_region === 'number' && Object.values(district_regions).includes(this.district_region));
        assert(typeof this.district_language === 'number' && Object.values(languages).includes(this.district_language));
    }

    get map_name() {
        return getMapName(this.map_id);
    }
    get map_info() {
        return getMapInfo(this.map_id);
    }
    get district_abbr() {
        return `${getDistrictAbbreviation(this.district_region,this.district_language)} - ${this.district_number}`;
    }
    get district_name() {
        return `${getDistrictName(this.district_region)}, district ${this.district_number}`
    }
    get search_type_name() {
        return party_search_types[this.search_type];
    }
}
