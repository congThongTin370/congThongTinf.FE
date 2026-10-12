// Hỗ trợ giao diện soạn bài: đường dẫn, xem trước ảnh và hộp thoại.
(function () {
    var form = document.getElementById('news-composer-form');
    if (!form) {
        return;
    }

    var titleInput = form.querySelector('[data-news-title]');
    var slugInput = form.querySelector('[data-news-slug]');

    function toSlug(value) {
        return value
            .replace(/đ/g, 'd')
            .replace(/Đ/g, 'D')
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .toLowerCase()
            .replace(/[^a-z0-9]+/g, '-')
            .replace(/^-+|-+$/g, '');
    }

    function updateSlug() {
        if (!titleInput || !slugInput) {
            return;
        }
        slugInput.value = toSlug(titleInput.value);
    }

    if (titleInput) {
        titleInput.addEventListener('input', updateSlug);
        updateSlug();
    }

    var cover = form.querySelector('[data-cover]');
    var coverUrl = form.querySelector('[data-cover-url]');
    var coverFile = form.querySelector('[data-cover-file]');
    var coverImage = form.querySelector('[data-cover-image]');
    var coverEmpty = form.querySelector('[data-cover-empty]');
    var localPreviewUrl = '';

    function setCoverPreview(src) {
        if (!coverImage || !coverEmpty) {
            return;
        }
        if (!src) {
            coverImage.hidden = true;
            coverImage.removeAttribute('src');
            coverEmpty.hidden = false;
            return;
        }
        coverImage.src = src;
        coverImage.hidden = false;
        coverEmpty.hidden = true;
    }

    function refreshCoverPreview() {
        var url = coverUrl ? coverUrl.value.trim() : '';
        setCoverPreview(localPreviewUrl || url);
    }

    if (coverImage) {
        coverImage.addEventListener('error', function () {
            coverImage.hidden = true;
            if (coverEmpty) {
                coverEmpty.hidden = false;
                coverEmpty.textContent = 'Không hiển thị được ảnh này.';
            }
        });
        coverImage.addEventListener('load', function () {
            if (coverEmpty) {
                coverEmpty.textContent = 'Chưa có ảnh đại diện';
            }
        });
    }

    var uploadButton = form.querySelector('[data-cover-upload]');
    if (uploadButton && coverFile) {
        uploadButton.addEventListener('click', function () {
            coverFile.click();
        });
        coverFile.addEventListener('change', function () {
            var file = coverFile.files && coverFile.files[0];
            if (!file) {
                return;
            }
            if (localPreviewUrl) {
                URL.revokeObjectURL(localPreviewUrl);
            }
            localPreviewUrl = URL.createObjectURL(file);
            if (coverEmpty) {
                coverEmpty.textContent = 'Chưa có ảnh đại diện';
            }
            refreshCoverPreview();
        });
    }

    if (cover) {
        refreshCoverPreview();
    }

    var libraryDialog = document.getElementById('library-dialog');
    var libraryOpen = form.querySelector('[data-cover-library]');
    if (libraryOpen) {
        libraryOpen.addEventListener('click', function () {
            window.AppDialog.open(libraryDialog);
        });
    }

    var previewDialog = document.getElementById('preview-dialog');
    var previewOpen = form.querySelector('[data-preview-open]');

    function selectedText(selector) {
        var field = form.querySelector(selector);
        if (!field || field.selectedIndex < 0) {
            return '';
        }
        var option = field.options[field.selectedIndex];
        return option && option.value ? option.text : '';
    }

    // Chèn ảnh: upload qua endpoint frontend (token nằm ở server), rồi chèn ![image](url) tại vị trí con trỏ
    var contentEditor = form.querySelector('[data-content-editor]');
    var imageButton = form.querySelector('[data-image-insert]');
    var imageFile = form.querySelector('[data-image-file]');
    var imageStatus = form.querySelector('[data-image-status]');
    var uploadUrl = imageButton ? imageButton.getAttribute('data-upload-url') : '';
    var antiForgery = form.querySelector('input[name="__RequestVerificationToken"]');

    function setImageStatus(text) {
        if (imageStatus) {
            imageStatus.textContent = text || '';
        }
    }

    function insertAtCursor(text) {
        var start = contentEditor.selectionStart;
        var end = contentEditor.selectionEnd;
        var value = contentEditor.value;
        var prefix = start > 0 && value.charAt(start - 1) !== '\n' ? '\n' : '';
        var suffix = end < value.length && value.charAt(end) !== '\n' ? '\n' : '';
        var insert = prefix + text + suffix;
        contentEditor.value = value.slice(0, start) + insert + value.slice(end);
        var caret = start + insert.length;
        contentEditor.setSelectionRange(caret, caret);
        contentEditor.focus();
    }

    if (imageButton && imageFile && contentEditor) {
        imageButton.addEventListener('click', function () {
            imageFile.click();
        });

        imageFile.addEventListener('change', function () {
            var file = imageFile.files && imageFile.files[0];
            imageFile.value = '';
            if (!file) {
                return;
            }

            setImageStatus('Đang tải ảnh lên...');
            imageButton.disabled = true;

            var data = new FormData();
            data.append('file', file);
            if (antiForgery) {
                data.append('__RequestVerificationToken', antiForgery.value);
            }

            fetch(uploadUrl, { method: 'POST', body: data, credentials: 'same-origin' })
                .then(function (response) {
                    return response.json().catch(function () {
                        return { success: false, message: 'Chưa tải được ảnh. Vui lòng thử lại.' };
                    });
                })
                .then(function (result) {
                    if (result && result.success && result.url) {
                        insertAtCursor('![image](' + result.url + ')');
                        setImageStatus('Đã chèn ảnh vào nội dung.');
                    } else {
                        setImageStatus((result && result.message) || 'Chưa tải được ảnh. Vui lòng thử lại.');
                    }
                })
                .catch(function () {
                    setImageStatus('Chưa tải được ảnh. Vui lòng kiểm tra kết nối và thử lại.');
                })
                .finally(function () {
                    imageButton.disabled = false;
                });
        });
    }

    if (previewOpen) {
        previewOpen.addEventListener('click', function () {
            var title = titleInput && titleInput.value.trim() ? titleInput.value.trim() : 'Chưa có tiêu đề';
            var category = selectedText('[name="CategoryId"]');
            var type = selectedText('[data-news-type]');
            var kicker = [category, type].filter(Boolean).join(' · ');
            var text = contentEditor ? contentEditor.value.trim() : '';

            var kickerEl = form.querySelector('[data-preview-kicker]');
            var titleEl = form.querySelector('[data-preview-title]');
            var figure = form.querySelector('[data-preview-figure]');
            var image = form.querySelector('[data-preview-image]');
            var body = form.querySelector('[data-preview-body]');

            if (kickerEl) {
                kickerEl.textContent = kicker;
                kickerEl.hidden = !kicker;
            }
            if (titleEl) {
                titleEl.textContent = title;
            }

            var src = localPreviewUrl || (coverUrl && coverUrl.value.trim());
            if (figure && image) {
                if (src) {
                    image.src = src;
                    image.alt = title;
                    figure.hidden = false;
                } else {
                    figure.hidden = true;
                    image.removeAttribute('src');
                }
            }
            if (body) {
                body.textContent = '';
                if (text) {
                    text.split(/\n\s*\n/).forEach(function (block) {
                        var p = document.createElement('p');
                        p.textContent = block;
                        body.appendChild(p);
                    });
                } else {
                    var empty = document.createElement('p');
                    empty.className = 'composer-preview-empty';
                    empty.textContent = 'Chưa có nội dung.';
                    body.appendChild(empty);
                }
            }

            window.AppDialog.open(previewDialog);
        });
    }
})();
