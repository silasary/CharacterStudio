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
            this.Content = this.TopPanel = new HPaned();
            this.TopPanel.Panel1.Content = this.Header = new Xwt.Notebook();
            this.Header.Add(
        }
            
        protected override void OnShown()
        {
            base.OnShown();
            this.Size = new Size(this.Screen.Bounds.Width, this.Screen.Bounds.Height);
            this.TopPanel.HeightRequest = this.ScreenBounds.Height;
        }

        public HPaned TopPanel { get; set; }

        public Notebook Header { get; set; }
    }
}
