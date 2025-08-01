function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019; // הפורט הנכון שלך
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}`
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1";

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
            { id: 1, text: "Which day had the highest number of logins in the past week?" },
            { id: 2, text: "Which day had the highest number of logins in the past month?" },
            { id: 3, text: "Which day had the highest number of saved articles in the past month?" },
            { id: 4, text: "Which category was saved the most in the past month?" },
            { id: 5, text: "Which day had the highest combined activity (logins + shares) in the past six months?" }
        ];

        questions.forEach(q => {
            const btn = document.createElement("button");
            btn.textContent = q.text;
            btn.className = "question-btn";
            btn.addEventListener("click", () => handleUserQuestion(q.id, q.text));
            questionChoices.appendChild(btn);
        });

        chatBox.appendChild(chatMessages);
        chatBox.appendChild(questionChoices);
        chatContainer.appendChild(chatBox);
    });
});

function handleUserQuestion(questionId, questionText) {
    const chatMessages = document.getElementById("chatMessages");

    const userMsg = document.createElement("div");
    userMsg.className = "chat-message user";
    userMsg.textContent = questionText;
    chatMessages.appendChild(userMsg);

    // קריאת AJAX ל-API עם baseApiUrl הנכון
    ajaxCall(
        "GET",
        `${baseApiUrl}/api/ai/question/${questionId}`,
        null,
        function (data) {
            const botMsg = document.createElement("div");
            botMsg.className = "chat-message bot";
            botMsg.textContent = `🤖 ${data.answer}`;
            chatMessages.appendChild(botMsg);
            chatMessages.scrollTop = chatMessages.scrollHeight;
        },
        function (error) {
            const botMsg = document.createElement("div");
            botMsg.className = "chat-message bot error";
            botMsg.textContent = "⚠️ Error fetching AI response.";
            chatMessages.appendChild(botMsg);
        }
    );
}
