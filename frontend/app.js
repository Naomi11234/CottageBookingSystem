document.addEventListener('DOMContentLoaded', async () => {
    updateNavAuth();
    await loadCottagesToPage();
});

function updateNavAuth() {
    const user = getCurrentUser();
    const welcomeUser = document.getElementById('welcome-user');
    const myBookingsLink = document.getElementById('my-bookings-link');
    const adminLink = document.getElementById('admin-link');
    const authLink = document.getElementById('auth-link');
    const logoutBtn = document.getElementById('logout-btn');

    if (user) {
        if (welcomeUser) {
            welcomeUser.style.display = 'inline-block';
            const isUserAdmin = user.role && user.role.toLowerCase() === 'admin';
            const isNaomi = user.email === 'naomi.avni21@gmail.com';
            
            if (isUserAdmin || isNaomi) {
                welcomeUser.innerText = `שלום מנהל המערכת, ${user.fullName || 'נעמי אבני'}`;
            } else {
                welcomeUser.innerText = `שלום, ${user.fullName || 'אורח'}`;
            }
        }
        if (myBookingsLink) myBookingsLink.style.display = 'inline-block';
        if (authLink) authLink.style.display = 'none';
        if (logoutBtn) logoutBtn.style.display = 'inline-block';

        const isUserAdmin = user.role && user.role.toLowerCase() === 'admin';
        const isNaomi = user.email === 'naomi.avni21@gmail.com';

        if (isUserAdmin || isNaomi) {
            if (adminLink) adminLink.style.display = 'inline-block';
        }
    } else {
        if (welcomeUser) welcomeUser.style.display = 'none';
        if (myBookingsLink) myBookingsLink.style.display = 'none';
        if (adminLink) adminLink.style.display = 'none';
        if (authLink) authLink.style.display = 'inline-block';
        if (logoutBtn) logoutBtn.style.display = 'none';
    }
}

