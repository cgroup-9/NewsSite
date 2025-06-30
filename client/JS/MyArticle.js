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
const pageSize = 20;   //num of articles in page
let totalPages = 1;

$(document).ready(() => {

    const divArticles = $("#myArticleContainer");

    const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));
    if (!currentUser) {
        alert("You must be logged in.");
        window.location.href = "login.html";
        return;
    }

    ajaxCall("GET", `${baseUrl}/Saved/${currentUser.id}`, null,
        res => {
            articles = res;     //articles from server into the array
            totalPages = Math.ceil(articles.length / pageSize);   //check how many pages will be
            currentPage = 1;
            console.log("Total pages:", totalPages);

            renderArticles(currentPage);
            renderPagination(currentPage, totalPages, function (page) {
                currentPage = page;
                renderArticles(currentPage);
            });        },
        err => alert("Failed to load articles: " + (err.responseText || err.statusText))
    );

    function renderArticles(page) {

        const startIndex = (page - 1) * pageSize;   //which page
        const endIndex = Math.min(startIndex + pageSize, articles.length);    //if last page-take the array length
        const pageArticles = articles.slice(startIndex, endIndex);

        if (articles.length === 0) {
            alert("No articles found in the database. You are being redirected to the main page.");
            window.location.href = "index.html";
            divArticles.empty();
            return;
        }

        divArticles.empty();

        for (let a of pageArticles) {

            const cardHtml = `
            <div class="articleCard">
                 <h2>${a.title}</h2>
                 <img src="${a.urlToImage || '../Img/logo.png'}" alt="Image" class="${a.urlToImage ? 'articleImage' : 'articleImage defaultImage'}"  />
                 <p><strong>Author:</strong> ${a.author || 'Unknown'}</p>
                 <p><strong>Published At:</strong> ${new Date(a.publishedAt).toLocaleString()}</p>
                 <p class="description">${a.description || ''}</p>
                 <p>${a.content || ''}</p>
                 <p class="categoryTag"> ${a.category || ""}</p>
                 <a href="${a.articleUrl }" target="_blank">Read More</a>
                 <button class="saveArticleBtn">Remove from saved</button>
                 
            </div>`;
            divArticles.append(cardHtml);

        }
      
    }

})