using Microsoft.EntityFrameworkCore;
using wallspace;

public class AppDbContext : DbContext 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Users> Users { get; set; }  // <-- Remember to add the equvelent when making a new table on the database :D
    public DbSet<Passwords> Passwords { get; set; }
    public DbSet<Images> Images { get; set; }
    public DbSet<Chatrooms> Chatrooms { get; set; }
    public DbSet<Messages> Messages { get; set; }
    public DbSet<Bind_Chatrooms_Users> Bind_Chatrooms_Users { get; set; }
    public DbSet<Chatroom_Names> Chatroom_Names { get; set; }
    public DbSet<Bind_User_User> Bind_User_User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {                                               //28 - lambda expressions
        modelBuilder.Entity<Passwords>().HasKey(p => p.Id);     // Explicitly set the primary key
        modelBuilder.Entity<Passwords>().Property(p => p.Id)  // Makes it auto increment
        .ValueGeneratedOnAdd();


        modelBuilder.Entity<Chatrooms>().HasKey(p => p.Id);     // Explicitly set the primary key
        modelBuilder.Entity<Chatrooms>().Property(p => p.Id)  // Makes it auto increment
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Messages>().HasKey(p => p.Id);     // Explicitly set the primary key
        modelBuilder.Entity<Messages>().Property(p => p.Id)  // Makes it auto increment
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Bind_Chatrooms_Users>().HasKey(p => p.Id);     // Explicitly set the primary key
        modelBuilder.Entity<Bind_Chatrooms_Users>().Property(p => p.Id)  // Makes it auto increment
        .ValueGeneratedOnAdd();

        modelBuilder.Entity<Bind_User_User>().HasKey(p => p.Id);     // Explicitly set the primary key
        modelBuilder.Entity<Bind_User_User>().Property(p => p.Id)  // Makes it auto increment
        .ValueGeneratedOnAdd();

        //School wifi sucks,
        //so here are some notes
        //to migrate the cs tables to the actual database
        //dotnet ef migrations add {Some really describing message}
        //to update the database
        //dotnet ef database update
        //Hope this helps :D


    }
}
