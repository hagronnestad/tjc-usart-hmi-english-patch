using System;
using System.IO;
using System.Security.Cryptography;

namespace TjcUsartHmiEnglishPatch {
    class Program {
        const string fileName = "hmitype.dll";
        const string fileNameBackUp = "hmitype.dll.original";
        const string originalFileSha256 = "B8-47-93-94-75-68-5E-88-80-31-DD-60-D5-9C-7B-44-43-05-5E-98-C5-5A-CC-EA-1C-EB-90-45-D1-FB-B6-4E";
        const string patchedFileSha256 = "03-CC-18-96-BF-E1-54-0A-B7-1B-F5-03-AE-FC-B9-FD-E2-54-4D-D4-80-25-F5-81-AD-F6-82-3F-F9-A5-F1-A0";

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
            data[0x00076766] = 0x17;
            data[0x00076806] = 0x17;
            data[0x0007689E] = 0x17;

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
