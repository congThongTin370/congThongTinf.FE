// Xem ảnh lớn trên trang chi tiết tin: phóng to, thu nhỏ, kéo và chuyển ảnh.
(function () {
    var article = document.querySelector('.news-detail');
    if (!article) {
        return;
    }

    var sources = Array.prototype.filter.call(
        article.querySelectorAll('.news-detail-cover img, .news-detail-body img'),
        function (img) {
            return img.getAttribute('src');
        }
    );
    if (!sources.length) {
        return;
    }

    sources.forEach(function (img) {
        img.setAttribute('tabindex', '0');
        img.setAttribute('role', 'button');
        if (!img.getAttribute('aria-label')) {
            img.setAttribute('aria-label', img.alt ? 'Xem lớn: ' + img.alt : 'Xem ảnh lớn');
        }
    });

    var root = document.createElement('div');
    root.className = 'lightbox';
    root.hidden = true;
    root.innerHTML =
        '<div class="lightbox-backdrop" data-lightbox-close></div>' +
        '<div class="lightbox-dialog" role="dialog" aria-modal="true" aria-label="Xem ảnh">' +
            '<button type="button" class="lightbox-close" data-lightbox-close aria-label="Đóng">&times;</button>' +
            '<button type="button" class="lightbox-nav lightbox-prev" data-lightbox-prev aria-label="Ảnh trước">&#8249;</button>' +
            '<button type="button" class="lightbox-nav lightbox-next" data-lightbox-next aria-label="Ảnh sau">&#8250;</button>' +
            '<div class="lightbox-viewport" data-lightbox-view>' +
                '<img class="lightbox-image" alt="" draggable="false" />' +
            '</div>' +
            '<div class="lightbox-bar">' +
                '<span class="lightbox-count" data-lightbox-count></span>' +
                '<div class="lightbox-tools">' +
                    '<button type="button" data-lightbox-out aria-label="Thu nhỏ">−</button>' +
                    '<span data-lightbox-scale>100%</span>' +
                    '<button type="button" data-lightbox-in aria-label="Phóng to">+</button>' +
                    '<button type="button" data-lightbox-reset>Vừa khung</button>' +
                '</div>' +
            '</div>' +
        '</div>';
    document.body.appendChild(root);

    var viewport = root.querySelector('[data-lightbox-view]');
    var image = root.querySelector('.lightbox-image');
    var countEl = root.querySelector('[data-lightbox-count]');
    var scaleEl = root.querySelector('[data-lightbox-scale]');
    var prevBtn = root.querySelector('[data-lightbox-prev]');
    var nextBtn = root.querySelector('[data-lightbox-next]');
    var index = 0;
    var scale = 1;
    var panX = 0;
    var panY = 0;
    var dragging = false;
    var pointerStart = null;
    var pointers = {};
    var pinchStart = null;
    var lastFocus = null;
    var minScale = 1;
    var maxScale = 4;

    function apply() {
        image.style.transform = 'translate(' + panX + 'px, ' + panY + 'px) scale(' + scale + ')';
        scaleEl.textContent = Math.round(scale * 100) + '%';
        viewport.classList.toggle('is-zoomed', scale > 1);
    }

    function resetZoom() {
        scale = 1;
        panX = 0;
        panY = 0;
        apply();
    }

    function show(nextIndex) {
        index = (nextIndex + sources.length) % sources.length;
        var source = sources[index];
        image.src = source.currentSrc || source.src;
        image.alt = source.alt || '';
        countEl.textContent = sources.length > 1 ? (index + 1) + ' / ' + sources.length : '';
        prevBtn.hidden = sources.length < 2;
        nextBtn.hidden = sources.length < 2;
        resetZoom();
    }

    function open(start) {
        lastFocus = document.activeElement;
        show(start);
        root.hidden = false;
        document.body.classList.add('lightbox-open');
        root.querySelector('.lightbox-close').focus();
    }

    function close() {
        root.hidden = true;
        document.body.classList.remove('lightbox-open');
        image.removeAttribute('src');
        if (lastFocus && lastFocus.focus) {
            lastFocus.focus();
        }
    }

    function zoomBy(factor, clientX, clientY) {
        var rect = viewport.getBoundingClientRect();
        var cx = clientX - rect.left - rect.width / 2;
        var cy = clientY - rect.top - rect.height / 2;
        var next = Math.min(maxScale, Math.max(minScale, scale * factor));
        if (next === scale) {
            return;
        }
        var pointX = (cx - panX) / scale;
        var pointY = (cy - panY) / scale;
        scale = next;
        if (scale === 1) {
            panX = 0;
            panY = 0;
        } else {
            panX = cx - pointX * scale;
            panY = cy - pointY * scale;
        }
        apply();
    }

    function zoomCenter(factor) {
        var rect = viewport.getBoundingClientRect();
        zoomBy(factor, rect.left + rect.width / 2, rect.top + rect.height / 2);
    }

    article.addEventListener('click', function (event) {
        var img = event.target.closest('.news-detail-cover img, .news-detail-body img');
        if (!img || sources.indexOf(img) < 0) {
            return;
        }
        event.preventDefault();
        event.stopPropagation();
        open(sources.indexOf(img));
    });

    article.addEventListener('keydown', function (event) {
        if (event.key !== 'Enter' && event.key !== ' ') {
            return;
        }
        var img = event.target.closest('.news-detail-cover img, .news-detail-body img');
        if (!img || sources.indexOf(img) < 0) {
            return;
        }
        event.preventDefault();
        open(sources.indexOf(img));
    });

    root.addEventListener('click', function (event) {
        if (event.target.closest('[data-lightbox-close]')) {
            close();
        } else if (event.target.closest('[data-lightbox-prev]')) {
            show(index - 1);
        } else if (event.target.closest('[data-lightbox-next]')) {
            show(index + 1);
        } else if (event.target.closest('[data-lightbox-in]')) {
            zoomCenter(1.25);
        } else if (event.target.closest('[data-lightbox-out]')) {
            zoomCenter(1 / 1.25);
        } else if (event.target.closest('[data-lightbox-reset]')) {
            resetZoom();
        }
    });

    document.addEventListener('keydown', function (event) {
        if (root.hidden) {
            return;
        }
        if (event.key === 'Escape') {
            event.preventDefault();
            close();
        } else if (event.key === 'ArrowLeft') {
            show(index - 1);
        } else if (event.key === 'ArrowRight') {
            show(index + 1);
        } else if (event.key === '+' || event.key === '=') {
            zoomCenter(1.25);
        } else if (event.key === '-' || event.key === '_') {
            zoomCenter(1 / 1.25);
        } else if (event.key === '0') {
            resetZoom();
        }
    });

    viewport.addEventListener('wheel', function (event) {
        if (root.hidden) {
            return;
        }
        event.preventDefault();
        zoomBy(event.deltaY < 0 ? 1.12 : 1 / 1.12, event.clientX, event.clientY);
    }, { passive: false });

    viewport.addEventListener('dblclick', function (event) {
        if (scale > 1) {
            resetZoom();
        } else {
            zoomBy(2, event.clientX, event.clientY);
        }
    });

    function pointerDistance() {
        var ids = Object.keys(pointers);
        if (ids.length < 2) {
            return 0;
        }
        var a = pointers[ids[0]];
        var b = pointers[ids[1]];
        var dx = a.x - b.x;
        var dy = a.y - b.y;
        return Math.sqrt(dx * dx + dy * dy);
    }

    viewport.addEventListener('pointerdown', function (event) {
        pointers[event.pointerId] = { x: event.clientX, y: event.clientY };
        viewport.setPointerCapture(event.pointerId);
        if (Object.keys(pointers).length === 2) {
            pinchStart = { distance: pointerDistance(), scale: scale };
            dragging = false;
            return;
        }
        if (scale <= 1) {
            return;
        }
        dragging = true;
        pointerStart = { x: event.clientX, y: event.clientY, panX: panX, panY: panY };
        viewport.classList.add('is-dragging');
    });

    viewport.addEventListener('pointermove', function (event) {
        if (!pointers[event.pointerId]) {
            return;
        }
        pointers[event.pointerId] = { x: event.clientX, y: event.clientY };
        if (pinchStart && Object.keys(pointers).length >= 2) {
            var distance = pointerDistance();
            if (!pinchStart.distance) {
                return;
            }
            scale = Math.min(maxScale, Math.max(minScale, pinchStart.scale * (distance / pinchStart.distance)));
            if (scale === 1) {
                panX = 0;
                panY = 0;
            }
            apply();
            return;
        }
        if (!dragging || !pointerStart) {
            return;
        }
        var dx = event.clientX - pointerStart.x;
        var dy = event.clientY - pointerStart.y;
        panX = pointerStart.panX + dx;
        panY = pointerStart.panY + dy;
        apply();
    });

    function endPointer(event) {
        delete pointers[event.pointerId];
        if (Object.keys(pointers).length < 2) {
            pinchStart = null;
        }
        dragging = false;
        pointerStart = null;
        viewport.classList.remove('is-dragging');
    }

    viewport.addEventListener('pointerup', endPointer);
    viewport.addEventListener('pointercancel', endPointer);
})();
