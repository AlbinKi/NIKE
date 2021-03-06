using System;
using System.IO;
using System.Reflection;
using Api.Model;
using Api.Model.MappingProfiles;
using Api.Repository;
using Api.Repository.CommentRepository;
using Api.Services.AuthorizationServices;
using Api.Services.CommentService;
using Api.Services.EntryServices;
using Api.Services.ForecastServices;
using Api.Services.POIServices;
using Api.Services.UserServices;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
             {
                options.AddPolicy("AllowAllHeaders",
                builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithHeaders("Authorization", "Accept", "Content-Type", "Origin");
                });
             });
            services.AddDbContext<NIKEContext>(options =>
            {
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddControllers()
                .AddNewtonsoftJson(x =>
                {
                    x.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    x.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Api", Version = "v1" });
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
            });
            services.AddSwaggerGenNewtonsoftSupport();

            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new UserMapping());
                mc.AddProfile(new POIMapping());
                mc.AddProfile(new EntryMapping());
                mc.AddProfile(new ForecastMapping());
                mc.AddProfile(new WeatherResultMapping());
                mc.AddProfile(new LikeDislikeMapping());

            });

            IMapper _mapper = mapperConfig.CreateMapper();
            services.AddSingleton(_mapper);
            RegisterServices(services);
            RegisterRepositorys(services);
        }

        public void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IPOIService, POIService>();
            services.AddScoped<IEntryService, EntryService>();
            services.AddScoped<IForceastService, ForecastService>();
            services.AddScoped<ICommentService, CommentService>();

        }
        public void RegisterRepositorys(IServiceCollection services)
        {
            services.AddScoped<IRepository<User>, UserRepository>();
            services.AddScoped<IPOIRepository, POIRepository>();
            services.AddScoped<IEntryRepository, EntryRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors("AllowAllHeaders");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
