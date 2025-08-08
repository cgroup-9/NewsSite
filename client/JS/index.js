// Detect if running locally (development) or in production
function isDevEnv() {
    return location.host.includes("localhost");
}

// Define API base URL based on environment
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

// Articles API base endpoint
const baseUrl = `${baseApiUrl}/api/Articles`;

$(document).ready(() => {
    loadNews();             // Load all articles when page loads
    renderCategoryFilters(); // Render category checkboxes on page load

    let articles = []; // Will hold the fetched articles

    // Fetch news from API (optionally filtered by category)
    function loadNews(category = "") {
        const url = category
            ? `${baseUrl}?country=us&categories=${category}`
            : `${baseUrl}?country=us`;

        ajaxCall("GET", url, null,
            res => {
                articles = res; // Store fetched articles
                renderArticles(); // Render them on the page
            },
            err => alert("❌ Failed to load articles: " + (err.responseText || err.statusText))
        );
    }

    // Render static category checkboxes and filter button
    function renderCategoryFilters() {
        const filterDiv = $("#categoryFilter");
        filterDiv.empty(); // Clear existing filters

        // Predefined list of categories
        const staticCategories = [
            "general", "business", "technology", "sports", "health", "science", "entertainment"
        ];

        // Add "All Articles" option
        filterDiv.append(`<div class="categoryItem">
            <label><input type="checkbox" class="categoryCheckbox" value=""> All Articles</label>
        </div>`);

        // Add each category as a checkbox
        staticCategories.forEach(cat => {
            const label = cat.charAt(0).toUpperCase() + cat.slice(1);
            filterDiv.append(`
                <div class="categoryItem">
                    <label><input type="checkbox" class="categoryCheckbox" value="${cat}"> ${label}</label>
                </div>`);
        });

        // Add a button to apply filters
        filterDiv.append(`<div><button id="filterBtn" class="filterBtn">Filter by Tag</button></div>`);
    }

    // Render article cards in the container
    function renderArticles() {
        const container = $("#newsContainer");
        container.empty(); // Clear old articles

        // If no articles, show a message
        if (articles.length === 0) {
            container.html("<p>No articles found.<p>");
            return;
        }

        // Loop through each article and create a card
        for (let a of articles) {
            const cardHtml = `
            <div class="articleCard" data-article='${JSON.stringify(a).replace(/'/g, "&apos;")}'>
                 <h2>${a.title}</h2>
                 <img src="${a.urlToImage || '../Img/logo.png'}" alt="Image" 
                      class="${a.urlToImage ? 'articleImage' : 'articleImage defaultImage'}" />
                 <p><strong>Author:</strong> ${a.author || 'Unknown'}</p>
                 <p><strong>Published At:</strong> ${new Date(a.publishedAt).toLocaleString()}</p>
                 <p class="description">${a.description || ''}</p>
                 <p>${a.content || ''}</p>
                 <p class="categoryTag"> ${a.category || ""}</p>
                 <a href="${a.url}" target="_blank">Read More</a>
                 <button class="saveArticleBtn">Save Article</button>
            </div>`;
            container.append(cardHtml);
        }
    }

    // Save an article to the server
    function saveArticle(savedArticle) {
        const url = `${baseApiUrl}/api/SavedArticle`;

        ajaxCall("POST", url, JSON.stringify(savedArticle),
            res => {
                alert(res.message); // Show success message from server
            },
            err => {
                alert(err.statusText); // Show error
            }
        );
    }

    // Handle filter button click
    $(document).on("click", "#filterBtn", function () {
        const categories = $(".categoryCheckbox:checked").map(function () {
            return $(this).val();
        }).get();

        // If "All" is selected or none selected → load all articles
        if (categories.includes("") || categories.length === 0)
            loadNews();
        else {
            const categoryParam = categories.join(',');
            loadNews(categoryParam);
        }
    });

    // Handle "Save Article" button click
    $(document).on("click", ".saveArticleBtn", function () {
        const user = getCurrentUser(); // Get logged-in user
        if (!user) {
            alert("❌ You must be logged in to save an article.");
            return;
        }

        // Retrieve article object from card's data attribute
        const articleDataStr = $(this).closest('.articleCard').attr('data-article').replace(/&apos;/g, "'");
        const article = JSON.parse(articleDataStr);

        // Build object for saving
        const savedArticle = {
            UserId: user.id,
            ArticleUrl: article.url,
            Title: article.title || "",
            Description: article.description || "",
            UrlToImage: article.urlToImage || "",
            Author: article.author || "",
            PublishedAt: article.publishedAt || "",
            Content: article.content || "",
            Category: article.category || ""
        };

        // Send save request to server
        saveArticle(savedArticle);
    });
});
