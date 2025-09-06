using System;
using System.Collections.Generic;

namespace TodoApps
{
    public class TodoTask
    {
        public string FirebaseId { get; set; }
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; }
        public string Category { get; set; } = "Без категории";
        public List<string> Tags { get; set; } = new List<string>();
        public string Recurrence { get; set; } = "None";
        public string TagsString => string.Join(", ", Tags);
    }

    public class FirebaseAuthResponse
    {
        public string idToken { get; set; }
        public string email { get; set; }
        public string refreshToken { get; set; }
        public string localId { get; set; }
        public bool registered { get; set; }
        public string expiresIn { get; set; }
    }
}