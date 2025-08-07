// ===== env (same pattern as other pages) =====
function isDevEnv() {
    return location.host.includes("localhost");
}
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
// const baseUrl = `${baseApiUrl}/api/...`; // use if needed later

// ===== page-only auth behavior (no duplication of helpers) =====
(function allowAdminOnThisPageOnly() {
    // Override the global requireLogin that auth.js will call on DOMContentLoaded.
    // We rely on auth.js's getCurrentUser() that already exists.
    window.requireLogin = function () {
        const user = (typeof getCurrentUser === "function") ? getCurrentUser() : null;

        // Not logged in -> go to login (same behavior as global)
        if (!user) {
            location.href = "login.html";
            return;
        }

        // Keep the SAME admin check as in auth.js (by name)
        const isAdmin = user.name && user.name.toLowerCase() === "admin";

        if (isAdmin) {
            // Admin is allowed to stay on AdvancedFeatures; do nothing.
            return;
        }

        // Non-admin users shouldn't view this admin page
        location.href = "index.html";
    };
})();
