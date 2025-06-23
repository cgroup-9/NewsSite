function getCurrentUser() {
    const json = sessionStorage.getItem("currentUser");
    return json ? JSON.parse(json) : null;
}

function requireLogin() {
    if (!location.pathname.endsWith("index.html") &&
        !location.pathname.endsWith("login.html") &&
        !getCurrentUser()) {
        location.href = "login.html";
    }
}

function logout() {
    sessionStorage.clear();
    alert("Logged out successfully!");
    location.href = "index.html";
}

function updateAuthButton() {
    const btn = document.getElementById("authBtn");
    const user = getCurrentUser();

    if (!btn) return; 

    if (user) {
        btn.textContent = `🚪 Logout (${user.name})`;
        btn.onclick = logout;
    } else {
        btn.textContent = "🔐 Login";
        btn.onclick = () => location.href = "login.html";
    }
}

document.addEventListener("DOMContentLoaded", () => {
    requireLogin();
    updateAuthButton();
});
