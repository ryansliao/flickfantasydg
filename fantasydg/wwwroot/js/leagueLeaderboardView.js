document.addEventListener("DOMContentLoaded", function () {
    const tablePoints = $('#playersPointsTable').DataTable({
        scrollX: true,
        scrollY: 'calc(70vh)',
        scrollCollapse: true,
        paging: false,
        searching: true,
        ordering: true,
        autoWidth: false,
        info: false,
        order: [[1, 'desc']],
        fixedColumns: {
            leftColumns: 2
        },
        columnDefs: [
            { targets: 0, width: "100px" },
            { targets: "_all", width: "60px" }
        ],
        dom: 'f rt<"bottom"ip>',
        initComplete: function () {
            $('#loadingSpinner').hide();
            $('#tablePointsWrapper').removeClass('d-none');

            const $filter = $('#playersPointsTable_filter');
            const $target = $('#searchContainer');

            if ($filter.length && $target.length && $target.children().length === 0) {
                $target.append($filter).show();
            }

            setTimeout(() => {
                tablePoints.columns.adjust().draw(false);
            }, 10);
        }
    });

    const tableWins = $('#playersWinsTable').DataTable({
        scrollX: true,
        scrollY: 'calc(70vh)',
        scrollCollapse: true,
        paging: false,
        searching: true,
        ordering: true,
        autoWidth: false,
        info: false,
        order: [[1, 'desc']],
        fixedColumns: {
            leftColumns: 2
        },
        columnDefs: [
            { targets: 0, width: "100px" },
            { targets: "_all", width: "60px" }
        ],
        dom: 'f rt<"bottom"ip>',
        initComplete: function () {
            $('#loadingSpinner').hide();
            $('#tableWinsWrapper').removeClass('d-none');

            const $filter = $('#playersWinsTable_filter').detach();
            const $target = $('#searchContainer');

            if ($filter.length && $target.length && $target.children().length === 0) {
                $target.append($filter).show();
            }

            setTimeout(() => {
                tableWins.columns.adjust().draw(false);
            }, 10);
        }
    });

    const topScroll = document.querySelector('.table-scroll-top');
    const bottomScroll = document.querySelector('.table-scroll-bottom');
    const wrapper = document.querySelector('.table-scroll-wrapper');
    const toggle = document.getElementById("scoringToggle");
    const winsContainer = document.getElementById("winsTable");
    const pointsContainer = document.getElementById("fantasyPointsTable");

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

    toggle?.addEventListener("change", function () {
        const isPoints = toggle.checked;

        if (winsContainer && pointsContainer) {
            winsContainer.style.display = isPoints ? "none" : "block";
            pointsContainer.style.display = isPoints ? "block" : "none";

            if (isPoints) {
                tablePoints?.columns.adjust();
            } else {
                tableWins?.columns.adjust();
            }
        }
    });
});
