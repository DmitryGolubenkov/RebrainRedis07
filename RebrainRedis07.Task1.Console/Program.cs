using StackExchange.Redis;

var redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
var db = redis.GetDatabase();
var server = redis.GetServer("localhost:6379");
var basePath = args[0];


// Чистим Redis
db.KeyDelete($"data:{basePath}");
foreach (var item in server.Keys(pattern: $"data:{basePath}/*"))
{
    _= db.KeyDeleteAsync(item, CommandFlags.FireAndForget);
}

// Поиск всех вложенных файлов
var files = GetAllAccessibleFiles(basePath);

foreach (var file in files)
{
    var content = File.ReadAllBytes(file);
    _ = db.StringSetAsync($"data:{file}", content, flags: CommandFlags.FireAndForget);
}


// Test
foreach (var item in server.Keys(pattern: $"data:{basePath}/*"))
{
    Console.WriteLine(await db.StringGetAsync(item));
}


static IEnumerable<string> GetAllAccessibleFiles(string rootPath)
{
    DirectoryInfo di = new DirectoryInfo(rootPath);
    var dirs = di.EnumerateDirectories();
    // Для каждой папки в данной папке
    foreach (DirectoryInfo dir in dirs)
    {
        // Если в папку можно зайти
        if (!((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
        {
            // Возвращаем все файлы из этой папки
            foreach (var file in dir.EnumerateFiles())
            {
                // Игнорируем ссылки
                if (file.LinkTarget is not null)
                { continue; }
                yield return file.FullName;
            }
            // Возвращаем все файлы из дочерних папок
            foreach (var file in GetAllAccessibleFiles(dir.FullName))
            {
                yield return file;
            }
        }
    }
}




/*
static IEnumerable<string> GetAllAccessibleFiles(string rootPath, List<string> alreadyFound = null)
{
    if (alreadyFound == null)
        alreadyFound = new List<string>();

    DirectoryInfo di = new DirectoryInfo(rootPath);
    var dirs = di.EnumerateDirectories();
    foreach (DirectoryInfo dir in dirs)
    {
        if (!((dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden))
        {
            alreadyFound = (List<string>)GetAllAccessibleFiles(dir.FullName, alreadyFound);
        }
    }

    var files = Directory.GetFiles(rootPath);
    foreach (string s in files)
    {
        alreadyFound.Add(s);
    }

    yield return alreadyFound;
}
*/