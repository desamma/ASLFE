document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('itemSearch');
    const sortSelect = document.getElementById('inventorySort');
    const filterButtons = Array.from(document.querySelectorAll('.filter-btn'));
    const inventoryGrid = document.getElementById('inventoryGrid');

    if (!inventoryGrid) {
        return;
    }

    const inventoryItems = Array.from(document.querySelectorAll('.inventory-item'));
    const rarityRanks = {
        common: 1,
        uncommon: 2,
        rare: 3,
        epic: 4,
        legendary: 5
    };

    const getActiveFilter = () => {
        const activeButton = filterButtons.find(button => button.classList.contains('active'));
        return activeButton?.dataset.filter ?? 'all';
    };

    const getComparableDate = (value) => {
        const date = value ? new Date(value) : null;
        return date && !Number.isNaN(date.getTime()) ? date.getTime() : 0;
    };

    const getRarityRank = (value) => {
        const normalized = (value ?? '').toLowerCase();
        return rarityRanks[normalized] ?? 0;
    };

    const compareItems = (left, right, sortValue) => {
        const leftName = (left.dataset.itemName ?? '').toLowerCase();
        const rightName = (right.dataset.itemName ?? '').toLowerCase();
        const leftQuantity = Number(left.dataset.itemQuantity ?? 0);
        const rightQuantity = Number(right.dataset.itemQuantity ?? 0);
        const leftRarity = getRarityRank(left.dataset.itemRarity);
        const rightRarity = getRarityRank(right.dataset.itemRarity);
        const leftCreatedAt = getComparableDate(left.dataset.itemCreatedAt);
        const rightCreatedAt = getComparableDate(right.dataset.itemCreatedAt);

        switch (sortValue) {
            case 'oldest':
                return leftCreatedAt - rightCreatedAt;
            case 'name-asc':
                return leftName.localeCompare(rightName);
            case 'name-desc':
                return rightName.localeCompare(leftName);
            case 'quantity-asc':
                return leftQuantity - rightQuantity;
            case 'quantity-desc':
                return rightQuantity - leftQuantity;
            case 'rarity-asc':
                return leftRarity - rightRarity;
            case 'rarity-desc':
                return rightRarity - leftRarity;
            case 'newest':
            default:
                return rightCreatedAt - leftCreatedAt;
        }
    };

    const filterAndSortItems = () => {
        const searchTerm = (searchInput?.value ?? '').trim().toLowerCase();
        const activeFilter = getActiveFilter();
        const sortValue = sortSelect?.value ?? 'newest';

        const filteredItems = inventoryItems.filter(item => {
            const itemName = item.dataset.itemName ?? '';
            const itemType = item.dataset.itemType ?? '';

            const matchesSearch = !searchTerm || itemName.includes(searchTerm);
            const matchesFilter = activeFilter === 'all' || itemType === activeFilter;

            return matchesSearch && matchesFilter;
        });

        inventoryItems
            .slice()
            .sort((left, right) => compareItems(left, right, sortValue))
            .forEach(item => {
                const isVisible = filteredItems.includes(item);
                item.style.display = isVisible ? '' : 'none';
                inventoryGrid.appendChild(item);
            });

        const existingMessage = inventoryGrid.querySelector('.no-items-message');
        if (filteredItems.length === 0) {
            if (!existingMessage) {
                const noItemsMsg = document.createElement('div');
                noItemsMsg.className = 'no-items-message';
                noItemsMsg.style.cssText = [
                    'grid-column: 1 / -1',
                    'text-align: center',
                    'padding: 2rem',
                    'color: #999',
                    'font-style: italic'
                ].join('; ');
                noItemsMsg.textContent = 'No items found matching your search.';
                inventoryGrid.appendChild(noItemsMsg);
            }
        } else if (existingMessage) {
            existingMessage.remove();
        }
    };

    if (searchInput) {
        searchInput.addEventListener('input', filterAndSortItems);
    }

    if (sortSelect) {
        sortSelect.addEventListener('change', filterAndSortItems);
    }

    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            filterButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            filterAndSortItems();
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && searchInput) {
            searchInput.value = '';
            filterAndSortItems();
        }
    });

    filterAndSortItems();
});
