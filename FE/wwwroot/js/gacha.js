document.addEventListener("DOMContentLoaded", function () {
    const API_BASE = '/gacha-proxy';
    const BANNER_ID = "11111111-1111-1111-1111-111111111111"; // Hardcode ID test

    const chestOverlay = document.getElementById('chest-animation-overlay');
    const resultModal = document.getElementById('gacha-result-modal');
    const resultGrid = document.getElementById('result-grid');
    const btnSingle = document.getElementById('wish-single');
    const btnMulti = document.getElementById('wish-multi');
    const pityText = document.getElementById('pity-counter');
    const vpDisplay = document.getElementById('vp-balance-display');

    const bannerDisplay = document.getElementById('banner-display');
    const bannerTitle = document.getElementById('banner-title');
    const bannerDesc = document.getElementById('banner-desc');
    const costSingle = document.getElementById('cost-single');
    const costMulti = document.getElementById('cost-multi');

    const historyModal = document.getElementById('history-modal');
    const historyTbody = document.getElementById('history-tbody');
    const btnHistory = document.getElementById('btn-history');

    // ── Toast Notification ───────────────────────────────────────────────────
    function showToast(message, type = 'info', duration = 4000) {
        let container = document.getElementById('toast-container');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toast-container';
            container.className = 'toast-container';
            document.body.appendChild(container);
        }
        const icons = { success: '✓', error: '✕', info: 'ℹ' };
        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.innerHTML = `<span>${icons[type] || icons.info}</span><span>${message}</span>`;
        container.appendChild(toast);
        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease forwards';
            setTimeout(() => toast.remove(), 300);
        }, duration);
    }

    // ── Load Banner Information ──────────────────────────────────────────────
    async function loadBannerInfo() {
        try {
            const res = await fetch(`${API_BASE}?path=banners/${BANNER_ID}`, {
                credentials: 'same-origin'
            });
            if (!res.ok) return;
            const json = await res.json();
            const data = json.data || json;

            if (data) {
                if (bannerTitle) bannerTitle.innerText = data.name || 'Event Banner';
                if (bannerDesc) bannerDesc.innerText = data.description || '';
                if (costSingle) costSingle.innerText = new Intl.NumberFormat('en-US').format(data.costPerSinglePull);
                if (costMulti) costMulti.innerText = new Intl.NumberFormat('en-US').format(data.costPerMultiPull);

                if (data.bannerImagePath && bannerDisplay) {
                    bannerDisplay.style.backgroundImage = `url('${data.bannerImagePath}')`;
                }
            }
        } catch (err) {
            console.error("Error loading banner info:", err);
        }
    }

    // ── Load User Status (Gems & Pity) ───────────────────────────────────────
    async function loadGachaStatus() {
        try {
            const res = await fetch(`${API_BASE}?path=status/${BANNER_ID}`, {
                credentials: 'same-origin'
            });
            if (!res.ok) return;
            const json = await res.json();
            const data = json.data || json;

            if (json.success || data) {
                if (pityText) pityText.innerText = data.pullsUntilGuaranteed5Star;
                if (vpDisplay) vpDisplay.innerText = new Intl.NumberFormat('en-US').format(data.currentGems);
            }
        } catch (err) {
            console.error("Error fetching status:", err);
        }
    }

    // ── Handle Wish Execution ────────────────────────────────────────────────
    async function performWish(isMulti) {
        const endpointPath = isMulti ? 'wish/multi' : 'wish/single';

        if (btnSingle) btnSingle.disabled = true;
        if (btnMulti) btnMulti.disabled = true;
        if (chestOverlay) chestOverlay.style.display = 'flex';

        try {
            const apiPromise = fetch(`${API_BASE}?path=${endpointPath}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify({ bannerId: BANNER_ID })
            }).then(res => {
                if (!res.ok) throw new Error("HTTP Error: " + res.status);
                return res.json();
            });

            // Minimum animation delay 2.5s
            const animationPromise = new Promise(resolve => setTimeout(resolve, 2500));
            const [result] = await Promise.all([apiPromise, animationPromise]);

            if (chestOverlay) chestOverlay.style.display = 'none';

            if (result.success || result.data) {
                const data = result.data || result;
                renderResults(data.results);
                if (resultModal) resultModal.style.display = 'flex';
                loadGachaStatus();
            } else {
                showToast(result.message || result.error || "Wish failed. Insufficient funds.", 'error');
            }
        } catch (error) {
            console.error("Perform Wish Error:", error);
            if (chestOverlay) chestOverlay.style.display = 'none';
            showToast("Server connection error! Please try again.", 'error');
        } finally {
            if (btnSingle) btnSingle.disabled = false;
            if (btnMulti) btnMulti.disabled = false;
        }
    }

    // ── Render Wish Results ──────────────────────────────────────────────────
    function renderResults(items) {
        if (!resultGrid) return;
        resultGrid.innerHTML = '';

        if (!items || !Array.isArray(items)) return;

        items.forEach(item => {
            let rarityClass = `rarity-${item.starRating}`;
            let imgSrc = item.imagePath || '';
            let itemName = item.itemName || 'Unknown Item';

            const cardHTML = `
                <div class="gacha-item-card ${rarityClass}">
                    <img src="${imgSrc}" alt="${itemName}" onerror="this.src='https://dummyimage.com/60x60/ccc/000&text=?'">
                    <span>${itemName}</span>
                    <small>${item.starRating}★</small>
                </div>
            `;
            resultGrid.innerHTML += cardHTML;
        });
    }

    // ── Handle History Log ───────────────────────────────────────────────────
    async function loadHistoryData() {
        if (historyModal) historyModal.style.display = 'flex';
        if (historyTbody) {
            historyTbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding: 30px; color: #ccc;">Fetching data from server...</td></tr>';
        }

        try {
            const res = await fetch(`${API_BASE}?path=history`, {
                credentials: 'same-origin'
            });

            if (!res.ok) throw new Error("HTTP Error: " + res.status);

            const json = await res.json();
            const items = json.data || json;

            if (Array.isArray(items) && items.length > 0) {
                historyTbody.innerHTML = '';

                items.forEach(item => {
                    let colorCode = '#ccc'; // Default 3-star
                    if (item.starRating === 4) colorCode = '#9c27b0'; // Epic (Purple)
                    if (item.starRating >= 5) colorCode = '#ff9800'; // Legendary (Orange)

                    const pullDate = item.pullDate || item.createdAt || new Date().toISOString();
                    const formattedDate = new Date(pullDate).toLocaleString('en-US', {
                        month: 'short', day: '2-digit', year: 'numeric',
                        hour: '2-digit', minute: '2-digit'
                    });

                    const row = `
    <tr style="border-bottom: 1px solid #333; transition: background 0.2s;">
        <td style="padding: 12px 8px; color: ${colorCode}; font-weight: bold;">
            ${item.itemName || 'Unknown Item'}
        </td>
        <td style="padding: 12px 8px; color: #ddd;">${item.itemCategory || 'Other'}</td>
        <td style="padding: 12px 8px; color: ${colorCode};">${item.starRating}★</td>
        <td style="padding: 12px 8px; color: #888; font-size: 14px;">${formattedDate}</td>
    </tr>
`;
                    historyTbody.innerHTML += row;
                });
            } else {
                historyTbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding: 30px; color: #888;">No wish records found.</td></tr>';
            }

        } catch (error) {
            console.error("Fetch History Error:", error);
            if (historyTbody) {
                historyTbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding: 30px; color: #f44336;">Failed to load history data.</td></tr>';
            }
        }
    }

    // ── Global Event Listeners ───────────────────────────────────────────────
    if (btnSingle) btnSingle.addEventListener('click', () => performWish(false));
    if (btnMulti) btnMulti.addEventListener('click', () => performWish(true));
    if (btnHistory) btnHistory.addEventListener('click', loadHistoryData);

    window.closeModal = function () {
        if (resultModal) resultModal.style.display = 'none';
    };

    window.closeHistory = function () {
        if (historyModal) historyModal.style.display = 'none';
    };

    // Init
    loadBannerInfo();
    loadGachaStatus();
});