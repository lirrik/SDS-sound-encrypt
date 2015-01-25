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
        /// WAV file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// WAV file header
        /// </summary>
        public WAVHeader Header { get; set; }

        /// <summary>
        /// Constructor to get file name and memory for header.
        /// </summary>
        /// <param name="fileName">File name</param>
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
            using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                WAVHeader header = new WAVHeader();

                header.ChunkID = br.ReadBytes(4);
                header.ChunkSize = br.ReadUInt32();
                header.Format = br.ReadBytes(4);
                header.Subchunk1ID = br.ReadBytes(4);
                header.Subchunk1Size = br.ReadUInt32();
                header.AudioFormat = br.ReadUInt16();
                header.NumChannels = br.ReadUInt16();
                header.SampleRate = br.ReadUInt32();
                header.ByteRate = br.ReadUInt32();
                header.BlockAlign = br.ReadUInt16();
                header.BitsPerSample = br.ReadUInt16();
                header.Subchunk2ID = br.ReadBytes(4);
                header.Subchunk2Size = br.ReadUInt32();

                this.Header = header;
            }
        }

        /// <summary>
        /// Creates a new WAV file and writes header info.
        /// </summary>
        public void CreateWAVFile()
        {
            using (FileStream fs = new FileStream(FileName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(Header.ChunkID);
                bw.Write(Header.ChunkSize);
                bw.Write(Header.Format);
                bw.Write(Header.Subchunk1ID);
                bw.Write(Header.Subchunk1Size);
                bw.Write(Header.AudioFormat);
                bw.Write(Header.NumChannels);
                bw.Write(Header.SampleRate);
                bw.Write(Header.ByteRate);
                bw.Write(Header.BlockAlign);
                bw.Write(Header.BitsPerSample);
                bw.Write(Header.Subchunk2ID);
                bw.Write(Header.Subchunk2Size);
            }
        }

        /// <summary>
        /// "Fixes" WAV header for encrypted files. Original file contains short values, encrypted - double.
        /// We change WAV header to supply correct data as if it contains more samples.
        /// Because of this encrypted file duration seems to be 4 times longer and file is ~4 bigger.
        /// </summary>
        public void CreateEncryptedWAVFile()
        {
            WAVHeader header = this.Header;

            // Fix difference between short and double
            header.Subchunk2Size *= sizeof(double) / sizeof(short);
            // And we also have one extra sample at the beginning (x1prev)
            header.Subchunk2Size += sizeof(double);
            // ChunkSize has to be changed as well as its value should always be 36 (bytes) greater than Subchunk2Size
            header.ChunkSize = Header.Subchunk2Size + 36;

            this.Header = header;

            // Create a WAV file and write modified header to it
            CreateWAVFile();
        }

        /// <summary>
        /// "Fixes" WAV header for decrypted files. Decrypted file contains one sample less.
        /// We change WAV header to supply correct data.
        /// </summary>
        public void CreateDecryptedWAVFile()
        {
            WAVHeader header = this.Header;

            // 2 last encrypted samples won't be decrypted because reverse calculation requires 3 consequent values
            // But we inserted 1 extra sample in the beginning (x1prev) while encrypting, so we'll lose only 1 sample instead of 2
            header.Subchunk2Size -= sizeof(short);
            // ChunkSize has to be changed as well as its value should always be 36 (bytes) greater than Subchunk2Size
            header.ChunkSize = Header.Subchunk2Size + 36;

            this.Header = header;

            // Create a WAV file and write modified header to it
            CreateWAVFile();
        }

        /// <summary>
        /// Calculates WAV file duration (in minutes and seconds).
        /// </summary>
        /// <param name="WAVheader">WAV file header to read info from.</param>
        /// <param name="durationMinutes">Output parameter for duration in minutes.</param>
        /// <param name="durationSeconds">Output parameter for duration in seconds.</param>
        public void GetWAVFileDuration(WAVHeader WAVheader, out int durationMinutes, out double durationSeconds)
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
            Console.WriteLine("File name: {0}", FileName);
            Console.WriteLine("Number of channels: {0}", Header.NumChannels);
            Console.WriteLine("Sample rate: {0}", Header.SampleRate);
            Console.WriteLine("Bytes per second: {0}", Header.ByteRate);
            Console.WriteLine("Bytes per sample: {0}", Header.BlockAlign);
            Console.WriteLine("Bits per sample: {0}", Header.BitsPerSample);
            Console.WriteLine("Size of data (bytes): {0}", Header.Subchunk2Size);
            Console.WriteLine("Size of chunk (data size + 36 bytes): {0}", Header.ChunkSize);

            int durationMinutes;
            double durationSeconds;
            GetWAVFileDuration(Header, out durationMinutes, out durationSeconds);

            Console.WriteLine("Sound duration: {0}:{1}", durationMinutes.ToString("00"), durationSeconds.ToString("00.00"));
            Console.WriteLine();
        }
    }
}