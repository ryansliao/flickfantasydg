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
});