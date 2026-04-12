document.addEventListener('DOMContentLoaded', () => {
    const typeFilter = document.getElementById('typeFilter');
    const locationFilter = document.getElementById('locationFilter');
    const searchInput = document.getElementById('searchInput');
    const resetFiltersBtn = document.getElementById('resetFilters');
    const clearFiltersBtnNoResults = document.getElementById('clearFiltersBtn');
    const npcCards = document.querySelectorAll('.npc-card');
    const npcCount = document.getElementById('npcCount');
    const noResults = document.getElementById('noResults');
    const npcsGrid = document.getElementById('npcsGrid');

    const applyFilters = () => {
        const selectedType = typeFilter.value.toLowerCase();
        const selectedLocation = locationFilter.value.toLowerCase();
        const searchTerm = searchInput.value.toLowerCase();

        let visibleCount = 0;

        npcCards.forEach(card => {
            const cardType = card.dataset.type.toLowerCase();
            const cardLocation = card.dataset.location.toLowerCase();
            const cardName = card.dataset.name.toLowerCase();
            const cardDescription = card.dataset.description.toLowerCase();

            const typeMatch = !selectedType || cardType === selectedType;
            const locationMatch = !selectedLocation || cardLocation === selectedLocation;
            const searchMatch = !searchTerm || cardName.includes(searchTerm) || cardDescription.includes(searchTerm);

            if (typeMatch && locationMatch && searchMatch) {
                card.style.display = 'flex';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Update NPC count
        npcCount.innerHTML = `Showing <strong>${visibleCount}</strong> NPCs`;

        // Show/hide no results message
        if (visibleCount === 0) {
            npcsGrid.style.display = 'none';
            noResults.style.display = 'flex';
        } else {
            npcsGrid.style.display = 'grid';
            noResults.style.display = 'none';
        }
    };

    const resetFilters = () => {
        typeFilter.value = '';
        locationFilter.value = '';
        searchInput.value = '';
        applyFilters();
    };

    // Event listeners
    typeFilter.addEventListener('change', applyFilters);
    locationFilter.addEventListener('change', applyFilters);
    searchInput.addEventListener('input', applyFilters);
    resetFiltersBtn.addEventListener('click', resetFilters);
    clearFiltersBtnNoResults.addEventListener('click', resetFilters);

    // Add click event to NPC cards to navigate to detail page
    npcCards.forEach(card => {
        card.addEventListener('click', () => {
            const npcId = card.dataset.id;
            if (npcId) {
                window.location.href = `/NPCs/Detail/${npcId}`;
            }
        });
    });
});
