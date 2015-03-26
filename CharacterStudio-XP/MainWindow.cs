using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xwt;

namespace CharacterStudio_XP
{
    class MainWindow : Window
    {
        public MainWindow()
        {
            this.Size = new Size(this.ScreenBounds.Width, this.ScreenBounds.Height);
            this.Content = this.TopPanel = new HPaned();
            this.TopPanel.HeightRequest = this.ScreenBounds.Height;
            this.TopPanel.Panel1.Content = this.Header = new Xwt.Notebook();
        }

        public HPaned TopPanel { get; set; }

        public Notebook Header { get; set; }
    }
}
