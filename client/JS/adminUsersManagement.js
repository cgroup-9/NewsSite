let users = [];

function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv() ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/Users`;

function loadUsers() {
    ajaxCall("GET", `${baseUrl}/getAllUseresAdmin`, null,
        res => { users = res; renderUsers(); },
        err => { console.error("Failed to load users:", err); alert("Couldn't fetch users."); }
    );
}

function toggleUserActive(userId) {
    const user = users.find(u => u.id == userId);
    if (!user) return;
    const payload = { id: user.id, active: !user.active };
    ajaxCall("PUT", `${baseUrl}/update-status`, JSON.stringify(payload),
        () => loadUsers(),
        err => { console.error("Failed to update user:", err); alert("Failed to update status."); }
    );
}

function handleResetToDefault() {
    ajaxCall("POST", `${baseUrl}/reset-to-default`, null,
        () => loadUsers(),
        err => { console.error("Reset failed:", err); alert("Failed to reset."); }
    );
}

function handleClearAllData() {
    ajaxCall("DELETE", `${baseUrl}/delete-all`, null,
        () => loadUsers(),
        err => { console.error("Delete-all failed:", err); alert("Failed to delete users."); }
    );
}

function renderUsers() {
    const container = document.getElementById('userTableContainer');
    if (!container) return;
    container.innerHTML = '';

    if (users.length === 0) {
        container.innerHTML = "<p style='text-align:center'>No user data found.</p>";
        return;
    }

    const table = document.createElement('table');
    const thead = document.createElement('thead');
    const headerRow = document.createElement('tr');
    ['ID', 'Name', 'Email', 'Status', 'Action'].forEach(text => {
        const th = document.createElement('th'); th.textContent = text; headerRow.appendChild(th);
    });
    thead.appendChild(headerRow); table.appendChild(thead);

    const tbody = document.createElement('tbody');
    users.forEach(user => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td>${user.id}</td>
            <td>${user.name}</td>
            <td>${user.email}</td>
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
    table.addEventListener('click', e => {
        if (e.target.tagName === 'BUTTON' && e.target.dataset.userId)
            toggleUserActive(e.target.dataset.userId);
    });
}

function init() {
    loadUsers();
    document.getElementById('resetToDefaultButton')?.addEventListener('click', handleResetToDefault);
    document.getElementById('clearAllDataButton')?.addEventListener('click', handleClearAllData);
}

document.addEventListener('DOMContentLoaded', init);
