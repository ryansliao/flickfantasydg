let playersDataTable = null;

function initializePlayersTable() {
    $('#tableWrapper').addClass('d-none');

    playersDataTable = $('#playersTable').DataTable({
        scrollX: true,
        scrollY: 'calc(70vh)',
        scrollCollapse: true,
        paging: false,
        searching: true,
        ordering: true,
        autoWidth: false,
        info: false,
        order: [[2, 'desc']],
        fixedColumns: {
            leftColumns: 2
        },
        columnDefs: [
            { orderable: false, targets: 0, width: "40px" },
            { targets: 1, width: "100px" },
            { targets: "_all", width: "50px" },
            {
                targets: "_all",
                render: function (data, type, row, meta) {
                    if (meta.col < 2) return data; // Skip columns 0 and 1

                    const num = parseFloat(data?.toString().replace(/[^0-9.\-]/g, ''));
                    if (isNaN(num)) return data;

                    return type === 'display' ? num.toFixed(1) : num;
                }
            }
        ],
        dom: 'f rt<"bottom"ip>',
        initComplete: function () {
            $('#loadingSpinner').hide();
            $('#tableWrapper').removeClass('d-none');
            $('#searchContainer').empty();

            const $filter = $('#playersTable_filter').detach();
            if ($filter.length) {
                $('#searchContainer').append($filter).show();
            }

            $('#playersTable thead th, #playersTable tbody td').each(function () {
                const html = $(this).html().trim();
                const text = $(this).text().trim();

                if (html === text) {
                    $(this).html(`<span class="ellipsis-text" title="${text}">${text}</span>`);
                }
            });

            setTimeout(() => {
                playersDataTable.columns.adjust();
                document.getElementById("playerTableContainer")?.classList.remove("d-none");
            }, 10);
        }
    });
}

async function fetchPlayersViaAjax() {
    console.log("fetchPlayersViaAjax START");

    const container = document.getElementById("playerTableContainer");
    if (!container) {
        console.error("Missing #playerTableContainer");
        return;
    }

    // Hide container BEFORE replacing HTML
    container.classList.add("d-none");

    const leagueId = container.dataset.leagueId;
    if (!leagueId) {
        console.warn("Missing leagueId for table reload");
        return;
    }

    const url = `/League/Players?leagueId=${leagueId}`;

    if ($.fn.DataTable.isDataTable("#playersTable")) {
        $('#playersTable').DataTable().destroy();
    }

    const response = await fetch(url, {
        headers: { "X-Requested-With": "XMLHttpRequest" }
    });

    const html = await response.text();
    container.innerHTML = html;

    setTimeout(() => {
        initializePlayersTable(); // will show container when ready
        rebindAddButtons();
    }, 0);
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
                    try {
                        await fetchPlayersViaAjax();
                    } catch (err) {
                        console.error("fetchPlayersViaAjax failed:", err);
                    }
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