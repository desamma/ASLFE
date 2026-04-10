document.addEventListener('DOMContentLoaded', () => {
    const tabs = document.querySelectorAll('.category-tab');
    const categories = document.querySelectorAll('.shop-category');

    const setActiveCategory = (categoryId, activeTab) => {
        tabs.forEach(tab => tab.classList.remove('active'));
        categories.forEach(category => category.classList.remove('active'));

        activeTab.classList.add('active');

        const targetCategory = document.getElementById(categoryId);
        if (targetCategory) {
            targetCategory.classList.add('active');
        }
    };

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const categoryId = tab.dataset.category;
            if (!categoryId) {
                return;
            }

            setActiveCategory(categoryId, tab);
        });
    });
});
