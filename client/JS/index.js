function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/Articles`;

$(document).ready(() => {
    loadNews(); // Load all articles on page load
    renderCategoryFilters(); // Render category checkboxes

    let articles = [];

    function loadNews(category = "") {
        const url = category
            ? `${baseUrl}?country=us&categories=${category}`
            : `${baseUrl}?country=us`;

        ajaxCall("GET", url, null,
            res => {
                articles = res;
                renderArticles();
            },
            err => alert("❌ Failed to load articles: " + (err.responseText || err.statusText))
        );
    }

    function renderCategoryFilters() {
        const filterDiv = $("#categoryFilter");
        filterDiv.empty();

        const staticCategories = [
            "general", "business", "technology", "sports", "health", "science", "entertainment"
        ];

        // All articles option
        filterDiv.append(`<div class="categoryItem">
            <label><input type="checkbox" class="categoryCheckbox" value=""> All Articles</label>
        </div>`);

        // Add each category
        staticCategories.forEach(cat => {
            const label = cat.charAt(0).toUpperCase() + cat.slice(1);
            filterDiv.append(`
                <div class="categoryItem">
                    <label><input type="checkbox" class="categoryCheckbox" value="${cat}"> ${label}</label>
                </div>`);
        });

        // Add filter button
        filterDiv.append(`<div><button id="filterBtn" class="filterBtn">Filter by Tag</button></div>`);
    }

    function renderArticles() {
        const container = $("#newsContainer");
        container.empty();

        if (articles.length === 0) {
            container.html("<p>No articles found.<p>");
            return;
        }

        for (let a of articles) {
            const cardHtml = `
            <div class="articleCard" data-article='${JSON.stringify(a).replace(/'/g, "&apos;")}'>
                 <h2>${a.title}</h2>
                 <img src="${a.urlToImage || '../Img/logo.png'}" alt="Image" class="${a.urlToImage ? 'articleImage' : 'articleImage defaultImage'}" />
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

    function saveArticle(savedArticle) {
        const url = `${baseApiUrl}/api/SavedArticle`;


        ajaxCall("POST", url, JSON.stringify(savedArticle),
            res => {
                alert(res.message);
            },
            err => {
                alert(err.statusText);
            }
        );
    }

    $(document).on("click", "#filterBtn", function () {
        const categories = $(".categoryCheckbox:checked").map(function () {
            return $(this).val();
        }).get();

        if (categories.includes("") || categories.length === 0)
            loadNews();
        else {
            const categoryParam = categories.join(',');
            loadNews(categoryParam);
        }
    });

    $(document).on("click", ".saveArticleBtn", function () {
        const user = getCurrentUser();
        if (!user) {
            alert("❌ You must be logged in to save an article.");
            return;
        }

        const articleDataStr = $(this).closest('.articleCard').attr('data-article').replace(/&apos;/g, "'");
        const article = JSON.parse(articleDataStr);

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

        saveArticle(savedArticle);
    });
});
