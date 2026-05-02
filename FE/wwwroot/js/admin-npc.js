window.adminNpcPageUrls = window.adminNpcPageUrls || {};

let currentUploadMode = 'create';

function openCreateModal() {
    document.getElementById('createModal')?.classList.add('show');
}

function closeCreateModal() {
    document.getElementById('createModal')?.classList.remove('show');
}

function openEditModal(btn) {
    document.getElementById('edit_id').value = btn.getAttribute('data-id');
    document.getElementById('edit_name').value = btn.getAttribute('data-name');
    document.getElementById('edit_description').value = btn.getAttribute('data-desc');
    document.getElementById('edit_npcType').value = btn.getAttribute('data-type');
    document.getElementById('edit_location').value = btn.getAttribute('data-location');
    document.getElementById('edit_imagePath').value = btn.getAttribute('data-img');
    document.getElementById('edit_imagePathDisplay').value = btn.getAttribute('data-img');

    updateImagePreview('edit');
    document.getElementById('editModal')?.classList.add('show');
}

function closeEditModal() {
    document.getElementById('editModal')?.classList.remove('show');
}

function showImageBrowser(mode) {
    currentUploadMode = mode;
    const dialog = document.getElementById('imageBrowserDialog');
    if (dialog) {
        dialog.style.display = 'flex';
        document.body.classList.add('modal-open');
        loadImageBrowser();
    }
}

function closeImageBrowser(event) {
    if (event && event.target.id !== 'imageBrowserDialog') return;
    const dialog = document.getElementById('imageBrowserDialog');
    if (dialog) dialog.style.display = 'none';
    document.body.classList.remove('modal-open');
}

