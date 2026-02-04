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
                {
                    q: "¿En qué consiste exactamente la automedicación?",
                    a: "La automedicación es el uso de medicamentos por iniciativa propia sin receta médica. Puede ser peligroso si no se hace bajo supervisión profesional, ya que puede enmascarar enfermedades reales.",
                    sub: [
                        { q: "¿Qué diferencia hay entre automedicación y autocuidado?", a: "El autocuidado es elegir hábitos saludables (dieta, ejercicio), mientras que la automedicación es usar fármacos sin receta para tratar síntomas. El autocuidado previene, la automedicación mal hecha pone en riesgo." },
                        { q: "¿Cuáles son los riesgos de ocultar síntomas graves?", a: "Automedicarse para un dolor de estómago fuerte podría 'tapar' una apendicitis. Al no sentir el dolor, no buscas ayuda profesional y una condición tratable puede volverse mortal." },
                        { q: "¿Cómo afecta la automedicación a la seguridad del paciente?", a: "Aumenta la probabilidad de interacciones peligrosas, errores en la dosis y desarrollo de alergias no detectadas, según la OMS es una de las mayores amenazas para la salud pública." }
                    ]
                },
                {
                    q: "¿Qué elementos debo revisar al leer una receta o etiqueta médica?",
                    a: "Leer la receta es clave para entender la dosis exacta, la frecuencia y la duración total del tratamiento, evitando errores que comprometan tu recuperación.",
                    sub: [
                        { q: "¿Dónde encuentro la fecha de vencimiento y por qué importa?", a: "Suele estar en el borde del blister o la caja. Tomar medicina vencida es peligroso porque los componentes químicos se degradan y pueden volverse tóxicos o perder su efecto." },
                        { q: "¿Qué significa 'Vía de Administración' (Oral, Tópica, etc.)?", a: "Indica cómo debe entrar el fármaco al cuerpo. Si pones gotas para el oído en el ojo, o tragas una pastilla que era sublingual, el medicamento no funcionará o causará daño." },
                        { q: "¿Cómo identifico excipientes que podrían darme alergia?", a: "En el prospecto (papel interno), busca la lista de excipientes. Sustancias como lactosa o gluten pueden causar reacciones graves en personas sensibles." }
                    ]
                },
                {
                    q: "¿Por qué es fundamental respetar los horarios indicados?",
                    a: "Respetar los horarios garantiza que el medicamento mantenga niveles estables en tu sangre durante todo el día, asegurando que el tratamiento realmente funcione.",
                    sub: [
                        { q: "¿Es lo mismo '3 veces al día' que 'cada 8 horas'?", a: "No. '3 veces' puede ser aleatorio (desayuno, almuerzo, cena). 'Cada 8 horas' es estricto para mantener el nivel de fármaco estable en sangre durante las 24 horas del día." },
                        { q: "¿Qué es la 'Ventana Terapéutica' de un medicamento?", a: "Es el rango exacto de dosis donde el fármaco cura. Si bajas de ahí no sirve; si subes de ahí se vuelve veneno para tus órganos (riñón o hígado)." },
                        { q: "¿Cómo influyen los alimentos en la absorción del fármaco?", a: "Algunos fármacos necesitan grasa para absorberse, otros se bloquean con el calcio de la leche. Seguir la instrucción 'con alimentos' o 'en ayunas' determina si la medicina entra a tu sangre." }
                    ]
                },
                {
                    q: "¿Qué debo hacer ante el olvido de una dosis?",
                    a: "Ante un olvido, lo más importante es no entrar en pánico. Debes evaluar cuánto tiempo ha pasado, pero recuerda: **nunca tomes doble dosis**.",
                    sub: [
                        { q: "¿Existe alguna 'regla de tiempo' para tomarla tarde?", a: "Generalmente, si te acuerdas antes de la mitad del tiempo para la siguiente dosis, tómala. Si falta poco para la siguiente, es mejor esperar y seguir con el horario normal." },
                        { q: "¿Por qué NUNCA debo duplicar la dosis para compensar?", a: "Duplicar la dosis NO arregla el olvido, solo sobrecarga tus riñones e hígado con una cantidad tóxica que tu cuerpo no puede procesar de golpe." },
                        { q: "¿Qué riesgos hay en tratamientos críticos como anticonceptivos?", a: "En tratamientos donde la hormona es constante, un olvido de más de 12 horas puede anular la eficacia totalmente. En estos casos, se debe usar un método de barrera (preservativo) adicional." }
                    ]
                }
            ]
        },
        {
            id: "resistencia",
            titulo: "🛡️ Peligros de la Resistencia a los Antibióticos",
            keywords: ["resistencia", "antibiotico", "bacteria", "ciclo", "flora", "virus"],
            preguntas: [
                {
                    q: "¿Qué es la resistencia bacteriana a los antibióticos?",
                    a: "La resistencia bacteriana ocurre cuando las bacterias aprenden a sobrevivir a los antibióticos. Esto hace que infecciones comunes vuelvan a ser peligrosas y difíciles de tratar.",
                    sub: [
                        { q: "¿Cómo hacen las bacterias para volverse 'superbacterias'?", a: "Las bacterias mutan y desarrollan 'escudos' o bombas para expulsar el antibiótico. Al reproducirse, pasan este 'superpoder' a otras bacterias, creando una familia resistente." },
                        { q: "¿Cuál es la diferencia entre resistencia natural y adquirida?", a: "La natural es propia de la bacteria. La adquirida ocurre por culpa nuestra: al usar mal los antibióticos obligamos a la bacteria a aprender cómo sobrevivir." },
                        { q: "¿Por qué la OMS considera esto una amenaza para la humanidad?", a: "Si los antibióticos dejan de funcionar, cirugías simples o partos volverán a ser mortales por infecciones que hoy consideramos fáciles de curar." }
                    ]
                },
                {
                    q: "¿Los antibióticos sirven para tratar la gripe o el resfriado común?",
                    a: "Los antibióticos NO sirven para combatir virus como la gripe. Usarlos sin necesidad solo daña tu flora intestinal y ayuda a crear bacterias más resistentes.",
                    sub: [
                        { q: "¿Por qué un antibiótico no mata a un virus?", a: "Los antibióticos atacan la estructura física de la bacteria (su pared). Los virus no tienen esa estructura, por lo que el antibiótico simplemente no tiene nada a qué atacar." },
                        { q: "¿Qué pasa con mi flora intestinal si tomo antibióticos sin necesidad?", a: "El antibiótico mata a las bacterias 'buenas' de tu vientre. Esto causa diarreas, debilita tus defensas y deja el camino libre a hongos y bacterias malas." },
                        { q: "¿Qué medicamentos sí son efectivos para síntomas virales?", a: "Para virus se usan analgésicos, hidratación y reposo. Los antibióticos NO bajan la fiebre ni quitan el moco si la causa es un virus." }
                    ]
                },
                {
                    q: "¿Es seguro interrumpir el tratamiento de antibióticos antes de tiempo?",
                    a: "Nunca dejes un tratamiento de antibióticos a la mitad. Aunque te sientas mejor, debes terminar la caja para asegurar que no sobreviva ninguna bacteria fuerte.",
                    sub: [
                        { q: "¿Por qué me siento bien antes de terminar la caja?", a: "Porque el antibiótico mató a las bacterias más débiles primero. Las que quedan vivas son las más fuertes y peligrosas; si dejas de tomarlo, esas sobrevivientes te volverán a enfermar peor." },
                        { q: "¿Qué sucede con las bacterias que 'sobreviven' al corte?", a: "Se vuelven líderes de una nueva infección que ya sabe cómo resistir a ese antibiótico. La próxima vez que lo tomes, ya no te servirá de nada." },
                        { q: "¿Cómo se crea una infección recurrente por falta de adherencia?", a: "Al no terminar el ciclo, dejas focos de infección dormidos que despertarán en semanas o meses con mucha más agresividad." }
                    ]
                },
                {
                    q: "¿Cómo afecta el mal uso de antibióticos a la salud global (One Health)?",
                    a: "El mal uso de fármacos afecta a humanos, animales y al medio ambiente por igual. Es un problema global que genera un entorno lleno de bacterias resistentes.",
                    sub: [
                        { q: "¿Qué tiene que ver la salud de los animales con la mía?", a: "Si se usan antibióticos para engordar pollos o vacas, las bacterias de esos animales se vuelven resistentes y saltan a los humanos a través de la comida o el contacto." },
                        { q: "¿Cómo llegan los antibióticos de la granja a nuestras mesas?", a: "A través del agua contaminada con desechos animales y el consumo de carne mal cocida que contiene bacterias que ya aprendieron a ser súper resistentes." },
                        { q: "¿Cómo afecta el desecho de medicinas al medio ambiente?", a: "Tirar medicinas al baño contamina ríos. Las bacterias del agua aprenden a resistir a esos fármacos, creando un ambiente donde hasta el agua puede ser foco de superbacterias." }
                    ]
                }
            ]
        },
        {
            id: "mitos",
            titulo: "⚖️ Mitos, Realidades y Precauciones",
            keywords: ["mito", "natural", "hierba", "conocido", "alergia", "efecto"],
            preguntas: [
                {
                    q: "¿Puedo usar medicamentos recomendados por otras personas?",
                    a: "Lo que le sirvió a un conocido podría ser tóxico para ti. Cada cuerpo es único y un fármaco 'seguro' para otro puede causarte una reacción grave.",
                    sub: [
                        { q: "¿Por qué lo que le sirve a un vecino me puede hacer daño a mí?", a: "Tu genética, historial de alergias y el estado de tus riñones son un mundo aparte. Un fármaco 'seguro' para tu vecino puede darte un ataque al corazón o insuficiencia renal a ti." },
                        { q: "¿Cómo influye el peso y la edad en la dosis de cada persona?", a: "Un niño o un anciano procesan los fármacos mucho más lento. Darle una dosis de adulto a un niño puede causar daños cerebrales o la muerte por sobredosis." },
                        { q: "¿Qué son las interacciones medicamentosas cruzadas?", a: "Es cuando un fármaco choca con otro que ya tomas. El recomendado por tu amigo podría anular tu medicina para la presión o causar una hemorragia interna." }
                    ]
                },
                {
                    q: "¿Son siempre inofensivos los productos naturales?",
                    a: "Es un mito que 'Natural' significa inofensivo. Muchas plantas medicinales tienen químicos potentes que pueden dañar tu hígado si se usan mal.",
                    sub: [
                        { q: "¿Significa 'Natural' que no tiene efectos secundarios?", a: "¡No! El veneno de serpiente es natural. Muchas plantas medicinales causan toxicidad hepática grave si se consumen en dosis incorrectas." },
                        { q: "¿Pueden las hierbas anular el efecto de mis medicinas?", a: "Sí. Por ejemplo, la hierba de San Juan anula el efecto de muchos anticonceptivos y antidepresivos. Lo natural también es químico." },
                        { q: "¿Por qué falta regulación en la dosis de productos botánicos?", a: "A diferencia de las pastillas, una planta puede tener más o menos veneno dependiendo de donde creció. No hay control exacto de cuánto químico 'natural' estás tragando." }
                    ]
                },
                {
                    q: "¿Cuáles son las señales de una reacción adversa a un farmaco?",
                    a: "Conocer las señales de una reacción adversa (como ronchas, picazón o falta de aire) te permite actuar rápido y evitar complicaciones vitales.",
                    sub: [
                        { q: "¿Cómo distingo un efecto secundario de una alergia?", a: "Un efecto secundario es 'esperado' (ej. sueño). Una alergia es una defensa extrema del cuerpo (ronchas, picazón, ojos hinchados) y es mucho más peligrosa." },
                        { q: "¿Qué es un choque anafiláctico y cómo detectarlo a tiempo?", a: "Es la reacción más grave: se cierra la garganta y baja la presión. Si te cuesta respirar tras una pastilla, es una emergencia vital de vida o muerte." },
                        { q: "¿A qué entidad debo reportar una reacción médica extraña?", a: "Debes avisar a tu médico y, si es posible, al sistema de Farmacovigilancia de tu país para que alerten a otros sobre ese lote de medicina." }
                    ]
                },
                {
                    q: "¿Cuándo es indispensable acudir a un médico profesional?",
                    a: "La consulta médica es la única forma de obtener un diagnóstico real. Este chat es educativo y nunca debe retrasar la atención profesional ante síntomas graves.",
                    sub: [
                        { q: "¿Qué síntomas de alerta requieren ir a urgencias ya mismo?", a: "Dolor de pecho, pérdida de visión, desmayos, fiebre que no baja o sangrados inusuales. No preguntes a un chat, ¡ve al hospital!" },
                        { q: "¿Por qué la receta médica es un documento de seguridad?", a: "La receta confirma que un experto analizó tu cuerpo y decidió que el beneficio de la medicina es mayor que el riesgo. Es tu escudo legal y de salud." },
                        { q: "¿Cuál es el peligro de postergar un diagnóstico real por usar IA?", a: "La IA analiza datos, no a la persona. Confiar ciegamente en un chat para una enfermedad real puede hacer que pierdas meses valiosos de tratamiento para algo grave." }
                    ]
                }
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

// Mostrar sub-preguntas de una categoría (Nivel 2)
function showSubMenu(id) {
    const category = chatbotConfig.menu.find(m => m.id === id);
    if (!category) return;

    let subMenuHtml = `<div class="menu-container">
        <h3>${category.titulo}</h3>
        <p style="font-size: 0.9em; color: #666; margin-bottom: 10px;">Selecciona una pregunta principal para profundizar:</p>
        <div class="menu-grid">`;

    category.preguntas.forEach((p, index) => {
        // Ahora al hacer clic, mostramos el menú de cascada (Nivel 3)
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
        // 1. Añadimos el mensaje del usuario y la respuesta principal
        appendMessage(mainQuestion.q, 'user');
        await saveMessageToSession(mainQuestion.q, false);

        appendMessage(mainQuestion.a, 'bot');
        await saveMessageToSession(mainQuestion.a, true);
    }

    // 2. Después de responder, mostramos el menú de los niveles
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
}

// Responder a una pregunta específica
async function answerQuestion(catId, qIndex, subIndex) {
    const category = chatbotConfig.menu.find(m => m.id === catId);
    const mainQuestion = category.preguntas[qIndex];
    const subQuestion = mainQuestion.sub[subIndex];

    // Añadir mensaje del usuario para el flujo
    appendMessage(subQuestion.q, 'user');
    await saveMessageToSession(subQuestion.q, false);

    // Responder
    appendMessage(subQuestion.a, 'bot');
    await saveMessageToSession(subQuestion.a, true);

    // Sugerencia de seguridad o volver
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

        // Reglas de identidad y comportamiento para la IA
        const identityRules = "\nTU IDENTIDAD: Eres MedIQ, un asistente experto para concientizar sobre el uso responsable de medicamentos. " +
            "REGLA DE ORO: Solo responde preguntas relacionadas con medicina, salud, fármacos y bienestar. " +
            "Si el usuario pregunta algo fuera de este tema (como matemáticas, historia, chistes o temas generales), declina amablemente y recuérdale que tu especialidad es el uso seguro de medicamentos.";

        const formatRules = "\nINSTRUCCIÓN DE FORMATO: Usa varios párrafos, saltos de línea y listas numeradas para que la información sea fácil de leer. No escribas todo en un solo bloque de texto.";
        const lengthInstruction = isShort ? " (Responde de forma muy breve y directa)" : " (Responde de forma detallada y educativa)";

        // Nueva instrucción para preguntas de seguimiento dinámicas (Con restricciones éticas estrictas)
        const suggestionsRule = "\n\nREGLA CRÍTICA DE SUGERENCIAS: Añade 2 sugerencias de preguntas cortas que el USUARIO podría hacerte a TI para profundizar. " +
            "PROHIBIDO: No sugieras preguntas sobre dosis, horarios específicos de toma, recetas o cualquier recomendación médica directa. " +
            "ENFOQUE: Sugiere temas sobre educación, riesgos de la automedicación, qué revisar en etiquetas o cuándo ir al médico. " +
            "Usa exactamente este formato al final: '[SUG]: pregunta1 | pregunta2'";

        const finalInput = message + identityRules + lengthInstruction + formatRules + suggestionsRule;

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
