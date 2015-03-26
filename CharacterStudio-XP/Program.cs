using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xwt;

namespace CharacterStudio_XP
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Initialize(ToolkitType.Wpf);
            var main = new MainWindow();
            main.Show();
            Application.Run();
        }
    }
}
