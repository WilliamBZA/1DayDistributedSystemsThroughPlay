using nanoFramework.WebServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Exercise4
{
    public class LedController
    {
        [Route("api/toggleleds")]
        [Method("POST")]
        public void ToggleLeds(WebServerEventArgs e)
        {
            Program.ToggleLEDs();

            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }

        [Route("api/toggleremoteleds")]
        [Method("POST")]
        public void ToggleTemoteLeds(WebServerEventArgs e)
        {
            // replace IP address with other device's IP
            using var request = WebRequest.Create("http://192.168.1.113/api/toggleleds");
            request.Method = "GET";

            using var response = request.GetResponse();

            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
        }
    }
}