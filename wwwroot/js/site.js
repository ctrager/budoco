// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


function hide_waiting() {
    document.body.style.cursor = 'default';
    $(".waiting").hide();
}

function show_waiting() {
    document.body.style.cursor = 'wait';
    $(".waiting").show();
}