using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.IO;

namespace Ajapaik.Helpers
{
    public class PostSubmitter
    {
        public string url { get; set; }
        public Dictionary<string, object> parameters { get; set; }
        string boundary = "----------" + DateTime.Now.Ticks.ToString();
        public event EventHandler theEvent;

        public PostSubmitter() { }

        public void Submit()
        {
            // Prepare web request...
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            myRequest.Method = "POST";
            myRequest.ContentType = string.Format("multipart/form-data; boundary={0}", boundary);

            myRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), myRequest);
        }

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            Stream postStream = request.EndGetRequestStream(asynchronousResult);

            writeMultipartObject(postStream, parameters);
            postStream.Close();

            request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            streamResponse.Close();
            streamRead.Close();
            // Release the HttpWebResponse
            response.Close();
            theEvent.Invoke(this, null);
        }


        public void writeMultipartObject(Stream stream, object data)
        {
            StreamWriter writer = new StreamWriter(stream);
            if (data != null)
            {
                foreach (var entry in data as Dictionary<string, object>)
                {
                    WriteEntry(writer, entry.Key, entry.Value);
                }
            }
            writer.Write("--");
            writer.Write(boundary);
            writer.WriteLine("--");
            writer.Flush();
        }

        private void WriteEntry(StreamWriter writer, string key, object value)
        {
            if (value != null)
            {
                writer.Write("--");
                writer.WriteLine(boundary);
                if (value is byte[])
                {
                    byte[] ba = value as byte[];

                    writer.WriteLine(@"Content-Disposition: form-data; name=""{0}""; filename=""{1}""", key, "sentPhoto.jpg");
                    writer.WriteLine(@"Content-Type: application/octet-stream");
                    //writer.WriteLine(@"Content-Type: image / jpeg");
                    writer.WriteLine(@"Content-Length: " + ba.Length);
                    writer.WriteLine();
                    writer.Flush();
                    Stream output = writer.BaseStream;

                    output.Write(ba, 0, ba.Length);
                    output.Flush();
                    writer.WriteLine();
                }
                else
                {
                    writer.WriteLine(@"Content-Disposition: form-data; name=""{0}""", key);
                    writer.WriteLine();
                    writer.WriteLine(value.ToString());
                }
            }
        }
    }
}
