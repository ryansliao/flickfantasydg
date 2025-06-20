function updatePlayerTable() {
    const tournamentId = document.getElementById('tournamentDropdown')?.value;
    const division = document.getElementById('divisionDropdown')?.value;
    const leagueId = document.body.dataset.leagueId;

    const url = `/League/FilterPlayers?leagueId=${leagueId}&tournamentId=${tournamentId}&division=${encodeURIComponent(division)}`;

    fetch(url, {
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        }
    })
        .then(res => res.text())
        .then(html => {
            document.getElementById("table-scroll-container").innerHTML = html;
            $('#playersTable').DataTable();
        });
}

document.getElementById("tournamentDropdown")?.addEventListener("change", updatePlayerTable);
document.getElementById("divisionDropdown")?.addEventListener("change", updatePlayerTable);

document.addEventListener("DOMContentLoaded", function () {
    const table = $('#playersTable').DataTable({
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

            setTimeout(() => {
                table.columns.adjust().draw(false);
            }, 10);
        }
    });

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
