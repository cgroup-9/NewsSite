$(document).ready(() => {
    const divAddUser = $("#registerContainer"); // Container where the registration form will be rendered

    // Function to detect if running on localhost or production server
    function isDevEnv() {
        return location.host.includes('localhost');
    }

    // Define API base URL based on environment
    const port = 7019;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
    const url = `${baseApiUrl}/api/Users/register`; // Endpoint for user registration

    // Function to send a new user object to the server
    function addToUser(user) {
        try {
            console.log("Sending user:", JSON.stringify(user)); // Debug: print data before sending
            ajaxCall("POST", url, JSON.stringify(user), addToUserSuc, addToUserFa); // Make POST request
        } catch (err) {
            console.error("❌ Error before POST:", err);
            alert("Failed to send user information to the server. Please try again.");
        }
    }

    // Success callback for user registration
    function addToUserSuc(res) {
        if (res === true) {
            alert("✔️ User added successfully!"); // Success message
            window.location.href = "login.html"; // Redirect to login page
        } else {
            alert("⚠️ User already exists."); // Duplicate user warning
        }
    }

    // Failure callback for user registration
    function addToUserFa(err) {
        alert("❌ Failed to add user: " + err.statusText);
    }

    // Create and insert the registration form into the container
    divAddUser.empty();
    divAddUser.append('<h2 class="fullRowTitle">Register</h2>');

    let formAddUser = `
        <form id="registerForm">
            <label for="username">Username</label>
            <input type="text" id="usernameTB" required placeholder="Username" />
            <div class="error-msg" id="err-usernameTB"></div>
            <br>

            <label for="password">Password</label>
            <input type="password" id="passwordTB" required placeholder="Password" />
            <div class="error-msg" id="err-passwordTB"></div>
            <br>

            <label for="email">Email</label>
            <input type="email" id="emailTB" required placeholder="Email" />
            <div class="error-msg" id="err-emailTB"></div>
            <br>

            <button type="button" id="submitRegister">Register</button>
        </form>
    `;

    divAddUser.append(formAddUser); // Add form to page

    // Handle the Register button click
    $("#submitRegister").click(() => {
        // Create object from form fields
        const userToSend = {
            name: $("#usernameTB").val().trim(),
            password: $("#passwordTB").val().trim(),
            email: $("#emailTB").val().trim()
        };

        // Clear previous error messages
        $(".error-msg").text("");
        let hasError = false;

        // Validation rules for each field
        const fields = [
            {
                id: "usernameTB",
                value: userToSend.name,
                regex: /^[A-Za-z]{2,}$/, // Only letters, min length 2
                msg: "Name must contain only letters and be at least 2 characters."
            },
            {
                id: "passwordTB",
                value: userToSend.password,
                regex: /^(?=.*[A-Z])(?=.*\d).{8,}$/, // Min 8 chars, 1 uppercase, 1 number
                msg: "Password must be at least 8 characters, include one uppercase letter and one number."
            },
            {
                id: "emailTB",
                value: userToSend.email,
                regex: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/, // Basic email format
                msg: "Invalid email format."
            }
        ];

        // Validate each field and show error if needed
        fields.forEach(({ id, value, regex, msg }) => {
            const input = $(`#${id}`);
            const errorDiv = $(`#err-${id}`);

            if (!regex.test(value)) {
                input.addClass("invalid"); // Add red border
                errorDiv.text(msg);        // Show error message
                hasError = true;
            } else {
                input.removeClass("invalid");
                errorDiv.text("");
            }
        });

        if (hasError) return; // Stop if validation failed

        // Send the new user data to the server
        addToUser(userToSend);
    });
});
