function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/News`;

$(document).ready(() => {
    loadNews(); //render latest articles  on load without categories

    let articles = [];     
    let currentPage = 1;   
    const pageSize = 20;   //num of articles in page
    let totalPages = 1;    

    function loadNews(category = "") {
        const url = category   
            ? `${baseUrl}?country=us&categories=${category}`
            : `${baseUrl}?country=us`;    // if category==null so bring all articles

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

    function renderArticles(page) {
        const container = $("#newsContainer");
        container.empty();

        const startIndex = (page - 1) * pageSize;   //which page
        const endIndex = Math.min(startIndex + pageSize, articles.length);    //if last page-take the array length
        const pageArticles = articles.slice(startIndex, endIndex);    //slice array for current page

        if (pageArticles.length === 0) {
            container.html("<p>No articles found.<p>");
            return;
        }

        for (let a of pageArticles) {
            //const articleData = JSON.stringify(a).replace(/'/g, "&apos");

            const cardHtml = `
            <div class="articleCard" data-article='${JSON.stringify(a)}'>
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
    function renderPagination() {
        const container = $("#paginationContainer");
        container.empty();

        if (isNaN(totalPages) || totalPages <= 1) return;  //if less than 2 pages- no need

        let html = `<div id="pagination">`;

        if (currentPage > 1) {
            html += `<button class="paginationBtn" data-page="${currentPage - 1}">« הקודם</button>`;
        }

        for (let i = 1; i <= totalPages; i++) {
            html += `<button class="paginationBtn" data-page="${i}" ${i === currentPage ? "disabled" : ""}>${i}</button>`;
        }

        if (currentPage < totalPages) {
            html += `<button class="paginationBtn" data-page="${currentPage + 1}">הבא »</button>`;
        }

        html += `</div>`;
        container.html(html);
    }

    $(document).on("click", ".paginationBtn", function () {
        const page = $(this).data("page");   //which page was clicked

        if (page && page !== currentPage) {
            currentPage = page; 
            renderArticles(currentPage);
            renderPagination();
        }
    })

    $("#filterBtn").on("click", function () {
        const categories = $(".categoryCheckbox:checked").map(function () { return $(this).val(); }).get();
        if (categories.includes("") || categories.length === 0)
            loadNews();
        else {
            const categoryParam = categories.join(',');
            loadNews(categoryParam);
        }
    })

   
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

    //    // עידכון מחיר דינאמי
    //    function updatePriceSummary() {
    //        const start = new Date($("#rentStart").val());
    //        const end = new Date($("#rentEnd").val());
    //        const msPerDay = 1000 * 60 * 60 * 24;
    //        const days = Math.ceil((end - start) / msPerDay);

    //        if (days > 0) {
    //            const total = days * priceToRent;
    //            $("#priceSummary").text(`Total price: ₪${total} (${days} days × ₪${priceToRent})`);
    //        } else {
    //            $("#priceSummary").text("");
    //        }
    //    }

    //    $("#rentStart, #rentEnd").on("change", updatePriceSummary);

    //    $("#confirmRentBtn").click(() => {
    //        const user = getCurrentUser(); 

    //        if (!user) return alert("⛔ Please login first.");

    //        const rentStart = $("#rentStart").val();
    //        const rentEnd = $("#rentEnd").val();

    //        const days = Math.ceil((new Date(rentEnd) - new Date(rentStart)) / (1000 * 60 * 60 * 24));
    //        const totalPrice = days * priceToRent;

    //        if (!rentStart || !rentEnd || days <= 0) {
    //            return alert("❌ Invalid date range.");
    //        }

    //        const rent = {
    //            userId: user.id,
    //            movieId,
    //            rentStart,
    //            rentEnd,
    //            totalPrice
    //        };

    //        ajaxCall("POST", `${baseUrl}/rent`, JSON.stringify(rent),
    //            res => {
    //                alert(res.message || "✅ Rental successful!");
    //                modal.hide();
    //            },
    //            err => alert(err.responseJSON?.message || "❌ Rental failed.")
    //        );
    //    });
    //}





    //// דפדוף בין עמודים
    //$(document).on("click", ".paginationBtn", function () {
    //    const page = $(this).data("page");
    //    if (page && page !== currentPage) {
    //        loadPagedMovies(page);
    //    }
    //});

    //// התחלה
    //btnLoad.click(() => loadPagedMovies(1));

    //// האזנה לחיפוש לפי כותרת
    //$(document).on("click", "#titleSearchBtn", function () {
    //    const title = $("#titleInput").val().trim();
    //    if (!title) return alert("❗ Please enter a title to search.");

    //    ajaxCall("GET", `${baseUrl}/search?title=${encodeURIComponent(title)}`, null,
    //        res => {
    //            movies = res;
    //            totalPages = 1;
    //            currentPage = 1;

    //            $(".fullRowTitle").remove();
    //            $("#searchByFilter").after(`<h2 class="fullRowTitle">Results for "${title}"</h2>`);
    //            $("#movieCard").empty();
    //            renderMovies(movies);
    //            renderPagination();
    //        },
    //        err => alert("❌ Search failed: " + (err.responseText || err.statusText))
    //    );
    //});

    //// האזנה לחיפוש לפי תאריכים
    //$(document).on("click", "#dateSearchBtn", function () {
    //    const startDate = $("#startDateInput").val();
    //    const endDate = $("#endDateInput").val();

    //    if (!startDate || !endDate) {
    //        return alert("❗ Please select both start and end dates.");
    //    }

    //    ajaxCall("GET", `${baseUrl}/searchByPath/startDate/${startDate}/endDate/${endDate}`, null,
    //        res => {
    //            movies = res;
    //            totalPages = 1;
    //            currentPage = 1;

    //            $(".fullRowTitle").remove();
    //            $("#searchByFilter").after(`<h2 class="fullRowTitle">Movies released between ${startDate} and ${endDate}</h2>`);
    //            $("#movieCard").empty();
    //            renderMovies(movies);
    //            renderPagination();
    //        },
    //        err => alert("❌ Date search failed: " + (err.responseText || err.statusText))
    //    );
    //});

});
