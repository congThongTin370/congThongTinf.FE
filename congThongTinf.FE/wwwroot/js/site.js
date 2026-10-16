// Dropdown chuyên mục trên thanh điều hướng: bật/tắt bằng nút mũi tên (cho mobile và bàn phím).
(function () {
    document.querySelectorAll('.nav-dropdown-toggle').forEach(function (toggle) {
        var dropdown = toggle.closest('.nav-dropdown');

        toggle.addEventListener('click', function (e) {
            e.stopPropagation();
            var isOpen = dropdown.classList.toggle('is-open');
            toggle.setAttribute('aria-expanded', String(isOpen));
        });
    });

    // Slider 5 bài mới nhất: tự chuyển, trượt bằng transform để vòng bài mượt.
    var slider = document.querySelector('.home-slider');
    if (slider && !slider.classList.contains('home-slider-single')) {
        var viewport = slider.querySelector('.home-slider-viewport');
        var track = slider.querySelector('.home-slider-track');
        var dots = slider.querySelectorAll('.home-slider-dot');
        var realSlides = Array.prototype.slice.call(track.children);
        var count = realSlides.length;
        var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        var delay = 3000;
        var index = 1;
        var timer = 0;
        var dragX = 0;
        var dragStart = 0;
        var dragging = false;

        var lastClone = realSlides[count - 1].cloneNode(true);
        var firstClone = realSlides[0].cloneNode(true);
        lastClone.setAttribute('aria-hidden', 'true');
        firstClone.setAttribute('aria-hidden', 'true');
        track.insertBefore(lastClone, realSlides[0]);
        track.appendChild(firstClone);

        function realIndex() {
            if (index === 0) {
                return count - 1;
            }
            if (index === count + 1) {
                return 0;
            }
            return index - 1;
        }

        function render(animate, extraPx) {
            var move = reduceMotion ? false : animate;
            track.classList.toggle('is-instant', !move);
            var shift = extraPx ? ' + ' + extraPx + 'px' : '';
            track.style.transform = 'translate3d(calc(' + (-index * 100) + '%' + shift + '), 0, 0)';
            var current = realIndex();
            dots.forEach(function (dot, i) {
                var active = i === current;
                dot.classList.toggle('is-active', active);
                dot.setAttribute('aria-current', active ? 'true' : 'false');
            });
        }

        var blockClick = false;
        var hovering = false;

        function settle() {
            if (index === 0) {
                index = count;
                render(false);
            } else if (index === count + 1) {
                index = 1;
                render(false);
            }
        }

        function move(delta) {
            if (index === 0 || index === count + 1) {
                settle();
                void track.offsetWidth;
            } else if (track.classList.contains('is-instant')) {
                void track.offsetWidth;
            }
            index += delta;
            render(true);
        }

        function show(real) {
            var target = real + 1;
            if (index === 0 || index === count + 1) {
                settle();
                void track.offsetWidth;
            }
            if (target === index) {
                return;
            }
            if (track.classList.contains('is-instant')) {
                void track.offsetWidth;
            }
            index = target;
            render(true);
        }

        function held() {
            return dragging || document.hidden || reduceMotion || hovering;
        }

        function start() {
            window.clearInterval(timer);
            if (held()) {
                return;
            }
            timer = window.setInterval(function () {
                move(1);
            }, delay);
        }

        function stop() {
            window.clearInterval(timer);
        }

        render(false);

        track.addEventListener('transitionend', function (e) {
            if (e.target !== track || e.propertyName !== 'transform') {
                return;
            }
            settle();
        });

        slider.querySelector('.home-slider-prev').addEventListener('click', function () {
            move(-1);
            start();
        });
        slider.querySelector('.home-slider-next').addEventListener('click', function () {
            move(1);
            start();
        });
        dots.forEach(function (dot, i) {
            dot.addEventListener('click', function () {
                show(i);
                start();
            });
        });

        viewport.addEventListener('keydown', function (e) {
            if (e.key === 'ArrowLeft') {
                e.preventDefault();
                move(-1);
                start();
            } else if (e.key === 'ArrowRight') {
                e.preventDefault();
                move(1);
                start();
            }
        });

        viewport.addEventListener('pointerdown', function (e) {
            if (e.pointerType === 'mouse' && e.button !== 0) {
                return;
            }
            dragging = true;
            blockClick = false;
            dragStart = e.clientX;
            dragX = 0;
            stop();
            viewport.setPointerCapture(e.pointerId);
        });

        viewport.addEventListener('pointermove', function (e) {
            if (!dragging) {
                return;
            }
            dragX = e.clientX - dragStart;
            render(false, dragX);
        });

        function endDrag(e) {
            if (!dragging) {
                return;
            }
            dragging = false;
            if (viewport.hasPointerCapture(e.pointerId)) {
                viewport.releasePointerCapture(e.pointerId);
            }
            var threshold = Math.max(36, viewport.clientWidth * 0.12);
            blockClick = Math.abs(dragX) > 8;
            if (dragX <= -threshold) {
                move(1);
            } else if (dragX >= threshold) {
                move(-1);
            } else if (track.classList.contains('is-instant')) {
                void track.offsetWidth;
                render(true);
            }
            start();
        }

        viewport.addEventListener('pointerup', endDrag);
        viewport.addEventListener('pointercancel', endDrag);

        viewport.addEventListener('click', function (e) {
            if (!blockClick) {
                return;
            }
            blockClick = false;
            e.preventDefault();
            e.stopPropagation();
        }, true);

        slider.addEventListener('pointerenter', function (e) {
            if (e.pointerType !== 'mouse') {
                return;
            }
            hovering = true;
            stop();
        });
        slider.addEventListener('pointerleave', function (e) {
            if (e.pointerType !== 'mouse') {
                return;
            }
            hovering = false;
            start();
        });
        slider.addEventListener('focusin', stop);
        slider.addEventListener('focusout', function (e) {
            if (!slider.contains(e.relatedTarget)) {
                start();
            }
        });
        document.addEventListener('visibilitychange', function () {
            if (document.hidden) {
                stop();
            } else {
                start();
            }
        });

        start();
    }

    // Bấm ra ngoài thì đóng dropdown
    document.addEventListener('click', function (e) {
        document.querySelectorAll('.nav-dropdown.is-open').forEach(function (dropdown) {
            if (!dropdown.contains(e.target)) {
                dropdown.classList.remove('is-open');
                dropdown.querySelector('.nav-dropdown-toggle').setAttribute('aria-expanded', 'false');
            }
        });
    });
})();
