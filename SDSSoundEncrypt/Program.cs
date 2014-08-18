using System;

namespace SDSSoundEncrypt
{  
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply correct WAV file name as the first argument for encryption.");
                Environment.Exit(1);
            }
            string fileNameOriginal = args[0];
            // Set encrypted file name to "%fileNameOriginal% (encrypted).wav"
            string fileNameEncrypted = fileNameOriginal.Insert(fileNameOriginal.IndexOf(".wav"), " (encrypted)");
            // Set decrypted file name to "%fileNameOriginal% (decrypted).wav"
            string fileNameDecrypted = fileNameOriginal.Insert(fileNameOriginal.IndexOf(".wav"), " (decrypted)");

            WAVHeader WAVHeaderOriginal = new WAVHeader();

            // Read WAV file header
            WAVHeaderOriginal = WAVFile.GetWAVHeader(fileNameOriginal);

            // Output basic WAV file info
            WAVFile.GetWAVFileInfo(fileNameOriginal, WAVHeaderOriginal);

            // Fix encrypted file header and write it
            WAVFile.SetEncryptedWAVHeader(fileNameEncrypted, WAVHeaderOriginal);

            // Encrypt WAV file with SDS and seed value 1
            SDS.EncryptWAVFile(fileNameOriginal, fileNameEncrypted, 1);

            // Fix decrypted file header and write it
            WAVFile.SetDecryptedWAVHeader(fileNameDecrypted, WAVHeaderOriginal);

            // Decrypt WAV file with SDS and seed value 1
            SDS.DecryptWAVFile(fileNameEncrypted, fileNameDecrypted, 1);
        }
    }
}
