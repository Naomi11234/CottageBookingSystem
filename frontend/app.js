const API_BASE_URL = 'https://cottagebookingsystem.onrender.com/api';

let allCottages = [];

document.addEventListener('DOMContentLoaded', async () => {
    updateNavAuth();

    if (document.getElementById('cottages-grid')) {
        await loadCottagesToPage();
    }
});

function getCurrentUser() {
    try {
        return JSON.parse(localStorage.getItem('currentUser') || 'null');
    } catch {
        return null;
    }
}

function setCurrentUser(user) {
    localStorage.setItem('currentUser', JSON.stringify(user));
}

function logout() {
    localStorage.removeItem('currentUser');
    localStorage.removeItem('authToken');
    alert('התנתקת מהמערכת בהצלחה.');
    window.location.href = 'index.html';
}

function updateNavAuth() {
    const user = getCurrentUser();

    const welcomeUser = document.getElementById('welcome-user');
    const myBookingsLink = document.getElementById('my-bookings-link');
    const adminLink = document.getElementById('admin-link');
    const authLink = document.getElementById('auth-link');
    const logoutBtn = document.getElementById('logout-btn');

    const isLoggedIn = Boolean(user && localStorage.getItem('authToken'));
    const isAdmin = isLoggedIn &&
        String(user.role || '').toLowerCase() === 'admin';

    if (welcomeUser) {
        welcomeUser.style.display = isLoggedIn ? 'inline-block' : 'none';

        if (isLoggedIn) {
            welcomeUser.textContent = isAdmin
                ? `שלום מנהל המערכת, ${user.fullName || user.email || ''}`
                : `שלום, ${user.fullName || user.email || 'אורח'}`;
        }
    }

    if (myBookingsLink) {
        myBookingsLink.style.display = isLoggedIn ? 'inline-block' : 'none';
    }

    if (adminLink) {
        adminLink.style.display = isAdmin ? 'inline-block' : 'none';
    }

    if (authLink) {
        authLink.style.display = isLoggedIn ? 'none' : 'inline-block';
    }

    if (logoutBtn) {
        logoutBtn.style.display = isLoggedIn ? 'inline-block' : 'none';
    }
}

async function apiRequest(url, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        ...(options.headers || {})
    };

    const token = localStorage.getItem('authToken');

    if (token) {
        headers.Authorization = `Bearer ${token}`;
    }

    let response;

    try {
        response = await fetch(`${API_BASE_URL}${url}`, {
            ...options,
            headers
        });
    } catch (error) {
        console.error('שגיאת חיבור לשרת:', error);

        throw new Error(
           'לא ניתן להתחבר לשרת המקוון. בדקי שהשרת ב-Render פועל ושהחיבור לאינטרנט תקין.'
        );
    }

    if (!response.ok) {
        let message = `שגיאת שרת: ${response.status}`;

        try {
            const errorData = await response.json();
            message = errorData.message || errorData.title || message;
        } catch {
            // תשובת השגיאה אינה JSON
        }

        throw new Error(message);
    }

    if (response.status === 204) {
        return null;
    }

    const contentType = response.headers.get('content-type') || '';

    if (contentType.includes('application/json')) {
        return await response.json();
    }

    return null;
}

async function fetchAllCottages() {
    const cottages = [];
    let pageNumber = 1;
    const pageSize = 100;

    while (true) {
        const result = await apiRequest(
            `/Cottages?pageNumber=${pageNumber}&pageSize=${pageSize}`
        );

        if (!result || !Array.isArray(result.items)) {
            throw new Error('השרת החזיר רשימת צימרים בפורמט לא תקין.');
        }

        cottages.push(...result.items);

        if (
            result.items.length === 0 ||
            cottages.length >= result.totalCount
        ) {
            break;
        }

        pageNumber++;
    }

    return cottages;
}

