using EFCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<IdentityContext>(op =>
    op.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnectionDefault"),
        b=>b.MigrationsAssembly("Identity_UI")));


builder.Services.AddAuthentication().AddGoogle(op =>
{
    op.ClientId = builder.Configuration.GetValue<string>(key: "GoogleClientId");
    op.ClientSecret = builder.Configuration.GetValue<string>(key: "ClientSecret");
});
builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>()
    .AddDefaultTokenProviders();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<IdentityContext>();

int port = builder.Configuration.GetValue<int>(key: "Email:port");
string? emailServer = builder.Configuration.GetSection("Email")["SmtpServer"];
builder.Services.AddTransient<IEmailSender>(x => new EmailSender(port, emailServer));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.Run();
