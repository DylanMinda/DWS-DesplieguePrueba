// Scanner.js - Scripts para la vista Scanner

// (La lógica de OCR se agregará cuando se solicite)

document.addEventListener('DOMContentLoaded', function () {
    var uploadArea = document.getElementById('uploadArea');
    var fileInput = document.getElementById('fileInput');
    var btnUpload = document.getElementById('btnUpload');

    // Click en área de upload abre selector de archivos
    if (uploadArea && fileInput) {
        uploadArea.addEventListener('click', function () {
            fileInput.click();
        });
    }

    // Botón de upload
    if (btnUpload && fileInput) {
        btnUpload.addEventListener('click', function () {
            fileInput.click();
        });
    }

    // Drag and drop visual feedback
    if (uploadArea) {
        uploadArea.addEventListener('dragover', function (e) {
            e.preventDefault();
            this.classList.add('dragging');
        });

        uploadArea.addEventListener('dragleave', function (e) {
            e.preventDefault();
            this.classList.remove('dragging');
        });

        uploadArea.addEventListener('drop', function (e) {
            e.preventDefault();
            this.classList.remove('dragging');
            // La lógica de procesamiento se agregará posteriormente
        });
    }

    // Cambio de tabs
    var tabButtons = document.querySelectorAll('.tab-button');
    var tabContents = document.querySelectorAll('.tab-content');

    tabButtons.forEach(function (button) {
        button.addEventListener('click', function () {
            var targetTab = this.getAttribute('data-tab');

            // Remover active de todos
            tabButtons.forEach(function (btn) {
                btn.classList.remove('active');
            });
            tabContents.forEach(function (content) {
                content.classList.remove('active');
            });

            // Activar seleccionado
            this.classList.add('active');
            var targetContent = document.getElementById(targetTab + 'Tab');
            if (targetContent) {
                targetContent.classList.add('active');
            }
        });
    });

    // Botones de acción (copiar y descargar)
    var btnCopy = document.getElementById('btnCopy');
    var btnDownload = document.getElementById('btnDownload');

    if (btnCopy) {
        btnCopy.addEventListener('click', function () {
            // La lógica se agregará posteriormente
        });
    }

    if (btnDownload) {
        btnDownload.addEventListener('click', function () {
            // La lógica se agregará posteriormente
        });
    }
});
