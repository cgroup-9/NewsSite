function renderPagination(currentPage, totalPages, onPageChange, containerSelector = "#paginationContainer") {
    const container = $(containerSelector); // jQuery object for the pagination container
    container.empty(); // Clear any existing pagination buttons

    // Exit early if totalPages is invalid or only one page exists
    if (isNaN(totalPages) || totalPages < 1) return;

    let html = `<div id="pagination">`; // Start building pagination HTML
    console.log("Pagination HTML:", html); // Debug log

    // Add "Previous" button if not on the first page
    if (currentPage > 1) {
        html += `<button class="paginationBtn" data-page="${currentPage - 1}">« Previous</button>`;
    }

    // Add numbered page buttons
    for (let i = 1; i <= totalPages; i++) {
        // Disable the button for the current active page
        html += `<button class="paginationBtn" data-page="${i}" ${i === currentPage ? "disabled" : ""}>${i}</button>`;
    }

    // Add "Next" button if not on the last page
    if (currentPage < totalPages) {
        html += `<button class="paginationBtn" data-page="${currentPage + 1}">Next »</button>`;
    }

    html += `</div>`; // Close the pagination div
    container.html(html); // Insert the built HTML into the container

    // Attach click event to all pagination buttons
    // Removes old handlers with .off() to prevent duplicates
    container.off("click", ".paginationBtn").on("click", ".paginationBtn", function () {
        const page = $(this).data("page"); // Get the page number from button
        if (page && typeof onPageChange === "function") {
            onPageChange(page); // Call the callback function with the new page
        }
    });
}
