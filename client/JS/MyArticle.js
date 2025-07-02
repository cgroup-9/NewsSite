function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/News`;

let articles = [];
let currentPage = 1;
const pageSize = 20;

$(document).ready(() => {
    const divArticles = $("#myArticleContainer");
    const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

    if (!currentUser) {
        alert("You must be logged in.");
        window.location.href = "login.html";
        return;
    }

    loadArticles();

    // Pagination is handled on the server using page and pageSize parameters.
    // We load one page at a time and show a "Next" button only if the current page is full.

    function loadArticles() {
        ajaxCall("GET", `${baseUrl}/Saved/${currentUser.id}?page=${currentPage}&pageSize=${pageSize}`, null,
            res => {
                articles = res;
                renderArticles();

                // If we got exactly pageSize results, maybe there are more pages.
                // If less than that → no more pages
                const hasNextPage = res.length === pageSize;

                renderPagination(currentPage, hasNextPage, function (nextPage) {
                    currentPage = nextPage;
                    loadArticles(); // fetch next page from server
                });
            },
            err => alert("Failed to load paged articles: " + (err.responseText || err.statusText))
        );
    }

    // Render the articles on the page
    function renderArticles() {
        if (articles.length === 0) {
            alert("No articles found in the database. You are being redirected to the main page.");
            window.location.href = "index.html";
            return;
        }

        divArticles.empty();

        for (let a of articles) {
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

    // Handle removing an article from saved list
    $(document).on("click", ".removeArticleBtn", function () {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        if (!user) {
            alert("❌ You must be logged in.");
            return;
        }

        const articleUrl = $(this).data("articleurl");

        const deleteRequest = {
            userId: user.id,
            articleUrl: articleUrl
        };

        ajaxCall("DELETE", `${baseApiUrl}/api/savedarticle`, JSON.stringify(deleteRequest),
            res => {
                alert("🗑️ " + res.message);

                // Option 1: Reload the page
                location.reload();

                // Option 2: Remove the card directly (uncomment if needed)
                // $(this).closest(".articleCard").remove();
            },
            err => {
                alert("❌ Failed to delete article: " + (err.responseJSON?.message || err.statusText));
            }
        );
    });

    // Create pagination buttons dynamically
    function renderPagination(currentPage, hasNextPage, onPageClick) {
        const paginationContainer = $("#paginationContainer");
        paginationContainer.empty();

        // Previous button
        if (currentPage > 1) {
            paginationContainer.append(`<button class="paginationBtn" data-page="${currentPage - 1}">⬅️ Prev</button>`);
        }

        // Current page
        paginationContainer.append(`<button class="paginationBtn activePage" data-page="${currentPage}">${currentPage}</button>`);

        // If current result count equals pageSize, maybe there's another page
        if (hasNextPage) {
            paginationContainer.append(`<button class="paginationBtn" data-page="${currentPage + 1}">Next ➡️</button>`);
        }

        // Bind page clicks
        $(".paginationBtn").click(function () {
            const page = $(this).data("page");
            onPageClick(page);
        });
    }

   

    // Save the current article to be shared
    let currentShareArticleUrl = null;

    // Open modal on share click
    $(document).on("click", ".shareArticleBtn", function () {
        currentShareArticleUrl = $(this).data("articleurl");
        $("#shareComment").val(""); // Clear textarea
        $("#shareModal").show();
    });

    // Close modal
    $(document).on("click", ".close", function () {
        $("#shareModal").hide();
        currentShareArticleUrl = null;
    });

    // Confirm share with comment
    $("#confirmShareBtn").click(() => {
        const user = JSON.parse(sessionStorage.getItem("currentUser"));
        const comment = $("#shareComment").val().trim();

        if (!user) {
            alert("❌ You must be logged in.");
            return;
        }

        if (!comment) {
            alert("✏️ Please enter a comment before sharing.");
            return;
        }

        const shareData = {
            userId: user.id,
            articleUrl: currentShareArticleUrl,
            comment: comment
        };

        ajaxCall("POST", `${baseApiUrl}/api/sharedarticle`, JSON.stringify(shareData),
            res => {
                alert("✅ Article shared successfully!");
                $("#shareModal").hide();
            },
            err => {
                alert("❌ Failed to share article: " + (err.responseText || err.statusText));
            }
        );
    });
});
