document.addEventListener('DOMContentLoaded', function () {
    // Try multiple selectors to find the file input
    const fileInput = document.querySelector('input[type="file"]') || document.querySelector('[name="AvatarFile"]');
    const previewContainer = document.getElementById('uploadPreview');
    const previewImage = document.getElementById('previewImage');

    if (!fileInput || !previewContainer || !previewImage) {
        console.warn('Required elements not found for image preview');
        return;
    }

    fileInput.addEventListener('change', function (event) {
        const file = event.target.files?.[0];
        if (file) {
            // Validate file is an image
            if (!file.type.startsWith('image/')) {
                alert('Please select a valid image file');
                return;
            }

            const reader = new FileReader();
            reader.onload = function (e) {
                previewImage.src = e.target?.result;
                previewContainer.style.display = 'block';
            };
            reader.readAsDataURL(file);
        }
    });
});
