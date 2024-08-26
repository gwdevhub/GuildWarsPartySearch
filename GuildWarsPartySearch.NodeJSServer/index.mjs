import './src/libs/leaflet/1.9.4/leaflet.js';
import './src/libs/moment/2.29.1/moment.min.js';
import {
    getMapInfo,
    getMapName,
    party_search_types,
    districts,
    district_regions,
    getDistrictName, district_region_info, region_from_district
} from "./src/js/gw_constants.mjs";
import {is_numeric, to_number} from "./src/js/string_functions.mjs";
import {groupBy, unique} from "./src/js/array_functions.mjs";

import map_data_0 from  './src/data/0.json';
import map_data_1 from  './src/data/1.json';
import map_data_2 from  './src/data/2.json';
import map_data_3 from  './src/data/3.json';
import map_data_4 from  './src/data/4.json';
import map_data_5 from  './src/data/5.json';
import {PartySearch} from "./src/js/PartySearch.class.mjs";
import {get_websocket_client, is_websocket_ready, start_websocket_client} from "./src/js/websocket_client.mjs";
import {sleep} from "./src/js/sleep.mjs";
import LZString from "./src/js/lz-string.min.mjs";
import {assert} from "./src/js/assert.mjs";

const leafletData = [
    map_data_0,
    map_data_1,
    map_data_2,
    map_data_3,
    map_data_4,
    map_data_5
];

let loading = true;

let urlCoordinates = {
    v: getURLParameter("v"),
    x: getURLParameter("x"),
    y: getURLParameter("y"),
    z: getURLParameter("z"),
    c: getURLParameter("c")
};

let currentContinent = urlCoordinates.c || 1;
let navigating = 0;

const markerSize = {
    "dungeon": "small",
    "arena": "medium",
    "arena_cantha": "large",
    "arena_elona": "large",
    "challenge_cantha": "large",
    "challenge_cantha_kurzick": "large",
    "challenge_cantha_luxon": "large",
    "challenge_elona": "large",
    "challenge_realm": "large",
    "city": "medium",
    "city_cantha": "large",
    "city_cantha_kurzick": "large",
    "city_cantha_luxon": "large",
    "city_elona": "large",
    "city_realm": "large",
    "guildhall": "medium",
    "gate": "medium",
    "gate_of_anguish": "large",
    "outpost": "small",
    "outpost_cantha": "medium",
    "outpost_cantha_kurzick": "medium",
    "outpost_cantha_luxon": "medium",
    "outpost_elona": "medium",
    "outpost_realm": "medium",
    "mission": "large",
    "mission_cantha": "large",
    "mission_cantha_kurzick": "large",
    "mission_cantha_luxon": "large",
    "mission_elona": "large",
    "mission_realm": "large",
    "mission_eotn": "medium",
    "zaishen": "large",
    "travel": "large",
    "vortex": "large"
};
let available_maps = [];
let all_parties = [];
let chosen_map_id = 0;
let map = null; // Leaflet map

const menu = document.querySelector('#menu');
const partyList = document.querySelector(".partyList");
const popupWindowTitle = document.querySelector("#partyWindowTitle");
const partyWindowDistricts = document.querySelector("#partyWindowDistricts");
const partyWindow = document.querySelector("#partyWindow");
const partyWindowTbody = document.querySelector("#partyWindowTable tbody");

partyWindow.addEventListener('click',(event) => {
    event.stopPropagation();
    toggleElement(partyWindow);
    return false;
})
partyWindow.querySelector('.popupInner').addEventListener('click',(event) => {
    event.stopPropagation();
    return false;
})

document.querySelectorAll('.mapLink').forEach((element) => {
    element.addEventListener('click',(event) => {
        loadMap(to_number(element.getAttribute('data-id')));
    })
})
document.querySelectorAll('.toggleElement').forEach((element) => {
    element.addEventListener('click',(event) => {
        const element = document.querySelector(event.currentTarget.getAttribute('data-selector'));
        if(element) toggleElement(element);
    })
})
document.querySelectorAll('input.search_type_toggle').forEach((element) => {
    element.addEventListener('change',() => {
        redrawPartyWindow();
    })
})

