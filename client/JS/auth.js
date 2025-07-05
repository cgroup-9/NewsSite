// Retrieves the current user from session storage, or returns null if not found
function getCurrentUser() {
    const json = sessionStorage.getItem("currentUser");
    return json ? JSON.parse(json) : null;
}

// Redirects to login page if user is not logged in (unless already on login or index)
// If user is admin, redirects to admin panel
function requireLogin() {
    const user = getCurrentUser();

    if (!location.pathname.endsWith("index.html") &&
        !location.pathname.endsWith("login.html") &&
        !user) {
        // Not logged in and not on login or index -> redirect to login
        location.href = "login.html";
        return;
    }

    // If logged in as admin and not on admin page -> redirect to admin page
    if (
        user &&
        user.name?.toLowerCase() === "admin" &&
        !location.pathname.endsWith("adminIndex.html") &&
        !location.pathname.endsWith("adminUsersManagement.html")
    ) {
        location.href = "adminIndex.html";
    }

}

// Logs out the current user: clears session storage, shows alert, redirects to homepage
function logout() {
    sessionStorage.clear();
    alert("Logged out successfully!");
    location.href = "index.html";
}

// Updates the login/logout button depending on user state
function updateAuthButton() {
    const btn = document.getElementById("authBtn");
    const user = getCurrentUser();

    if (!btn) return;

    if (user) {
        // If user is logged in, show logout with their name
        btn.textContent = `🚪 Logout (${user.name})`;
        btn.onclick = logout;
    } else {
        // If not logged in, show login button
        btn.textContent = "🔐 Login";
        btn.onclick = () => location.href = "login.html";
    }
}

// When page loads, check if user is logged in and update the auth button
document.addEventListener("DOMContentLoaded", () => {
    requireLogin();
    updateAuthButton();
});
