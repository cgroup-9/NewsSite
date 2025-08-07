const port = 7019;
const baseApiUrl = location.host.includes("localhost")
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

const apiUrl = `${baseApiUrl}/api/SharedArticle`;
const getAllUsersUrl = `${baseApiUrl}/api/Users`;

let currentPage = 1;
const pageSize = 12;

$(document).ready(() => {
    fetchAllUsers();         // Load all users into the modal
    fetchSharedArticles();   // Load all shared articles

    $("#openFilterModalBtn").on("click", () => $("#filterModal").show());
    $(".closeBtn").on("click", () => $("#filterModal").hide());

    $("#applyUserFilterBtn").on("click", function () {
        $("#filterModal").hide();
        currentPage = 1; // reset to first page when filter changes
        fetchSharedArticles();
    });

    $(document).on("click", ".reportBtn", function () {
        const sharedId = $(this).data("id");
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

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

    function getHiddenUserIds() {
        return $(".userCheckbox:checked")
            .map(function () { return $(this).val(); })
            .get()
            .join(",");
    }

    function fetchSharedArticles() {
        const hiddenIds = getHiddenUserIds();
        const queryParams = new URLSearchParams();

        if (hiddenIds) queryParams.append("hiddenUserIds", hiddenIds);
        queryParams.append("page", currentPage);
        queryParams.append("pageSize", pageSize);

        const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));
        if (currentUser?.id) {
            queryParams.append("userId", currentUser.id);
        }

        const url = `${apiUrl}?${queryParams.toString()}`;

        ajaxCall("GET", url, null,
            res => {
                renderSharedArticles(res);

                // Determine if there are more pages
                const hasNextPage = res.length === pageSize;
                const totalPages = hasNextPage ? currentPage + 1 : Math.max(currentPage, 1);

                console.log("Calling renderPagination with:", currentPage, totalPages);
                console.log("res.length =", res.length);
                console.log("pageSize =", pageSize);

                renderPagination(currentPage, totalPages, (page) => {
                    currentPage = page;
                    console.log("Rendering pagination. Current:", currentPage, "Total:", totalPages);

                    fetchSharedArticles();
                });
            },
            err => console.error("❌ Failed to fetch shared articles:", err.responseText || err.statusText)
        );
    }

    function renderSharedArticles(articles) {
        const container = $("#sharedArticlesContainer");
        container.empty();

        if (articles.length === 0) {
            container.append(`<p>No shared articles available.</p>`);
            return;
        }

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


    function fetchAllUsers() {
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser")); // Get logged-in user

        ajaxCall("GET", getAllUsersUrl, null,
            res => {
                const usersContainer = $("#userCheckboxList");
                usersContainer.empty();

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

    $(document).on("click", ".likeToggleBtn", function () {
        const btn = $(this);
        const sharedId = btn.data("id");
        const isLiked = btn.data("liked");
        const currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

        if (!currentUser) {
            alert("❌ You must be logged in to like/unlike.");
            return;
        }

        const data = {
            sharedArticleId: sharedId,
            userId: currentUser.id
        };

        const endpoint = isLiked ? "unlike" : "like";

        ajaxCall("POST", `${apiUrl}/${endpoint}`, JSON.stringify(data),
            res => {
                const counterElem = $(`#likeCount-${sharedId}`);
                let count = parseInt(counterElem.text()) || 0;

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
