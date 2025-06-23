const STORAGE_KEY = 'adminPanelUsersData'; // כבר לא בשימוש
let users = [];

function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7110;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/Users`;

// ========== AJAX HELPER ==========
function ajaxCall(method, url, data, successCB, errorCB) {
    $.ajax({
        type: method,
        url: url,
        data: data,
        contentType: "application/json",
        dataType: "json",
        success: successCB,
        error: errorCB
    });
}

// ========== LOAD USERS FROM SERVER ==========
function loadUsers() {
    ajaxCall("GET", baseUrl, null,
        res => {
            users = res;
            renderUsers();
        },
        err => {
            console.error("❌ Failed to load users:", err);
            alert("❌ Couldn't fetch users from server.");
        }
    );
}

// ========== UPDATE USER ACTIVE STATUS ==========
function toggleUserActive(userId) {
    const user = users.find(u => u.id == userId);
    if (!user) return;

    const payload = {
        id: user.id,
        active: !user.active
    };

    ajaxCall("PUT", `${baseUrl}/update-status`, JSON.stringify(payload),
        res => {
            console.log("✅ Status updated");
            loadUsers();
        },
        err => {
            console.error("❌ Failed to update user status:", err);
            alert("❌ Failed to update user status.");
        }
    );

}


// ========== RESET TO DEFAULT USERS ==========
function handleResetToDefault() {
    ajaxCall("POST", `${baseUrl}/reset-to-default`, null,
        res => {
            console.log("✅ Reset to default completed");
            loadUsers();
        },
        err => {
            console.error("❌ Failed to reset:", err);
            alert("❌ Failed to reset users.");
        }
    );
}

// ========== CLEAR ALL USERS ==========
function handleClearAllData() {
    ajaxCall("DELETE", `${baseUrl}/delete-all`, null,
        res => {
            console.log("✅ All users cleared");
            loadUsers();
        },
        err => {
            console.error("❌ Failed to clear users:", err);
            alert("❌ Failed to delete users.");
        }
    );
}

// ========== RENDER TABLE ==========
function renderUsers() {
    const container = document.getElementById('userTableContainer');
    if (!container) return console.error('User table container not found!');
    container.innerHTML = '';

    if (users.length === 0) {
        const messageP = document.createElement('p');
        messageP.innerHTML = "No user data found. Click 'Reset to Default Users' or check your server.";
        messageP.style.textAlign = 'center';
        container.appendChild(messageP);
        return;
    }

    const table = document.createElement('table');
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');

    const headerRow = document.createElement('tr');
    ['ID', 'Name', 'Email', 'Status', 'Action'].forEach(text => {
        const th = document.createElement('th');
        th.textContent = text;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    users.forEach(user => {
        const tr = document.createElement('tr');

        tr.innerHTML = `
            <td>${user.id}</td>
            <td>${user.name}</td>
            <td>${user.email}</td>
            <td class="${user.active ? 'status-active' : 'status-inactive'}">${user.active ? 'Active' : 'Inactive'}</td>
            <td><button class="${user.active ? 'deactivate' : 'activate'}" data-user-id="${user.id}">
                ${user.active ? 'Deactivate' : 'Activate'}</button></td>
        `;
        tbody.appendChild(tr);
    });

    table.appendChild(tbody);
    container.appendChild(table);

    table.addEventListener('click', handleTableClick);
}

// ========== EVENT: BUTTON CLICK ==========
function handleTableClick(event) {
    if (event.target.tagName === 'BUTTON' && event.target.dataset.userId) {
        toggleUserActive(event.target.dataset.userId);
    }
}

function init() {
    loadUsers();
}

document.addEventListener('DOMContentLoaded', init);
