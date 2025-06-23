$(document).ready(() => {
    const divAddUser = $("#registerContainer");

    // Detect if running locally
    function isDevEnv() {
        return location.host.includes('localhost');
    }

    // Define API base URL based on environment
    const port = 7110;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
    const url = `${baseApiUrl}/api/Users/register`; // Use this URL for registration

    // Function to add user to the database
    function addToUser(user) {
        try {
            console.log("Sending user:", JSON.stringify(user));
            ajaxCall("POST", url, JSON.stringify(user), addToUserSuc, addToUserFa);
        } catch (err) {
            console.error("❌ Error before POST:", err);
            alert("Failed to send user information to the server. Please try again.");
        }
    }

    function addToUserSuc(res) {
        if (res === true) {
            alert("🎬 User added successfully!");
            window.location.href = "login.html";
        } else {
            alert("⚠️ User already exists.");
        }
    }

    function addToUserFa(err) {
        alert("❌ Failed to add user: " + err.statusText);
    }

    // Prepare the registration form
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

    divAddUser.append(formAddUser);

    // Handle register form submission
    $("#submitRegister").click(() => {
        const userToSend = {
            name: $("#usernameTB").val().trim(),
            password: $("#passwordTB").val().trim(),
            email: $("#emailTB").val().trim()
        };

        // Clear previous error messages
        $(".error-msg").text("");

        let hasError = false;

        // Validation rules for form fields
        const fields = [
            {
                id: "usernameTB",
                value: userToSend.name,
                regex: /^[A-Za-z]{2,}$/,
                msg: "Name must contain only letters and be at least 2 characters."
            },
            {
                id: "passwordTB",
                value: userToSend.password,
                regex: /^(?=.*[A-Z])(?=.*\d).{8,}$/,
                msg: "Password must be at least 8 characters, include one uppercase letter and one number."
            },
            {
                id: "emailTB",
                value: userToSend.email,
                regex: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                msg: "Invalid email format."
            }
        ];

        // Check if all fields are valid
        fields.forEach(({ id, value, regex, msg }) => {
            const input = $(`#${id}`);
            const errorDiv = $(`#err-${id}`);

            if (!regex.test(value)) {
                input.addClass("invalid");
                errorDiv.text(msg);
                hasError = true;
            } else {
                input.removeClass("invalid");
                errorDiv.text("");
            }
        });

        if (hasError) return;

        // Call the function to add the user to the server
        addToUser(userToSend);
    });
});
