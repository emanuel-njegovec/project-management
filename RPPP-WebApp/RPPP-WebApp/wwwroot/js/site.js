$(function () {
    $(document).on('click', '.delete', function (event) {
        if (!confirm("Obrisati zapis?")) {
            event.preventDefault();
        }
    });
});

$(function () {
    $(document).on('click', '.create', function (event) {
        if (!confirm("Dodati Zapis?")) {
            event.preventDefault();
        }
    });
});