namespace OAuthImplementation.Views.SignIn
{
    public class Email
    {
        public string Id { get; set; }
        public string ThreadId { get; set; }
        public string Subject { get; set; }
        public string Snippet { get; set; }
    }

    public class EmailListResponse
    {
        public List<EmailListItem> Messages { get; set; }
        // Add other properties as needed to represent the email list response
    }

    public class EmailListItem
    {
        public string Id { get; set; }
        // Add other properties as needed for the email list item
    }
}