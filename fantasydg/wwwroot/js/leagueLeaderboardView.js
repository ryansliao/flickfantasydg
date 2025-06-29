document.addEventListener("DOMContentLoaded", function () {
    const table = $('#playersTable').DataTable({
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
            { targets: 0, width: "150px" },
            { targets: 1, width: "70px" },
            { targets: "_all", width: "70px" }
        ],
        dom: 'f rt<"bottom"ip>',
        initComplete: function () {
            $('#loadingSpinner').hide();
            $('#tableWrapper').removeClass('d-none');

            const filter = $('#playersTable_filter').detach();
            $('#searchContainer').empty().append(filter).show();

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
