// Lớp chờ khi chuyển trang hoặc gửi form, và popup cho thông báo có sẵn.
(function () {
    var reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var loader = document.createElement('div');
    loader.className = 'page-loader';
    loader.hidden = true;
    loader.setAttribute('role', 'status');
    loader.setAttribute('aria-live', 'polite');
    loader.innerHTML = '<div class="page-loader-card"><span class="page-loader-spin" aria-hidden="true"></span><span>Đang tải</span></div>';
    document.body.appendChild(loader);

    function showLoader() {
        loader.hidden = false;
    }

    function hideLoader() {
        loader.hidden = true;
    }

    function isModifiedClick(event) {
        return event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey;
    }

    document.addEventListener('click', function (event) {
        if (event.defaultPrevented || isModifiedClick(event)) {
            return;
        }
        var link = event.target.closest('a[href]');
        if (!link || (link.target && link.target !== '_self') || link.hasAttribute('download')) {
            return;
        }
        var href = link.getAttribute('href');
        if (!href || href.charAt(0) === '#' || href.indexOf('javascript:') === 0 || href.indexOf('mailto:') === 0 || href.indexOf('tel:') === 0) {
            return;
        }
        var url;
        try {
            url = new URL(link.href, window.location.href);
        } catch (error) {
            return;
        }
        if (url.origin !== window.location.origin) {
            return;
        }
        if (url.pathname === window.location.pathname && url.search === window.location.search) {
            return;
        }
        showLoader();
    });

    document.addEventListener('submit', function (event) {
        var form = event.target;
        if (event.defaultPrevented || !form || form.nodeName !== 'FORM') {
            return;
        }
        if (form.target && form.target !== '_self') {
            return;
        }
        showLoader();
    });

    window.addEventListener('pageshow', hideLoader);

    var notices = document.querySelectorAll('.admin-alert, .admin-alert-error');
    if (notices.length) {
        var stack = document.createElement('div');
        stack.className = 'notice-stack';
        stack.setAttribute('aria-live', 'polite');
        document.body.appendChild(stack);
        notices.forEach(function (notice) {
            var close = document.createElement('button');
            close.type = 'button';
            close.className = 'notice-toast-close';
            close.setAttribute('aria-label', 'Đóng thông báo');
            close.textContent = '\u00d7';
            close.addEventListener('click', function () {
                dismissNotice(notice);
            });
            notice.classList.add('notice-toast');
            notice.appendChild(close);
            stack.appendChild(notice);
            if (!notice.classList.contains('admin-alert-error')) {
                window.setTimeout(function () {
                    dismissNotice(notice);
                }, 5000);
            }
        });
    }

    function dismissNotice(notice) {
        if (!notice.isConnected || notice.classList.contains('is-leaving')) {
            return;
        }
        notice.classList.add('is-leaving');
        window.setTimeout(function () {
            notice.remove();
        }, reduceMotion ? 0 : 200);
    }

    window.PageLoader = {
        show: showLoader,
        hide: hideLoader
    };
})();
