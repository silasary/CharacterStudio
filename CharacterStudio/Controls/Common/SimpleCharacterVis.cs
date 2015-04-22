using CharacterStudio.Controls.Panes;
using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Linq;
using ParagonLib;

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
            try
            {
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
                                this.Name = reader.Value.Trim();
                                break;

                            case "Portrait":
                                reader.MoveToContent();
                                reader.Read();
                                this.Image = reader.Value.Trim();
                                break;

                            case "Level":
                                if (Level != 0)
                                    break;
                                reader.MoveToContent();
                                reader.Read();
                                int l;
                                if (int.TryParse(reader.Value, out l))
                                    this.Level = l;
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
            }
            catch (XmlException) { this.Visible = false; }
            finally { reader.Close(); }
            
            if (string.IsNullOrWhiteSpace(Image))
                Image = Character.DefaultPortrait(Class,Race, Gender);
            //else
            //    this.pictureBox1.ErrorImage = DefaultPortrait(Class, Race, Gender);
            if (!string.IsNullOrWhiteSpace(Image))
            {
                this.pictureBox1.ImageLocation = Image;
                //this.pictureBox1.ErrorImage = // Fallack to default for Race/Class/Etc?
            }

            //this.label1.Text = string.Format("{0}\n Level {1} {2} {3}", Name, Level, Race, Class);
            this.label1.Text = string.Format("{0}, Level {1}\n {2} {3}", Name, Level, Race, Class);

        }

        private void SimpleCharacterVis_DoubleClick(object sender, System.EventArgs e)
        {
            this.PrimaryForm.Text = "Character Studio - Loading...";
            LoadCharacter(new ParagonLib.Serializer().Load(this.location));
            DisplayPanel<DetailsPane>();
        }

        public string Class { get; set; }

        public string Race { get; set; }

        public string Gender { get; set; }
    }
}