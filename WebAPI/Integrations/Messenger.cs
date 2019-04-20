using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;

namespace WebAPI.Integrations
{
    public class Messenger
    {
        private const string EndPoint = "http://postmaster.devnestsystems.com/api/v1/messaging";
        private const string ApiKey = "SVSDWV9EFRMYMBDFBYPFOIH1RM4IKVUN";
        private const string Sender = "Borlaman";

        public async Task<Tuple<bool, string>> SendMessage(string phoneNumber, string  message)
        {
            var res = await EndPoint.AppendPathSegment("sendsms").PostJsonAsync(new {
                ApiKey,
                Sender,
                Recipients = phoneNumber,
                Text = message
            }).ReceiveJson<MsgRes>();

            return new Tuple<bool, string>(res.Success, res.Message);
        }


        public class MsgRes
        {
            public long Total { get; set; }
            public string Message { get; set; }
            public bool Success { get; set; }
        }
    }

    
}
