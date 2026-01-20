// Global function to append messages (used by both chat.js and chat-sessions.js)
function appendMessage(content, sender) {
    const chatMessages = document.getElementById('chatMessages');
    const messageDiv = document.createElement('div');
    messageDiv.classList.add('message', sender);

    let htmlContent = '';
    if (sender === 'bot') {
        htmlContent = `
            <div class="message-avatar">
                <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="12" r="10" /></svg>
            </div>`;
    }
    htmlContent += `<div class="message-content">${content}</div>`;

    messageDiv.innerHTML = htmlContent;
    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

document.addEventListener('DOMContentLoaded', function () {
    const chatMessages = document.getElementById('chatMessages');
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const loadingIndicator = document.getElementById('loadingIndicator');

    async function handleSend() {
        const message = messageInput.value.trim();
        if (!message) return;

        // 1. Agregar mensaje del usuario al chat
        appendMessage(message, 'user');
        messageInput.value = '';
        messageInput.style.height = 'auto'; // Reset altura

        // 1.5 Guardar mensaje del usuario en BD
        await saveMessageToSession(message, false);

        // 2. Mostrar loading
        loadingIndicator.classList.add('active');
        chatMessages.scrollTop = chatMessages.scrollHeight;

        try {
            // 3. Enviar al backend
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ Texto: message })
            });

            if (response.ok) {
                const data = await response.json();

                // n8n envía un JSON dentro de otro JSON. 
                // Debemos extraer solo la propiedad 'texto'
                let respuestaFinal = data.texto;

                // Si la respuesta viene como un string con formato JSON, lo limpiamos
                if (typeof respuestaFinal === 'string' && respuestaFinal.startsWith('{')) {
                    try {
                        const tempObj = JSON.parse(respuestaFinal);
                        respuestaFinal = tempObj.texto;
                    } catch (e) {
                        console.error("Error al parsear respuesta:", e);
                    }
                }

                if (data.alerta) {
                    // Mostrar alerta visualmente distinta (ej. un borde rojo o icono warning)
                    const alertaHtml = `<div style="background-color: #fee2e2; border: 1px solid #ef4444; color: #b91c1c; padding: 10px; border-radius: 8px; margin-bottom: 10px;">
                                            <strong>${data.alerta}</strong>
                                        </div>`;
                    appendMessage(alertaHtml, 'bot');
                    // Guardar alerta en BD
                    await saveMessageToSession(data.alerta, true);
                }

                const botMessage = respuestaFinal || "El asistente no devolvió respuesta.";
                appendMessage(botMessage, 'bot');
                // Guardar respuesta del bot en BD
                await saveMessageToSession(botMessage, true);
            } else {
                appendMessage("Error al conectar con el servidor.", 'bot');
                console.error("Server error:", response.status);
            }
        } catch (error) {
            console.error("Error de red:", error);
            appendMessage("No hay conexión con el servidor.", 'bot');
        } finally {
            // 4. Ocultar loading
            loadingIndicator.classList.remove('active');
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    }

    sendBtn.addEventListener('click', handleSend);
    messageInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    });

    // Botón de imagen - trigger file input
    const imageBtn = document.getElementById('imageBtn');
    const imageInput = document.getElementById('imageInput');

    if (imageBtn && imageInput) {
        imageBtn.addEventListener('click', () => {
            imageInput.click();
        });

        imageInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                console.log('Imagen seleccionada:', file.name);
                // TODO: Implementar subida de imagen al servidor
                alert('Funcionalidad de subida de imagen en desarrollo');
            }
        });
    }
});