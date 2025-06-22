document.addEventListener("DOMContentLoaded", function () {
    const isOwnTeam = JSON.parse(document.body.dataset.isOwnTeam);
    const teamId = document.body.dataset.teamId;
    const token = document.body.dataset.requestVerificationToken;

    document.getElementById("mainContent").style.display = "block";

    if (!isOwnTeam) return;

    let mouseDownTime = null;

    const getClassForStatus = (status) => {
        const statusClasses = {
            "Starter": "starter-card",
            "Bench": "bench-card",
            "InjuryReserve": "ir-card"
        };
        return statusClasses[status] || "";
    };

    const updateStatus = (pdgaNumber, newStatus) => {
        fetch(`/Team/ChangeStatus/${pdgaNumber}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ newStatus })
        });
    };

    const setupDraggableCard = (card) => {
        card.addEventListener("dragstart", ev => {
            ev.dataTransfer.setData("playerId", card.dataset.id);
        });

        card.addEventListener("mousedown", () => {
            mouseDownTime = new Date().getTime();
        });

        card.addEventListener("mouseup", () => {
            const duration = new Date().getTime() - mouseDownTime;
            if (duration < 200) {
                showPlayerModal(card.dataset.name, card.dataset.id);
            }
        });
    };

    const setupDropSlot = (slot) => {
        slot.addEventListener("dragover", ev => ev.preventDefault());

        slot.addEventListener("drop", ev => {
            ev.preventDefault();
            const draggedId = ev.dataTransfer.getData("playerId");
            const draggedCard = document.querySelector(`[data-id='${draggedId}']`);
            const dropSlot = ev.currentTarget;
            const existingCard = dropSlot.querySelector('.player-card');

            const originalSlot = draggedCard.closest('.drop-slot');
            const fromStatus = originalSlot.dataset.status;
            const toStatus = dropSlot.dataset.status;

            if (existingCard && existingCard !== draggedCard) {
                originalSlot.appendChild(existingCard);
                dropSlot.appendChild(draggedCard);

                draggedCard.className = `player-card draggable-player ${getClassForStatus(toStatus)}`;
                existingCard.className = `player-card draggable-player ${getClassForStatus(fromStatus)}`;

                updateStatus(draggedId, toStatus);
                updateStatus(existingCard.dataset.id, fromStatus);
            } else if (originalSlot !== dropSlot) {
                dropSlot.appendChild(draggedCard);
                originalSlot.innerHTML = '';

                draggedCard.className = `player-card draggable-player ${getClassForStatus(toStatus)}`;
                updateStatus(draggedId, toStatus);
            }
        });
    };

    const showPlayerModal = (name, pdgaNumber) => {
        document.getElementById('playerModalName').textContent = name;
        document.getElementById('playerModalId').textContent = pdgaNumber;
        document.getElementById('modalPdgaNumber').value = pdgaNumber;
        document.getElementById('playerModal').style.display = 'block';
    };

    const closeModal = () => {
        document.getElementById('playerModal').style.display = 'none';
    };

    const openLockModal = () => {
        document.getElementById("lockModal").style.display = "block";
        loadStarterPreview();
        updateLockButtonLabel();
    };

    const closeLockModal = () => {
        document.getElementById("lockModal").style.display = "none";
    };

    const loadStarterPreview = () => {
        const dropdown = document.getElementById("lockTournamentDropdown");
        const selectedOption = dropdown.options[dropdown.selectedIndex];
        const tournamentId = selectedOption.value;
        const isLocked = selectedOption.getAttribute("data-locked") === "true";

        fetch(`/Team/GetStarterPreview?teamId=${teamId}&tournamentId=${tournamentId}&isLocked=${isLocked}`)
            .then(res => res.text())
            .then(html => {
                document.getElementById("starterPreview").innerHTML = html;
            });
    };

    const unlockRoster = () => {
        const tournamentId = document.getElementById("lockTournamentDropdown").value;
        const form = document.createElement("form");
        form.method = "post";
        form.action = "/Team/UnlockRoster";

        const inputs = [
            { name: "teamId", value: teamId },
            { name: "tournamentId", value: tournamentId },
            { name: "__RequestVerificationToken", value: token }
        ];

        inputs.forEach(({ name, value }) => {
            const input = document.createElement("input");
            input.type = "hidden";
            input.name = name;
            input.value = value;
            form.appendChild(input);
        });

        document.body.appendChild(form);
        form.submit();
    };

    const updateLockButtonLabel = () => {
        const dropdown = document.getElementById("lockTournamentDropdown");
        const selected = dropdown.options[dropdown.selectedIndex];
        const button = document.getElementById("lockButton");
        const isLocked = selected.getAttribute("data-locked") === "true";

        button.innerText = isLocked ? "Unlock Tournament" : "Confirm Lock";
        button.className = isLocked ? "btn btn-danger mt-3" : "btn btn-success mt-3";
    };

    document.querySelectorAll(".draggable-player").forEach(setupDraggableCard);
    document.querySelectorAll(".drop-slot").forEach(setupDropSlot);

    document.body.addEventListener("click", (event) => {
        if (event.target.closest('.dropdown-menu, .dropdown-toggle')) return;
        if (event.target.id === 'playerModal') closeModal();
    });

    window.closeModal = closeModal;
    window.openLockModal = openLockModal;
    window.closeLockModal = closeLockModal;
    window.loadStarterPreview = loadStarterPreview;
    window.unlockRoster = unlockRoster;
    window.updateLockButtonLabel = updateLockButtonLabel;

    const toastScript = document.getElementById("server-toast-data");
    if (!toastScript) {
        console.warn("No toast data block found.");
        return;
    }

    try {
        const messages = JSON.parse(toastScript.textContent);
        messages.forEach(m => showToast(m.text, m.type));
    } catch (err) {
        console.error("Toast parse error:", err);
    }

    window.showToast = showToast;

    document.querySelectorAll('.dropdown-toggle').forEach(function (toggle) {
        const dropdown = new bootstrap.Dropdown(toggle);
        toggle.addEventListener('click', function (e) {
            e.stopImmediatePropagation(); // force override
            e.preventDefault(); // prevent other handlers from stealing it
            dropdown.toggle();
        });
    });
});

function showToast(message, type = "success") {
    const toastId = `toast-${Date.now()}`;
    const bgMap = {
        success: "bg-success",
        warning: "bg-warning text-dark", 
        danger: "bg-danger" 
    };

    const bgClass = bgMap[type] || "bg-secondary";

    const toastHtml = `
            <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0 mb-2"
                 role="alert" aria-live="assertive" aria-atomic="true">
                <div class="d-flex">
                    <div class="toast-body">${message}</div>
                    <button type="button" class="btn-close btn-close-white me-2 m-auto"
                            data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
            </div>`;

    const container = document.getElementById("toastContainer");
    if (!container) {
        console.error("Missing #toastContainer");
        return;
    }

    container.insertAdjacentHTML("beforeend", toastHtml);
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    toastElement.addEventListener("hidden.bs.toast", () => toastElement.remove());
}
