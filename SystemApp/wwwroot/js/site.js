// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.getElementById("toggleSidebar").addEventListener("click", function () {
    let sidebar = document.querySelector(".sidebar");
    let mainContent = document.querySelector(".main-content");

    sidebar.classList.toggle("collapsed");

    if (sidebar.classList.contains("collapsed")) {
        mainContent.style.marginLeft = "80px";
        mainContent.style.width = "calc(100% - 80px)";
    } else {
        mainContent.style.marginLeft = "260px";
        mainContent.style.width = "calc(100% - 260px)";
    }
});
    function toggleSidebar() {
        const sidebar = document.querySelector(".sidebar");
    sidebar.classList.toggle("collapsed");
    }
