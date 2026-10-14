(function () {
    var switches = document.querySelectorAll(".ios-switch input");
    if (switches.length === 0) {
        return;
    }

    var requestId = 0;
    var syncing = false;

    function showError(message) {
        var existing = document.querySelector(".admin-alert-error[data-banner-switch]");
        if (!existing) {
            existing = document.createElement("div");
            existing.className = "admin-alert-error";
            existing.setAttribute("data-banner-switch", "");
            existing.setAttribute("role", "alert");
            var list = document.getElementById("banner-list-title");
            var panel = list ? list.closest("section") : null;
            if (panel && panel.parentNode) {
                panel.parentNode.insertBefore(existing, panel);
            }
        }

        existing.textContent = message;
    }

    switches.forEach(function (input) {
        input.addEventListener("change", function () {
            if (syncing) {
                return;
            }

            if (!input.checked) {
                input.checked = true;
                return;
            }

            var previous = null;
            syncing = true;
            switches.forEach(function (other) {
                if (other !== input && other.checked) {
                    previous = other;
                    other.checked = false;
                }
            });
            syncing = false;

            var current = ++requestId;
            var form = input.form;
            fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                headers: { "X-Requested-With": "fetch" },
                credentials: "same-origin"
            }).then(function (response) {
                if (response.status === 401) {
                    window.location.assign(form.getAttribute("data-login-url") || "/Account/Login");
                    return null;
                }

                if (!response.ok) {
                    return response.json().catch(function () {
                        return {};
                    }).then(function (data) {
                        throw new Error(data.message || "Chưa thực hiện được. Vui lòng thử lại sau.");
                    });
                }

                return null;
            }).catch(function (error) {
                if (current !== requestId) {
                    return;
                }

                syncing = true;
                input.checked = false;
                if (previous) {
                    previous.checked = true;
                }
                syncing = false;

                showError(error.message || "Chưa thực hiện được. Vui lòng thử lại sau.");
            });
        });
    });
})();
