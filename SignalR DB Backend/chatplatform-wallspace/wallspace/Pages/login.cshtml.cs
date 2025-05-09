using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace wallspace.Pages
{
    public class loginModel : PageModel
    {
        private readonly AppDbContext _context;

        // Constructor that takes AppDbContext and assigns it to the _context field
        public loginModel(AppDbContext context)
        {
            _context = context;
        }

        public List<Users> users { get; set; }
        public List<Passwords> passwords { get; set; }

        public void OnGet()
        {
            users = _context.Users.ToList();
            passwords = _context.Passwords.ToList();
        }
    }
}
