(function () {
    var zones = document.querySelectorAll("[data-banner-zone]");
    var hint = document.querySelector("[data-banner-hint]");
    if (zones.length === 0) {
        return;
    }

    function apply(position) {
        zones.forEach(function (zone) {
            var on = zone.getAttribute("data-banner-zone") === String(position);
            zone.classList.toggle("is-selected", on);
            zone.setAttribute("aria-pressed", on ? "true" : "false");
        });

        var selectedZone = document.querySelector(".banner-zone.is-selected");
        var activeUrl = selectedZone ? selectedZone.getAttribute("data-active-url") || "" : "";

        document.querySelectorAll("[data-banner-position]").forEach(function (input) {
            input.value = position;
        });

        document.querySelectorAll("[data-banner-pick]").forEach(function (button) {
            var card = button.closest(".banner-seat");
            var url = card ? card.getAttribute("data-url") || "" : "";
            var current = activeUrl.length > 0 && url === activeUrl;
            var label = button.querySelector("[data-banner-pick-label]");
            button.disabled = current;
            if (label) {
                label.textContent = current ? "Đang gắn ở ô này" : "Dùng cho ô này";
            }
            if (card) {
                card.classList.toggle("is-live", current);
            }
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

    var search = document.querySelector("[data-banner-search]");
    if (search) {
        search.addEventListener("input", function () {
            var term = search.value.trim().toLowerCase();
            document.querySelectorAll(".banner-seat[data-title]").forEach(function (card) {
                var title = (card.getAttribute("data-title") || "").toLowerCase();
                card.hidden = term.length > 0 && title.indexOf(term) === -1;
            });
        });
    }
})();
