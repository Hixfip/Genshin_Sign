using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Genshin_Sign
{
    class Program
    {
        public static string act_id = "e202009291139501";
        public static string cookie;
        public static string uid;
        public static string url_role = "https://api-takumi.mihoyo.com/binding/api/getUserGameRolesByCookie?game_biz=hk4e_cn";
        public static string url_sign = "https://api-takumi.mihoyo.com/event/bbs_sign_reward/sign";
        public static string url_award = "https://api-takumi.mihoyo.com/event/bbs_sign_reward/home?act_id=" + act_id;
        public static string url_info = "https://api-takumi.mihoyo.com/event/bbs_sign_reward/info?act_id=" + act_id + "&region=cn_gf01&uid=";
        static void Main(string[] args)
        {
            Console.WriteLine("请输入米游社Cookie：");
            cookie = Console.ReadLine();
            
            Role role = Get_Role();
            uid = role.data.list[0].game_uid;
            url_info += uid;
            
            Response_Sign info = Sign();
            Console.WriteLine(info.retcode + info.message);

            SignInfo signinfo = Get_SignInfo();
            Console.WriteLine("总共签到 " + signinfo.data.total_sign_day + " 天");

            Award award = Get_Award();
            Console.WriteLine("今日奖励：" + award.data.awards[signinfo.data.total_sign_day - 1].name + "×" + award.data.awards[signinfo.data.total_sign_day - 1].cnt);
        }


        public static Award Get_Award()
        {
            RestClient client = new RestClient(url_award);
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.GET);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<Award>(response.Content);
        }

        public static SignInfo Get_SignInfo()
        {
            RestClient client = new RestClient(url_info);
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", cookie);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<SignInfo>(response.Content);
        }

        public static Role Get_Role()
        {
            RestClient client = new RestClient(url_role);
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.GET);
            request.AddHeader("Cookie", "ltoken=S8I5JkmCNbsSYi3QZD5KV6SIP35G69o5kuQBtnXW; ltuid=83600683; login_ticket=lqn9qlk2YK04kIgJSyFnyOiXZQiUZHgQdpjzREm3; account_id=83600683; cookie_token=kOfG2CYRnIefAb0DDz9geAKcvPTQccmAJT3rlhzg; _ga_KJ6J9V9VZQ=GS1.1.1609941959.1.1.1609941986.0; _ga=GA1.1.591506173.1609941959; account_id=83600683; cookie_token=kOfG2CYRnIefAb0DDz9geAKcvPTQccmAJT3rlhzg");
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<Role>(response.Content);
        }

        public static Response_Sign Sign()
        {
            RestClient client = new RestClient(url_sign);
            client.Timeout = -1;
            RestRequest request = new RestRequest(Method.POST);
            client.UserAgent = "Mozilla/5.0 (Linux; Android 6.0.1; MuMu Build/V417IR; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/52.0.2743.100 Mobile Safari/537.36 miHoYoBBS/2.4.0";
            //request.AddHeader("Referer", "https://webstatic.mihoyo.com/bbs/event/signin-ys/index.html?bbs_auth_required=true&act_id=" + act_id + "&utm_source=bbs&utm_medium=mys&utm_campaign=icon");
            //request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Content-Type", "text/plain");
            request.AddHeader("x-rpc-device_id", "fa498beb-eddf-345d-84e1-a3145b225309");
            request.AddHeader("x-rpc-client_type", "5");
            request.AddHeader("x-rpc-app_version", "2.2.1");
            request.AddHeader("DS", Get_DS());
            request.AddHeader("Cookie", cookie);
            request.AddParameter("text/plain", "{" +
                "\"act_id\":\"e202009291139501\"," +
                "\"region\":\"cn_gf01\"," +
                "\"uid\":\"" + uid + "\"" +
                "}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<Response_Sign>(response.Content);
        }

        static string Get_DS()
        {
            string n = "cx2y9z9a29tfqvr1qsq6c7yz99b5jsqt";
            string i = GetTimeStamp();
            string r = GetRandomString(6);
            string c = GenerateMD5(string.Format("salt={0}&t={1}&r={2}", n, i, r));
            return string.Format("{0},{1},{2}", i, r, c);
        }

        /// <summary>
        /// MD5字符串加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns>加密后字符串</returns>
        public static string GenerateMD5(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes(txt);
                //开始加密
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        /// <summary>
        /// 随机生成字母与数字组合的字符串
        /// </summary>
        /// <param name="length">/param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            byte[] r = new byte[length];
            Random rand = new Random((int)(DateTime.Now.Ticks % 1000000));
            //生成8字节原始数据
            for (int i = 0; i < length; i++)
            {
                int ran;
                //while循环剔除非字母和数字的随机数
                do
                {
                    //数字范围是ASCII码中字母数字和一些符号
                    ran = rand.Next(48, 122);
                    r[i] = Convert.ToByte(ran);
                } while ((ran >= 58 && ran <= 64) || (ran >= 91 && ran <= 96));
            }
            //转换成8位String类型               
            return Encoding.ASCII.GetString(r);
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

    }

    #region 签到实体类

    public class Response_Sign
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_Sign data { get; set; }
    }
    public class Data_Sign
    {
        public string code { get; set; }
    }

    #endregion

    #region 角色信息实体类

    public class Role
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_Role data { get; set; }
    }

    public class Data_Role
    {
        public List[] list { get; set; }
    }

    public class List
    {
        public string game_biz { get; set; }
        public string region { get; set; }
        public string game_uid { get; set; }
        public string nickname { get; set; }
        public int level { get; set; }
        public bool is_chosen { get; set; }
        public string region_name { get; set; }
        public bool is_official { get; set; }
    }

    #endregion

    #region 签到信息实体类

    public class SignInfo
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_SignInfo data { get; set; }
    }

    public class Data_SignInfo
    {
        public int total_sign_day { get; set; }
        public string today { get; set; }
        public bool is_sign { get; set; }
        public bool first_bind { get; set; }
        public bool is_sub { get; set; }
        public bool month_first { get; set; }
    }

    #endregion

    #region 签到奖励

    public class Award
    {
        public int retcode { get; set; }
        public string message { get; set; }
        public Data_Award data { get; set; }
    }

    public class Data_Award
    {
        public int month { get; set; }
        public Awards[] awards { get; set; }
    }

    public class Awards
    {
        public string icon { get; set; }
        public string name { get; set; }
        public int cnt { get; set; }
    }

    #endregion
}
