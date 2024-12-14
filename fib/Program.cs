// CLI project, packaging code files into one file.

using System.CommandLine;

var rootCommand = new RootCommand("root command");
var bundleCommand = new Command("bundle", "bundle several code files to single file");
rootCommand.AddCommand(bundleCommand);


var languageOption = new Option<List<string>>(aliases: new[] { "--language", "-lang" }, "Selected code languages")
{
    IsRequired = true,  //חובה
    Arity = ArgumentArity.OneOrMore,
    AllowMultipleArgumentsPerToken = true
};
languageOption.AddValidator(result =>
{
    var listOfLanguages = new[] { 
        "all", 
        "java", 
        "python", 
        "c#", 
        "c++" , 
        "sql" , 
        "css", 
        "typeScript", 
        "html", 
        "javaScript"
    };
    var invalidValues = result.Tokens.Select(t => t.Value)
        .Where(v => !listOfLanguages.Contains(v))
        .ToList();

    if (invalidValues.Any())
    {
        result.ErrorMessage = $"The languages ​​are invalid: " +
                              $"{string.Join(", ", invalidValues)}. Allowed values are: {string.Join(", ", listOfLanguages)}";
    }
});


var outputOption = new Option<FileInfo>(new[] { "--output", "-o" }, "File Location: routing or name"); //שם קובץ ה-bundle המיוצא 
var noteOption = new Option<bool>(new[] { "--note", "-n" }, "Should I list the name and relative path of the original code file?"); // רישום מקור הקוד כהערה בקובץ הbundle

noteOption.AddValidator( result =>
{
    var val = result.Tokens.First().Value ?? " ";
    if (val != null && !(val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)))
        result.ErrorMessage = $"Invalid value. Allowed values are: true or false ";
});

var sortOption = new Option<string>(new[] { "--sort", "-s" }, getDefaultValue: () => "order alphabet", description: "sort files");
sortOption.AddValidator(result =>
{
    var val = result.Tokens.FirstOrDefault()?.Value;
    if (val != null && !(val.Equals("order alphabet") || val.Equals("type")))
        result.ErrorMessage = $"Invalid value. Allowed values are: type or alphabet ";
});

var deleteOption = new Option<bool>(new[] { "--remove-empty-lines", "-rel" }); // מחיקת שורות ריקות

deleteOption.AddValidator(result =>
{
    var val = result.Tokens.First().Value ?? " ";
    if (val != null && !(val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase)))
        result.ErrorMessage = $"Invalid value. Allowed values are: true or false ";
});

var authorOption = new Option<string>(new[] { "--author", "-a" }) { IsRequired = false }; // רישום שם יוצר הקובץ

List<String> languages = new List<string>() {
    "java", 
    "python", 
    "c#", 
    "c++", 
    "sql", 
    "css", 
    "typeScript", 
    "html", 
    "javaScript"
};
var extentionTolang = new Dictionary<string, string>()
{
    {".java", "java" },
    { ".py", "python" },
    {".cs", "c#" },
    {".cpp", "c++" },
    {".sql", "sql" },
    {".css", "css" },
    { ".ts","typeScript" },
    {".html", "html" },
    {".js",  "javaScript"}
};



bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(deleteOption);
bundleCommand.AddOption(authorOption);
bundleCommand.SetHandler((output, lang, isNote, sort, delete, author) =>
{
    List<string> FilesList = new List<string>();
    string f = output.FullName;
    string[] sortedFiles;
    try
    {
        File.Create(f).Dispose(); ;
    }
    catch (Exception e)
    {
        Console.WriteLine("The path is invalid!");
    }

    File.WriteAllText(f, $"{author} \n");
    string[] files = Directory.GetFiles(Directory.GetCurrentDirectory());

    if ((sort.Equals("type")))
        sortedFiles = files.OrderBy(f => Path.GetExtension(f)).ThenBy(f => Path.GetFileName(f)).ToArray();
    else 
        sortedFiles = files.OrderBy(f => Path.GetFileName(f)).ToArray();

    foreach (string file in sortedFiles)
    {
        var l = extentionTolang.GetValueOrDefault(Path.GetExtension(file));
        if (lang[0] == "all" && extentionTolang.ContainsValue(l ?? "") || l != null && lang.Contains(l))
        {
            if (isNote)
                File.AppendAllText(f, Path.GetFileName(file) + "\n");

            if (delete)
            {
                var lines = File.ReadAllLines(file);
                var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l));
                File.AppendAllLines(f, nonEmptyLines);
            }
            else
                File.AppendAllText(f, File.ReadAllText(file));
        }
        File.AppendAllText(f, "\n\n");
    }

    Console.WriteLine("bundle");
}, outputOption, languageOption, noteOption, sortOption, deleteOption, authorOption);


var rspCommand = new Command("create-rsp"); // אריזת קבצי הפרוייקט
rootCommand.AddCommand(rspCommand);

// מפעילה את הפקודה בעת הקלדת bundle
rspCommand.SetHandler(() =>
{
    String path = "File.rsp";
    File.Create(path).Dispose();
    String author, fileName, codeLanguages, note, rel, sort;

    List<String> lst = new List<string>();

    Console.WriteLine("Enter the name of new file: ");
    fileName = Console.ReadLine() ?? " ResponseFile";

    Console.WriteLine("Enter the progarm languages: ");
    codeLanguages = Console.ReadLine();

    if (codeLanguages != "all")
    {
        lst = codeLanguages.Split(" ").ToList();
        codeLanguages = isValidList(languages, lst);
    }

    Console.WriteLine("Do you want note the code source?");
    note = Console.ReadLine();

    if (!string.IsNullOrEmpty(note))
        note = isValid(new List<string> { "true", "false" }, note);
    else note = "true";

    Console.WriteLine("Enter sort type, sort by:\r\ntype\r\nalphabet ");
    sort = Console.ReadLine();

    if (!string.IsNullOrEmpty(sort))
        sort = isValid(new List<string> { "order alphabet", "type" }, sort);
    else sort = "order alphabet";

    Console.WriteLine("Remove empty lines?");
    rel = Console.ReadLine();

    if (!string.IsNullOrEmpty(rel))
        rel = isValid(new List<string> { "true", "false" }, rel);
    else rel = "true";

    Console.WriteLine("Enter the name of author: ");
    author = Console.ReadLine() ?? " ";


    File.AppendAllText(path,
        $"package bundle --output {fileName} \n" +
        $"--language {codeLanguages} \n" +
        $"--note {note}\n " +
        $"--sort {sort}\n " +
        $"--remove-empty-lines {rel} \n");

    if (!string.IsNullOrEmpty(author))
        File.AppendAllText(path, $"--author {author}");

});

static String isValid(List<String> validVal, String inputVal)
{
    bool valid = validVal.Contains(inputVal);
    while (!valid)
    {
        Console.WriteLine($"invalid input\n the valid inputs are: {String.Join(" ", validVal)} \n try again");
        inputVal = Console.ReadLine();
        valid = string.IsNullOrEmpty(inputVal) || validVal.Contains(inputVal);
    }
    return inputVal ?? "";
}

static String isValidList(List<String> validVal, List<String> inputVal)
{

    string st = String.Join(" ", inputVal);
    Console.WriteLine(st);
    bool valid = inputVal.All(item => validVal.Contains(item));
    while (!valid || inputVal == null)
    {
        Console.WriteLine($"invalid input\n the valid inputs are: {String.Join(" ", validVal)} \n try again");
        st = Console.ReadLine() ?? " ";
        if (st != " ")
        {
            inputVal = st.Split(" ").ToList();
            valid = inputVal.All(item => validVal.Contains(item));
        }
    }

    return st;
}

rootCommand.InvokeAsync(args);
