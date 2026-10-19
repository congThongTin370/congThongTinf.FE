(function () {
    var zones = document.querySelectorAll("[data-banner-zone]");
    var hint = document.querySelector("[data-banner-hint]");
    var pools = document.querySelectorAll("[data-banner-pool]");
    if (zones.length === 0) {
        return;
    }

    function apply(position) {
        zones.forEach(function (zone) {
            var on = zone.getAttribute("data-banner-zone") === String(position);
            zone.classList.toggle("is-selected", on);
            zone.setAttribute("aria-pressed", on ? "true" : "false");
        });

        pools.forEach(function (pool) {
            pool.hidden = pool.getAttribute("data-banner-pool") !== String(position);
        });

        if (hint) {
            hint.hidden = true;
        }
    }

    zones.forEach(function (zone) {
        zone.addEventListener("click", function () {
            apply(zone.getAttribute("data-banner-zone"));
        });
    });

    var selected = document.querySelector(".banner-zone.is-selected");
    if (selected) {
        apply(selected.getAttribute("data-banner-zone"));
    }

    document.querySelectorAll("[data-banner-search]").forEach(function (search) {
        search.addEventListener("input", function () {
            var term = search.value.trim().toLowerCase();
            var pool = search.closest("[data-banner-pool]");
            if (!pool) {
                return;
            }

            pool.querySelectorAll(".banner-seat[data-title]").forEach(function (card) {
                var title = (card.getAttribute("data-title") || "").toLowerCase();
                card.hidden = term.length > 0 && title.indexOf(term) === -1;
            });
        });
    });
})();
