using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Services;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddScoped<UrlShortener>();

WebApplication app = builder.Build();
UrlShortener shortener = new UrlShortener();


app.MapPost("/login", async (string username, string password, DBContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.username == username && u.password == password);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(user);
});

app.MapGet("/get_links", (DBContext db) =>
{
    var shortenedLinks = db.ShortenedLinks;

    if (shortenedLinks is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(shortenedLinks);
});

app.MapGet("/get_users", (DBContext db) =>
{
    var users = db.Users;

    if (users is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(users);
});

app.MapGet("/get_link/{id:int}", (int id, DBContext db) =>
{
    var shortenedLink = db.ShortenedLinks.FirstOrDefaultAsync(u => u.Id == id);

    if (shortenedLink is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(shortenedLink);
});

app.MapPost("/add_link", async (string originalUrl, int userId, DBContext db) =>
{
    if (await db.ShortenedLinks.FirstOrDefaultAsync(record => record.originalUrl == originalUrl) != null)
    {
        return Results.Conflict();
    }

    string shortenedUrlUniquePart = await shortener.ShortenUrlAsync(db, originalUrl, userId);

    ShortenedLink shortenedLink = new ShortenedLink()
    {
        originalUrl = originalUrl,
        shortenedUrl = shortenedUrlUniquePart,
        shortenedBy = userId,
        shortenedAt = DateTime.UtcNow
    };
    db.ShortenedLinks.Add(shortenedLink);
    await db.SaveChangesAsync();

    return Results.Ok(shortenedLink);
});

app.MapGet("/short/{shortUrl}", async (string shortUrl, DBContext db) =>
{
    var shortenedLink = await db.ShortenedLinks.FirstOrDefaultAsync(link => link.shortenedUrl == shortUrl);

    if (shortenedLink is null)
    {
        return Results.NotFound("Short URL not found.");
    }

    return Results.Redirect(shortenedLink.originalUrl);
});

app.Run();
