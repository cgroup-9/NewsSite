function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/News`;

$(document).ready(() => {

    loadNews(); //render latest articles on load without categories

    let articles = [];
    let currentPage = 1;
    const pageSize = 20;   //num of articles in page
    let totalPages = 1;

    function loadNews(category = "") {
        const url = category
            ? `${baseUrl}?country=us&categories=${category}`
            : `${baseUrl}?country=us`;

        ajaxCall("GET", url, null,
            res => {
                articles = res;
                totalPages = Math.ceil(articles.length / pageSize);
                currentPage = 1;

                // extract unique categories and render them dynamically
                const allCategories = [...new Set(articles.map(a => a.category).filter(Boolean))];
                renderCategoryFilters(allCategories);

                renderArticles(currentPage);
                renderPagination(currentPage, totalPages, function (page) {
                    currentPage = page;
                    renderArticles(currentPage);
                });
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
        <label><input type="checkbox" class="categoryCheckbox" value=""> All articles</label>
    </div>`);

        staticCategories.forEach(cat => {
            const label = cat.charAt(0).toUpperCase() + cat.slice(1);
            filterDiv.append(`
            <div class="categoryItem">
                <label><input type="checkbox" class="categoryCheckbox" value="${cat}"> ${label}</label>
            </div>`);
        });

        // Add the filter button
        filterDiv.append(`<div><button id="filterBtn" class="filterBtn">Filter by Tag</button></div>`);
    }


    function renderArticles(page) {
        const container = $("#newsContainer");
        container.empty();

        const startIndex = (page - 1) * pageSize;
        const endIndex = Math.min(startIndex + pageSize, articles.length);
        const pageArticles = articles.slice(startIndex, endIndex);

        if (pageArticles.length === 0) {
            container.html("<p>No articles found.<p>");
            return;
        }

        for (let a of pageArticles) {
            const cardHtml = `
            <div class="articleCard" data-article='${JSON.stringify(a).replace(/'/g, "&apos;")}'>
                 <h2>${a.title}</h2>
                 <img src="${a.urlToImage || '../Img/logo.png'}" alt="Image" class="${a.urlToImage ? 'articleImage' : 'articleImage defaultImage'}"  />
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
        url = `${baseUrl}/Save-Article`;

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
        const categories = $(".categoryCheckbox:checked").map(function () { return $(this).val(); }).get();
        if (categories.includes("") || categories.length === 0)
            loadNews();
        else {
            const categoryParam = categories.join(',');
            loadNews(categoryParam);
        }
    });

    // Get the article data stored in the 'data-article' attribute of the closest .articleCard element.
    // This data is a JSON string that may contain encoded apostrophes (&apos;), so we replace them with regular single quotes (')
    // to make sure JSON.parse works correctly.
    $(document).on("click", ".saveArticleBtn", function () {
        const user = getCurrentUser();

        // If the user is not logged in, show an alert and stop
        if (!user) {
            alert("❌ You must be logged in to save an article.");
            return;
        }

        // Get article data from the data-article attribute
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



   
    //function openRentModal(movieId, priceToRent) {
    //    const modal = $("#rentModal");
    //    const today = new Date().toISOString().split("T")[0];

    //    modal.html(`
    //    <div class="modal-content">
    //        <span class="close">&times;</span>
    //        <h3>📅 Rent Movie</h3>
    //        <p>Choose rental dates:</p>
    //        <input type="date" id="rentStart" value="${today}"><br><br>
    //        <input type="date" id="rentEnd"><br><br>
    //        <p id="priceSummary" style="font-weight:bold;"></p>
    //        <button id="confirmRentBtn">Confirm Rent</button>
    //    </div>
    //`);

    //    modal.show();

    //    $(".close").click(() => modal.hide());

    

    //// דפדוף בין עמודים
    //$(document).on("click", ".paginationBtn", function () {
    //    const page = $(this).data("page");
    //    if (page && page !== currentPage) {
    //        loadPagedMovies(page);
    //    }
    //});


});
