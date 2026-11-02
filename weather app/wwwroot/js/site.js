document.addEventListener('DOMContentLoaded', () => {
    initCityAutocomplete();
    initUnitToggle();
    initGeolocation();
});

function initCityAutocomplete() {
    const input = document.getElementById('citySearchInput');
    const dropdown = document.getElementById('citySearchDropdown');
    if (!input || !dropdown) return;

    let debounceTimer;

    input.addEventListener('input', (e) => {
        const query = e.target.value.trim();
        clearTimeout(debounceTimer);

        if (query.length < 2) {
            dropdown.style.display = 'none';
            dropdown.innerHTML = '';
            return;
        }

        debounceTimer = setTimeout(async () => {
            try {
                const res = await fetch(`/Weather/Search?query=${encodeURIComponent(query)}`);
                if (!res.ok) return;
                const items = await res.json();

                if (!items || items.length === 0) {
                    dropdown.innerHTML = '<div class="p-3 text-muted text-center">No locations found</div>';
                    dropdown.style.display = 'block';
                    return;
                }

                dropdown.innerHTML = items.map(item => `
                    <div class="search-dropdown-item"
                         onclick="selectCity('${item.name.replace(/'/g, "\\'")}', '${(item.country || '').replace(/'/g, "\\'")}', ${item.latitude}, ${item.longitude})">
                        <div>
                            <strong class="text-white">${item.name}</strong>
                            <small class="text-secondary ms-1">${item.adminArea ? item.adminArea + ', ' : ''}${item.country || ''}</small>
                        </div>
                        <span class="badge bg-secondary-subtle text-secondary" style="font-size: 0.7rem;">
                            ${item.latitude.toFixed(1)}°, ${item.longitude.toFixed(1)}°
                        </span>
                    </div>
                `).join('');
                dropdown.style.display = 'block';
            } catch (err) {
                console.error('Error fetching city autocomplete:', err);
            }
        }, 300);
    });

    document.addEventListener('click', (e) => {
        if (!input.contains(e.target) && !dropdown.contains(e.target)) {
            dropdown.style.display = 'none';
        }
    });
}

function selectCity(name, country, lat, lon) {
    window.location.href = `/Weather?city=${encodeURIComponent(name)}&lat=${lat}&lon=${lon}`;
}

// Unit switcher (°C <-> °F)
function initUnitToggle() {
    const toggleBtn = document.getElementById('unitToggleBtn');
    if (!toggleBtn) return;

    let isFahrenheit = localStorage.getItem('weather_unit') === 'F';
    updateUnitDisplay(isFahrenheit);

    toggleBtn.addEventListener('click', () => {
        isFahrenheit = !isFahrenheit;
        localStorage.setItem('weather_unit', isFahrenheit ? 'F' : 'C');
        updateUnitDisplay(isFahrenheit);
    });
}

function updateUnitDisplay(isFahrenheit) {
    const cElements = document.querySelectorAll('.unit-celsius');
    const fElements = document.querySelectorAll('.unit-fahrenheit');
    const toggleBtn = document.getElementById('unitToggleBtn');

    if (isFahrenheit) {
        cElements.forEach(el => el.classList.add('d-none'));
        fElements.forEach(el => el.classList.remove('d-none'));
        if (toggleBtn) toggleBtn.innerHTML = 'Switch to <strong>°C</strong>';
    } else {
        cElements.forEach(el => el.classList.remove('d-none'));
        fElements.forEach(el => el.classList.add('d-none'));
        if (toggleBtn) toggleBtn.innerHTML = 'Switch to <strong>°F</strong>';
    }
}

// Geolocation
function initGeolocation() {
    const geoBtn = document.getElementById('geoLocateBtn');
    if (!geoBtn) return;

    geoBtn.addEventListener('click', () => {
        if (!navigator.geolocation) {
            alert('Geolocation is not supported by your browser.');
            return;
        }

        geoBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Locating...';
        geoBtn.disabled = true;

        navigator.geolocation.getCurrentPosition(
            (pos) => {
                const lat = pos.coords.latitude;
                const lon = pos.coords.longitude;
                window.location.href = `/Weather?lat=${lat}&lon=${lon}&city=My%20Location`;
            },
            (err) => {
                alert('Could not determine your location. Please ensure location permissions are enabled.');
                geoBtn.innerHTML = '📍 My Location';
                geoBtn.disabled = false;
            },
            { timeout: 10000 }
        );
    });
}

// Clipboard copy helper
function copyToClipboard(text, elementId) {
    navigator.clipboard.writeText(text).then(() => {
        const el = document.getElementById(elementId);
        if (el) {
            const original = el.innerText;
            el.innerText = 'Copied!';
            setTimeout(() => { el.innerText = original; }, 1800);
        }
    }).catch(err => {
        console.error('Failed to copy: ', err);
    });
}

// Chart.js initialization for hourly trend
function renderHourlyChart(canvasId, labels, tempsC, rainProbs) {
    const ctx = document.getElementById(canvasId);
    if (!ctx || typeof Chart === 'undefined') return;

    new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: 'Temperature (°C)',
                    data: tempsC,
                    borderColor: '#38bdf8',
                    backgroundColor: 'rgba(56, 189, 248, 0.15)',
                    borderWidth: 3,
                    tension: 0.35,
                    fill: true,
                    yAxisID: 'y'
                },
                {
                    type: 'bar',
                    label: 'Precipitation Prob (%)',
                    data: rainProbs,
                    backgroundColor: 'rgba(99, 102, 241, 0.4)',
                    borderColor: 'rgba(99, 102, 241, 0.8)',
                    borderWidth: 1,
                    borderRadius: 4,
                    yAxisID: 'y1'
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: {
                mode: 'index',
                intersect: false
            },
            plugins: {
                legend: {
                    labels: { color: '#94a3b8', font: { family: 'Plus Jakarta Sans' } }
                },
                tooltip: {
                    backgroundColor: 'rgba(15, 23, 42, 0.9)',
                    titleColor: '#fff',
                    bodyColor: '#cbd5e1',
                    borderColor: 'rgba(255, 255, 255, 0.1)',
                    borderWidth: 1
                }
            },
            scales: {
                x: {
                    grid: { color: 'rgba(255, 255, 255, 0.05)' },
                    ticks: { color: '#94a3b8' }
                },
                y: {
                    type: 'linear',
                    display: true,
                    position: 'left',
                    grid: { color: 'rgba(255, 255, 255, 0.05)' },
                    ticks: { color: '#38bdf8', callback: v => v + '°' }
                },
                y1: {
                    type: 'linear',
                    display: true,
                    position: 'right',
                    grid: { drawOnChartArea: false },
                    ticks: { color: '#818cf8', callback: v => v + '%' },
                    min: 0,
                    max: 100
                }
            }
        }
    });
}
