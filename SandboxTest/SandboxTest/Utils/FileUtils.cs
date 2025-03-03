namespace SandboxTest.Utils
{
    public static class FileUtils
    {
        /// <summary>
        /// Writes text to a file without changing the file name in any way.
        /// </summary>
        /// <param name="fileName">The full path of the file to write to</param>
        /// <param name="text">The text to write</param>
        /// <returns></returns>
        public static async Task WriteTextToFileAsync(string fileName, string text)
        {
            using var fileStream = new FileStream(fileName, FileMode.Truncate, FileAccess.Write);
            using var textWriter = new StreamWriter(fileStream);
            await textWriter.WriteAsync(text);
            await textWriter.FlushAsync();
        }
    }
}
