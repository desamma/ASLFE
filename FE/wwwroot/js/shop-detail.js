document.addEventListener('DOMContentLoaded', () => {
    loadRelatedItems();
});

function loadRelatedItems() {
    const placeholder = document.querySelector('.related-items-placeholder');
    if (placeholder) {
        placeholder.innerHTML = '<p>Similar items from this category will be displayed here</p>';
    }
}

function showMessage(message, type = 'success') {
    const container = document.querySelector('.detail-container');
    if (!container) {
        alert(message);
        return;
    }

    const old = document.getElementById('shop-action-alert');
    if (old) old.remove();

    const alert = document.createElement('div');
    alert.id = 'shop-action-alert';
    alert.className = `alert alert-${type === 'success' ? 'success' : 'danger'} alert-dismissible fade show`;
    alert.setAttribute('role', 'alert');
    alert.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;

    container.prepend(alert);
}

window.purchaseItemDetail = async (itemId, buttonEl) => {
    if (!itemId) {
        showMessage('Invalid item.', 'error');
        return;
    }

    const buyUrl = window.shopBuyUrl;
    if (!buyUrl) {
        showMessage('Buy endpoint is not configured.', 'error');
        return;
    }

   
    const tokenInput = document.querySelector('#af-form input[name="__RequestVerificationToken"]');
    const token = tokenInput ? tokenInput.value : '';

    try {
        if (buttonEl) {
            buttonEl.disabled = true;
            buttonEl.innerHTML = '<span>Processing...</span>';
        }

        const response = await fetch(buyUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                // FIX: Đúng tên header mà ASP.NET Core AntiForgery middleware đọc
                'RequestVerificationToken': token
            },
            credentials: 'same-origin',
            body: JSON.stringify({
                shopItemId: itemId,
                quantity: 1
            })
        });

        const json = await response.json().catch(() => ({}));

        if (!response.ok) {
            showMessage(json.message || 'Purchase failed.', 'error');
            return;
        }

        showMessage(json.message || 'Purchase successful!', 'success');

    } catch (error) {
        console.error('Purchase error:', error);
        showMessage('Cannot connect to server.', 'error');
    } finally {
        if (buttonEl) {
            buttonEl.disabled = false;
            // Khôi phục lại text gốc của button
            const isGem = buttonEl.classList.contains('btn-purchase-detail') &&
                !buttonEl.classList.contains('btn-purchase-detail-primary');
            buttonEl.innerHTML = `<span>${isGem ? 'Purchase with Gems' : 'Buy Now'}</span>`;
        }
    }
};
