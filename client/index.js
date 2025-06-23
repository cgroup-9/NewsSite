$(document).ready(() => {
    const btnLoad = $("#loadMovies");
    const divCards = $("#movieCard");

    function isDevEnv() {
        return location.host.includes("localhost");
    }

    const port = 7110;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
    const baseUrl = `${baseApiUrl}/api/Movies`;

    let movies = [];
    let currentPage = 1;
    const pageSize = 20;
    let totalPages = 1;

    function renderSearchBox() {
        $("#searchByFilter").html(`
            <div class="searchBy">
                <div class="searchBox">
                    <input type="text" id="titleInput" class="searchTextBox" placeholder="Search by title">
                    <button id="titleSearchBtn">🔍</button>
                </div>
                <div class="searchBox">
                    <input type="date" id="startDateInput" class="searchTextBox">
                    <input type="date" id="endDateInput" class="searchTextBox">
                    <button id="dateSearchBtn">🔍</button>
                </div>
            </div>
        `);
    }

    function loadPagedMovies(page) {
        ajaxCall("GET", `${baseUrl}/paged?page=${page}&pageSize=${pageSize}`, null,
            res => {
                movies = res.movies;
                totalPages = Number(res.totalPages);
                currentPage = page;

                renderSearchBox();
                $(".fullRowTitle").remove();
                $("#searchByFilter").after('<h2 class="fullRowTitle">All Movies</h2>');
                $("#movieCard").empty();

                renderMovies(movies);
                renderPagination();
            },
            err => alert("❌ Failed to load movies: " + (err.responseText || err.statusText))
        );
    }

    function renderMovies(movies) {
        for (let m of movies) {
            const cardHtml = `
            <div class="card">
                <div class="topcard">
                    <button class="btnaddcart" data-id="${m.id}" data-price="${m.priceToRent ?? 40}">Rent me</button>
                    <p class="rating">★${m.averageRating}/10</p>
                </div>
                <img class="movieimg" src="${m.primaryImage}" />
                <h2>${m.primaryTitle}</h2>
                <div class="shortinfo">
                    <p class="year">${m.startYear || new Date(m.releaseDate).getFullYear()}</p>
                    <p class="time">${m.runtimeMinutes} min</p>
                    <p class="isAdult">${m.isAdult ? "+18" : "All"}</p>
                </div>
                <p class="description">${m.description}</p>
                <div class="geners">
                    ${(m.genres || "").split(',').map(g => `<p class="interests">${g.trim()}</p>`).join("")}
                </div>
                <div class="financial">
                    <div class="budget"><h2>Budget</h2><p>${m.budget}</p></div>
                    <div class="boxoffice"><h2>Box Office</h2><p>$${m.grossWorldwide}M</p></div>
                    <div class="votes"><h2>Votes</h2><p>${m.numVotes}</p></div>
                </div>
            </div>`;
            divCards.append(cardHtml);
        }
    }


    function renderPagination() {
        const container = $("#paginationContainer");
        container.empty();

        if (isNaN(totalPages) || totalPages <= 1) return;

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

    // ✅ פותח את מודל ההשכרה
    function openRentModal(movieId, priceToRent) {
        const modal = $("#rentModal");
        const today = new Date().toISOString().split("T")[0];

        modal.html(`
        <div class="modal-content">
            <span class="close">&times;</span>
            <h3>📅 Rent Movie</h3>
            <p>Choose rental dates:</p>
            <input type="date" id="rentStart" value="${today}"><br><br>
            <input type="date" id="rentEnd"><br><br>
            <p id="priceSummary" style="font-weight:bold;"></p>
            <button id="confirmRentBtn">Confirm Rent</button>
        </div>
    `);

        modal.show();

        $(".close").click(() => modal.hide());

        // עידכון מחיר דינאמי
        function updatePriceSummary() {
            const start = new Date($("#rentStart").val());
            const end = new Date($("#rentEnd").val());
            const msPerDay = 1000 * 60 * 60 * 24;
            const days = Math.ceil((end - start) / msPerDay);

            if (days > 0) {
                const total = days * priceToRent;
                $("#priceSummary").text(`Total price: ₪${total} (${days} days × ₪${priceToRent})`);
            } else {
                $("#priceSummary").text("");
            }
        }

        $("#rentStart, #rentEnd").on("change", updatePriceSummary);

        $("#confirmRentBtn").click(() => {
            const user = getCurrentUser(); 

            if (!user) return alert("⛔ Please login first.");

            const rentStart = $("#rentStart").val();
            const rentEnd = $("#rentEnd").val();

            const days = Math.ceil((new Date(rentEnd) - new Date(rentStart)) / (1000 * 60 * 60 * 24));
            const totalPrice = days * priceToRent;

            if (!rentStart || !rentEnd || days <= 0) {
                return alert("❌ Invalid date range.");
            }

            const rent = {
                userId: user.id,
                movieId,
                rentStart,
                rentEnd,
                totalPrice
            };

            ajaxCall("POST", `${baseUrl}/rent`, JSON.stringify(rent),
                res => {
                    alert(res.message || "✅ Rental successful!");
                    modal.hide();
                },
                err => alert(err.responseJSON?.message || "❌ Rental failed.")
            );
        });
    }


    $(document).on("click", ".btnaddcart", function () {
        const movieId = $(this).data("id");
        const rawPrice = $(this).data("price");

        const price = (rawPrice !== undefined && rawPrice !== null) ? rawPrice : 40;

        openRentModal(movieId, price);
    });


    // דפדוף בין עמודים
    $(document).on("click", ".paginationBtn", function () {
        const page = $(this).data("page");
        if (page && page !== currentPage) {
            loadPagedMovies(page);
        }
    });

    // התחלה
    btnLoad.click(() => loadPagedMovies(1));

    // האזנה לחיפוש לפי כותרת
    $(document).on("click", "#titleSearchBtn", function () {
        const title = $("#titleInput").val().trim();
        if (!title) return alert("❗ Please enter a title to search.");

        ajaxCall("GET", `${baseUrl}/search?title=${encodeURIComponent(title)}`, null,
            res => {
                movies = res;
                totalPages = 1;
                currentPage = 1;

                $(".fullRowTitle").remove();
                $("#searchByFilter").after(`<h2 class="fullRowTitle">Results for "${title}"</h2>`);
                $("#movieCard").empty();
                renderMovies(movies);
                renderPagination();
            },
            err => alert("❌ Search failed: " + (err.responseText || err.statusText))
        );
    });

    // האזנה לחיפוש לפי תאריכים
    $(document).on("click", "#dateSearchBtn", function () {
        const startDate = $("#startDateInput").val();
        const endDate = $("#endDateInput").val();

        if (!startDate || !endDate) {
            return alert("❗ Please select both start and end dates.");
        }

        ajaxCall("GET", `${baseUrl}/searchByPath/startDate/${startDate}/endDate/${endDate}`, null,
            res => {
                movies = res;
                totalPages = 1;
                currentPage = 1;

                $(".fullRowTitle").remove();
                $("#searchByFilter").after(`<h2 class="fullRowTitle">Movies released between ${startDate} and ${endDate}</h2>`);
                $("#movieCard").empty();
                renderMovies(movies);
                renderPagination();
            },
            err => alert("❌ Date search failed: " + (err.responseText || err.statusText))
        );
    });

});
