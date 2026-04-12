// payment.js — shared across all payment pages

const API_BASE = window.API_BASE || '/Payment/Proxy';

// ── Helpers ──────────────────────────────────────────────────────────────────

function formatVnd(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + ' ₫';
}

function formatVp(vp) {
    return new Intl.NumberFormat('en-US').format(vp) + ' VP';
}

function formatDate(dateStr) {
    if (!dateStr) return '—';
    const d = new Date(dateStr);
    return d.toLocaleString('en-US', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

// ── Toast ─────────────────────────────────────────────────────────────────────

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

// ── Tab switcher ─────────────────────────────────────────────────────────────

function initTabs() {
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.addEventListener('click', () => {
            const target = btn.dataset.tab;
            document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
            document.querySelectorAll('.tab-panel').forEach(p => p.classList.remove('active'));
            btn.classList.add('active');
            document.getElementById(`tab-${target}`)?.classList.add('active');
            if (target === 'history') loadHistory();
        });
    });
}

// ── Package selection ─────────────────────────────────────────────────────────

let selectedPackage = null;

async function loadPackages() {
    const grid = document.getElementById('packages-grid');
    if (!grid) return;

    try {
        const res = await fetch(`${API_BASE}?path=packages`);
        const json = await res.json();

        const packages = json.data || json;
        if (!Array.isArray(packages)) throw new Error('Invalid response format');

        grid.innerHTML = '';
        packages.forEach(pkg => {
            const card = document.createElement('div');
            card.className = 'pkg-card';
            card.dataset.vpKey = pkg.id || pkg.vpKey;

            const totalVp = pkg.virtualPoint || pkg.totalVp || 0;
            const bonusVp = pkg.bonus || pkg.bonusVp || 0;
            const baseVp = totalVp - bonusVp;
            const displayPrice = pkg.price || pkg.priceVnd || 0;

            card.innerHTML = `
                ${bonusVp > 0 ? `<div class="pkg-bonus-badge">+${formatVp(bonusVp)}</div>` : ''}
                <div class="pkg-icon-wrap">
                    <div class="pkg-icon-bg">
                        <img src="/images/virtual-currency.png" alt="VP"
                             style="width:28px;height:28px;object-fit:contain;" />
                    </div>
                </div>
                <div class="pkg-vp">${formatVp(totalVp)}</div>
                <div class="pkg-bonus-text">${bonusVp > 0 ? `(${formatVp(baseVp)} + ${formatVp(bonusVp)} bonus)` : ''}</div>
                <div class="pkg-price">${formatVnd(displayPrice)}</div>
            `;
            card.addEventListener('click', () => selectPackage(pkg, card));
            grid.appendChild(card);
        });
    } catch (error) {
        console.error('Error loading packages:', error);
        grid.innerHTML = '<p style="color:var(--text-muted);padding:20px;text-align:center">Failed to load packages. Please try again.</p>';
    }
}

function selectPackage(pkg, cardEl) {
    document.querySelectorAll('.pkg-card').forEach(c => c.classList.remove('selected'));
    cardEl.classList.add('selected');
    selectedPackage = pkg;
    updateCheckout(pkg);
}

function updateCheckout(pkg) {
    const panel = document.getElementById('checkout-detail');
    const payBtn = document.getElementById('btn-pay');
    if (!panel) return;

    if (!pkg) {
        panel.innerHTML = `
            <div class="empty-selection">
                <svg viewBox="0 0 24 24" fill="currentColor"><path d="M11 9h2V7h-2m1 13c-4.41 0-8-3.59-8-8s3.59-8 8-8 8 3.59 8 8-3.59 8-8 8m0-18A10 10 0 0 0 2 12a10 10 0 0 0 10 10 10 10 0 0 0 10-10A10 10 0 0 0 12 2m-1 15h2v-6h-2v6z"/></svg>
                <p>Select a VP package above</p>
            </div>`;
        payBtn && (payBtn.disabled = true);
        return;
    }

    const totalVp = pkg.virtualPoint || pkg.totalVp || 0;
    const bonusVp = pkg.bonus || pkg.bonusVp || 0;
    const baseVp = totalVp - bonusVp;
    const displayPrice = pkg.price || pkg.priceVnd || 0;

    panel.innerHTML = `
        <div class="checkout-row">
            <span class="key">Package</span>
            <span class="val">${pkg.description || pkg.displayName || formatVp(totalVp)}</span>
        </div>
        <div class="checkout-row">
            <span class="key">VP Received</span>
            <span class="val" style="color:var(--accent2)">${formatVp(totalVp)}</span>
        </div>
        ${bonusVp > 0 ? `
        <div class="checkout-row">
            <span class="key">Bonus VP</span>
            <span class="val" style="color:var(--accent2);font-size:14px">+${formatVp(bonusVp)}</span>
        </div>` : ''}
        <div class="checkout-row total">
            <span class="key">Total</span>
            <span class="val">${formatVnd(displayPrice)}</span>
        </div>
    `;
    payBtn && (payBtn.disabled = false);
}

// ── Pay ──────────────────────────────────────────────────────────────────────

async function submitPayment() {
    if (!selectedPackage) {
        showToast('Please select a VP package', 'error');
        return;
    }

    const btn = document.getElementById('btn-pay');
    btn.classList.add('loading');
    btn.disabled = true;

    try {
        const packageId = selectedPackage.id || selectedPackage.vpKey;
        const payload = { vpPackage: packageId };

        const res = await fetch(`${API_BASE}?path=create`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(payload)
        });

        const json = await res.json();
        const paymentUrl = json.paymentLink || json.checkoutUrl || json.data?.paymentLink;

        if (json.success && paymentUrl) {
            showToast('Redirecting to payment page...', 'success');
            setTimeout(() => { window.location.href = paymentUrl; }, 800);
        } else {
            showToast(json.message || 'Something went wrong. Please try again.', 'error');
            btn.classList.remove('loading');
            btn.disabled = false;
        }
    } catch (error) {
        console.error('Payment error:', error);
        showToast('Cannot connect to server.', 'error');
        btn.classList.remove('loading');
        btn.disabled = false;
    }
}

