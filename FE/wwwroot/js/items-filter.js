document.addEventListener('DOMContentLoaded', () => {
    const typeFilter = document.getElementById('typeFilter');
    const rarityFilter = document.getElementById('rarityFilter');
    const searchInput = document.getElementById('searchInput');
    const resetFiltersBtn = document.getElementById('resetFilters');
    const clearFiltersBtnNoResults = document.getElementById('clearFiltersBtn');
    const itemCards = document.querySelectorAll('.item-card');
    const itemCount = document.getElementById('itemCount');
    const noResults = document.getElementById('noResults');
    const itemsGrid = document.getElementById('itemsGrid');

    const applyFilters = () => {
        const selectedType = typeFilter.value.toLowerCase();
        const selectedRarity = rarityFilter.value.toLowerCase();
        const searchTerm = searchInput.value.toLowerCase();

        let visibleCount = 0;

        itemCards.forEach(card => {
            const cardType = card.dataset.type.toLowerCase();
            const cardRarity = card.dataset.rarity.toLowerCase();
            const cardName = card.dataset.name.toLowerCase();
            const cardDescription = card.dataset.description.toLowerCase();

            const typeMatch = !selectedType || cardType === selectedType;
            const rarityMatch = !selectedRarity || cardRarity === selectedRarity;
            const searchMatch = !searchTerm || cardName.includes(searchTerm) || cardDescription.includes(searchTerm);

            if (typeMatch && rarityMatch && searchMatch) {
                card.style.display = 'flex';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Update item count
        itemCount.innerHTML = `Showing <strong>${visibleCount}</strong> items`;

        // Show/hide no results message
        if (visibleCount === 0) {
            itemsGrid.style.display = 'none';
            noResults.style.display = 'flex';
        } else {
            itemsGrid.style.display = 'grid';
            noResults.style.display = 'none';
        }
    };

    const resetFilters = () => {
        typeFilter.value = '';
        rarityFilter.value = '';
        searchInput.value = '';
        applyFilters();
    };

    // Event listeners
    typeFilter.addEventListener('change', applyFilters);
    rarityFilter.addEventListener('change', applyFilters);
    searchInput.addEventListener('input', applyFilters);
    resetFiltersBtn.addEventListener('click', resetFilters);
    clearFiltersBtnNoResults.addEventListener('click', resetFilters);

    // Add click event to item cards to navigate to detail page
    itemCards.forEach(card => {
        card.addEventListener('click', () => {
            const itemId = card.dataset.id;
            if (itemId) {
                window.location.href = `/Items/Detail/${itemId}`;
            }
        });
    });
});
