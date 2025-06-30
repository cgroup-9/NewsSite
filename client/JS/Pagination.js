function renderPagination(currentPage, totalPages, onPageChange, containerSelector = "#paginationContainer") {
    const container = $(containerSelector);
    container.empty();

    if (isNaN(totalPages) || totalPages <= 1) return;

    let html = `<div id="pagination">`;

    if (currentPage > 1) {
        html += `<button class="paginationBtn" data-page="${currentPage - 1}">« הקודם</button>`;
    }

    for (let i = 1; i <= totalPages; i++) {
        html += `<button class="paginationBtn" data-page="${i}" ${i === currentPage ? "disabled" : ""}>${i}</button>`;
    }

    if (currentPage < totalPages) {
        html += `<button class="paginationBtn" data-page="${currentPage + 1}">הבא »</button>`;
    }

    html += `</div>`;
    container.html(html);

    container.off("click", ".paginationBtn").on("click", ".paginationBtn", function () {
        const page = $(this).data("page");
        if (page && typeof onPageChange === "function") {
            onPageChange(page);
        }
    });
}
