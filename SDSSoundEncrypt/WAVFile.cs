using System;
using System.IO;

namespace SDSSoundEncrypt
{
    /// <summary>
    /// Class for WAV file manipulations.
    /// </summary>
    static class WAVFile
    {
        /// <summary>
        /// Opens WAV file and reads header info.
        /// </summary>
        /// <param name="fileName">The name of file to open.</param>
        /// <returns>Structure with WAV header info.</returns>
        public static WAVHeader GetWAVHeader(string fileName)
        {
            WAVHeader WAVheader = new WAVHeader();

            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                WAVheader.ChunkID = br.ReadBytes(4);
                WAVheader.ChunkSize = br.ReadUInt32();
                WAVheader.Format = br.ReadBytes(4);
                WAVheader.Subchunk1ID = br.ReadBytes(4);
                WAVheader.Subchunk1Size = br.ReadUInt32();
                WAVheader.AudioFormat = br.ReadUInt16();
                WAVheader.NumChannels = br.ReadUInt16();
                WAVheader.SampleRate = br.ReadUInt32();
                WAVheader.ByteRate = br.ReadUInt32();
                WAVheader.BlockAlign = br.ReadUInt16();
                WAVheader.BitsPerSample = br.ReadUInt16();
                WAVheader.Subchunk2ID = br.ReadBytes(4);
                WAVheader.Subchunk2Size = br.ReadUInt32();
            }
            return WAVheader;
        }

        /// <summary>
        /// Creates WAV file and writes header info.
        /// </summary>
        /// <param name="fileName">The name of file to create.</param>
        /// <param name="WAVheader">WAV header which will be written.</param>
        public static void SetWAVHeader(string fileName, WAVHeader WAVheader)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(WAVheader.ChunkID);
                bw.Write(WAVheader.ChunkSize);
                bw.Write(WAVheader.Format);
                bw.Write(WAVheader.Subchunk1ID);
                bw.Write(WAVheader.Subchunk1Size);
                bw.Write(WAVheader.AudioFormat);
                bw.Write(WAVheader.NumChannels);
                bw.Write(WAVheader.SampleRate);
                bw.Write(WAVheader.ByteRate);
                bw.Write(WAVheader.BlockAlign);
                bw.Write(WAVheader.BitsPerSample);
                bw.Write(WAVheader.Subchunk2ID);
                bw.Write(WAVheader.Subchunk2Size);
            }
        }

        /// <summary>
        /// This method "fixes" WAV header for encrypted files. Original file contains short values, encrypted - double.
        /// We change WAV header to supply correct data as if it contains more samples.
        /// Because of this, encrypted file duration is 4 times longer.
        /// </summary>
        /// <param name="fileName">The name of file to write to.</param>
        /// <param name="WAVheader">WAV header which will be fixed and written.</param>
        public static void SetEncryptedWAVHeader(string fileName, WAVHeader WAVheader)
        {
            // Fix difference between short and double
            WAVheader.Subchunk2Size *= sizeof(double) / sizeof(short);
            // And we also have one extra sample at the beginning (x1prev)
            WAVheader.Subchunk2Size += sizeof(double);
            // ChunkSize has to be changed as well as its value should always be 36 (bytes) greater than Subchunk2Size
            WAVheader.ChunkSize = WAVheader.Subchunk2Size + 36;
            // Write header to encrypted file
            SetWAVHeader(fileName, WAVheader);
        }

        /// <summary>
        /// This method "fixes" WAV header for decrypted files. Decrypted file contains one sample less (see below).
        /// We change WAV header to supply correct data.
        /// </summary>
        /// <param name="fileName">The name of file to write to.</param>
        /// <param name="WAVheader">WAV header which will be fixed and written.</param>
        public static void SetDecryptedWAVHeader(string fileName, WAVHeader WAVheader)
        {
            // 2 last encrypted samples won't be decrypted because reverse calculation requires 3 consequent values
            // But we inserted 1 extra sample in the beginning (x1prev) while encrypting, so we'll lose only 1 sample instead of 2
            WAVheader.Subchunk2Size -= sizeof(short);
            // ChunkSize has to be changed as well as its value should always be 36 (bytes) greater than Subchunk2Size
            WAVheader.ChunkSize = WAVheader.Subchunk2Size + 36;
            SetWAVHeader(fileName, WAVheader);
        }

        /// <summary>
        /// Calculate WAV file duration (minutes and seconds).
        /// </summary>
        /// <param name="WAVheader">WAV file header, info is read from it.</param>
        /// <param name="durationMinutes">Output parameter for duration in minutes.</param>
        /// <param name="durationSeconds">Output parameter for duration in seconds.</param>
        public static void GetDuration(WAVHeader WAVheader, out int durationMinutes, out double durationSeconds)
        {
            // We get duration from header data, data size / bytes per sample / number of channels / sample rate
            durationSeconds = (double)WAVheader.Subchunk2Size / (WAVheader.BitsPerSample / 8) / WAVheader.NumChannels / WAVheader.SampleRate;
            durationMinutes = (int)Math.Floor(durationSeconds / 60);
            durationSeconds -= durationMinutes * 60;
        }

        /// <summary>
        /// Output some basic info about WAV file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="WAVheader">WAV file header.</param>
        public static void GetWAVFileInfo(string fileName, WAVHeader WAVheader)
        {
            Console.WriteLine("File name: {0}", fileName);
            Console.WriteLine("Number of channels: {0}", WAVheader.NumChannels);
            Console.WriteLine("Sample rate: {0}", WAVheader.SampleRate);
            Console.WriteLine("Bytes per second: {0}", WAVheader.ByteRate);
            Console.WriteLine("Bytes per sample: {0}", WAVheader.BlockAlign);
            Console.WriteLine("Bits per sample: {0}", WAVheader.BitsPerSample);
            Console.WriteLine("Size of data (bytes): {0}", WAVheader.Subchunk2Size);
            Console.WriteLine("Size of chunk (data size + 36 bytes): {0}", WAVheader.ChunkSize);

            int durationMinutes;
            double durationSeconds;
            GetDuration(WAVheader, out durationMinutes, out durationSeconds);

            Console.WriteLine("Sound duration: {0}:{1}", durationMinutes.ToString("00"), durationSeconds.ToString("00.00"));
            Console.WriteLine();
        }
    }
}