using CharacterStudio.Controls.Panes;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;

namespace CharacterStudio.Controls.Common
{
    public partial class SimpleCharacterVis : ContentControl
    {
        public string Image { get; set; }

        public int Level { get; set; }

        private string location;

        public SimpleCharacterVis()
        {
            InitializeComponent();
            this.pictureBox1.DoubleClick += SimpleCharacterVis_DoubleClick;
            this.label1.DoubleClick += SimpleCharacterVis_DoubleClick;

        }

        public SimpleCharacterVis(string c)
            : this()
        {
            this.location = c;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Name = "";

            var tt = new ToolTip();
            tt.AutoPopDelay = 5000;
            tt.InitialDelay = 1000;
            tt.ReshowDelay = 500;
            tt.ShowAlways = true;
            tt.SetToolTip(this, c);
            tt.SetToolTip(label1, c);
            tt.SetToolTip(pictureBox1, c);

            var reader = XmlReader.Create(c);
            bool complete = false;
            while (complete == false)
            {
                reader.Read();
                if (reader.EOF)
                    break;
                if (reader.NodeType == XmlNodeType.Element)
                    switch (reader.Name)
                    {
                        case "name":
                        case "Name":
                            if (!string.IsNullOrEmpty(Name))
                                break;
                            reader.MoveToContent();
                            reader.Read();
                            this.Name = reader.Value;
                            break;

                        case "Portrait":
                            reader.MoveToContent();
                            reader.Read();
                            this.Image = reader.Value;
                            break;

                        case "Level":
                            if (Level != 0)
                                break;
                            reader.MoveToContent();
                            reader.Read();
                            this.Level = int.Parse(reader.Value);
                            break;

                        case "AbilityScores":
                        case "StatBlock":
                            //complete = true;
                            break;
                        case "RulesElement":
                            if (reader.GetAttribute("type") == "Class")
                                this.Class = reader.GetAttribute("name").Trim();
                            if (reader.GetAttribute("type") == "Race")
                                this.Race = reader.GetAttribute("name").Trim();
                            if (reader.GetAttribute("type") == "Gender")
                                this.Gender = reader.GetAttribute("name").Trim();

                            break;
                    }
            }
            // TODO: Race/Class
            reader.Close();
            if (string.IsNullOrWhiteSpace(Image))
                Image = DefaultPortrait(Class,Race, Gender);
            //else
            //    this.pictureBox1.ErrorImage = DefaultPortrait(Class, Race, Gender);
            if (!string.IsNullOrWhiteSpace(Image))
            {
                this.pictureBox1.ImageLocation = Image;
                //this.pictureBox1.ErrorImage = // Fallack to default for Race/Class/Etc?
            }

            this.label1.Text = string.Format("{0}\n Level {1} {2} {3}", Name, Level, Race, Class);
        }

        // TODO: Move this method somewhere better.
        public static string DefaultPortrait(string Class, string Race, string Gender)
        {
            var ClassPort = String.Format("ClassPort{0}", Class).ToLower();
            var RaceGendered = string.Format("{0}_{1}",(Gender ?? "").FirstOrDefault(), Race).ToLower();
            var RacePort = string.Format("RacePort{0}", Race).ToLower();
            var Generic = "Generic".ToLower();
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Character Studio", "Character Portraits");
            Directory.CreateDirectory(folder);
            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories).Select(n => n.ToLower());
            var port = files.Where(n => n.Contains(ClassPort)).FirstOrDefault()
                ?? files.Where(n => n.Contains(RaceGendered)).FirstOrDefault()
                ?? files.Where(n => n.Contains(RacePort)).FirstOrDefault()
                ?? files.Where(n => n.Contains(Generic)).FirstOrDefault();
            return port;
        }

        private void SimpleCharacterVis_DoubleClick(object sender, System.EventArgs e)
        {
            LoadCharacter(new ParagonLib.Serializer().Load(this.location));
            DisplayPanel<DetailsPane>();
        }

        public string Class { get; set; }

        public string Race { get; set; }

        public string Gender { get; set; }
    }
}