function escapeHtml(value) {
    return String(value ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#39;');
}

function getCottageImage(cottage) {
    const fallback =
        'https://images.unsplash.com/photo-1542718610-a1d656d1884c?auto=format&fit=crop&w=1000&q=80';

    const image = cottage.imageUrl || cottage.image;

    if (!image || typeof image !== 'string') {
        return fallback;
    }

    if (image.startsWith('/')) {
       return `https://cottagebookingsystem.onrender.com${image}`;
    }

    if (/^https?:\/\//i.test(image)) {
        return image;
    }

    return fallback;
}

function amenityLabel(name) {
    const labels = {
        'wi-fi': '📶 Wi-Fi',
        'wifi': '📶 Wi-Fi',
        'pool': '🏊 בריכה',
        'hot tub': '🛁 ג׳קוזי',
        'jacuzzi': '🛁 ג׳קוזי',
        'parking': '🅿️ חניה',
        'air conditioning': '❄️ מיזוג אוויר'
    };

    const text = String(name || '').trim();

    return labels[text.toLowerCase()] || `✓ ${text}`;
}

function renderCottages(cottages) {
    const grid = document.getElementById('cottages-grid');

    if (!grid) return;

    grid.innerHTML = '';

    if (!Array.isArray(cottages) || cottages.length === 0) {
        grid.innerHTML = `
            <p style="
                text-align:center;
                grid-column:1/-1;
                font-size:1.2rem;
                color:#666;
                padding:30px;
            ">
                לא נמצאו צימרים התואמים לחיפוש.
            </p>
        `;
        return;
    }

    const user = getCurrentUser();

    const isAdmin =
        Boolean(localStorage.getItem('authToken')) &&
        String(user?.role || '').toLowerCase() === 'admin';

    if (isAdmin) {
        const addCard = document.createElement('div');
        addCard.className = 'cottage-card';

        addCard.style.cssText = `
            border:2px dashed #1e392a;
            display:flex;
            align-items:center;
            justify-content:center;
            cursor:pointer;
            background:#f0fdf4;
            min-height:300px;
        `;

        addCard.innerHTML = `
            <div style="text-align:center;padding:40px;">
                <span style="font-size:3rem;">➕</span>
                <h3 style="color:#1e392a;margin-top:10px;">
                    הוספת צימר חדש
                </h3>
            </div>
        `;

        addCard.addEventListener('click', () => {
            window.location.href = 'admin.html';
        });

        grid.appendChild(addCard);
    }

    cottages.forEach(cottage => {
        const card = document.createElement('div');
        card.className = 'cottage-card';

        const id = Number(cottage.id);
        const detailsUrl = `cottage-details.html?id=${id}`;

        const amenities = Array.isArray(cottage.amenities)
            ? cottage.amenities
            : [];

        const amenitiesHtml = amenities.length
            ? amenities.map(name => `
                <span style="
                    display:inline-block;
                    background:#f0fdf4;
                    color:#166534;
                    padding:6px 10px;
                    border-radius:20px;
                    font-size:0.85rem;
                    margin:3px;
                ">
                    ${escapeHtml(amenityLabel(name))}
                </span>
            `).join('')
            : '<span style="color:#64748b;">לא צוינו מתקנים</span>';

        card.innerHTML = `
            <div class="cottage-img-wrapper">
                <a href="${detailsUrl}">
                    <img
                        src="${escapeHtml(getCottageImage(cottage))}"
                        alt="${escapeHtml(cottage.name)}"
                        loading="lazy"
                    >
                </a>

                <span class="badge-location">
                    📍 ${escapeHtml(cottage.location)}
                </span>
            </div>

            <div class="cottage-info">
                <h3>
                    <a href="${detailsUrl}" style="
                        color:inherit;
                        text-decoration:none;
                    ">
                        ${escapeHtml(cottage.name)}
                    </a>
                </h3>

                <p>
                    👥 <strong>אורחים:</strong>
                    עד ${Number(cottage.maxGuests)} נופשים
                </p>

                <div style="margin-bottom:18px;">
                    <strong style="
                        display:block;
                        margin-bottom:8px;
                        color:#1e392a;
                    ">
                        ✨ מתקנים בצימר
                    </strong>

                    ${amenitiesHtml}
                </div>

                <div class="card-footer">
                    <div class="price-tag">
                        ₪${Number(cottage.pricePerNight).toLocaleString('he-IL')}
                        <span style="
                            font-size:0.85rem;
                            font-weight:normal;
                            color:#777;
                        ">
                            / לילה
                        </span>
                    </div>

                    <div style="display:flex;gap:8px;flex-wrap:wrap;">
                        <a
                            class="book-btn"
                            href="${detailsUrl}"
                            style="text-decoration:none;"
                        >
                            פרטים והזמנה
                        </a>

                        ${isAdmin ? `
                            <a
                                href="admin.html?edit=${id}"
                                style="
                                    background:#0284c7;
                                    color:white;
                                    padding:10px 14px;
                                    border-radius:10px;
                                    text-decoration:none;
                                "
                            >
                                ✏️ ערוך
                            </a>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;

        grid.appendChild(card);
    });
}

async function loadCottagesToPage(cottagesArray = null) {
    const grid = document.getElementById('cottages-grid');

    if (!grid) return;

    if (Array.isArray(cottagesArray)) {
        renderCottages(cottagesArray);
        return;
    }

    grid.innerHTML = `
        <p style="text-align:center;grid-column:1/-1;">
            טוען צימרים מהשרת...
        </p>
    `;

    try {
        allCottages = await fetchAllCottages();
        renderCottages(allCottages);
    } catch (error) {
        console.error('שגיאה בטעינת צימרים:', error);

        grid.innerHTML = `
            <div style="
                grid-column:1/-1;
                text-align:center;
                padding:35px;
                background:#fff1f2;
                border-radius:14px;
                color:#991b1b;
            ">
                <h3>לא ניתן לטעון את הצימרים</h3>
                <p>${escapeHtml(error.message)}</p>
                <button
                    class="book-btn"
                    type="button"
                    onclick="loadCottagesToPage()"
                >
                    נסי שוב
                </button>
            </div>
        `;
    }
}

function filterCottages() {
    const searchInput = document.getElementById('search-input');
    const regionFilter = document.getElementById('region-filter');
    const priceFilter = document.getElementById('price-filter');

    const searchText = (searchInput?.value || '')
        .trim()
        .toLowerCase();

    const region = regionFilter?.value || '';
    const price = priceFilter?.value || '';

    const filtered = allCottages.filter(cottage => {
        const name = String(cottage.name || '').toLowerCase();
        const location = String(cottage.location || '').toLowerCase();

        const matchesSearch =
            !searchText ||
            name.includes(searchText) ||
            location.includes(searchText);

        const regionNames = {
            'הצפון': ['הצפון', 'צפון', 'north'],
            'המרכז': ['המרכז', 'מרכז', 'center'],
            'הדרום': ['הדרום', 'דרום', 'south']
        };

        const matchesRegion =
            !region ||
            (regionNames[region] || [region.toLowerCase()])
                .some(value => location.includes(value));

        const nightlyPrice = Number(cottage.pricePerNight);

        const matchesPrice =
            !price ||
            (price === '2000'
                ? nightlyPrice > 900
                : nightlyPrice <= Number(price));

        return matchesSearch && matchesRegion && matchesPrice;
    });

    renderCottages(filtered);
}

function editCottage(id) {
    window.location.href = `admin.html?edit=${Number(id)}`;
}

async function deleteCottageFromHome(id) {
    if (!confirm('האם למחוק את הצימר ממסד הנתונים?')) {
        return;
    }

    try {
        await apiRequest(`/Cottages/${Number(id)}`, {
            method: 'DELETE'
        });

        alert('הצימר נמחק בהצלחה.');
        await loadCottagesToPage();
    } catch (error) {
        alert('המחיקה נכשלה: ' + error.message);
    }
}
