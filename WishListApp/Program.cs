// Program.cs
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WishListApp;
using WishListApp.Interfaces;
using WishListApp.Models;
using WishListApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste your JWT token here"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity with AddIdentityCore — no cookie scheme interference
builder.Services.AddIdentityCore<User>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager();

// CORS — needed because React runs on a different port during development.
// AllowCredentials() is essential so cookies are included in cross-origin requests.
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()                           // required for cookies to work cross-origin
              .WithOrigins(builder.Configuration["Frontend:BaseUrl"]!);
    });
});

builder.Services.AddAuthentication(opt =>
    {
        // JWT handles all regular API authentication
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        // The temporary holding area for Google's identity during the OAuth handshake
        opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    // Cookie scheme: only used as the temporary sign-in scheme during Google's OAuth handshake.
    // Not used for any API authentication — that's all JWT.
    .AddCookie()
    // Google: note SignInScheme explicitly pointing to Cookie.
    // This is what allows the OAuth handshake to work alongside JWT.
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"]
            ?? throw new InvalidOperationException("Google ClientId is not configured.");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]
            ?? throw new InvalidOperationException("Google ClientSecret is not configured.");

        // Cookie scheme temporarily holds Google's identity during the callback
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        options.Scope.Add("profile"); // request name and avatar
        options.SaveTokens = true;
    })
    // JWT: handles all [Authorize] checks on your API endpoints
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };

        // Read the JWT from the HttpOnly cookie rather than the Authorization header.
        // The browser sends the cookie automatically — the frontend doesn't manage it.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["ACCESS_TOKEN"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor(); // needed by AuthTokenProcessor to write cookies
builder.Services.AddAutoMapper(typeof(Program));

// Register your services
builder.Services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<IWishListItemService, WishListItemService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWishListAccessService, WishListAccessService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CorsPolicy"); // must be before auth middleware
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();