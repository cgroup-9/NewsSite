    let reportedArticles = [];

    function isDevEnv() {
        return location.host.includes("localhost");
    }

    const port = 7019;
    const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

    const reportsApiUrl = `${baseApiUrl}/api/SharedArticle/get-reported`;

    // Loads reported comments from the server
    function loadReports() {
        console.log("📡 Sending GET request to:", reportsApiUrl);

    ajaxCall("GET", reportsApiUrl, null,
            res => {
        console.log("✅ Response from server:", res);
    reportedArticles = Array.isArray(res) ? res : [res]; // Ensure it's always an array
    renderReports();
            },
            err => {
        console.error("❌ Failed to load reports:", err);
    alert("❌ Couldn't fetch reported articles.");
            }
    );
    }

    // Renders the reports into a table
    function renderReports() {
        const container = document.getElementById('reportsTableContainer');
    if (!container) {
        console.error("📛 Element #reportsTableContainer not found in DOM!");
    alert("Container for reports not found.");
    return;
        }

    container.innerHTML = '';

    if (!reportedArticles || reportedArticles.length === 0) {
            const msg = document.createElement('p');
    msg.innerHTML = "No reports found.";
    msg.style.textAlign = "center";
    container.appendChild(msg);
    return;
        }

    const table = document.createElement('table');
    table.className = "reports-table";

    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
        ['The Offensive Comment', 'Shared By', 'User ID', 'Reported On'].forEach(text => {
            const th = document.createElement('th');
    th.textContent = text;
    headerRow.appendChild(th);
        });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    const tbody = document.createElement('tbody');

        reportedArticles.forEach(report => {
        console.log("📄 Report loaded:", report);
    const tr = document.createElement('tr');
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

    // Runs when the page loads
    function init() {
        console.log("🧪 init started");
    loadReports();
    }

    document.addEventListener('DOMContentLoaded', init);
