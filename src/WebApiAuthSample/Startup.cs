using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApiAuthSample
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
            #region services.AddAuthentication
            services.AddAuthentication(
                //既定の認証スキーマ。
                JwtBearerDefaults.AuthenticationScheme
            )
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true, // 署名キー検証
                        IssuerSigningKey = AuthConfig.ApiJwtSigningKey,
                        ValidateIssuer = true,
                        ValidIssuer = AuthConfig.ApiJwtIssuer, // iss（issuer）クレーム
                        ValidateAudience = true, // aud（audience）クレーム
                        ValidAudience = AuthConfig.ApiJwtAudience,
                        ValidateLifetime = true, // トークンの有効期限の検証
                        ClockSkew = TimeSpan.Zero // クライアントとサーバーの間の時刻の設定で許容される最大の時刻のずれ
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            //トークンを正しく取れたときの処理。ログとか出したいときはここに何か書く。
                            // var token = context.SecurityToken as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
                            return Task.FromResult(0);
                        },
                        OnChallenge = context =>
                        {
                            string errorMessage = context.AuthenticateFailure != null ?
                                "The Access code is expired or invalid." : // アクセスコードが不正な文字列で復元できない場合
                                "The access code is required."; // アクセスコードがヘッダに設定されていない場合

                            // 失敗した際のメッセージをレスポンスに格納する
                            context.Response.OnStarting(async state =>
                            {
                                // アクセスコードがヘッダに設定されていない場合はこちらに入る

                                await new CustomJsonResult(HttpStatusCode.Unauthorized,
                                    new
                                    {
                                        Type = this.GetType().FullName,
                                        Title = errorMessage,
                                        Instance = context.Request?.Path.Value
                                    }
                                    ).SerializeJsonAsync(((JwtBearerChallengeContext)state).Response);
                                return;
                            }, context);
                            return Task.FromResult(0);
                        },
                    };
                });
            #endregion

            services.AddSwaggerGen(options =>
            {
                // APIの署名を記載
                options.SwaggerDoc("v1", new Info { Title = "Sample API", Version = "v1" });

                //トークン認証用のUIを追加する
                options.AddSecurityDefinition("api_key", new ApiKeyScheme()
                {
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey", //この指定が必須。https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/124
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
                });

                // 入力したトークンをリクエストに含めるためのフィルタを追加
                options.OperationFilter<AssignJwtSecurityRequirements>();
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "SAMPLE API V1");
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
