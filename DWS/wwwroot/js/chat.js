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
    nombre: "Asistente MedIQ",
    bienvenida: "¡Hola! Soy MedIQ, tu asistente inteligente para concientizar sobre el uso responsable de medicamentos. Estoy aquí para informarte, pero recuerda: **no doy diagnósticos ni recetas médicas**.",

    menu: [
        {
            id: "medicacion",
            titulo: "💊 Guía de Medicación y Uso Responsable",
            keywords: ["medicacion", "medicamento", "pastilla", "medicina", "dosis", "horario"],
            preguntas: [
                { q: "¿En qué consiste exactamente la automedicación?", a: "La automedicación consiste en el consumo de medicamentos por iniciativa propia, sin la intervención de un profesional de la salud. Hacerlo de forma incorrecta puede enmascarar enfermedades graves o causar intoxicaciones." },
                { q: "¿Qué elementos debo revisar al leer una receta o etiqueta médica?", a: "Es vital revisar el nombre del fármaco, la dosis (ej. 500mg), la frecuencia (ej. cada 8h) y la duración total del tratamiento. Nunca modifiques estos parámetros sin consultar a tu médico." },
                { q: "¿Por qué es fundamental respetar los horarios indicados?", a: "Para que un medicamento sea efectivo, debe mantener una concentración constante en tu sangre. Si saltas dosis o cambias el horario, pierdes eficacia y puedes generar resistencia al tratamiento." },
                { q: "¿Qué debo hacer ante el olvido de una dosis?", a: "Si te das cuenta pocas horas después, tómala. Pero si ya falta poco para la siguiente, sáltala. **Nunca tomes una dosis doble** para compensar, ya que aumenta el riesgo de efectos tóxicos." }
            ]
        },
        {
            id: "resistencia",
            titulo: "🛡️ Peligros de la Resistencia a los Antibióticos",
            keywords: ["resistencia", "antibiotico", "bacteria", "ciclo", "flora", "virus"],
            preguntas: [
                { q: "¿Qué es la resistencia bacteriana a los antibióticos?", a: "Ocurre cuando las bacterias cambian para sobrevivir al uso de antibióticos. Esto hace que las infecciones comunes vuelvan a ser peligrosas y difíciles de tratar a nivel mundial." },
                { q: "¿Los antibióticos sirven para tratar la gripe o el resfriado común?", a: "No. Los antibióticos **solo matan bacterias**, no virus. La gripe y el resfriado son virales, por lo que tomar antibióticos en estos casos solo daña tu flora intestinal y genera resistencia." },
                { q: "¿Es seguro interrumpir el tratamiento de antibióticos antes de tiempo?", a: "¡No! Aunque te sientas mejor, debes terminar el ciclo indicado. Si lo dejas antes, las bacterias más fuertes sobreviven, se multiplican y se vuelven resistentes al tratamiento." },
                { q: "¿Cómo afecta el mal uso de antibióticos a la salud global (One Health)?", a: "Bajo el enfoque 'One Health', sabemos que el mal uso de antibióticos en humanos y animales contamina el ambiente, creando 'superbacterias' que ponen en riesgo la medicina moderna." }
            ]
        },
        {
            id: "mitos",
            titulo: "⚖️ Mitos, Realidades y Precauciones",
            keywords: ["mito", "natural", "hierba", "conocido", "alergia", "efecto"],
            preguntas: [
                { q: "¿Puedo usar medicamentos recomendados por otras personas?", a: "No. Cada persona tiene un historial clínico, peso y alergias distintas. Lo que ayudó a un conocido podría causarte una reacción alérgica o interactuar mal con otros fármacos que ya tomes." },
                { q: "¿Son siempre inofensivos los productos naturales?", a: "Es un mito común. Los productos naturales tienen compuestos químicos que también pueden causar efectos secundarios graves o interactuar peligrosamente con medicamentos convencionales." },
                { q: "¿Cuáles son las señales de una reacción adversa a un farmaco?", a: "Si notas sarpullidos, dificultad para respirar, hinchazón en la cara o mareos intensos tras tomar un fármaco, busca atención médica de urgencia. No esperes a que pase." },
                { q: "¿Cuándo es indispensable acudir a un médico profesional?", a: "Siempre que presentes síntomas nuevos o persistentes. Este chat es educativo; si te sientes mal, necesitas una evaluación profesional presencial." }
            ]
        }
    ]
};

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

    chatbotConfig.menu.forEach(item => {
        menuHtml += `<button class="menu-btn" onclick="showSubMenu('${item.id}')">${item.titulo}</button>`;
    });

    menuHtml += `</div></div>`;
    appendMessage(menuHtml, 'bot');
}

// Mostrar sub-preguntas de una categoría
function showSubMenu(id) {
    const category = chatbotConfig.menu.find(m => m.id === id);
    if (!category) return;

    let subMenuHtml = `<div class="menu-container">
        <h3>${category.titulo}</h3>
        <div class="menu-grid">`;

    category.preguntas.forEach((p, index) => {
        subMenuHtml += `<button class="question-btn" onclick="answerQuestion('${id}', ${index})">${p.q}</button>`;
    });

    subMenuHtml += `<button class="back-btn" onclick="showMainMenu()">⬅️ Volver al menú principal</button>
    </div></div>`;
    appendMessage(subMenuHtml, 'bot');
}

// Responder a una pregunta específica
async function answerQuestion(catId, qIndex) {
    const category = chatbotConfig.menu.find(m => m.id === catId);
    const question = category.preguntas[qIndex];

    // Añadir mensaje del usuario para el flujo
    appendMessage(question.q, 'user');
    await saveMessageToSession(question.q, false);

    // Responder
    appendMessage(question.a, 'bot');
    await saveMessageToSession(question.a, true);

    // Volver a mostrar el submenú después de un momento
    setTimeout(() => {
        showSubMenu(catId);
    }, 1000);
}

// Función para inicializar el flujo del chatbot (llamada desde fuera)
function startChatbotFlow() {
    const chatMessages = document.getElementById('chatMessages');
    if (!chatMessages) return;

    // Limpiar mensajes previos si se desea un reinicio total o si está vacío
    chatMessages.innerHTML = '';

    appendMessage(chatbotConfig.bienvenida, 'bot');

    // Verificar si hay una categoría preddefinida en la URL (Dashboard)
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

    async function handleSend() {
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

        // Reglas de formato para la IA
        const formatRules = "\nINSTRUCCIÓN DE FORMATO: Usa varios párrafos, saltos de línea y listas numeradas para que la información sea fácil de leer. No escribas todo en un solo bloque de texto.";
        const lengthInstruction = isShort ? " (Responde de forma muy breve y directa)" : " (Responde de forma detallada y educativa)";

        const finalInput = message + lengthInstruction + formatRules;

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

                appendMessage(data.texto || "Lo siento, no pude procesar eso.", 'bot');
                await saveMessageToSession(data.texto, true);

                setTimeout(() => showSafetyNet(), 1500);
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

    sendBtn.addEventListener('click', handleSend);
    if (messageInput) {
        messageInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSend();
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
