window.addEventListener("load", () => {
    document.body.classList.add("app-loaded");
});

// Apply automatic lazy loading only to images that explicitly opt in.
const autoLazyImages = document.querySelectorAll("img[data-auto-lazy]:not([loading])");
autoLazyImages.forEach((img) => {
    img.setAttribute("loading", "lazy");
    img.setAttribute("decoding", "async");
});

const sidebarToggle = document.getElementById("sidebarToggle");
const sidebar = document.querySelector(".sidebar");
const overlay = document.getElementById("sidebarOverlay");

if (sidebarToggle && sidebar && overlay) {
    sidebarToggle.addEventListener("click", () => {
        sidebar.classList.toggle("mobile-open");
        overlay.classList.toggle("active");
    });

    overlay.addEventListener("click", () => {
        sidebar.classList.remove("mobile-open");
        overlay.classList.remove("active");
    });
}
