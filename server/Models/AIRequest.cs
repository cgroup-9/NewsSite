namespace server.Models
{
    //
    // ======================================
    // AIRequest Model
    // ======================================
    //
    // Purpose:
    // - Represents a request sent by the client to the AI service.
    // - Contains the question or prompt text that will be processed by the AI.
    //
    public class AIRequest
    {
        // --------------------------
        // Properties
        // --------------------------

        public string Question { get; set; } // The question or prompt to send to the AI model.
    }
}
