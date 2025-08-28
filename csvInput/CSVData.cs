using System;
using System.IO;

namespace csvHandling
{
    public class CSVData
    {
        public CSVData() { }

        public List<string>? getCSVData(string filepath)
        {
            if (filepath == null)
            {
                Console.WriteLine(" CSVData is not inizialized");
                return null;
            }
            List<string> data = new List<string>();

            using (StreamReader reader = new StreamReader(filepath))
            {

                string line;
                while ((line = reader.ReadLine()) != null) { 
                    string[] values = line.Split(','); 
                    data.Add(values[0]);
                }
            }
                return data;
        }

        public void setCSVData(string filepath, List<string?> data)
        {
            if (filepath == null)
            {
                Console.WriteLine("No valid Filepath given");
                return;
            }
            using (StreamWriter writer = new StreamWriter(filepath))
            {

                foreach (string? elem in data)
                {
                    if (elem != null) writer.WriteLine(elem);
                    else continue;
                }

                Console.WriteLine($"Finished Writing {data.Count} Elements to {filepath}");
                writer.Close();
                return;
            }
            
            
        }

    }
}
