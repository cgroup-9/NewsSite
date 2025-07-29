document.addEventListener("DOMContentLoaded", () => {
    const startBtn = document.querySelector(".avatar button");
    const chatContainer = document.querySelector(".chat-container");

    startBtn.addEventListener("click", () => {
        startBtn.style.display = "none";

        const chatBox = document.createElement("div");
        chatBox.className = "chat-box";

        const chatMessages = document.createElement("div");
        chatMessages.id = "chatMessages";
        chatMessages.className = "messages";

        const questionChoices = document.createElement("div");
        questionChoices.className = "question-choices";

        const questions = [
            "How many users logged in during the past week?",
            "How many users registered in the past month?",
            "How many users registered in the past six months?",
            "Which category was saved the most in the last month?",
            "Which day had the highest activity in the past week?"
        ];

        questions.forEach(q => {
            const btn = document.createElement("button");
            btn.textContent = q;
            btn.className = "question-btn";
            btn.addEventListener("click", () => handleUserQuestion(q));
            questionChoices.appendChild(btn);
        });

        chatBox.appendChild(chatMessages);
        chatBox.appendChild(questionChoices);
        chatContainer.appendChild(chatBox);
    });
});

function handleUserQuestion(questionText) {
    const chatMessages = document.getElementById("chatMessages");

    const userMsg = document.createElement("div");
    userMsg.className = "chat-message user";
    userMsg.textContent = questionText;
    chatMessages.appendChild(userMsg);

    // Simulated bot response – will later connect to real AI
    setTimeout(() => {
        const botMsg = document.createElement("div");
        botMsg.className = "chat-message bot";
        botMsg.textContent = "🤖 Analyzing your data... (simulated response)";
        chatMessages.appendChild(botMsg);
        chatMessages.scrollTop = chatMessages.scrollHeight;
    }, 800);
}
