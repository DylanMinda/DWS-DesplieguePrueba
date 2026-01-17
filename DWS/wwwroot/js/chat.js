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
        const text = messageInput.value.trim();
        if (!text) return;

        // Mostrar mensaje del usuario
        appendMessage(text, 'user');

        // Limpiar input y mostrar carga
        messageInput.value = '';
        loadingIndicator.style.display = 'block';

        try {
            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ texto: text })
            });

            if (response.ok) {
                const data = await response.json();
                appendMessage(data.texto || "El agente no devolvió respuesta.", 'bot');
            } else {
                appendMessage("Error al conectar con el asistente.", 'bot');
            }
        } catch (error) {
            console.error("Error:", error);
            appendMessage("Error de conexión.", 'bot');
        } finally {
            loadingIndicator.style.display = 'none';
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