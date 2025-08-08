// ===== Detect environment (same pattern as other pages) =====
function isDevEnv() {
    return location.host.includes("localhost"); // True if running on localhost
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}` // Local API URL
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1"; // Deployed API URL
// const baseUrl = `${baseApiUrl}/api/...`; // Uncomment if this page calls an API later

// ===== Page-specific auth override (no duplication of global helpers) =====
(function allowAdminOnThisPageOnly() {
    // This replaces the global requireLogin from auth.js
    // so we can customize rules for this page.

    window.requireLogin = function () {
        // Use the global getCurrentUser() if it exists
        const user = (typeof getCurrentUser === "function") ? getCurrentUser() : null;

        // If not logged in → send to login page
        if (!user) {
            location.href = "login.html";
            return;
        }

        // Determine if user is admin (by name, case-insensitive)
        const isAdmin = user.name && user.name.toLowerCase() === "admin";

        if (isAdmin) {
            // Admin is allowed → stay on this page
            return;
        }

        // If not admin → redirect to home
        location.href = "index.html";
    };
})();