async function onMapChanged() {
    send_map_id(chosen_map_id);
    if(!chosen_map_id) {
        window.location.hash = '';
        return;
    }
    window.location.hash = getMapName(chosen_map_id);
    redrawPartyWindow();
    redrawPartyList();
    await navigateToLocation(chosen_map_id, true);
    toggleElement(partyWindow,true);
}

async function selectMapId(map_id) {
    if(!(map_id && is_numeric(map_id) && getMapInfo(map_id)))
        return;
    chosen_map_id = to_number(map_id);
    await onMapChanged();
}

function updateHash() {
    const zoom = map.getZoom();
    const point = project(map.getCenter());

    const url = new URL(window.location.href);
    url.searchParams.set("v", 1);
    url.searchParams.set("x", point.x);
    url.searchParams.set("y", point.y);
    url.searchParams.set("z", zoom);
    url.searchParams.set("c", currentContinent);
    window.history.replaceState({}, '', url);
}

function getURLParameter(name) {
    return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [null, ''])[1].replace(/\+/g, '%20')) || null;
}

function toggleElement(element, show) {
    const is_hidden = () => { return element.classList.contains("hidden"); };
    const was_hidden = is_hidden();
    if(typeof show === 'undefined')
        show = was_hidden;
    if((show && element.classList.contains("hidden"))
        || (!show && !element.classList.contains("hidden")))
        element.classList.toggle("hidden");
    if(element === partyWindow
    && !was_hidden && is_hidden()
    && chosen_map_id) {
        // Party window hidden, set map id to 0 to avoid server sending info
        selectMapId(0);
    }
}

function redrawPartyList() {
    //console.log('redrawPartyList',all_parties);
    const by_map = groupBy(available_maps,(available_map) => {
        return available_map.map_id;
    });
    const party_list_html = Object.keys(by_map).map((map_id) => {
        // Across all districts for this map
        const aggregate_party_count = by_map[map_id].reduce((currentValue, available_map) => {
            return currentValue + available_map.party_count;
        },0);
        const map_info = getMapInfo(map_id);
        //console.log(map_id,map_info);
        if(!map_info) return '';
        return `<div><div class="mapRow" data-map-id="${map_id}">${map_info.name} - ${aggregate_party_count}</div>`
    }).join('');
    partyList.innerHTML = party_list_html;
    partyList.querySelectorAll(".mapRow").forEach((element) => {
        element.addEventListener('click',(event) => {
            selectMapId(to_number(event.currentTarget.getAttribute('data-map-id')));
        })
    })
}

function getLeafletLocationByMapId(map_id) {
    map_id = to_number(map_id);
    for (const continentData of leafletData) {
        for (const region of continentData.regions || []) {
            for (const location of region.locations || []) {
                if(location && to_number(location.mapId || 0) === map_id)
                    return {location:location,region:region,continent:continentData};
            }
        }
    }
    return null;
}

async function navigateToLocation(map_id, showParties) {
    const leaflet_location = getLeafletLocationByMapId(map_id);
    if (!leaflet_location) return;
    if (leaflet_location.continent.id !== currentContinent) {
        loadMap(leaflet_location.continent.id);
    }

    await waitForLoaded();
    if (!map) {
        return;
    }

    const pixelBounds = map.getPixelBounds();
    const desiredBounds = pixelBounds.getSize().divideBy(2);
    const quarterSize = pixelBounds.getSize().divideBy(4);
    const center = project(map.getCenter());

    const coordinates = leaflet_location.location.coordinates;

    const reducedPixelBounds = L.bounds(L.point(center.x - quarterSize.x, center.y - quarterSize.y), L.point(center.x + quarterSize.x, center.y + quarterSize.y));
    if (reducedPixelBounds.contains(coordinates)) {
        //selectMapId(leaflet_location.location.mapId);
        return;
    }

    // Adjust the offset by the zoom level
    const currentZoom = map.getZoom();
    const baseZoom = map.getMaxZoom();
    const scaleFactor = map.getZoomScale(baseZoom, currentZoom);

    const offset = L.point(
        (coordinates[0] - center.x) / scaleFactor,
        (coordinates[1] - center.y) / scaleFactor
    );
    navigating = 1;
    map.panBy(offset, { animate: true, duration: 1 });
    // Map panBy is not blocking. We need to wait until the animation is finished before we finish navigation
    setTimeout(() => {
        navigating = 0;
    }, 1200);

}

