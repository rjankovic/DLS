using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CD.DLS.Clients.Controls.Dialogs.Misc
{
    /// <summary>
    /// Interaction logic for AboutPane.xaml
    /// </summary>
    public partial class AboutPane : UserControl
    {
        public AboutPane()
        {
            InitializeComponent();
            var asm = Assembly.GetEntryAssembly();

            object[] productAttributes = asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            AssemblyProductAttribute productAttribute = null;
            if (productAttributes.Length > 0)
            {
                productAttribute = productAttributes[0] as AssemblyProductAttribute;
                AssemblyProductLabel.Content = productAttribute.Product;
            }

            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            string version = fvi.FileVersion;
            AssemblyVersionLabel.Content = version;

            object[] copyrightAttributes = asm.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            AssemblyCopyrightAttribute copyrightAttribute = null;
            if (copyrightAttributes.Length > 0)
            {
                copyrightAttribute = copyrightAttributes[0] as AssemblyCopyrightAttribute;
                AssemblyCopyrightLabel.Content = copyrightAttribute.Copyright;
            }
        }
    }
}
