using System;

namespace SDSSoundEncrypt
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 1)
            {
                string fileNameOriginal = args[0];

                if (fileNameOriginal.Contains(".wav"))
                {
                    // Set encrypted file name to "%fileNameOriginal% (encrypted).wav"
                    string fileNameEncrypted = fileNameOriginal.Insert(fileNameOriginal.IndexOf(".wav"), " (encrypted)");
                    // Set decrypted file name to "%fileNameOriginal% (decrypted).wav"
                    string fileNameDecrypted = fileNameOriginal.Insert(fileNameOriginal.IndexOf(".wav"), " (decrypted)");

                    //
                    // Input
                    //

                    WAVFile WAVFileOriginal = new WAVFile(fileNameOriginal);

                    // Read WAV file header
                    WAVFileOriginal.ReadWAVHeader();

                    // Output basic WAV file info
                    WAVFileOriginal.GetWAVFileInfo();

                    //
                    // Encryption
                    //

                    WAVFile WAVFileEncrypted = new WAVFile(fileNameEncrypted);

                    // Copy original WAV file header
                    WAVFileEncrypted.Header = WAVFileOriginal.Header;

                    // Fix WAV header for encrypted file and write it
                    WAVFileEncrypted.CreateEncryptedWAVFile();

                    // Encrypt WAV file with SDS and seed value 1
                    SDS.EncryptWAVFile(WAVFileOriginal, WAVFileEncrypted, 1);

                    //
                    // Decryption
                    //

                    WAVFile WAVFileDecrypted = new WAVFile(fileNameDecrypted);

                    // Copy original WAV file header
                    WAVFileDecrypted.Header = WAVFileOriginal.Header;

                    // Fix WAV header for decrypted file and write it
                    WAVFileDecrypted.CreateDecryptedWAVFile();

                    // Decrypt WAV file with SDS and seed value 1
                    SDS.DecryptWAVFile(WAVFileEncrypted, WAVFileDecrypted, 1);
                }
                else
                {
                    Console.WriteLine("Please supply a correct WAV file for encryption.");
                }
            }
            else
            {
                Console.WriteLine("Please supply only a WAV file for encryption.");
            }
        }
    }
}