function get_parties_for_map(map_id) {
    return all_parties.filter((party) => {
        return party.map_id === chosen_map_id;
    });
}

function redrawPartyWindow() {
    if(!chosen_map_id)
        return;
    const map_info = getMapInfo(chosen_map_id);
    assert(map_info);
    const parties = get_parties_for_map(chosen_map_id);
    popupWindowTitle.textContent = `Party Search - ${getMapName(chosen_map_id)}`;

    let listening_districts = unique(available_maps,(available_map) => {
        return `${available_map.map_id}-${region_from_district(available_map.district)}`
    }).filter((available_map) => {
        return available_map.map_id === chosen_map_id;
    }).map((available_map) => {
        const district_region = region_from_district(available_map.district);
        const info = district_region_info[district_region];
        assert(info, `No district_region_info(${district_region})`);
        return `<span title="${info.name}">${info.abbr}</span>`;
    }).join(', ');
    if(!listening_districts)
        listening_districts = 'None';
    partyWindowDistricts.innerHTML = listening_districts;

    let party_rows_html = '';
    Object.keys(party_search_types).forEach((name) => {
        const search_type_id = party_search_types[name];
        const is_shown = partyWindow.querySelector(`input.search_type_toggle[data-id="${search_type_id}"]:checked`);
        const row_class = `search_type_${search_type_id}`;
        const parties_by_type = parties.filter((party) => {
            return party.search_type === search_type_id;
        });
        document.querySelector(`.search_type_count_${search_type_id}`).innerHTML = `${parties_by_type.length}`;
        if(!parties_by_type.length)
            return;
        if(!is_shown)
            return;
        party_rows_html += `<tr class="divider row" id="search_type_divider_${search_type_id}">
            <th colSpan="5">${name} - ${parties_by_type.length}</th>
        </tr>`


        party_rows_html += parties_by_type.map((party) => {
            return `<tr class="${row_class} hide-on-mobile row small centered party-search-result">
            <td class="text-start">${party.sender || ''}</td>\
            <td class="minw-10rem text-nowrap">${getDistrictName(party.district_region) || ''}</td>\
            <td>${party.party_size}/${map_info.max_party_size}</td>\
            <td>${party.district_number}</td>\
            <td class="text-start">${party.message || ''}</td>\
        </tr>
        <tr class="${row_class} show-on-mobile row small centered party-search-result">
            <td class="text-start">
                <div class="d-flex">
                    <div class="w-40">${party.sender || ''}</div>
                    <div class="w-20 text-center">${party.party_size}/${map_info.max_party_size}</div>
                    <div class="w-40 text-end">${getDistrictName(party.district_region)} ${party.district_number}</div>
                </div>
                <div class="party-search-result-message">${party.message}</div>
            </td>
        </tr>`;
        }).join('');
    });
    partyWindowTbody.innerHTML = party_rows_html;
}

function unproject(coord) {
    return map.unproject(coord, map.getMaxZoom());
}

function project(coord) {
    return map.project(coord, map.getMaxZoom());
}

/**
 * Trigger a (re)draw of the world map to view a new continent
 * @param continent {number}
 * @return {Promise<void>}
 */
