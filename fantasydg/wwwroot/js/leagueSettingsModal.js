function openTournamentModal() {
    const modal = document.getElementById("tournamentInputModal");
    modal.classList.add("show");
    document.body.classList.add("modal-open");
}

function closeTournamentModal() {
    const modal = document.getElementById("tournamentInputModal");
    modal.classList.remove("show");
    document.body.classList.remove("modal-open");
}

function showToast(message, isSuccess = true) {
    const toastId = `toast_${Date.now()}`;
    const toast = document.createElement("div");

    toast.className = `toast align-items-center text-white bg-${isSuccess ? 'success' : 'danger'} border-0`;
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");
    toast.setAttribute("id", toastId);

    toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;

    const container = document.getElementById("toastContainer");
    container.appendChild(toast);

    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();

    // Auto-remove after shown
    toast.addEventListener('hidden.bs.toast', () => toast.remove());
}

// Clicking outside modal-dialog closes the modal
document.addEventListener("DOMContentLoaded", function () {
    const modal = document.getElementById("tournamentInputModal");

    modal.addEventListener("click", function (e) {
        // Only close if the backdrop is clicked (not dialog or form)
        if (e.target === modal) {
            closeTournamentModal();
        }
    });

    const container = document.getElementById("toastContainer");
    const json = document.getElementById("server-toast-data");
    if (!container || !json) return;

    const toasts = JSON.parse(json.textContent || "[]");

    for (const t of toasts) {
        const bgMap = {
            success: "bg-success",
            danger: "bg-danger",
            warning: "bg-warning text-dark",
            info: "bg-info text-dark"
        };
        const bgClass = bgMap[t.type] || "bg-secondary";

        const toastId = "toast-" + Date.now() + Math.random();
        const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0 mb-2"
                     role="alert" aria-live="assertive" aria-atomic="true">
                    <div class="d-flex">
                        <div class="toast-body">${t.text}</div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
                    </div>
                </div>
            `;

        container.insertAdjacentHTML("beforeend", toastHtml);

        const toastElem = document.getElementById(toastId);
        const toast = new bootstrap.Toast(toastElem, { delay: 4000 });
        toast.show();
    }

    document.querySelectorAll("form").forEach(form => {
        // Skip if it has data-no-ajax (for exceptions)
        if (form.dataset.noAjax !== undefined) return;

        form.addEventListener("submit", async function (e) {
            e.preventDefault();
            e.stopPropagation();

            const formData = new FormData(form);
            const action = form.getAttribute("action") || window.location.href;
            const method = form.getAttribute("method")?.toUpperCase() || "POST";

            const tokenInput = form.querySelector('input[name="__RequestVerificationToken"]');
            const headers = tokenInput
                ? { 'RequestVerificationToken': tokenInput.value }
                : {};

            const response = await fetch(action, {
                method,
                body: formData,
                headers
            });

            if (response.ok) {
                showToast("Saved successfully!", true);
            } else {
                showToast("Failed to save. Please try again.", false);
            }
        });
    });

    const tournamentForm = document.getElementById("tournamentInputForm");
    const spinner = document.getElementById("tournamentLoadingSpinner");

    if (tournamentForm && spinner) {
        tournamentForm.addEventListener("submit", async function (e) {
            e.preventDefault();
            e.stopPropagation();

            spinner.classList.remove("d-none");

            const formData = new FormData(tournamentForm);
            const action = tournamentForm.getAttribute("action");
            const token = tournamentForm.querySelector('input[name="__RequestVerificationToken"]')?.value;

            try {
                const response = await fetch(action, {
                    method: "POST",
                    body: formData,
                    headers: { 'RequestVerificationToken': token }
                });

                spinner.classList.add("d-none");

                if (response.ok) {
                    showToast("Tournament added successfully!", true);
                    closeTournamentModal();

                    // Optional: manually insert new tournament into UI here
                    // if you're not refreshing the whole page.
                } else {
                    const errorText = await response.text();
                    console.error("Failed to add tournament:", errorText);
                    showToast("Failed to add tournament.", false);
                }
            } catch (err) {
                console.error("Fetch error:", err);
                spinner.classList.add("d-none");
                showToast("Error occurred. Please try again.", false);
            }
        });
    }
});