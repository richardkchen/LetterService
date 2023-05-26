using System.Linq;

public class LetterService
{
  string admissionInputDirectory = @"CombinedLetters/Input/Admission";
  string scholarshipInputDirectory = @"CombinedLetters/Input/Scholarship";
  string admissionArchiveDirectory = @"CombinedLetters/Archive/Admission";
  string scholarshipArchiveDirectory = @"CombinedLetters/Archive/Scholarship";
  string outputDirectory = @"CombinedLetters/Output";

  public static async Task Main()
  {
    LetterService letterService = new LetterService();

    await ArchiveFiles(letterService.admissionInputDirectory, letterService.admissionArchiveDirectory);
    await ArchiveFiles(letterService.scholarshipInputDirectory, letterService.scholarshipArchiveDirectory);

    await CombineLetters(letterService);

    await GenerateReport(letterService);
  }

  // Archives letters based on dated folder name
  // from CombinedLetters/Input/Admission/yyyyMMdd/admission-XXXXXXXX.txt
  // to CombinedLetters/Archive/Admission/yyyyMMdd/admission-XXXXXXXX.txt
  // Will overwrite if archived letter exists
  static async Task ArchiveFiles(string inputDirectory, string archiveDirectory)
  {
    Console.WriteLine($"Archiving {inputDirectory} to {archiveDirectory}.");
    foreach (string datedFolder in Directory.EnumerateDirectories(inputDirectory))
    {
      string datedFolderName = new DirectoryInfo(datedFolder).Name;
      string destinationDirectory = Path.Combine(archiveDirectory, datedFolderName);

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

  // Iterates over letters of matching StudentIDs to combine text files
  static async Task CombineLetters(LetterService letterService)
  {
    foreach(Letter letter in QueryAdmissionWithScholarship(letterService))
    {
      string resultFile = $@"{letterService.outputDirectory}/{letter.StudentID}.txt";
      Console.WriteLine($"Combining Letters and Writing to {resultFile}.");
      await CombineTwoLetters(letter.AdmissionLetter, letter.ScholarshipLetter, resultFile);
    }
  }

  // Query for letter filenames in Input Admission and Scholarhip with matching StudentIDs
  // Returns Iterable of type Letter
  static IEnumerable<Letter> QueryAdmissionWithScholarship(LetterService letterService)
  {
    IEnumerable<string> admissionLetters = new string[] { };
    IEnumerable<string> scholarshipLetters = new string[] { };

    // Loops over datedFolder directories in case LetterService was not run the previous day
    foreach (string datedFolder in Directory.EnumerateDirectories(letterService.admissionInputDirectory))
    {
      admissionLetters = Directory.EnumerateFiles(datedFolder);
    }

    foreach (string datedFolder in Directory.EnumerateDirectories(letterService.scholarshipInputDirectory))
    {
      scholarshipLetters = Directory.EnumerateFiles(datedFolder);
    }

    var admittedScholarshipLetters =
      from admissionLetter in admissionLetters
      from scholarshipLetter in scholarshipLetters
      let studentID = GetStudentIDFromFile(admissionLetter)
      where GetStudentIDFromFile(admissionLetter) == GetStudentIDFromFile(scholarshipLetter)
      select new Letter(studentID, admissionLetter, scholarshipLetter);

    foreach (Letter letter in admittedScholarshipLetters)
    {
      yield return letter;
    }
  }

  static string GetStudentIDFromFile(string filePath)
  {
    return Path.GetFileNameWithoutExtension(filePath).Split('-')[1];
  }

  // Combines Admission and Scholarship letters asynchronously, in case large number
  // of scholarships awarded. Writes resulting stream to text file in Output path.
  static async Task CombineTwoLetters(string inputFile1, string inputFile2, string resultFile)
  {
    using (FileStream outputStream = File.Create(resultFile))
    {
      using (FileStream admissionLetterStream = File.OpenRead(inputFile1))
      {
        await admissionLetterStream.CopyToAsync(outputStream);
      }
      using (FileStream scholarshipLetterStream = File.OpenRead(inputFile2))
      {
        await scholarshipLetterStream.CopyToAsync(outputStream);
      }
    }
  }

  // Generates Report by iterating over QueryAdmissionWithScholarship() for StudentID
  static async Task GenerateReport(LetterService letterService)
  {
    DateTime now = DateTime.Now;
    string today = now.ToString("MMddyyyy");
    string todayFormatted = now.ToString("MM/dd/yyyy");
    string divider = new string('-', 32);
    int combinedLettersCount = QueryAdmissionWithScholarship(letterService).Count();

    using (StreamWriter reportWriter = File.CreateText($@"{letterService.outputDirectory}/Report-{today}.txt"))
    {
      await reportWriter.WriteLineAsync($"{todayFormatted} Report");
      await reportWriter.WriteLineAsync(divider);
      await reportWriter.WriteLineAsync($"\nNumber of Combined Letters: {combinedLettersCount}");
      foreach (Letter letter in QueryAdmissionWithScholarship(letterService))
      {
        await reportWriter.WriteLineAsync($"\t{letter.StudentID}");
      }
    }

  }

  struct Letter
  {
    public string StudentID { get; set; }
    public string AdmissionLetter { get; set; }
    public string ScholarshipLetter { get; set; }
    public Letter(string studentID, string admissionLetter, string scholarshipLetter)
    {
      StudentID = studentID;
      AdmissionLetter = admissionLetter;
      ScholarshipLetter = scholarshipLetter;
    }
  }

}