async function loadMap(continent) {
    toggleElement(menu,false);
    currentContinent = continent;

    let maplinks = document.querySelectorAll(".mapLink");
    for (let m = 0; m < maplinks.length; m++) {
        maplinks[m].classList.remove("selected");
    }

    let mapLink = document.querySelector(".mapLink[data-id='" + continent + "']");

    if (mapLink !== null) {
        mapLink.classList.add("selected");
    }

    if (map) {
        map.off();
        map.remove();
    }

    loading = true;
    const mapData = leafletData[continent];

    document.title = "Guild Wars Party Search [" + mapData.name + "]";

    map = L.map("mapdiv", {
        minZoom: mapData.minZoom,
        maxZoom: mapData.maxZoom,
        zoom: mapData.zoom,
        crs: L.CRS.Simple,
        attributionControl: false
    }).on('click', function (e) {
    }).on('zoomend', function () {
        zoomEnd();
    }).on('moveend zoomend', function () {
        if (loading) {
            loading = false;
        } else {
            updateHash();
        }
    }).on('click focus movestart', function () {
        toggleElement(menu,false);
        if (!navigating) {
            window.location.hash = '';
            toggleElement(partyWindow,false);
        }
    });

    const mapbounds = new L.LatLngBounds(unproject([0, 0]), unproject(mapData.dims));
    map.setMaxBounds(mapbounds);
    if (urlCoordinates.x !== null && urlCoordinates.y !== null && urlCoordinates.z !== null) {
        map.setView(unproject([urlCoordinates.x, urlCoordinates.y]), urlCoordinates.z);
        urlCoordinates.x, urlCoordinates.y, urlCoordinates.z = null;
    } else if (mapData.center !== undefined) {
        map.setView(unproject(mapData.center), 4);
    } else {
        map.setView(unproject([mapData.dims[0] / 2, mapData.dims[1] / 2]), 4);
    }

    map.addLayer(L.tileLayer("tiles/" + mapData.id + "/{z}/{x}/{y}.jpg", { minZoom: 0, maxZoom: 4, continuousWorld: true, bounds: mapbounds }));

    for (let r = 0; r < mapData.regions.length; r++) {

        let region = mapData.regions[r];

        if (region.areas !== undefined) {
            for (let a = 0; a < region.areas.length; a++) {
                areaLabel(region.areas[a]).addTo(map);
            }
        }

        if (region.locations !== undefined) {
            for (let o = 0; o < region.locations.length; o++) {
                locationMarker(region.locations[o]).addTo(map);
            }
        }
    }

    zoomEnd();
    await updateMarkers();
}

function zoomEnd() {

    let zoom = map.getZoom();
    let locationLabels = document.querySelectorAll(".marker_location .label");
    let areaLabels = document.querySelectorAll(".marker_area .label");
    let icons = document.querySelectorAll(".icon");


    for (let l = 0; l < locationLabels.length; l++) {
        locationLabels[l].classList.remove("hidden");
    }

    for (let l = 0; l < areaLabels.length; l++) {
        areaLabels[l].classList.remove("hidden");
    }

    for (let i = 0; i < icons.length; i++) {
        icons[i].classList.remove("hidden");
        icons[i].classList.remove("scaled_70");
        icons[i].classList.remove("scaled_40");
    }

    if (zoom === 3) {
        for (let l = 0; l < locationLabels.length; l++) {
            locationLabels[l].classList.add("hidden");
        }

        for (let i = 0; i < icons.length; i++) {
            icons[i].classList.add("scaled_70");
        }
    } else if (zoom === 2) {

        for (let l = 0; l < locationLabels.length; l++) {
            locationLabels[l].classList.add("hidden");
        }

        for (let l = 0; l < areaLabels.length; l++) {
            areaLabels[l].classList.add("hidden");
        }

        for (let i = 0; i < icons.length; i++) {
            icons[i].classList.add("scaled_40");
        }
    } else if (zoom < 2) {
        for (let l = 0; l < locationLabels.length; l++) {
            locationLabels[l].classList.add("hidden");
        }

        for (let l = 0; l < areaLabels.length; l++) {
            areaLabels[l].classList.add("hidden");
        }

        for (let i = 0; i < icons.length; i++) {
            icons[i].classList.add("hidden");
        }
    }
}

function areaLabel(data) {

    let wiki = data.name.replace(/ /g, "_");
    if (data.wiki !== undefined) {
        wiki = data.wiki;
    }

    return L.marker(unproject(data.coordinates), {
        icon: L.divIcon({
            iconSize: null,
            className: "marker marker_area seethru unclickable",
            html: "<div class='holder'><span class='label'>" + data.name + "</span></div>"
        }),
        options: {
            wiki: wiki
        }
    });
}

function locationMarker(data) {
    const icon_html = `<div class="holder" data-map-id="${data.mapId || ''}">
<div class="icon" style="background-image:url('resources/icons/${data.type}.png');"></div>
<div class="label">${data.label || data.name}</div>
</div>`;
    let className = `marker marker_location ${markerSize[data.type] || ''} `;
    if(!is_map_available(data.mapId || 0)) {
        className += `seethru`;
    }
    return L.marker(unproject(data.coordinates), {
        icon: L.divIcon({
            iconSize: null,
            className: className,
            html:icon_html
        }),
        options: {
            wiki: data.wiki || data.name.replace(/ /g, "_"),
            mapId: data.mapId || 0
        }
    }).on('click', function (e) {
        if (map.getZoom() > 1) {
            selectMapId(e.target.options.options.mapId);
        }
    });
}

