function isDevEnv() {
    // Check if the app is running locally by looking for "localhost" in the URL
    return location.host.includes("localhost");
}

// Define API base URL depending on environment (local or production)
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/SavedArticle`;

// Global state variables
let articles = [];              // Store loaded articles from the server
let currentPage = 1;            // Current page for pagination
const pageSize = 20;            // Number of articles per page
let selectedCategories = [];    // Categories selected for filtering
let searchTerm = "";            // Search term for filtering

$(document).ready(() => {
    const divArticles = $("#myArticleContainer"); // Container where articles will be rendered
    const currentUser = JSON.parse(sessionStorage.getItem("currentUser")); // Logged-in user info

    loadArticles(); // Load articles when page is ready

    // Load saved articles from server with pagination and filters
    function loadArticles() {
        const categoryParam = selectedCategories.join(","); // Selected categories as CSV
        const searchParam = searchTerm ? `&searchTerm=${encodeURIComponent(searchTerm)}` : ""; // Add search if exists

        // Send GET request to fetch paginated saved articles for the logged-in user
        ajaxCall("GET",
            `${baseUrl}/${currentUser.id}?page=${currentPage}&pageSize=${pageSize}&categories=${categoryParam}${searchParam}`,
            null,
            res => {
                articles = res; // Save articles locally
                renderCategoryFilters(); // Show category checkboxes
                renderArticles(); // Show article cards

                const hasNextPage = res.length === pageSize; // True if more pages exist
                renderPagination(currentPage, hasNextPage, function (nextPage) {
                    currentPage = nextPage;
                    loadArticles(); // Load the next page
                });
            },
            err => alert("Failed to load paged articles: " + (err.responseText || err.statusText))
        );
    }

    // Display articles in the page
    function renderArticles() {
        divArticles.empty(); // Clear old content

        // Filter by selected categories
        const filtered = articles.filter(a => {
            return (
                (selectedCategories.length === 0 || selectedCategories.includes(a.category))
            );
        });

        // Show "no results" message if nothing matches
        if (filtered.length === 0) {
            divArticles.append(`<p class="noResults">📭 No articles found matching your filters.</p>`);
            return;
        }

        // Create HTML for each article
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

    // Show category filter checkboxes based on loaded articles
    function renderCategoryFilters() {
        const allCategories = [...new Set(articles.map(a => a.category))].filter(Boolean); // Unique non-empty categories
        const categoryDiv = $("#categoryFilters");
        categoryDiv.empty();

        // Build checkbox HTML for each category
        const categoryHtml = allCategories.map(cat => `
            <label>
                <input type="checkbox" class="categoryCheckbox" value="${cat}" checked /> ${cat}
            </label>`).join("");

        categoryDiv.append(categoryHtml);
    }

    // Track changes in category checkboxes
    $(document).on("change", ".categoryCheckbox", function () {
        selectedCategories = $(".categoryCheckbox:checked").map(function () {
            return this.value;
        }).get();
    });

    // Apply filters and reload articles
    $(document).on("click", "#applyFilterBtn", function () {
        selectedCategories = $(".categoryCheckbox:checked").map(function () {
            return this.value;
        }).get();
        currentPage = 1;
        loadArticles();
    });

    // Render pagination buttons
    function renderPagination(currentPage, hasNextPage, onPageClick) {
        const paginationContainer = $("#paginationContainer");
        paginationContainer.empty();

        // Show "Prev" button if not on first page
        if (currentPage > 1) {
            paginationContainer.append(`<button class="paginationBtn" data-page="${currentPage - 1}">⬅️ Prev</button>`);
        }

        // Show current page button
        paginationContainer.append(`<button class="paginationBtn activePage" data-page="${currentPage}">${currentPage}</button>`);

        // Show "Next" button if more pages exist
        if (hasNextPage) {
            paginationContainer.append(`<button class="paginationBtn" data-page="${currentPage + 1}">Next ➡️</button>`);
        }

        // Handle pagination button clicks
        $(".paginationBtn").click(function () {
            const page = $(this).data("page");
            onPageClick(page);
        });
    }

    // Remove saved article from server
    $(document).on("click", ".removeArticleBtn", function () {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        if (!user) return alert("❌ You must be logged in.");

        const articleUrl = $(this).data("articleurl");
        const deleteRequest = { userId: user.id, articleUrl: articleUrl };

        ajaxCall("DELETE", `${baseApiUrl}/api/savedarticle`, JSON.stringify(deleteRequest),
            res => {
                alert("🗑️ " + res.message);
                loadArticles(); // Refresh article list
            },
            err => alert("❌ Failed to delete article: " + (err.responseJSON?.message || err.statusText))
        );
    });

    // Share article modal - track which article is being shared
    let currentShareArticleUrl = null;

    // Open share modal
    $(document).on("click", ".shareArticleBtn", function () {
        currentShareArticleUrl = $(this).data("articleurl");
        $("#shareComment").val(""); // Clear comment field
        $("#shareModal").show();
    });

    // Close share modal
    $(document).on("click", ".close", function () {
        $("#shareModal").hide();
        currentShareArticleUrl = null;
    });

    // Confirm sharing an article
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

    // Search for articles by keyword
    $(document).on("click", "#searchBtn", function () {
        searchTerm = $("#searchInput").val().trim();
        currentPage = 1;
        loadArticles(); // Reload with search filter
    });
});
