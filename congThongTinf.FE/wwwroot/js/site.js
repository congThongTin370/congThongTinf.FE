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
