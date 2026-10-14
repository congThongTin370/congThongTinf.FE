// Hộp thoại dùng chung thay cho prompt/alert/confirm và <dialog> mặc định.
// Dùng: AppDialog.prompt({ title, label, placeholder, value, validate }).then(v => ...)
//       AppDialog.alert({ title, message }).then(() => ...)
//       AppDialog.confirm({ title, message, danger }).then(ok => ...)
//       AppDialog.open(element) / AppDialog.close(element) để mở/đóng hộp thoại có sẵn trong HTML.
(function () {
    var openStack = [];
    var motionMs = 240;

    function reducedMotion() {
        return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }

    function present(root) {
        root._dialogToken = (root._dialogToken || 0) + 1;
        root.classList.remove('is-leaving');
        root.classList.add('is-present', 'is-open');
    }

    function dismiss(root, done) {
        var token = (root._dialogToken || 0) + 1;
        root._dialogToken = token;
        root.classList.remove('is-open');
        root.classList.add('is-leaving');

        function settle() {
            if (root._dialogToken !== token) return;
            root._dialogToken = token + 1;
            root.classList.remove('is-leaving', 'is-present');
            if (done) done();
        }

        if (reducedMotion()) {
            settle();
            return;
        }

        var panel = root.querySelector('.app-dialog-panel');
        function onEnd(event) {
            if (event.target !== panel) return;
            panel.removeEventListener('animationend', onEnd);
            settle();
        }
        if (panel) panel.addEventListener('animationend', onEnd);
        window.setTimeout(settle, motionMs + 80);
    }

    function esc(text) {
        var div = document.createElement('div');
        div.textContent = text == null ? '' : String(text);
        return div.innerHTML;
    }

    function build(config) {
        var root = document.createElement('div');
        root.className = 'app-dialog';
        root.innerHTML =
            '<div class="app-dialog-backdrop" data-dialog-dismiss></div>' +
            '<div class="app-dialog-panel" role="dialog" aria-modal="true" aria-labelledby="app-dialog-title">' +
                '<div class="app-dialog-header">' +
                    '<h2 id="app-dialog-title" class="app-dialog-title">' + esc(config.title) + '</h2>' +
                    '<button type="button" class="app-dialog-close" data-dialog-dismiss aria-label="Đóng">&times;</button>' +
                '</div>' +
                '<form class="app-dialog-form" novalidate>' +
                    '<div class="app-dialog-body">' +
                        (config.message ? '<p class="app-dialog-message">' + esc(config.message) + '</p>' : '') +
                        (config.input
                            ? '<label class="app-dialog-label" for="app-dialog-input">' + esc(config.label || '') + '</label>' +
                              '<input id="app-dialog-input" class="form-control app-dialog-input" type="' + (config.inputType || 'text') + '"' +
                              ' placeholder="' + esc(config.placeholder || '') + '" value="' + esc(config.value || '') + '" />' +
                              '<p class="app-dialog-error" data-dialog-error hidden></p>'
                            : '') +
                    '</div>' +
                    '<div class="app-dialog-footer">' +
                        (config.cancelText !== null
                            ? '<button type="button" class="btn btn-outline-secondary" data-dialog-cancel>' + esc(config.cancelText || 'Hủy') + '</button>'
                            : '') +
                        '<button type="submit" class="btn ' + (config.danger ? 'btn-danger' : 'btn-primary') + '">' +
                            esc(config.okText || 'Đồng ý') +
                        '</button>' +
                    '</div>' +
                '</form>' +
            '</div>';
        return root;
    }

    function show(config) {
        return new Promise(function (resolve) {
            var root = build(config);
            var form = root.querySelector('form');
            var input = root.querySelector('input');
            var errorEl = root.querySelector('[data-dialog-error]');
            var previouslyFocused = document.activeElement;
            var finished = false;

            function finish(result) {
                if (finished) return;
                finished = true;
                document.removeEventListener('keydown', onKey, true);
                dismiss(root, function () {
                    root.remove();
                    if (previouslyFocused && previouslyFocused.focus) previouslyFocused.focus();
                });
                resolve(result);
            }

            function onKey(e) {
                if (e.key === 'Escape') {
                    e.preventDefault();
                    finish(config.input ? null : false);
                }
            }

            root.addEventListener('click', function (e) {
                if (e.target.hasAttribute('data-dialog-dismiss') || e.target.hasAttribute('data-dialog-cancel')) {
                    finish(config.input ? null : false);
                }
            });

            form.addEventListener('submit', function (e) {
                e.preventDefault();
                if (input) {
                    var value = input.value.trim();
                    var error = config.validate ? config.validate(value) : (value ? '' : (config.required === false ? '' : 'Vui lòng nhập nội dung.'));
                    if (error) {
                        errorEl.textContent = error;
                        errorEl.hidden = false;
                        input.classList.add('is-invalid');
                        input.focus();
                        return;
                    }
                    finish(value);
                } else {
                    finish(true);
                }
            });

            if (input) {
                input.addEventListener('input', function () {
                    errorEl.hidden = true;
                    input.classList.remove('is-invalid');
                });
            }

            document.addEventListener('keydown', onKey, true);
            document.body.appendChild(root);
            present(root);
            (input || form.querySelector('button[type="submit"]')).focus();
            if (input) input.select();
        });
    }

    // Hộp thoại có sẵn trong HTML (<div class="app-dialog-panel">...)
    function open(el) {
        if (!el) return;
        if (el.classList.contains('is-open')) return;
        openStack.push(document.activeElement);
        present(el);
        var focusable = el.querySelector('input, select, textarea, button[type="submit"]');
        if (focusable) focusable.focus();
    }

    function close(el) {
        if (!el || !el.classList.contains('is-open')) return;
        var prev = openStack.pop();
        dismiss(el, function () {
            if (prev && prev.focus) prev.focus();
        });
    }

    document.addEventListener('click', function (e) {
        var target = e.target.closest('[data-dialog-open]');
        if (target) {
            open(document.getElementById(target.getAttribute('data-dialog-open')));
            return;
        }
        var closeBtn = e.target.closest('[data-dialog-close]');
        if (closeBtn) {
            close(closeBtn.closest('.app-dialog'));
            return;
        }
        // Chỉ xử lý hộp thoại có sẵn trong HTML; hộp thoại tạo bằng show() tự đóng qua listener riêng
        var dismiss = e.target.closest('[data-dialog-dismiss]');
        if (dismiss && dismiss.closest('.app-dialog[data-dialog-root]')) {
            close(dismiss.closest('.app-dialog'));
        }
    });

    window.AppDialog = {
        show: show,
        open: open,
        close: close,
        prompt: function (opts) {
            return show(Object.assign({ input: true, label: opts.title, okText: 'Đồng ý' }, opts));
        },
        alert: function (opts) {
            return show(Object.assign({ input: false, cancelText: null, okText: 'Đã hiểu' }, opts));
        },
        confirm: function (opts) {
            return show(Object.assign({ input: false, okText: 'Đồng ý' }, opts));
        }
    };
})();
