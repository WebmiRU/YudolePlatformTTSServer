#pragma warning disable CA1416

using System.Net.Sockets;
using System.Speech.Synthesis;
using System.Text.Json;

namespace YudolePlatformTTSServer;

internal class Program
{
    private static readonly SpeechSynthesizer synth = new();
    private static string? voiceSelected;
    private static readonly Thread speakerThread = new(Speaker);
    private static readonly Queue<Message> msgQueue = new();

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
                synth.Speak(message.Text);
            }
            else
            {
                Thread.Sleep(50);
            }
    }

    private static int Main(string[] args)
    {
        voiceSelected = "IVONA 2 Maxim OEM";
        speakerThread.Start();

        Log("Available voices:");
        var voices = synth.GetInstalledVoices();


        for (var i = 0; i < voices.Count; i++)
        {
            if (!voices[i].Enabled) continue;

            Console.Write("{0}. ", i + 1);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(voices[i].VoiceInfo.Name);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" [{0}]", voices[i].VoiceInfo.Culture.DisplayName);
            Console.ResetColor();
        }

        Console.Write("You select voice: \"");
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write(voiceSelected);
        Console.ResetColor();
        Console.WriteLine("\"");

        try
        {
            synth.SelectVoice(voiceSelected);
        }
        catch (Exception e)
        {
            Log(e.Message, ConsoleColor.Red);
            return -1;
        }

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

                        if (message == null)
                        {
                            socket.Close();
                            break;
                        }

                        Message? msg;
                        try
                        {
                            msg = JsonSerializer.Deserialize<Message>(message);
                        }
                        catch (Exception e)
                        {
                            Log("Error while encoding JSON message: " + e.Message, ConsoleColor.Red);
                            continue;
                        }

                        switch (msg.Type)
                        {
                            case "message/speak":
                                msgQueue.Enqueue(msg);

                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write("Speaking: \"");
                                Console.ResetColor();
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.Write(msg.Text);
                                Console.ResetColor();
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.WriteLine("\"");
                                Console.ResetColor();
                                break;

                            default:
                                Log("Unknown message type: " + msg.Type, ConsoleColor.Red);
                                break;
                        }

                        {
                        }
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