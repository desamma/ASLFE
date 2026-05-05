document.addEventListener("DOMContentLoaded", function () {
    const API_BASE = '/gacha-proxy';

    // State
    let activeBannerId = null;
    let allBanners = [];

    // DOM refs
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

    const bannerTabsContainer = document.getElementById('banner-tabs');

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

    function getActiveBannerMultiCost() {
        const activeBanner = allBanners.find(b => b.id === activeBannerId);
        if (activeBanner && Number.isFinite(Number(activeBanner.costPerMultiPull))) {
            return Number(activeBanner.costPerMultiPull);
        }

        const uiCost = Number((costMulti?.innerText || '0').replace(/,/g, ''));
        return Number.isFinite(uiCost) ? uiCost : 0;
    }

    async function fetchStatusByBannerId() {
        if (!activeBannerId) return null;

        const res = await fetch(`${API_BASE}?path=status/${activeBannerId}`, {
            credentials: 'same-origin'
        });

        if (!res.ok) {
            throw new Error("HTTP Error: " + res.status);
        }

        const json = await res.json();
        return json.data || json;
    }

    function updateMultiButtonByStatus(statusData) {
        if (!btnMulti || !statusData) return;

        const currentGems = Number(statusData.currentGems ?? 0);
        const multiCost = getActiveBannerMultiCost();
        const canMultiWish = currentGems >= multiCost;

        btnMulti.disabled = !canMultiWish;
    }

    // ── Build Banner Tabs ────────────────────────────────────────────────────
    function buildBannerTabs(banners) {
        if (!bannerTabsContainer) return;
        bannerTabsContainer.innerHTML = '';

        banners.forEach((banner, index) => {
            const tab = document.createElement('button');
            tab.className = 'banner-tab' + (index === 0 ? ' active' : '');
            tab.dataset.bannerId = banner.id;
            tab.innerHTML = `
                <span class="tab-dot"></span>
                <span class="tab-name">${banner.name}</span>
            `;
            tab.addEventListener('click', () => switchBanner(banner.id));
            bannerTabsContainer.appendChild(tab);
        });
    }

    // ── Switch Active Banner ─────────────────────────────────────────────────
    function switchBanner(bannerId) {
        if (activeBannerId === bannerId) return;
        activeBannerId = bannerId;

        document.querySelectorAll('.banner-tab').forEach(tab => {
            tab.classList.toggle('active', tab.dataset.bannerId === bannerId);
        });

        const banner = allBanners.find(b => b.id === bannerId);
        if (banner) applyBannerToUI(banner);

        loadGachaStatus();

        const card = document.querySelector('.gacha-horizontal-card');
        if (card) {
            card.style.opacity = '0';
            card.style.transform = 'translateY(10px)';
            setTimeout(() => {
                card.style.transition = 'opacity 0.35s ease, transform 0.35s ease';
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 50);
        }
    }

    // ── Apply Banner Data to UI ──────────────────────────────────────────────
    function applyBannerToUI(data) {
        if (bannerTitle) bannerTitle.innerText = data.name || 'Event Banner';
        if (bannerDesc) bannerDesc.innerText = data.description || '';
        if (costSingle) costSingle.innerText = new Intl.NumberFormat('en-US').format(data.costPerSinglePull);
        if (costMulti) costMulti.innerText = new Intl.NumberFormat('en-US').format(data.costPerMultiPull);

        if (bannerDisplay) {
            if (data.bannerImagePath) {
                bannerDisplay.style.backgroundImage = `url('${data.bannerImagePath}')`;
            } else {
                bannerDisplay.style.backgroundImage = '';
            }
        }

        const videoEl = document.getElementById('banner-video');
        const videoSrc = document.getElementById('banner-video-source');
        if (videoEl && videoSrc) {
            if (data.bannerVideoPath) {
                videoSrc.src = data.bannerVideoPath;
                videoEl.load();
                videoEl.play().catch(() => { });
                videoEl.style.display = '';
            } else {
                videoEl.style.display = 'none';
            }
        }
    }

    // ── Load All Active Banners ──────────────────────────────────────────────
    async function loadAllBanners() {
        try {
            const res = await fetch(`${API_BASE}?path=banners`, {
                credentials: 'same-origin'
            });
            if (!res.ok) return;
            const json = await res.json();
            const banners = json.data || json;

            if (!Array.isArray(banners) || banners.length === 0) {
                if (bannerTitle) bannerTitle.innerText = 'No Active Banners';
                return;
            }

            allBanners = banners;
            buildBannerTabs(banners);

            activeBannerId = banners[0].id;
            applyBannerToUI(banners[0]);
            loadGachaStatus();

        } catch (err) {
            console.error("Error loading banners:", err);
            showToast("Failed to load banners.", 'error');
        }
    }

    // ── Load User Status (Gems & Pity) ───────────────────────────────────────
    async function loadGachaStatus() {
        if (!activeBannerId) return;
        try {
            const data = await fetchStatusByBannerId();
            if (!data) return;

            if (pityText) pityText.innerText = data.pullsUntilGuaranteed5Star;
            if (vpDisplay) vpDisplay.innerText = new Intl.NumberFormat('en-US').format(data.currentGems ?? 0);

            updateMultiButtonByStatus(data);
        } catch (err) {
            console.error("Error fetching status:", err);
        }
    }

    // ── Handle Wish Execution ────────────────────────────────────────────────
    async function performWish(isMulti) {
        if (!activeBannerId) {
            showToast("No banner selected.", 'error');
            return;
        }

        // BẮT BUỘC: Trước khi wish phải kiểm tra lại status theo banner
        let latestStatus = null;
        try {
            latestStatus = await fetchStatusByBannerId();

            if (vpDisplay && latestStatus) {
                vpDisplay.innerText = new Intl.NumberFormat('en-US').format(latestStatus.currentGems ?? 0);
            }
            if (pityText && latestStatus) {
                pityText.innerText = latestStatus.pullsUntilGuaranteed5Star ?? '--';
            }

            if (isMulti && latestStatus) {
                const currentGems = Number(latestStatus.currentGems ?? 0);
                const multiCost = getActiveBannerMultiCost();

                if (currentGems < multiCost) {
                    if (btnMulti) btnMulti.disabled = true;
                    showToast("Không đủ Gems", 'error');
                    return;
                }
            }
        } catch (error) {
            console.error("Pre-check status error:", error);
            showToast("Không thể kiểm tra Gems. Vui lòng thử lại.", 'error');
            return;
        }

        const endpointPath = isMulti ? 'wish/multi' : 'wish/single';

        if (btnSingle) btnSingle.disabled = true;
        if (btnMulti) btnMulti.disabled = true;
        if (chestOverlay) chestOverlay.style.display = 'flex';

        try {
            const apiPromise = fetch(`${API_BASE}?path=${endpointPath}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                credentials: 'same-origin',
                body: JSON.stringify({ bannerId: activeBannerId })
            }).then(res => {
                if (!res.ok) throw new Error("HTTP Error: " + res.status);
                return res.json();
            });

            const animationPromise = new Promise(resolve => setTimeout(resolve, 2500));
            const [result] = await Promise.all([apiPromise, animationPromise]);

            if (chestOverlay) chestOverlay.style.display = 'none';

            if (result.success || result.data) {
                const data = result.data || result;
                renderResults(data.results);
                if (resultModal) resultModal.style.display = 'flex';
            } else {
                showToast(result.message || result.error || "Wish failed. Insufficient funds.", 'error');
            }
        } catch (error) {
            console.error("Perform Wish Error:", error);
            if (chestOverlay) chestOverlay.style.display = 'none';
            showToast("Server connection error! Please try again.", 'error');
        } finally {
            if (btnSingle) btnSingle.disabled = false;
            await loadGachaStatus();
        }
    }

    // ── Render Wish Results ──────────────────────────────────────────────────
    function renderResults(items) {
        if (!resultGrid) return;
        resultGrid.innerHTML = '';
        if (!items || !Array.isArray(items)) return;

        items.forEach(item => {
            const cardHTML = `
                <div class="gacha-item-card rarity-${item.starRating}">
                    <img src="${item.imagePath || ''}" alt="${item.itemName || ''}" 
                         onerror="this.src='https://dummyimage.com/60x60/ccc/000&text=?'">
                    <span>${item.itemName || 'Unknown Item'}</span>
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
            const res = await fetch(`${API_BASE}?path=history`, { credentials: 'same-origin' });
            if (!res.ok) throw new Error("HTTP Error: " + res.status);

            const json = await res.json();
            const items = json.data || json;

            if (Array.isArray(items) && items.length > 0) {
                historyTbody.innerHTML = '';
                items.forEach(item => {
                    let colorCode = '#ccc';
                    if (item.starRating === 4) colorCode = '#9c27b0';
                    if (item.starRating >= 5) colorCode = '#ff9800';

                    const pullDate = item.pulledAt || item.pullDate || item.createdAt || new Date().toISOString();
                    const formattedDate = new Date(pullDate).toLocaleString('en-US', {
                        month: 'short', day: '2-digit', year: 'numeric',
                        hour: '2-digit', minute: '2-digit'
                    });

                    historyTbody.innerHTML += `
                        <tr>
                            <td style="padding:12px 8px; color:${colorCode}; font-weight:bold;">${item.itemName || 'Unknown'}</td>
                            <td style="padding:12px 8px; color:#ddd;">${item.itemCategory || 'Other'}</td>
                            <td style="padding:12px 8px; color:${colorCode};">${item.starRating}★</td>
                            <td style="padding:12px 8px; color:#888; font-size:13px;">${formattedDate}</td>
                        </tr>`;
                });
            } else {
                historyTbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding:30px; color:#888;">No wish records found.</td></tr>';
            }
        } catch (error) {
            console.error("Fetch History Error:", error);
            if (historyTbody)
                historyTbody.innerHTML = '<tr><td colspan="4" style="text-align:center; padding:30px; color:#f44336;">Failed to load history.</td></tr>';
        }
    }

    // ── Event Listeners ──────────────────────────────────────────────────────
    if (btnSingle) btnSingle.addEventListener('click', () => performWish(false));
    if (btnMulti) btnMulti.addEventListener('click', () => performWish(true));
    if (btnHistory) btnHistory.addEventListener('click', loadHistoryData);

    window.closeModal = function () {
        if (resultModal) resultModal.style.display = 'none';
    };
    window.closeHistory = function () {
        if (historyModal) historyModal.style.display = 'none';
    };

    // ── Init ─────────────────────────────────────────────────────────────────
    loadAllBanners();
});