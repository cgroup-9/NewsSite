$(document).ready(() => {
    const divLogin = $("#loginContainer");

    // Detect if running locally
    function isDevEnv() {
        return location.host.includes("localhost");
    }

    // Define API base URL based on environment
    const port = 7110;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
    const loginUrl = `${baseApiUrl}/api/Users/login`;

    divLogin.empty();
    divLogin.append('<h2 class="fullRowTitle">Log-in</h2>');

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

    $("#submitLogin").click(() => {
        const user = {
            email: $("#emailTB").val().trim(),
            password: $("#passwordTB").val().trim()
        };

        if (!user.email || !user.password) {
            return alert("📛 Please enter email & password.");
        }

        ajaxCall("POST", loginUrl, JSON.stringify(user),
            res => {
                if (res.isActive === false) {
                    alert("🚫 Your account is inactive. Please contact support.");
                    return;
                }

                sessionStorage.setItem("isLoggedIn", "true");
                sessionStorage.setItem("currentUser", JSON.stringify(res));
                alert("✅ Logged-in!");
                location.href = "index.html";
            },
            err => {
                if (err.status === 401)
                    alert("❌ Bad credentials");
                else
                    alert("❌ Server error: " + err.statusText);
            }
        );
    });
});
