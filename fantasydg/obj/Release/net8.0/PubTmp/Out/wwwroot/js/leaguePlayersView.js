let playersDataTable = null;
let currentSearchTerm = '';

function initializePlayersTable() {
    playersDataTable = $('#playersTable').DataTable({
        scrollX: true,
        scrollY: 'calc(100vh - 355px)',
        scrollCollapse: true,
        paging: false,
        searching: true,
        ordering: true,
        autoWidth: false,
        info: false,
        order: [[3, 'asc']],
        fixedColumns: {
            leftColumns: 3
        },
        columnDefs: [
            { orderable: false, targets: 0, width: "40px" },
            { targets: 1, width: "150px" },
            { targets: 2, width: "50px" },
            { targets: 3, width: "40px" },
            { targets: "_all", width: "50px" }
        ],
        dom: 'f rt<"bottom"ip>',
        initComplete: function () {
            $('#tableWrapper').removeClass('d-none');
            $('#loadingSpinner').hide();

            const filter = $('#playersTable_filter').detach();
            $('#searchContainer').html(filter).show();

            if (currentSearchTerm) {
                // ✅ Apply the filter first
                playersDataTable.search(currentSearchTerm).draw(false);

                // ✅ Then manually update input box (for visual consistency)
                setTimeout(() => {
                    const searchInput = document.querySelector('#playersTable_filter input[type="search"]');
                    if (searchInput) {
                        searchInput.value = currentSearchTerm;
                        searchInput.dispatchEvent(new Event('input', { bubbles: true }));
                    }
                }, 0);
            }

            setTimeout(() => {
                playersDataTable.columns.adjust();
            }, 10);
        }
    });
}

async function fetchPlayersViaAjax() {
    const tournamentId = document.getElementById("tournamentDropdown").value;
    const division = document.getElementById("divisionDropdown").value;
    const leagueId = document.getElementById("playerFilterControls").dataset.leagueId;

    const url = `/League/Players?leagueId=${leagueId}&tournamentId=${tournamentId}&division=${encodeURIComponent(division)}`;

    // ✅ Save current search term
    if ($.fn.DataTable.isDataTable("#playersTable")) {
        currentSearchTerm = $('#playersTable').DataTable().search();
        $('#playersTable').DataTable().destroy();
        $('#playersTable').remove();
    }

    $('#searchContainer').empty();

    const response = await fetch(url, {
        headers: {
            "X-Requested-With": "XMLHttpRequest"
        }
    });

    const html = await response.text();
    document.getElementById("playerTableContainer").innerHTML = html;

    initializePlayersTable();
    rebindAddButtons();
}

function rebindAddButtons() {
    document.querySelectorAll(".ajax-add-player-form").forEach(form => {
        form.addEventListener("submit", async function (e) {
            e.preventDefault();

            const formData = new FormData(this);
            const alertBox = document.getElementById("playerAddAlert");

            try {
                const response = await fetch(this.action, {
                    method: "POST",
                    body: formData,
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    }
                });

                const data = await response.json();

                if (data.success) {
                    showToast(data.message, "success");
                    await fetchPlayersViaAjax();
                } else {
                    showToast(data.message, "danger");
                }

            } catch (err) {
                showToast("Error adding player.", "danger");
            }
        });
    });
}


document.addEventListener("DOMContentLoaded", function () {
    initializePlayersTable();
    rebindAddButtons();

    document.getElementById("tournamentDropdown").addEventListener("change", fetchPlayersViaAjax);
    document.getElementById("divisionDropdown").addEventListener("change", fetchPlayersViaAjax);

    const topScroll = document.querySelector('.table-scroll-top');
    const bottomScroll = document.querySelector('.table-scroll-bottom');
    const wrapper = document.querySelector('.table-scroll-wrapper');

    function syncScrollBar(scrollDiv) {
        const fakeScroll = document.createElement('div');
        fakeScroll.style.width = wrapper.scrollWidth + 'px';
        fakeScroll.style.height = '1px';
        scrollDiv.appendChild(fakeScroll);
    }

    if (topScroll && bottomScroll && wrapper) {
        syncScrollBar(topScroll);
        syncScrollBar(bottomScroll);

        topScroll.addEventListener('scroll', () => {
            wrapper.scrollLeft = topScroll.scrollLeft;
            bottomScroll.scrollLeft = topScroll.scrollLeft;
        });

        bottomScroll.addEventListener('scroll', () => {
            wrapper.scrollLeft = bottomScroll.scrollLeft;
            topScroll.scrollLeft = bottomScroll.scrollLeft;
        });

        wrapper.addEventListener('scroll', () => {
            topScroll.scrollLeft = wrapper.scrollLeft;
            bottomScroll.scrollLeft = wrapper.scrollLeft;
        });
    }
});

function showToast(message, type = "success") {
    const toastId = `toast-${Date.now()}`;
    const bgClass = type === "success" ? "bg-success" : "bg-danger";

    const toastHtml = `
        <div id="${toastId}" class="toast align-items-center text-white ${bgClass} border-0 mb-2" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;

    const container = document.getElementById("toastContainer");
    container.insertAdjacentHTML("beforeend", toastHtml);

    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Optional: auto-remove the element after it disappears
    toastElement.addEventListener("hidden.bs.toast", () => toastElement.remove());
}