function is_map_available(map_id) {
    map_id = to_number(map_id);
    return available_maps.find((available_map) => {
        return available_map.map_id === map_id;
    });
}

function updateMarkers() {
    document.querySelectorAll(".marker.marker_location .holder[data-map-id]").forEach((element) => {
        if(!is_map_available(element.getAttribute('data-map-id') || 0)) {
            element.parentElement.classList.add('seethru');
        } else {
            element.parentElement.classList.remove('seethru');
        }
    });
}

/**
 *
 * @param {string} data
 */
function onWebsocketMessage(data) {
    if(data === 'pong')
        return;
    try {
        data = JSON.parse(data);
    } catch(e) {
        console.warn("Failed to parse json from websocket message");
        return;
    }
    data.type = data.type || null;
    if(data.Searches) {
        locationMap.clear();
        data.Searches.forEach(searchEntry => {
            let combinedKey = `${searchEntry.map_id}-${searchEntry.district}`;
            locationMap.set(combinedKey, searchEntry);
        });
    }
    switch(data.type) {
        case "all_parties":
            all_parties = (data.parties || []).map((json) => {
                return new PartySearch(json);
            })
            break;
        case "map_parties":
            // Parties for a specific map; flush any existing parties for this map and repopulate
            const map_id = to_number(data.map_id);
            const other_parties = all_parties.filter((party) => {
                return party.map_id !== map_id;
            });
            all_parties = (data.parties || []).map((json) => {
                json.map_id = map_id;
                return new PartySearch(json);
            }).concat(other_parties);
            break;
        case "available_maps":
            available_maps = data.available_maps || [];
            break;
    }
    updateMarkers();
    redrawPartyList();
    redrawPartyWindow();
    if(!chosen_map_id && all_parties.length) {
        const parties_by_map = groupBy(all_parties,(party) => {
            return party.map_id;
        });
        const sorted_by_count = Object.keys(parties_by_map).map((key) => {
            return {map_id:key,count:parties_by_map[key].length};
        }).sort((a,b) => {
            return a.count > b.count ? 1 : 0;
        });
        if(sorted_by_count.length) {
            selectMapId((sorted_by_count[0].map_id));
        }
    }
}

/**
 * Let the server know we're viewing a new map id
 */
function send_map_id() {
    const ws = get_websocket_client();
    if(chosen_map_id && is_websocket_ready(ws))
        ws.send(JSON.stringify({"type":"map_id","map_id":chosen_map_id}));
}

/**
 * Periodically make sure the websocket connection is maintained
 * @return {Promise<void>}
 */
async function check_websocket() {
    try {
        if(!is_websocket_ready(get_websocket_client())) {
            const ws = await start_websocket_client();
            if(!ws) return;
            ws.addEventListener('message',(event) => {
                let data = event.data;
                let decompressed = LZString.decompressFromUTF16(data);
                if(decompressed === "@@@\u0000")
                    decompressed = null;
                if(decompressed) {
                    data = decompressed;
                    console.log(`[websocket]`,`!-->`,decompressed);
                } else {
                    console.log(`[websocket]`,`-->`,data);
                }

                onWebsocketMessage(data);
            });
            ws.sendOriginal = ws.send;
            ws.send = (data) => {
                if(ws.compression === 'lz') {
                    const compressed = LZString.compressToUTF16(data);
                    const decompressed = LZString.decompressFromUTF16(compressed);
                    assert(decompressed === data);
                    console.log(`[websocket]`,`!<--`,data);
                    data = compressed;
                } else {
                    console.log(`[websocket]`,`<--`,data);
                }
                ws.sendOriginal(data);
            }
            ws.send(JSON.stringify({"compression":"lz"}));
            ws.compression = "lz";
            console.log("Websocket message compression set to LZW, see https://pieroxy.net/blog/pages/lz-string/index.html for examples");
            send_map_id();
        }
    } catch(e) {
        console.log(e.message);
    }
    await sleep(5000);
    check_websocket();

}
loadMap(currentContinent).then(() => {
    if(partyWindow.classList.contains('hidden'))
        toggleElement(menu,true);
})

check_websocket();

async function waitForLoaded(){
    while (loading) {
        await new Promise(resolve => setTimeout(resolve, 50));
    }
}
