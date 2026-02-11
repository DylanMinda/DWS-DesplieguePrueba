// ==========================================
// CONFIGURACIÓN DEL CHATBOT (EDITABLE)
// ==========================================
/**
 * 💡 GUÍA PARA AUMENTAR MÁS PUNTOS:
 * Para añadir un nuevo tema (ej. "Primeros Auxilios"), simplemente:
 * 1. Copia un bloque del array 'menu' y pégalo al final.
 * 2. Cambia el 'id', 'titulo', 'keywords' y las 'preguntas'.
 * 3. ¡Listo! El chat detectará automáticamente el nuevo tema.
 */
const chatbotConfig = {
    nombre: "Educador MedIQ",
    bienvenida: "¡Hola! Soy MedIQ, un asistente automatizado para la **educación sobre medicamentos**. Mi propósito es ayudarte a entender mejor temas de salud, pero **no soy médico, no receto dosis ni diagnostico enfermedades**. Todo el contenido es educativo y gestionado por expertos. Para cualquier duda médica, consulta siempre a tu doctor.",
    menu: [] // Se cargará dinámicamente
};

// Función para cargar el menú desde la DB
async function loadDynamicMenu() {
    console.log("📡 Iniciando carga de menú dinámico desde /Chat/GetDynamicMenu...");
    try {
        const response = await fetch('/Chat/GetDynamicMenu');
        if (response.ok) {
            const data = await response.json();
            chatbotConfig.menu = data;
            console.log("✅ Menú dinámico cargado con éxito. Categorías encontradas:", data.length);
            return data;
        } else {
            console.error("❌ Error en la respuesta del servidor:", response.status, response.statusText);
        }
    } catch (error) {
        console.error("❌ Error de red al cargar menú:", error);
    }
    return [];
}

