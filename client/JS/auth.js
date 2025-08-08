// Retrieves the current user from sessionStorage (returns null if not found)
function getCurrentUser() {
    const json = sessionStorage.getItem("currentUser"); // Get stored JSON string
    return json ? JSON.parse(json) : null; // Parse to object if exists
}

// Enforces login rules and redirects if needed
function requireLogin() {
    const user = getCurrentUser(); // Get logged-in user object

    // If not logged in AND not on index or login page → redirect to login
    if (!location.pathname.endsWith("index.html") &&
        !location.pathname.endsWith("login.html") &&
        !user) {
        alert("You must be logged in.");
        location.href = "login.html";
        return;
    }

    // If logged in as admin AND not already on an admin page → redirect to admin dashboard
    if (
        user &&
        user.name?.toLowerCase() === "admin" &&
        !location.pathname.endsWith("adminIndex.html") &&
        !location.pathname.endsWith("adminUsersManagement.html") &&
        !location.pathname.endsWith("adminReportedComments.html") &&
        !location.pathname.endsWith("adminAiQuestions.html")
    ) {
        location.href = "adminIndex.html";
    }
}

// Logs the user out: clears session, alerts, redirects to home
function logout() {
    sessionStorage.clear(); // Remove all stored session data
    alert("Logged out successfully!");
    location.href = "index.html"; // Redirect to homepage
}

// Updates the login/logout button based on current user state
function updateAuthButton() {
    const btn = document.getElementById("authBtn"); // Auth button element
    const user = getCurrentUser(); // Current logged-in user

    if (!btn) return; // Exit if button is missing

    if (user) {
        // Show logout with user name
        btn.textContent = `🚪 Logout (${user.name})`;
        btn.onclick = logout;
    } else {
        // Show login button
        btn.textContent = "🔐 Login";
        btn.onclick = () => location.href = "login.html";
    }
}

// On page load: enforce login rules and update button state
document.addEventListener("DOMContentLoaded", () => {
    requireLogin();
    updateAuthButton();
});
