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
});