// Función simple para parsear Markdown (Negritas y saltos de línea)
function parseMarkdown(text) {
    if (!text) return "";
    // Reemplazar **texto** o ##texto## por <strong>texto</strong>
    let parsed = text.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    parsed = parsed.replace(/##(.*?)##/g, '<strong>$1</strong>');
    return parsed;
}

// Global function to append messages (used by both chat.js and chat-sessions.js)
function appendMessage(content, sender, imagenUrl = null) {
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

    let messageBody = `<div class="message-content">`;
    if (imagenUrl) {
        messageBody += `<img src="${imagenUrl}" class="chat-image" style="max-width: 200px; border-radius: 8px; margin-bottom: 8px; display: block;" />`;
    }

    // Aplicamos el parseo de markdown solo al contenido de texto
    const finalContent = sender === 'bot' ? parseMarkdown(content) : content;
    messageBody += `${finalContent}</div>`;

    htmlContent += messageBody;

    messageDiv.innerHTML = htmlContent;
    chatMessages.appendChild(messageDiv);
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

// Mostrar sugerencia de volver al menú (Safe Navigation)
function showSafetyNet() {
    const safetyHtml = `<div class="safety-net">
        <p>¿Te fue útil esta información? ¿Deseas explorar un tema de mi guía oficial o prefieres continuar con la conversación libre?</p>
        <div style="display: flex; gap: 10px; flex-wrap: wrap;">
            <button class="mini-menu-btn" onclick="showMainMenu()">🏠 Ver Menú Principal</button>
            <button class="mini-menu-btn" style="border-color: #999; color: #666;" onclick="this.parentElement.parentElement.remove()">💬 Continuar Conversación</button>
        </div>
    </div>`;
    appendMessage(safetyHtml, 'bot');
}

// Mostrar el menú principal
function showMainMenu() {
    let menuHtml = `<div class="menu-container">
        <h3>Selecciona un tema de interés:</h3>
        <div class="menu-grid">`;

    if (chatbotConfig.menu.length === 0) {
        menuHtml += `
            <p style="color: #666; font-size: 0.9em; grid-column: 1/-1;">No se encontraron categorías en la base de datos.</p>
            <button class="mini-menu-btn" onclick="location.reload()" style="grid-column: 1/-1;">🔄 Reintentar Carga</button>
        `;
    } else {
        chatbotConfig.menu.forEach(item => {
            const iconHtml = item.icono ? `<i class="${item.icono}" style="margin-right: 8px;"></i>` : "";
            menuHtml += `<button class="menu-btn" onclick="showSubMenu('${item.id}')">${iconHtml}${item.titulo}</button>`;
        });
    }

    menuHtml += `</div></div>`;
    appendMessage(menuHtml, 'bot');
}

// Mostrar sub-preguntas de una categoría (Nivel 2)
function showSubMenu(id) {
    const category = chatbotConfig.menu.find(m => m.id === id);
    if (!category) return;

    let subMenuHtml = `<div class="menu-container">
        <h3>${category.titulo}</h3>
        <p style="font-size: 0.9em; color: #666; margin-bottom: 10px;">Selecciona una pregunta principal para profundizar:</p>
        <div class="menu-grid">`;

    category.preguntas.forEach((p, index) => {
        // Al hacer clic, mostramos el menú de cascada (Nivel 3)
        subMenuHtml += `<button class="question-btn" onclick="showCascadeMenu('${id}', ${index})">${p.q}</button>`;
    });

    subMenuHtml += `<button class="back-btn" onclick="showMainMenu()">⬅️ Volver al menú principal</button>
    </div></div>`;
    appendMessage(subMenuHtml, 'bot');
}

// Mostrar el menú de cascada de sub-preguntas (Nivel 3)
async function showCascadeMenu(catId, qIndex, skipMessages = false) {
    const category = chatbotConfig.menu.find(m => m.id === catId);
    const mainQuestion = category.preguntas[qIndex];

    if (!skipMessages) {
        appendMessage(mainQuestion.q, 'user');
        await saveMessageToSession(mainQuestion.q, false);

        appendMessage(mainQuestion.a, 'bot');
        await saveMessageToSession(mainQuestion.a, true);
    }

    // Si tiene sub-preguntas, las mostramos
    if (mainQuestion.sub && mainQuestion.sub.length > 0) {
        setTimeout(() => {
            let cascadeHtml = `<div class="menu-container">
                <p style="font-weight: bold; color: #5b7bd5; margin-bottom: 10px;">¿Quieres profundizar más sobre este tema?</p>
                <div class="menu-grid">`;

            mainQuestion.sub.forEach((subP, sIndex) => {
                cascadeHtml += `<button class="question-btn" style="text-align: left;" onclick="answerQuestion('${catId}', ${qIndex}, ${sIndex})">
                    ${subP.q}
                </button>`;
            });

            cascadeHtml += `<button class="back-btn" onclick="showSubMenu('${catId}')">⬅️ Ver otras preguntas de este tema</button>
            </div></div>`;
            appendMessage(cascadeHtml, 'bot');
        }, 1200);
    } else {
        setTimeout(() => {
            showSafetyNet();
        }, 1200);
    }
}

// Responder a una pregunta de Nivel 3
async function answerQuestion(catId, qIndex, subIndex) {
    const category = chatbotConfig.menu.find(m => m.id === catId);
    const mainQuestion = category.preguntas[qIndex];
    const subQuestion = mainQuestion.sub[subIndex];

    appendMessage(subQuestion.q, 'user');
    await saveMessageToSession(subQuestion.q, false);

    appendMessage(subQuestion.a, 'bot');
    await saveMessageToSession(subQuestion.a, true);

    setTimeout(() => {
        const afterAnswerHtml = `<div class="safety-net">
            <p>¿Qué deseas hacer ahora?</p>
            <div style="display: flex; gap: 10px; flex-wrap: wrap;">
                <button class="mini-menu-btn" onclick="showCascadeMenu('${catId}', ${qIndex}, true)">🔄 Ver otros niveles de esta pregunta</button>
                <button class="mini-menu-btn" onclick="showSubMenu('${catId}')">📑 Otra pregunta del tema</button>
                <button class="mini-menu-btn" style="background: #f3f4f6;" onclick="showMainMenu()">🏠 Menú Principal</button>
            </div>
        </div>`;
        appendMessage(afterAnswerHtml, 'bot');
    }, 1500);
}

// Función para inicializar el flujo del chatbot (llamada desde fuera)
async function startChatbotFlow() {
    const chatMessages = document.getElementById('chatMessages');
    if (!chatMessages) return;

    // Cargar menú dinámico si está vacío
    if (chatbotConfig.menu.length === 0) {
        await loadDynamicMenu();
    }

    // Limpiar mensajes previos si se desea un reinicio total o si está vacío
    chatMessages.innerHTML = '';

    appendMessage(chatbotConfig.bienvenida, 'bot');

    // Verificar si hay una categoría predefinida en la URL (Dashboard)
    const urlParams = new URLSearchParams(window.location.search);
    const categoryParam = urlParams.get('category');

    if (categoryParam && chatbotConfig.menu.some(m => m.id === categoryParam)) {
        const cat = chatbotConfig.menu.find(m => m.id === categoryParam);
        appendMessage(`Has seleccionado **${cat.titulo}**. Aquí tienes las preguntas clave:`, 'bot');
        showSubMenu(categoryParam);
    } else {
        showMainMenu();
    }
}

// Detectar si la consulta tiene intención médica/salud (Refinado)
function isMedicalQuery(text) {
    const medicalKeywords = [
        "medicamento", "pastilla", "dosis", "fármaco", "salud", "enfermedad", "síntoma",
        "dolor", "tratamiento", "antibiótico", "bacteria", "virus", "infección",
        "médico", "receta", "presión", "arterial", "corazón", "diabetes", "farmacia",
        "vacuna", "alergia", "efecto", "secundario", "intoxicación", "jarabe",
        "vitamina", "suplemento", "sangre", "examen", "cuerpo", "organismo", "vida",
        "droga", "medicina", "doctor", "clínica", "hospital", "recetado", "tomar",
        "medir", "tensión", "ritmo", "cardíaco", "glucosa", "insulina", "pastillas",
        "paracetamol", "ibuprofeno", "aspirina", "amoxicilina", "loratadina", "omeprazol"
    ];

    // Si contiene temas ultra conocidos no médicos, es falso (aunque lo enviamos a IA)
    const blackList = ["messi", "ronaldo", "fútbol", "futbol", "película", "musica", "cancion", "receta de cocina"];

    const lowerText = text.toLowerCase();

    if (blackList.some(no => lowerText.includes(no))) return false;

    return medicalKeywords.some(k => lowerText.includes(k)) || medicalKeywords.some(k => {
        const regex = new RegExp(`\\b${k}\\b`, 'i');
        return regex.test(lowerText);
    });
}

// Variables globales para manejo de consultas pendientes de IA
let pendingMessage = null;
let pendingImage = null;

// Detectar palabras clave en el mensaje del usuario (mejorado)
function checkKeywords(text) {
    const lowerText = text.toLowerCase().trim();

    // Si es una pregunta larga o estructurada, priorizamos a MedIQ (IA)
    if (text.length > 40 || text.includes('?')) return null;

    // Buscamos coincidencia de palabra completa para evitar falsos positivos
    for (const category of chatbotConfig.menu) {
        if (category.keywords && category.keywords.some(k => {
            const regex = new RegExp(`\\b${k}\\b`, 'i');
            return regex.test(lowerText);
        })) {
            return category.id;
        }
    }
    return null;
}

document.addEventListener('DOMContentLoaded', function () {
    const chatMessages = document.getElementById('chatMessages');
    const messageInput = document.getElementById('messageInput');
    const sendBtn = document.getElementById('sendBtn');
    const imageInput = document.getElementById('imageInput');
    const loadingIndicator = document.getElementById('loadingIndicator');

    // La inicialización ahora la controla chat-sessions.js llamando a startChatbotFlow()

    window.handleSend = async function () {
        const message = messageInput.value.trim();
        const imageFile = imageInput.files[0];

        if (!message && !imageFile) return;

        // Mostrar el mensaje del usuario con imagen local si existe
        const localImageUrl = imageFile ? URL.createObjectURL(imageFile) : null;
        appendMessage(message || (imageFile ? "Imagen enviada" : ""), 'user', localImageUrl);

        messageInput.value = '';
        messageInput.style.height = 'auto';
        resetImageInput();

        // 1. Control de Mensajes Breves
        if (!imageFile && message.length < 3) {
            appendMessage("Parece que tu mensaje es muy corto. Por favor selecciona un tema de mi menú:", 'bot');
            showMainMenu();
            return;
        }

        // 2. Palabras clave (Menú automático)
        const matchedId = checkKeywords(message);
        if (matchedId && !imageFile) {
            appendMessage("He detectado que te interesa este tema. Aquí tienes opciones relacionadas:", 'bot');
            showSubMenu(matchedId);
            return;
        }

        // 3. Selección de Detalle Inteligente
        pendingMessage = message;
        pendingImage = imageFile;

        if (imageFile || isMedicalQuery(message)) {
            // Si parece médico, preguntamos el nivel de detalle
            showDetailSelector();
        } else {
            // Si no parece médico, NO mostramos el menú de detalle (para no molestar)
            // Pero SÍ lo enviamos a la IA para que ella dé la respuesta de rechazo oficial.
            processIA(true);
        }
    }

    // Muestra el menú de selección de detalle
    window.showDetailSelector = function () {
        const selectorHtml = `
            <div class="menu-container" id="detailSelector">
                <p>¿Qué nivel de detalle prefieres para esta respuesta?</p>
                <div style="display: flex; gap: 10px; flex-wrap: wrap;">
                    <button class="mini-menu-btn" onclick="processIA(true)">⏱️ Respuesta Corta</button>
                    <button class="mini-menu-btn" onclick="processIA(false)">📚 Respuesta Detallada</button>
                </div>
            </div>`;
        appendMessage(selectorHtml, 'bot');
    };

    // Envía finalmente la consulta a n8n
    window.processIA = async function (isShort) {
        const selector = document.getElementById('detailSelector');
        if (selector) selector.parentElement.parentElement.remove(); // Quitar el menú de selección

        const message = pendingMessage;
        const imageFile = pendingImage;

        const identityRules = "\n\n[INSTRUCCIONES PARA LA IA]:\nEres MedIQ, experto en uso responsable de medicamentos. Solo responde sobre medicina/salud. Si preguntan fuera de tema, declina amablemente.";
        const formatRules = "\nUsa párrafos, listas y saltos de línea. [SUG]: pregunta1 | pregunta2";
        const lengthInstruction = isShort ? " (Respuesta corta)" : " (Respuesta detallada)";

        // Enviamos el mensaje limpio PRIMERO para que n8n/Pinecone lo use como búsqueda
        // y las instrucciones después separadas por un delimitador claro.
        const finalInput = message + identityRules + lengthInstruction + formatRules;

        loadingIndicator.classList.add('active');
        chatMessages.scrollTop = chatMessages.scrollHeight;

        try {
            const formData = new FormData();
            formData.append('chatInput', finalInput);
            if (imageFile) formData.append('image', imageFile);

            const response = await fetch('/Chat/SendMessage', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const data = await response.json();
                await saveMessageToSession(message || "Imagen enviada", false, data.imagenUrl);

                if (data.alerta) {
                    const alertaHtml = `<div style="background-color: #fee2e2; border: 1px solid #ef4444; color: #b91c1c; padding: 10px; border-radius: 8px; margin-bottom: 10px;"><strong>${data.alerta}</strong></div>`;
                    appendMessage(alertaHtml, 'bot');
                    await saveMessageToSession(data.alerta, true);
                }

                // Procesar texto y sugerencias dinámicas
                let rawText = data.texto || "Lo siento, no pude procesar eso.";
                let cleanText = rawText;
                let suggestions = [];

                if (rawText.includes("[SUG]:")) {
                    const parts = rawText.split("[SUG]:");
                    cleanText = parts[0].trim();
                    const sugPart = parts[1].trim();
                    suggestions = sugPart.split("|").map(s => s.trim().replace(/\?$/, "") + "?");
                }

                appendMessage(cleanText, 'bot');
                await saveMessageToSession(cleanText, true);

                // Mostrar Sugerencias Dinámicas si existen
                setTimeout(() => {
                    if (suggestions.length > 0) {
                        const sugHtml = `
                            <div class="menu-container" style="border: 1px dashed var(--blue-300); background: var(--blue-50);">
                                <p style="font-size: 0.85em; font-weight: bold; color: var(--blue-700); margin-bottom: 8px;">💡 Tal vez te interese saber:</p>
                                <div style="display: flex; flex-direction: column; gap: 8px;">
                                    ${suggestions.map(s => `
                                        <button class="question-btn" style="text-align: left;" onclick="document.getElementById('messageInput').value='${s}'; handleSend(); this.parentElement.parentElement.remove();">
                                            ${s}
                                        </button>
                                    `).join('')}
                                </div>
                            </div>`;
                        appendMessage(sugHtml, 'bot');
                    }
                    showSafetyNet();
                }, 1500);
            }
        } catch (error) {
            console.error("Error:", error);
            appendMessage("No hay conexión con el servidor.", 'bot');
        } finally {
            loadingIndicator.classList.remove('active');
            chatMessages.scrollTop = chatMessages.scrollHeight;
            pendingMessage = null;
            pendingImage = null;
        }
    };

    sendBtn.addEventListener('click', window.handleSend);
    if (messageInput) {
        messageInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                window.handleSend();
            }
        });
    }

    const imageBtn = document.getElementById('imageBtn');
    if (imageBtn && imageInput) {
        imageBtn.addEventListener('click', () => imageInput.click());
        imageInput.addEventListener('change', (e) => {
            const file = e.target.files[0];
            if (file) {
                imageBtn.style.color = '#5b7bd5';
                imageBtn.style.backgroundColor = '#eef2ff';
                // Añadir un indicador visual de que hay una imagen seleccionada
                const indicator = document.createElement('span');
                indicator.id = 'imageIndicator';
                indicator.innerHTML = ` 📎 ${file.name.substring(0, 10)}...`;
                indicator.style.fontSize = '12px';
                indicator.style.color = '#5b7bd5';
                imageBtn.after(indicator);
            }
        });
    }

    function resetImageInput() {
        if (imageInput) imageInput.value = '';
        if (imageBtn) {
            imageBtn.style.color = '';
            imageBtn.style.backgroundColor = '';
            const indicator = document.getElementById('imageIndicator');
            if (indicator) indicator.remove();
        }
        if (messageInput) messageInput.placeholder = "Escribe tu pregunta...";
    }
});