function getCurrentUser() {
    const userJson = localStorage.getItem('currentUser');
    return userJson ? JSON.parse(userJson) : null;
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

async function apiRequest(url, options = {}) {
    const defaultHeaders = {
        'Content-Type': 'application/json'
    };
    
    const token = localStorage.getItem('authToken');
    if (token) {
        defaultHeaders['Authorization'] = `Bearer ${token}`;
    }

    options.headers = { ...defaultHeaders, ...options.headers };

    const response = await fetch(`https://localhost:7076/api${url}`, options);
    if (!response.ok) {
        throw new Error(`שגיאה בשרת: ${response.statusText}`);
    }
    
    const contentType = response.headers.get("content-type");
    if (contentType && contentType.includes("application/json")) {
        return await response.json();
    }
    return null;
}

async function loadCottagesToPage(cottagesArray = null) {
    const grid = document.getElementById('cottages-grid');
    if (!grid) return;

    grid.innerHTML = '';
    
    let cottages = cottagesArray;
    if (!cottages) {
        try {
            cottages = await apiRequest('/cottages');
        } catch (e) {
            console.warn("לא ניתן לטעון מהשרת, טוען מנתונים מקומיים", e);
            cottages = JSON.parse(localStorage.getItem('localCottages') || '[]');
        }
    }

    if (!cottages || cottages.length === 0) {
        grid.innerHTML = '<p style="text-align:center; grid-column:1/-1; font-size:1.2rem; color:#666;">לא נמצאו צימרים במערכת.</p>';
        return;
    }

    const currentUser = getCurrentUser();
    const isUserAdmin = currentUser && currentUser.role && currentUser.role.toLowerCase() === 'admin';
    const isNaomi = currentUser && currentUser.email === 'naomi.avni21@gmail.com';
    const isAdmin = isUserAdmin || isNaomi;

    if (isAdmin) {
        const addCard = document.createElement('div');
        addCard.className = 'cottage-card';
        addCard.style.border = '2px dashed var(--primary-color)';
        addCard.style.display = 'flex';
        addCard.style.alignItems = 'center';
        addCard.style.justifyContent = 'center';
        addCard.style.cursor = 'pointer';
        addCard.style.background = '#f0fdf4';
        addCard.innerHTML = `
            <div style="text-align: center; padding: 40px;">
                <span style="font-size: 3rem;">➕</span>
                <h3 style="color: var(--primary-color); margin-top: 10px;">הוספת צימר חדש</h3>
            </div>
        `;
        addCard.onclick = () => {
            window.location.href = 'admin.html';
        };
        grid.appendChild(addCard);
    }

    cottages.forEach(cottage => {
        const card = document.createElement('div');
        card.className = 'cottage-card';

        card.innerHTML = `
            <div class="cottage-img-wrapper" onclick="${isAdmin ? `editCottage(${cottage.id})` : `location.href='cottage-details.html?id=${cottage.id}'`}" style="cursor:pointer;">
                <img src="${cottage.image || 'https://images.unsplash.com/photo-1542718610-a1d656d1884c?auto=format&fit=crop&w=1000&q=80'}" alt="${cottage.name}">
                <span class="badge-location">📍 ${cottage.location}</span>
            </div>
            <div class="cottage-info">
                <h3 onclick="${isAdmin ? `editCottage(${cottage.id})` : `location.href='cottage-details.html?id=${cottage.id}'`}" style="cursor:pointer;">${cottage.name}</h3>
                <p style="font-size:0.9rem; color:#555;">👥 <strong>אורחים:</strong> עד ${cottage.maxGuests} נופשים</p>
                <div class="card-footer">
                    <div class="price-tag">₪${cottage.pricePerNight} <span style="font-size:0.85rem; font-weight:normal; color:#777;">/ לילה</span></div>
                    <div style="display: flex; gap: 8px; align-items: center;">
                        ${isAdmin ? `
                            <button onclick="event.stopPropagation(); editCottage(${cottage.id})" style="background:#0284c7; color:white; border:none; padding:6px 10px; border-radius:6px; cursor:pointer;" title="ערוך צימר">✏️ ערוך</button>
                            <button onclick="event.stopPropagation(); deleteCottageFromHome(${cottage.id})" style="background:#ef4444; color:white; border:none; padding:6px 10px; border-radius:6px; cursor:pointer;" title="מחק צימר">🗑️ מחק</button>
                        ` : `
                            <button class="book-btn" onclick="location.href='cottage-details.html?id=${cottage.id}'">להזמנה</button>
                        `}
                    </div>
                </div>
            </div>
        `;
        grid.appendChild(card);
    });
}

async function deleteCottageFromHome(id) {
    if (confirm('האם את בטוחה שברצונך למחוק צימר זה?')) {
        try {
            await apiRequest(`/cottages/${id}`, { method: 'DELETE' });
        } catch (e) {
            console.warn('מחיקה מהשרת נכשלה, מסיר מקומית');
        }
        
        let localCottages = JSON.parse(localStorage.getItem('localCottages') || '[]');
        localCottages = localCottages.filter(c => c.id !== id);
        localStorage.setItem('localCottages', JSON.stringify(localCottages));

        alert('הצימר נמחק בהצלחה!');
        window.location.reload();
    }
}

function editCottage(id) {
    window.location.href = `admin.html?edit=${id}`;
}

async function filterCottages() {
    const searchText = document.getElementById('search-input').value.toLowerCase().trim();
    const regionText = document.getElementById('region-filter').value;
    const maxPrice = document.getElementById('price-filter').value;

    let cottages = [];
    try {
        cottages = await apiRequest('/cottages');
    } catch (e) {
        cottages = JSON.parse(localStorage.getItem('localCottages') || '[]');
    }

    const filtered = cottages.filter(c => {
        const matchesText = !searchText || 
            c.name.toLowerCase().includes(searchText) || 
            c.location.toLowerCase().includes(searchText);

        const matchesRegion = !regionText || c.region === regionText;
        const matchesPrice = !maxPrice || c.pricePerNight <= Number(maxPrice);

        return matchesText && matchesRegion && matchesPrice;
    });

    await loadCottagesToPage(filtered);
}