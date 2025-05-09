
    public class Messages
    {
        public int Id { get; set; }
        public int chatroom_id { get; set; }
        public int user_id { get; set; }
        public string message { get; set; }

        public DateTime time_stamp { get; set; }

        public Messages()  
                    {
        time_stamp = DateTime.UtcNow;
                    }
    }

