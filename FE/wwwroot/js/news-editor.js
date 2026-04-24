// Add Separator to Text
function addSeparator() {
    const editor = document.getElementById('ContentEditor');
    const separatorMarker = '[SEPARATOR]';
    
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const text = editor.value;
    
    // Insert separator at cursor position
    const newText = text.substring(0, start) + '\n' + separatorMarker + '\n' + text.substring(end);
    editor.value = newText;
    editor.selectionStart = editor.selectionEnd = start + separatorMarker.length + 2;
    
    // Trigger input event to update counter and preview
    editor.dispatchEvent(new Event('input', { bubbles: true }));
    editor.focus();
}

// Add Image to Text
function showImageDialog() {
    document.getElementById('imageDialog').style.display = 'flex';
    document.body.classList.add('modal-open');
    document.getElementById('imageName').focus();
}

function closeImageDialog(event) {
    if (event && event.target.id !== 'imageDialog') return;
    document.getElementById('imageDialog').style.display = 'none';
    document.body.classList.remove('modal-open');
    document.getElementById('imageName').value = '';
}

function insertImage() {
const imageUrl = document.getElementById('imageName').value.trim();
    
if (!imageUrl) {
    alert('Please enter an image URL');
    return;
}
    
const editor = document.getElementById('ContentEditor');
const imageMarker = `[IMAGE:${imageUrl}]`;
    
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const text = editor.value;
    
    // Insert image marker at cursor position
    const newText = text.substring(0, start) + '\n' + imageMarker + '\n' + text.substring(end);
    editor.value = newText;
    editor.selectionStart = editor.selectionEnd = start + imageMarker.length + 2;
    
    // Trigger input event to update counter and preview
    editor.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Close dialog
    closeImageDialog();
    editor.focus();
}

// Show Image Browser
function showImageBrowser() {
    document.getElementById('imageBrowserDialog').style.display = 'flex';
    document.body.classList.add('modal-open');
    loadImageBrowser();
}

function closeImageBrowser(event) {
    if (event && event.target.id !== 'imageBrowserDialog') return;
    document.getElementById('imageBrowserDialog').style.display = 'none';
    document.body.classList.remove('modal-open');
}

function loadImageBrowser() {
const content = document.getElementById('imageBrowserContent');
content.innerHTML = '<div class="loading-spinner">Loading images...</div>';
    
const getImagesUrl = window.createNewsPageUrls?.getImages || '/News/CreateNews?handler=GetImages';

fetch(getImagesUrl, { headers: { 'Accept': 'application/json' }, credentials: 'same-origin' })
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
    const editor = document.getElementById('ContentEditor');
    const imageMarker = `[IMAGE:${imageUrl}]`;
    
    const start = editor.selectionStart;
    const end = editor.selectionEnd;
    const text = editor.value;
    
    // Insert image marker at cursor position
    const newText = text.substring(0, start) + '\n' + imageMarker + '\n' + text.substring(end);
    editor.value = newText;
    editor.selectionStart = editor.selectionEnd = start + imageMarker.length + 2;
    
    // Trigger input event to update counter and preview
    editor.dispatchEvent(new Event('input', { bubbles: true }));
    
    // Close dialog
    closeImageBrowser();
    editor.focus();
}

// Update Preview
function updatePreview() {
    const editor = document.getElementById('ContentEditor');
    const preview = document.getElementById('contentPreview');
    const content = editor.value;
    
    // Split content by lines and process markers
    const lines = content.split('\n');
    let previewHTML = '';
    
    lines.forEach((line, index) => {
        const trimmedLine = line.trim();
        
        if (trimmedLine === '[SEPARATOR]') {
            // Render separator with SVG arrows and connecting line
            previewHTML += '<div class="separator-preview" style="display: flex; align-items: center; justify-content: center; margin: 24px 0; gap: 0;"><div role="separator" aria-hidden="true" style="display: flex; align-items: center; justify-content: center; width: 100%; gap: 12px;"><svg width="19" height="16" viewBox="0 0 19 16" fill="none" xmlns="http://www.w3.org/2000/svg" style="flex-shrink: 0;"><rect y="1.44135" width="7.02738" height="7.02738" transform="matrix(0.693276 0.720673 -0.693276 0.720673 13.4194 2.053)" stroke="currentColor" stroke-width="2"></rect><path d="M9 0.75L2 7.75L9 14.75" stroke="currentColor" stroke-width="2"></path></svg><div style="flex: 1; height: 2px; background-color: currentColor;"></div><svg width="21" height="18" viewBox="0 0 21 18" fill="none" xmlns="http://www.w3.org/2000/svg" style="flex-shrink: 0;"><rect y="1.44135" width="8.03936" height="8.03936" transform="matrix(0.693276 0.720673 -0.693276 0.720673 8.28148 2.35573)" stroke="currentColor" stroke-width="2"></rect><path d="M11.2214 1L19.2214 9L11.2214 17" stroke="currentColor" stroke-width="2"></path></svg></div></div>';
        } else if (trimmedLine.startsWith('[IMAGE:')) {
            // Extract image name
            const imageMatch = trimmedLine.match(/\[IMAGE:(.*?)\]/);
            if (imageMatch && imageMatch[1]) {
                const imageUrl = resolveNewsImageUrl(imageMatch[1]);
                previewHTML += `<img src="${imageUrl}" alt="News Image" style="max-width: 100%; height: auto; margin: 12px 0; border-radius: 6px;" onerror="this.style.display='none';" />`;
            }
        } else if (trimmedLine.length > 0) {
            // Regular text
            previewHTML += `<p style="margin: 8px 0; line-height: 1.6;">${escapeHtml(trimmedLine)}</p>`;
        } else if (index > 0 && index < lines.length - 1) {
            // Preserve empty lines as spacing
            previewHTML += '<br />';
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
    });
    
    preview.innerHTML = previewHTML || '<p style="color: #999;">Preview will appear here...</p>';
}

// Escape HTML to prevent XSS
function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

// Handle keyboard shortcuts
document.addEventListener('DOMContentLoaded', function() {
    const editor = document.getElementById('ContentEditor');
    
    if (editor) {
        editor.addEventListener('keydown', function(event) {
            // Ctrl+Shift+S for separator
            if (event.ctrlKey && event.shiftKey && event.key === 'S') {
                event.preventDefault();
                addSeparator();
            }
            // Ctrl+Shift+I for image
            if (event.ctrlKey && event.shiftKey && event.key === 'I') {
                event.preventDefault();
                showImageDialog();
            }
        });
    }

    // Allow closing dialog with Escape key
    document.addEventListener('keydown', function(event) {
        if (event.key === 'Escape') {
            const imageDialog = document.getElementById('imageDialog');
            const imageBrowserDialog = document.getElementById('imageBrowserDialog');
            if (imageDialog && imageDialog.style.display !== 'none') {
                closeImageDialog();
            }
            if (imageBrowserDialog && imageBrowserDialog.style.display !== 'none') {
                closeImageBrowser();
            }
        }
    });
});

// Handle image dialog Enter key
document.addEventListener('DOMContentLoaded', function() {
    const imageName = document.getElementById('imageName');
    if (imageName) {
        imageName.addEventListener('keypress', function(event) {
            if (event.key === 'Enter') {
                event.preventDefault();
                insertImage();
            }
        });
    }
});
