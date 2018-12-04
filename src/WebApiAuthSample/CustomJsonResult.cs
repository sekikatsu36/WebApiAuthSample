using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebApiAuthSample
{
    /// <summary>
    /// Json形式のレスポンスを返すためのActionResultクラス
    /// </summary>
    public class CustomJsonResult : JsonResult
    {
        /// <summary>
        /// 日付フォーマット。
        /// yyyy-MM-dd'T'HH:mm:ss.ffK(UTCだとZになる)
        /// </summary>
        private const string DateFormat = "yyyy-MM-dd'T'HH:mm:ss.ffK";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="code">ステータスコード</param>
        /// <param name="data">データ</param>
        public CustomJsonResult(HttpStatusCode code, object data)
            : base(data)
        {
            base.StatusCode = (int)code;
        }

        /// <summary>
        /// MVCのアクションメソッドの結果を処理
        /// </summary>
        /// <param name="context">実行コンテキスト</param>
        /// <inheritdoc />
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new UnauthorizedAccessException("The context of this HTTP request is not defined.");
            }

            HttpResponse response = context.HttpContext.Response;
            await SerializeJsonAsync(response);
        }

        /// <summary>
        /// 指定されたデータをJSONにシリアライズしレスポンスに格納
        /// </summary>
        /// <param name="response">格納するレスポンス</param>
        public async Task SerializeJsonAsync(HttpResponse response)
        {
            if (!String.IsNullOrEmpty(ContentType))
            {
                //MIME設定
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json; charset=utf-8";
            }

            // Content Sniffering 対策
            response.Headers.Add("X-Content-Type-Options", "nosniff");

            // キャッシュ回避
            response.Headers.Add("Pragma", "no-cache");
            response.Headers.Add("Cache-Control", "no-store, no-cache");

            // クロスサイトスクリプティング防御機構を有効化
            response.Headers.Add("X-XSS-Protection", "1; mode=block");

            //CORS設定。クロスドメインアクセスが必要なら、適宜設定する。
            if (response.Headers.ContainsKey("Access-Control-Allow-Origin"))
            {
                response.Headers["Access-Control-Allow-Origin"] = "*";
            }
            else
            {
                response.Headers.Add("Access-Control-Allow-Origin", "*");
            }
            //response.Headers.Add("Access-Control-Allow-Credentials", "true");
            //response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, X-CSRF-Token, X-Requested-With, Accept, Accept-Version, Content-Length, Content-MD5, Date, X-Api-Version, X-File-Name");
            //response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");

            // HTTPS対応
            response.Headers.Add("Strict-Transport-Security", "max-age=15768000");

            response.StatusCode = StatusCode == null ? StatusCodes.Status200OK : StatusCode.Value;
            if (Value != null)
            {
                // Json.NETでシリアライズ
                var converter = new IsoDateTimeConverter();
                converter.DateTimeStyles = System.Globalization.DateTimeStyles.AdjustToUniversal; //時刻はUTCで
                converter.DateTimeFormat = DateFormat;

                // 結果をレスポンスボディに書き込み
                await response.WriteAsync(JsonConvert.SerializeObject(
                    Value, new JsonSerializerSettings()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver(),
                        Converters = new List<JsonConverter>() { converter },
                        Formatting = Formatting.Indented,
                        StringEscapeHandling = StringEscapeHandling.Default,
                    }),
                    Encoding.UTF8 //指定しなくてもデフォルトでUTF8になるが、念のため明記
                );
                return;
            }
            else if (response.StatusCode != StatusCodes.Status204NoContent) //NoContentは結果を書けないので、それ以外の場合だけ空で埋める
            {
                await response.WriteAsync("", Encoding.UTF8);
            }
            return;
        }
    }
}
