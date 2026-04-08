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
    const imageName = document.getElementById('imageName').value.trim();
    
    if (!imageName) {
        alert('Please enter an image name');
        return;
    }
    
    const editor = document.getElementById('ContentEditor');
    const imageMarker = `[IMAGE:${imageName}]`;
    
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
    
    fetch('/News/CreateNews?handler=Images')
        .then(response => response.json())
        .then(data => {
            if (!data.success || !data.images || data.images.length === 0) {
                content.innerHTML = '<div class="loading-spinner">No images found</div>';
                return;
            }
            
            let html = '';
            data.images.forEach(image => {
                html += `
                    <div class="image-browser-item" onclick="selectImageFromBrowser('${image.name}')">
                        <img src="${image.path}" alt="${image.name}" class="image-browser-thumbnail" />
                        <div class="image-browser-name" title="${image.name}">${image.name}</div>
                    </div>
                `;
            });
            
            content.innerHTML = html;
        })
        .catch(error => {
            console.error('Error loading images:', error);
            content.innerHTML = '<div class="loading-spinner">Error loading images</div>';
        });
}

function selectImageFromBrowser(imageName) {
    const editor = document.getElementById('ContentEditor');
    const imageMarker = `[IMAGE:${imageName}]`;
    
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
            // Render separator
            previewHTML += '<div class="separator-preview" style="border-top: 2px solid #f5a623; margin: 16px 0;"></div>';
        } else if (trimmedLine.startsWith('[IMAGE:')) {
            // Extract image name
            const imageMatch = trimmedLine.match(/\[IMAGE:(.*?)\]/);
            if (imageMatch && imageMatch[1]) {
                const imageName = imageMatch[1];
                previewHTML += `<img src="/images/${imageName}" alt="News Image" style="max-width: 100%; height: auto; margin: 12px 0; border-radius: 6px;" onerror="this.style.display='none';" />`;
            }
        } else if (trimmedLine.length > 0) {
            // Regular text
            previewHTML += `<p style="margin: 8px 0; line-height: 1.6;">${escapeHtml(trimmedLine)}</p>`;
        } else if (index > 0 && index < lines.length - 1) {
            // Preserve empty lines as spacing
            previewHTML += '<br />';
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
