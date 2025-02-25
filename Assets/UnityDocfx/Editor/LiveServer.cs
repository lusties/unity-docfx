using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Lustie.UnityDocfx
{
    public class LiveServer : IDisposable
    {
        const string http = "http://localhost:";
        private readonly HttpListener listener;
        private readonly string folderPath;
        private readonly int port;
        private Thread serverThread;

        public LiveServer(string folderPath, int port)
        {
            if (!Directory.Exists(folderPath))
                throw new ArgumentException("Folder does not exist: " + folderPath);

            this.folderPath = folderPath;
            this.port = port;
            listener = new HttpListener();
            listener.Prefixes.Add($"{http}{port}/");
        }

        public void Run()
        {
            serverThread = new Thread(() =>
            {
                listener.Start();
                OpenUrl(GetUrl());
                ListenAsync().Wait();
            });
            serverThread.IsBackground = true;
            serverThread.Start();
        }

        private async Task ListenAsync()
        {
            while (listener.IsListening)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequestAsync(context));
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Listen error: " + ex.Message);
                }
            }
        }

        private async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string relativePath = context.Request.Url.AbsolutePath.TrimStart('/');
            if (string.IsNullOrEmpty(relativePath))
                relativePath = "index.html";

            string filePath = Path.Combine(folderPath, relativePath);

            if (File.Exists(filePath))
            {
                try
                {
                    byte[] buffer = await File.ReadAllBytesAsync(filePath);
                    context.Response.ContentType = GetContentType(filePath);
                    context.Response.ContentLength64 = buffer.Length;
                    await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = 500;
                    using var writer = new StreamWriter(context.Response.OutputStream);
                    await writer.WriteAsync("Server error: " + ex.Message);
                }
            }
            else
            {
                context.Response.StatusCode = 404;
                using var writer = new StreamWriter(context.Response.OutputStream);
                await writer.WriteAsync("File not found");
            }
            context.Response.OutputStream.Close();
        }

        private static string GetContentType(string fileName)
        {
            return Path.GetExtension(fileName).ToLower() switch
            {
                ".html" => "text/html",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot open URL: " + ex.Message);
            }
        }

        public void Dispose()
        {
            listener?.Stop();
            listener?.Close();
            serverThread?.Join();
        }

        public string GetUrl() => $"http://localhost:{port}/";

        public static implicit operator bool(LiveServer server)
            => server is not null;
    }
}