using AutoMapper;
using DevAdventCalendarCompetition.Repository;
using DevAdventCalendarCompetition.Repository.Context;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.Services;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DevAdventCalendarCompetition.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddErrorDescriber<CustomIdentityErrorDescriber>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/LogIn");

            services.AddTransient<IAdminRepository, AdminRepository>();
            services.AddTransient<IBaseTestRepository, BaseTestRepository>();
            services.AddTransient<IHomeRepository, HomeRepository>();

            return services;
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IAdminService, AdminService>();
            services.AddTransient<IBaseTestService, BaseTestService>();
            services.AddTransient<IHomeService, HomeService>();
            services.AddTransient<IManageService, ManageService>();
            services.AddTransient<IdentityService>();
            return services;
        }

        public static IServiceCollection RegisterMapping(this IServiceCollection services)
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Test, TestDto>();
                cfg.CreateMap<TestDto, Test>();

                cfg.CreateMap<TestAnswer, TestAnswerDto>();
                cfg.CreateMap<TestAnswer, TestWithAnswerListDto>();
            });
            return services;
        }

        public static IServiceCollection AddExternalLoginProviders(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication()
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = configuration["Authentication:Facebook:AppSecret"];
                })
                .AddTwitter(twitterOptions =>
                {
                    twitterOptions.ConsumerKey = configuration["Authentication:Twitter:ConsumerKey"];
                    twitterOptions.ConsumerSecret = configuration["Authentication:Twitter:ConsumerSecret"];
                })
                .AddGoogle(googleOptions =>
                {
                    googleOptions.ClientId = configuration["Authentication:Google:ClientId"];
                    googleOptions.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                })
                .AddGitHub(githubOptions =>
                {
                    githubOptions.ClientId = configuration["Authentication:GitHub:ClientId"];
                    githubOptions.ClientSecret = configuration["Authentication:GitHub:ClientSecret"];
                    githubOptions.Scope.Add("user:email");
                });

            return services;
        }
    }
}