namespace SDSSoundEncrypt
{
    /// <summary>
    /// WAV file header structure https://ccrma.stanford.edu/courses/422/projects/WaveFormat/
    /// </summary>
    struct WAVHeader
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
}