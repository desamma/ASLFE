let notificationAction = null;

function escapeHtml(unsafe) {
    return String(unsafe ?? '')
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#039;');
}

function getEditor() {
    return document.getElementById('ContentEditor');
}

function showNotification(title, message, type, actionCallback = null) {
    const modal = document.getElementById('notificationModal');
    const titleElem = document.getElementById('notificationTitle');
    const contentElem = document.getElementById('notificationContent');
    const btnElem = document.getElementById('notificationBtn');

    if (!modal || !titleElem || !contentElem || !btnElem) return;

    titleElem.textContent = title;
    contentElem.innerHTML = `<div class="notification-${type}">${escapeHtml(message)}</div>`;

    btnElem.className = type === 'success' ? 'btn-primary btn-success' : type === 'error' ? 'btn-primary btn-danger' : 'btn-primary';

    notificationAction = actionCallback;
    modal.style.display = 'flex';
    btnElem.focus();
}

function closeNotificationModal(event) {
    if (event && event.target.id !== 'notificationModal') return;

    const modal = document.getElementById('notificationModal');
    if (modal) {
        modal.style.display = 'none';
    }

    notificationAction = null;
}

function handleNotificationAction() {
    const action = notificationAction;
    closeNotificationModal();

    if (typeof action === 'function') {
        action();
    }
}

function showUploadDialog() {
    const dialog = document.getElementById('uploadDialog');
    const uploadFile = document.getElementById('uploadFile');

    if (!dialog || !uploadFile) return;

    dialog.style.display = 'flex';
    document.body.classList.add('modal-open');
    uploadFile.focus();
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

function performImageUpload() {
    const fileInput = document.getElementById('uploadFile');
    const file = fileInput?.files?.[0];

    if (!file) {
        alert('Please select a file');
        return;
    }

    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!antiForgeryToken) {
        showNotification('Error', 'Upload failed: missing security token.', 'error');
        return;
    }

    const uploadBtn = document.getElementById('uploadBtn');
    if (!uploadBtn) return;

    uploadBtn.disabled = true;
    uploadBtn.textContent = 'Uploading...';

    const formData = new FormData();
    formData.append('file', file);
    formData.append('__RequestVerificationToken', antiForgeryToken);

    const uploadUrl = window.createNewsPageUrls?.uploadImage || '?handler=UploadImage';

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
                showNotification('Success', 'Image uploaded and inserted!', 'success');
            } else {
                showNotification('Error', `Upload failed: ${data.error}`, 'error');
            }
        })
        .catch(error => {
            console.error('Upload error:', error);
            showNotification('Error', `Upload failed: ${error.message}`, 'error');
        })
        .finally(() => {
            uploadBtn.disabled = false;
            uploadBtn.textContent = 'Upload & Insert';
        });
}

function addSeparator() {
    const editor = getEditor();
    if (!editor) return;

    const separatorMarker = '[SEPARATOR]';
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const text = editor.value;

    editor.value = text.substring(0, start) + '\n' + separatorMarker + '\n' + text.substring(end);
    editor.selectionStart = editor.selectionEnd = start + separatorMarker.length + 2;
    editor.dispatchEvent(new Event('input', { bubbles: true }));
    editor.focus();
}

function showImageDialog() {
    const dialog = document.getElementById('imageDialog');
    const imageName = document.getElementById('imageName');

    if (!dialog || !imageName) return;

    dialog.style.display = 'flex';
    document.body.classList.add('modal-open');
    imageName.focus();
}

function closeImageDialog(event) {
    if (event && event.target.id !== 'imageDialog') return;

    const dialog = document.getElementById('imageDialog');
    const imageName = document.getElementById('imageName');

    if (dialog) dialog.style.display = 'none';
    document.body.classList.remove('modal-open');
    if (imageName) imageName.value = '';
}

function insertImage() {
    const imageUrl = document.getElementById('imageName')?.value.trim();
    if (!imageUrl) {
        alert('Please enter an image URL');
        return;
    }

    const editor = getEditor();
    if (!editor) return;

    const imageMarker = `[IMAGE:${imageUrl}]`;
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const text = editor.value;

    editor.value = text.substring(0, start) + '\n' + imageMarker + '\n' + text.substring(end);
    editor.selectionStart = editor.selectionEnd = start + imageMarker.length + 2;
    editor.dispatchEvent(new Event('input', { bubbles: true }));

    closeImageDialog();
    editor.focus();
}

