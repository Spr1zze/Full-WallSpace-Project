using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;
using System.Linq;

namespace wallspace.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;

        // Constructor that takes AppDbContext and assigns it to the _context field
        public IndexModel(AppDbContext context)
        {
            _context = context;
        }

        // Property for holding chat messages to be displayed on the Razor page
        //public List<ChatMessage> ChatHistory { get; set; }

        public List <Chatrooms> chatrooms { get; set; }

        // OnGet method retrieves chat messages from the database
        public void OnGet()
        {
            //ChatHistory = _context.Messages.ToList();  // Fetching chat messages from the database
            //chatrooms = _context.Chatrooms.ToList();
        }
    }
}
