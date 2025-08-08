$(document).ready(() => {
    const divLogin = $("#loginContainer"); // Container where the login form will be placed

    // Check if running on localhost (development) or production
    function isDevEnv() {
        return location.host.includes("localhost");
    }

    // Set base API URL depending on environment
    const port = 7019;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
    const loginUrl = `${baseApiUrl}/api/Users/login`; // Login API endpoint

    // Clear container and add login form title
    divLogin.empty();
    divLogin.append('<h2 class="fullRowTitle">Log-in</h2>');

    // Append login form HTML
    divLogin.append(`
      <form id="loginForm">
        <label for="email">E-mail</label>
        <input type="text" id="emailTB" required />
        <br />

        <label for="password">Password</label>
        <input type="password" id="passwordTB" required />
        <br />

        <button type="button" id="submitLogin">Login</button>
        <br />
        <a href="register.html">New User? register here</a>
      </form>
    `);

    // Handle login button click
    $("#submitLogin").click(() => {
        // Build user object from form inputs
        const user = {
            email: $("#emailTB").val().trim(),
            password: $("#passwordTB").val().trim()
        };

        // Validate that both email and password are entered
        if (!user.email || !user.password) {
            return alert("📛 Please enter email & password.");
        }

        // Send login request to server
        ajaxCall("POST", loginUrl, JSON.stringify(user),
            res => {
                // If account is inactive, block login
                if (res.isActive === false) {
                    alert("🚫 Your account is inactive. Please contact support.");
                    return;
                }

                // Store login state and user info in session storage
                sessionStorage.setItem("isLoggedIn", "true");
                sessionStorage.setItem("currentUser", JSON.stringify(res));

                // Confirm login and redirect to homepage
                alert("✔️ Logged-in!");
                location.href = "index.html";
            },
            err => {
                // Show message for wrong credentials
                if (err.status === 401)
                    alert("❌ Bad credentials");
                else
                    alert("❌ Server error: " + err.statusText);
            }
        );
    });
});
