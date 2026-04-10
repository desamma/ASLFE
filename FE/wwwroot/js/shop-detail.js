document.addEventListener('DOMContentLoaded', () => {
    // Initialize related items section
    loadRelatedItems();
});

function loadRelatedItems() {
    // Placeholder for loading related items functionality
    // This can be extended to load items from the same category
    const placeholder = document.querySelector('.related-items-placeholder');
    if (placeholder) {
        placeholder.innerHTML = '<p>Similar items from this category will be displayed here</p>';
    }
}

window.purchaseItemDetail = itemId => {
    alert(`Purchase functionality for item: ${itemId}`);
};
