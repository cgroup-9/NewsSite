let users = [];

/* Detect dev env */
function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/Users`;

/* Robust Admin detection (adapt to your model if needed) */
function isAdminUser(u) {
    if (!u) return false;
    const role = (u.role || u.Role || "").toString().toLowerCase();
    const name = (u.userName || u.username || u.name || "").toString().toLowerCase();
    return u.isAdmin === true || role === "admin" || name === "admin";
}

/* Fetch all users (server returns everyone) */
function loadUsers() {
    ajaxCall(
        "GET",
        `${baseUrl}/getAllUseresAdmin`,
        null,
        res => { users = Array.isArray(res) ? res : []; renderUsers(); },
        err => { console.error("Failed to load users:", err); alert("Couldn't fetch users."); }
    );
}

/* Toggle user active flag (protect against accidental admin ops anyway) */
function toggleUserActive(userId) {
    const user = users.find(u => u.id == userId);
    if (!user) return;

    // Safety guard: do nothing for admin, even if somehow clicked
    if (isAdminUser(user)) {
        alert("Admin user is protected and won't be shown/modified here.");
        return;
    }

    const payload = { id: user.id, active: !user.active };
    ajaxCall(
        "PUT",
        `${baseUrl}/update-status`,
        JSON.stringify(payload),
        () => loadUsers(),
        err => { console.error("Failed to update user:", err); alert("Failed to update status."); }
    );
}

/* Reset to default (server-side action) */
function handleResetToDefault() {
    ajaxCall(
        "POST",
        `${baseUrl}/reset-to-default`,
        null,
        () => loadUsers(),
        err => { console.error("Reset failed:", err); alert("Failed to reset."); }
    );
}

/* Delete all (server-side action) */
function handleClearAllData() {
    ajaxCall(
        "DELETE",
        `${baseUrl}/delete-all`,
        null,
        () => loadUsers(),
        err => { console.error("Delete-all failed:", err); alert("Failed to delete users."); }
    );
}

/* Render table — filter out Admin client-side */
function renderUsers() {
    const container = document.getElementById('userTableContainer');
    if (!container) return;
    container.innerHTML = '';

    // Filter out admin ONLY for this table
    const visibleUsers = users.filter(u => !isAdminUser(u));

    if (visibleUsers.length === 0) {
        container.innerHTML = "<p style='text-align:center'>No user data to display for this action.</p>";
        return;
    }

    const table = document.createElement('table');

    // Header
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    ['ID', 'Name', 'Email', 'Status', 'Action'].forEach(text => {
        const th = document.createElement('th');
        th.textContent = text;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    // Body
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

    // Event delegation for action buttons
    table.addEventListener('click', e => {
        if (e.target.tagName === 'BUTTON' && e.target.dataset.userId) {
            toggleUserActive(e.target.dataset.userId);
        }
    });
}

/* Init page */
function init() {
    loadUsers();
    document.getElementById('resetToDefaultButton')?.addEventListener('click', handleResetToDefault);
    document.getElementById('clearAllDataButton')?.addEventListener('click', handleClearAllData);
}

document.addEventListener('DOMContentLoaded', init);
