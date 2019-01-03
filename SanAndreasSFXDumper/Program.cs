using GTAAudioSharp;
using System;
using System.IO;

/// <summary>
/// San andreas SFX dumper namespace
/// </summary>
namespace SanAndreasSFXDumper
{
    /// <summary>
    /// Program class
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        /// <param name="args">Command line arguments</param>
        static void Main(string[] args)
        {
            try
            {
                string path = null;
                if (args.Length > 0)
                {
                    path = args[0];
                }
                else
                {
                    Console.Write("Please specify a path to dump the audio files from (press return to exit): ");
                    path = Console.ReadLine();
                }
                if (path != null)
                {
                    if (path.Trim().Length > 0)
                    {
                        string sfx_directory = Path.Combine(Environment.CurrentDirectory, "sfx");
                        byte[] buffer = new byte[4096];
                        if (!(Directory.Exists(sfx_directory)))
                        {
                            Directory.CreateDirectory(sfx_directory);
                        }
                        using (GTAAudioFiles gta_audio_files = GTAAudio.OpenRead(path))
                        {
                            if (gta_audio_files != null)
                            {
                                GTAAudioSFXFile[] sfx_audio_files = gta_audio_files.SFXAudioFiles;
                                foreach (GTAAudioSFXFile sfx_audio_file in sfx_audio_files)
                                {
                                    if (sfx_audio_file != null)
                                    {
                                        string sfx_sfx_directory = Path.Combine(sfx_directory, sfx_audio_file.Name);
                                        try
                                        {
                                            if (!(Directory.Exists(sfx_sfx_directory)))
                                            {
                                                Directory.CreateDirectory(sfx_sfx_directory);
                                            }
                                            for (int i = 0, j; i < sfx_audio_file.NumBanks; i++)
                                            {
                                                try
                                                {
                                                    GTAAudioBankData bank_data = sfx_audio_file.GetBankData((uint)i);
                                                    string sfx_bank_directory = Path.Combine(sfx_sfx_directory, i.ToString());
                                                    if (!(Directory.Exists(sfx_bank_directory)))
                                                    {
                                                        Directory.CreateDirectory(sfx_bank_directory);
                                                    }
                                                    for (j = 0; j < bank_data.NumAudioClips; j++)
                                                    {
                                                        using (Stream audio_stream = sfx_audio_file.Open((uint)i, (uint)j))
                                                        {
                                                            if (audio_stream is GTAAudioStream)
                                                            {
                                                                GTAAudioStream gta_audio_stream = (GTAAudioStream)audio_stream;
                                                                string audio_file_path = Path.Combine(sfx_bank_directory, sfx_audio_file.Name + "." + i + "." + j + ".wav");
                                                                if (File.Exists(audio_file_path))
                                                                {
                                                                    File.Delete(audio_file_path);
                                                                }
                                                                using (FileStream file_stream = File.Open(audio_file_path, FileMode.Create))
                                                                {
                                                                    using (BinaryWriter writer = new BinaryWriter(file_stream))
                                                                    {
                                                                        int len;
                                                                        long audio_stream_length = audio_stream.Length;
                                                                        writer.Write("RIFF".ToCharArray());
                                                                        writer.Write(4 + (8 + 16) + (8 + (gta_audio_stream.SampleRate * 1 * 16 / 8)));
                                                                        writer.Write("WAVEfmt ".ToCharArray());
                                                                        writer.Write(16);
                                                                        writer.Write((short)1);
                                                                        writer.Write((short)1);
                                                                        writer.Write((int)(gta_audio_stream.SampleRate));
                                                                        writer.Write(gta_audio_stream.SampleRate * 1 * 16 / 8);
                                                                        writer.Write((short)(1 * 16 / 8));
                                                                        writer.Write((short)16);
                                                                        writer.Write("data".ToCharArray());
                                                                        writer.Write(gta_audio_stream.SampleRate * 1 * 16 / 8);
                                                                        while ((len = Math.Min((int)(audio_stream_length - audio_stream.Position), buffer.Length)) > 0)
                                                                        {
                                                                            if (audio_stream.Read(buffer, 0, len) == len)
                                                                            {
                                                                                file_stream.Write(buffer, 0, len);
                                                                            }
                                                                            else
                                                                            {
                                                                                break;
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                catch (Exception e)
                                                {
                                                    Console.Error.WriteLine(e);
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.Error.WriteLine(e);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("Can't open audio directory \"" + path + "\".");
                                Environment.ExitCode = 3;
                            }
                        }
                    }
                    else
                    {
                        Environment.ExitCode = 2;
                    }
                }
                else
                {
                    Environment.ExitCode = 1;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                Environment.ExitCode = -1;
            }
        }
    }
}
