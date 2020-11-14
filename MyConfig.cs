using System;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

public static class MyConfig
{
    public static string var1 = "foo";
    public static string var2 = "bar";

    public class InnerConfig {
        public string Database {get; set;}
        public string Something {get; set;}
    }

    public static InnerConfig inner_config;

    static MyConfig() {
        Console.WriteLine("MyConfig ctor");
        string json_string = File.ReadAllText("my_settings.json");
        Console.WriteLine(json_string);
        inner_config = JsonSerializer.Deserialize<InnerConfig>(json_string);
        Console.WriteLine(inner_config.Database); 
    }
}