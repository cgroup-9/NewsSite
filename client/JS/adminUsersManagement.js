// holds the list of users fetched from the server
let users = [];

// checks if we're running in local development environment
function isDevEnv() {
    return location.host.includes("localhost");
}

// sets the base API URL depending on the environment
const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
const baseUrl = `${baseApiUrl}/api/Users`;


// fetches all users from the server
function loadUsers() {
    ajaxCall("GET", `${baseUrl}/getAllUseresAdmin`, null,
        res => {
            users = res; // save fetched users to local array
            renderUsers(); // display them in the UI
        },
        err => {
            console.error("❌ Failed to load users:", err);
            alert("❌ Couldn't fetch users from server.");
        }
    );
}

// toggles a user's active status (activate/deactivate)
function toggleUserActive(userId) {
    const user = users.find(u => u.id == userId);
    if (!user) return;

    const payload = {
        id: user.id,
        active: !user.active // flip the current active status
    };

    ajaxCall("PUT", `${baseUrl}/update-status`, JSON.stringify(payload),
        res => {
            console.log("✅ Status updated");
            loadUsers(); // reload to see updated status
        },
        err => {
            console.error("❌ Failed to update user status:", err);
            alert("❌ Failed to update user status.");
        }
    );
}

// resets all users to default data from the server
function handleResetToDefault() {
    ajaxCall("POST", `${baseUrl}/reset-to-default`, null,
        res => {
            console.log("✅ Reset to default completed");
            loadUsers(); // reload new user list
        },
        err => {
            console.error("❌ Failed to reset:", err);
            alert("❌ Failed to reset users.");
        }
    );
}

// deletes all users from the database
function handleClearAllData() {
    ajaxCall("DELETE", `${baseUrl}/delete-all`, null,
        res => {
            console.log("✅ All users cleared");
            loadUsers(); // refresh empty list
        },
        err => {
            console.error("❌ Failed to clear users:", err);
            alert("❌ Failed to delete users.");
        }
    );
}

// builds the user table and displays it on screen
function renderUsers() {
    // === CHANGED: target the <tbody id="userTableBody"> from new HTML === // NEW-BS
    const tbody = document.getElementById('userTableBody');
    if (!tbody) return console.error('tbody#userTableBody not found!');
    tbody.innerHTML = ''; // clear previous rows

    // if no users found, show a message
    if (users.length === 0) {
        const tr = document.createElement('tr');
        tr.innerHTML = `<td colspan="5" class="text-center py-4">
            No user data found. Click <strong>Reset to Default</strong> or check the server.</td>`;
        tbody.appendChild(tr);
        return;
    }

    // create row for each user
    users.forEach(user => {
        const tr = document.createElement('tr');

        // keep the same cell order as before, but use Bootstrap btn classes // NEW-BS
        tr.innerHTML = `
            <td>${user.id}</td>
            <td>${user.name}</td>
            <td>${user.email}</td>
            <td class="${user.active ? 'text-success fw-semibold' : 'text-danger fw-semibold'}">
                ${user.active ? 'Active' : 'Inactive'}
            </td>
            <td>
                <button
                    class="btn btn-sm ${user.active ? 'btn-danger' : 'btn-success'}"
                    data-user-id="${user.id}">
                    ${user.active ? 'Deactivate' : 'Activate'}
                </button>
            </td>
        `;
        tbody.appendChild(tr);
    });

    // attach click handler to all buttons (using delegation on <tbody>)
    tbody.addEventListener('click', handleTableClick);
}

// handles click events on buttons inside the user table
function handleTableClick(event) {
    if (event.target.tagName === 'BUTTON' && event.target.dataset.userId) {
        toggleUserActive(event.target.dataset.userId); // trigger activate/deactivate
    }
}

// initializes the page on load
function init() {
    loadUsers(); // fetch and display users

    // attach global handlers for the reset / clear buttons if present
    document.getElementById('resetToDefaultButton')?.addEventListener('click', handleResetToDefault);
    document.getElementById('clearAllDataButton')?.addEventListener('click', handleClearAllData);
}

// wait for DOM to fully load before starting
document.addEventListener('DOMContentLoaded', init);
