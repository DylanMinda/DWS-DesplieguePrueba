document.addEventListener('DOMContentLoaded', function () {
    // Referencias a elementos del DOM
    const chatMessages = document.getElementById('chatMessages');
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const imageBtn = document.getElementById('imageBtn');
    const imageInput = document.getElementById('imageInput');
    const loadingIndicator = document.getElementById('loadingIndicator');

    // 1. Función para agregar burbujas al chat
    function appendMessage(content, sender, isImage = false) {
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('message', sender);

        let htmlContent = '';
        if (sender === 'bot') {
            htmlContent = `
                <div class="message-avatar">
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="12" r="10" /></svg>
                </div>`;
        }

        if (isImage) {
            htmlContent += `<div class="message-content"><img src="${content}" style="max-width: 250px; border-radius: 10px;" /></div>`;
        } else {
            htmlContent += `<div class="message-content">${content}</div>`;
        }

        messageDiv.innerHTML = htmlContent;
        chatMessages.appendChild(messageDiv);

        // Auto-scroll al final
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }

    // 2. Función principal para enviar mensajes
    async function handleSend() {
        const text = messageInput.value.trim();
        const file = imageInput.files[0];

        if (!text && !file) return;

        // Si hay imagen, la mostramos primero
        if (file) {
            const reader = new FileReader();
            reader.onload = (e) => appendMessage(e.target.result, 'user', true);
            reader.readAsDataURL(file);
        }

        // Si hay texto, lo mostramos
        if (text) {
            appendMessage(text, 'user');
        }

        // Limpiar inputs y mostrar carga
        messageInput.value = '';
        imageInput.value = ''; // Limpiar el input de archivo
        loadingIndicator.style.display = 'block';
        chatMessages.scrollTop = chatMessages.scrollHeight;

        // ENVIAR AL CONTROLADOR (CONEXIÓN N8N)
        const formData = new FormData();
        formData.append("mensaje", text);
        if (file) formData.append("imagen", file);

        try {
            const response = await fetch('/Chat/EnviarAn8n', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const data = await response.json();
                appendMessage(data.respuesta || "Procesado correctamente.", 'bot');
            } else {
                appendMessage("Hubo un error al conectar con el asistente.", 'bot');
            }
        } catch (error) {
            console.error("Error:", error);
            appendMessage("Error de conexión.", 'bot');
        } finally {
            loadingIndicator.style.display = 'none';
        }
    }

    // 3. Eventos
    sendBtn.addEventListener('click', handleSend);

    messageInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    });

    imageBtn.addEventListener('click', () => imageInput.click());

    // Evento para cuando seleccionas una imagen (opcional: pre-visualización rápida)
    imageInput.addEventListener('change', function () {
        if (this.files[0]) {
            messageInput.placeholder = "Imagen seleccionada: " + this.files[0].name;
        }
    });
});

// 4. Función global para las preguntas frecuentes (Question Cards)
function sendQuestion(element) {
    const questionText = element.innerText;
    const input = document.getElementById('messageInput');
    input.value = questionText;

    // Disparar el evento de envío
    document.getElementById('sendBtn').click();

    // Ocultar el contenedor de preguntas sugeridas para limpiar la vista
    const questionsContainer = element.closest('.questions-container');
    if (questionsContainer) questionsContainer.style.display = 'none';
}

document.getElementById('imageInput').addEventListener('change', function () {
    if (this.files && this.files[0]) {
        // Opción A: Redirigir pasando un parámetro (ej. el nombre del archivo)
        const fileName = this.files[0].name;

        // Redirigimos a una acción de tu controlador, por ejemplo "ProcesarImagen"
        window.location.href = `/Chat/ProcesarImagen?archivo=${encodeURIComponent(fileName)}`;

        /* Nota: Si necesitas enviar la imagen real a la otra ventana, 
           lo mejor es subirla primero vía AJAX y que el servidor te devuelva 
           la URL de la nueva página.
        */
    }
});

// Esto es para que el botón bonito active al input oculto
document.getElementById('imageBtn').addEventListener('click', function () {
    document.getElementById('imageInput').click();
});