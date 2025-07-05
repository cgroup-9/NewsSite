function renderPagination(currentPage, totalPages, onPageChange, containerSelector = "#paginationContainer") {
    const container = $(containerSelector);
    container.empty();

    // Exit if totalPages is invalid or only one page
    if (isNaN(totalPages) || totalPages <= 1) return;

    let html = `<div id="pagination">`;

    // Previous button
    if (currentPage > 1) {
        html += `<button class="paginationBtn" data-page="${currentPage - 1}">« Previous</button>`;
    }

    // Numbered page buttons
    for (let i = 1; i <= totalPages; i++) {
        html += `<button class="paginationBtn" data-page="${i}" ${i === currentPage ? "disabled" : ""}>${i}</button>`;
    }

    // Next button
    if (currentPage < totalPages) {
        html += `<button class="paginationBtn" data-page="${currentPage + 1}">Next »</button>`;
    }

    html += `</div>`;
    container.html(html);

    // Attach click handler
    container.off("click", ".paginationBtn").on("click", ".paginationBtn", function () {
        const page = $(this).data("page");
        if (page && typeof onPageChange === "function") {
            onPageChange(page);
        }
    });
}
