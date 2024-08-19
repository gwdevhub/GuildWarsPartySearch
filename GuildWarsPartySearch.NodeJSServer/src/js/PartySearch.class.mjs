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

const json_keys = {
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
        this.message = json.message || json[json_keys['message']] || '';
        this.sender = json.sender || json[json_keys['sender']] || '';
        this.party_id = to_number(json.party_id || json[json_keys['i']] || 0);
        this.hardmode = to_number(json.hardmode || json[json_keys['hardmode']] || 0);
        this.party_size = to_number(json.party_size || json[json_keys['party_size']] ||  1);
        this.hero_count = to_number(json.hero_count || json[json_keys['hero_count']] || 0);
        this.level = to_number(json.level || json[json_keys['level']] || 20);
        this.search_type = to_number(json.search_type || json[json_keys['search_type']]);
        this.primary = to_number(json.primary || json[json_keys['primary']] || 0);
        this.secondary = to_number(json.secondary || json[json_keys['secondary']] || 0);
        this.district_number = to_number(json.district_number || json[json_keys['district_number']] || 0);
        this.district_region = to_number(json.district_region || json[json_keys['district_region']] || 0);
        this.district_language = to_number(json.district_language || json.language || json[json_keys['district_language']] || 0);
        this.map_id = to_number(json.map_id || json[json_keys['map_id']] || 0);

        this.validate();

        this.district = json.district || json[json_keys['district']] || district_from_region(this.district_region);
    }
    toJSON() {
        const json_to_set = [
            'message',
            'sender',
            'hardmode',
            'party_size',
            'hero_count',
            'search_type',
            'district',
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
            obj[json_keys[key]] = obj[key];
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
