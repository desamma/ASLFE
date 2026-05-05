document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('itemSearch');
    const filterButtons = document.querySelectorAll('.filter-btn');
    const inventoryGrid = document.getElementById('inventoryGrid');
    const inventoryItems = document.querySelectorAll('.inventory-item');

    // Search functionality
    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const searchTerm = this.value.toLowerCase();
            filterItems(searchTerm, getActiveFilter());
        });
    }

    // Filter functionality
    filterButtons.forEach(button => {
        button.addEventListener('click', function () {
            // Update active button
            filterButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');

            const filter = this.dataset.filter;
            const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
            filterItems(searchTerm, filter);
        });
    });

    function getActiveFilter() {
        const activeButton = document.querySelector('.filter-btn.active');
        return activeButton ? activeButton.dataset.filter : 'all';
    }

    function filterItems(searchTerm, filter) {
        let visibleCount = 0;

        inventoryItems.forEach(item => {
            const itemName = item.dataset.itemName;
            const itemType = item.dataset.itemType;

            // Check search term
            const matchesSearch = !searchTerm || itemName.includes(searchTerm);

            // Check filter
            const matchesFilter = filter === 'all' || itemType === filter;

            // Show/hide item
            if (matchesSearch && matchesFilter) {
                item.style.display = '';
                visibleCount++;
            } else {
                item.style.display = 'none';
            }
        });

        // Show empty message if no items found
        if (visibleCount === 0) {
            if (!document.querySelector('.no-items-message')) {
                const noItemsMsg = document.createElement('div');
                noItemsMsg.className = 'no-items-message';
                noItemsMsg.style.cssText = `
                    grid-column: 1 / -1;
                    text-align: center;
                    padding: 2rem;
                    color: #999;
                    font-style: italic;
                `;
                noItemsMsg.textContent = 'No items found matching your search.';
                inventoryGrid.appendChild(noItemsMsg);
            }
        } else {
            const noItemsMsg = document.querySelector('.no-items-message');
            if (noItemsMsg) {
                noItemsMsg.remove();
            }
        }
    }

    // Add keyboard shortcuts for filter buttons
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            if (searchInput) {
                searchInput.value = '';
                searchInput.dispatchEvent(new Event('input'));
            }
        }
    });
});
