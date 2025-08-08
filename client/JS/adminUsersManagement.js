let users = [];

/* Detect if running locally (development) */
function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}` // Local API URL
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1"; // Deployed API URL
const baseUrl = `${baseApiUrl}/api/Users`;

/* Check if a user is an Admin (safe against property name variations) */
function isAdminUser(u) {
    if (!u) return false;
    const role = (u.role || u.Role || "").toString().toLowerCase();
    const name = (u.userName || u.username || u.name || "").toString().toLowerCase();
    return u.isAdmin === true || role === "admin" || name === "admin";
}

/* Fetch all users from the server */
function loadUsers() {
    ajaxCall(
        "GET",
        `${baseUrl}/getAllUseresAdmin`,
        null,
        res => {
            users = Array.isArray(res) ? res : [];
            renderUsers();
        },
        err => {
            console.error("Failed to load users:", err);
            alert("Couldn't fetch users.");
        }
    );
}

/* Toggle active/inactive status for a user (skips admins) */
function toggleUserActive(userId) {
    const user = users.find(u => u.id == userId);
    if (!user) return;

    // Protect admin accounts
    if (isAdminUser(user)) {
        alert("Admin user is protected and cannot be modified.");
        return;
    }

    const payload = { id: user.id, active: !user.active };
    ajaxCall(
        "PUT",
        `${baseUrl}/update-status`,
        JSON.stringify(payload),
        () => loadUsers(),
        err => {
            console.error("Failed to update user:", err);
            alert("Failed to update status.");
        }
    );
}

/* Reset all users to default state (server-side) */
function handleResetToDefault() {
    ajaxCall(
        "POST",
        `${baseUrl}/reset-to-default`,
        null,
        () => loadUsers(),
        err => {
            console.error("Reset failed:", err);
            alert("Failed to reset.");
        }
    );
}

/* Delete all users (server-side) */
function handleClearAllData() {
    ajaxCall(
        "DELETE",
        `${baseUrl}/delete-all`,
        null,
        () => loadUsers(),
        err => {
            console.error("Delete-all failed:", err);
            alert("Failed to delete users.");
        }
    );
}

/* Render users table (filters out admins from display) */
function renderUsers() {
    const container = document.getElementById('userTableContainer');
    if (!container) return;
    container.innerHTML = '';

    // Only show non-admin users in table
    const visibleUsers = users.filter(u => !isAdminUser(u));

    if (visibleUsers.length === 0) {
        container.innerHTML = "<p style='text-align:center'>No user data to display.</p>";
        return;
    }

    const table = document.createElement('table');

    // Build header
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    ['ID', 'Name', 'Email', 'Status', 'Action'].forEach(text => {
        const th = document.createElement('th');
        th.textContent = text;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    // Build body rows
    const tbody = document.createElement('tbody');
    visibleUsers.forEach(user => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${user.id ?? ''}</td>
            <td>${user.name ?? user.userName ?? ''}</td>
            <td>${user.email ?? ''}</td>
            <td class="${user.active ? 'status-active' : 'status-inactive'}">
                ${user.active ? 'Active' : 'Inactive'}
            </td>
            <td>
                <button class="${user.active ? 'deactivate' : 'activate'}"
                        data-user-id="${user.id}">
                    ${user.active ? 'Deactivate' : 'Activate'}
                </button>
            </td>`;
        tbody.appendChild(tr);
    });
    table.appendChild(tbody);
    container.appendChild(table);

    // Handle button clicks in table
    table.addEventListener('click', e => {
        if (e.target.tagName === 'BUTTON' && e.target.dataset.userId) {
            toggleUserActive(e.target.dataset.userId);
        }
    });
}

/* Initialize page: load users & set button events */
function init() {
    loadUsers();
    document.getElementById('resetToDefaultButton')?.addEventListener('click', handleResetToDefault);
    document.getElementById('clearAllDataButton')?.addEventListener('click', handleClearAllData);
}

document.addEventListener('DOMContentLoaded', init);
