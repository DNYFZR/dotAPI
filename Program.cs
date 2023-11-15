// JSON API Handler Module
// SEPA Rainfall Example

using Newtonsoft.Json;

class dotAPI {
    const string cmdLineSep = "=";
    const string apiBaseURL = "https://www2.sepa.org.uk/rainfall";

    static void Main(string[] args) {
        // Parse Input
        Dictionary <string, string> arguments = parseCliArgs(args);

        // Check Input
        if (!arguments.ContainsKey("endpoint") || !new [] {"hourly", "daily", "monthly"}.Contains(arguments["endpoint"])){
            throw new ArgumentException("Please specify an execution endpoint e.g. endpoint=hourly");
            }

        // Set Paths
        string filePathMeta = $"data/metadata.json";
        string filePathData = $"data/{arguments["endpoint"]}_data.json";

        // Get Metadata
        var metadata = jsonConvert(GetData(apiBaseURL, GetEndpoint("metadata")).Result);
        
        // Extract IDs       
        var data = new Dictionary<object, object>(); 
        foreach(Dictionary<string, object> dct in metadata) {data[dct["station_name"]] = dct["station_no"];}

        // Get Data
        foreach(KeyValuePair<object, object> item in data) {
            data[item.Key] = jsonConvert(GetData(apiBaseURL, GetEndpoint(arguments["endpoint"], item.Value.ToString()!)).Result);
        }

        // Write Results
        File.WriteAllText(filePathMeta, stringConvert(metadata));
        File.WriteAllText(filePathData, stringConvert(data));

        Console.WriteLine("Execution Complete...");
    }

    public static Dictionary<string, string> parseCliArgs(string[] args) {
        var arguments = new Dictionary<string, string>();

        foreach (string argument in args){
            string[] chunk = argument.Split(cmdLineSep);

            if (chunk.Length == 2) {arguments[chunk[0]] = chunk[1];}
        }
        return arguments;
    }

    public static List<Dictionary<string, object>> jsonConvert(string str){
        return JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(str)!;
    }

    public static string stringConvert<T>(T obj){
        return JsonConvert.SerializeObject(obj);
    }

    public static async Task<string> GetData(string apiBaseURL, string apiMetaEndpoint) {   
        string result = string.Empty;

        try {
            var httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync($"{apiBaseURL}{apiMetaEndpoint}");
            result = await response.Content.ReadAsStringAsync();
            
        } catch (HttpRequestException e) {
            Console.WriteLine($"Request exception: {e.Message}");
        }
        
        return result;  
    }

    public static string GetEndpoint(string endpoint, string id=""){
        string apiEndpoint = string.Empty;

        // Get metadata
        if (endpoint.ToLower() == "metadata") {
            apiEndpoint = "/api/Stations";
            
        // Get monthly data
        } else if (endpoint.ToLower() == "monthly") {
            apiEndpoint = $"/api/Month/{id}";
            
        // Get daily data
        } else if (endpoint.ToLower() == "daily") {
            apiEndpoint = $"/api/Daily/{id}";
            
        // Get hourly data
        } else if (endpoint.ToLower() == "hourly") {
            apiEndpoint = $"/api/Hourly/{id}";
            
        } else {
            throw new ArgumentException("Invalid Endpoint Selected");
        }

        return apiEndpoint;
    }
}


