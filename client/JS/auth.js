function getCurrentUser() {
    const json = sessionStorage.getItem("currentUser");
    return json ? JSON.parse(json) : null;
}

function requireLogin() {
    const user = getCurrentUser();

    // אם לא מחובר ומנסה להיכנס לעמוד אחר מ-login/index → מפנה ל-login
    if (!location.pathname.endsWith("index.html") &&
        !location.pathname.endsWith("login.html") &&
        !user) {
        location.href = "login.html";
        return;
    }

    // אם מחובר כ-admin ונמצא לא בעמוד adminIndex → מפנה לשם
    if (user && user.name?.toLowerCase() === "admin" &&
        !location.pathname.endsWith("adminIndex.html")) {
        location.href = "adminIndex.html";
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
