using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beyova.VsExtension
{
    public class BaseWindowsDialog : Microsoft.VisualStudio.PlatformUI.DialogWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseWindowsDialog"/> class.
        /// </summary>
        public BaseWindowsDialog()
        {
            HasMaximizeButton = true;
            HasMinimizeButton = true;
        }
    }
}
