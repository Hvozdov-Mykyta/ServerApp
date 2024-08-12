using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Services;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("AllowAnyOrigins", _ =>
    {
        _.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddDbContext<DBContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
builder.Services.AddScoped<UrlShortener>();

WebApplication app = builder.Build();
UrlShortener shortener = new UrlShortener();

app.UseCors("AllowAnyOrigins");


app.MapPost("/login", async (string username, string password, DBContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(user => user.username == username && user.password == password);

    if (user is null)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(user);
});

app.MapGet("/get_urls", (DBContext db) =>
{
    var shortenedUrls = db.ShortenedLinks;

    if (shortenedUrls is null)
    {
        return Results.Ok(null);
    }

    return Results.Ok(shortenedUrls);
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

app.MapGet("/get_url", async (int urlId, DBContext db) =>
{
    var shortenedUrl = await db.ShortenedLinks.FirstOrDefaultAsync(url => url.Id == urlId);

    if (shortenedUrl is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(shortenedUrl);
});

app.MapPost("/add_url", async (string originalUrl, int userId, DBContext db) =>
{
    if (await db.ShortenedLinks.FirstOrDefaultAsync(url => url.originalUrl == originalUrl) != null)
    {
        return Results.Conflict();
    }

    string shortenedUrl = await shortener.ShortenUrlAsync(db, originalUrl, userId);

    ShortenedLink shortenedUrlObject = new ShortenedLink()
    {
        originalUrl = originalUrl,
        shortenedUrl = shortenedUrl,
        shortenedBy = userId,
        shortenedAt = DateTime.UtcNow
    };
    db.ShortenedLinks.Add(shortenedUrlObject);
    await db.SaveChangesAsync();

    return Results.Ok(shortenedUrlObject);
});

app.MapDelete("/delete_url", async (int linkId, DBContext db) =>
{
    var shortenedLink = await db.ShortenedLinks.FirstOrDefaultAsync(record => record.Id == linkId);
    if (shortenedLink == null)
    {
        return Results.NotFound();
    }

    db.ShortenedLinks.Remove(shortenedLink);
    await db.SaveChangesAsync();

    return Results.Ok(shortenedLink);
});

app.MapGet("/short/{shortUrl}", async (string shortUrl, DBContext db) =>
{
    var shortenedLink = await db.ShortenedLinks.FirstOrDefaultAsync(url => url.shortenedUrl == shortUrl);

    if (shortenedLink is null)
    {
        return Results.NotFound();
    }

    return Results.Redirect(shortenedLink.originalUrl);
});

app.Run();
