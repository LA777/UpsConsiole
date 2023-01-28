using System.Security.Cryptography;

namespace UpsConsole.Services
{
    public static class Utilities
    {
        public static int GetMillisecondsFromMinutes(int minutes)
        {
            return (int)TimeSpan.FromMinutes(minutes).TotalMilliseconds;
        }

        public static int GetMillisecondsFromSeconds(int seconds)
        {
            return (int)TimeSpan.FromSeconds(seconds).TotalMilliseconds;
        }

        public static byte[] ComputeHash(string filePath)
        {
            var runCount = 1;

            while (runCount < 4)
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        using (var fs = File.OpenRead(filePath))
                        {
                            return SHA1
                                .Create().ComputeHash(fs);
                        }
                    }

                    throw new FileNotFoundException();
                }
                catch (IOException ex)
                {
                    if (runCount == 3 || ex.HResult != -2147024864)
                    {
                        throw;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, runCount)));
                    runCount++;
                }
            }

            return new byte[20];
        }
    }
}