namespace server.Models
{
    // ==========================
    // UserStatusUpdateRequest
    // ==========================
    // Purpose:
    // - Represents a request sent from the client to update a user's status.
    // - Used in admin panels where an administrator can activate or deactivate a user.
    //
    // Usage Example:
    // - Sent as JSON in a PUT/PATCH request to the server.
    //   {
    //       "id": 5,
    //       "active": true
    //   }
    // - The server reads this model in the controller action and updates the database.
    public class UserStatusUpdateRequest
    {
        // The unique identifier of the user whose status is being updated
        public int Id { get; set; }

        // Represents whether the user is active (true) or deactivated (false)
        public bool Active { get; set; }
    }
}