function loadImageBrowser() {
    const content = document.getElementById('imageBrowserContent');
    if (!content) return;

    content.innerHTML = '<div class="loading-spinner">Loading images...</div>';

    const getImagesUrl = window.adminNpcPageUrls?.getImages || '/Admin/AdminNPC?handler=GetImages';

    fetch(getImagesUrl, { headers: { Accept: 'application/json' }, credentials: 'same-origin' })
        .then(async response => {
            const body = await response.text();
            const contentType = response.headers.get('content-type') || '';

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${body}`);
            }

            if (!contentType.includes('application/json')) {
                throw new Error(`Expected JSON but received ${contentType || 'unknown content type'}`);
            }

            return JSON.parse(body);
        })
        .then(data => {
            if (!data.success || !data.images || data.images.length === 0) {
                content.innerHTML = '<div class="loading-spinner">No images found</div>';
                return;
            }

            let html = '';
            data.images.forEach(image => {
                const imageUrl = image.url || image.path || '';
                html += `
                    <div class="image-browser-item" role="button" tabindex="0" data-image-url="${imageUrl}">
                        <img src="${imageUrl}" alt="${image.name}" class="image-browser-thumbnail" onerror="this.style.display='none';" />
                        <div class="image-browser-name" title="${image.name}">${image.name}</div>
                    </div>
                `;
            });

            content.innerHTML = html;
            content.querySelectorAll('.image-browser-item').forEach(item => {
                item.addEventListener('click', () => selectImageFromBrowser(item.dataset.imageUrl || ''));
                item.addEventListener('keydown', event => {
                    if (event.key === 'Enter' || event.key === ' ') {
                        event.preventDefault();
                        selectImageFromBrowser(item.dataset.imageUrl || '');
                    }
                });
            });
        })
        .catch(error => {
            console.error('Error loading images:', error);
            content.innerHTML = `<div class="loading-spinner">${error.message || 'Error loading images'}</div>`;
        });
}

function selectImageFromBrowser(imageUrl) {
    if (currentUploadMode === 'create') {
        document.getElementById('create_imagePath').value = imageUrl;
        updateImagePreview('create');
    } else {
        document.getElementById('edit_imagePath').value = imageUrl;
        document.getElementById('edit_imagePathDisplay').value = imageUrl;
        updateImagePreview('edit');
    }
    closeImageBrowser();
}

function showUploadDialog(mode) {
    currentUploadMode = mode;
    const dialog = document.getElementById('uploadDialog');
    const uploadFile = document.getElementById('uploadFile');

    if (dialog && uploadFile) {
        dialog.style.display = 'flex';
        document.body.classList.add('modal-open');
        uploadFile.focus();
    }
}

function closeUploadDialog(event) {
    if (event && event.target.id !== 'uploadDialog') return;

    const dialog = document.getElementById('uploadDialog');
    const uploadFile = document.getElementById('uploadFile');
    const uploadPreview = document.getElementById('uploadPreview');
    const uploadStatus = document.getElementById('uploadStatus');

    if (dialog) dialog.style.display = 'none';
    document.body.classList.remove('modal-open');

    if (uploadFile) uploadFile.value = '';
    if (uploadPreview) uploadPreview.style.display = 'none';
    if (uploadStatus) uploadStatus.style.display = 'none';
}

document.addEventListener('DOMContentLoaded', function () {
    const uploadFile = document.getElementById('uploadFile');
    const uploadPreview = document.getElementById('uploadPreview');
    const uploadImage = document.getElementById('uploadImage');

    if (uploadFile && uploadPreview && uploadImage) {
        uploadFile.addEventListener('change', function (e) {
            const file = e.target.files?.[0];
            if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = (event) => {
                    uploadImage.src = event.target?.result || '';
                    uploadPreview.style.display = 'block';
                };
                reader.readAsDataURL(file);
            }
        });
    }
});

function performImageUpload() {
    const fileInput = document.getElementById('uploadFile');
    const file = fileInput?.files?.[0];

    if (!file) {
        alert('Please select a file');
        return;
    }

    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!antiForgeryToken) {
        alert('Upload failed: missing security token.');
        return;
    }

    const uploadBtn = document.getElementById('uploadBtn');
    if (!uploadBtn) return;

    uploadBtn.disabled = true;
    uploadBtn.textContent = 'Uploading...';

    const formData = new FormData();
    formData.append('file', file);
    formData.append('__RequestVerificationToken', antiForgeryToken);

    const uploadUrl = window.adminNpcPageUrls?.uploadImage || '/Admin/AdminNPC?handler=UploadImage';

    fetch(uploadUrl, {
        method: 'POST',
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(`HTTP ${response.status}: ${errorText}`);
            }

            return response.json();
        })
        .then(data => {
            if (data.success) {
                selectImageFromBrowser(data.url);
                closeUploadDialog();
                alert('Image uploaded and inserted!');
            } else {
                alert(`Upload failed: ${data.error}`);
            }
        })
        .catch(error => {
            console.error('Upload error:', error);
            alert(`Upload failed: ${error.message}`);
        })
        .finally(() => {
            uploadBtn.disabled = false;
            uploadBtn.textContent = 'Upload & Insert';
        });
}

function updateImagePreview(mode) {
    const imagePathInput = mode === 'create'
        ? document.getElementById('create_imagePath')
        : document.getElementById('edit_imagePath');

    const previewBox = mode === 'create'
        ? document.getElementById('create_imagePreview')
        : document.getElementById('edit_imagePreview');

    const previewImg = mode === 'create'
        ? document.getElementById('create_previewImg')
        : document.getElementById('edit_previewImg');

    if (imagePathInput && previewBox && previewImg) {
        const imageUrl = imagePathInput.value;
        if (imageUrl) {
            previewImg.src = imageUrl;
            previewBox.style.display = 'block';
            previewImg.onerror = () => { previewBox.style.display = 'none'; };
        } else {
            previewBox.style.display = 'none';
        }
    }
}

document.addEventListener('DOMContentLoaded', function () {
    const createImagePath = document.getElementById('create_imagePath');
    const editImagePath = document.getElementById('edit_imagePath');

    if (createImagePath) {
        createImagePath.addEventListener('change', () => updateImagePreview('create'));
    }
    if (editImagePath) {
        editImagePath.addEventListener('change', () => updateImagePreview('edit'));
    }
});