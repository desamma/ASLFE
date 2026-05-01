document.addEventListener('DOMContentLoaded', function () {
    const successMsg = document.querySelector('.validation-summary[role="status"]');
    const errorMsg = document.querySelector('.validation-summary[role="alert"]');

    if (successMsg && successMsg.textContent.trim()) {
        // Clear success message after 4 seconds
        setTimeout(function () {
            successMsg.textContent = '';
            successMsg.style.display = 'none';
        }, 4000);
    }
});

let notificationAction = null;
function showNotification(title, message, type, actionCallback = null) {
    const modal = document.getElementById('notificationModal');
    const titleElem = document.getElementById('notificationTitle');
    const contentElem = document.getElementById('notificationContent');
    const btnElem = document.getElementById('notificationBtn');

    titleElem.textContent = title;
    contentElem.innerHTML = `<div class="notification-${type}">${escapeHtml(message)}</div>`;

    // Add type-specific styling
    if (type === 'success') {
        btnElem.className = 'btn-primary btn-success';
    } else if (type === 'error') {
        btnElem.className = 'btn-primary btn-danger';
    } else {
        btnElem.className = 'btn-primary';
    }

    notificationAction = actionCallback;
    modal.classList.add('active');
    btnElem.focus();
}

function closeNotificationModal(event) {
    if (event && event.target.id !== 'notificationModal') return;
    document.getElementById('notificationModal').classList.remove('active');
    notificationAction = null;
}

function handleNotificationAction() {
    closeNotificationModal();
    if (notificationAction && typeof notificationAction === 'function') {
        notificationAction();
    }
}

function escapeHtml(unsafe) {
    return unsafe
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

document.addEventListener('DOMContentLoaded', function () {
    const titleInput = document.getElementById('Input_Title');
    const descriptionInput = document.getElementById('Input_Description');
    const stepsInput = document.getElementById('Input_Steps');
    const expectedInput = document.getElementById('Input_ExpectedBehavior');
    const actualInput = document.getElementById('Input_ActualBehavior');
    const validationSummary = document.querySelector('.validation-summary');

    // Show validation summary if there are errors
    if (validationSummary && validationSummary.querySelector('ul li')) {
        validationSummary.style.display = 'block';
    }

    // Character counter for title
    if (titleInput) {
        titleInput.addEventListener('input', function () {
            document.getElementById('titleCount').textContent = this.value.length;
        });
    }

    // Character counter for description
    if (descriptionInput) {
        descriptionInput.addEventListener('input', function () {
            document.getElementById('descriptionCount').textContent = this.value.length;
        });
    }

    // Character counter for steps
    if (stepsInput) {
        stepsInput.addEventListener('input', function () {
            document.getElementById('stepsCount').textContent = this.value.length;
        });
    }

    // Character counter for expected
    if (expectedInput) {
        expectedInput.addEventListener('input', function () {
            document.getElementById('expectedCount').textContent = this.value.length;
        });
    }

    // Character counter for actual behavior
    if (actualInput) {
        actualInput.addEventListener('input', function () {
            document.getElementById('actualCount').textContent = this.value.length;
        });
    }

    // Initial character counts
    if (titleInput) {
        document.getElementById('titleCount').textContent = titleInput.value.length;
    }
    if (descriptionInput) {
        document.getElementById('descriptionCount').textContent = descriptionInput.value.length;
    }
    if (stepsInput) {
        document.getElementById('stepsCount').textContent = stepsInput.value.length;
    }
    if (expectedInput) {
        document.getElementById('expectedCount').textContent = expectedInput.value.length;
    }
    if (actualInput) {
        document.getElementById('actualCount').textContent = actualInput.value.length;
    }

    // Show notification if there's a success or error message
    const errorMessage = `@Model.ErrorMessage`;
    const successMessage = `@Model.SuccessMessage`;

    if (errorMessage && errorMessage.trim()) {
        showNotification('⚠️ Error', errorMessage, 'error');
    } else if (successMessage && successMessage.trim()) {
        showNotification(
            '✓ Success',
            successMessage,
            'success',
            function () {
                // Stay on page after success to allow submitting another report
            }
        );
    }

    // Allow closing notification modal with Escape key
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            const notificationModal = document.getElementById('notificationModal');
            if (notificationModal && notificationModal.classList.contains('active')) {
                closeNotificationModal();
            }
        }
    });
});