document.addEventListener('DOMContentLoaded', () => {
    // Wait until the entire HTML document has loaded before running this code

    const nav = document.getElementById('mainNav');
    // Get the main navigation bar element by its ID

    const btn = nav.querySelector('.hamburger');
    // Find the hamburger menu button inside the nav element

    if (!btn) return;
    // If the hamburger button is not found, stop execution (prevents errors)

    btn.addEventListener('click', () =>
        nav.classList.toggle('open')
    );
    // When the hamburger is clicked:
    // - Add the "open" class if it’s not there (show menu)
    // - Remove the "open" class if it’s there (hide menu)
});
