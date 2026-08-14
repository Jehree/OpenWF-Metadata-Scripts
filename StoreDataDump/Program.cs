using System.Text.Json;

//prints a metadata patch that caps recipes at 8 hours
//Main.PrintCraftsCappedAtSeconds(Main.RecipesPath, 28800);

//prints all recipe names so that they can all be modified on a single line
//can be used to set crafting times to all be 5 seconds, all have 0 plat skip cost, etc.
//Main.PrintAllRecipeNames();

//prints all warframes with a premium price, can use to set plat prices to 0 so they can't be bought with plat, etc.
//Main.GetAllItemNames(Main.WarframesPath, ["PremiumPrice"]);

public static class Main
{
    public static string WorkingDirectory = Environment.CurrentDirectory;
    public static string ProjectDirectory = Directory.GetParent(WorkingDirectory)!.Parent!.Parent!.FullName;

    public static string DataDumpPath = $"{ProjectDirectory}/../../warframe-packages-bin-data-senpai";
    public static string StoreItemsPath = $"{DataDumpPath}/Lotus/StoreItems";
    public static string WeaponsPath = $"{StoreItemsPath}/Weapons";
    public static string WarframesPath = $"{StoreItemsPath}/Powersuits";
    public static string SentinelsPath = $"{StoreItemsPath}/Types/Sentinels/SentinelPowersuits";
    public static string UIStuff = $"{DataDumpPath}/Lotus/Types/StoreItems/SuitCustomizations/";
    public static string RecipesPath = $"{DataDumpPath}/Lotus/StoreItems/Types/Recipes";

    public static List<string> GetAllItemNames(string dirPath, Func<string, bool>? jsonDataFilter = null, Func<string, bool>? itemNameFilter = null)
    {
        List<string> allNames = [];

        foreach (string filePath in Directory.GetFiles(dirPath))
        {
            if (!PassesFilterCheck(filePath, jsonDataFilter, itemNameFilter)) continue;

            string storeItemName = GetItemName(filePath);

            if (storeItemName == null) continue;

            allNames.Add(storeItemName);
        }

        foreach (string subdir in Directory.GetDirectories(dirPath))
        { 
            allNames.AddRange(GetAllItemNames(subdir, jsonDataFilter, itemNameFilter));
        }

        return allNames;
    }

    public static bool PassesFilterCheck(string filePath, Func<string, bool>? jsonDataFilter = null, Func<string, bool>? itemNameFilter = null)
    {
        if (itemNameFilter != null)
        {
            string itemName = GetItemName(filePath);
            if (!itemNameFilter(itemName)) return false;
        }

        if (jsonDataFilter != null)
        {
            string jsonData = File.ReadAllText(filePath);

            if (!jsonDataFilter(jsonData)) return false;
        }

        return true;
    }

    public static string GetItemName(string storeItemFilePath)
    {
        string storeItemName = storeItemFilePath.Replace(DataDumpPath, "").Replace(".json", "").Replace("\\", "/");

        return storeItemName;
    }

    public static int ConvertPlatToCred(string jsonData, int multiplier)
    {
        using JsonDocument doc = JsonDocument.Parse(jsonData);

        int premiumPrice = doc.RootElement
            .GetProperty("data")
            .GetProperty("PremiumPrice")
            .GetInt32();

        return premiumPrice * multiplier;
    }

    // this doesn't work very well, lots of things get missed
    public static void PrintPlatToCredConversions(string dirPath, int multiplier)
    {
        foreach (string subdir in Directory.GetDirectories(dirPath))
        {
            PrintPlatToCredConversions(subdir, multiplier);
        }

        foreach (string filePath in Directory.GetFiles(dirPath))
        {
            string jsonData = File.ReadAllText(filePath);

            if (!jsonData.Contains("\"PremiumPrice\"")) continue;

            string storeItemName = GetItemName(filePath);

            Console.WriteLine($">{storeItemName}");
            //Console.WriteLine("ShowInMarket=1");
            Console.WriteLine("PremiumPrice=0");
            Console.WriteLine($"RegularPrice={ConvertPlatToCred(jsonData, multiplier)}");
        }
    }

    public static void PrintAllRecipeNames()
    {
        List<string> recipeNames = GetAllItemNames($"{DataDumpPath}/Lotus", default, itemName => itemName.EndsWith("Blueprint"));

        Console.WriteLine($">{string.Join(" & ", recipeNames)}");
    }

    public static void PrintCraftsCappedAtSeconds(string dirPath, int cap)
    {
        foreach (string subdir in Directory.GetDirectories(dirPath))
        {
            PrintCraftsCappedAtSeconds(subdir, cap);
        }

        foreach (string filePath in Directory.GetFiles(dirPath))
        {
            string jsonData = File.ReadAllText(filePath);

            if (!jsonData.Contains("\"BuildTime\"")) continue;

            string storeItemName = GetItemName(filePath);

            int? newCraftTime = ProcessCraftTimeOrNull(jsonData, cap);

            if (newCraftTime != null)
            {
                Console.WriteLine($">{storeItemName}");
                Console.WriteLine($"BuildTime={newCraftTime}");
            }
        }
    }

    public static int? ProcessCraftTimeOrNull(string jsonData, int cap)
    {
        using JsonDocument doc = JsonDocument.Parse(jsonData);

        int buildTime = doc.RootElement
            .GetProperty("data")
            .GetProperty("BuildTime")
            .GetInt32();

        if (buildTime >  cap)
        {
            return cap;
        }
        else
        {
            return null;
        }
    }
}

