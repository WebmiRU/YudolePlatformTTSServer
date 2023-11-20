// See https://aka.ms/new-console-template for more information

using System.Speech.Synthesis;

var synth = new SpeechSynthesizer();
var voices = synth.GetInstalledVoices();

Console.WriteLine("Available voices:");
for (var i = 0; i < voices.Count; i++)
{
    Console.Write(i + 1);
    Console.Write(". ");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write(voices[i].VoiceInfo.Name);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write(" [");
    Console.Write(voices[i].VoiceInfo.Culture.DisplayName);
    Console.WriteLine("]");
    Console.ResetColor();
}

synth.SelectVoice("IVONA 2 Maxim OEM");
synth.Speak("Я программа говорильня, привет!");
Console.ReadKey();