using CharacterStudio.Controls.Panes;
using System.Windows.Forms;
using System.Xml;

namespace CharacterStudio.Controls.Common
{
    public partial class SimpleCharacterVis : UserControl
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
                    }
            }
            // TODO: Race/Class
            reader.Close();
            if (!string.IsNullOrWhiteSpace(Image))
            {
                this.pictureBox1.ImageLocation = Image;
                //this.pictureBox1.ErrorImage = // Fallack to default for Race/Class/Etc?
            }
            this.label1.Text = this.Name;
        }

        private void SimpleCharacterVis_DoubleClick(object sender, System.EventArgs e)
        {
            var parent = (this.ParentForm as PrimaryForm);
            parent.LoadCharacter(new ParagonLib.Serializer().Load(this.location));
            parent.DisplayPanel<HomePane>();
        }
    }
}