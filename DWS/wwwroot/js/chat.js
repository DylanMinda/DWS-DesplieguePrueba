document.addEventListener('DOMContentLoaded', function () {
    const chatMessages = document.getElementById('chatMessages');
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const loadingIndicator = document.getElementById('loadingIndicator');

    function appendMessage(content, sender) {
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

    async function handleSend() {
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

            appendMessage(respuestaFinal || "El asistente no devolvió respuesta.", 'bot');
        }
    }

    sendBtn.addEventListener('click', handleSend);
    messageInput.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    });
});