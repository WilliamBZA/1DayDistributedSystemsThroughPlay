using nanoFramework.WebServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Exercise4
{
    public class FileController
    {
        [Route("api/files")]
        [Method("GET")]
        public void GetAllFiles(WebServerEventArgs e)
        {
            var sampleFiles = "\"data.bin\", \"2020-04-24.log\", \"passwords.txt\"";
            string output = $"{{\"files\": [{sampleFiles}]}}";

            e.Context.Response.ContentType = "application/json";
            e.Context.Response.ContentLength64 = output.Length;
            WebServer.OutputHttpCode(e.Context.Response, HttpStatusCode.OK);
            WebServer.OutPutStream(e.Context.Response, output);
        }
    }
}