// ── History ───────────────────────────────────────────────────────────────────

async function loadHistory() {
    const tbody = document.getElementById('history-tbody');
    const empty = document.getElementById('history-empty');
    if (!tbody) return;

    tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-muted);padding:40px">Loading...</td></tr>';

    try {
        const res = await fetch(`${API_BASE}?path=history`);
        const json = await res.json();

        const transactions = json.data || json;
        if (!Array.isArray(transactions)) throw new Error('Invalid response format');

        if (transactions.length === 0) {
            tbody.innerHTML = '';
            empty?.classList.remove('hidden');
            return;
        }
        empty?.classList.add('hidden');

        const badgeMap = {
            'Paid': '<span class="badge badge-paid"><span class="dot"></span>Success</span>',
            'Pending': '<span class="badge badge-pending"><span class="dot"></span>Pending</span>',
            'Failed': '<span class="badge badge-failed"><span class="dot"></span>Failed</span>',
            'Cancelled': '<span class="badge badge-cancelled"><span class="dot"></span>Cancelled</span>',
        };

        tbody.innerHTML = transactions.map(tx => {
            const displayName = tx.name || tx.displayName || formatVp(tx.amount);
            const amount = tx.amount || tx.priceVnd || 0;
            const vp = tx.currencyAwarded || tx.virtualPoint || 0;
            const status = tx.status || 'Unknown';
            const createdAt = tx.createdAt || tx.createdDate;
            const paidAt = tx.paidAt || tx.paidDate;

            return `
                <tr>
                    <td class="tx-name">${displayName}</td>
                    <td class="tx-amount">${formatVnd(amount)}</td>
                    <td class="tx-vp">${formatVp(vp)}</td>
                    <td>${badgeMap[status] || status}</td>
                    <td>${formatDate(createdAt)}</td>
                    <td>${formatDate(paidAt)}</td>
                </tr>
            `;
        }).join('');
    } catch (error) {
        console.error('History error:', error);
        tbody.innerHTML = '<tr><td colspan="6" style="text-align:center;color:var(--text-muted);padding:40px">Failed to load history.</td></tr>';
    }
}

// ── Init ──────────────────────────────────────────────────────────────────────

document.addEventListener('DOMContentLoaded', () => {
    initTabs();
    loadPackages();

    document.getElementById('btn-pay')?.addEventListener('click', submitPayment);

    const urlParams = new URLSearchParams(window.location.search);
    const orderCode = urlParams.get('orderCode');
    if (orderCode && document.getElementById('result-section')) {
        checkPaymentResult(orderCode);
    }
});

// ── Payment result check ─────────────────────────────────────────────────────

async function checkPaymentResult(orderCode) {
    const section = document.getElementById('result-section');
    try {
        const res = await fetch(`${API_BASE}?path=status/${orderCode}`);
        const json = await res.json();

        const transaction = json.data || json;
        if (!transaction) throw new Error('No transaction data');

        renderResult(transaction);
    } catch (error) {
        console.error('Result check error:', error);
        section.innerHTML = `<div class="result-wrap">
            <div class="result-icon fail">✕</div>
            <div class="result-title fail">Check Failed</div>
            <div class="result-desc">Unable to verify payment. Please check your transaction history.</div>
            <a href="/Payment" style="color:var(--accent)">← Back to Top Up</a>
        </div>`;
    }
}

function renderResult(tx) {
    const isPaid = tx.status === 'Paid';
    const displayAmount = tx.currencyAwarded || tx.virtualPoint || 0;
    const orderCode = tx.orderCode || tx.id || 'N/A';
    const displayName = tx.name || tx.displayName || 'Transaction';
    const transactionAmount = tx.amount || tx.priceVnd || 0;
    const paidDate = tx.paidAt || tx.paidDate || tx.createdAt;

    document.getElementById('result-section').innerHTML = `
        <div class="result-wrap">
            <div class="result-icon ${isPaid ? 'success' : 'fail'}">${isPaid ? '✓' : '✕'}</div>
            <div class="result-title ${isPaid ? 'success' : 'fail'}">${isPaid ? 'Payment Successful' : 'Payment Failed'}</div>
            <div class="result-desc">${isPaid
            ? `<strong style="color:var(--accent2)">${formatVp(displayAmount)}</strong> has been added to your account.`
            : 'Transaction was not completed. You have not been charged.'
        }</div>
            <div class="result-card">
                <div class="checkout-row"><span class="key">Order ID</span><span class="val" style="font-size:14px">#${orderCode}</span></div>
                <div class="checkout-row"><span class="key">Package</span><span class="val">${displayName}</span></div>
                <div class="checkout-row"><span class="key">Amount</span><span class="val" style="color:var(--accent)">${formatVnd(transactionAmount)}</span></div>
                ${isPaid ? `<div class="checkout-row"><span class="key">VP Received</span><span class="val" style="color:var(--accent2)">${formatVp(displayAmount)}</span></div>` : ''}
                <div class="checkout-row"><span class="key">Date</span><span class="val" style="font-size:13px">${formatDate(paidDate)}</span></div>
            </div>
            <a href="/Payment" style="color:var(--text-dim);font-size:14px;text-decoration:none">← Back to Top Up</a>
        </div>
    `;
}
