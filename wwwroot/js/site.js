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

budoco = window.budoco || {};

$(function () {
    var factory = function () {
        var $body = $(document.body);
        var $modal = $("#modal");
        var $modalWindow = $modal.find(".window");
        var $modalHeader = $modalWindow.find(".header");
        var $modalBody = $modalWindow.find(".body");
        var $okButton = $modalWindow.find(".btn");

        function show(title, content) {
            $modalHeader.text(title);
            $modalBody.html(content);

            $body.append($modal);

            $okButton.on('click', function () {
                close();
            });

            $body.css("overflow", "hidden");
            $modal.show();
            $modalWindow.animate({
                top: 100,
                opacity: 1
            }, 300);
        }

        function close() {
            $modalWindow.animate({
                top: -1000,
                opacity: 0
            }, 300, function () {
                $modal.hide();
                $body.css("overflow", "auto");
            });
        }

        return {
            show: function (title, content) {
                show(title, content);
            },
            close: function () {
                close();
            }
        };
    }

    budoco.modal = factory();
});