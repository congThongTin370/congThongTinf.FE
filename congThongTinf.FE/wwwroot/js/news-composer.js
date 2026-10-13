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

    var contentEditor = form.querySelector('[data-content-editor]');
    var antiForgery = form.querySelector('input[name="__RequestVerificationToken"]');
    var previewUrl = previewOpen ? previewOpen.getAttribute('data-preview-url') : '';

    if (previewOpen) {
        previewOpen.addEventListener('click', function () {
            var editorHost = form.querySelector('[data-rich-editor]');
            if (editorHost && typeof editorHost.syncContent === 'function') {
                editorHost.syncContent();
            }

            var title = titleInput && titleInput.value.trim() ? titleInput.value.trim() : 'Chưa có tiêu đề';
            var category = selectedText('[name="CategoryId"]');
            var type = selectedText('[data-news-type]');
            var kicker = [category, type].filter(Boolean).join(' · ');
            var summaryInput = form.querySelector('[name="Summary"]');
            var summary = summaryInput ? summaryInput.value.trim() : '';
            var text = contentEditor ? contentEditor.value.trim() : '';

            var kickerEl = form.querySelector('[data-preview-kicker]');
            var titleEl = form.querySelector('[data-preview-title]');
            var summaryEl = form.querySelector('[data-preview-summary]');
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
            if (summaryEl) {
                summaryEl.textContent = summary;
                summaryEl.hidden = !summary;
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
                body.textContent = 'Đang tải xem trước...';
            }

            var previewData = new FormData();
            previewData.append('content', text);
            if (antiForgery) {
                previewData.append('__RequestVerificationToken', antiForgery.value);
            }

            window.AppDialog.open(previewDialog);

            // Render phía server (Markdown -> HTML đã làm sạch) để xem trước đúng như bài đã đăng, gồm cả ảnh
            fetch(previewUrl, { method: 'POST', body: previewData, credentials: 'same-origin' })
                .then(function (response) {
                    return response.json();
                })
                .then(function (result) {
                    if (!body) {
                        return;
                    }
                    if (result && result.success && result.html) {
                        body.innerHTML = result.html;
                    } else if (text) {
                        body.textContent = 'Chưa xem trước được nội dung.';
                    } else {
                        body.innerHTML = '<p class="composer-preview-empty">Chưa có nội dung.</p>';
                    }
                })
                .catch(function () {
                    if (body) {
                        body.textContent = 'Chưa xem trước được nội dung.';
                    }
                });
        });
    }
})();

