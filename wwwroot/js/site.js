// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener("DOMContentLoaded", function () {
    const photoInput = document.querySelector("input[name='Photos']");
    const errorSpan = document.getElementById("photoError");
    const allowedExtensions = ["jpg", "jpeg", "png", "gif"];

    photoInput.addEventListener("change", function () {
        errorSpan.textContent = ""; // Clear previous errors

        for (let file of photoInput.files) {
            let fileExtension = file.name.split(".").pop().toLowerCase();

            if (!allowedExtensions.includes(fileExtension)) {
                errorSpan.textContent = "❌ Зөвхөн JPG, JPEG, PNG, болон GIF зураг оруулна уу.";
                photoInput.value = ""; // Clear the invalid selection
                break;
            }
        }
    });
});