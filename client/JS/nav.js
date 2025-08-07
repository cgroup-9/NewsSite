document.addEventListener('DOMContentLoaded', () => {
    const nav = document.getElementById('mainNav');
    const btn = nav.querySelector('.hamburger');
    if (!btn) return;
    btn.addEventListener('click', () => nav.classList.toggle('open'));
});
