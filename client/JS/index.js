$(document).ready(() => {
    const divLogin = $("#loginContainer");

    // Detect if running locally
    function isDevEnv() {
        return location.host.includes("localhost");
    }

    // Define API base URL based on environment
    const port = 7019;
    const baseApiUrl = isDevEnv()
        ? `https://localhost:${port}`
        : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";
    const loginUrl = `${baseApiUrl}/api/Users/login`;

    divLogin.empty();
    divLogin.append('<h2 class="fullRowTitle">Log-in</h2>');

    divLogin.append(`
      <form id="loginForm">
        <label for="name">username</label>
        <input type="text" id="nameTB" required />
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
            name: $("#nameTB").val().trim(),
            password: $("#passwordTB").val().trim()
        };

        if (!user.name || !user.password) {
            return alert("📛 Please enter name & password.");
        }

        ajaxCall("POST", loginUrl, JSON.stringify(user),
            res => {
                if (res.Active === false) {
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