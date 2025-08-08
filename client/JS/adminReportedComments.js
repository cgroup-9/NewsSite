// Holds the list of reported articles fetched from the server.
// Starts empty and is populated by loadReports().
let reportedArticles = [];

// Utility: returns true when the app runs on localhost (dev environment).
// Used to decide which base URL (dev/prod) to call.
function isDevEnv() {
    return location.host.includes("localhost");
}

// Build the base API URL according to environment.
// In dev -> https://localhost:7019 ; otherwise -> production URL.
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

// Concrete endpoint that returns all reported shared-article comments.
const reportsApiUrl = `${baseApiUrl}/api/SharedArticle/get-reported`;

// Loads reported comments from the server and then renders them.
// Side effects:
// 1) Logs request/response to the console (useful for debugging).
// 2) Updates the global reportedArticles array.
// 3) Calls renderReports() to show data in the table.
function loadReports() {
    console.log("📡 Sending GET request to:", reportsApiUrl);

    ajaxCall("GET", reportsApiUrl, null,
        res => {
            console.log("✅ Response from server:", res);

            // Normalize the response to always be an array.
            // (Some APIs may return a single object when there’s one item; this keeps code simple.)
            reportedArticles = Array.isArray(res) ? res : [res];

            // After data is ready, render the table.
            renderReports();
        },
        err => {
            // Network/HTTP error handling path.
            console.error("❌ Failed to load reports:", err);
            alert("❌ Couldn't fetch reported articles.");
        }
    );
}

// Renders the reports into an HTML table inside #reportsTableContainer.
// Handles three cases:
// 1) Missing container in DOM (logs + alert and exits).
// 2) No data (shows 'No reports found' message).
// 3) Has data (builds table head + body).
function renderReports() {
    const container = document.getElementById('reportsTableContainer');
    if (!container) {
        console.error("📛 Element #reportsTableContainer not found in DOM!");
        alert("Container for reports not found.");
        return;
    }

    // Clear any previous content before re-rendering.
    container.innerHTML = '';

    // Empty-state UI: when there are no reported articles to show.
    if (!reportedArticles || reportedArticles.length === 0) {
        const msg = document.createElement('p');
        msg.innerHTML = "No reports found."; // Simple text message to user
        msg.style.textAlign = "center";
        container.appendChild(msg);
        return;
    }

    // Create the main table element and give it a CSS class for styling.
    const table = document.createElement('table');
    table.className = "reports-table";

    // ====== Build table header ======
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');

    // Column labels shown at the top of the table.
    ['The Offensive Comment', 'Shared By', 'User ID', 'Reported On'].forEach(text => {
        const th = document.createElement('th');
        th.textContent = text;
        headerRow.appendChild(th);
    });

    thead.appendChild(headerRow);
    table.appendChild(thead);

    // ====== Build table body ======
    const tbody = document.createElement('tbody');

    // For each report, add a new row with the relevant fields.
    reportedArticles.forEach(report => {
        console.log("📄 Report loaded:", report);

        const tr = document.createElement('tr');

        // Using template literal for readability.
        // `toLocaleString()` formats the date in a user-friendly way based on the browser locale.
        tr.innerHTML = `
                <td>${report.comment}</td>
                <td>${report.sharedByUserName}</td>
                <td>${report.sharedByUserId}</td>
                <td>${new Date(report.reportDate).toLocaleString()}</td>
            `;

        tbody.appendChild(tr);
    });

    table.appendChild(tbody);
    container.appendChild(table);
}

// Entry point: runs once the DOM is ready.
// Calls loadReports() to kick off data fetching + rendering flow.
function init() {
    console.log("🧪 init started");
    loadReports();
}

// Hook the init flow to the DOMContentLoaded event so the DOM exists before we render into it.
document.addEventListener('DOMContentLoaded', init);
