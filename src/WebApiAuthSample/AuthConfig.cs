using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApiAuthSample
{
    public static class AuthConfig
    {

        /// <summary>
        /// APIでトークンのAudience (aud) クレームに指定する文字列。
        /// トークンの受け取り手（のリスト）を表す。
        /// 必要であれば、受け手側で検証を行う。
        /// </summary>
        public const string ApiJwtAudience = "SampleAudience";
        /// <summary>
        /// APIでトークンのIssuer (iss) クレームに指定する文字列。
        /// 発行者を表す。
        /// 必要であれば、受け手側で検証を行う。
        /// </summary>
        public const string ApiJwtIssuer = "SampleIssur";
        /// <summary>
        /// APIでトークンのExpiration (exp) クレームに指定する数値。
        /// トークンの有効期限（秒）。
        /// </suemmary>
        public const int ApiJwtExpirationSec = 60 * 60 * 24; //1日

        /// <summary>
        /// APIで共通鍵の生成に使うパスフレーズ
        /// </summary>
        private const string ApiSecurityTokenPass = "1234567890QWERTYUIOPASDFGHJKLZXCVBNN";

        /// <summary>
        /// APIでトークンの生成に使う共通鍵のシングルトン。
        /// </summary>
        private static SymmetricSecurityKey signingKey;

        /// <summary>
        /// APIでトークンの生成に使う共通鍵を取得する。
        /// </summary>
        public static SymmetricSecurityKey ApiJwtSigningKey
        {
            get
            {
                if (signingKey == null)
                {
                    byte[] key = Encoding.UTF8.GetBytes(ApiSecurityTokenPass, 0, 32);
                    signingKey = new SymmetricSecurityKey(key);
                }
                return signingKey;
            }
        }
    }
}