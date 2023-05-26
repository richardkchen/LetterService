public interface ILetterService
{
  void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile);
}

public class LetterService: ILetterService
{
  public static async Task Main()
  {
    string inputDirectory = @"CombinedLetters/Input";
    string archiveDirectory = @"CombinedLetters/Archive";
    await ArchiveFiles(inputDirectory, archiveDirectory, "Admission");
    await ArchiveFiles(inputDirectory, archiveDirectory, "Scholarship");
  }

  // Archives Letters based on dated folder name
  // from CombinedLetters/Input/Admission/yyyyMMdd/admission-XXXXXXXX.txt
  // to CombinedLetters/Archive/Admission/yyyyMMdd/admission-XXXXXXXX.txt
  public static async Task ArchiveFiles(string inputDirectory, string archiveDirectory, string fixedFolderName)
  {
    Console.WriteLine($"Archiving {fixedFolderName} Letters to Archive directory.");
    string sourceDirectory = $@"{inputDirectory}/{fixedFolderName}";
    foreach (string datedFolder in Directory.EnumerateDirectories(sourceDirectory))
    {
      string datedFolderName = new DirectoryInfo(datedFolder).Name;
      string destinationDirectory = Path.Combine(archiveDirectory, fixedFolderName, datedFolderName);

      if (!Directory.Exists(destinationDirectory)) {
        Console.WriteLine($@"Creating {destinationDirectory}");
      }
      
      Directory.CreateDirectory(destinationDirectory);

      foreach (string letterFile in Directory.EnumerateFiles(datedFolder))
      {
        using (FileStream sourceStream = File.Open(letterFile, FileMode.Open))
        {
          string destinationPath = Path.Combine(destinationDirectory, Path.GetFileName(letterFile));

          using (FileStream destinationStream = File.Create(destinationPath))
          {
            Console.WriteLine($@"Copying {letterFile} to {destinationDirectory}.");
            await sourceStream.CopyToAsync(destinationStream);
          }
        }
      }
    }
  }
  public void CombineTwoLetters(string inputFile1, string inputFile2, string resultFile)
  {
  }
}