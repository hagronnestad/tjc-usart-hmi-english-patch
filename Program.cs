using System;
using System.IO;
using System.Security.Cryptography;

namespace TjcUsartHmiEnglishPatch {
    class Program {
        const string fileName = "hmitype.dll";
        const string fileNameBackUp = "hmitype.dll.original";
        const string originalFileSha256 = "DC-C8-D4-44-0D-EC-1D-08-F8-B1-EA-4E-1F-0C-0C-87-CE-A5-03-E3-C9-53-6B-58-F9-59-52-95-9C-67-4F-0A";
        const string patchedFileSha256 = "FD-2A-3B-98-F8-7D-86-69-76-2D-69-B7-44-B4-B0-B5-E0-54-CA-FC-63-9F-C1-48-75-A8-F2-09-A2-37-CA-4D";

        static void Main(string[] args) {
            try {

                Console.WriteLine($"TJC USART HMI English Patch");
                Console.WriteLine($"*** Make sure to run me as administrator! ***");
                Console.WriteLine($"");
                Console.WriteLine($"This program will try to locate and patch '{fileName}'.");
                Console.WriteLine($"A backup of the original file will be created as '{fileNameBackUp}'.");
                Console.WriteLine($"\nPress any key to continue...");
                Console.ReadKey();

                // Try and find the file automatically
                Console.WriteLine($"\nTrying to locate '{fileName}'...");
                var fp = GetFilePath();

                // Let the user specify the file path
                if (fp == null) {
                    Console.WriteLine($"\nCould not find '{fileName}', please run this program from the same folder as '{fileName}' or paste the complete path now.");
                    Console.Write($"\nPath to {fileName}: ");
                    fp = Console.ReadLine().Trim();

                    if (!File.Exists(fp)) {
                        Console.WriteLine("\nThe file you entered doesn't exist. Exiting...");
                        Console.WriteLine("\nPress any key to exit...");
                        Console.ReadKey();
                        return;
                    }
                }

                var fInfo = new FileInfo(fp);

                var fileData = File.ReadAllBytes(fInfo.FullName);
                var hash = ComputeHash(fileData);

                // Check if file is already patched
                if (hash == patchedFileSha256) {
                    Console.WriteLine($"\n'{fileName}' has already been patched! :)");
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }

                // Check the original file hash
                if (hash == originalFileSha256) {
                    Console.WriteLine($"\n'{fileName}' has been located and the hash has been confirmed.");

                    var backUpFile = Path.Combine(fInfo.DirectoryName, fileNameBackUp);
                    if (!File.Exists(backUpFile)) {
                        Console.WriteLine("\nCreating a backup...");
                        File.Copy(fInfo.FullName, backUpFile);
                        Console.WriteLine($"\nThe backup file '{fileNameBackUp}' has been created.");

                        Console.WriteLine($"\nApplying patch to '{fileName}'...");
                        var fileDataPatched = ApplyPatch(fileData);
                        File.WriteAllBytes(fInfo.FullName, fileDataPatched);
                        Console.WriteLine($"\nPatch was applied to '{fileName}'...");

                        Console.WriteLine($"\nVerifying patch");

                        var patchedFileHash = ComputeHash(File.ReadAllBytes(fInfo.FullName));
                        if (patchedFileHash == patchedFileSha256) {
                            Console.WriteLine($"\nPatch has been verified! :)");
                            Console.WriteLine("\nPress any key to exit...");
                            Console.ReadKey();

                        } else {
                            Console.WriteLine($"\nPatch could not be verified. Please restore '{fileName}' from '{fileNameBackUp}' and try again.");
                            Console.WriteLine("\nPress any key to exit...");
                            Console.ReadKey();
                        }

                    } else {
                        Console.WriteLine($"\nThe backup file '{fileNameBackUp}' already exists.");
                    }


                } else {
                    Console.WriteLine($"\n'{fileName}' hash is not correct, can't patch the file.");
                    Console.WriteLine("\nPress any key to exit...");
                    Console.ReadKey();
                    return;
                }

            } catch (Exception e) {
                Console.WriteLine("\n");
                Console.WriteLine("An exception was caught:");
                Console.WriteLine(e.Message);
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static byte[] ApplyPatch(byte[] data) {
            data[0x0007675E] = 0x17;
            data[0x000767FE] = 0x17;
            data[0x00076896] = 0x17;

            return data;
        }

        static string ComputeHash(byte[] data) {
            using (var cryptoProvider = new SHA256CryptoServiceProvider()) {
                return BitConverter.ToString(cryptoProvider.ComputeHash(data));
            }
        }

        static string GetFilePath() {
            if (File.Exists(fileName)) {
                return fileName;
            }

            var pf = Path.Combine(ProgramFilesx86(), "USART HMI", fileName);
            if (File.Exists(pf)) {
                return pf;
            }

            return null;
        }

        static string ProgramFilesx86() {
            if (8 == IntPtr.Size
                || (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432")))) {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }
    }
}
