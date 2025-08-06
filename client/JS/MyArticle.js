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
let searchTerm = "";

$(document).ready(() => {
    const divArticles = $("#myArticleContainer");
    const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

    loadArticles();

    // Load saved articles from server with pagination
    function loadArticles() {
        const categoryParam = selectedCategories.join(",");
        const searchParam = searchTerm ? `&searchTerm=${encodeURIComponent(searchTerm)}` : "";

        ajaxCall("GET",
            `${baseUrl}/${currentUser.id}?page=${currentPage}&pageSize=${pageSize}&categories=${categoryParam}${searchParam}`,
            null,
            res => {
                articles = res;
                renderCategoryFilters();
                renderArticles();

                const hasNextPage = res.length === pageSize;
                renderPagination(currentPage, hasNextPage, function (nextPage) {
                    currentPage = nextPage;
                    loadArticles();
                });
            },
            err => alert("Failed to load paged articles: " + (err.responseText || err.statusText))
        );
    }

    // Render articles
    function renderArticles() {
        divArticles.empty();

        const filtered = articles.filter(a => {
            return (
                (selectedCategories.length === 0 || selectedCategories.includes(a.category))
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
                    <img src="${a.urlToImage || '../Img/logo.png'}" 
                         alt="Image" 
                         class="${a.urlToImage ? 'articleImage' : 'articleImage defaultImage'}" />
                    <p><strong>Author:</strong> ${a.author || 'Unknown'}</p>
                    <p><strong>Published At:</strong> ${new Date(a.publishedAt).toLocaleString()}</p>
                    <p class="description">${a.description || ''}</p>
                    <p>${a.content || ''}</p>
                    <p class="categoryTag">${a.category || ""}</p>
                    <a href="${a.articleUrl}" target="_blank">Read More</a>
                    <button class="removeArticleBtn" data-articleurl="${a.articleUrl}">Remove</button>
                    <button class="shareArticleBtn" data-articleurl="${a.articleUrl}">📤 Share</button>
                </div>`;
            divArticles.append(cardHtml);
        }
    }

    // Render category checkboxes
    function renderCategoryFilters() {
        const allCategories = [...new Set(articles.map(a => a.category))].filter(Boolean);
        const categoryDiv = $("#categoryFilters");
        categoryDiv.empty();

        const categoryHtml = allCategories.map(cat => `
            <label>
                <input type="checkbox" class="categoryCheckbox" value="${cat}" checked /> ${cat}
            </label>`).join("");

        categoryDiv.append(categoryHtml);
    }

    // Category selection
    $(document).on("change", ".categoryCheckbox", function () {
        selectedCategories = $(".categoryCheckbox:checked").map(function () {
            return this.value;
        }).get();
    });

    // Apply filter button
    $(document).on("click", "#applyFilterBtn", function () {
        selectedCategories = $(".categoryCheckbox:checked").map(function () {
            return this.value;
        }).get();
        currentPage = 1;
        loadArticles();
    });

    // Pagination
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

    // Remove article
    $(document).on("click", ".removeArticleBtn", function () {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        if (!user) return alert("❌ You must be logged in.");

        const articleUrl = $(this).data("articleurl");

        const deleteRequest = { userId: user.id, articleUrl: articleUrl };

        ajaxCall("DELETE", `${baseApiUrl}/api/savedarticle`, JSON.stringify(deleteRequest),
            res => {
                alert("🗑️ " + res.message);
                loadArticles();
            },
            err => alert("❌ Failed to delete article: " + (err.responseJSON?.message || err.statusText))
        );
    });

    // Share modal
    let currentShareArticleUrl = null;

    $(document).on("click", ".shareArticleBtn", function () {
        currentShareArticleUrl = $(this).data("articleurl");
        $("#shareComment").val("");
        $("#shareModal").show();
    });

    $(document).on("click", ".close", function () {
        $("#shareModal").hide();
        currentShareArticleUrl = null;
    });

    $("#confirmShareBtn").click(() => {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        const comment = $("#shareComment").val().trim();

        if (!user) return alert("❌ You must be logged in.");
        if (!comment) return alert("✏️ Please enter a comment before sharing.");

        const shareData = { userId: user.id, articleUrl: currentShareArticleUrl, comment: comment };

        ajaxCall("POST", `${baseApiUrl}/api/sharedArticle`, JSON.stringify(shareData),
            res => {
                alert("✔️ Article shared successfully!");
                $("#shareModal").hide();
            },
            err => alert("❌ Failed to share article: " + (err.responseText || err.statusText))
        );
    });

    // Search
    $(document).on("click", "#searchBtn", function () {
        searchTerm = $("#searchInput").val().trim();
        currentPage = 1;
        loadArticles();
    });
});
