(() => {
    'use strict';

    const photoInput = document.querySelector('[data-profile-photo-input]');
    const previewImage = document.querySelector('[data-profile-preview-image]');
    const previewFallback = document.querySelector('[data-profile-preview-fallback]');
    const fileName = document.querySelector('[data-profile-file-name]');
    const removePhoto = document.querySelector('[data-remove-profile-photo]');
    const avatarInputs = document.querySelectorAll('[data-avatar-icon]');
    const feedbackText = document.querySelector('[data-feedback-text]');
    const feedbackCount = document.querySelector('[data-feedback-count]');
    const originalPhoto = previewImage?.getAttribute('src') || '';

    const updateFallback = () => {
        const selected = document.querySelector('[data-avatar-icon]:checked');
        if (selected && previewFallback) {
            previewFallback.textContent = selected.dataset.avatarIcon || '</>';
        }
    };

    avatarInputs.forEach(input => {
        input.addEventListener('change', updateFallback);
    });

    photoInput?.addEventListener('change', () => {
        const file = photoInput.files?.[0];
        if (!file) {
            if (fileName) fileName.textContent = 'Ningún archivo seleccionado';
            return;
        }

        if (fileName) fileName.textContent = file.name;
        if (removePhoto) removePhoto.checked = false;

        const objectUrl = URL.createObjectURL(file);
        if (previewImage) {
            previewImage.src = objectUrl;
            previewImage.hidden = false;
            previewImage.onload = () => URL.revokeObjectURL(objectUrl);
        }
        if (previewFallback) previewFallback.hidden = true;
    });

    removePhoto?.addEventListener('change', () => {
        if (removePhoto.checked) {
            if (photoInput) photoInput.value = '';
            if (fileName) fileName.textContent = 'Ningún archivo seleccionado';
            if (previewImage) {
                previewImage.src = '';
                previewImage.hidden = true;
            }
            if (previewFallback) previewFallback.hidden = false;
            updateFallback();
            return;
        }

        if (originalPhoto && previewImage) {
            previewImage.src = originalPhoto;
            previewImage.hidden = false;
            if (previewFallback) previewFallback.hidden = true;
        }
    });

    const updateFeedbackCount = () => {
        if (feedbackText && feedbackCount) {
            feedbackCount.textContent = String(feedbackText.value.length);
        }
    };

    feedbackText?.addEventListener('input', updateFeedbackCount);
    updateFeedbackCount();
    updateFallback();
})();
