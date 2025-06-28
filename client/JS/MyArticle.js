function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/News`;

$(document).ready(() => {
    const divArticles = $("#myArticleContainer");

    const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));
    if (!currentUser) {
        alert("You must be logged in.");
        window.location.href = "login.html";
        return;
    }

    $.ajax({
        type: "GET",
        url: `${baseUrl}/Saved/${currentUser.id}`,
        success: function (data) {
            renderArticles(data);
        },
        error: function (err) {
            alert("❌ Failed to load saved articles: " + err.statusText);
        }
    });

    function renderArticles(articles) {
        if (articles.length === 0) {
            alert("No articles found in the database. You are being redirected to the main page.");
            window.location.href = "index.html";
            divArticles.empty();
            return;
        }

        divArticles.empty();
        divCards.append('<h2 class="fullRowTitle">My Articles</h2>');

        for (let a of articles) {
             const url =`${baseUrl}?url=${a.articleUrl}`;    // if category==null so bring all articles

        }


        ajaxCall("GET", url, null,
            res => {
                articles = res;     //articles from server into the array
                totalPages = Math.ceil(articles.length / pageSize);   //check how many pages will be
                currentPage = 1;
                console.log("Total pages:", totalPages);

                renderArticles(currentPage);
                renderPagination();
            },
            err => alert("❌ Failed to load articles: " + (err.responseText || err.statusText))
        );
    }

})