const port = 7019;
// Choose base API URL depending on whether we're running locally or on the server
const baseApiUrl = location.host.includes("localhost")
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

// Define specific API endpoints for shared articles and users
const apiUrl = `${baseApiUrl}/api/SharedArticle`;
const getAllUsersUrl = `${baseApiUrl}/api/Users`;

// Pagination variables
let currentPage = 1;
const pageSize = 12;

$(document).ready(() => {
    fetchAllUsers();         // Load all users into the filter modal
    fetchSharedArticles();   // Load all shared articles on page load

    // Open the filter modal
    $("#openFilterModalBtn").on("click", () => $("#filterModal").show());

    // Close the filter modal
    $(".closeBtn").on("click", () => $("#filterModal").hide());

    // Apply the user filter and reload articles from page 1
    $("#applyUserFilterBtn").on("click", function () {
        $("#filterModal").hide();
        currentPage = 1; // Reset pagination to first page when filter changes
        fetchSharedArticles();
    });

    // Handle "Report as Offensive" button click
    $(document).on("click", ".reportBtn", function () {
        const sharedId = $(this).data("id"); // Get the article's shared ID
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser")); // Logged-in user

        // Confirm before reporting
        if (!confirm("Are you sure you want to report this article as offensive?")) return;

        const reportData = {
            reporterUserId: currentUser.id,
            sharedArticleId: sharedId
        };

        // Send POST request to report API
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

    // Get a comma-separated list of selected user IDs to filter out
    function getHiddenUserIds() {
        return $(".userCheckbox:checked")
            .map(function () { return $(this).val(); })
            .get()
            .join(",");
    }

    // Fetch shared articles from the server with filters and pagination
    function fetchSharedArticles() {
        const hiddenIds = getHiddenUserIds(); // Users to hide
        const queryParams = new URLSearchParams();

        // Add filters and pagination parameters to the request
        if (hiddenIds) queryParams.append("hiddenUserIds", hiddenIds);
        queryParams.append("page", currentPage);
        queryParams.append("pageSize", pageSize);

        // Include current logged-in user ID (for like/report permissions)
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));
        if (currentUser?.id) {
            queryParams.append("userId", currentUser.id);
        }

        const url = `${apiUrl}?${queryParams.toString()}`;

        // Make GET request to fetch shared articles
        ajaxCall("GET", url, null,
            res => {
                renderSharedArticles(res); // Render cards in UI

                // Check if there are more pages to determine pagination
                const hasNextPage = res.length === pageSize;
                const totalPages = hasNextPage ? currentPage + 1 : Math.max(currentPage, 1);

                // Render pagination controls
                renderPagination(currentPage, totalPages, (page) => {
                    currentPage = page;
                    fetchSharedArticles();
                });
            },
            err => console.error("❌ Failed to fetch shared articles:", err.responseText || err.statusText)
        );
    }

    // Render shared articles as cards in the page
    function renderSharedArticles(articles) {
        const container = $("#sharedArticlesContainer");
        container.empty();

        // Show a message if there are no shared articles
        if (articles.length === 0) {
            container.append(`<p>No shared articles available.</p>`);
            return;
        }

        // Loop through each article and build HTML
        articles.forEach(a => {
            const isLiked = a.alreadyLiked;
            const likeBtnHtml = `
            <button class="likeToggleBtn" 
                    data-id="${a.sharedId}" 
                    data-liked="${isLiked}">
                ${isLiked ? "💔 Unlike" : "❤️ Like"}
            </button>`;

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

                <div class="likeSection">
                    ${likeBtnHtml}
                    <span class="likeCount" id="likeCount-${a.sharedId}">${a.likesCount}</span>
                </div>

                <button class="reportBtn" data-id="${a.sharedId}">🚫 Report as Offensive</button>
            </div>`;

            container.append(cardHtml);
        });
    }

    // Fetch all users from the server for the filter modal
    function fetchAllUsers() {
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser")); // Get logged-in user

        ajaxCall("GET", getAllUsersUrl, null,
            res => {
                const usersContainer = $("#userCheckboxList");
                usersContainer.empty();

                // Add a checkbox for each user except "admin" and current user
                res.forEach(u => {
                    if (u.name.toLowerCase() !== "admin" && u.id !== currentUser.id) {
                        usersContainer.append(`
                    <label>
                        <input type="checkbox" class="userCheckbox" value="${u.id}" />
                        ${u.name}
                    </label>`);
                    }
                });
            },
            err => console.error("Failed to load users", err)
        );
    }

    // Handle like/unlike button clicks
    $(document).on("click", ".likeToggleBtn", function () {
        const btn = $(this);
        const sharedId = btn.data("id"); // Article ID
        const isLiked = btn.data("liked"); // Current state (liked or not)
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

        // Block if not logged in
        if (!currentUser) {
            alert("❌ You must be logged in to like/unlike.");
            return;
        }

        const data = {
            sharedArticleId: sharedId,
            userId: currentUser.id
        };

        const endpoint = isLiked ? "unlike" : "like";

        // Send like/unlike request to server
        ajaxCall("POST", `${apiUrl}/${endpoint}`, JSON.stringify(data),
            res => {
                const counterElem = $(`#likeCount-${sharedId}`);
                let count = parseInt(counterElem.text()) || 0;

                // Update count and button state in UI
                if (isLiked) {
                    count--;
                    btn.html("❤️ Like");
                    btn.data("liked", false);
                } else {
                    count++;
                    btn.html("💔 Unlike");
                    btn.data("liked", true);
                }

                counterElem.text(count);
            },
            err => {
                alert("❌ Failed to process like/unlike: " + (err.responseText || err.statusText));
            }
        );
    });
});