function showImageBrowser() {
    const dialog = document.getElementById('imageBrowserDialog');
    if (!dialog) return;

    dialog.style.display = 'flex';
    document.body.classList.add('modal-open');
    loadImageBrowser();
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

    const getImagesUrl = window.createNewsPageUrls?.getImages || '/News/CreateNews?handler=GetImages';

    fetch(getImagesUrl, { headers: { Accept: 'application/json' }, credentials: 'same-origin' })
        .then(async response => {
            const body = await response.text();
            const contentType = response.headers.get('content-type') || '';

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${body}`);
            }

            if (!contentType.includes('application/json')) {
                const isSignInPage = /<title>\s*Sign In\s*-\s*FE\s*<\/title>/i.test(body) || /<title>\s*Sign In\s*<\/title>/i.test(body);
                throw new Error(isSignInPage
                    ? 'Sign in required to browse cloud images.'
                    : `Expected JSON but received ${contentType || 'unknown content type'}: ${body.slice(0, 200)}`);
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
                    <div class="image-browser-item" role="button" tabindex="0" data-image-url="${escapeHtml(imageUrl)}">
                        <img src="${escapeHtml(imageUrl)}" alt="${escapeHtml(image.name)}" class="image-browser-thumbnail" />
                        <div class="image-browser-name" title="${escapeHtml(image.name)}">${escapeHtml(image.name)}</div>
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
            content.innerHTML = `<div class="loading-spinner">${escapeHtml(error.message || 'Error loading images')}</div>`;
        });
}

function selectImageFromBrowser(imageUrl) {
    const editor = getEditor();
    if (!editor) return;

    const imageMarker = `[IMAGE:${imageUrl}]`;
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const text = editor.value;

    editor.value = text.substring(0, start) + '\n' + imageMarker + '\n' + text.substring(end);
    editor.selectionStart = editor.selectionEnd = start + imageMarker.length + 2;
    editor.dispatchEvent(new Event('input', { bubbles: true }));

    closeImageBrowser();
    editor.focus();
}

function resolveNewsImageUrl(imageValue) {
    if (!imageValue) return '';

    if (/^https?:\/\//i.test(imageValue)) {
        return imageValue;
    }

    const bucket = window.createNewsPageUrls?.firebaseBucket;
    if (bucket) {
        const normalizedPath = imageValue.replace(/^[\\/]+/, '').replace(/^News\//i, '');
        return `https://firebasestorage.googleapis.com/v0/b/${bucket}/o/${encodeURIComponent(normalizedPath)}?alt=media`;
    }

    return imageValue;
}

function updatePreview() {
    const editor = getEditor();
    const preview = document.getElementById('contentPreview');
    if (!editor || !preview) return;

    const lines = editor.value.split('\n');
    let previewHTML = '';

    lines.forEach((line, index) => {
        const trimmedLine = line.trim();

        if (trimmedLine === '[SEPARATOR]') {
            previewHTML += '<div class="separator-preview" style="display: flex; align-items: center; justify-content: center; margin: 24px 0; gap: 0;"><div role="separator" aria-hidden="true" style="display: flex; align-items: center; justify-content: center; width: 100%; gap: 12px;"><svg width="19" height="16" viewBox="0 0 19 16" fill="none" xmlns="http://www.w3.org/2000/svg" style="flex-shrink: 0;"><rect y="1.44135" width="7.02738" height="7.02738" transform="matrix(0.693276 0.720673 -0.693276 0.720673 13.4194 2.053)" stroke="currentColor" stroke-width="2"></rect><path d="M9 0.75L2 7.75L9 14.75" stroke="currentColor" stroke-width="2"></path></svg><div style="flex: 1; height: 2px; background-color: currentColor;"></div><svg width="21" height="18" viewBox="0 0 21 18" fill="none" xmlns="http://www.w3.org/2000/svg" style="flex-shrink: 0;"><rect y="1.44135" width="8.03936" height="8.03936" transform="matrix(0.693276 0.720673 -0.693276 0.720673 8.28148 2.35573)" stroke="currentColor" stroke-width="2"></rect><path d="M11.2214 1L19.2214 9L11.2214 17" stroke="currentColor" stroke-width="2"></path></svg></div></div>';
        } else if (trimmedLine.startsWith('[IMAGE:')) {
            const imageMatch = trimmedLine.match(/\[IMAGE:(.*?)\]/);
            if (imageMatch?.[1]) {
                const imageUrl = resolveNewsImageUrl(imageMatch[1]);
                previewHTML += `<img src="${escapeHtml(imageUrl)}" alt="News Image" style="max-width: 100%; height: auto; margin: 12px 0; border-radius: 6px;" onerror="this.style.display='none';" />`;
            }
        } else if (trimmedLine.length > 0) {
            previewHTML += `<p style="margin: 8px 0; line-height: 1.6;">${escapeHtml(trimmedLine)}</p>`;
        } else if (index > 0 && index < lines.length - 1) {
            previewHTML += '<br />';
        }
    });

    preview.innerHTML = previewHTML || '<p style="color: #999;">Preview will appear here...</p>';
}

function initializeNewsEditorPage() {
    const editor = getEditor();
    const descriptionInput = document.getElementById('Input_Description') || document.querySelector('[name="Input.Description"]');
    const uploadFileInput = document.getElementById('uploadFile');
    const uploadPreview = document.getElementById('uploadPreview');
    const uploadImage = document.getElementById('uploadImage');
    const imageName = document.getElementById('imageName');
    const state = window.createNewsPageState || {};

    if (uploadFileInput && uploadPreview && uploadImage) {
        uploadFileInput.addEventListener('change', function (e) {
            const file = e.target.files?.[0];
            if (file && file.type.startsWith('image/')) {
                const reader = new FileReader();
                reader.onload = function (event) {
                    uploadImage.src = event.target?.result;
                    uploadPreview.style.display = 'block';
                };
                reader.readAsDataURL(file);
            } else {
                uploadPreview.style.display = 'none';
            }
        });
    }

    if (descriptionInput) {
        descriptionInput.addEventListener('input', function () {
            const counter = document.getElementById('descriptionCount');
            if (counter) counter.textContent = this.value.length.toString();
        });

        const descriptionCounter = document.getElementById('descriptionCount');
        if (descriptionCounter) descriptionCounter.textContent = descriptionInput.value.length.toString();
    }

    if (editor) {
        editor.addEventListener('input', function () {
            const counter = document.getElementById('contentCount');
            if (counter) counter.textContent = this.value.length.toString();
            updatePreview();
        });

        editor.addEventListener('keydown', function (event) {
            if (event.ctrlKey && event.shiftKey && event.key === 'S') {
                event.preventDefault();
                addSeparator();
            }

            if (event.ctrlKey && event.shiftKey && event.key === 'I') {
                event.preventDefault();
                showImageDialog();
            }
        });

        const contentCounter = document.getElementById('contentCount');
        if (contentCounter) contentCounter.textContent = editor.value.length.toString();
        updatePreview();
    }

    if (imageName) {
        imageName.addEventListener('keypress', function (event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                insertImage();
            }
        });
    }

    if (state.errorMessage?.trim()) {
        showNotification('Error', state.errorMessage, 'error');
    } else if (state.successMessage?.trim()) {
        showNotification('Success', state.successMessage, 'success', state.successRedirectUrl ? () => { window.location.href = state.successRedirectUrl; } : null);
    }

    document.addEventListener('keydown', function (event) {
        if (event.key !== 'Escape') return;

        const notificationModal = document.getElementById('notificationModal');
        const imageDialog = document.getElementById('imageDialog');
        const imageBrowserDialog = document.getElementById('imageBrowserDialog');
        const uploadDialog = document.getElementById('uploadDialog');

        if (notificationModal && notificationModal.style.display !== 'none') {
            closeNotificationModal();
            return;
        }

        if (imageDialog && imageDialog.style.display !== 'none') {
            closeImageDialog();
            return;
        }

        if (imageBrowserDialog && imageBrowserDialog.style.display !== 'none') {
            closeImageBrowser();
            return;
        }

        if (uploadDialog && uploadDialog.style.display !== 'none') {
            closeUploadDialog();
        }
    });
}

document.addEventListener('DOMContentLoaded', initializeNewsEditorPage);
