$(document).ready(() => {
    const divEditUser = $("#editProfileContainer");

    // Detect if running on localhost (development mode)
    function isDevEnv() {
        return location.host.includes("localhost");
    }

    // Set base API URL depending on environment (localhost or deployed)
    const port = 7019;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

    const url = `${baseApiUrl}/api/Users/update-user`;

    // Get current user from session storage
    let currentUser = JSON.parse(sessionStorage.getItem("currentUser"));

    // Function to send updated user to server
    function updateUser(user) {
        try {
            console.log("Sending updated user:", JSON.stringify(user));
            ajaxCall("PUT", url, JSON.stringify(user), updateUserSuc, updateUserFa);
        } catch (err) {
            console.error("❌ Error before PUT:", err);
            alert("Failed to update user. Please try again.");
        }
    }

    // Callback on success
    function updateUserSuc(res) {
        try {
            if (res && res.message) {
                alert("✔️ " + res.message);
            } else {
                alert("✔️ Profile updated successfully.");
            }

            // Update session storage with new values
            const updatedUser = {
                name: $("#usernameTB").val().trim() || currentUser.name,
                password: $("#passwordTB").val().trim() || $("#currentPasswordTB").val().trim(),
                email: $("#emailTB").val().trim() || currentUser.email
            };

            sessionStorage.setItem("currentUser", JSON.stringify(updatedUser));

            // Reload page to reflect changes
            location.reload();
        } catch (e) {
            alert("⚠️ Profile updated, but response was invalid.");
            console.error("Response parse error:", e, res);
        }
    }

    // Callback on failure
    function updateUserFa(err) {
        alert("❌ Failed to update user: " + err.statusText);
    }

    // Render form to the page
    divEditUser.empty();
    divEditUser.append('<h2 class="fullRowTitle">Edit My Profile</h2>');

    // Build and inject the form HTML
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

            <label for="currentPassword">Current Password</label>
            <input type="password" id="currentPasswordTB" placeholder="Current Password" />
            <div class="error-msg" id="err-currentPasswordTB"></div>
            <br>

            <label for="email">Email</label>
            <input type="email" id="emailTB" placeholder="${currentUser.email}" />
            <div class="error-msg" id="err-emailTB"></div>
            <br>

            <button type="button" id="submitEdit">Save Changes</button>
        </form>
    `;

    divEditUser.append(formHtml);

    // Submit button click handler
    $("#submitEdit").click(() => {
        const inputName = $("#usernameTB").val().trim();
        const inputPassword = $("#passwordTB").val().trim();
        const currentPassword = $("#currentPasswordTB").val().trim();
        const inputEmail = $("#emailTB").val().trim();

        // Clear previous errors
        $(".error-msg").text("");
        let hasError = false;

        // Require either current or new password
        if (inputPassword === "" && currentPassword === "") {
            $("#currentPasswordTB").addClass("invalid");
            $("#err-currentPasswordTB").text("You must enter your current password or a valid new password.");
            hasError = true;
        }

        // Validate new password if entered
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

        // Validate name and email format if filled in
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

        if (hasError) return;

        // Build final object to send
        const userToUpdate = {
            id: currentUser.id,
            name: inputName || currentUser.name,
            password: inputPassword || currentPassword || currentUser.password,
            email: inputEmail || currentUser.email
        };

        // Send update request
        updateUser(userToUpdate);
    });
});
