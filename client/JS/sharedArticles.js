function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

const apiUrl = `${baseApiUrl}/api/SharedArticle`;

$(document).ready(() => {
    fetchSharedArticles();
});

function fetchSharedArticles() {
    ajaxCall("GET", apiUrl, null,
        res => renderSharedArticles(res),
        err => console.error("❌ Failed to fetch shared articles:", err.responseText || err.statusText)
    );
}

function renderSharedArticles(articles) {
    const container = $("#sharedArticlesContainer");
    container.empty();

    articles.forEach(a => {
        const cardHtml = `
            <div class="sharedArticleCard">
                <div class="userName">👤 ${a.userName}</div>
                <div class="comment">"${a.comment}"</div>
                <img src="${a.urlToImage || '../Img/logo.png'}" alt="Image" class="articleImage" />
                <div class="articleInfo">
                    <strong>Author:</strong> ${a.author || "Unknown"}<br>
                    <strong>Title:</strong> ${a.title}<br>
                    <strong>Link:</strong> <a href="${a.articleUrl}" target="_blank">Read Article</a>
                </div>
            </div>
        `;
        container.append(cardHtml);
    });
}
