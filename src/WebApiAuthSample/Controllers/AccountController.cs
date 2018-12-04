using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net;

namespace WebApiAuthSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        [HttpPost]
        [AllowAnonymous] //ログイン機能自体は認証無しで使えるようにする
        public async Task<IActionResult> Login([FromBody] LoginInputModel model)
        {
            //認証
            bool authResult = (await AuthenticateAsync(model.UserName, model.Password));
            if (authResult == false)
            {
                return new CustomJsonResult(HttpStatusCode.BadRequest, "User name or password is incorrect.");
            }

            //認可処理してトークンを作成
            var token = GenerateToken(model.UserName);

            var result = new
            {
                Token = token,
                UserName = model.UserName,
                ExpiresIn = AuthConfig.ApiJwtExpirationSec
            };
            return new CustomJsonResult(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// 認証処理
        /// </summary>
        private async Task<bool> AuthenticateAsync(string userName, string password)
        {
            //何かしらの認証処理（Kerberos認証したり、LDAPしたり、よしなに）
            await Task.CompletedTask;
            //今はユーザ名/パスワードがhoge/hugaならtrue
            return userName == "hoge" && password == "huga";
        }

        /// <summary>
        /// 認可処理
        /// </summary>
        private List<Claim> Authorize(string userName)
        {
            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, userName));

            //何かしらの認可処理（グループ付けたり、ロール付けたり、よしなに）
            string groupId = "piyo";
            claims.Add(new Claim(ClaimTypes.GroupSid, groupId));

            return claims;
        }

        /// <summary>
        /// JWTトークンを発行する
        /// </summary>
        /// <param name="userName">ユーザ名</param>
        /// <param name="expiresIn">有効期限（秒）</param>
        private string GenerateToken(string userName)
        {
            //トークンに含めるクレームの入れ物
            List<Claim> claims = Authorize(userName);

            //現在時刻をepochタイムスタンプに変更
            var now = DateTime.UtcNow;
            long epochTime = (long)Math.Round((now.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);

            //JWT ID（トークン生成ごとに一意になるようなトークンのID）。ランダムなGUIDを採用する。
            string jwtId = Guid.NewGuid().ToString();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName)); //Subject ユーザ名を指定する
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, jwtId));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, epochTime.ToString(), ClaimValueTypes.Integer64)); //Issured At 発行日時

            //期限が切れる時刻
            DateTime expireDate = now + TimeSpan.FromSeconds(AuthConfig.ApiJwtExpirationSec);

            // Json Web Tokenを生成
            var jwt = new JwtSecurityToken(
                AuthConfig.ApiJwtIssuer, //発行者(iss)
                AuthConfig.ApiJwtAudience, //トークンの受け取り手（のリスト）
                claims, //付与するクレーム(sub,jti,iat)
                now, //開始時刻(nbf)（not before = これより早い時間のトークンは処理しない）
                expireDate, //期限(exp)
                new SigningCredentials(AuthConfig.ApiJwtSigningKey, SecurityAlgorithms.HmacSha256) //署名に使うCredential
                );
            //トークンを作成（トークンは上記クレームをBase64エンコードしたものに署名をつけただけ）
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        /// <summary>
        /// ログインの入力モデル
        /// </summary>
        public class LoginInputModel
        {
            /// <summary>ユーザ名</summary>
            public string UserName { get; set; }
            /// <summary>パスワード</summary>
            public string Password { get; set; }
        }
    }
}