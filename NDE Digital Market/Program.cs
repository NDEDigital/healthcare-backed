using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using NDE_Digital_Market.Services.CompanyRegistrationServices;
using NDE_Digital_Market.Services.HK_GetsServices;
using NDE_Digital_Market.Data_Access_Layer;

var builder = WebApplication.CreateBuilder(args);

//CORS
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});
// Add services to the container.
builder.Services.AddControllers();

//injecting user uservices
builder.Services.AddScoped<IHK_Gets, HK_Gets>();
builder.Services.AddScoped<HK_Gets_DAL>();

builder.Services.AddScoped<ICompanyRegistration, CompanyRegistration>();
builder.Services.AddScoped<CompanyRegistration_DAL>();

builder.Services.AddScoped<NDE_Digital_Market.Controllers.UserController>();
builder.Services.AddScoped<NDE_Digital_Market.Services.CompanyRegistrationServices.ICompanyRegistration, NDE_Digital_Market.Services.CompanyRegistrationServices.CompanyRegistration>();
builder.Services.AddScoped<NDE_Digital_Market.Data_Access_Layer.CompanyRegistration_DAL>();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "Standard Authorization header using the Bearer scheme (\"bearer {token}\")",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero

        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
//CORS
app.UseCors(MyAllowSpecificOrigins);
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();


app.Run();
