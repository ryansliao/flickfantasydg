document.addEventListener("DOMContentLoaded", () => {
    const context = document.getElementById("teamContext");
    if (!context) return;

    const token = context.dataset.antiforgery;
    const teamId = context.dataset.teamId;
    const isOwnTeam = context.dataset.isOwnTeam === "true";
    let mouseDownTime = null;

    if (!isOwnTeam) return;

    const getClassForStatus = (status) => {
        switch (status) {
            case "Starter": return "starter-card";
            case "Bench": return "bench-card";
            case "InjuryReserve": return "ir-card";
            default: return "";
        }
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

    const showPlayerModal = (name, pdgaNumber) => {
        document.getElementById('playerModalName').textContent = name;
        document.getElementById('playerModalId').textContent = pdgaNumber;
        document.getElementById('modalPdgaNumber').value = pdgaNumber;
        document.getElementById('playerModal').style.display = 'block';
    };

    const closeModal = () => {
        document.getElementById('playerModal').style.display = 'none';
    };

    document.querySelectorAll(".draggable-player").forEach(card => {
        card.addEventListener("dragstart", ev => {
            ev.dataTransfer.setData("playerId", ev.target.dataset.id);
        });

        card.addEventListener("mousedown", () => {
            mouseDownTime = Date.now();
        });

        card.addEventListener("mouseup", ev => {
            const duration = Date.now() - mouseDownTime;
            if (duration < 200) {
                showPlayerModal(card.textContent.trim(), card.dataset.id);
            }
        });
    });

    document.querySelectorAll(".drop-slot").forEach(slot => {
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
    });

    document.getElementById("lockTournamentDropdown")?.addEventListener("change", () => {
        loadStarterPreview();
        updateLockButtonLabel();
    });

    const loadStarterPreview = () => {
        const dropdown = document.getElementById("lockTournamentDropdown");
        const selected = dropdown.options[dropdown.selectedIndex];
        const tournamentId = selected.value;
        const isLocked = selected.getAttribute("data-locked") === "true";

        fetch(`/Team/GetStarterPreview?teamId=${teamId}&tournamentId=${tournamentId}&isLocked=${isLocked}`)
            .then(res => res.text())
            .then(html => {
                document.getElementById("starterPreview").innerHTML = html;
            });
    };

    const updateLockButtonLabel = () => {
        const dropdown = document.getElementById("lockTournamentDropdown");
        const selected = dropdown.options[dropdown.selectedIndex];
        const button = document.getElementById("lockButton");
        const isLocked = selected.getAttribute("data-locked") === "true";

        if (isLocked) {
            button.innerText = "Unlock Tournament";
            button.className = "btn btn-danger mt-3";
        } else {
            button.innerText = "Confirm Lock";
            button.className = "btn btn-success mt-3";
        }
    };

    const lockBtn = document.getElementById("lockRosterBtn");
    lockBtn?.addEventListener("click", () => {
        document.getElementById("lockModal").style.display = "block";
        loadStarterPreview();
        updateLockButtonLabel();
    });

    document.getElementById("lockModal")?.addEventListener("click", ev => {
        if (ev.target.id === "lockModal") {
            document.getElementById("lockModal").style.display = "none";
        }
    });

    document.getElementById("playerModal")?.addEventListener("click", ev => {
        if (ev.target.id === "playerModal") {
            closeModal();
        }
    });

    const unlockBtn = document.getElementById("unlockButton");
    unlockBtn?.addEventListener("click", () => {
        const tournamentId = document.getElementById("lockTournamentDropdown").value;
        const form = document.createElement("form");
        form.method = "post";
        form.action = "/Team/UnlockRoster";

        const teamInput = document.createElement("input");
        teamInput.type = "hidden";
        teamInput.name = "teamId";
        teamInput.value = teamId;

        const tourInput = document.createElement("input");
        tourInput.type = "hidden";
        tourInput.name = "tournamentId";
        tourInput.value = tournamentId;

        const tokenInput = document.createElement("input");
        tokenInput.type = "hidden";
        tokenInput.name = "__RequestVerificationToken";
        tokenInput.value = token;

        form.append(teamInput, tourInput, tokenInput);
        document.body.appendChild(form);
        form.submit();
    });
});
