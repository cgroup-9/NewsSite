$(document).ready(() => {
    const divEditUser = $("#editProfileContainer"); // Container for edit profile form

    // Check if running in local dev environment
    function isDevEnv() {
        return location.host.includes("localhost");
    }

    // API base URL (switch between local and deployed)
    const port = 7019;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

    const url = `${baseApiUrl}/api/Users/update-user`; // Endpoint for updating user

    // Get the currently logged-in user from sessionStorage
    let currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

    // Send PUT request to update user details
    function updateUser(user) {
        try {
            console.log("Sending updated user:", JSON.stringify(user));
            ajaxCall("PUT", url, JSON.stringify(user), updateUserSuc, updateUserFa);
        } catch (err) {
            console.error("❌ Error before PUT:", err);
            alert("Failed to update user. Please try again.");
        }
    }

    // Success callback for update request
    function updateUserSuc(res) {
        try {
            if (res && res.message) {
                alert("✔️ " + res.message + "\nPlease log in again to continue.");
            } else {
                alert("✔️ Profile updated successfully.\nPlease log in again to continue.");
            }
            logout(); // Clear session and redirect to login
        } catch (e) {
            alert("⚠️ Profile updated, but response was invalid.");
            console.error("Response parse error:", e, res);
        }
    }

    // Failure callback for update request
    function updateUserFa(err) {
        alert("❌ Failed to update user: " + err.statusText);
    }

    // Clear container and add page title
    divEditUser.empty();
    divEditUser.append('<h2 class="fullRowTitle">Edit My Profile</h2>');

    // Build HTML form dynamically (placeholders show current values)
    let formHtml = `
        <form id="editForm">
            <label for="username">Username</label>
            <input type="text" id="usernameTB" placeholder="${currentUser.name}" />
            <div class="error-msg" id="err-usernameTB"></div>
            <br>

            <label for="password">New Password</label>
            <input type="password" id="passwordTB" placeholder="New Password" />
            <div class="error-msg" id="err-passwordTB"></div>
            <br>

            <label for="email">Email</label>
            <input type="email" id="emailTB" placeholder="${currentUser.email}" />
            <div class="error-msg" id="err-emailTB"></div>
            <br>

            <button type="button" id="submitEdit">Save Changes</button>
        </form>
    `;

    // Insert form into the container
    divEditUser.append(formHtml);

    // Handle Save Changes button click
    $("#submitEdit").click(() => {
        const inputName = $("#usernameTB").val().trim();  // New name (if entered)
        const inputPassword = $("#passwordTB").val().trim(); // New password (if entered)
        const inputEmail = $("#emailTB").val().trim();  // New email (if entered)

        // Clear previous error messages
        $(".error-msg").text("");
        let hasError = false;

        // Password validation (only if a new password was entered)
        if (inputPassword !== "") {
            const passRegex = /^(?=.*[A-Z])(?=.*\d).{8,}$/;
            if (!passRegex.test(inputPassword)) {
                $("#passwordTB").addClass("invalid");
                $("#err-passwordTB").text("Password must be at least 8 characters, include one uppercase letter and one number.");
                hasError = true;
            } else {
                $("#passwordTB").removeClass("invalid");
            }
        }

        // Validation rules for name and email (only if entered)
        const fields = [
            {
                id: "usernameTB",
                value: inputName,
                regex: /^[A-Za-z]{2,}$/,
                msg: "Name must contain only letters and be at least 2 characters."
            },
            {
                id: "emailTB",
                value: inputEmail,
                regex: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                msg: "Invalid email format."
            }
        ];

        // Check each field's validity
        fields.forEach(({ id, value, regex, msg }) => {
            const input = $(`#${id}`);
            const errorDiv = $(`#err-${id}`);

            if (value !== "" && !regex.test(value)) {
                input.addClass("invalid");
                errorDiv.text(msg);
                hasError = true;
            } else {
                input.removeClass("invalid");
                errorDiv.text("");
            }
        });

        // Stop if validation failed
        if (hasError) return;

        // Create the object to send to the server (use old values if new ones not entered)
        const userToUpdate = {
            id: currentUser.id,
            name: inputName || currentUser.name,
            password: inputPassword || currentUser.password,
            email: inputEmail || currentUser.email
        };

        // Send the update request
        updateUser(userToUpdate);
    });
});
