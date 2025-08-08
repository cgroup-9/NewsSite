// Check if running in development (localhost)
function isDevEnv() {
    return location.host.includes("localhost");
}

const port = 7019;
const baseApiUrl = isDevEnv()
    ? `https://localhost:${port}` // Local API
    : "https://proj.ruppin.ac.il/cgroup9/test2/tar1"; // Deployed API

// When the page is ready, set up the chat UI
document.addEventListener("DOMContentLoaded", () => {
    const startBtn = document.querySelector(".avatar button");
    const chatContainer = document.querySelector(".chat-container");

    startBtn.addEventListener("click", () => {
        // Hide start button
        startBtn.style.display = "none";

        // Create chat elements
        const chatBox = document.createElement("div");
        chatBox.className = "chat-box";

        const chatMessages = document.createElement("div");
        chatMessages.id = "chatMessages";
        chatMessages.className = "messages";

        const questionChoices = document.createElement("div");
        questionChoices.className = "question-choices";

        // Predefined questions for admin
        const questions = [
            { id: 1, text: "Which day had the highest number of logins in the past week?" },
            { id: 2, text: "Which day had the highest number of logins in the past month?" },
            { id: 3, text: "Which day had the highest number of saved articles in the past month?" },
            { id: 4, text: "Which category was saved the most in the past month?" },
            { id: 5, text: "Which day had the highest combined activity (logins + shares) in the past six months?" }
        ];

        // Create a button for each question
        questions.forEach(q => {
            const btn = document.createElement("button");
            btn.textContent = q.text;
            btn.className = "question-btn";
            btn.addEventListener("click", () => handleUserQuestion(q.id, q.text));
            questionChoices.appendChild(btn);
        });

        // Add chat messages area and question buttons to the chat box
        chatBox.appendChild(chatMessages);
        chatBox.appendChild(questionChoices);
        chatContainer.appendChild(chatBox);
    });
});

// Handles when an admin clicks a question
function handleUserQuestion(questionId, questionText) {
    const chatMessages = document.getElementById("chatMessages");

    // Show admin's question
    const userMsg = document.createElement("div");
    userMsg.className = "chat-message user";
    userMsg.textContent = questionText;
    chatMessages.appendChild(userMsg);

    // Send request to AI API
    ajaxCall(
        "GET",
        `${baseApiUrl}/api/ai/question/${questionId}`,
        null,
        data => {
            // Show AI's response
            const botMsg = document.createElement("div");
            botMsg.className = "chat-message bot";
            botMsg.textContent = `🤖 ${data.answer}`;
            chatMessages.appendChild(botMsg);
            chatMessages.scrollTop = chatMessages.scrollHeight; // Auto-scroll to bottom
        },
        error => {
            // Show error if request fails
            const botMsg = document.createElement("div");
            botMsg.className = "chat-message bot error";
            botMsg.textContent = "⚠️ Error fetching AI response.";
            chatMessages.appendChild(botMsg);
        }
    );
}
