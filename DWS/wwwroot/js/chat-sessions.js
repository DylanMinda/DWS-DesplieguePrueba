// ========================================
// CONVERSATION HISTORY MANAGEMENT
// ========================================

let currentSessionId = null;

// Load user's conversation sessions on page load
async function loadUserSessions() {
    try {
        const response = await fetch('/Chat/GetUserSessions');
        if (response.ok) {
            const sessions = await response.json();
            displaySessions(sessions);

            // Auto-load most recent session if exists
            if (sessions.length > 0 && !currentSessionId) {
                await switchSession(sessions[0].id);
            }
        }
    } catch (error) {
        console.error('Error loading sessions:', error);
    }
}

// Display sessions in sidebar
function displaySessions(sessions) {
    const conversationsList = document.getElementById('conversationsList');
    conversationsList.innerHTML = '';

    sessions.forEach(session => {
        const sessionItem = document.createElement('div');
        sessionItem.className = 'conversation-item';
        sessionItem.dataset.sessionId = session.id;

        if (session.id === currentSessionId) {
            sessionItem.classList.add('active');
        }

        const fecha = new Date(session.fecha);
        const fechaFormateada = fecha.toLocaleDateString('es-ES', {
            day: 'numeric',
            month: 'short'
        });

        sessionItem.innerHTML = `
            <div class="conversation-title">${session.titulo}</div>
            <div class="conversation-date">${fechaFormateada}</div>
        `;

        sessionItem.addEventListener('click', () => switchSession(session.id));
        conversationsList.appendChild(sessionItem);
    });
}

// Create new chat session
async function startNewChat() {
    console.log('🔵 startNewChat() llamada');
    try {
        console.log('📡 Enviando request a /Chat/CreateSession...');
        const response = await fetch('/Chat/CreateSession', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify("Nuevo Chat")
        });

        console.log('📥 Response status:', response.status);

        if (response.ok) {
            const data = await response.json();
            console.log('✅ Sesión creada:', data);
            currentSessionId = data.sessionId;

            // Clear chat messages
            const chatMessages = document.getElementById('chatMessages');
            chatMessages.innerHTML = `
                <div class="message bot">
                    <div class="message-avatar">
                        <svg width="24" height="24" viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="12" r="10" /></svg>
                    </div>
                    <div class="message-content">
                        ¡Hola! Soy tu asistente de medicación. ¿En qué puedo ayudarte hoy?
                    </div>
                </div>
            `;

            // Reload sessions list
            await loadUserSessions();
            console.log('✅ Nueva conversación creada exitosamente');
        } else {
            console.error('❌ Error en response:', response.statusText);
        }
    } catch (error) {
        console.error('❌ Error creating session:', error);
    }
}

// Switch to different conversation
async function switchSession(sessionId) {
    currentSessionId = sessionId;

    // Update active state in sidebar
    document.querySelectorAll('.conversation-item').forEach(item => {
        item.classList.remove('active');
        if (parseInt(item.dataset.sessionId) === sessionId) {
            item.classList.add('active');
        }
    });

    // Load messages for this session
    try {
        const response = await fetch(`/Chat/GetSessionMessages?sessionId=${sessionId}`);
        if (response.ok) {
            const mensajes = await response.json();
            const chatMessages = document.getElementById('chatMessages');
            chatMessages.innerHTML = '';

            mensajes.forEach(msg => {
                appendMessage(msg.texto, msg.esIA ? 'bot' : 'user');
            });
        }
    } catch (error) {
        console.error('Error loading messages:', error);
    }
}

// Save message to database
async function saveMessageToSession(texto, esIA) {
    if (!currentSessionId) {
        // Create session if doesn't exist
        const response = await fetch('/Chat/CreateSession', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify("Nuevo Chat")
        });
        if (response.ok) {
            const data = await response.json();
            currentSessionId = data.sessionId;
            await loadUserSessions();
        }
    }

    try {
        await fetch('/Chat/SaveMessage', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                SessionId: currentSessionId,
                Texto: texto,
                EsIA: esIA,
                ImagenUrl: null
            })
        });
    } catch (error) {
        console.error('Error saving message:', error);
    }
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
    loadUserSessions();
});
