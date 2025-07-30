// ✅ Fixed with category + start/end date filters + English comments throughout

function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/SavedArticle`;

let articles = [];
let currentPage = 1;
const pageSize = 20;
let selectedCategories = [];
let selectedStartDate = null;
let selectedEndDate = null;

$(document).ready(() => {
    const divArticles = $("#myArticleContainer");
    const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

    loadArticles();

    // Load saved articles from the server with pagination
    function loadArticles() {
        ajaxCall("GET", `${baseUrl}/${currentUser.id}?page=${currentPage}&pageSize=${pageSize}`, null,
            res => {
                articles = res;
                renderCategoryFilters();   // Render category checkboxes
                renderDateFilters();       // Render start and end date inputs
                renderArticles();          // Display filtered articles

                const hasNextPage = res.length === pageSize;
                renderPagination(currentPage, hasNextPage, function (nextPage) {
                    currentPage = nextPage;
                    loadArticles();
                });
            },
            err => alert("Failed to load paged articles: " + (err.responseText || err.statusText))
        );
    }

    // Display articles matching selected filters
    function renderArticles() {
        divArticles.empty();

        const filtered = articles.filter(a => {
            const pubDate = new Date(a.publishedAt);
            return (
                (selectedCategories.length === 0 || selectedCategories.includes(a.category)) &&
                (!selectedStartDate || pubDate >= new Date(selectedStartDate)) &&
                (!selectedEndDate || pubDate <= new Date(selectedEndDate))
            );
        });

        if (filtered.length === 0) {
            divArticles.append(`<p class="noResults">📭 No articles found matching your filters.</p>`);
            return;
        }

        for (let a of filtered) {
            const cardHtml = `
                <div class="articleCard">
                    <h2>${a.title}</h2>
                    <img src="${a.urlToImage || '../Img/logo.png'}" alt="Image" class="${a.urlToImage ? 'articleImage' : 'articleImage defaultImage'}" />
                    <p><strong>Author:</strong> ${a.author || 'Unknown'}</p>
                    <p><strong>Published At:</strong> ${new Date(a.publishedAt).toLocaleString()}</p>
                    <p class="description">${a.description || ''}</p>
                    <p>${a.content || ''}</p>
                    <p class="categoryTag">${a.category || ""}</p>
                    <a href="${a.articleUrl}" target="_blank">Read More</a>
                    <button class="removeArticleBtn" data-articleurl="${a.articleUrl}">Remove from saved</button>
                    <button class="shareArticleBtn" data-articleurl="${a.articleUrl}">📤 Share</button>
                </div>`;
            divArticles.append(cardHtml);
        }
    }

    // Renders checkbox filters for categories
    function renderCategoryFilters() {
        const allCategories = [...new Set(articles.map(a => a.category))].filter(Boolean);
        const filterDiv = $("#filterContainer");
        filterDiv.empty();

        const categoryHtml = allCategories.map(cat => `
            <label>
                <input type="checkbox" class="categoryCheckbox" value="${cat}" checked /> ${cat}
            </label>`).join("");

        filterDiv.append(`<div class="checkbox-scroll">${categoryHtml}</div>`);
    }

    // Renders input fields for filtering by start and end dates
    function renderDateFilters() {
        const dateDiv = $("#filterContainer");

        const dateInputs = `
            <div class="dateFilters">
                <label>From: <input type="date" id="startDateInput"></label>
                <label>To: <input type="date" id="endDateInput"></label>
                <button id="applyFilterBtn" class="filterBtn">Apply Filters</button>
            </div>`;

        dateDiv.append(dateInputs);
    }

    // Handle category checkbox selection
    $(document).on("change", ".categoryCheckbox", function () {
        selectedCategories = $(".categoryCheckbox:checked").map(function () {
            return this.value;
        }).get();
    });

    // Apply filters when filter button is clicked
    $(document).on("click", "#applyFilterBtn", function () {
        selectedCategories = $(".categoryCheckbox:checked").map(function () {
            return this.value;
        }).get();

        selectedStartDate = $("#startDateInput").val();
        selectedEndDate = $("#endDateInput").val();

        renderArticles();
    });

    // Render pagination controls for previous/current/next pages
    function renderPagination(currentPage, hasNextPage, onPageClick) {
        const paginationContainer = $("#paginationContainer");
        paginationContainer.empty();

        if (currentPage > 1) {
            paginationContainer.append(`<button class="paginationBtn" data-page="${currentPage - 1}">⬅️ Prev</button>`);
        }

        paginationContainer.append(`<button class="paginationBtn activePage" data-page="${currentPage}">${currentPage}</button>`);

        if (hasNextPage) {
            paginationContainer.append(`<button class="paginationBtn" data-page="${currentPage + 1}">Next ➡️</button>`);
        }

        $(".paginationBtn").click(function () {
            const page = $(this).data("page");
            onPageClick(page);
        });
    }

    // Remove article from saved list
    $(document).on("click", ".removeArticleBtn", function () {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        if (!user) return alert("❌ You must be logged in.");

        const articleUrl = $(this).data("articleurl");

        const deleteRequest = {
            userId: user.id,
            articleUrl: articleUrl
        };

        ajaxCall("DELETE", `${baseApiUrl}/api/savedarticle`, JSON.stringify(deleteRequest),
            res => {
                alert("🗑️ " + res.message);
                location.reload();
            },
            err => alert("❌ Failed to delete article: " + (err.responseJSON?.message || err.statusText))
        );
    });

    // Share article modal handling
    let currentShareArticleUrl = null;

    // Open share modal
    $(document).on("click", ".shareArticleBtn", function () {
        currentShareArticleUrl = $(this).data("articleurl");
        $("#shareComment").val("");
        $("#shareModal").show();
    });

    // Close share modal
    $(document).on("click", ".close", function () {
        $("#shareModal").hide();
        currentShareArticleUrl = null;
    });

    // Submit shared article with comment
    $("#confirmShareBtn").click(() => {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        const comment = $("#shareComment").val().trim();

        if (!user) return alert("❌ You must be logged in.");
        if (!comment) return alert("✏️ Please enter a comment before sharing.");

        const shareData = {
            userId: user.id,
            articleUrl: currentShareArticleUrl,
            comment: comment
        };

        ajaxCall("POST", `${baseApiUrl}/api/sharedArticle`, JSON.stringify(shareData),
            res => {
                alert("✔️ Article shared successfully!");
                $("#shareModal").hide();
            },
            err => alert("❌ Failed to share article: " + (err.responseText || err.statusText))
        );
    });
});
