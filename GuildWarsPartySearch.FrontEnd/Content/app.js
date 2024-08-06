var map;
var mapData = {};
var loading = true;

var continent = 1;

var markerSize = {
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

var locationMap = new Map();


function updateHash() {
    var zoom = map.getZoom();
    var point = project(map.getCenter());

    history.replaceState(undefined, undefined, "?v=1&x=" + point.x + "&y=" + point.y + "&z=" + zoom + "&c=" + continent);
}

function getURLParameter(name) {
    return decodeURIComponent((new RegExp('[?|&]' + name + '=' + '([^&;]+?)(&|#|;|$)').exec(location.search) || [null, ''])[1].replace(/\+/g, '%20')) || null;
}

var urlCoordinates = {
    v: getURLParameter("v"),
    x: getURLParameter("x"),
    y: getURLParameter("y"),
    z: getURLParameter("z"),
    c: getURLParameter("c")
};


function toggleMenu() {
    document.querySelector("#menu").classList.toggle("hidden");
}

function unproject(coord) {
    return map.unproject(coord, map.getMaxZoom());
}

function project(coord) {
    return map.project(coord, map.getMaxZoom());
}

function loadMap(mapIndex) {

    continent = mapIndex;
    document.querySelector("#menu").classList.add("hidden");

    let maplinks = document.querySelectorAll(".mapLink");
    for (let m = 0; m < maplinks.length; m++) {
        maplinks[m].classList.remove("selected");
    }

    let mapLink = document.querySelector(".mapLink[data-id='" + mapIndex + "']");

    if (mapLink !== null) {
        mapLink.classList.add("selected");
    }

    if (map !== undefined) {
        map.off();
        map.remove();
    }

    fetch("data/" + mapIndex + ".json?v=20200516001")
        .then((response) => {
            return response.json();
        })
        .then((data) => {
            mapData = data;

            document.title = "Guild Wars Party Search [" + mapData.name + "]";

            map = L.map("mapdiv", {
                minZoom: 0,
                maxZoom: 4,
                crs: L.CRS.Simple,
                attributionControl: false
            }).on('click', function (e) {
                //let p = project(e.latlng);
                //console.log("[" + p.x + "," + p.y + "]");
            }).on('zoomend', function () {
                zoomEnd();
            }).on('moveend zoomend', function () {
                if (loading) {
                    loading = false;
                } else {
                    updateHash();
                }
            }).on('click focus movestart', function () {
                document.querySelector("#menu").classList.add("hidden");
            });



            var mapbounds = new L.LatLngBounds(unproject([0, 0]), unproject(mapData.dims));
            map.setMaxBounds(mapbounds);
            if (urlCoordinates.x !== null && urlCoordinates.y !== null && urlCoordinates.z !== null) {
                map.setView(unproject([urlCoordinates.x, urlCoordinates.y]), urlCoordinates.z);
                urlCoordinates.x, urlCoordinates.y, urlCoordinates.z = null;
            } else if (data.center !== undefined) {
                map.setView(unproject(data.center), 4);
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
        });
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
            className: "marker marker_area seethru",
            html: "<div class='holder'><span class='label'>" + data.name + "</span></div>"
        }),
        options: {
            wiki: wiki
        }
    });
}

function locationMarker(data) {

    let wiki = data.name.replace(/ /g, "_");
    if (data.wiki !== undefined) {
        wiki = data.wiki;
    }

    let label = data.name;
    if (data.label !== undefined) {
        label = data.label;
    }

    let divId = data.mapId ? " id='" + data.mapId + "'" : "";

    return L.marker(unproject(data.coordinates), {
        icon: L.divIcon({
            iconSize: null,
            className: "marker marker_location " + markerSize[data.type] + (data.mapName === undefined ? " seethru" : ""),
            html: "<div" + divId + " class='holder'><img class='icon' src='resources/icons/" + data.type + ".png'/><div class='label'>" + label + "</div></div>"
        }),
        options: {
            wiki: wiki,
            mapName: data.mapName
        }
    }).on('click', function (e) {

        if (map.getZoom() > 1 &&
            e.target.options.options.mapName) {
            window.open(window.location.origin + "/party-search/maps/" + e.target.options.options.mapName);
        }
    });
}

if (urlCoordinates.c !== null) {
    continent = urlCoordinates.c;
}

function updateEntriesDiv() {
    let allInnerDivs = document.querySelectorAll(".marker.marker_location .holder[id]");
    allInnerDivs.forEach(function (label) {
        let parentMarker = label.closest('.marker.marker_location');
        if (parentMarker) {
            parentMarker.classList.add('seethru');
        }
    });

    locationMap.keys().forEach(function (key) {
        let matchingInnerDivs = Array.prototype.filter.call(allInnerDivs, function (innerDiv) {
            return innerDiv.id === key;
        });
        if (locationMap.get(key).parties.length > 0) {
            matchingInnerDivs.forEach(function (innerDiv) {
                let parentMarker = innerDiv.closest('.marker.marker_location');
                if (parentMarker) {
                    parentMarker.classList.remove('seethru');
                }
            });
        }
    });
}


function connectToLiveFeed() {
    const wsUrl = (window.location.protocol === 'https:' ? 'wss://' : 'ws://') + window.location.host + '/party-search/live-feed';
    let socket;
    let retryDelay = 1000;

    function openWebSocket() {
        socket = new WebSocket(wsUrl);

        socket.onopen = function (event) {
            console.log('WebSocket connection established:', event);
            retryDelay = 1000;
        };

        socket.onmessage = function (event) {
            console.log('WebSocket message received:', event);
            if (event.data === 'pong') {
                console.log('Pong received from server.');
            } else {
                let obj = JSON.parse(event.data);
                console.log(obj);
                obj.Searches.forEach(searchEntry => {
                    let combinedKey = `${searchEntry.map_id}`;
                    locationMap.set(combinedKey, searchEntry);
                });

                updateEntriesDiv();
            }
        };

        socket.onerror = function (event) {
            console.error('WebSocket error:', event);
        };

        socket.onclose = function (event) {
            console.log('WebSocket connection closed:', event);
            if (!event.wasClean) {
                console.log(`WebSocket closed unexpectedly. Reconnecting in ${retryDelay / 1000} seconds...`);
                setTimeout(openWebSocket, retryDelay);
                retryDelay = Math.min(retryDelay * 2, 30000); // Exponential backoff with a max delay of 30 seconds
            }
        };
    }

    openWebSocket();
}

loadMap(continent);
connectToLiveFeed();