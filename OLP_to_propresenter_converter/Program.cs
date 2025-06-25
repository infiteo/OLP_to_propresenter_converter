using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace OLP_to_propresenter_converter
{

    /*
    Example OpenLP XML song file:


    <?xml version='1.0' encoding='UTF-8'?>
    <song xmlns="http://openlyrics.info/namespace/2009/song" version="0.8" createdIn="OpenLP 3.0.2" modifiedIn="OpenLP 3.0.2" modifiedDate="2023-11-28T20:24:15">
      <properties>
        <titles>
          <title>Gesù al centro</title>
        </titles>
        <verseOrder>v1 v2 v3 v4 v5 v6 v7 v8 v9</verseOrder>
        <authors>
          <author>Raccolta Interna</author>
        </authors>
      </properties>
      <lyrics>
        <verse name="v1">
          <lines>Gesù al centro di ogni cosa sei<br/>Gesù al centro di ogni cosa sei <br/>Dal principio Tu eri li<br/>e fino alla fine Tu sarai <br/>Gesù Gesù</lines>
        </verse>
        <verse name="v2">
          <lines>Tutto ciò che conta <br/>è restare accanto a Te<br/>Gesù sei al di sopra<br/>Niente è più importante per me,<br/>Oh Gesù.</lines>
        </verse>
        <verse name="v3">
          <lines>Gesù la mia vita è in mano a Te<br/>Gesù la mia vita è in mano a Te<br/>Dal principio Tu eri li<br/>e fino alla fine Tu sarai Gesù Gesù</lines>
        </verse>
        <verse name="v4">
          <lines>Tutto ciò che conta<br/>È restare accanto a Te<br/>Gesù sei al di sopra<br/>Niente è più importante per me , Oh Gesù</lines>
        </verse>
        <verse name="v5">
          <lines>Nel mio cuor e nel cielo<br/>Regni Oh signor perchè tutto è in te<br/>Ogni cosa è in Te</lines>
        </verse>
        <verse name="v6">
          <lines>Nel mio cuor e nel cielo<br/>Regni oh Signor<br/>perché tutto è in Te<br/>Ogni cosa è in Te</lines>
        </verse>
        <verse name="v7">
          <lines>Nel mio cuor e nel cielo<br/>Regni Oh Signor perché tutto è in Te<br/>Ogni cosa è in Te</lines>
        </verse>
        <verse name="v8">
          <lines>Gesù la Tua Chiesa vive in Te<br/>Gesù la Tua Chiesa vive in Te<br/>In ginocchio ai piedi Tuoi<br/>Innalziam col cuore il Tuo nome<br/><chord name="Gesù Gesù "/> x 3</lines>
        </verse>
        <verse name="v9">
          <lines>Nel mio cuor e nel cielo<br/>Regni Oh Signor perché tutto è in Te<br/>Ogni cosa è in Te<br/>Ge sù..</lines>
        </verse>
      </lyrics>
    </song>


    */


    [XmlRoot("song", Namespace = "http://openlyrics.info/namespace/2009/song")]
    public class Input
    {
        [XmlAttribute("version")] public string Version { get; set; }
        [XmlAttribute("createdIn")] public string CreatedIn { get; set; }
        [XmlAttribute("modifiedIn")] public string ModifiedIn { get; set; }
        [XmlAttribute("modifiedDate")] public DateTime ModifiedDate { get; set; }

        [XmlElement("properties", Namespace = "http://openlyrics.info/namespace/2009/song")]
        public PropertiesWrapper Properties { get; set; }

        [XmlElement("lyrics", Namespace = "http://openlyrics.info/namespace/2009/song")]
        public LyricsWrapper Lyrics { get; set; }

        [XmlIgnore] public List<string> Titles { get; set; }
        [XmlIgnore] public List<string> Authors { get; set; }
        [XmlIgnore] public string VerseOrder { get; set; }
        [XmlIgnore] public List<Verse> Verses { get; set; }

        /// <summary>
        /// Flattens nested properties and lyrics into direct lists for easier access.
        /// </summary>
        public void Flatten()
        {
            Titles = Properties?.Titles ?? new();
            Authors = Properties?.Authors ?? new();
            VerseOrder = Properties?.VerseOrder;
            Verses = Lyrics?.Verses ?? new();
        }

        /// <summary>
        /// Wrapper for song properties (titles, authors, verse order).
        /// </summary>
        public class PropertiesWrapper
        {
            [XmlArray("titles", Namespace = "http://openlyrics.info/namespace/2009/song")]
            [XmlArrayItem("title", Namespace = "http://openlyrics.info/namespace/2009/song")]
            public List<string> Titles { get; set; }

            [XmlArray("authors", Namespace = "http://openlyrics.info/namespace/2009/song")]
            [XmlArrayItem("author", Namespace = "http://openlyrics.info/namespace/2009/song")]
            public List<string> Authors { get; set; }

            [XmlElement("verseOrder", Namespace = "http://openlyrics.info/namespace/2009/song")]
            public string VerseOrder { get; set; }
        }

        /// <summary>
        /// Wrapper for song lyrics (list of verses).
        /// </summary>
        public class LyricsWrapper
        {
            [XmlElement("verse", Namespace = "http://openlyrics.info/namespace/2009/song")]
            public List<Verse> Verses { get; set; }
        }

        /// <summary>
        /// Represents a single verse in the song.
        /// </summary>
        public class Verse
        {
            [XmlAttribute("name")] public string Name { get; set; }

            [XmlElement("lines", Namespace = "http://openlyrics.info/namespace/2009/song")]
            public string Lines { get; set; }
        }
    }


    public class Program
    {
        /// <summary>
        /// Entry point: converts OpenLP XML song files to formatted text files for ProPresenter.
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: OLP_to_propresenter_converter <input_folder>");
                return;
            }

            string inputFolder = args[0];

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine($"Folder not found: {inputFolder}");
                return;
            }
            string outputFolder = args[1];

            if (!Directory.Exists(outputFolder))
            {
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine($"Folder '{outputFolder}' created.");
            }

            XmlSerializer serializer = new XmlSerializer(typeof(Input));
            int fileCount = Directory.GetFiles(inputFolder, "*.xml").Length;
            int processedCount = 0;
            foreach (string inputFile in Directory.GetFiles(inputFolder, "*.xml"))
            {
                try
                {
                    string rawXml = File.ReadAllText(inputFile);

                    // 1. Replace <br>, <br/>, <br /> with newline
                    rawXml = Regex.Replace(rawXml, @"<\s*br\s*/?\s*>", "\n", RegexOptions.IgnoreCase);

                    // 2. Remove all types of <chord> tags

                    // <chord name="..."/>
                    rawXml = Regex.Replace(
                        rawXml,
                        @"<\s*chord\s+name\s*=\s*['""]([^'""]+)['""]\s*/?\s*>",
                        m => m.Groups[1].Value.Trim(),
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    // <chord name="...">...</chord>
                    rawXml = Regex.Replace(
                        rawXml,
                        @"<\s*chord\s+name\s*=\s*['""]([^'""]+)['""]\s*>(.*?)<\s*/\s*chord\s*>",
                        m => m.Groups[1].Value.Trim() + " " + m.Groups[2].Value.Trim(),
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    // <chord>content</chord>
                    rawXml = Regex.Replace(
                        rawXml,
                        @"<\s*chord\s*>(.*?)<\s*/\s*chord\s*>",
                        m => m.Groups[1].Value.Trim(),
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    // Orphan <chord ...> tags
                    rawXml = Regex.Replace(
                        rawXml,
                        @"<\s*/?\s*chord\b[^>]*>",
                        "",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    // 3. Remove <tag name="...">...</tag> tags, even if nested (recursively)
                    while (Regex.IsMatch(rawXml, @"<tag\b[^>]*>.*?</tag>", RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    {
                        rawXml = Regex.Replace(
                            rawXml,
                            @"<tag\b[^>]*>(.*?)</tag>",
                            m => m.Groups[1].Value,
                            RegexOptions.IgnoreCase | RegexOptions.Singleline
                        );
                    }

                    // 4. Remove self-closing <tag ... /> tags
                    rawXml = Regex.Replace(
                        rawXml,
                        @"<tag\b[^>]*/>",
                        "",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    // 5. Remove orphan </tag> tags (defensive)
                    rawXml = Regex.Replace(
                        rawXml,
                        @"</tag>",
                        "",
                        RegexOptions.IgnoreCase | RegexOptions.Singleline
                    );

                    // Create a temporary file before deserialization due to HTML tags in <lines>
                    string tempPath = Path.GetTempFileName();
                    File.WriteAllText(tempPath, rawXml);

                    using FileStream fileStream = new FileStream(tempPath, FileMode.Open);
                    Input input = (Input)serializer.Deserialize(fileStream);
                    input.Flatten();

                    string safeTitle = MakeSafeFileName(input.Titles.FirstOrDefault() ?? "Canto_senza_nome");
                    string outputPath = Path.Combine(outputFolder, safeTitle + ".txt");

                    // If verse order is missing, generate it from available verses
                    if (input.VerseOrder == null)
                    {
                        foreach (var verse in input.Verses)
                        {
                            input.VerseOrder += verse.Name + "  ";
                        }
                    }

                    using (StreamWriter writer = new StreamWriter(outputPath))
                    {
                        // Write each verse in the specified order, with appropriate label
                        foreach (string verseId in input.VerseOrder.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        {
                            var verse = input.Verses.FirstOrDefault(v => v.Name == verseId);
                            if (verse == null) continue;

                            string prefix = verse.Name.Substring(0, 1).ToLower();
                            string number = verse.Name.Substring(1);

                            string label = prefix switch
                            {
                                "v" => $"[Verse {number}]",
                                "c" => $"[Chorus {number}]",
                                "f" => $"[Final {number}]",
                                "b" => $"[Bridge {number}]",
                                "o" => $"[Other {number}]",
                                "e" => $"[Ending {number}]",
                                _ => $"[{verse.Name}]"
                            };

                            writer.WriteLine(label);
                            writer.WriteLine(verse.Lines.Trim());
                            writer.WriteLine(); // empty line between verses
                        }
                    }
                    processedCount++;
                    Console.WriteLine($"Processed {processedCount}({fileCount} files in total). Output saved to {outputPath}.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file: {inputFile}");
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }
        }

        /// <summary>
        /// Replaces invalid filename characters with underscores.
        /// </summary>
        public static string MakeSafeFileName(string name)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
