// === Run when page finishes loading ===
document.addEventListener("DOMContentLoaded", () => {
    requireLogin();     // 1️⃣ Check if the user is logged in — if not, redirect to login page.
    updateAuthButton(); // 2️⃣ Set the top-right auth button text/action (Login or Logout).
    loadAdminStats();   // 3️⃣ Fetch today's statistics from the server and display them.
});

// === Detect environment ===
// Returns true if running locally (localhost), otherwise false.
function isDevEnv() {
    return location.host.includes("localhost");
}

// === Set base API URL depending on environment ===
// Localhost → use local port for API calls.
// Deployed → use hosted project path.
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

// Base URL for user-related API endpoints.
const baseUrl = `${baseApiUrl}/api/Users`;

// === Fetch admin statistics from the server and display ===
function loadAdminStats() {
    // 1️⃣ Send GET request to fetch today's usage stats
    fetch(`${baseUrl}/stats`)
        .then(response => {
            // Check for network/HTTP errors
            if (!response.ok) throw new Error("Failed to fetch");
            return response.json(); // Parse JSON body
        })
        .then(stats => {
            // 2️⃣ Update numeric counters in the HTML
            document.getElementById("loginCount").textContent = stats.loginCounter;
            document.getElementById("apiFetchCount").textContent = stats.apiFetchCounter;
            document.getElementById("savedCount").textContent = stats.savedNewsCounter;

            // 3️⃣ Draw a bar chart with Chart.js
            const ctx = document.getElementById('statsChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar', // Bar chart visualization
                data: {
                    labels: ['Logins', 'API Fetches', 'Articles Saved'], // X-axis labels
                    datasets: [{
                        label: `Usage on ${stats.date}`, // Chart title (legend)
                        data: [
                            stats.loginCounter,    // Number of logins today
                            stats.apiFetchCounter, // Number of API calls today
                            stats.savedNewsCounter // Number of saved articles today
                        ],
                        borderWidth: 1 // Border around each bar
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true, // Start Y-axis at 0
                            ticks: {
                                precision: 0   // Show whole numbers only
                            }
                        }
                    }
                }
            });
        })
        .catch(err => {
            // 4️⃣ Handle failure gracefully
            console.error("❌ Failed to load admin stats", err);
            alert("Failed to load admin stats");
        });
}
