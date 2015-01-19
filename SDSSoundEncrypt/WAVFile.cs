using System;
using System.IO;

namespace SDSSoundEncrypt
{
    /// <summary>
    /// Class for WAV file manipulations.
    /// </summary>
    class WAVFile
    {
        /// <summary>
        /// WAV file header structure https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
        /// </summary>
        public struct WAVHeader
        {
            // Contains the letters "RIFF" in ASCII, 4 bytes
            public byte[] ChunkID;

            // 36 + SubChunk2Size, size of the rest of the chunk following this number, 4 bytes
            public uint ChunkSize;

            // Contains the letters "WAVE", 4 bytes
            public byte[] Format;

            // Contains the letters "fmt ", 4 bytes
            public byte[] Subchunk1ID;

            // 16 for PCM, size of the rest of the Subchunk, which follows this number, 4 bytes
            public uint Subchunk1Size;

            // PCM = 1, other values mean compression, 2 bytes
            public ushort AudioFormat;

            // Mono = 1, Stereo = 2, 2 bytes
            public ushort NumChannels;

            // 8000 Hz, 44100 Hz, etc, 4 bytes
            public uint SampleRate;

            // SampleRate * NumChannels * BitsPerSample/8 (bytes per second), 4 bytes
            public uint ByteRate;

            // NumChannels * BitsPerSample/8 (bytes per sample, ALL channels), 2 bytes
            public ushort BlockAlign;

            // 8 bit, 16 bit, etc (bits per sample, ONE channel), 2 bytes
            public ushort BitsPerSample;

            // Contains the letters "data", 4 bytes
            public byte[] Subchunk2ID;

            // NumSamples * NumChannels * BitsPerSample/8 (number of bytes in actual following data), 4 bytes
            public uint Subchunk2Size;
        }

        /// <summary>
        /// WAV file name.
        /// </summary>
        public string FileName;

        /// <summary>
        /// WAV file header.
        /// </summary>
        public WAVHeader Header;

        /// <summary>
        /// Constructor to get file name and memory for header.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public WAVFile(string fileName)
        {
            this.FileName = fileName;
            this.Header = new WAVHeader();
        }

        /// <summary>
        /// Opens existing WAV file and reads header info.
        /// </summary>
        public void ReadWAVHeader()
        {
            using (FileStream fs = new FileStream(this.FileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                this.Header.ChunkID = br.ReadBytes(4);
                this.Header.ChunkSize = br.ReadUInt32();
                this.Header.Format = br.ReadBytes(4);
                this.Header.Subchunk1ID = br.ReadBytes(4);
                this.Header.Subchunk1Size = br.ReadUInt32();
                this.Header.AudioFormat = br.ReadUInt16();
                this.Header.NumChannels = br.ReadUInt16();
                this.Header.SampleRate = br.ReadUInt32();
                this.Header.ByteRate = br.ReadUInt32();
                this.Header.BlockAlign = br.ReadUInt16();
                this.Header.BitsPerSample = br.ReadUInt16();
                this.Header.Subchunk2ID = br.ReadBytes(4);
                this.Header.Subchunk2Size = br.ReadUInt32();
            }
        }

        /// <summary>
        /// Creates a new WAV file and writes header info.
        /// </summary>
        public void WriteWAVHeader()
        {
            using (FileStream fs = new FileStream(this.FileName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(this.Header.ChunkID);
                bw.Write(this.Header.ChunkSize);
                bw.Write(this.Header.Format);
                bw.Write(this.Header.Subchunk1ID);
                bw.Write(this.Header.Subchunk1Size);
                bw.Write(this.Header.AudioFormat);
                bw.Write(this.Header.NumChannels);
                bw.Write(this.Header.SampleRate);
                bw.Write(this.Header.ByteRate);
                bw.Write(this.Header.BlockAlign);
                bw.Write(this.Header.BitsPerSample);
                bw.Write(this.Header.Subchunk2ID);
                bw.Write(this.Header.Subchunk2Size);
            }
        }

        /// <summary>
        /// "Fixes" WAV header for encrypted files. Original file contains short values, encrypted - double.
        /// We change WAV header to supply correct data as if it contains more samples.
        /// Because of this encrypted file duration seems to be 4 times longer and file is ~4 bigger.
        /// </summary>
        public void WriteEncryptedWAVHeader()
        {
            // Fix difference between short and double
            this.Header.Subchunk2Size *= sizeof(double) / sizeof(short);
            // And we also have one extra sample at the beginning (x1prev)
            this.Header.Subchunk2Size += sizeof(double);
            // ChunkSize has to be changed as well as its value should always be 36 (bytes) greater than Subchunk2Size
            this.Header.ChunkSize = this.Header.Subchunk2Size + 36;
            // Write header to encrypted file
            WriteWAVHeader();
        }

        /// <summary>
        /// "Fixes" WAV header for decrypted files. Decrypted file contains one sample less.
        /// We change WAV header to supply correct data.
        /// </summary>
        public void WriteDecryptedWAVHeader()
        {
            // 2 last encrypted samples won't be decrypted because reverse calculation requires 3 consequent values
            // But we inserted 1 extra sample in the beginning (x1prev) while encrypting, so we'll lose only 1 sample instead of 2
            this.Header.Subchunk2Size -= sizeof(short);
            // ChunkSize has to be changed as well as its value should always be 36 (bytes) greater than Subchunk2Size
            this.Header.ChunkSize = this.Header.Subchunk2Size + 36;
            WriteWAVHeader();
        }

        /// <summary>
        /// Calculates WAV file duration (in minutes and seconds).
        /// </summary>
        /// <param name="WAVheader">WAV file header to read info from.</param>
        /// <param name="durationMinutes">Output parameter for duration in minutes.</param>
        /// <param name="durationSeconds">Output parameter for duration in seconds.</param>
        public void GetDuration(WAVHeader WAVheader, out int durationMinutes, out double durationSeconds)
        {
            // We get duration from header data, data size / bytes per sample / number of channels / sample rate
            durationSeconds = (double)WAVheader.Subchunk2Size / (WAVheader.BitsPerSample / 8) / WAVheader.NumChannels / WAVheader.SampleRate;
            durationMinutes = (int)Math.Floor(durationSeconds / 60);
            durationSeconds -= durationMinutes * 60;
        }

        /// <summary>
        /// Outputs some basic info about WAV file.
        /// </summary>
        public void GetWAVFileInfo()
        {
            Console.WriteLine("File name: {0}", this.FileName);
            Console.WriteLine("Number of channels: {0}", this.Header.NumChannels);
            Console.WriteLine("Sample rate: {0}", this.Header.SampleRate);
            Console.WriteLine("Bytes per second: {0}", this.Header.ByteRate);
            Console.WriteLine("Bytes per sample: {0}", this.Header.BlockAlign);
            Console.WriteLine("Bits per sample: {0}", this.Header.BitsPerSample);
            Console.WriteLine("Size of data (bytes): {0}", this.Header.Subchunk2Size);
            Console.WriteLine("Size of chunk (data size + 36 bytes): {0}", this.Header.ChunkSize);

            int durationMinutes;
            double durationSeconds;
            GetDuration(this.Header, out durationMinutes, out durationSeconds);

            Console.WriteLine("Sound duration: {0}:{1}", durationMinutes.ToString("00"), durationSeconds.ToString("00.00"));
            Console.WriteLine();
        }
    }
}