using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Speech.Synthesis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace YudolePlatformTTSServer;

public class Message
{
    [JsonPropertyName("id")] public string? Id { get; set; }
    [JsonPropertyName("type")] public string? Type { get; set; }
    [JsonPropertyName("service")] public string? Service { get; set; }
    [JsonPropertyName("html")] public string? Html { get; set; }
    [JsonPropertyName("text")] public string? Text { get; set; }
}

internal class Program
{
    private static SpeechSynthesizer synth = new();
    private static ReadOnlyCollection<InstalledVoice> voices = synth.GetInstalledVoices();
    private static Thread speakerThread = new Thread(Speaker); 
    private static readonly Queue<string> msgQueue = new();

    private static void Log(string message, ConsoleColor color = ConsoleColor.White)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private static void Speaker()
    {
        for (;;)
            if (msgQueue.Count > 0)
            {
                var message = msgQueue.Dequeue();
                synth.Speak(message);
            }
            else
            {
                Thread.Sleep(50);
            }
    }

    private static void Main(string[] args)
    {
        synth.SelectVoice("IVONA 2 Maxim OEM");
        speakerThread.Start();

        for (;;)
        {
            try
            {
                var socket = new TcpClient();
                socket.Connect("127.0.0.1", 1337);

                if (socket.Connected) Log("Connection with YudolePlatform server established", ConsoleColor.Green);

                var writer = new BinaryWriter(socket.GetStream());
                writer.Write("Hello 101"u8.ToArray());

                var reader = new StreamReader(socket.GetStream());
                for (;;)
                    try
                    {
                        var message = reader.ReadLine();
                        Console.WriteLine(message);

                        if (message == null)
                        {
                            socket.Close();
                            break;
                        }

                        var msg = JsonSerializer.Deserialize<Message>(message);
                        Log(msg.Text, ConsoleColor.DarkYellow);
                        msgQueue.Enqueue(msg.Text);
                    }
                    catch (Exception e)
                    {
                        Log(e.Message, ConsoleColor.Red);
                        socket.Close();
                        break;
                    }
            }
            catch (Exception e)
            {
                Log(e.Message, ConsoleColor.Red);
            }

            Log("Connection with YudolePlatform server has been broken, reconnecting", ConsoleColor.Yellow);
            Thread.Sleep(3000);
        }
    }
}



// Console.WriteLine("Available voices:");
// for (var i = 0; i < voices.Count; i++)
// {
//     if (!voices[i].Enabled) continue;
//
//     Console.Write(i + 1);
//     Console.Write(". ");
//     Console.ForegroundColor = ConsoleColor.Yellow;
//     Console.Write(voices[i].VoiceInfo.Name);
//     Console.ForegroundColor = ConsoleColor.Green;
//     Console.Write(" [");
//     Console.Write(voices[i].VoiceInfo.Culture.DisplayName);
//     Console.WriteLine("]");
//     Console.ResetColor();
// }
//
// try
// {
//     synth.SelectVoice("IVONA 2 Maxim OEM");
// }
// catch (Exception e)
// {
//     Console.ForegroundColor = ConsoleColor.Red;
//     Console.WriteLine(e.Message);
//     Console.ResetColor();
// }
//
// synth.Speak("Hello world from speech server");
// // Console.ReadKey();