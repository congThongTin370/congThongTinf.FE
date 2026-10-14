(function () {
    var zones = document.querySelectorAll("[data-banner-zone]");
    var pools = document.querySelectorAll("[data-banner-pool]");
    var hint = document.querySelector("[data-banner-hint]");
    if (zones.length === 0) {
        return;
    }

    function select(position) {
        var current = document.querySelector(".banner-zone.is-selected");
        var same = current && current.getAttribute("data-banner-zone") === String(position);

        zones.forEach(function (zone) {
            var on = !same && zone.getAttribute("data-banner-zone") === String(position);
            zone.classList.toggle("is-selected", on);
            zone.setAttribute("aria-pressed", on ? "true" : "false");
        });

        pools.forEach(function (pool) {
            pool.hidden = same || pool.getAttribute("data-banner-pool") !== String(position);
        });

        if (hint) {
            hint.hidden = !same;
        }
    }

    zones.forEach(function (zone) {
        zone.addEventListener("click", function () {
            select(zone.getAttribute("data-banner-zone"));
        });
    });

    document.querySelectorAll("[data-banner-search]").forEach(function (input) {
        input.addEventListener("input", function () {
            var pool = input.closest("[data-banner-pool]");
            if (!pool) {
                return;
            }

            var term = input.value.trim().toLowerCase();
            pool.querySelectorAll(".banner-seat").forEach(function (card) {
                var title = (card.getAttribute("data-title") || "").toLowerCase();
                card.hidden = term.length > 0 && title.indexOf(term) === -1;
            });
        });
    });
})();
