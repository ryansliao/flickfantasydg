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

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("playersViewWrapper").style.display = "block";
    document.getElementById("tournamentDropdown")?.addEventListener("change", updatePlayerTable);
    document.getElementById("divisionDropdown")?.addEventListener("change", updatePlayerTable);

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
