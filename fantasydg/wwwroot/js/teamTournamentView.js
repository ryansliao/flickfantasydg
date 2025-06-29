let playersDataTable = null;
let currentSearchTerm = '';

function initializePlayersTable() {
    playersDataTable = $('#playersTable').DataTable({
        scrollX: true,
        scrollY: 'calc(70vh)',
        scrollCollapse: true,
        paging: false,
        searching: true,
        ordering: true,
        autoWidth: false,
        info: false,
        order: [[0, 'asc']],
        fixedColumns: {
            leftColumns: 3
        },
        columnDefs: [
            { targets: 0, width: "100px" },
            { targets: 1, width: "150px" },
            { targets: "_all", width: "50px" }
        ],
        dom: 'f rt<"bottom"ip>',
        initComplete: function () {
            $('#loadingSpinner').hide();
            $('#tableWrapper').removeClass('d-none');

            const filter = $('#playersTable_filter').detach();
            $('#searchContainer').html(filter).show();

            if (currentSearchTerm) {
                const searchInput = document.querySelector('#playersTable_filter input[type="search"]');
                if (searchInput) {
                    searchInput.value = currentSearchTerm;
                    searchInput.dispatchEvent(new Event('input', { bubbles: true }));
                }
            }

            setTimeout(() => {
                playersDataTable.columns.adjust();
            }, 10);
        }
    });
}

function fetchTournamentResults() {
    const tournamentId = document.getElementById("tournamentDropdown")?.value;
    const division = document.getElementById("divisionDropdown")?.value;
    const leagueId = document.getElementById("resultsFilterControls")?.dataset.leagueId;

    const url = `/League/TeamTournamentResultsView?leagueId=${leagueId}&tournamentId=${tournamentId}&division=${encodeURIComponent(division)}`;

    if ($.fn.DataTable.isDataTable("#playersTable")) {
        currentSearchTerm = $('#playersTable').DataTable().search();
        $('#playersTable').DataTable().destroy();
        $('#playersTable').remove();
    }

    $('#searchContainer').empty();

    fetch(url, {
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(res => res.text())
        .then(html => {
            document.getElementById("teamTournamentResultsContainer").innerHTML = html;
            initializePlayersTable();
        });
}

document.addEventListener("DOMContentLoaded", function () {
    initializePlayersTable();

    document.getElementById("resultsViewWrapper").style.display = "block";

    document.getElementById("tournamentDropdown")?.addEventListener("change", fetchTournamentResults);
    document.getElementById("divisionDropdown")?.addEventListener("change", fetchTournamentResults);

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
