function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar5";

const apiUrl = `${baseApiUrl}/api/SharedArticle`;
const getAllUsersUrl = `${baseApiUrl}/api/Users`;

$(document).ready(() => {
    fetchAllUsers();         // Load all users into the modal
    fetchSharedArticles();   // Load all shared articles

    // Open filter modal
    $("#openFilterModalBtn").on("click", function () {
        $("#filterModal").show();
    });

    // Close filter modal
    $(".closeBtn").on("click", function () {
        $("#filterModal").hide();
    });

    // Apply filter and reload articles
    $("#applyUserFilterBtn").on("click", function () {
        $("#filterModal").hide();
        fetchSharedArticles();
    });

    // Handle article report
    $(document).on("click", ".reportBtn", function () {
        const sharedId = $(this).data("id");
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

        console.log("Reporting sharedId:", sharedId); // ← תראי אם זה NULL או לא קיים

        if (!currentUser) {
            alert("❌ You must be logged in.");
            return;
        }

        if (!confirm("Are you sure you want to report this article as offensive?")) return;

        const reportData = {
            reporterUserId: currentUser.id,
            sharedArticleId: sharedId
        };

        ajaxCall("POST", `${apiUrl}/report`, JSON.stringify(reportData),
            res => alert("✅ Report submitted. Thank you!"),
            err => {
                if (err.responseText.includes("your own")) {
                    alert("❌ You can't report your own article.");
                } else {
                    alert("❌ Failed to report article: " + (err.responseText || err.statusText));
                }
            }

        );
    });



    // Return list of selected user IDs to hide
    function getHiddenUserIds() {
        return $(".userCheckbox:checked")
            .map(function () { return $(this).val(); })
            .get()
            .join(",");
    }

    // Fetch and display shared articles, optionally filtering by hidden user IDs
    function fetchSharedArticles() {
        const hiddenIds = getHiddenUserIds();
        const url = hiddenIds
            ? `${apiUrl}?hiddenUserIds=${hiddenIds}`
            : apiUrl;

        ajaxCall("GET", url, null,
            res => renderSharedArticles(res),
            err => console.error("❌ Failed to fetch shared articles:", err.responseText || err.statusText)
        );
    }

    // Render shared article cards on the page
    function renderSharedArticles(articles) {
        const container = $("#sharedArticlesContainer");
        container.empty();

        if (articles.length === 0) {
            container.append(`<p>No shared articles available.</p>`);
            return;
        }

        articles.forEach(a => {

            console.log("Shared article object:", a);
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
                <button class="reportBtn" data-id="${a.sharedId}">🚫 Report as Offensive</button>
            </div>
        `;
            container.append(cardHtml);
        });
    }

    // Fetch all users and populate the modal with checkboxes
    function fetchAllUsers() {
        ajaxCall("GET", getAllUsersUrl, null,
            res => {
                const usersContainer = $("#userCheckboxList");
                usersContainer.empty();

                res.forEach(u => {
                    if (u.name.toLowerCase() !== "admin") {
                        usersContainer.append(`
                        <label>
                            <input type="checkbox" class="userCheckbox" value="${u.id}" />
                            ${u.name}
                        </label>
                    `);
                    }
                });
            },
            err => console.error("Failed to load users", err)
        );
    }
});