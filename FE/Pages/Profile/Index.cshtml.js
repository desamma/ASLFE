document.addEventListener('DOMContentLoaded', function () {
    const fileInput = document.querySelector('input[type="file"][name="AvatarFile"]');
    const uploadBox = document.querySelector('.avatar-upload-box');
    const previewContainer = document.getElementById('uploadPreview');
    const previewImage = document.getElementById('previewImage');

    if (!fileInput) return;

    // File selection
    fileInput.addEventListener('change', handleFileSelect);

    // Drag and drop
    if (uploadBox) {
        uploadBox.addEventListener('dragover', handleDragOver);
        uploadBox.addEventListener('dragleave', handleDragLeave);
        uploadBox.addEventListener('drop', handleDrop);
    }

    function handleFileSelect(event) {
        const file = event.target.files?.[0];
        if (file) {
            displayPreview(file);
        }
    }

    function handleDragOver(event) {
        event.preventDefault();
        event.stopPropagation();
        uploadBox.classList.add('dragover');
    }

    function handleDragLeave(event) {
        event.preventDefault();
        event.stopPropagation();
        uploadBox.classList.remove('dragover');
    }

    function handleDrop(event) {
        event.preventDefault();
        event.stopPropagation();
        uploadBox.classList.remove('dragover');

        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
            const file = files[0];
            
            // Validate file type
            if (!file.type.startsWith('image/')) {
                alert('Please select an image file');
                return;
            }

            // Set file to input
            fileInput.files = files;
            displayPreview(file);
        }
    }

    function displayPreview(file) {
        const reader = new FileReader();
        reader.onload = function (event) {
            previewImage.src = event.target?.result;
            previewContainer.style.display = 'block';
        };
        reader.readAsDataURL(file);
    }
});
