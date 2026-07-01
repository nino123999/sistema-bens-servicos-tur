function downloadFileFromBase64(fileName, mimeType, base64) {
    const link = document.createElement('a');
    link.href = `data:${mimeType};base64,${base64}`;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

const roteiroMap = (() => {
    const maps = {};

    return {
        render(elementId, points) {
            if (maps[elementId]) {
                maps[elementId].remove();
                delete maps[elementId];
            }

            const el = document.getElementById(elementId);
            if (!el || points.length === 0) return;

            const map = L.map(elementId);
            maps[elementId] = map;

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors'
            }).addTo(map);

            const latLngs = [];
            points.forEach((p, i) => {
                const latlng = [p.latitude, p.longitude];
                latLngs.push(latlng);

                const marker = L.marker(latlng).addTo(map);
                marker.bindPopup(`<b>${i + 1}. ${p.title}</b><br><a href="${p.mapsUrl}" target="_blank">Ver no Maps</a>`);
            });

            if (latLngs.length > 1) {
                L.polyline(latLngs, { color: '#3b82f6', weight: 3 }).addTo(map);
            }

            const bounds = L.latLngBounds(latLngs);
            map.fitBounds(bounds, { padding: [32, 32] });
        }
    };
})();

const estabelecimentosMap = (() => {
    const maps = {};

    return {
        render(elementId, points) {
            if (maps[elementId]) {
                maps[elementId].remove();
                delete maps[elementId];
            }

            const el = document.getElementById(elementId);
            if (!el || points.length === 0) return;

            const map = L.map(elementId);
            maps[elementId] = map;

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '© OpenStreetMap contributors'
            }).addTo(map);

            const latLngs = [];
            points.forEach(p => {
                const latlng = [p.latitude, p.longitude];
                latLngs.push(latlng);

                const marker = L.marker(latlng).addTo(map);
                marker.bindPopup(`<b>${p.title}</b><br><a href="${p.mapsUrl}" target="_blank">Ver no Maps</a>`);
            });

            const bounds = L.latLngBounds(latLngs);
            map.fitBounds(bounds, { padding: [32, 32] });
        }
    };
})();
