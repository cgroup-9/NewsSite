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
    const container = document.getElementById('userTableContainer');
    if (!container) return console.error('User table container not found!');
    container.innerHTML = ''; // clear previous content

    // if no users found, show a message
    if (users.length === 0) {
        const messageP = document.createElement('p');
        messageP.innerHTML = "No user data found. Click 'Reset to Default Users' or check your server.";
        messageP.style.textAlign = 'center';
        container.appendChild(messageP);
        return;
    }

    // create table structure
    const table = document.createElement('table');
    const thead = document.createElement('thead');
    const tbody = document.createElement('tbody');

    // create header row
    const headerRow = document.createElement('tr');
    ['ID', 'Name', 'Email', 'Status', 'Action'].forEach(text => {
        const th = document.createElement('th');
        th.textContent = text;
        headerRow.appendChild(th);
    });
    thead.appendChild(headerRow);
    table.appendChild(thead);

    // create row for each user
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

    // attach click event to the table buttons
    table.addEventListener('click', handleTableClick);
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
}

// wait for DOM to fully load before starting
document.addEventListener('DOMContentLoaded', init);
