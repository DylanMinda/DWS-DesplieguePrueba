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
            <button class="delete-conversation-btn" onclick="deleteConversation(${session.id}, event)" title="Eliminar conversación">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <polyline points="3 6 5 6 21 6"></polyline>
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                </svg>
            </button>
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
    console.log('🔄 Cambiando a sesión:', sessionId);
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
        console.log('📡 Cargando mensajes de sesión:', sessionId);
        const response = await fetch(`/Chat/GetSessionMessages?sessionId=${sessionId}`);
        if (response.ok) {
            const mensajes = await response.json();
            console.log('📥 Mensajes recibidos:', mensajes.length, mensajes);
            const chatMessages = document.getElementById('chatMessages');
            chatMessages.innerHTML = '';

            if (mensajes.length === 0) {
                // Show welcome message if conversation is empty
                appendMessage('¡Hola! Soy tu asistente de medicación. ¿En qué puedo ayudarte hoy?', 'bot');
            } else {
                mensajes.forEach(msg => {
                    console.log('➕ Agregando mensaje:', msg.texto.substring(0, 50) + '...');
                    appendMessage(msg.texto, msg.esIA ? 'bot' : 'user');
                });
            }
            console.log('✅ Mensajes cargados exitosamente');
        } else {
            console.error('❌ Error al cargar mensajes:', response.status);
        }
    } catch (error) {
        console.error('❌ Error loading messages:', error);
    }
}

// Delete conversation
async function deleteConversation(sessionId, event) {
    event.stopPropagation(); // Prevent switching to conversation

    if (!confirm('¿Estás seguro de que quieres eliminar esta conversación?')) {
        return;
    }

    try {
        console.log('🗑️ Eliminando conversación:', sessionId);
        const response = await fetch(`/Chat/DeleteSession?sessionId=${sessionId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            console.log('✅ Conversación eliminada');

            // If we're deleting the current session, create a new one
            if (currentSessionId === sessionId) {
                await startNewChat();
            } else {
                // Just reload the sessions list
                await loadUserSessions();
            }
        } else {
            console.error('❌ Error al eliminar:', response.status);
            alert('Error al eliminar la conversación');
        }
    } catch (error) {
        console.error('❌ Error deleting session:', error);
        alert('Error al eliminar la conversación');
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
    console.log('🟢 chat-sessions.js cargado');

    // Attach event listener to new chat button
    const newChatBtn = document.getElementById('newChatBtn');
    if (newChatBtn) {
        console.log('✅ Botón Nueva Conversación encontrado');
        newChatBtn.addEventListener('click', function () {
            console.log('🔵 Click en Nueva Conversación detectado');
            startNewChat();
        });
    } else {
        console.error('❌ Botón newChatBtn NO encontrado');
    }

    loadUserSessions();
});
