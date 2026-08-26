using EduCode.WebMVC.Services;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.Name = ".EduCode.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

string apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("Falta ApiSettings:BaseUrl.");

builder.Services.AddHttpClient<IEduCodeApiClient, EduCodeApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(65);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    IHeaderDictionary headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' https://esm.sh https://cdn.jsdelivr.net https://unpkg.com; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: blob:; font-src 'self'; " +
        "connect-src 'self' https://esm.sh https://cdn.jsdelivr.net https://unpkg.com; " +
        "object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    await next();
});

var staticFileContentTypes = new FileExtensionContentTypeProvider();
staticFileContentTypes.Mappings[".glb"] = "model/gltf-binary";
staticFileContentTypes.Mappings[".gltf"] = "model/gltf+json";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = staticFileContentTypes,
    OnPrepareResponse = context =>
    {
        PathString requestPath = context.Context.Request.Path;
        bool isEduCodeRobotAsset =
            requestPath.StartsWithSegments("/models/educode") ||
            requestPath.StartsWithSegments("/js/educode-robot");

        if (isEduCodeRobotAsset)
        {
            // Los recursos del robot están versionados en la URL. Reutilizarlos
            // evita volver a transferir/preparar varios MB en cada navegación.
            context.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
        }
        else if (context.File.Name.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) ||
                 context.File.Name.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers["Cache-Control"] = "public,max-age=86400";
        }

        if (context.File.Name.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) ||
            context.File.Name.EndsWith(".gltf", StringComparison.OrdinalIgnoreCase))
        {
            context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        }
    }
});
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
