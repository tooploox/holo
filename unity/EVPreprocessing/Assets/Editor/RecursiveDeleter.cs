using System;
using System.IO;
using System.Threading;

public class RecursiveDeleter 
{
    private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

    public static void DeleteRecursivelyWithSleep(string destinationPath)
    {
        const int tries = 10;

        for (var attempt = 0; attempt < tries; attempt++)
        {
            try
            {
                TryDeleting(destinationPath);
            }
            catch (DirectoryNotFoundException)
            {
                return;
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (UnauthorizedAccessException)
            { // Someone or something hasn't closed a file yet.
                Log.Debug($"Unauthorized Access at path: {destinationPath}, attempt #{attempt + 1}. Sleeping for 50ms and trying again.");
                Thread.Sleep(50);
                if (attempt == tries)
                {
                    break;
                }
                else
                {
                    continue;
                }
            }
            return;
        }
    }

    private static void TryDeleting(string destinationPath)
    {
        FileAttributes attr = File.GetAttributes(destinationPath);

        if (attr.HasFlag(FileAttributes.Directory))
        {
            Directory.Delete(destinationPath, true);
        }
        else
        {
            File.Delete(destinationPath);
        }
    }

    class RecursiveDeleteException : Exception
    {
        public RecursiveDeleteException() : base() { }

        public RecursiveDeleteException(string msg) : base(msg) { }
    }
}