// When the page finishes loading
document.addEventListener("DOMContentLoaded", () => {
    requireLogin();         // Ensure user is logged in
    updateAuthButton();     // Update login/logout button
    loadAdminStats();       // Load and display admin statistics
});
// checks if we're running in local development environment
function isDevEnv() {
    return location.host.includes("localhost");
}

// sets the base API URL depending on the environment
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/Users`;
// Fetches admin statistics from the server and displays them
function loadAdminStats() {
    fetch(`${baseUrl}/stats`)
        .then(response => {
            if (!response.ok) throw new Error("Failed to fetch"); // Handle failed request
            return response.json(); // Parse JSON response
        })
        .then(stats => {
            // === Update HTML elements with numeric stats ===
            document.getElementById("loginCount").textContent = stats.loginCounter;
            document.getElementById("apiFetchCount").textContent = stats.apiFetchCounter;
            document.getElementById("savedCount").textContent = stats.savedNewsCounter;

            // === Create a bar chart using Chart.js ===
            // This is not an HTML <table> — it's a visual chart rendered inside a <canvas> element.
            const ctx = document.getElementById('statsChart').getContext('2d');
            new Chart(ctx, {
                type: 'bar', // Bar chart type
                data: {
                    labels: ['Logins', 'API Fetches', 'Articles Saved'], // X-axis labels
                    datasets: [{
                        label: `Usage on ${stats.date}`, // Dataset title (shown above the chart)
                        data: [
                            stats.loginCounter,         // Height of 'Logins' bar
                            stats.apiFetchCounter,      // Height of 'API Fetches' bar
                            stats.savedNewsCounter      // Height of 'Articles Saved' bar
                        ],
                        borderWidth: 1 // Thickness of bar borders
                    }]
                },
                options: {
                    scales: {
                        y: {
                            beginAtZero: true, // Start Y-axis at 0
                            ticks: {
                                precision: 0   // Force integer values on Y-axis
                            }
                        }
                    }
                }
            });
        })
        .catch(err => {
            console.error("❌ Failed to load admin stats", err);
            alert("Failed to load admin stats");
        });
}
